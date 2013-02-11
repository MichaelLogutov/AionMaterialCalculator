function EditNumber(options)
{
	var opt = $.extend
	({
		value: 0,
		name: Localize(103),
		comment: null,
		additionalHtml: null,
		type: "integer",
		typeErrorMessage: Localize(51),
		minValue: 0,
		minValueErrorMessage: Localize(52),
		OnOk: function(n)
		{
			alert(n);
		}
	}, options);

	var dlg =
		$("<div>" +
			"<input type='text' maxlength='8' class='number req' formField='" + opt.name + "'" + (opt.comment ? " comment=\"" + opt.comment + "\"" : "") + " />" +
			(opt.additionalHtml ? opt.additionalHtml : "") +
		"</div>");

	var buttons = {};
	buttons[Localize("cancel")] = function()
	{
		dlg.dialog("close");
	};
	buttons[Localize("ok")] = function()
	{
		if (!$.validate("EditNumberDialog"))
			return false;

		if (opt.OnOk(parseInt($(".number", dlg).val(), 10)) == false)
			return;

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

			$(".number", dlg).val(opt.value);
			$(".number", dlg).select().focus().clickOnEnter($("button:contains('" + Localize("ok") + "')", dlg.next()));

			$.validatorsClear("EditNumberDialog");

			$.validator
			({
				group: "EditNumberDialog",
				targetSelector: $(".number", dlg),
				message: Localize(50)
			});
			$.validator
			({
				group: "EditNumberDialog",
				targetSelector: $(".number", dlg),
				type: opt.type,
				message: opt.typeErrorMessage
			});
			$.validator
			({
				group: "EditNumberDialog",
				targetSelector: $(".number", dlg),
				type: "compare",
				compareType: "greaterOrEqual",
				targetValue: opt.minValue,
				valueType: opt.type,
				message: opt.minValueErrorMessage
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

function FeedbackDialog()
{
	var dlg =
		$("<div>" +
			"<input type='text' maxlength='255' class='email req' formField='" + Localize(104) + "' size='50' />" +
			"<br />" +
			"<textarea class='message req' formField='" + Localize(105) + "' rows='15' style='width: 560px'></textarea>" +
		"</div>");

	var buttons = {};
	buttons[Localize("cancel")] = function()
	{
		dlg.dialog("close");
	};
	buttons[Localize("ok")] = function()
	{
		if (!$.validate("Feedback"))
			return false;

		Config.feedbackEmail = $(".email", dlg).val();
		Config.Save();

		var lb = ShowLoadingBox(Localize(110));
		$.jmsajax
		({
			url: "/Aion/Ajax.asmx/Feedback",
			data:
			{
				email: $(".email", dlg).val(),
				message: $(".message", dlg).val(),
				additionalInfo: "recipe id: " + $.queryString.hash["recipeId"] + ", item id: " + $.queryString.hash["itemId"]
			},
			cache: false,
			complete: function()
			{
				HideLoadingBox(lb);
				dlg.dialog("close");
			},
			success: function(data, textStatus)
			{
				if (data.Code != 0)
				{
					ShowMessageBox(data.Data);
					return;
				}

				ShowMessageBox(Localize(109));
			}
		});
	};

	dlg.dialog
	({
		autoOpen: true,
		modal: true,
		width: 600,
		minHeight: 10,
		open: function()
		{
			InitContextAddons(dlg);

			if (Config.feedbackEmail)
			{
				$(".email", dlg).val(Config.feedbackEmail);
				$(".message", dlg).focus();
			}
			else
				$(".email", dlg).focus();

			$.validatorsClear("Feedback");

			$.validator
			({
				group: "Feedback",
				targetSelector: $(".email", dlg),
				message: Localize(106)
			});
			$.validator
			({
				group: "Feedback",
				targetSelector: $(".email", dlg),
				type: "email",
				message: Localize(107)
			});
			$.validator
			({
				group: "Feedback",
				targetSelector: $(".message", dlg),
				message: Localize(108)
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

//function ConfigDialog()
//{
//	var dlg =
//		$("<div>" +
//			"<input type='text' maxlength='8' class='priceC req' formField='Vendor prices coefficient' comment='A coefficient for all vendor prices. Depends on ingame server taxes.' />" +
//		"</div>");

//	dlg.dialog
//	({
//		autoOpen: true,
//		modal: true,
//		minWidth: 300,
//		minHeight: 10,
//		open: function()
//		{
//			InitContextAddons(dlg);

//			$(".priceC", dlg).val(Config.priceCoefficient);
//			$(".priceC", dlg).focus();

//			$.validatorsClear("Config");

//			$.validator
//			({
//				group: "Config",
//				targetSelector: $(".priceC", dlg),
//				message: "You must enter a number"
//			});
//			$.validator
//			({
//				group: "Config",
//				targetSelector: $(".priceC", dlg),
//				type: "float",
//				message: "Please enter valid floating-point number"
//			});
//		},
//		buttons:
//		{
//			"Cancel": function()
//			{
//				dlg.dialog("close");
//			},
//			"OK": function()
//			{
//				if (!$.validate("Config"))
//					return false;

//				Config.priceCoefficient = parseFloat($(".priceC", dlg).val());
//				Config.Save();

//				dlg.dialog("close");

//				UpdateTotal();
//			}
//		},
//		close: function(e, ui)
//		{
//			$(this)
//				.hideErrors()
//				.dialog("destroy")
//				.remove();
//		}
//	});
//}

function Localize(id)
{
	if (typeof (id) == "number")
		id = "s" + id;
	else
		id = id.toString();

	var str = Localization[id] || "NOT_LOCALIZED_" + id;

	if (arguments.length > 1)
	{
		for (var k = 1; k < arguments.length; k++)
		{
			var r = new RegExp("\\{" + (k - 1) + "\\}", "ig");
			str = str.replace(r, arguments[k] || "");
		}
	}

	return str;
}