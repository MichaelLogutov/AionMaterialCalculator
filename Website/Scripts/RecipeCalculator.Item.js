function Item(recipe, parent, data, level)
{
	this.id = NewId();
	this.recipe = recipe;
	this.index = this.recipe.items.length;
	this.parent = parent;
	this.data = data;
	this.level = level;
	this.checked = false;
	this.childNodes = null;
}

Item.prototype.GetProbability = function()
{
	var res = Parameters.itemProbabilities[this.data.id.toString()];
	if (!res || isNaN(res))
		res = 100.0;

	return res;
}

Item.prototype.SetProbability = function(n)
{
	n = parseFloat(n);
	if (isNaN(n) || n >= 100)
		n = null;

	Parameters.itemProbabilities[this.data.id.toString()] = n;
	Parameters.Save();
}

Item.prototype.IsSelfOrSiblingsChecked = function()
{
	if (this.checked)
		return true;

	var res = false;
	if (this.childNodes)
	{
		for (var k = 0; k < this.childNodes.length; k++)
		{
			res = this.childNodes[k].IsSelfOrSiblingsChecked();
			if (res)
				break;
		}
	}

	return res;
}

Item.prototype.Update = function(have)
{
	var need = parseInt(this.data.nq, 10);
	this.needless = false;

	if (this.parent)
	{
		need *= Math.ceil(this.parent.need / parseFloat(this.parent.data.qn));

		var prob = this.parent.GetProbability();
		if (prob < 100)
		{
			need = Math.round(need / (prob / 100.0));
			need = Math.ceil(need / this.parent.data.nq) * this.parent.data.nq;
		}

		if (this.parent.needless)
			this.needless = true;
	}

	var id = this.data.id.toString();

	if (!this.needless)
		this.needless = !this.IsSelfOrSiblingsChecked();

	this.needMax = need;

	if (!this.needless)
	{
		var v = have[id] || 0;

		this.need = Math.max(0, need - v);
		have[id] = v - (need - this.need);

		this.needless = (this.need < 1);
	}
	else
		this.need = need;

	if (this.childNodes)
	{
		for (var k = 0; k < this.childNodes.length; k++)
			this.childNodes[k].Update(have);
	}
}

Item.prototype.GetHaveCount = function()
{
	var n = Parameters.haveCount[this.data.id.toString()];
	if (!n || isNaN(n))
		return 0;

	return n;
};

Item.prototype.SetHaveCount = function(n)
{
	Parameters.haveCount[this.data.id.toString()] = n;
	Parameters.Save();
};

Item.prototype.GetPrice = function()
{
	var res = null;
	var id = this.data.id.toString();

	var res = parseInt(Parameters.itemPrices[id], 10);
	if (isNaN(res))
	{
		res = parseInt(Config.itemPrices[id], 10);

		if (isNaN(res))
			res = parseInt(this.data.p, 10);
	}

	if (isNaN(res))
		res = 0;

	return res;
};

Item.prototype.SetPrice = function(n)
{
	var id = this.data.id.toString();

	Config.itemPrices[id] = n;
	Parameters.itemPrices[id] = n;

	Parameters.Save();
	Config.Save();
};

Item.prototype.Check = function(check, mode, mode2)
{
	if (mode == "manual")
	{
		if (!this.parent || (!this.parent.parent && !check))
			return;
	}

	switch (mode)
	{
		case "silent":
			break;

		case "copy":
			{
				if (this.childNodes)
				{
					for (var k = 0; k < this.childNodes.length; k++)
						this.childNodes[k].Check(check, mode);
				}

				break;
			}

		default:
			{
				if (check)
				{
					// check siblings
					if (this.parent && this.parent.checked)
					{
						for (var k = 0; k < this.parent.childNodes.length; k++)
						{
							var sibling = this.parent.childNodes[k];

							if (sibling.id == this.id)
								continue;

							sibling.Check(true, "silent");
						}
					}

					// uncheck parents (from root to current node)
					if (mode != "no parent notify")
					{
						var parents = [];

						var p = this.parent;
						while (p)
						{
							parents.push(p);
							p = p.parent;
						}

						for (var k = parents.length - 1; k >= 0; k--)
							parents[k].Check(false, "no parent notify", "child nodes only");
					}

					// uncheck child nodes
					if (this.childNodes)
					{
						for (var k = 0; k < this.childNodes.length; k++)
							this.childNodes[k].Check(false, "copy");
					}
				}
				else
				{
					if (this.parent && mode != "no parent notify")
						this.parent.Check(true, mode);

					if (mode2 == "child nodes only" && this.childNodes && this.checked != check)
					{
						for (var k = 0; k < this.childNodes.length; k++)
							this.childNodes[k].Check(true, "silent");
					}
				}
			}
	}

	this.checked = check;
};

Item.prototype.ItemInfoDialog = function()
{
	var dlg =
	$(
		"<div>" +
			"<table>" +
				"<tr>" +
					"<td>" +
						"<a href='" + AionArmoryUrl + "/item.aspx?id=" + this.data.id + "' target='_blank'><img src='" + ItemIconBaseDir + this.data.i + ".jpg' /></a>" +
					"</td>" +
					"<td style='vertical-align: middle; padding-left: 10px'>" +
						"<a href='" + AionArmoryUrl + "/item.aspx?id=" + this.data.id + "' target='_blank' class='quality" + this.data.q + "'>" +
							this.data.d +
						"</a>" +
						"<div class='comment'>" + Localize(47, this.need) + "</div>" +
					"</td>" +
				"</tr>" +
			"</table>" +
			"<br />" +
			"<input type='text' maxlength='8' class='have req' formField='" + Localize(48) + "' value='" + this.GetHaveCount() + "' />" +
			"<input type='text' maxlength='8' class='price req' formField='" + Localize(49) + "' value='" + this.GetPrice() + "' />" +
		"</div>"
	);

	var item = this;

	var buttons = {};
	buttons[Localize("cancel")] = function()
	{
		dlg.dialog("close");
	};
	buttons[Localize("ok")] = function()
	{
		if (!$.validate("ItemInfoDialog"))
			return false;

		item.SetHaveCount(parseInt($(".have", dlg).val(), 10));
		item.SetPrice(parseInt($(".price", dlg).val(), 10));

		item.recipe.Redraw();
		UpdateTotal();

		dlg.dialog("close");
	};

	dlg.dialog
	({
		autoOpen: true,
		modal: true,
		minWidth: 300,
		minHeight: 10,
		open: function()
		{
			InitContextAddons(dlg);
			cg_aiondbsyndication.parseLinks();

			$(".have", dlg).select().focus();
			$("input:text", dlg).clickOnEnter($("button:contains('" + Localize("ok") + "')", dlg.next()));

			$.validatorsClear("ItemInfoDialog");

			$.validator
			({
				group: "ItemInfoDialog",
				targetSelector: $(".have", dlg),
				message: Localize(50)
			});
			$.validator
			({
				group: "ItemInfoDialog",
				targetSelector: $(".have", dlg),
				type: "integer",
				message: Localize(51)
			});
			$.validator
			({
				group: "ItemInfoDialog",
				targetSelector: $(".have", dlg),
				type: "compare",
				compareType: "greaterOrEqual",
				targetValue: 0,
				valueType: "integer",
				message: Localize(52)
			});

			$.validator
			({
				group: "ItemInfoDialog",
				targetSelector: $(".price", dlg),
				message: Localize(50)
			});
			$.validator
			({
				group: "ItemInfoDialog",
				targetSelector: $(".price", dlg),
				type: "integer",
				message: Localize(51)
			});
			$.validator
			({
				group: "ItemInfoDialog",
				targetSelector: $(".price", dlg),
				type: "compare",
				compareType: "greaterOrEqual",
				targetValue: 0,
				valueType: "integer",
				message: Localize(52)
			});
		},
		buttons: buttons,
		close: function(e, ui)
		{
			$(this)
				.hideErrors()
				.dialog("destroy")
				.remove();
		}
	});
}

Item.prototype.EditProbabilityDialog = function()
{
	var item = this;

	EditNumber
	({
		value: this.GetProbability(),
		name: Localize(53),
		comment: Localize(54, this.data.d),
		type: "float",
		typeErrorMessage: Localize(55),
		minValue: 1,
		minValueErrorMessage: Localize(56),
		OnOk: function(n)
		{
			item.SetProbability(Math.min(Math.round(n * 100) / 100.0, 100.0));
			item.recipe.Redraw();
			UpdateTotal();
		}
	});
};

Item.prototype.GetTotalNeed = function()
{
	var total_need = 0;

	for (var k = 0; k < this.recipe.items.length; k++)
	{
		var item = this.recipe.items[k];
		if (item.needless || item.data.id != this.data.id)
			continue;

		total_need += item.need;
	}

	return total_need;
};

Item.prototype.GetMaxNeed = function()
{
	var total_need = 0;

	for (var k = 0; k < this.recipe.items.length; k++)
	{
		var item = this.recipe.items[k];
		if (item.data.id != this.data.id)
			continue;

		total_need += item.needMax;
	}

	return total_need;
};