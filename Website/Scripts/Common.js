//
// Get xmlToJSON object element
//
function GetJsonElementText(name)
{
	var p = this[name];
	return p ? p[0].Text : null;
}

//
// Generate unique random id (as string)
//

var UniqueId = parseInt(new Date().getTime().toString().substr(8), 10);
function NewId()
{
	return UniqueId++;
}

//
// Log message in debug console
//
function log()
{
	if (typeof (console) != "undefined")
		console.log.apply(this, arguments);
	else if (typeof (opera) != "undefined")
		opera.postError(arguments);
}

//
// Convert urls into links in the specified text
//

var RegexUrlsInText = new RegExp("([a-z]{3,5}://)?([\\w\\-\\.\\/]+\\.(?:[a-z]{2,3}|aero|asia|cat|coop|edu|gov|int|jobs|mil|mobi|museum|tel|travelarpa|nato)[^\\s\\,\\.\\;\\-]*)", "ig");

function UrlsToLinks(text)
{
	if (!text)
		return null;

	var res = "";
	var prev_match_ends = 0;

	var match;
	while ((match = RegexUrlsInText.exec(text)) != null)
	{
		var protocol = match[1];
		var url = match[2];

		if (!protocol)
			protocol = "http://";

		var match_from = match.index;
		var match_length = (match[1] ? match[1].length : 0) + match[2].length;

		if (match_from > prev_match_ends)
			res += text.substr(prev_match_ends, match_from - prev_match_ends);

		res += "<noindex><a href='" + protocol + url + "' target='_blank' rel='nofollow'>" + url + "</a></noindex>";

		prev_match_ends = match_from + match_length;
	}

	if (prev_match_ends < text.length)
		res += text.substr(prev_match_ends);

	return res;
}

var RegexEmailsInText = new RegExp("([^\\s]+@[^\\s\\@\\,\\;]+)", "ig");
function EmailsToLinks(text)
{
	if (!text)
		return null;

	var res = "";
	var prev_match_ends = 0;

	var match;
	while ((match = RegexEmailsInText.exec(text)) != null)
	{
		var email = match[1];

		var match_from = match.index;
		var match_length = match[1].length;

		if (match_from > prev_match_ends)
			res += text.substr(prev_match_ends, match_from - prev_match_ends);

		res += "<noindex><a href='mailto:" + email + "' target='_blank' rel='nofollow'>" + email + "</a></noindex>";

		prev_match_ends = match_from + match_length;
	}

	if (prev_match_ends < text.length)
		res += text.substr(prev_match_ends);

	return res;
}

//
// Randomize items array
//
function RandomizeArray(items)
{
	var new_items = new Array();
	while (items.length > 0)
	{
		var item = items.splice(Math.round(Math.random() * (items.length - 1)), 1);
		new_items.push(item[0]);
	}

	return new_items;
}

jQuery.fn.outerHTML = function()
{
	return $('<div>').append(this.eq(0).clone()).html();
};

jQuery.fn.clickOnEnter = function(targetSelector)
{
	return this.each(function()
	{
		$(this).keypress(function(e)
		{
			if (e.which == 13 && e.target.type != 'textarea')
			{
				e.preventDefault();

				window.setTimeout(function()
				{
					$(targetSelector).click();
				}, 50);

				return false;
			}
		});
	});
};

//
// Aligns element(s) inside target element using specified align:
//		verticalAlign: top, center, bottom
//		horizontalAlign: left, center, right
//
jQuery.fn.alignInside = function(targetSelector, isAbsolute, verticalAlign, horizontalAlign)
{
	return this.each(function()
	{
		var t = $(this);
		var target = $(targetSelector);

		t.css({ position: isAbsolute ? "absolute" : "relative" });

		var x, y;
		var t_dx, t_dy;
		var target_dx, target_dy;

		if (isAbsolute)
		{
			var pos = target.offset();
			x = pos.left;
			y = pos.top;

			t_dx = t.outerWidth();
			t_dy = t.outerHeight();
			target_dx = target.outerWidth();
			target_dy = target.outerHeight();
		}
		else
		{
			x = 0;
			y = 0;
			t_dx = t.width();
			t_dy = t.height();
			target_dx = target.width();
			target_dy = target.height();
		}

		switch (horizontalAlign)
		{
			case "center":
				{
					x += (target_dx - t_dx) / 2;
					break;
				}

			case "right":
				{
					x += target_dx - t_dx;
					break;
				}
		}

		switch (verticalAlign)
		{
			case "center":
				{
					y += (target_dy - t_dy) / 2;
					break;
				}

			case "bottom":
				{
					y += target_dy - t_dy;
					break;
				}
		}

		t.css
		({
			left: x,
			top: y
		});
	});
};

//
// Image scaling
//

function _ImageResizeToLoadHandler()
{
	var t = $(this);
	var rparts = t.attr("resizeTo").split(';');
	var mx = parseInt(rparts[0], 10);
	var my = parseInt(rparts[1], 10);

	if (!isFinite(mx) || mx <= 0)
		mx = null;

	if (!isFinite(my) || my <= 0)
		my = null;

	if (!mx && !my)
		return;

	t.scaleImage(mx, my);
	t.css({ visibility: "visible" });
}

function _ImageScaleLoadHandler()
{
	var t = $(this);
	t.scaleImage(t.data("imageScaleMx"), t.data("imageScaleMy"));
}

jQuery.fn.getImageScaling = function(mx, my)
{
	var t = $(this);
	var x = t.width();
	var y = t.height();

	if (x <= 0)
		x = t.get(0).width;

	if (y <= 0)
		y = t.get(0).height;

	if (x <= 0 || y <= 0)
		return null;

	var a = 1.0;

	if (mx && my)
	{
		if (x > y)
			a = mx / x;
		else
			a = my / y;
	}
	else if (mx)
	{
		a = mx / x;
	}
	else if (my)
	{
		a = my / y;
	}

	var res =
	{
		width: x,
		height: y,
		a: a,
		scaledWidth: Math.round(x * a),
		scaledHeight: Math.round(y * a)
	};

	return res;
}

jQuery.fn.scaleImage = function(mx, my)
{
	if (!mx && !my)
		return;

	var t = $(this);
	var s = t.getImageScaling(mx, my);
	if (!s)
	{
		// image not yet loaded

		t
			.data("imageScaleMx", mx)
			.data("imageScaleMy", my)
			.unbind("load", _ImageScaleLoadHandler)
			.bind("load", _ImageScaleLoadHandler);

		return;
	}

	t.css
	({
		width: s.scaledWidth,
		height: s.scaledHeight
	});
}

//
// Placeholder functions
//

jQuery.fn.getPh = function(name)
{
	return this.children(".ph" + name);
};

jQuery.fn.switchPh = function(name)
{
	this.children(".ph").not(".ph" + name).hide();
	return this.getPh(name).show();
};

jQuery.fn.getControl = function(className)
{
	return $("." + className, this);
};

//
// Parse string to date object by "dd.MM.yyyy" format and (optionaly) time in "hh:mm" format
//
function ParseDate(str, time)
{
	try
	{
		regexDate = /^\s*(\d{2})\.(\d{2})\.(\d{4})\s*.*$/i;

		parts = regexDate.exec(str);
		if (!parts || parts.length != 4)
			return null;

		var res = new Date(parts[3], parts[2] - 1, parts[1]);

		if (time)
		{
			var time_parts = time.split(":");
			if (time_parts.length > 1)
			{
				var h = parseInt(time_parts[0], 10);
				var m = parseInt(time_parts[1], 10);

				if (!isNaN(h) && !isNaN(m))
				{
					res.setHours(h);
					res.setMinutes(m);
				}
			}
		}

		return res;
	}
	catch (ex)
	{
		return null;
	}
}


//
// Parse string to date object by "MM/dd/yyyy hh:mm:ss" format
//
function ParseDateSQL(str)
{
	try
	{
		regexDate = /^\s*(\d{2})\/(\d{2})\/(\d{4})\s+(\d{2})\:(\d{2})\:(\d{2})$/i;

		parts = regexDate.exec(str);
		if (!parts || parts.length != 7)
			return null;

		return new Date(parts[3], parts[1] - 1, parts[2], parts[4], parts[5], parts[6]);
	}
	catch (ex)
	{
		return null;
	}
}

//
// Parse string to date object by "yyyy-mm-dd hh:mi:ss" (24h) format
//
function ParseDateSQL120(str)
{
	try
	{
		regexDate = /^\s*(\d{4})-(\d{2})-(\d{2})\s+(\d{2})\:(\d{2})\:(\d{2})$/i;

		parts = regexDate.exec(str);
		if (!parts || parts.length != 7)
			return null;

		return new Date(parts[1], parts[2] - 1, parts[3], parts[4], parts[5], parts[6]);
	}
	catch (ex)
	{
		return null;
	}
}

//
// Calculates date differentse (in days) between two dates)
//
function DateDiff(date1, date2)
{
	return Math.abs(date2.getTime() - date1.getTime()) / (1000 * 60 * 60 * 24);
}

//
// Calculates date differentse (in years) between two dates)
//
function DateDiffYears(date1, date2)
{
	if (date1 > date2)
	{
		var tmp = date1;
		date1 = date2;
		date2 = tmp;
	}

	var res = date2.getFullYear() - date1.getFullYear() - 1;

	date2.setFullYear(date1.getFullYear());

	if (date1 < date2)
		res++;

	return res;
}

//
// This function will shows information message with a popup window (id = "msgBox")
//
function ShowMessageBox(message, close)
{
	var dlg = $(
		"<div>" +
			"<div class='message'>&nbsp;</div>" +
			"<div style='text-align: center; padding-top: 20px'>" +
				"<input class='butClose' type='button' value='OK' style='width: 75px' />" +
			"</div>" +
		"</div>");

	dlg.dialog
	({
		autoOpen: true,
		modal: true,
		width: 500,
		minHeight: 10,
		close: function(e, ui)
		{
			if (close && typeof (close) == "function")
				close();

			$(this)
				.hideErrors()
				.dialog("destroy")
				.remove();
		}
	});

	$(".message", dlg).html(message);

	$(".butClose", dlg)
		.click(function()
		{
			dlg.dialog("close");
			return false;
		})
		.focus();
}

//
// This function will shows yes/no message with a popup window (id = "yesNoBox")
//
function ShowYesNoBox(message, yes, no)
{
	dlg = $(
		"<div>" +
			"<div class='message'>&nbsp;</div>" +
			"<div style='text-align: center; padding: 10px 0;'>" +
				"<input class='butYes' style='width: 50px;' type='button' value='Да' style='width: 75px' />" +
				"<input class='butNo' style='width: 50px;' type='button' value='Нет' style='width: 75px' />" +
			"</div>" +
		"</div>"
	);

	dlg.dialog
	({
		autoOpen: true,
		modal: true,
		minHeight: 10,
		close: function(e, ui)
		{
			$(this)
				.hideErrors()
				.dialog("destroy")
				.remove();
		}
	});

	$(".message", dlg).html(message);

	$(".butYes", dlg).click(function()
	{
		if (yes && typeof (yes) == "function")
			yes();

		dlg.dialog("close");
		return false;
	});

	$(".butNo", dlg).click(function()
	{
		if (no && typeof (no) == "function")
			no();

		dlg.dialog("close");
		return false;
	});
}

//
// Opens "loading" popup dialog
//
function ShowLoadingBox(text)
{
	var dlg = $(
		"<div style='overflow: hidden'>" +
			"<div style='float: left; padding: 2px 10px 0 0'><img src='/Images/loading.gif' alt='' /></div>" +
			"<div style='float: left'>" + (text ? text : "Loading...") + "</div>" +
		"</div>"
	);

	dlg.dialog
	({
		autoOpen: true,
		modal: true,
		minHeight: 10,
		close: function()
		{
			$(this)
					.hideErrors()
					.dialog("destroy")
					.remove();
		}
	});

	return dlg;
}

//
// Set text for loading dialog
//
function SetLoadingBoxText(dlg, text)
{
	$(".lbText", dlg).text(text);
}

//
// Hides "loading" popup dialog
//
function HideLoadingBox(dlg)
{
	dlg.dialog("close");
}

//
// Date time formatting
//
Date.prototype.format = function(format)
{
	var months = new Array("Января", "Февраля", "Марта", "Апреля", "Мая", "Июня", "Июля", "Августа", "Сентября", "Октября", "Ноября", "Декабря");
	var yyyy = this.getFullYear();
	var yy = yyyy.toString().substring(2);
	var m = this.getMonth() + 1;
	var mm = m < 10 ? "0" + m : m;
	var mmm = months[m - 1];
	var d = this.getDate();
	var dd = d < 10 ? "0" + d : d;

	var h = this.getHours();
	var hh = h < 10 ? "0" + h : h;
	var n = this.getMinutes();
	var nn = n < 10 ? "0" + n : n;
	var s = this.getSeconds();
	var ss = s < 10 ? "0" + s : s;

	format = format.replace(/yyyy/i, yyyy);
	format = format.replace(/yy/i, yy);
	format = format.replace(/mmm/i, mmm);
	format = format.replace(/mm/i, mm);
	format = format.replace(/m/i, m);
	format = format.replace(/dd/i, dd);
	format = format.replace(/d/i, d);
	format = format.replace(/hh/i, hh);
	format = format.replace(/h/i, h);
	format = format.replace(/nn/i, nn);
	format = format.replace(/n/i, n);
	format = format.replace(/ss/i, ss);
	format = format.replace(/s/i, s);

	return format;
}

//
// Number formatting
//
Number.prototype.toStringFormatted = function()
{
	var s = this.toString();

	var res = "";
	var d = 0;

	for (k = s.length - 1; k >= 0; k--)
	{
		if (d == 3)
		{
			res = " " + res;
			d = 0;
		}

		var ch = s.charAt(k);
		var n = parseInt(ch, 10);
		if (n >= 0 && n <= 9)
			d++;

		res = ch + res;
	}

	return res;
}

Number.prototype.getStringPlural = function(single, lessThanFive, plural)
{
	if (isNaN(this))
		return "";

	var n = Math.abs(this);
	var s = n.toString();
	if (!s)
		return null;

	var last_digit = parseInt(s[s.length - 1], 10);

	if (last_digit == 1)
		return single;
	else if (last_digit > 1 && last_digit < 5)
		return lessThanFive;
	else
		return plural;
}

if (typeof (Array.prototype.indexOf) == "undefined")
{
	Array.prototype.indexOf = function(item)
	{
		for (var k = 0; k < this.length; k++)
			if (this[k] == item)
			return k;

		return -1;
	}
}

//
// String capitalization
//
String.prototype.capitalize = function()
{
	if (!this)
		return this;

	return this.charAt(0).toLocaleUpperCase() + this.substring(1, this.length);
}

//
// Handles google map api load
//
function GoogleMapsLoadFailed()
{
	$(document).trigger("GoogleMapsLoadFailed");
}

function GoogleMapsLoaded()
{
	$(document).trigger("GoogleMapsLoaded");

	if (typeof (google) == "undefined" || typeof (google.maps) == "undefined")
	{
		GoogleMapsLoadFailed()
		return;
	}

	$(document.body).unload(function()
	{
		GUnload();
	});
}

//
// Startup initialization
//

function InitContextAddons(context)
{
	context = $(context);

	// setup tooltips
	$(".tp", context).each(function()
	{
		var t = $(this);
		t.data("tp", t.next().html()).tooltip
		({
			bodyHandler: function() { return $(this).data("tp"); },
			showURL: false,
			fixPNG: true,
			extraClass: t.attr("tooltipExtraClass")
		});
	})

	// resize images
	$("img[resizeTo]", context).each(function()
	{
		var t = $(this);

		var rparts = t.attr("resizeTo").split(';');
		var mx = parseInt(rparts[0], 10);
		var my = parseInt(rparts[1], 10);

		if (t.get(0).width > 0 && t.get(0).height > 0)
			t.scaleImage(mx, my);
		else
		{
			t
				.css({ visibility: "hidden" })
				.unbind("load refresh", _ImageResizeToLoadHandler)
				.bind("load refresh", _ImageResizeToLoadHandler);
		}
	});

	$(".calculateAge[birthday]", context).each(function()
	{
		var t = $(this);
		var age = DateDiffYears(new Date(), ParseDate(t.attr("birthday")));
		if (!age || !isFinite(age))
			return;

		//t.text(age + " " + age.getStringPlural("год", "года", "лет"));
		t.text(age + " years");
	});

	$(".urlsToLinks", context).each(function()
	{
		var t = $(this);
		t.html(UrlsToLinks(t.html()));
	});

	$(".emailsToLinks", context).each(function()
	{
		var t = $(this);
		t.html(EmailsToLinks(t.html()));
	});

	$("*[formField]", context).each(function()
	{
		var t = $(this);
		var id = t.attr("id");

		if (!id)
		{
			id = "_c" + NewId();
			t.attr("id", id);
		}

		t.wrap("<div class='formField'></div>");

		var label = $("<label class='title' for='" + id + "'></label>");
		label.html(t.attr("formField"));

		if (t.hasClass("req"))
			label.append("<span class='required' title='This field is required'>*</span>");

		t.before(label);

		var comment = t.attr("comment");
		if (comment)
			t.after("<span class='comment'>" + comment + "</span>");
	});
}

$(function()
{
	InitContextAddons(document);

	$.queryString.load();

	$.datepicker.setDefaults
	({
		dateFormat: 'dd.mm.yy',
		showOn: 'button',
		buttonImage: '/Static/Images/Calendar16x16.gif',
		buttonText: 'Change date',
		buttonImageOnly: true
	});
});