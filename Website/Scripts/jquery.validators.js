//
// Author: Logutov Michael
// Date: 03 december 2008
// Description: This plugin handle form validations
//

var validateRegexRequired = /\s*[^\s]+\s*/i;
var validateRegexEmail = /^\s*[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})\s*$/i;
var validateRegexDate = /^\s*(\d{2})\.(\d{2})\.(\d{4})\s*$/i;
var validateRegexTime = /^\s*(\d{2})\:(\d{2})\s*$/i;
var validateRegexFloat = /^\s*[+-]?\d*\.?\d*\s*$/i;
var validateRegexInteger = /^\s*[+-]?\d+\s*$/i;
var ValidatorErrorOptionsKey = "validatorError.options";

//
// This function will shows error message inside element by specified selector
//
function ShowError(errorElementSelectorOrObject, message)
{
	var t = $(errorElementSelectorOrObject);

	if (!t.data("errorDisplayed"))
		t.data("oldHtml", t.html());

	t.data("errorDisplayed", true).addClass("error").html(message).show();
}

//
// This function will clears error message inside element by specified selector
//
function HideError(errorElementSelectorOrObject)
{
	var t = $(errorElementSelectorOrObject);

	if (t.data("errorDisplayed"))
		t.removeData("errorDisplayed").removeClass("error").html(t.data("oldHtml") || "");
}

; (function($)
{
	$.validatorsClear = function(group)
	{
		$(document).data("validators" + group, null);
	};

	$.validator = function(options)
	{
		opt = $.extend
		({
			group: "Default",
			targetSelector: null, 	// can be selector (single element only) or object
			type: "required",
			message: "Validate error",
			zIndex: 99999,
			showError: function(target, message)
			{
				target
					.attr("error", 1)
					.showValidatorError({ content: message, zIndex: this.zIndex });
			},
			clearError: function()
			{
				var t = $(this.targetSelector);

				t
					.attr("error", 0)
					.hideValidatorError();
			},
			isEnabled: function()
			{
				return true;
			}
		}, options);

		if (!opt.targetSelector)
			throw "No targetSelector specified for validator";

		v = opt;

		validators = $(document).data("validators" + v.group);
		if (!validators)
		{
			validators = new Array();
			$(document).data("validators" + v.group, validators);
		}
		validators.push(v);


		if (typeof (v.type) == "function")
		{
			v.validate = v.type;
		}
		else
		{
			switch (v.type)
			{
				case "required":
					{
						v.validate = function(v, target)
						{
							if (!validateRegexRequired.test(target.val()))
								return v.message;

							return null;
						};

						break;
					}

				case "regex":
					{
						v.validate = function(v, target)
						{
							if (!v.regex.test(target.val()))
								return v.message;

							return null;
						};

						break;
					}

				case "email":
					{
						v.validate = function(v, target)
						{
							if (!validateRegexEmail.test(target.val()))
								return v.message;

							return null;
						};

						break;
					}

				case "date":
					{
						v.validate = function(v, target)
						{
							if (!validateRegexDate.test(target.val()))
								return v.message;

							var dt;
							try
							{
								dt = ParseDate(target.val());

								if (!dt)
									return v.message;
							}
							catch (e)
							{
								return v.message;
							}

							if (v.minDate)
							{
								check_dt = new Date(v.minDate.getFullYear(), v.minDate.getMonth(), v.minDate.getDate());
								if (dt < check_dt)
									return v.message;
							}

							if (v.maxDate)
							{
								check_dt = new Date(v.maxDate.getFullYear(), v.maxDate.getMonth(), v.maxDate.getDate());
								if (dt > check_dt)
									return v.message;
							}

							return null;
						};

						break;
					}

				case "time":
					{
						v.validate = function(v, target)
						{
							if (!validateRegexTime.test(target.val()))
								return v.message;

							var t = target.val().split(":");
							if (t.length != 2)
								return v.message;

							var h = parseInt(t[0]);
							var m = parseInt(t[1]);

							if (h < 0 || h > 24 || m < 0 || m > 59)
								return v.message;

							return null;
						};

						break;
					}

				case "float":
					{
						v.validate = function(v, target)
						{
							try
							{
								if (!validateRegexFloat.test(target.val()))
									return v.message;
							}
							catch (e)
							{
								return v.message;
							}

							return null;
						};

						break;
					}

				case "integer":
					{
						v.validate = function(v, target)
						{
							try
							{
								if (!validateRegexInteger.test(target.val()))
									return v.message;
							}
							catch (e)
							{
								return v.message;
							}

							return null;
						};

						break;
					}

				case "compare":
					{
						v.validate = function(v, target)
						{
							var a = target.val();
							var b = v.targetSelector2 ? $(v.targetSelector2).val() : v.targetValue;

							if (!a || !b)
								return null;

							switch (v.valueType)
							{
								case "integer":
									{
										a = parseInt(a);
										b = parseInt(b);

										if (isNaN(a) || isNaN(b))
											return null;

										break;
									}

								case "float":
									{
										a = parseFloat(a);
										b = parseFloat(b);

										if (isNaN(a) || isNaN(b))
											return null;

										break;
									}

								case "date":
									{
										a = ParseDate(a);
										b = ParseDate(b);

										if (!a || !b)
											return null;

										break;
									}
							}

							switch (v.compareType)
							{
								case "equal":
									{
										if (!(a == b))
											return v.message;

										break;
									}

								case "less":
									{
										if (!(a < b))
											return v.message;

										break;
									}

								case "lessOrEqual":
									{
										if (!(a <= b))
											return v.message;

										break;
									}

								case "greater":
									{
										if (!(a > b))
											return v.message;

										break;
									}

								case "greaterOrEqual":
									{
										if (!(a >= b))
											return v.message;

										break;
									}

								case "notEqual":
									{
										if (!(a != b))
											return v.message;

										break;
									}

								default:
									throw "Invalid or unsupported compare type '" + v.compareType + "'";
							}

							return null;
						};

						break;
					}

				case "mod10":
					{
						v.validate = function(v, target)
						{
							// Strip any non-digits (useful for credit card numbers with spaces and hyphens)
							var number = target.val().replace(/\D/g, '');

							// Set the string length and parity
							var number_length = number.length;
							var parity = number_length % 2;

							// Loop through each digit and do the maths
							var total = 0;
							for (i = 0; i < number_length; i++)
							{
								var digit = number.charAt(i);
								// Multiply alternate digits by two
								if (i % 2 == parity)
								{
									digit = digit * 2;
									// If the sum is two digits, add them together (in effect)
									if (digit > 9)
									{
										digit = digit - 9;
									}
								}
								// Total up the digits
								total = total + parseInt(digit);
							}

							// If the total mod 10 equals 0, the number is valid
							if (total % 10 == 0)
								return null;

							return v.message;
						};

						break;
					}

				default:
					throw "Unsupported validator type '" + v.type + "'";
			}
		}

		v.performValidation = function(target)
		{
			if (!this.isEnabled() || (this.type != "required" && !target.val()))
				return true;

			message = this.validate(this, target);

			if (message)
			{
				this.showError(target, message);

				//target.focus();
				return false;
			}
			else
				return true;
		}

		// save validator into array associated with the target element
		target_validators = $(v.targetSelector).data("validators");
		if (!target_validators)
		{
			target_validators = new Array();
			$(v.targetSelector).data("validators", target_validators);
		}
		target_validators.push(v);

		// validate on target value possibly changed
		$(v.targetSelector).bind("change", function()
		{
			target_validators = $(this).data("validators");

			// clear all errors first
			for (k = target_validators.length - 1; k >= 0; k--)
				target_validators[k].clearError();

			// now validate
			for (k = target_validators.length - 1; k >= 0; k--)
			{
				v = target_validators[k];
				v.performValidation($(this));
			}
		});
	};

	$.validate = function(group)
	{
		var validators = $(document).data("validators" + group);
		if (!validators)
			throw new "No validators defined for group '" + group + "'";

		var is_valid = true;

		for (var k = validators.length - 1; k >= 0; k--)
			validators[k].clearError();

		for (k = validators.length - 1; k >= 0; k--)
		{
			try
			{
				var v = validators[k];

				$(v.targetSelector).each(function()
				{
					if (!v.performValidation($(this)))
						is_valid = false;
				});
			}
			catch (ex)
			{
				is_valid = false;
			}
		}

		return is_valid;
	};

	$.validatorsHideErrors = function(group)
	{
		var validators = $(document).data("validators" + group);
		if (!validators)
			throw new "No validators defined for group '" + group + "'";

		for (k = 0; k < validators.length; k++)
			validators[k].clearError();
	};

	$.fn.extend
	({
		hideErrors: function()
		{
			return this.find("*[error=1]").each(function()
			{
				var t = $(this);

				t
					.attr("error", 0)
					.hideValidatorError();
			});
		},

		//
		// Hides validator error
		//
		hideValidatorError: function()
		{
			var t = $(this);
			var opt = t.data(ValidatorErrorOptionsKey, opt);
			if (opt)
			{
				opt.controls.p
							.stop(true)
							.remove();

				t.removeData(ValidatorErrorOptionsKey);
			}
		},

		//
		// Shows validator error message near the control
		//
		showValidatorError: function(options)
		{
			var t = $(this);
			t.hideValidatorError();

			if (typeof (options) == "string")
				options = { content: options };

			var opt = $.extend
			({
				content: null,
				animate:
				{
					left: "+=10"
				},
				zIndex: 100
			}, options);

			if (!opt.content)
				throw new "Not content specified for showValidatorError.";

			t.data(ValidatorErrorOptionsKey, opt);

			opt.controls = new Object();
			opt.controls.p = $(
				"<div class='validatorError' style='position: absolute; display: none; z-index: " + opt.zIndex + "'>" +
					"<div class='content'></div>" +
				"</div>");

			$(document.body).append(opt.controls.p);

			if (typeof (opt.content) == "function")
				$(".content", opt.controls.p).html(opt.content(opt));
			else
				$(".content", opt.controls.p).html(opt.content);

			var pos = t.offset();

			opt.controls.p
				.css
				({
					left: pos.left + t.outerWidth(),
					top: pos.top - 3,
					opacity: 0,
					display: "block"
				})
				.animate($.extend({ opacity: 1 }, opt.animate), 200);
		}
	});

})(jQuery);