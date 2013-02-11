var RecipeData = null;

function SelectItemDialog()
{
	if (!Config.recipeList.race || !Config.recipeList.skill)
	{
		SelectRaceAndSkillDialog(function()
		{
			SelectItemDialog();
		});
		return;
	}

	if (!RecipeData)
	{
		var lb = ShowLoadingBox(Localize(85));
		$.getJSON
		(
			"/AionRecipeData.axd",
			{
				rid: Config.recipeList.race,
				id: Config.recipeList.skill,
				l: Lang,
				v: Version
			},
			function(data)
			{
				HideLoadingBox(lb);

				if (!data)
				{
					ShowMessageBox(Localize(86));
					return;
				}

				RecipeData = data;
				SelectItemDialog();
			}
		);

		return;
	}

	var GetSkillOptionsHtml = function()
	{
		var res = "";

		for (var k = 0; k < Skills.length; k++)
			res += "<option value='" + k + "'>" + Skills[k] + "</option>";

		return res;
	}

	var GetQualityHtml = function(recipeData)
	{
		var res = "";

		for (var k = 0; k < Quality.length; k++)
		{
			var q = k * 10;

			for (var i = 0; i < recipeData.length; i++)
			{
				if (recipeData[i].q == q)
				{
					res += "<label class='quality" + q + "'><input type='checkbox' class='quality' value='" + q + "' /> " + Quality[k] + "</label><br />";
					break;
				}
			}
		}

		return res;
	}

	var GetItemTypeHtml = function(recipeData)
	{
		var html = [];

		html.push
		(
			"<table style='width: 100%'>" +
				"<tr><td>"
		);

		var first_column = true;
		$.each(ItemTypes, function(k, v)
		{
			for (var i = 0; i < recipeData.length; i++)
			{
				if (recipeData[i].t == v)
				{

					if (first_column && v > 9)
					{
						first_column = false;
						html.push("</td><td>");
					}

					html.push("<label><input type='checkbox' class='itemType' value='" + v + "' /> " + k + "</label><br />");
					break;
				}
			}
		});

		html.push
		(
				"</td></tr>" +
			"</table>"
		);

		return html.join("");
	}

	var dlg =
	$("<div>" +
		"<table style='width: 100%'>" +
			"<tr>" +
				"<td style='width: 40%'>" +
					"<table class='recipeListParams' style='width: 100%'>" +
						"<tr>" +
							"<td colspan='2'>" +
								"<h2 class='butRaceAndSkill' style='cursor: pointer; padding-bottom: 10px' tooltip='" + Localize(62) + "'>" +
									Races[Config.recipeList.race] + " " + Skills[Config.recipeList.skill] +
								"</h2>" +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td style='width: 150px'>" +
								Localize(90) +
							"</td>" +
							"<td style='white-space: nowrap'>" +
								"<input type='text' class='filter' maxlength='255' size='25' /> " +
								"<span class='filterIgnoreCase toggleButton' tooltip='" + Localize(91) + "'>&nbsp;" + Localize(92) + "&nbsp;</span>" +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								Localize(93) +
							"</td>" +
							"<td style='white-space: nowrap'>" +
								"<select class='sortBy'>" +
									"<option value='sl'>" + Localize(94) + "</option>" +
									"<option value='l'>" + Localize(95) + "</option>" +
									"<option value='q'>" + Localize(96) + "</option>" +
								"</select> " +
								"<label style='margin-left: 10px'><input type='radio' name='sortDir' value='asc'>▲</label>" +
								"<label style='margin-left: 10px'><input type='radio' name='sortDir' value='desc'>▼</label>" +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								Localize(97) +
							"</td>" +
							"<td>" +
								"<input type='text' class='lMin' maxlength='3' size='5' />" +
								" - " +
								"<input type='text' class='lMax' maxlength='3' size='5' />" +
							"</td>" +
						"</tr>" +
						"<tr>" +
							"<td>" +
								Localize(98) +
							"</td>" +
							"<td>" +
								"<input type='text' class='slMin' maxlength='3' size='5' />" +
								" - " +
								"<input type='text' class='slMax' maxlength='3' size='5' />" +
							"</td>" +
						"</tr>" +
					"</table>" +
				"</td>" +
				"<td style='padding-left: 10px'>" +
					"<table style='width: 100%'>" +
						"<tr>" +
							"<td>" +
								GetQualityHtml(RecipeData) +
								"<br />" +
								"<label><input type='checkbox' class='procVersions' value='1' />" + Localize(111) + "</label><br />" +
								"<label><input type='checkbox' class='procVersions' value='0' />" + Localize(112) + "</label><br />" +
							"</td>" +
							"<td style='padding-left: 10px'>" +
								GetItemTypeHtml(RecipeData) +
							"</td>" +
							"<td style='vertical-align: bottom'>" +
								"<a href='javascrip:;' class='reset' title='" + Localize(99) + "'><img src='/Images/cross.png' /></a>" +
							"</td>" +
						"</tr>" +
					"</table>" +
				"</td>" +
			"</tr>" +
		"</table>" +
		"<div class='recipeList'>" +
			"<div class='ph phEmpty'>" +
				"<br />" +
				"<br />" +
				"<div style='text-align: center'>" +
					Localize(100) +
				"</div>" +
			"</div>" +
			"<div class='ph phDefault dn'>" +
				"<div class='result' style='padding-top: 20px'></div>" +
				"<div class='pager' style='padding-top: 10px; text-align: center'></div>" +
			"</div>" +
		"</div>" +
	"</div>");

	var first_time = true;

	var RedrawRecipeList = function()
	{
		var ph = $(".recipeList", dlg);
		var recipes = ph.data("recipes");

		if (!recipes || recipes.length < 1)
		{
			ph.switchPh("Empty");
			return;
		}

		$(".pager", ph)
			.show()
			.pager
			({
				items: recipes,
				autofire: false,
				changed: function(items, pageNumber, startIndex, endIndex, pager)
				{
					Config.recipeList.pageIndex = pageNumber;

					var html = [];
					html.push("<table style='width: 100%'>");

					for (var k = startIndex; k < endIndex + 1; k++)
					{
						var r = items[k];
						html.push
						(
							"<tr class='item' id='rcdlgItem" + k + "' itemId='" + r.id + "' recipeId='" + r.rid + "'>" +
								"<td class='itemIcon'>" +
									"<a href='" + AionArmoryUrl + "/item.aspx?id=" + r.id + "' class='stopPropagation selectLink quality" + r.q + "' target='_blank'><img src='" + ItemIconBaseDir + r.i + ".jpg' /></a>" +
								"</td>" +
								"<td class='itemInfo'>" +
									"<a href='" + AionArmoryUrl + "/item.aspx?id=" + r.id + "' class='stopPropagation selectLink quality" + r.q + "' target='_blank'>" + r.d + "</a>" +
									"<a href='javascript:;' class='openAionArmory' itemId='" + r.id + "' style='padding-left: 10px' target='_blank' tooltip='" + Localize(42) + "'><img src='/Images/arrow-045-small.png' /></a>" +
									"<a href='javascript:;' class='viewInformation' itemId='" + r.id + "' target='_blank' tooltip='" + Localize(59) + "'><img src='/Images/information-small.png' /></a>" +
									"<div style='overflow: hidden'>" +
										"<div class='comment' style='float: left'>" + r.l + " lvl</div>" +
										"<div style='float: left; padding-left: 10px;'><a class='comment hoverUnderline' href='" + AionArmoryUrl + "/recipe.aspx?id=" + r.rid + "' target='_blank'>" + Localize(101) + "</a></div>" +
									"</div>" +
								"</td>" +
								"<td class='recipeInfo comment'>" + Localize(102, r.sl) + "</td>" +
								"<td class='commands'><input type='button' class='select' value='" + Localize("ok") + "' /></td>" +
							"</tr>"
						);
					}

					html.push("</table>");

					var ph_default = ph.switchPh("Default");
					var result = $(".result", ph_default);
					result.html(html.join(""));

					ApplyDefaultActions(result);

					$("input:button.select, td.itemInfo, td.recipeInfo, td.commands", result).click(function()
					{
						var t = $(this).closest("tr.item");
						AddItem(t.attr("recipeId"), t.attr("itemId"));
						dlg.dialog("close");
					});

					$("a.selectLinkAionArmory", result).click(function(e)
					{
						e.stopPropagation();
					});

					$("a.selectLink", result).click(function()
					{
						var t = $(this).closest("tr.item");
						AddItem(t.attr("recipeId"), t.attr("itemId"));
						dlg.dialog("close");

						return false;
					});
				}
			});

		if (first_time)
		{
			$(".pager", ph).pagerSetPage(Config.recipeList.pageIndex);
			first_time = false;
		}
		else
			$(".pager", ph).pagerSetPage(0);
	};

	var SortResults = function()
	{
		var ph = $(".recipeList", dlg);
		var recipes = ph.data("recipes");

		if (recipes && recipes.length > 0 && Config.recipeList.sortBy && Config.recipeList.sortDirection)
		{
			var m = (Config.recipeList.sortDirection == "asc" ? 1 : -1);

			var DefaultSort = function(a, b)
			{
				if (a.q > b.q)
					return m;
				else if (a.q < b.q)
					return -m;
				else
				{
					if (a.l < b.l)
						return m;
					else if (a.l > b.l)
						return -m;
					else
					{
						if (a.d > b.d)
							return m;
						else if (a.d < b.d)
							return -m;
					}
				}

				return 0;
			}

			switch (Config.recipeList.sortBy)
			{
				case "sl":
					{
						recipes = recipes.sort(function(a, b)
						{
							if (a.sl > b.sl)
								return m;
							else if (a.sl < b.sl)
								return -m;

							return DefaultSort(a, b);
						});

						break;
					}

				case "l":
					{
						recipes = recipes.sort(function(a, b)
						{
							if (a.l > b.l)
								return m;
							else if (a.l < b.l)
								return -m;

							return DefaultSort(a, b);
						});

						break;
					}

				case "q":
					{
						recipes = recipes.sort(function(a, b)
						{
							if (a.q > b.q)
								return m;
							else if (a.q < b.q)
								return -m;

							return DefaultSort(a, b);
						});

						break;
					}

				default:
					throw "Unsupported sort type '" + sort_by + "'.";
			}

			ph.data("recipes", recipes);
		}

		RedrawRecipeList();
	};

	var FilterResults = function()
	{
		var ph = $(".recipeList", dlg);
		ph.removeData("recipes");

		if (Config.recipeList.race && Config.recipeList.skill)
		{
			var recipes = RecipeData;

			if (Config.recipeList.filter)
			{
				if (Config.recipeList.ignoreCase)
				{
					var filter_str = Config.recipeList.filter.toLowerCase();
					recipes = $.grep(recipes, function(x)
					{
						return x.d.toLowerCase().indexOf(filter_str) >= 0;
					});
				}
				else
				{
					recipes = $.grep(recipes, function(x)
					{
						return x.d.indexOf(Config.recipeList.filter) >= 0;
					});
				}
			}

			if (Config.recipeList.quality.length > 0)
			{
				var v = {};
				for (var k = 0; k < Config.recipeList.quality.length; k++)
					v[Config.recipeList.quality[k].toString()] = true;

				recipes = $.grep(recipes, function(x)
				{
					return v[x.q.toString()] == true;
				});
			}

			if (Config.recipeList.types.length > 0)
			{
				var v = {};
				for (var k = 0; k < Config.recipeList.types.length; k++)
					v[Config.recipeList.types[k].toString()] = true;

				recipes = $.grep(recipes, function(x)
				{
					return v[x.t.toString()] == true;
				});
			}

			if (Config.recipeList.procVersions.length > 0)
			{
				var v = {};
				for (var k = 0; k < Config.recipeList.procVersions.length; k++)
					v[Config.recipeList.procVersions[k].toString()] = true;

				recipes = $.grep(recipes, function(x)
				{
					return v[x.rt.toString()] == true;
				});
			}

			// item level
			{
				var min = Config.recipeList.itemLevelMin || 0;
				var max = Config.recipeList.itemLevelMax || 999;

				if (!isNaN(min) && !isNaN(max))
					recipes = $.grep(recipes, function(x)
					{
						return x.l >= min && x.l <= max;
					});
			}

			// skill level
			{
				var min = Config.recipeList.skillLevelMin || 0;
				var max = Config.recipeList.skillLevelMax || 999;

				if (!isNaN(min) && !isNaN(max))
					recipes = $.grep(recipes, function(x)
					{
						return x.sl >= min && x.sl <= max;
					});
			}

			ph.data("recipes", recipes);
		}

		SortResults();
	}

	var buttons = {};
	buttons[Localize("close")] = function() { dlg.dialog("close"); };

	dlg.dialog
	({
		autoOpen: true,
		modal: true,
		width: 1000,
		height: 800,
		buttons: buttons,
		open: function()
		{
			$(".filter", dlg).val(Config.recipeList.filter);
			//	$(".race", dlg).val(Config.recipeList.race);
			//	$(".skill", dlg).val(Config.recipeList.skill);
			$(".sortBy", dlg).val(Config.recipeList.sortBy);
			$("input:radio[name='sortDir'][value='" + Config.recipeList.sortDirection + "']", dlg).attr("checked", true);
			$(".lMin", dlg).val(Config.recipeList.itemLevelMin || "");
			$(".lMax", dlg).val(Config.recipeList.itemLevelMax || "");
			$(".slMin", dlg).val(Config.recipeList.skillLevelMin || "");
			$(".slMax", dlg).val(Config.recipeList.skillLevelMax || "");

			if (Config.recipeList.ignoreCase)
				$(".filterIgnoreCase", dlg).addClass("checked");

			for (var k = 0; k < Config.recipeList.quality.length; k++)
				$("input.quality[value=" + Config.recipeList.quality[k] + "]", dlg).attr("checked", true);

			for (var k = 0; k < Config.recipeList.types.length; k++)
				$("input.itemType[value=" + Config.recipeList.types[k] + "]", dlg).attr("checked", true);

			for (var k = 0; k < Config.recipeList.procVersions.length; k++)
				$("input.procVersions[value=" + Config.recipeList.procVersions[k] + "]", dlg).attr("checked", true);

			$(".filter", dlg).keyup(function()
			{
				var t = $(this);

				Config.recipeList.filter = $(this).val();
				var last_f = t.data("last");

				if (Config.recipeList.filter)
					Config.recipeList.filter = $.trim(Config.recipeList.filter);

				if (Config.recipeList.filter == last_f)
					return;

				t.data("last", Config.recipeList.filter);
				FilterResults();
			});

			//			$(".race, .skill", dlg).change(function()
			//			{
			//				Config.recipeList.filter = "";
			//				$(".filter", dlg).val("");

			//				Config.recipeList.race = $(".race", dlg).val();
			//				Config.recipeList.skill = $(".skill", dlg).val();

			//				FilterResults();
			//			});

			$(".butRaceAndSkill", dlg).click(function()
			{
				SelectRaceAndSkillDialog(function()
				{
					dlg.dialog("close");
					SelectItemDialog();
				});

				return false;
			});

			$(".sortBy", dlg).change(function()
			{
				Config.recipeList.sortBy = $(".sortBy", dlg).val();
				SortResults();
			});

			$("input:radio[name='sortDir']", dlg).bind("click change", function()
			{
				Config.recipeList.sortDirection = $("input:checked[name='sortDir']", dlg).val();
				SortResults();
			});

			$("input.quality", dlg).bind("click change", function()
			{
				Config.recipeList.quality = [];
				$("input.quality:checked", dlg).each(function()
				{
					Config.recipeList.quality.push(parseInt($(this).val(), 10));
				});

				FilterResults();
			});

			$("input.itemType", dlg).bind("click change", function()
			{
				Config.recipeList.types = [];
				$("input.itemType:checked", dlg).each(function()
				{
					Config.recipeList.types.push(parseInt($(this).val(), 10));
				});

				FilterResults();
			});

			$("input.procVersions", dlg).bind("click change", function()
			{
				Config.recipeList.procVersions = [];
				$("input.procVersions:checked", dlg).each(function()
				{
					Config.recipeList.procVersions.push(parseInt($(this).val(), 10));
				});

				FilterResults();
			});

			$(".lMin, .lMax, .slMin, .slMax", dlg).keyup(function()
			{
				Config.recipeList.itemLevelMin = parseInt($(".lMin", dlg).val(), 10);
				Config.recipeList.itemLevelMax = parseInt($(".lMax", dlg).val(), 10);
				Config.recipeList.skillLevelMin = parseInt($(".slMin", dlg).val(), 10);
				Config.recipeList.skillLevelMax = parseInt($(".slMax", dlg).val(), 10);

				FilterResults();
			});

			$(".reset", dlg).click(function()
			{
				$(".filter", dlg).val("");
				Config.recipeList.filter = null;

				$(".lMin", dlg).val("");
				Config.recipeList.itemLevelMin = null;

				$(".lMax", dlg).val("");
				Config.recipeList.itemLevelMax = null;

				$(".slMin", dlg).val("");
				Config.recipeList.skillLevelMin = null;

				$(".slMax", dlg).val("");
				Config.recipeList.skillLevelMax = null;

				$("input.quality", dlg).attr("checked", false);
				Config.recipeList.quality = [];

				$("input.itemType", dlg).attr("checked", false);
				Config.recipeList.types = [];

				$("input.procVersions", dlg).attr("checked", false);
				Config.recipeList.procVersions = [];

				$(".sortBy", dlg).val("sl");
				Config.recipeList.sortBy = "sl";

				$("input:radio[name='sortDir']", dlg).attr("checked", false);
				$("input:radio[name='sortDir'][value='asc']", dlg).attr("checked", true);
				Config.recipeList.sortDirection = "asc";

				FilterResults();

				return false;
			});

			$(".filterIgnoreCase", dlg).click(function()
			{
				var t = $(this);

				t.toggleClass("checked");
				Config.recipeList.ignoreCase = t.hasClass("checked");

				FilterResults();

				return false;
			});

			$("input:checkbox", dlg).bind("click change updateLightUp", function()
			{
				var t = $(this);

				if (t.attr("checked"))
					t.parent().addClass("lightUp");
				else
					t.parent().removeClass("lightUp");
			});

			FilterResults();
			InitContextAddons(dlg);
			ApplyDefaultActions(dlg);

			$("input:checkbox", dlg).trigger("updateLightUp");
			$(".filter", dlg).select().focus();
		},
		close: function(e, ui)
		{
			Config.Save();

			$(this)
				.hideErrors()
				.dialog("destroy")
				.remove();
		}
	});
}

function SelectRaceAndSkillDialog(okCallback)
{
	var html = [];

	html.push("<table style='width: 100%'><tr>");

	for (var race = 0; race < Races.length; race++)
	{
		html.push
		(
			"<td style='" + (race == 0 ? "padding-right: 10px" : "padding-left: 20px; border-left: solid 2px #F0F0F0;") + "'>" +
				"<h2 style='padding-bottom: 20px'>" + Races[race] + "</h2>"
		);

		for (var skill = 0; skill < Skills.length; skill++)
		{
			html.push("<a href='javascript:;' class='selectSkillButton' race='" + race + "' skill='" + skill + "'>" + Skills[skill] + "</a>");
		}

		html.push("</td>");
	}

	html.push("</tr></table>");

	var buttons = {};
	buttons[Localize("close")] = function() { dlg.dialog("close"); };

	var dlg = $("<div>" + html.join("") + "</div>");
	dlg.dialog
	({
		autoOpen: true,
		modal: true,
		width: 500,
		minHeight: 300,
		buttons: buttons,
		open: function()
		{
			$(".selectSkillButton", dlg).click(function()
			{
				var t = $(this);

				RecipeData = null;

				Config.recipeList.race = t.attr("race");
				Config.recipeList.skill = t.attr("skill");

				Config.recipeList.filter = null;
				Config.recipeList.types = [];

				if (okCallback)
					okCallback();

				dlg.dialog("close");

				return false;
			});
		},
		close: function(e, ui)
		{
			$(this)
				.hideErrors()
				.dialog("destroy")
				.remove();
		}
	});
}