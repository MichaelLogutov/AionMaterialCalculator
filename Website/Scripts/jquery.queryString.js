//
// Author: Logutov Michael
// Date: 04 december 2008
// Description: This plugin handle query string operations
// Methods:
//		load() - parses current url hash into $.queryString.hash
//		save() - saves $.queryString.hash values into current url hash. Only values that has not null value will be saved.
//

; (function($)
{
	$.queryString =
	{
		hashCheckOnChangedCallback: null,
		hashCheckTimer: null,
		hashCheckLastHash: null
	};

	$.queryString.parse = function(str)
	{
		var parts = str.split('|');
		var regex_name_value = /^\s*(.*?)\s*\=\s*(.*?)\s*$/i;
		var res = new Object();

		for (k = 0; k < parts.length; k++)
		{
			var part = $.trim(parts[k]);

			if (!part || part.length < 1)
				continue;

			var sub_parts = regex_name_value.exec(part);
			if (sub_parts)
				res[sub_parts[1]] = sub_parts[2];
			else
				res[part] = part;
		}

		return res;
	}

	$.queryString.load = function()
	{
		this.hash = new Object();
		if (document.location.hash)
			this.hash = $.queryString.parse(window.location.hash.substring(1));
	}

	$.queryString.save = function()
	{
		$.queryString.hashCheckerDisable();

		if (!this.hash)
			return;

		var new_hash = null;
		$.each(this.hash, function(key, val)
		{
			if (val == null)
				return;

			if (new_hash)
				new_hash += "|";
			else
				new_hash = "#";

			if (key != val)
				new_hash += key + "=" + val;
			else
				new_hash += key;
		});

		window.location.hash = new_hash;
		$.queryString.hashCheckerEnable();
	}

	$.queryString.hashCheckerInit = function(callback)
	{
		this.hashCheckOnChangedCallback = callback;
	}

	$.queryString.hashCheckerDisable = function()
	{
		if (this.hashCheckTimer)
		{
			window.clearInterval(this.hashCheckTimer);
			this.hashCheckTimer = null;
		}
	}

	$.queryString.hashCheckerEnable = function()
	{
		this.hashCheckerDisable();
		this.hashCheckLastHash = document.location.hash;

		if (this.hashCheckOnChangedCallback)
		{
			this.hashCheckTimer = window.setInterval(function()
			{
				if (document.location.hash != $.queryString.hashCheckLastHash)
					$.queryString.hashCheckOnChangedCallback();
			}, 300);
		}
	}

})(jQuery);