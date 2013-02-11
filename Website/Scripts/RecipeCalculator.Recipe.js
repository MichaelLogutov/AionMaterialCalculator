function Recipe(data)
{
	this.items = [];
	this.itemsById = {};
	this.root = null;
	this.leafs = [];

	//this._prepareRecipe(null, $.xmlToJSON(data, { outputType: 1 }).Item[0]);
	this._prepareRecipe(null, data);

	this.SetTotalQuantity(isNaN(Parameters.totalQuantity) ? this.root.data.qn : Parameters.totalQuantity);
}

Recipe.prototype.SetTotalQuantity = function(n)
{
	n = parseInt(n, 10);

	this.root.data.nq = n;
	Parameters.totalQuantity = n;
	Parameters.Save();
}

Recipe.prototype.SetSellPrice = function(n)
{
	n = parseInt(n, 10);

	Parameters.sellPrice = n;
	Parameters.Save();
}

Recipe.prototype.GetSellPrice = function()
{
	return isNaN(Parameters.sellPrice) ? Math.round(parseInt(this.root.data.p, 10) * 0.2) : Parameters.sellPrice;
}

function RecipeItemSortCallback(a, b)
{
	var as = a.q;
	var bs = b.q;

	if (a.Items)
		as++;
	if (a.t == 1)
		as++;

	if (b.Items)
		bs++;
	if (b.t == 1)
		bs++;

	if (as > bs)
		return -1;
	else if (as < bs)
		return 1;
	else
	{
		if (a.l > b.l)
			return -1;
		else if (a.l < b.l)
			return 1;
		else
		{
			if (a.d > b.d)
				return 1;
			else if (a.d < b.d)
				return -1;
		}
	}

	return 0;
}

Recipe.prototype._prepareRecipe = function(parent, data, level)
{
	if (!level)
		level = 0;

	if (!data.nq)
		data.nq = 1;

	var item = new Item(this, parent, data, level);
	this.items.push(item);
	this.itemsById[item.data.id.toString()] = item;

	if (!parent)
		this.root = item;

	//	if (data.Items)
	//	{
	//		var child_items = data.Items[0].Item;
	//		child_items = child_items.sort(RecipeItemSortCallback);

	//		item.childNodes = [];

	//		for (var k = 0; k < child_items.length; k++)
	//			item.childNodes.push(this._prepareRecipe(item, child_items[k], level + 1));
	//	}

	if (data.c)
	{
		var child_items = data.c;
		child_items = child_items.sort(RecipeItemSortCallback);

		item.childNodes = [];

		for (var k = 0; k < child_items.length; k++)
			item.childNodes.push(this._prepareRecipe(item, child_items[k], level + 1));
	}
	else
	{
		this.leafs.push(item);
	}

	return item;
}

Recipe.prototype.Redraw = function()
{
	this.Update();

	var ph_default = PhMain.getPh("Default");
	var result = $(".result", ph_default);

	result.html
	(
		"<div class='recipe'>" +
			this._GetItemHtml(this.root, []) +
		"</div>"
	);

	// inital check items
	if (!this.firstTimeRedrawComplete)
	{
		this.firstTimeRedrawComplete = true;

		if (Parameters.checkedItems.length > 0)
		{
			for (var k = 0; k < Parameters.checkedItems.length; k++)
			{
				var index = Parameters.checkedItems[k];
				if (index >= this.items.length)
					continue;

				var item = this.items[index];
				if (!item)
					continue;

				item.Check(true, "silent");
			}
		}
		else
		{
			for (var k = 0; k < this.leafs.length; k++)
			{
				var item = this.leafs[k];
				var proceeded = false;

				// if the whole recipe is not "morph" recipe then skip init check of all leaf nodes that are in morph recipes
				if (this.root.data.s != 2 && item.parent && item.parent.data.s == 2)
				{
					var p = item.parent;
					while (p && p.data.s == 2)
					{
						if (p.parent && p.parent.data.s != 2)
						{
							p.Check(true, "silent");
							proceeded = true;

							break;
						}

						p = p.parent;
					}
				}

				if (!proceeded)
					item.Check(true, "silent");
			}
		}

		this.Redraw();
		return;
	}

	// on recipe item click
	var recipe = this;
	$(".recipeItem", result).click(function()
	{
		var item = recipe.items[parseInt($(this).attr("itemIndex"), 10)];
		item.Check(!item.checked, "manual");

		Parameters.CheckItemsChanged(recipe);
		recipe.Redraw();
		UpdateTotal();
	});

	$(".resultMultiply", result)
		.click(function(e)
		{
			e.stopPropagation();
		})
		.keypress(function(e)
		{
			if (e.which == 13)
			{
				e.preventDefault();

				var n = parseInt($(this).val(), 10);
				if (isNaN(n) || n < 1)
				{
					n = 1;
					$(this).val(n);
				}

				recipe.SetTotalQuantity(n);
				recipe.Redraw();
				UpdateTotal();

				$(".resultMultiply", result).focus();
			}
		});

	$(".otherProcVersion", result).click(function()
	{
		AddItem(recipe.root.data.rid, recipe.root.data.idp);
		return false;
	});

	$(".itemProbabilityEdit", result).click(function(e)
	{
		e.stopPropagation();

		var t = $(this);

		var item = Recipes[0].items[parseInt(t.attr("itemIndex"), 10)];
		if (!item)
			return;

		item.EditProbabilityDialog();

		return false;
	});

	ApplyDefaultActions(result);
}

Recipe.prototype._GetItemHtml = function(item, levelBranchEnded)
{
	var res = "";

	res +=
		"<table id='rcItem" + item.id + "' itemIndex='" + item.index + "' class='item recipeItem" + (item.childNodes ? " craftable" : "") + (item.checked ? " checked" : "") + "'>" +
			"<tr>";

	for (var k = 0; k < item.level; k++)
	{
		if (levelBranchEnded[k])
		{
			if (k == item.level - 1)
				res += "<td class='itemLevelPh lineC'></td>";
			else
				res += "<td class='itemLevelPh'></td>";
		}
		else
		{
			if (k == item.level - 1)
				res += "<td class='itemLevelPh lineB'></td>";
			else
				res += "<td class='itemLevelPh lineA'></td>";
		}
	}

	res +=
				"<td class='itemIcon" + (item.needless ? " needless" : "") + "'>";

	if (item.childNodes)
		res +=
					"<a href='" + AionArmoryUrl + "/item.aspx?id=" + item.data.id + "' itemId='" + item.data.id + "' recipeId='" + item.data.rid + "' class='openItemNewWindow'>";
	else
		res +=
					"<a href='" + AionArmoryUrl + "/item.aspx?id=" + item.data.id + "' target='_blank' class='stopPropagation'>";

	res +=
						"<img src='" + ItemIconBaseDir + item.data.i + ".jpg' />" +
					"</a>" +
				"</td>" +
				"<td class='itemRowCount" + (item.needless ? " needless" : "") + "'>";

	if (item.id == this.root.id)
		res += "<input type='text' class='resultMultiply' maxlength='3' size='3' value='" + item.data.nq + "' />";
	else
		res += item.data.nq + "</td>";

	res +=
				"<td class='itemInfo" + (item.needless ? " needless" : "") + "'>" +
					(item.data.rt == 1 ? "[proc] " : "");

	if (item.childNodes)
		res +=
					"<a href='" + AionArmoryUrl + "/item.aspx?id=" + item.data.id + "' itemId='" + item.data.id + "' recipeId='" + item.data.rid + "' class='openItemNewWindow quality" + item.data.q + "'>" + item.data.d + "</a>" +
					"<span class='comment' style='padding-right: 10px'> " + item.data.l + " lvl</span>" +
					"<a href='javascript:;' class='openAionArmory' itemId='" + item.data.id + "' target='_blank' tooltip='" + Localize(42) + "'><img src='/Images/arrow-045-small.png' /></a>" +
					"<a href='javascript:;' class='viewInformation' itemId='" + item.data.id + "' target='_blank' tooltip='" + Localize(59) + "'><img src='/Images/information-small.png' /></a>" +
					"<div class='comment'>" +
						Localize(79,
							item.data.qn,
							"<a href='javascript:;' class='itemProbabilityEdit dashedUnderline comment' itemIndex='" + item.index + "' tooltip='" + Localize(80) + "'>" + item.GetProbability() + "%</a>",
							"<b>" + Skills[item.data.s].toLowerCase() + "</b> " + item.data.sl + " lvl"
						) + ":" +
					"</div>";
	else
		res +=
					"<a href='" + AionArmoryUrl + "/item.aspx?id=" + item.data.id + "' target='_blank' class='stopPropagation quality" + item.data.q + "'>" + item.data.d + "</a>" +
					"<span class='comment' style='padding-right: 10px'> " + item.data.l + " lvl</span>" +
					"<a href='javascript:;' class='openAionArmory' itemId='" + item.data.id + "' target='_blank' tooltip='" + Localize(42) + "'><img src='/Images/arrow-045-small.png' /></a>" +
					"<a href='javascript:;' class='viewInformation' itemId='" + item.data.id + "' target='_blank' tooltip='" + Localize(59) + "'><img src='/Images/information-small.png' /></a>";

	res += "</td>";

	if (item.id == this.root.id)
	{
		res += "<td class='v'></td>";

		if (item.data.idp)
			res += "<td class='recipeItemButton' tooltip='" + (item.data.rt == 1 ? Localize(81) : Localize(82)) + "'><a href='#itemId=" + item.data.idp + "|recipeId=" + item.data.rid + "' class='otherProcVersion'><img src='/Images/universal.png' /></a></td>";
		else
			res += "<td class='recipeItemButton'></td>";
	}
	else
	{
		// need
		res += "<td class='v itemInfo itemInfoEdit" + (item.needless ? " needless" : "") + "' itemIndex='" + item.index + "' tooltip='" + Localize(83) + "'><b>" + item.need + "</b></td>";

		// selection mark
		res += "<td class='recipeItemSelect' tooltip='" + Localize(84) + "'>&nbsp;</td>";
	}

	res +=
			"</tr>" +
		"</table>";

	if (item.childNodes)
	{
		for (var k = 0; k < item.childNodes.length; k++)
		{
			levelBranchEnded[item.level] = (k == item.childNodes.length - 1);
			res += this._GetItemHtml(item.childNodes[k], levelBranchEnded);
		}
	}

	return res;
}

Recipe.prototype.Update = function()
{
	var have = {};
	$.each(Parameters.haveCount, function(k, v)
	{
		have[k] = v;
	});

	this.root.Update(have);
}