using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Website.Code
{
	/// <summary>
	/// <para>XSLT helper methods extension.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 05 august 2009</para>
	/// </summary>
	public class XsltScriptHelpers
	{
		#region Accessors

		#region Data

		/// <summary>
		/// <see cref="Data"/> internal field.
		/// </summary>
		private Dictionary<string, string> _Data = new Dictionary<string, string> ();

		/// <summary>
		/// (GET, SET) Data dictionary that can be used to store some information from XSLT. Default value is empty list.
		/// </summary>
		[Browsable (false)]
		public Dictionary<string, string> Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				this._Data = value;
			}
		}

		#endregion

		#endregion

		#region Static helpers

		/// <summary>
		/// Return new instance of <see cref="XsltArgumentList"/> object with added <see cref="XsltScriptHelpers"/> as extension object.
		/// </summary>
		/// <returns>New instance of <see cref="XsltArgumentList"/>.</returns>
		public static XsltArgumentList GetDefaultArguments ()
		{
			var xslt_args = new XsltArgumentList ();
			xslt_args.AddExtensionObject ("urn:script", new XsltScriptHelpers ());

			return xslt_args;
		}

		#endregion

		#region String helpers

		/// <summary>
		/// Prints specified <paramref name="xml"/> as HTML.
		/// </summary>
		/// <param name="xml">Source XML.</param>
		/// <returns>HTML representation of XML.</returns>
		public string PrintXml (XPathNodeIterator xml)
		{
			if (xml == null)
				return string.Empty;

			using (TextWriter tw = new StringWriter ())
			{
				var xw = new XmlTextWriter (tw);
				xw.Formatting = Formatting.Indented;
				xw.Indentation = 1;
				xw.IndentChar = '\t';

				if (!xml.MoveNext ())
					return null;

				xml.Current.WriteSubtree (xw);

				return tw.ToString ().Replace ("<", "&lt;").Replace (">", "&gt;");
			}
		}

		/// <summary>
		/// Replace string with another string.
		/// </summary>
		/// <param name="str">Source string.</param>
		/// <param name="oldValue">Old value to be replaced.</param>
		/// <param name="newValue">Replacement string.</param>
		/// <returns>Modified string.</returns>
		public string Replace (string str, string oldValue, string newValue)
		{
			return str.Replace (oldValue, newValue);
		}

		/// <summary>
		/// Return capitalized string.
		/// </summary>
		/// <param name="str">Source string.</param>
		/// <returns>Capitalized string.</returns>
		public string Capitalize (string str)
		{
			if (string.IsNullOrEmpty (str))
				return string.Empty;

			return str.Substring (0, 1).ToUpper () + str.Substring (1).ToLower ();
		}

		/// <summary>
		/// Return string in lower case.
		/// </summary>
		/// <param name="str">Source string.</param>
		/// <returns>String in lower case.</returns>
		public string ToLower (string str)
		{
			if (string.IsNullOrEmpty (str))
				return string.Empty;

			return str.ToLower ();
		}

		/// <summary>
		/// Removes escaped CDDATA definition.
		/// </summary>
		/// <param name="str">Source string.</param>
		/// <returns>Procceeded string.</returns>
		public string RemoveCddata (string str)
		{
			return Regex.Replace (str, @"^\<\!\[CDATA\[(.*?)\]\]\>$", "$1", Utils.RegexDefaultOptions);
		}

		/// <summary>
		/// Separates string by inserting <paramref name="separator"/> each specified <paramref name="step"/> chars counting from the end.
		/// </summary>
		/// <param name="str">Source string.</param>
		/// <param name="separator">Separator.</param>
		/// <param name="step">Step.</param>
		/// <returns>Result string.</returns>
		public string Separate (string str, string separator, int step)
		{
			if (string.IsNullOrEmpty (str))
				return string.Empty;

			var res = new StringBuilder ();
			var cur_step = 0;

			for (var k = str.Length - 1; k >= 0; k--)
			{
				if (cur_step == step)
				{
					cur_step = 0;
					res.Insert (0, separator);
				}

				res.Insert (0, str[k]);

				cur_step++;
			}

			return res.ToString ();
		}

		/// <summary>
		/// Converts all found URLs within specified text into links (HTML anchors).
		/// </summary>
		/// <param name="text">Source text.</param>
		/// <returns>Converted text.</returns>
		public string UrlsToLinks (string text)
		{
			return Utils.UrlsToLinks (text);
		}

		/// <summary>
		/// Converts new lines within specified text into &lt;br /&gt;.
		/// </summary>
		/// <param name="text">Source text.</param>
		/// <returns>Converted text.</returns>
		public string NewLinesToBreaks (string text)
		{
			return text.Replace ("\n", "<br />");
		}

		#endregion

		#region Data methods

		/// <summary>
		/// Gets data value by specified <paramref name="key"/>.
		/// </summary>
		/// <param name="key">Data key.</param>
		/// <returns>Data value or null if no data with specified key was found.</returns>
		public string GetData (string key)
		{
			string value;
			if (!this.Data.TryGetValue (key, out value))
				return null;

			return value;
		}

		/// <summary>
		/// Sets data value with specified <paramref name="key"/>.
		/// </summary>
		/// <param name="key">Data key.</param>
		/// <param name="value">Data value</param>
		public void SetData (string key, string value)
		{
			if (this.Data.ContainsKey (key))
				this.Data[key] = value;
			else
				this.Data.Add (key, value);
		}

		/// <summary>
		/// Determines if specified date is in future.
		/// </summary>
		/// <param name="dt">Date string.</param>
		/// <param name="format">Date string format (can be null).</param>
		public bool IsFutureDate (string dt, string format)
		{
			DateTime d;
			if (string.IsNullOrEmpty (format))
				d = DateTime.Parse (dt, CultureInfo.InvariantCulture);
			else
				d = DateTime.ParseExact (dt, format, CultureInfo.InvariantCulture);

			return (d > DateTime.Now);
		}

		#endregion

		#region Date time

		/// <summary>
		/// Format specified number <paramref name="value"/> as money value.
		/// </summary>
		/// <param name="value">Source number.</param>
		/// <returns>Formatted value.</returns>
		public string FormatNumberAsMoney (string value)
		{
			double v;
			if (double.TryParse (value.Replace (',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out v))
				return v.ToString ("#.##", CultureInfo.InvariantCulture);

			return value;
		}

		/// <summary>
		/// Gets current date in specified format.
		/// </summary>
		/// <param name="format">Format speicification.</param>
		/// <returns>Formatted date time value string.</returns>
		public string GetCurrentDate (string format)
		{
			return DateTime.Now.ToString (format, Utils.RuCulture);
		}

		/// <summary>
		/// Return ticks for current date.
		/// </summary>
		/// <returns>Ticks number.</returns>
		public long GetCurrentDateTicks ()
		{
			return DateTime.Now.Ticks;
		}

		/// <summary>
		/// Formats date time.
		/// </summary>
		/// <param name="dt">Date time value.</param>
		/// <param name="format">Format speicification.</param>
		/// <returns>Formatted date time value string.</returns>
		public string FormatDateTime (string dt, string format)
		{
			if (!string.IsNullOrEmpty (dt))
			{
				DateTime d;
				if (DateTime.TryParse (dt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d))
					return d.ToString (format, Utils.RuCulture);
			}

			return string.Empty;
		}

		/// <summary>
		/// Formats date time.
		/// </summary>
		/// <param name="dt">Date time value.</param>
		/// <param name="format">Format speicification</param>
		/// <returns>Formatted date time value string.</returns>
		public string FormatDateTimeDMY (string dt, string format)
		{
			if (!string.IsNullOrEmpty (dt))
			{
				DateTime d;
				if (DateTime.TryParseExact (dt, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d))
					return d.ToString (format, Utils.RuCulture);
			}

			return string.Empty;
		}

		/// <summary>
		/// Formats date time using <see cref="DateTime.ToLongTimeString"/>.
		/// </summary>
		/// <param name="dt">Date time value.</param>
		/// <returns>Formatted date time value string.</returns>
		public string FormatDateTimeToLongTimeString (string dt)
		{
			if (!string.IsNullOrEmpty (dt))
			{
				DateTime d;
				if (DateTime.TryParse (dt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d))
					return d.ToLongTimeString ();
			}

			return string.Empty;
		}

		/// <summary>
		/// Formats date time using <see cref="DateTime.ToLongDateString"/>.
		/// </summary>
		/// <param name="dt">Date time value.</param>
		/// <returns>Formatted date time value string.</returns>
		public string FormatDateTimeToLongDateString (string dt)
		{
			if (!string.IsNullOrEmpty (dt))
			{
				DateTime d;
				if (DateTime.TryParse (dt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d))
					return d.ToLongDateString ();
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns age based on specified birth date.
		/// </summary>
		/// <param name="dt">Birth date</param>
		/// <returns>Age number.</returns>
		public string GetAge (string dt)
		{
			if (!string.IsNullOrEmpty (dt))
			{
				DateTime bd;
				if (DateTime.TryParse (dt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out bd))
				{
					var n = DateTime.Now;

					var age = n.Year - bd.Year - 1;
					if (new DateTime (bd.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second) > bd)
						age++;

					return age.ToString (CultureInfo.InvariantCulture);
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// Return date difference in days.
		/// </summary>
		/// <param name="dt1">Date 1.</param>
		/// <param name="dt2">Date 2.</param>
		/// <returns>Number of days.</returns>
		public int DateDiffInDays (string dt1, string dt2)
		{
			DateTime d1;
			DateTime d2;
			if (!DateTime.TryParse (dt1, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d1)
				||
				!DateTime.TryParse (dt2, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d2))
				return 0;

			return (int) Math.Abs ((d2 - d1).TotalDays);
		}

		/// <summary>
		/// Return date difference in days.
		/// </summary>
		/// <param name="dt1">Date 1.</param>
		/// <param name="dt2">Date 2.</param>
		/// <returns>Number of days.</returns>
		public int DateDiffInDaysDMY (string dt1, string dt2)
		{
			DateTime d1;
			DateTime d2;
			if (!DateTime.TryParseExact (dt1, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d1)
				||
				!DateTime.TryParseExact (dt2, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d2))
				return 0;

			return (int) Math.Abs ((d2 - d1).TotalDays);
		}

		#endregion

		#region XML methods

		/// <summary>
		/// Group nodes by specified attribute value.
		/// </summary>
		/// <param name="nodes">Source nodes.</param>
		/// <param name="attrName">Attribute name (without '@').</param>
		/// <returns>New XML tree.</returns>
		public XPathNodeIterator GroupByAttribute (XPathNodeIterator nodes, string attrName)
		{
			var res = new XElement ("Root");

			XElement group = null;
			string last_group_value = null;

			while (nodes.MoveNext ())
			{
				var current_group_value = nodes.Current.GetAttribute (attrName, string.Empty);
				if (string.IsNullOrEmpty (last_group_value) || last_group_value != current_group_value)
				{
					last_group_value = current_group_value;

					group = new XElement (attrName, new XAttribute (attrName, current_group_value));
					res.Add (group);
				}

				group.Add (XElement.Parse (nodes.Current.OuterXml));
			}

			return res.CreateNavigator ().SelectChildren (XPathNodeType.Element);
		}

		#endregion

		#region Url methods

		/// <summary>
		/// Return current url.
		/// </summary>
		/// <returns>Current url.</returns>
		public string GetCurrentUrl ()
		{
			return HttpContext.Current.Request.RawUrl;
		}

		/// <summary>
		/// Return current host.
		/// </summary>
		/// <returns>Current host.</returns>
		public string GetCurrentHost ()
		{
			return HttpContext.Current.Request.Url.Host;
		}

		/// <summary>
		/// Return current query string parameter by name.
		/// </summary>
		/// <param name="name">Parameter name.</param>
		/// <returns>Parameter value.</returns>
		public string GetQueryStringParameter (string name)
		{
			return HttpContext.Current.Request.QueryString[name];
		}

		/// <summary>
		/// Sets specified GET parameter for specified url.
		/// </summary>
		/// <param name="name">Parameter name.</param>
		/// <param name="value">Parameter value.</param>
		/// <returns>New url.</returns>
		public string SetUrlParameter (string url, string name, string value)
		{
			return Utils.UrlSetParameter (url, name, value);
		}

		/// <summary>
		/// Return print version url for current page.
		/// </summary>
		/// <returns>Current page print version url.</returns>
		public string GetPrintVersionUrl ()
		{
			return Utils.UrlSetParameter (HttpContext.Current.Request.RawUrl, Utils.PrintVersionUrlParameterName, "1");
		}

		#endregion

		#region Cache methods

		/// <summary>
		/// Gets the value for the custom string cache item.
		/// </summary>
		/// <param name="name">Item name.</param>
		/// <returns>Item value.</returns>
		public string GetVaryByCustomStringValue (string name)
		{
			return CacheManager.GetVaryByCustomStringValue (name) ?? string.Empty;
		}

		#endregion

		#region Site specific methods

		public string GetSupportEmail ()
		{
			return ConfigurationManager.AppSettings["SupportEmail"];
		}

		#endregion
	}
}
