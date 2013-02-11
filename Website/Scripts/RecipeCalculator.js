var PhMain;

var Races = [Localize(88), Localize(89)];
var Skills = [Localize(20), Localize(21), Localize(22), Localize(23), Localize(24), Localize(25), Localize(26)];
var Quality = [Localize(27), Localize(28), Localize(29), Localize(30), Localize(31), Localize(32), Localize(33)];

var ItemTypes = {};
ItemTypes[Localize(1)] = 1;
ItemTypes[Localize(2)] = 2;
ItemTypes[Localize(3)] = 3;
ItemTypes[Localize(4)] = 8;
ItemTypes[Localize(5)] = 6;
ItemTypes[Localize(6)] = 4;
ItemTypes[Localize(7)] = 5;
ItemTypes[Localize(8)] = 7;
ItemTypes[Localize(9)] = 9;
ItemTypes[Localize(10)] = 110;
ItemTypes[Localize(11)] = 20;
ItemTypes[Localize(12)] = 30;
ItemTypes[Localize(13)] = 40;
ItemTypes[Localize(14)] = 50;
ItemTypes[Localize(15)] = 100;
ItemTypes[Localize(16)] = 60;
ItemTypes[Localize(17)] = 70;
ItemTypes[Localize(18)] = 80;
ItemTypes[Localize(19)] = 90;

var Recipes = [];
var AionArmoryUrl = null;

$(function()
{
	PhMain = $("#RecipeCalculator_aspx");
	Config.Load();

	Config.recipeList.race = null;
	Config.recipeList.skill = null;

	if (!Lang || Lang == "en" || Lang == "ru")
		AionArmoryUrl = "http://www.aionarmory.com";
	else
		AionArmoryUrl = "http://" + Lang + ".aionarmory.com";

	$(".addRecipe", PhMain).click(function()
	{
		SelectItemDialog();
		return false;
	});

	$(".feedback", PhMain).click(function()
	{
		FeedbackDialog();
		return false;
	});

	$(".reset", PhMain).click(function()
	{
		if (!isNaN(Parameters.recipeId) && !isNaN(Parameters.itemId))
		{
			var recipe_id = Parameters.recipeId;
			var item_id = Parameters.itemId;
			Parameters.Reset();

			AddItem(recipe_id, item_id);
		}

		return false;
	});

	ApplyDefaultActions(PhMain);

	Load();
});

// ---------------------------------------------------------------------
//	Main functions
// ---------------------------------------------------------------------

function Load()
{
	Parameters.Load();

	if (!isNaN(Parameters.recipeId) && !isNaN(Parameters.itemId))
		AddItem(Parameters.recipeId, Parameters.itemId);
}


function AddItem(recipeId, itemId)
{
	var lb = ShowLoadingBox();
	$.getJSON
	(
		"/AionRecipeData.axd",
		{
			rid: recipeId,
			id: itemId,
			l: Lang
		},
		function(data)
		{
			HideLoadingBox(lb);

			if (!data)
			{
				ShowMessageBox(Localize(57));
				return;
			}

			if (Parameters.recipeId != recipeId || Parameters.itemId != itemId)
			{
				Parameters.Reset();
				Parameters.recipeId = recipeId;
				Parameters.itemId = itemId;
				Parameters.Save();
			}

			var recipe = new Recipe(data);
			Recipes[0] = recipe;

			var ph_default = PhMain.switchPh("Default");
			$(".recipeLink", ph_default).attr("recipeId", recipeId);
			$(".recipeInfoLink", ph_default).attr("recipeId", recipeId);

			recipe.Redraw();
			UpdateTotal();

			document.title = Localize(58, recipe.root.data.d);
		}
	);
}

function UpdateTotal()
{
	var recipe = Recipes[0];
	var result = $(".total", PhMain);

	var _sortItemCallback = function(a, b)
	{
		var ad = a.item.data;
		var bd = b.item.data;

		var as = 0;
		var bs = 0;

		if (ad.q > bd.q)
			as++;
		else if (ad.q < bd.q)
			bs++;
		else
		{
			if (ad.l > bd.l)
				as++;
			else if (ad.l < bd.l)
				bs++;
			else
			{
				if (ad.d < bd.d)
					as++;
				else if (ad.d > bd.d)
					bs++;
			}
		}

		if (as > bs)
			return -1;
		else if (as < bs)
			return 1;

		return 0;
	};

	var _getItemHtml = function(v)
	{
		var res =
			"<td class='itemIcon'>" +
				"<a href='" + AionArmoryUrl + "/item.aspx?id=" + v.item.data.id + "' target='_blank'><img src='" + ItemIconBaseDir + v.item.data.i + ".jpg' /></a>" +
			"</td>" +
			"<td class='itemInfo'>" +
				"<a href='" + AionArmoryUrl + "/item.aspx?id=" + v.item.data.id + "' target='_blank' class='quality" + v.item.data.q + "' style='padding-right: 10px'>" +
					v.item.data.d +
				"</a>" +
				"<a href='javascript:;' class='openAionArmory' itemId='" + v.item.data.id + "' target='_blank' tooltip='" + Localize(42) + "'><img src='/Images/arrow-045-small.png' /></a>" +
				"<a href='javascript:;' class='viewInformation' itemId='" + v.item.data.id + "' target='_blank' tooltip='" + Localize(59) + "'><img src='/Images/information-small.png' /></a>" +
				"<div class='comment'>" + v.item.data.l + " lvl</div>" +
			"</td>";

		return res;
	}

	var html = [];
	var have_items_list;
	var need_items_list;

	// have
	var total_have_price = 0;
	{
		var vv = [];
		$.each(Parameters.haveCount, function(k, v)
		{
			if (v > 0)
				vv.push
				({
					amount: v,
					item: recipe.itemsById[k]
				});
		});

		vv = vv.sort(_sortItemCallback);

		if (vv.length > 0)
		{
			html.push
			(
				"<table class='resultTable'>" +
					"<tr>" +
						"<th colspan='3'>" +
							"<h2>" + Localize(60) + " <a href='javascript:;' class='export exportHave' tooltip='" + Localize(61) + "'><img src='/Images/table-export.png' style='vertical-align: middle' /></a></h2>" +
						"</th>" +
					"</tr>"
			);

			for (var k = 0; k < vv.length; k++)
			{
				var v = vv[k];

				var need = v.item.GetMaxNeed();
				var amount = Math.min(v.amount, need);
				var more_than_need = (v.amount > need);

				var p = v.item.GetPrice();
				var tp = amount * p;

				html.push
				(
					"<tr class='itemRow'>" +
						_getItemHtml(v) +
						"<td class='amountInfo itemInfoEdit' itemIndex='" + v.item.index + "' tooltip='" + Localize(62) + "' style='white-space: nowrap'>" +
							"<b>" + (more_than_need ? amount.toStringFormatted() + " (" + v.amount.toStringFormatted() + ")" : amount.toStringFormatted()) + "</b> x <span class='kinah'>" + p.toStringFormatted() + "</span><br />= <span class='kinah'>" + tp.toStringFormatted() + "</span>" +
						"</td>" +
					"</tr>"
				);

				total_have_price += tp;
			}

			html.push
			(
					"<tr>" +
						"<td colspan='3' class='subtotal'>" +
							Localize(63) + ": <span class='kinah'>" + total_have_price.toStringFormatted() + "</span>" +
						"</td>" +
					"</tr>" +
				"</table>"
			);
		}

		have_items_list = vv;
	}

	// need
	var total_need_price = 0;
	{
		var vv = {};
		for (var k = 0; k < recipe.items.length; k++)
		{
			var item = recipe.items[k];
			if (!item.checked || item.need < 1)
				continue;

			var id = item.data.id.toString();
			if (!vv[id])
				vv[id] =
				{
					amount: 0,
					item: item
				};

			vv[id].amount += item.need;
		}

		{
			var tmp = [];
			$.each(vv, function(k, v)
			{
				tmp.push(v);
			});

			vv = tmp.sort(_sortItemCallback);
		}

		if (vv.length > 0)
		{
			html.push
			(
				"<table class='resultTable'>" +
					"<tr>" +
						"<th colspan='3'>" +
							"<h2>" + Localize(64) + " <a href='javascript:;' class='export exportNeed' tooltip='" + Localize(61) + "'><img src='/Images/table-export.png' style='vertical-align: middle' /></a></h2>" +
						"</th>" +
					"</tr>"
			);

			for (var k = 0; k < vv.length; k++)
			{
				var v = vv[k];
				var p = v.item.GetPrice();
				var tp = v.amount * p;

				html.push
				(
					"<tr class='itemRow'>" +
						_getItemHtml(v) +
						"<td class='amountInfo itemInfoEdit' itemIndex='" + v.item.index + "' tooltip='" + Localize(62) + "' style='white-space: nowrap'>" +
							"<b>" + v.amount.toStringFormatted() + "</b> x <span class='kinah'>" + p.toStringFormatted() + "</span><br />= <span class='kinah'>" + tp.toStringFormatted() + "</span>" +
						"</td>" +
					"</tr>"
				);

				total_need_price += tp;
			}

			html.push
			(
					"<tr>" +
						"<td colspan='3' class='subtotal'>" +
							Localize(63) + ": <span class='kinah'>" + total_need_price.toStringFormatted() + "</span>" +
						"</td>" +
					"</tr>" +
				"</table>"
			);
		}

		need_items_list = vv;
	}

	var sell_price = recipe.GetSellPrice();

	html.push(
		"<br />" +
		"<table class='totalValues'>" +
			"<tr>" +
				"<th>" +
					Localize(65) +
				"</th>" +
				"<td>" +
					"<span class='kinah'>" + (total_have_price + total_need_price).toStringFormatted() + "</span>" +
				"</td>" +
			"</tr>" +
			"<tr><td colspan='2' class='line'></td></tr>" +
			"<tr>" +
				"<th>" +
					Localize(66) +
				"</th>" +
				"<td>" +
					"<a href='javascript:;' class='sellPrice kinah dashedUnderline' tooltip='" + Localize(67) + "'>" + sell_price.toStringFormatted() + "</a>" +
				"</td>" +
			"</tr>" +
			"<tr>" +
				"<th>" +
					Localize(68) +
				"</th>" +
				"<td>" +
					"<span class='kinah'>" + (sell_price * recipe.root.data.nq).toStringFormatted() + "</span>" +
				"</td>" +
			"</tr>" +
			"<tr><td colspan='2' class='line'>&nbsp;</td></tr>"
	);

	if (total_have_price > 0)
	{
		html.push
		(
				"<tr>" +
					"<th>" +
						Localize(69) +
					"</th>" +
					"<td>" +
						"<span class='kinah colorNumber'>" + (sell_price * recipe.root.data.nq - total_need_price).toStringFormatted() + "</span>" +
					"</td>" +
				"</tr>"
		);
	}

	html.push
	(
			"<tr>" +
				"<th>" +
					Localize(70) +
				"</th>" +
				"<td>" +
					"<span class='kinah colorNumber'>" + (sell_price * recipe.root.data.nq - total_have_price - total_need_price).toStringFormatted() + "</span>" +
				"</td>" +
			"</tr>" +
		"</table>"
	);

	result.html(html.join(""));

	$(".sellPrice", result).click(function()
	{
		EditNumber
		({
			value: recipe.GetSellPrice(),
			name: Localize(71),
			comment: Localize(72, recipe.root.data.d),
			OnOk: function(n)
			{
				recipe.SetSellPrice(n);
				UpdateTotal();
			}
		});

		return false;
	});

	$(".export", result).click(function()
	{
		var t = $(this);
		var bb_code = [];
		var ingame_code = [];
		var names_code = [];

		var _Export = function(vv)
		{
			for (var k = 0; k < vv.length; k++)
			{
				var v = vv[k];

				bb_code.push("[ITEM]" + v.item.data.id + "[/ITEM] x " + v.amount);
				ingame_code.push("[item: " + v.item.data.id + "] x " + v.amount);
				names_code.push(v.item.data.d + " x " + v.amount);
			}
		}

		var export_have = (t.hasClass("exportHave") && have_items_list && have_items_list.length > 0);
		var export_need = (t.hasClass("exportNeed") && need_items_list && need_items_list.length > 0);

		if (export_have)
			_Export(have_items_list);

		if (export_need)
			_Export(need_items_list);

		var dlg =
		$(
			"<div>" +
				"<textarea class='bbcode' formField='" + Localize(73) + "' style='width: 100%' rows='5'></textarea>" +
				"<textarea class='ingame' formField='" + Localize(74) + "' style='width: 100%' rows='5'></textarea>" +
				"<textarea class='names' formField='" + Localize(75) + "' style='width: 100%' rows='5'></textarea>" +
			"</div>"
		);

		var buttons = {};
		buttons[Localize("close")] = function()
		{
			dlg.dialog("close");
		};

		dlg.dialog
		({
			autoOpen: true,
			modal: true,
			width: 500,
			minHeight: 10,
			open: function()
			{
				$(".bbcode", dlg).val("[LIST]\n[*]" + bb_code.join("\n[*]") + "\n[/LIST]");
				$(".ingame", dlg).val(ingame_code.join(", "));
				$(".names", dlg).val(names_code.join("\n"));

				InitContextAddons(dlg);
				$("button:contains('" + Localize("close") + "')", dlg.next()).focus();
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

		return false;
	});

	ApplyDefaultActions(result);
	cg_aiondbsyndication.parseLinks();
}

var Tooltip = null;

function TooltipMouseMoveCallback(e)
{
	var t = $(this);

	if (!Tooltip)
	{
		Tooltip = $("#tooltip");
		if (Tooltip.size() == 0)
		{
			Tooltip = $("<div id='tooltip' class='tooltip' style='display: none; white-space: nowrap'></div>");
			$(document.body).append(Tooltip);
		}

		Tooltip
			.html(t.attr("tooltip"))
			.show();
	}

	var x = e.pageX;
	var y = e.pageY;
	var w = Tooltip.outerWidth();
	var h = Tooltip.outerHeight();

	if (x + w + 30 - $(window).scrollLeft() > $(window).width())
		x -= w + 10;
	else
		x += 15;

	if (y + h + 30 - $(window).scrollTop() > $(window).height())
		y -= h + 10;
	else
		y += 15;

	Tooltip.css
	({
		left: x,
		top: y
	});
}

function TooltipMouseLeaveCallback(e)
{
	if (Tooltip)
	{
		Tooltip.hide();
		Tooltip = null;
	}
}

function ApplyDefaultActions(context)
{
	if (!context)
		context = PhMain;

	$(".itemInfoEdit[itemIndex]", context).unbind("click").click(function()
	{
		var t = $(this);

		var item = Recipes[0].items[parseInt(t.attr("itemIndex"), 10)];
		if (!item)
			return;

		item.ItemInfoDialog();

		return false;
	});

	$("*[tooltip]", context)
		.unbind("mousemove", TooltipMouseMoveCallback)
		.unbind("mouseleave", TooltipMouseLeaveCallback)
		.bind("mousemove", TooltipMouseMoveCallback)
		.bind("mouseleave", TooltipMouseLeaveCallback);

	$(".openAionArmory[itemId]", context).unbind("click").click(function(e)
	{
		e.stopPropagation();
		window.open(AionArmoryUrl + "/item.aspx?id=" + $(this).attr("itemId"), "_blank");
		return false;
	});

	$(".openAionArmory[recipeId]", context).unbind("click").click(function(e)
	{
		e.stopPropagation();
		window.open(AionArmoryUrl + "/recipe.aspx?id=" + $(this).attr("recipeId"), "_blank");
		return false;
	});

	var stopPropagation = function(e)
	{
		e.stopPropagation();
	};
	$(".stopPropagation", context).unbind("click", stopPropagation).click(stopPropagation);

	$(".openItemNewWindow[itemId][recipeId]", context).unbind("click").click(function(e)
	{
		e.stopPropagation();

		var t = $(this);
		window.open("#itemId=" + t.attr("itemId") + "|recipeId=" + t.attr("recipeId"), "_blank");

		return false;
	});

	$(".colorNumber", context).each(function()
	{
		var t = $(this);
		var n = parseInt(t.text(), 10);

		if (!isNaN(n))
		{
			if (n < 0)
			{
				t.removeClass("positive");
				t.addClass("negative");
			}
			else
			{
				t.removeClass("negative");
				t.addClass("positive");
			}
		}
		else
		{
			t.removeClass("positive");
			t.removeClass("negative");
		}
	});

	$(".viewInformation", context).unbind("click").click(function()
	{
		var t = $(this);
		var id = parseInt(t.attr("itemId"), 10);
		var recipe = Recipes[0];
		var item = null;
		var total_need = 0;

		var is_item = !isNaN(id);
		if (!is_item)
			id = parseInt(t.attr("recipeId"), 10);
		else if (recipe)
		{
			item = recipe.itemsById[id];

			if (item)
				total_need = item.GetTotalNeed();
		}

		var dlg =
		$(
			"<div>" +
				(item ? "<input type='text' class='name' formField='" + Localize(76) + "' size='30' />" : "") +
				"<input type='text' class='ingameLink' formField='" + Localize(77) + "' size='30' />" +
				(total_need > 1 ? "<input type='text' class='ingameLink2' formField='" + Localize(78) + "' size='30' />" : "") +
			"</div>"
		);

		var buttons = {};
		buttons[Localize("close")] = function()
		{
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
				if (is_item)
				{
					$(".ingameLink", dlg).val("[item: " + id + "]");

					if (item)
						$(".name", dlg).val(item.data.d);

					$(".ingameLink2", dlg).val("[item: " + id + "] x " + total_need);
				}
				else
				{
					$(".ingameLink", dlg).val("[recipe: " + id + "]");
				}

				InitContextAddons(dlg);
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

		return false;
	});

	cg_aiondbsyndication.parseLinks();
}