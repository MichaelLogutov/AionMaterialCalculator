using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Website.Code
{
	#region class InvalidOperationExceptionNoReport

	/// <summary>
	/// Copy of <see cref="System.InvalidOperationException"/> class specified that no error should be reported to the site admin.
	/// </summary>
	public class InvalidOperationExceptionNoReport : InvalidOperationException
	{
		public InvalidOperationExceptionNoReport ()
		{
		}

		public InvalidOperationExceptionNoReport (string message)
			: base (message)
		{
		}
	}

	#endregion

	#region class LanguageInfo

	/// <summary>
	/// Information about language.
	/// </summary>
	public class LanguageInfo
	{
		public string Name { get; set; }
		public string Culture { get; set; }
	}

	#endregion

	/// <summary>
	/// <para>Static class utility methods for web.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 5 august 2009</para>
	/// </summary>
	public static class Utils
	{
		#region Accessors

		#region UrlParseRegex

		/// <summary>
		/// <see cref="UrlParseRegex"/> internal field.
		/// </summary>
		private static Regex _UrlParseRegex;

		/// <summary>
		/// (GET) Regular expression for parsing URI into two groups: "url" and "parameters". Default value is null.
		/// </summary>
		[Browsable (false)]
		[DefaultValue (null)]
		public static Regex UrlParseRegex
		{
			get
			{
				return Utils._UrlParseRegex;
			}
			private set
			{
				Utils._UrlParseRegex = value;
			}
		}

		#endregion

		#endregion

		#region Non public fields

		private static TimeSpan xsltCacheDuration = TimeSpan.MinValue;

		#endregion

		#region Constants

		/// <summary>
		/// Default regular expression options.
		/// </summary>
		public const RegexOptions RegexDefaultOptions =
			RegexOptions.Compiled |
			RegexOptions.CultureInvariant |
			RegexOptions.IgnoreCase |
			RegexOptions.IgnorePatternWhitespace |
			RegexOptions.Multiline |
			RegexOptions.Singleline;

		/// <summary>
		/// Russian culture info.
		/// </summary>
		public static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo ("ru-RU");

		#endregion

		#region Init

		static Utils ()
		{
			Utils.UrlParseRegex = new Regex (@"^(?<url>.*?)(\?(?<parameters>.*)?)?$",
				RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

			try
			{
				Utils.xsltCacheDuration = CacheManager.GetConfigurationTimeSpan ("XsltCacheDuration");

				if (Utils.xsltCacheDuration.TotalSeconds < 1)
					Utils.xsltCacheDuration = TimeSpan.MinValue;
			}
			catch
			{
				Utils.xsltCacheDuration = TimeSpan.MinValue;
			}
		}

		#endregion

		#region XDocument methods

		#region XGetText

		/// <summary>
		/// Returns XML node's text by specified path or null if no such element present.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		/// <returns>Text by specified path or null if no such element present.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static string XGetText (XElement xml, string path)
		{
			if (xml != null)
			{
				var node = xml.XPathSelectElement (path);
				if (node != null)
					return node.Value;
				else
					return null;
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns XML node's attribute text by specified path or null if no such element present.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <returns>Text by specified path or null if no such element present.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static string XGetText (XElement xml, string path, string attributeName)
		{
			if (xml != null)
			{
				var node = xml.XPathSelectElement (path);
				if (node != null)
				{
					var attribute = node.Attribute (attributeName);
					return attribute == null ? string.Empty : attribute.Value;
				}
				else
					return null;
			}

			return string.Empty;
		}

		#endregion

		#region XGetValue<T>

		/// <summary>
		/// Returns XML node's text (converted to specified type) by specified path or default type value if no such element present.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		/// <returns>Text by specified path or default type value if no such element present.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T XGetValue<T> (XElement xml, string path)
		{
			if (xml != null)
			{
				var node = xml.XPathSelectElement (path);
				if (node != null)
				{
					T value;
					var type = typeof (T);
					if (type.FullName.StartsWith ("System.Nullable", StringComparison.Ordinal))
					{
						var convert = new NullableConverter (type);
						value = (T) convert.ConvertFromString (node.Value);
					}
					else
					{
						value = (T) Convert.ChangeType (node.Value, type, CultureInfo.InvariantCulture);
					}
					return value;

				}
			}

			return default (T);
		}

		/// <summary>
		/// Returns XML node's text (converted to specified type) by specified path or <paramref name="defaultValue"/> if no such element present.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Text by specified path or default type value if no such element present.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T XGetValueOrDefault<T> (XElement xml, string path, T defaultValue)
		{
			if (xml != null)
			{
				var node = xml.XPathSelectElement (path);
				if (node != null)
					return (T) Convert.ChangeType (node.Value, typeof (T), CultureInfo.InvariantCulture);
			}

			return defaultValue;
		}

		/// <summary>
		/// Returns XML node's attribute text (converted to specified type) by specified path or default type value if no such element present.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <returns>Text by specified path or default type value if no such element present.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T XGetValue<T> (XElement xml, string path, string attributeName)
		{
			if (xml != null)
			{
				var node = xml.XPathSelectElement (path);
				if (node != null)
				{
					var attribute = node.Attribute (attributeName);
					if (attribute != null)
					{
						T value;
						var type = typeof (T);
						if (type.FullName.StartsWith ("System.Nullable", StringComparison.Ordinal))
						{
							var convert = new NullableConverter (type);
							value = (T) convert.ConvertFromString (attribute.Value);
						}
						else
						{
							value = (T) Convert.ChangeType (attribute.Value, type, CultureInfo.InvariantCulture);
						}
						return value;
					}
				}
			}

			return default (T);
		}

		/// <summary>
		/// Returns XML node's attribute text (converted to specified type) by specified path or <paramref name="defaultValue"/> if no such element present.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <returns>Text by specified path or default type value if no such element present.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T XGetValueOrDefault<T> (XElement xml, string path, string attributeName, T defaultValue)
		{
			if (xml != null)
			{
				var node = xml.XPathSelectElement (path);
				if (node != null)
				{
					var attribute = node.Attribute (attributeName);
					if (attribute != null)
						return (T) Convert.ChangeType (attribute.Value, typeof (T), CultureInfo.InvariantCulture);
				}
			}

			return defaultValue;
		}

		#endregion

		#region XRemove

		/// <summary>
		/// Removes all child nodes from element by Xml path <paramref name="path"/>.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void XRemoveAllChildNodes (XElement xml, string path)
		{
			if (xml == null)
				throw new InvalidOperationException ("Parameter 'xml' can not be null.");

			var node = xml.XPathSelectElement (path);
			if (node != null)
				node.RemoveNodes ();
		}

		/// <summary>
		/// Removes nodes from xml by Xml path <paramref name="path"/>.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void XRemoveNode (XElement xml, string path)
		{
			if (xml == null)
				throw new InvalidOperationException ("Parameter 'xml' can not be null.");

			var node = xml.XPathSelectElement (path);
			if (node != null)
				node.Remove ();
		}

		/// <summary>
		/// Removes all attributes from element by Xml path <paramref name="path"/>.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void XRemoveAttributes (XElement xml, string path)
		{
			if (xml == null)
				throw new InvalidOperationException ("Parameter 'xml' can not be null.");

			var node = xml.XPathSelectElement (path);
			if (node != null)
				node.RemoveAttributes ();
		}

		/// <summary>
		/// Removes attribute from xml node by Xml path <paramref name="path"/>.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path.</param>
		/// <param name="attributeName">Name of the attribute to remove.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void XRemoveAttribute (XElement xml, string path, string attributeName)
		{
			if (xml == null)
				throw new InvalidOperationException ("Parameter 'xml' can not be null.");

			var node = xml.XPathSelectElement (path);
			if (node != null)
				node.SetAttributeValue (attributeName, null);
		}

		#endregion

		#region XAddNode

		/// <summary>
		/// Adds child node by Xml path <paramref name="path"/>.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		public static XElement XAddNode (XElement xml, string path)
		{
			if (xml == null)
				throw new InvalidOperationException ("Parameter 'xml' can not be null.");

			if (string.IsNullOrEmpty (path))
				throw new InvalidOperationException ("Parameter 'path' can not be null or empty.");

			var parent_node = xml;
			var elements = path.Split (new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			for (var k = 0; k < elements.Length; k++)
			{
				var element_name = elements[k];
				var node = parent_node.Element (element_name);

				if (node == null)
				{
					node = new XElement (element_name);
					parent_node.Add (node);
				}

				parent_node = node;
			}

			return parent_node;
		}

		#endregion

		#region XGetNode

		/// <summary>
		/// Returns node by Xml path <paramref name="path"/>. If node doesn't exists then it will be created.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		public static XElement XGetNode (XElement xml, string path)
		{
			return Utils.XGetNode (xml, path, true);
		}

		/// <summary>
		/// Returns node by Xml path <paramref name="path"/>. If node node exists then it will be created (if <paramref name="createIfNotExist"/> == true).
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		/// <param name="createIfNotExist">If true, then node will be create if not exist.</param>
		public static XElement XGetNode (XElement xml, string path, bool createIfNotExist)
		{
			if (xml == null)
				throw new InvalidOperationException ("Parameter 'xml' can not be null.");

			if (string.IsNullOrEmpty (path))
				throw new InvalidOperationException ("Parameter 'path' can not be null or empty.");

			var node = xml.XPathSelectElement (path);
			if (node == null && createIfNotExist)
				node = Utils.XAddNode (xml, path);

			return node;
		}

		#endregion

		#region XSetText

		/// <summary>
		/// Sets XML node's text by specified path or null if no such element present. The node will be created if not exist. The node will be deleted if <paramref name="value"/> is null or empty.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		///	<param name="value">New value for inner text of the node.</param>
		public static void XSetText (XElement xml, string path, string value)
		{
			Utils.XSetText (xml, path, value, true, true);
		}

		/// <summary>
		/// Sets XML node's text by specified path or null if no such element present. The node will be created if not exist if <paramref name="createIfNotExist"/> == true. If <paramref name="deleteIfEmpty"/> == true and <paramref name="value"/> is null or empty then node <paramref name="path"/> will be deleted (if exist).
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		///	<param name="value">New value for inner text of the node.</param>
		/// <param name="createIfNotExist">If true, then node will be create if not exist.</param>
		/// <param name="deleteIfEmpty">If true, then deletes <paramref name="path"/> node if <paramref name="value"/> is null or empty.</param>
		public static void XSetText (XElement xml, string path, string value, bool createIfNotExist, bool deleteIfEmpty)
		{
			if (deleteIfEmpty && string.IsNullOrEmpty (value))
				Utils.XRemoveNode (xml, path);
			else
			{
				var node = Utils.XGetNode (xml, path, createIfNotExist);
				if (node != null)
				{
					node.Value = value;

					if (node != null && string.IsNullOrEmpty (value))
						Utils.XRemoveNode (xml, path);
				}
			}
		}

		/// <summary>
		/// Sets XML node's attribute text by specified path or null if no such element present. The node and attribute will be created if not exist. The attribute will be deleted if <paramref name="value"/> is null or empty.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		/// <param name="attributeName">Attribute name to set.</param>
		///	<param name="value">New value for inner text of the node.</param>
		public static void XSetText (XElement xml, string path, string attributeName, string value)
		{
			Utils.XSetText (xml, path, attributeName, value, true, true);
		}

		/// <summary>
		/// Sets XML node's attribute text by specified path or null if no such element present. The node and attribute will be created if not exist if <paramref name="createIfNotExist"/> == true. If <paramref name="deleteIfEmpty"/> == true and <paramref name="value"/> is null or empty then attribute will be deleted (if exist).
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		/// <param name="attributeName">Attribute name to set.</param>
		///	<param name="value">New value for inner text of the node.</param>
		/// <param name="createIfNotExist">If true, then node will be create if not exist.</param>
		/// <param name="deleteIfEmpty">If true, then deletes <paramref name="path"/> node if <paramref name="value"/> is null or empty.</param>
		public static void XSetText (XElement xml, string path, string attributeName, string value, bool createIfNotExist, bool deleteIfEmpty)
		{
			if (deleteIfEmpty && string.IsNullOrEmpty (value))
				Utils.XRemoveAttribute (xml, path, attributeName);
			else
			{
				var node = Utils.XGetNode (xml, path, createIfNotExist);
				if (node != null)
					node.SetAttributeValue (attributeName, value);
			}
		}

		#endregion

		#region XSetValue

		/// <summary>
		/// Sets XML node's text by specified path or null if no such element present. The node will be created if not exist. The node will be deleted if <paramref name="value"/> is null.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		///	<param name="value">New value for inner text of the node.</param>
		[SuppressMessage ("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.X.XElement")]
		public static void XSetValue (XElement xml, string path, object value)
		{
			Utils.XSetValue (xml, path, value, true, true);
		}

		/// <summary>
		/// Sets XML node's text by specified path or null if no such element present. The node will be created if not exist if <paramref name="createIfNotExist"/> == true. If <paramref name="deleteIfEmpty"/> == true and <paramref name="value"/> is null then node <paramref name="path"/> will be deleted (if exist).
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		///	<param name="value">New value for inner text of the node.</param>
		/// <param name="createIfNotExist">If true, then node will be create if not exist.</param>
		/// <param name="deleteIfEmpty">If true, then deletes <paramref name="path"/> node if <paramref name="value"/> is null or empty.</param>
		[SuppressMessage ("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.X.XElement")]
		public static void XSetValue (XElement xml, string path, object value, bool createIfNotExist, bool deleteIfEmpty)
		{
			if (deleteIfEmpty && value == null)
				Utils.XRemoveNode (xml, path);
			else
			{
				var node = Utils.XGetNode (xml, path, createIfNotExist);
				if (node != null)
				{
					node.Value = Convert.ToString (value, CultureInfo.InvariantCulture);

					if (node != null && string.IsNullOrEmpty (node.Value))
						Utils.XRemoveNode (xml, path);
				}
			}
		}

		/// <summary>
		/// Sets XML node's attribute text by specified path or null if no such element present. The attribute and node will be created if not exist. The attribute will be deleted if <paramref name="value"/> is null.
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		/// <param name="attributeName">Attribute name to set.</param>
		///	<param name="value">New value for inner text of the node.</param>
		[SuppressMessage ("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.X.XElement")]
		public static void XSetValue (XElement xml, string path, string attributeName, object value)
		{
			Utils.XSetValue (xml, path, attributeName, value, true, true);
		}

		/// <summary>
		/// Sets XML node's attribute text by specified path or null if no such element present. The attribute and node will be created if not exist if <paramref name="createIfNotExist"/> == true. If <paramref name="deleteIfEmpty"/> == true and <paramref name="value"/> is null then attribute will be deleted (if exist).
		/// </summary>
		/// <param name="xml">Source xml document.</param>
		/// <param name="path">Xml path path. <b>Warning!</b> Xml path should contain only simple Xml path syntax for this method to correctly created node if it wasn't exist.</param>
		/// <param name="attributeName">Attribute name to set.</param>
		///	<param name="value">New value for inner text of the node.</param>
		/// <param name="createIfNotExist">If true, then node will be create if not exist.</param>
		/// <param name="deleteIfEmpty">If true, then deletes <paramref name="path"/> node if <paramref name="value"/> is null or empty.</param>
		[SuppressMessage ("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.X.XElement")]
		public static void XSetValue (XElement xml, string path, string attributeName, object value, bool createIfNotExist, bool deleteIfEmpty)
		{
			if (deleteIfEmpty && value == null)
				Utils.XRemoveAttribute (xml, path, attributeName);
			else
			{
				var node = Utils.XGetNode (xml, path, createIfNotExist);
				if (node != null)
					node.SetAttributeValue (attributeName, Convert.ToString (value, CultureInfo.InvariantCulture));
			}
		}

		#endregion

		#region XGetAttributeText

		/// <summary>
		/// Returns XML node's attribute text by name or null if no such element present.
		/// </summary>
		/// <param name="node">Source xml node.</param>
		/// <param name="attributeName">Xml attribute name.</param>
		/// <returns>Attribute text by specified name or null if no such element present.</returns>
		public static string XGetAttributeText (XElement node, string attributeName)
		{
			if (node != null)
			{
				var attr = node.Attribute (attributeName);
				if (attr != null)
					return attr.Value;
			}

			return null;
		}

		#endregion

		#region XGetAttributeValue<T>

		/// <summary>
		/// Returns XML node's attribute value by name or null if no such element present.
		/// </summary>
		/// <typeparam name="T">Type of the return value.</typeparam>
		/// <param name="node">Source xml node.</param>
		/// <param name="attributeName">Xml attribute name.</param>
		/// <returns>Attribute value by specified name or null if no such element present.</returns>
		public static T XGetAttributeValue<T> (XElement node, string attributeName)
		{
			return Utils.XGetAttributeValue (node, attributeName, default (T));
		}

		/// <summary>
		/// Returns XML node's attribute value by name or null if no such element present.
		/// </summary>
		/// <typeparam name="T">Type of the return value.</typeparam>
		/// <param name="node">Source xml node.</param>
		/// <param name="attributeName">Xml attribute name.</param>
		/// <param name="defaultValue">Default value to be returned if node is null or attribute not found.</param>
		/// <returns>Attribute value by specified name or null if no such element present.</returns>
		public static T XGetAttributeValue<T> (XElement node, string attributeName, T defaultValue)
		{
			if (node != null)
			{
				var attr = node.Attribute (attributeName);
				if (attr != null)
					return (T) Convert.ChangeType (attr.Value, typeof (T), CultureInfo.InvariantCulture);
			}

			return defaultValue;
		}

		#endregion

		#region XSetAttribute

		/// <summary>
		/// Sets XML node's attribute text. The attribute will be created if not exist.
		/// </summary>
		/// <param name="node">Source xml node.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		///	<param name="value">New value.</param>
		[SuppressMessage ("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
		public static void XSetAttribute (XElement node, string attributeName, object value)
		{
			var a = node.Attribute (attributeName);
			if (a == null)
				node.Add (new XAttribute (attributeName, value));
			else
				a.Value = (value == null ? null : value.ToString ());
		}

		#endregion

		#endregion

		#region Url methods

		public const string PrintVersionUrlParameterName = "printVersion";

		/// <summary>
		/// Returns true if current page hase a "print version" parameter in url.
		/// </summary>
		/// <returns>True if current page hase a "print version" parameter in url.</returns>
		public static bool IsPrintVersion ()
		{
			return HttpContext.Current.Request.QueryString[Utils.PrintVersionUrlParameterName] == "1";
		}

		/// <summary>
		/// Set (add, replace or remove) query parameter in <paramref name="url"/>.
		/// </summary>
		/// <param name="url">Source Url.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="parameterValue">Parameter value. If null then parameter will be removed from query.</param>
		/// <returns>New Url.</returns>
		public static string UrlSetParameter (string url, string parameterName, string parameterValue)
		{
			var m = Utils.UrlParseRegex.Match (url);
			if (!m.Success)
				return url;

			var parameters = m.Groups["parameters"].Value;
			if (!string.IsNullOrEmpty (parameters))
			{
				var parameters_collection = HttpUtility.ParseQueryString (parameters);

				if (parameterValue == null)
					parameters_collection.Remove (parameterName);
				else
				{
					if (!string.IsNullOrEmpty (parameters_collection[parameterName]))
						parameters_collection[parameterName] = parameterValue;
					else
						parameters_collection.Add (parameterName, parameterValue);
				}

				var sb = new StringBuilder ("?");
				for (var k = 0; k < parameters_collection.Count; k++)
				{
					if (sb.Length > 1)
						sb.Append ('&');

					sb.Append (parameters_collection.GetKey (k));
					sb.Append ('=');
					sb.Append (parameters_collection.Get (k));
				}

				if (sb.Length == 1)
					parameters = string.Empty;
				else
					parameters = sb.ToString ();
			}
			else if (parameterValue != null)
				parameters = string.Concat ("?", parameterName, "=", HttpUtility.UrlEncode (parameterValue));

			return string.Concat (m.Groups["url"], parameters);
		}

		/// <summary>
		/// Remove get parameters from url.
		/// </summary>
		/// <param name="url">Source url.</param>
		/// <returns>Modified url.</returns>
		public static string RemoveUrlParameters (string url)
		{
			var index = url.IndexOf ('?');
			if (index >= 0)
				return url.Remove (index);

			return url;
		}

		#endregion

		#region Forms fields methods

		/// <summary>
		/// Prepeare <paramref name="value"/> for using in DB oprations as column value.
		/// </summary>
		/// <param name="value">Form field value.</param>
		/// <returns>DB field value</returns>
		public static string FormFieldPrepareForDBColumnValue (string value)
		{
			return Utils.FormFieldPrepareForDBColumnValue (value, true, false);
		}

		/// <summary>
		/// Prepeare <paramref name="value"/> for using in DB oprations as column value.
		/// </summary>
		/// <param name="value">Form field value.</param>
		/// <param name="removeTags">If true then will remove html tags from value.</param>
		/// <param name="insertBreaks">If true then \n will be replaced by &lt;br /&gt;.</param>
		/// <returns>DB field value</returns>
		public static string FormFieldPrepareForDBColumnValue (string value, bool removeTags, bool insertBreaks)
		{
			if (string.IsNullOrEmpty (value))
				return null;

			value = value.Trim ();
			if (string.IsNullOrEmpty (value))
				return null;

			if (removeTags)
				value = Utils.RemoveHtmlTags (value, insertBreaks).Trim ();
			else if (insertBreaks)
				value = Utils.ReplaceNewLinesToBreaks (value).Trim ();

			return value;
		}

		/// <summary>
		/// Prepeare <paramref name="url"/> for using in DB oprations as column value.
		/// </summary>
		/// <param name="url">Form field url value.</param>
		/// <returns>DB field value</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.StartsWith(System.String,System.StringComparison)"), SuppressMessage ("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings"), SuppressMessage ("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		public static string FormFieldPrepareUrlForDBColumnValue (string url)
		{
			url = Utils.FormFieldPrepareForDBColumnValue (url);
			if (url != null && !url.StartsWith ("http://", StringComparison.InvariantCultureIgnoreCase) && !url.StartsWith ("https://", StringComparison.InvariantCultureIgnoreCase))
				url = "http://" + url;

			return url;
		}

		/// <summary>
		/// Prepeare <paramref name="value"/> (<see cref="DateTime"/> string) for using in DB oprations as column value.
		/// </summary>
		/// <param name="value">Form field value.</param>
		/// <param name="format">Format to be used when parsing <paramref name="value"/> into <see cref="DateTime"/> object.</param>
		/// <returns>DB field value</returns>
		public static object FormFieldPrepareDateTimeForDBColumnValue (string value, string format)
		{
			value = Utils.FormFieldPrepareForDBColumnValue (value);
			if (value != null)
				return DateTime.ParseExact (value, format, CultureInfo.InvariantCulture);

			return null;
		}

		#endregion

		#region Code methods

		/// <summary>
		/// (extension methos) Compares two strings with no case sensivity and invariant culture.
		/// </summary>
		/// <param name="a">String a.</param>
		/// <param name="b">String b.</param>
		/// <returns>True if strings are equal or false otherwise.</returns>
		public static bool CompareIgnoreCase (this string a, string b)
		{
			return string.Compare (a, b, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		/// <summary>
		/// Gets the version of the current executing assembly.
		/// </summary>
		/// <returns>Version number.</returns>
		public static string GetCurrentVersion ()
		{
			return Utils.GetCurrentVersion (false);
		}

		/// <summary>
		/// Gets the version of the current executing assembly.
		/// </summary>
		/// <param name="asNumber">True if result should be represented as number (no dots).</param>
		/// <returns>Version number.</returns>
		public static string GetCurrentVersion (bool asNumber)
		{
			var res = Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
			if (asNumber)
				res = res.Replace (".", "");

			return res;
		}

		/// <summary>
		/// Convert string <paramref name="value"/>.
		/// </summary>
		/// <param name="value">String value.</param>
		/// <param name="type">Desired value type.</param>
		/// <param name="defaultValue">Default value to be returned if <paramref name="value"/> is null or empty.</param>
		/// <returns>Converted value.</returns>
		public static object ConvertString (string value, Type type, object defaultValue)
		{
			if (value != null)
				value = value.Trim ();
			else
				return defaultValue;

			if (string.IsNullOrEmpty (value))
				return defaultValue;

			return Convert.ChangeType (value, type, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Convert string <paramref name="value"/>.
		/// </summary>
		/// <typeparam name="T">Desired value type.</typeparam>
		/// <param name="value">String value.</param>
		/// <param name="defaultValue">Default value to be returned if <paramref name="value"/> is null or empty.</param>
		/// <returns>Converted value.</returns>
		public static T ConvertString<T> (string value, T defaultValue)
		{
			if (value != null)
				value = value.Trim ();
			else
				return defaultValue;

			if (string.IsNullOrEmpty (value))
				return defaultValue;

			return (T) Convert.ChangeType (value, typeof (T), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Restricts a <paramref name="value"/> in specific range.
		/// </summary>
		/// <typeparam name="T">Type of the values.</typeparam>
		/// <param name="value">Course.</param>
		/// <param name="min">Minimal range value.</param>
		/// <param name="max">Maximal range value.</param>
		/// <returns>Clamped value.</returns>
		public static T Clamp<T> (T value, T min, T max) where T : IComparable<T>
		{
			var result = value;

			if (value.CompareTo (max) > 0)
				result = max;
			else if (value.CompareTo (min) < 0)
				result = min;

			return result;
		}

		/// <summary>
		/// Converts list of values to it's string representation with specified separator.
		/// </summary>
		/// <typeparam name="T">Type of the values in list.</typeparam>
		/// <param name="values">Course list.</param>
		/// <param name="separator">Separator to be used.</param>
		/// <returns>String representation.</returns>
		public static string ConvertToString<T> (IEnumerable<T> values, string separator)
		{
			return string.Join (separator, (from x in values
											select Convert.ToString (x, CultureInfo.InvariantCulture)).ToArray ());
		}

		/// <summary>
		/// Converts list of values to it's string representation with comma separator.
		/// </summary>
		/// <typeparam name="T">Type of the values in list.</typeparam>
		/// <param name="values">Course list.</param>
		/// <returns>String representation.</returns>
		public static string ConvertToString<T> (IEnumerable<T> values)
		{
			return Utils.ConvertToString (values, ",");
		}

		/// <summary>
		/// Extends <see cref="IDictionary.TryGetValue"/> with default return value.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static TValue TryGetValue<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
		{
			TValue res;

			if (!dictionary.TryGetValue (key, out res))
				return defaultValue;

			return res;
		}

		/// <summary>
		/// Reports specified exception to the developer email.
		/// </summary>
		/// <param name="ex">Exception thrown.</param>
		public static void ReportError (Exception ex)
		{
			if (ex is InvalidOperationExceptionNoReport)
				return;

			try
			{
				var msg = new MailMessage ();
				var context = HttpContext.Current;

				msg.To.Add (new MailAddress (ConfigurationManager.AppSettings["ErrorEmail"]));
				msg.Subject = string.Format (CultureInfo.InvariantCulture, "[ERROR] site: {0}, url: {1}, message: {2}",
					context.Request.Url.Host,
					context.Request.Url.OriginalString,
					ex.Message);

				msg.IsBodyHtml = true;
				msg.Body =
					"<ul>" +
						"<li>Url.OriginalString <a href='" + context.Request.Url.OriginalString + "'>" + context.Request.Url.OriginalString + "</a></li>" +
						"<li>RawUrl: <a href='" + context.Request.RawUrl + "'>" + context.Request.RawUrl + "</a></li>" +
						"<li>User: " + (context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Guest") + "</li>" +
						"<li>User Host Address: " + context.Request.UserHostAddress + "</li>" +
						"<li>User Agent: " + context.Request.UserAgent + "</li>" +
						"<li>Browser: " + context.Request.Browser.Browser + " " + context.Request.Browser.Version + "</li>" +
						"<li>Referrer: " + context.Request.UrlReferrer + "</li>" +
					"</ul>" +
					"<hr />" +
					"<pre>" + ex + "</pre>";

				var smtp = new SmtpClient ();
				smtp.Send (msg);
			}
			catch
			{
			}
		}

		#endregion

		#region XSLT methods

		#region class XmlWebsiteUrlResolver

		private class XmlWebsiteUrlResolver : XmlUrlResolver
		{
			public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
			{
				return base.GetEntity (absoluteUri, role, ofObjectToReturn);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
			public override Uri ResolveUri (Uri baseUri, string relativeUri)
			{
				Uri resUri = null;
				try
				{
					resUri = new Uri (relativeUri);
				}
				catch
				{
					resUri = new Uri (HttpContext.Current.Server.MapPath (relativeUri));
				}

				return resUri;
			}

		}

		#endregion

		#region Results as string

		/// <summary>
		/// Perform XSLT tranformation on specified <paramref name="xml"/>.
		/// </summary>
		/// <param name="xml">Source XML.</param>
		/// <param name="arguments">XSLT arguments.</param>
		/// <param name="xsltPath">Path to XSLT.</param>
		/// <returns>Transformed document.</returns>
		public static string XsltTransform (string xml, XsltArgumentList arguments, string xsltPath)
		{
			var reader_settings = new XmlReaderSettings ();
			reader_settings.ProhibitDtd = false;

			var cache_key = "XSLT." + xsltPath;
			var xslt = CacheManager.GetItem<XslCompiledTransform> (cache_key);

			if (xslt == null)
			{
				xslt = new XslCompiledTransform ();
				var xslt_settings = new XsltSettings (true, true);

				using (var input_stream = XmlReader.Create (HttpContext.Current.Server.MapPath (xsltPath), reader_settings))
				{
					xslt.Load (input_stream, xslt_settings, new XmlWebsiteUrlResolver ());
				}

				if (Utils.xsltCacheDuration != TimeSpan.MinValue)
					CacheManager.AddItem (cache_key, xslt, null, Cache.NoAbsoluteExpiration, Utils.xsltCacheDuration, CacheItemPriority.Default, null);
			}

			using (var output_stream = new StringWriter (CultureInfo.InvariantCulture))
			{
				using (var xml_stream = new StringReader (xml))
				{
					using (var input_stream = XmlReader.Create (xml_stream, reader_settings))
					{
						xslt.Transform (input_stream, arguments, output_stream);
					}
				}

				return output_stream.ToString ();
			}
		}

		/// <summary>
		/// Perform XSLT tranformation on specified <paramref name="xml"/>.
		/// </summary>
		/// <param name="xmlPath">Path to source XML.</param>
		/// <param name="arguments">XSLT arguments.</param>
		/// <param name="xsltPath">Path to XSLT.</param>
		/// <returns>Transformed document.</returns>
		public static string XsltTransformByPath (string xmlPath, XsltArgumentList arguments, string xsltPath)
		{
			var reader_settings = new XmlReaderSettings ();
			reader_settings.ProhibitDtd = false;

			var cache_key = "XSLT." + xsltPath;
			var xslt = CacheManager.GetItem<XslCompiledTransform> (cache_key);

			if (xslt == null)
			{
				xslt = new XslCompiledTransform ();
				var xslt_settings = new XsltSettings (true, true);

				using (var input_stream = XmlReader.Create (HttpContext.Current.Server.MapPath (xsltPath), reader_settings))
				{
					xslt.Load (input_stream, xslt_settings, new XmlWebsiteUrlResolver ());
				}

				if (Utils.xsltCacheDuration != TimeSpan.MinValue)
					CacheManager.AddItem (cache_key, xslt, null, Cache.NoAbsoluteExpiration, Utils.xsltCacheDuration, CacheItemPriority.Default, null);
			}

			using (var output_stream = new StringWriter (CultureInfo.InvariantCulture))
			{
				using (var input_stream = XmlReader.Create (HttpContext.Current.Server.MapPath (xmlPath), reader_settings))
				{
					xslt.Transform (input_stream, arguments, output_stream);
				}

				return output_stream.ToString ();
			}
		}

		/// <summary>
		/// Perform XSLT tranformation on specified <paramref name="xml"/>.
		/// </summary>
		/// <param name="xml">Source XML.</param>
		/// <param name="arguments">XSLT arguments.</param>
		/// <param name="xsltStream">XSLT stream.</param>
		/// <returns>Transformed document.</returns>
		public static string XsltTransform (string xml, XsltArgumentList arguments, Stream xsltStream)
		{
			var reader_settings = new XmlReaderSettings ();
			reader_settings.ProhibitDtd = false;

			var xslt = new XslCompiledTransform ();
			{
				var xslt_settings = new XsltSettings (true, true);

				using (var input_stream = XmlReader.Create (xsltStream, reader_settings))
				{
					xslt.Load (input_stream, xslt_settings, new XmlWebsiteUrlResolver ());
				}
			}

			using (var output_stream = new StringWriter (CultureInfo.InvariantCulture))
			{
				using (var xml_stream = new StringReader (xml))
				{
					using (var input_stream = XmlReader.Create (xml_stream, reader_settings))
					{
						xslt.Transform (input_stream, arguments, output_stream);
					}
				}

				return output_stream.ToString ();
			}
		}


		/// <summary>
		/// Perform XSLT tranformation on specified
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="arguments"></param>
		/// <param name="xsltPath"></param>
		/// <returns></returns>
		public static string XsltTransform (IXPathNavigable xml, XsltArgumentList arguments, string xsltPath)
		{
			var reader_settings = new XmlReaderSettings ();
			reader_settings.ProhibitDtd = false;

			var cache_key = "XSLT." + xsltPath;
			var xslt = CacheManager.GetItem<XslCompiledTransform> (cache_key);

			if (xslt == null)
			{
				xslt = new XslCompiledTransform ();
				var xslt_settings = new XsltSettings (true, true);

				using (var input_stream = XmlReader.Create (HttpContext.Current.Server.MapPath (xsltPath), reader_settings))
				{
					xslt.Load (input_stream, xslt_settings, new XmlWebsiteUrlResolver ());
				}

				if (Utils.xsltCacheDuration != TimeSpan.MinValue)
					CacheManager.AddItem (cache_key, xslt, null, Cache.NoAbsoluteExpiration, Utils.xsltCacheDuration, CacheItemPriority.Default, null);
			}

			using (var output_stream = new StringWriter (CultureInfo.InvariantCulture))
			{
				xslt.Transform (xml, arguments, output_stream);
				return output_stream.ToString ();
			}
		}

		#endregion

		#region Results to stream

		/// <summary>
		/// Perform XSLT tranformation on specified <paramref name="xml"/>.
		/// </summary>
		/// <param name="xml">Source XML.</param>
		/// <param name="arguments">XSLT arguments.</param>
		/// <param name="xsltPath">Path to XSLT.</param>
		/// <param name="result">Stream where the result will be written.</param>
		public static void XsltTransform (string xml, XsltArgumentList arguments, string xsltPath, Stream result)
		{
			var reader_settings = new XmlReaderSettings ();
			reader_settings.ProhibitDtd = false;

			var cache_key = "XSLT." + xsltPath;
			var xslt = CacheManager.GetItem<XslCompiledTransform> (cache_key);

			if (xslt == null)
			{
				xslt = new XslCompiledTransform ();
				var xslt_settings = new XsltSettings (true, true);

				using (var input_stream = XmlReader.Create (HttpContext.Current.Server.MapPath (xsltPath), reader_settings))
				{
					xslt.Load (input_stream, xslt_settings, new XmlWebsiteUrlResolver ());
				}

				if (Utils.xsltCacheDuration != TimeSpan.MinValue)
					CacheManager.AddItem (cache_key, xslt, null, Cache.NoAbsoluteExpiration, Utils.xsltCacheDuration, CacheItemPriority.Default, null);
			}

			using (var xml_stream = new StringReader (xml))
			{
				using (var input_stream = XmlReader.Create (xml_stream, reader_settings))
				{
					xslt.Transform (input_stream, arguments, result);
				}
			}
		}

		/// <summary>
		/// Perform XSLT tranformation on specified <paramref name="xml"/>.
		/// </summary>
		/// <param name="xmlPath">Path to source XML.</param>
		/// <param name="arguments">XSLT arguments.</param>
		/// <param name="xsltPath">Path to XSLT.</param>
		/// <param name="result">Stream where the result will be written.</param>
		public static void XsltTransformByPath (string xmlPath, XsltArgumentList arguments, string xsltPath, Stream result)
		{
			var reader_settings = new XmlReaderSettings ();
			reader_settings.ProhibitDtd = false;

			var cache_key = "XSLT." + xsltPath;
			var xslt = CacheManager.GetItem<XslCompiledTransform> (cache_key);

			if (xslt == null)
			{
				xslt = new XslCompiledTransform ();
				var xslt_settings = new XsltSettings (true, true);

				using (var input_stream = XmlReader.Create (HttpContext.Current.Server.MapPath (xsltPath), reader_settings))
				{
					xslt.Load (input_stream, xslt_settings, new XmlWebsiteUrlResolver ());
				}

				if (Utils.xsltCacheDuration != TimeSpan.MinValue)
					CacheManager.AddItem (cache_key, xslt, null, Cache.NoAbsoluteExpiration, Utils.xsltCacheDuration, CacheItemPriority.Default, null);
			}

			using (var input_stream = XmlReader.Create (HttpContext.Current.Server.MapPath (xmlPath), reader_settings))
			{
				xslt.Transform (input_stream, arguments, result);
			}
		}

		/// <summary>
		/// Perform XSLT tranformation on specified <paramref name="xml"/>.
		/// </summary>
		/// <param name="xml">Source XML.</param>
		/// <param name="arguments">XSLT arguments.</param>
		/// <param name="xsltStream">XSLT stream.</param>
		/// <param name="result">Stream where the result will be written.</param>
		public static void XsltTransform (string xml, XsltArgumentList arguments, Stream xsltStream, Stream result)
		{
			var reader_settings = new XmlReaderSettings ();
			reader_settings.ProhibitDtd = false;

			var xslt = new XslCompiledTransform ();
			{
				var xslt_settings = new XsltSettings (true, true);

				using (var input_stream = XmlReader.Create (xsltStream, reader_settings))
				{
					xslt.Load (input_stream, xslt_settings, new XmlWebsiteUrlResolver ());
				}
			}

			using (var xml_stream = new StringReader (xml))
			{
				using (var input_stream = XmlReader.Create (xml_stream, reader_settings))
				{
					xslt.Transform (input_stream, arguments, result);
				}
			}
		}


		/// <summary>
		/// Perform XSLT tranformation on specified <paramref name="xml"/>.
		/// </summary>
		/// <param name="xml">Source XML.</param>
		/// <param name="arguments">XSLT arguments.</param>
		/// <param name="xsltStream">Path to XSLT.</param>
		/// <param name="result">Stream where the result will be written.</param>
		public static void XsltTransform (IXPathNavigable xml, XsltArgumentList arguments, string xsltPath, Stream result)
		{
			var reader_settings = new XmlReaderSettings ();
			reader_settings.ProhibitDtd = false;

			var cache_key = "XSLT." + xsltPath;
			var xslt = CacheManager.GetItem<XslCompiledTransform> (cache_key);

			if (xslt == null)
			{
				xslt = new XslCompiledTransform ();
				var xslt_settings = new XsltSettings (true, true);

				using (var input_stream = XmlReader.Create (HttpContext.Current.Server.MapPath (xsltPath), reader_settings))
				{
					xslt.Load (input_stream, xslt_settings, new XmlWebsiteUrlResolver ());
				}

				if (Utils.xsltCacheDuration != TimeSpan.MinValue)
					CacheManager.AddItem (cache_key, xslt, null, Cache.NoAbsoluteExpiration, Utils.xsltCacheDuration, CacheItemPriority.Default, null);
			}

			using (var output_stream = new StringWriter (CultureInfo.InvariantCulture))
			{
				xslt.Transform (xml, arguments, result);
			}
		}

		#endregion

		#endregion

		#region Mail

		/// <summary>
		/// Sends mail using xslt for formatting. Returns true if mail send successfully, otherwise - false.
		/// </summary>
		/// <param name="xml">Mail data.</param>
		/// <param name="xsltPath">Path to mail formatting xslt (web path).</param>
		/// <param name="to">Recipient (can be many - in that case separate them with semicolon).</param>
		/// <param name="additionalXsltArguments">List of name-value pairs of additional arguments that will be passed to xslt.</param>
		/// <returns>True if mail send successfully, otherwise - false.</returns>
		static public bool SendMailXslt (IXPathNavigable xml, string xsltPath, string to, params object[] additionalXsltArguments)
		{
			var msg = new MailMessage ();

#if DEBUG
			if (!string.IsNullOrEmpty (ConfigurationManager.AppSettings["Debug.Email.RedirectAll"]))
				to = ConfigurationManager.AppSettings["Debug.Email.RedirectAll"];
#endif

			var to_emails = to.Split (new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (to_emails.Length < 1)
				return false;

			foreach (var email in to_emails)
				msg.To.Add (new MailAddress (email));

			var args = new XsltArgumentList ();
			var script_helpers = new XsltScriptHelpers ();
			args.AddExtensionObject ("urn:script", script_helpers);

			msg.Body = Utils.XsltTransform (xml, args, xsltPath);
			msg.Subject = script_helpers.GetData ("Subject");

			var reply_to = script_helpers.GetData ("ReplyTo");
			if (!string.IsNullOrEmpty (reply_to))
			{
				var reply_to_display_name = script_helpers.GetData ("ReplyToDisplayName");
				if (!string.IsNullOrEmpty (reply_to_display_name))
					msg.ReplyTo = new MailAddress (reply_to, reply_to_display_name);
				else
					msg.ReplyTo = new MailAddress (reply_to);
			}

			if (string.IsNullOrEmpty (msg.Subject))
				throw new InvalidOperationException ("Mail formatting xslt '" + xsltPath + "' does not set 'Subject' data via script:SetData method.");

			msg.IsBodyHtml = true;

			try
			{
				var smtp = new SmtpClient ();
				smtp.Send (msg);

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Sends mail using xslt for formatting. Returns true if mail send successfully, otherwise - false.
		/// </summary>
		/// <param name="xml">Mail data.</param>
		/// <param name="xsltPath">Path to mail formatting xslt (web path).</param>
		/// <param name="to">Recipient (can be many - in that case separate them with semicolon).</param>
		/// <param name="additionalXsltArguments">List of name-value pairs of additional arguments that will be passed to xslt.</param>
		/// <returns>True if mail send successfully, otherwise - false.</returns>
		static public bool SendMailXslt (string xml, string xsltPath, string to, params object[] additionalXsltArguments)
		{
			var msg = new MailMessage ();

#if DEBUG
			if (!string.IsNullOrEmpty (ConfigurationManager.AppSettings["Debug.Email.RedirectAll"]))
				to = ConfigurationManager.AppSettings["Debug.Email.RedirectAll"];
#endif

			var to_emails = to.Split (new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (to_emails.Length < 1)
				return false;

			foreach (var email in to_emails)
				msg.To.Add (new MailAddress (email));

			var args = new XsltArgumentList ();
			var script_helpers = new XsltScriptHelpers ();
			args.AddExtensionObject ("urn:script", script_helpers);

			msg.Body = Utils.XsltTransform (xml, args, xsltPath);
			msg.Subject = script_helpers.GetData ("Subject");

			var reply_to = script_helpers.GetData ("ReplyTo");
			if (!string.IsNullOrEmpty (reply_to))
			{
				var reply_to_display_name = script_helpers.GetData ("ReplyToDisplayName");
				if (!string.IsNullOrEmpty (reply_to_display_name))
					msg.ReplyTo = new MailAddress (reply_to, reply_to_display_name);
				else
					msg.ReplyTo = new MailAddress (reply_to);
			}

			if (string.IsNullOrEmpty (msg.Subject))
				throw new InvalidOperationException ("Mail formatting xslt '" + xsltPath + "' does not set 'Subject' data via script:SetData method.");

			msg.IsBodyHtml = true;

			try
			{
				var smtp = new SmtpClient ();
				smtp.Send (msg);

				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		#region LINQ

		///// <summary>
		///// Apply sorting based on specified property <paramref name="name"/> and sort <paramref name="direction"/>.
		///// </summary>
		///// <typeparam name="T">Type of the objects.</typeparam>
		///// <param name="source">Objects that will be sorted.</param>
		///// <param name="name">Name of the property which to sort on.</param>
		///// <param name="direction">Sort direction.</param>
		///// <returns>Sorted objects.</returns>
		//public static IQueryable<T> OrderBy<T> (this IQueryable<T> source, string name, SortDirection direction) where T : class
		//{
		//    Type type = typeof (T);
		//    PropertyInfo property = type.GetProperty (name);
		//    ParameterExpression parameter = Expression.Parameter (type, "p");
		//    MemberExpression member_expression = Expression.MakeMemberAccess (parameter, property);
		//    LambdaExpression expression = Expression.Lambda (member_expression, parameter);

		//    MethodCallExpression xml = Expression.Call (
		//        typeof (Queryable),
		//        direction == SortDirection.Ascending ? "OrderBy" : "OrderByDescending",
		//        new Type[] { type, property.PropertyType },
		//        source.Expression, Expression.Quote (expression));

		//    return (IQueryable<T>) source.Provider.CreateQuery (xml);
		//}

		#endregion

		#region DB

		private static readonly Regex RegexKeywordsFormsOfCleanUp = new Regex (@"[^\""\*\s\w]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Создание поискового запроса по ключевым словам
		/// </summary>
		/// <param name="keywords">Входная строка поиска</param>
		/// <returns>Преобразованая строка</returns>
		public static string MakeKeywordsFormsOf (string keywords)
		{
			return MakeKeywordsFormsOf (keywords, 3);
		}

		/// <summary>
		/// Создание поискового запроса по ключевым словам
		/// </summary>
		/// <param name="keywords">Входная строка поиска</param>
		/// <param name="minLength">Minimal length.</param>
		/// <returns>Преобразованая строка</returns>
		public static string MakeKeywordsFormsOf (string keywords, int minLength)
		{
			if (string.IsNullOrEmpty (keywords))
				throw new InvalidOperationException ("Слишком короткое ключевое слово");

			keywords = Utils.RegexKeywordsFormsOfCleanUp.Replace (keywords, " ");
			keywords = keywords.Trim ();

			if (keywords.Length < minLength)
				throw new InvalidOperationException ("Слишком короткое ключевое слово");

			return Utils.ConvertToString (
				from x in keywords.Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				select "formsof(inflectional," + x.Trim () + ")",
				" AND ");
		}

		private static readonly Regex RegexForSelect = new Regex (@"(?:(?<1>[\w\-\.]+@([\w\-]+\.)+\w{2,4})|(?<1>\w+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Создание регекса для подсвечивание найденых слов в результатах поиска
		/// </summary>
		/// <param name="keywords">поисковые слова</param>
		/// <returns>регекс</returns>
		public static Regex MakeRegexForSelectWord (string keywords)
		{
			Regex regMark = null;

			if (!String.IsNullOrEmpty (keywords))
			{
				var mc = RegexForSelect.Matches (keywords);

				int i;
				string word;
				var sb = new StringBuilder ();
				foreach (Match m in mc)
				{
					word = m.Groups[0].Value;

					if (!Int32.TryParse (word, out i) && word.IndexOf ('@') < 0)
					{
						if (word.Length > 5)
							word = word.Substring (0, word.Length - 3);
						else
							word = word.Substring (0, word.Length - 1);
					}

					if (sb.Length > 0)
						sb.Append ("|");

					sb.AppendFormat (CultureInfo.InvariantCulture, "(\\b" + word + "\\w*\\b)");
				}

				regMark = new Regex ("(" + sb + ")", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}

			return regMark;
		}
		#endregion

		#region HTML

		private static readonly Regex regexRemoveHtmlTags1 = new Regex ("<[^>]*>", RegexOptions.ECMAScript | RegexOptions.Compiled);
		private static readonly Regex regexRemoveHtmlTags2 = new Regex ("\\S{50}", RegexOptions.Compiled);
		private static readonly Regex regexRemoveHtmlTags3 = new Regex ("([.,])(\\S)", RegexOptions.ECMAScript | RegexOptions.Compiled);
		private static readonly Regex regexRemoveHtmlTags4 = new Regex ("(\\S{49})(\\S)", RegexOptions.Compiled);

		/// <summary>
		/// Метод, осуществляющий удаления тегов из строки, а так же заменяющий \n на &lt;br /&gt;
		/// </summary>
		/// <param name="str">Входная строка</param>
		/// <param name="insertBreaks">если true, заменяет \n на &lt;br /&gt;</param>
		/// <returns>Результат</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "str")]
		public static string RemoveHtmlTags (string str, bool insertBreaks)
		{
			str = Utils.regexRemoveHtmlTags1.Replace (str, "");

			if (Utils.regexRemoveHtmlTags2.Match (str).Success)
				str = Utils.regexRemoveHtmlTags3.Replace (str, "$1 $2");

			str = Utils.regexRemoveHtmlTags4.Replace (str, "$1 $2");

			if (insertBreaks)
				str = Utils.ReplaceNewLinesToBreaks (str);

			return str;
		}


		/// <summary>
		/// Метод, осуществляющий trim, удаления тегов из строки, а так же заменяющий \n на &lt;br /&gt;
		/// </summary>
		/// <param name="str">Входная строка</param>
		/// <param name="insertBreaks">если true, заменяет \n на &lt;br /&gt;</param>
		/// <returns>Результат</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "str")]
		public static string RemoveHtmlTagsAndTrim (string str, bool insertBreaks)
		{
			if (str == null)
				return null;

			str = str.Trim ();
			return RemoveHtmlTags (str, insertBreaks);
		}




		private static readonly Regex regexReplaceBreaksToNewLines = new Regex (@"<br\s*\/?>", RegexOptions.Compiled);
		private static readonly Regex regexReplaceNewLinesToBreaks = new Regex (@"\r?\n", RegexOptions.Compiled);

		/// <summary>
		/// Replaces &lt;br /&gt; with \r\n.
		/// </summary>
		/// <param name="str">Source string.</param>
		/// <returns>Replaced string.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "str")]
		public static string ReplaceBreaksToNewLines (string str)
		{
			if (string.IsNullOrEmpty (str))
				return str;

			return Utils.regexReplaceBreaksToNewLines.Replace (str, "\r\n");
		}

		/// <summary>
		/// Replaces \r\n with &lt;br /&gt;.
		/// </summary>
		/// <param name="str">Source string.</param>
		/// <returns>Replaced string.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "str")]
		public static string ReplaceNewLinesToBreaks (string str)
		{
			if (string.IsNullOrEmpty (str))
				return str;

			return Utils.regexReplaceNewLinesToBreaks.Replace (str, "<br />");
		}

		/// <summary>
		/// Соединяет строки и убирает лишние пробелы
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ConcatAndNormalizeSpace (params string[] value)
		{
			var sb = new StringBuilder ("");

			foreach (var s in value)
			{
				sb.AppendFormat (" {0}", s);
			}

			var res = sb.ToString ().Trim ();
			res = Regex.Replace (res, @"\s{2,}", " ");

			return res;
		}

		/// <summary>
		/// Converts <paramref name="value"/> (can be null) to string representation that can be used directly in javascript.
		/// </summary>
		/// <param name="value">Object value.</param>
		/// <param name="forSingleQuotes">If true then replace ' to  \', otherwise replace " to \"</param>
		/// <returns>String representation of the object.</returns>
		public static string ConvertToStringForJs (object value, bool forSingleQuotes)
		{
			if (value == null)
				return string.Empty;

			var res = value.ToString ();

			if (forSingleQuotes)
				res = res.Replace ("'", "\\'");
			else
				res = res.Replace ("\"", "\\\"");

			return res;
		}

		/// <summary>
		/// Converts all found URLs within specified text into links (HTML anchors).
		/// </summary>
		/// <param name="text">Source text.</param>
		/// <returns>Converted text.</returns>
		public static string UrlsToLinks (string text)
		{
			if (string.IsNullOrEmpty (text))
				return string.Empty;

			foreach (Match m in Regex.Matches (text,
				@"
					(?<protocol>
						[a-z]{3,5}://
					)?
					(?<url>
						[\w\.\/\-]+\.(?:[a-z]{2,3}|aero|asia|cat|coop|edu|gov|int|jobs|mil|mobi|museum|tel|travelarpa|nato)
						[^\s]*
					)",
					  Utils.RegexDefaultOptions))
			{
				var protocol = m.Groups["protocol"].Value;
				if (string.IsNullOrEmpty (protocol))
					protocol = "http://";

				text = text.Replace (m.Groups[0].Value,
					string.Format (CultureInfo.InvariantCulture,
						"<noindex><a href=\"{1}{2}\" target=\"_blank\" rel=\"nofollow\">{0}</a></noindex>",
						m.Groups[0].Value,
						protocol, m.Groups["url"].Value));
			}

			return text;
		}

		#endregion

		#region Keywords

		/// <summary>
		/// Normalize keywords string.
		/// </summary>
		/// <param name="keywords">Keywords string.</param>
		/// <returns></returns>
		public static string NormalizeKeywords (string keywords)
		{
			if (string.IsNullOrEmpty (keywords))
				return null;

			var res = new Dictionary<string, object> ();
			foreach (var keyword in keywords.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var k = keyword.Trim ().ToLowerInvariant ();
				if (!res.ContainsKey (k))
					res.Add (k, null);
			}

			return string.Join (", ", res.Keys.ToArray ());
		}

		#endregion

		#region WebServices

		/// <summary>
		/// Information about login attempt.
		/// </summary>
		public class HackAttemptInfo
		{
			public DateTime StartDate { get; set; }
			public int AttemptsCount { get; set; }
		}

		/// <summary>
		/// Check for bruteforce hacking attempt. If function with the same <paramref name="actionName"/> will be called more or equal to <paramref name="maxAttempts"/> times withting this time window the function will return false. Otherwise will return true.
		/// </summary>
		/// <param name="actionName">Name of action to check (i.e. "login" + user_email).</param>
		/// <param name="inactiveResetTime">Time of inactivity after which data about action attempts will be resetted.</param>
		/// <param name="maxAttempts">Max allowed attempts count.</param>
		/// <param name="maxAttemptsWindow">If attempts will be more or equal to <paramref name="maxAttempts"/> withting this time window the function will return false.</param>
		/// <returns>Hack attempt flag.</returns>
		public static bool CheckForBruteforceHackAttempt (string actionName, TimeSpan inactiveResetTime, int maxAttempts, TimeSpan maxAttemptsWindow)
		{
			var cache_key = "HackAttempt." + actionName;
			var info = CacheManager.GetItem<HackAttemptInfo> (cache_key);
			if (info == null)
			{
				info = new HackAttemptInfo ();
				info.StartDate = DateTime.Now;
				info.AttemptsCount = 1;

				CacheManager.AddItem (cache_key, info, null, Cache.NoAbsoluteExpiration, inactiveResetTime, CacheItemPriority.Normal, null);
			}
			else
			{
				info.AttemptsCount++;

				if (info.AttemptsCount >= maxAttempts && (DateTime.Now - info.StartDate) <= maxAttemptsWindow)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Calls <see cref="CheckForBruteforceHackAttempt"/> with inactiveResetTime = 10 mins, maxAttempts = 20 and maxAttemptsWindow = 1 min
		/// </summary>
		/// <param name="actionName">Name of action to check (i.e. "login" + user_email).</param>
		/// <returns>Hack attempt flag.</returns>
		public static bool CheckForBruteforceHackAttempt (string actionName)
		{
			return Utils.CheckForBruteforceHackAttempt (actionName, TimeSpan.FromMinutes (10), 20, TimeSpan.FromMinutes (1));
		}

		#endregion

		#region Validators

		/// <summary>
		/// Validates list of required fields.
		/// </summary>
		/// <param name="values">List of values.</param>
		/// <returns>True if all values are valid or false otherwise.</returns>
		public static bool ValidateRequired (params string[] values)
		{
			foreach (var value in values)
			{
				if (string.IsNullOrEmpty (value) || string.IsNullOrEmpty (value.Trim ()))
					return false;
			}

			return true;
		}

		private static readonly Regex validateRegExEmail = new Regex (@"^\s*[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})\s*$",
			RegexOptions.Compiled |
			RegexOptions.CultureInvariant |
			RegexOptions.IgnoreCase |
			RegexOptions.IgnorePatternWhitespace |
			RegexOptions.Multiline |
			RegexOptions.Singleline);

		/// <summary>
		/// Validates email field.
		/// </summary>
		/// <param name="value">Course to validate.</param>
		/// <returns>True if value is valid or false otherwise.</returns>
		public static bool ValidateEmail (string value)
		{
			return Utils.validateRegExEmail.IsMatch (value);
		}

		#endregion

		#region HTTP

		/// <summary>
		/// Generates ETag
		/// </summary>
		/// <param name="url">Page url.</param>
		/// <param name="modified">Date of the last modification.</param>
		/// <returns>ETag value.</returns>
		public static string GetETag (string url, DateTime modified)
		{
			return Utils.GetETag (url, modified, null);
		}

		/// <summary>
		/// Generates ETag with vary by custom string cache values.
		/// </summary>
		/// <param name="url">Page url.</param>
		/// <param name="modified">Date of the last modification.</param>
		/// <param name="varyByCustom">Vary by custom cache item value(s).</param>
		/// <returns>ETag value.</returns>
		public static string GetETag (string url, DateTime modified, IEnumerable<string> varyByCustom)
		{
			return Utils.GetETag (url, modified, varyByCustom, null);
		}

		/// <summary>
		/// Generates ETag with vary by custom string cache values.
		/// </summary>
		/// <param name="url">Page url.</param>
		/// <param name="modified">Date of the last modification.</param>
		/// <param name="varyByCustom">Vary by custom cache item value(s).</param>
		/// <param name="getParams">Name of the GET parameters which values to include into etag computation.</param>
		/// <returns>ETag value.</returns>
		public static string GetETag (string url, DateTime modified, IEnumerable<string> varyByCustom, IEnumerable<string> getParams)
		{
			var str = url + modified.ToString ("G");

			if (varyByCustom != null)
			{
				foreach (var n in varyByCustom)
					str += CacheManager.GetVaryByCustomStringValue (n);
			}

			if (getParams != null)
			{
				var context = HttpContext.Current;

				foreach (var n in varyByCustom)
					str += context.Request.QueryString[n];
			}

			return Utils.GetETag (str);
		}

		/// <summary>
		/// Generates ETag with vary by custom string cache values.
		/// </summary>
		/// <param name="str">ETag unhashed string.</param>
		/// <returns>ETag value.</returns>
		public static string GetETag (string str)
		{
			var p = SHA1.Create ();
			var hash = p.ComputeHash (Encoding.Default.GetBytes (str));

			return Convert.ToBase64String (hash);
		}

		/// <summary>
		/// Редирект с 301 кодом
		/// </summary>
		/// <param name="url">урл</param>
		public static void PermanentRedirect (string url)
		{
			HttpContext.Current.Response.Status = "301 Moved Permanently";
			HttpContext.Current.Response.AddHeader ("Location", url);
			HttpContext.Current.Response.End ();
		}

		/// <summary>
		/// Return the value of "If-Modified-Since" header.
		/// </summary>
		/// <returns>Date time value or null.</returns>
		public static DateTime? GetIfModifiedSince ()
		{
			var str = HttpContext.Current.Request.Headers["If-Modified-Since"];
			if (!string.IsNullOrEmpty (str))
			{
				DateTime dt;
				if (DateTime.TryParseExact (str.Split (new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0], "r", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt))
					return dt;
			}

			return null;
		}

		/// <summary>
		/// Gets the list of all accepted encodings for current request.
		/// </summary>
		/// <returns>List of names in lower case.</returns>
		public static string[] GetAcceptEncodings ()
		{
			var accept_encoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
			if (!string.IsNullOrEmpty (accept_encoding))
				return accept_encoding.ToLowerInvariant ().Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			return new string[] { };
		}

		/// <summary>
		/// Sets the current context response compression filter if possible.
		/// </summary>
		public static void SetCompressFilter ()
		{
			if (string.Compare (ConfigurationManager.AppSettings["EnableHttpCompression"], "true", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				var encodings = Utils.GetAcceptEncodings ();
				if (encodings.Any (x => x == "gzip"))
				{
					if (HttpContext.Current.Response.Filter == null || !(HttpContext.Current.Response.Filter is GZipStream))
					{
						HttpContext.Current.Response.Filter = new GZipStream (HttpContext.Current.Response.Filter, CompressionMode.Compress);
						HttpContext.Current.Response.AppendHeader ("Content-Encoding", "gzip");
						HttpContext.Current.Response.AppendHeader ("Vary", "Content-Encoding");
					}
				}
				else if (encodings.Any (x => x == "deflate"))
				{
					if (HttpContext.Current.Response.Filter == null || !(HttpContext.Current.Response.Filter is DeflateStream))
					{
						HttpContext.Current.Response.Filter = new DeflateStream (HttpContext.Current.Response.Filter, CompressionMode.Compress);
						HttpContext.Current.Response.AppendHeader ("Content-Encoding", "deflate");
						HttpContext.Current.Response.AppendHeader ("Vary", "Content-Encoding");
					}
				}
			}
		}

		/// <summary>
		/// Checks if current request comes from old browser.
		/// </summary>
		/// <returns>True if browser considered old, false - otherwise.</returns>
		public static bool IsOldBrowser ()
		{
			var c = HttpContext.Current;

			var browser = c.Request.Browser.Browser.ToLowerInvariant ();
			var version_str = Regex.Replace (c.Request.Browser.Version, @".*?(\d+\.\d*).*", "${1}", Utils.RegexDefaultOptions);
			var version = Utils.ConvertString (version_str, 0.0);

			return
					(browser == "firefox" && version < 3)
					||
					(browser == "ie" && version < 7)
					||
					(browser == "opera" && version < 9.6)
					||
					(browser == "safari" && version < 4)
					||
					(browser == "chrome" && version < 3);
		}

		#endregion

		#region Cookie

		private const string CookieListCachePrefix = "CookieList.";

		/// <summary>
		/// Set specified items list to the cookie.
		/// </summary>
		/// <param name="name">Cookie name.</param>
		/// <param name="items">List of items.</param>
		public static void SetCookieList<T> (string name, T[] items)
		{
			var c = HttpContext.Current;
			c.Items[Utils.CookieListCachePrefix + name] = items;

			var cookie = new HttpCookie (name, Utils.ConvertToString (items));
			cookie.Path = "/";

			c.Response.Cookies.Add (cookie);
		}

		/// <summary>
		/// Add specified item to cookie items list.
		/// </summary>
		/// <param name="name">Cookie name.</param>
		/// <param name="item">Item to be added.</param>
		/// <param name="maxItems">Max items in cookie list.</param>
		public static void AddCookieListItem<T> (string name, T item, int maxItems)
		{
			var items = new List<T> (Utils.GetCookieList<T> (name, maxItems));
			if (!items.Contains (item))
				items.Insert (0, item);

			var c = HttpContext.Current;

			c.Items[Utils.CookieListCachePrefix + name] = items.Take (maxItems).ToArray ();

			var cookie = new HttpCookie (name, Utils.ConvertToString (items));
			cookie.Path = "/";

			c.Response.Cookies.Add (cookie);
		}

		/// <summary>
		/// Gets the list of items from cookie (default is empty list).
		/// </summary>
		/// <typeparam name="T">Type of the item.</typeparam>
		/// <param name="name">Cookie name.</param>
		/// <param name="maxItems">Maximum items to take.</param>
		/// <returns>List of items.</returns>
		public static T[] GetCookieList<T> (string name, int maxItems)
		{
			var c = HttpContext.Current;

			var res = c.Items[Utils.CookieListCachePrefix + name] as T[];
			if (res != null)
				return res;

			var cookie = c.Request.Cookies[name];
			if (cookie == null)
				return new T[] { };

			var t = typeof (T);
			res = (from x in cookie.Value.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				   select (T) Convert.ChangeType (x, t, CultureInfo.InvariantCulture)).Distinct ().Take (maxItems).ToArray ();

			c.Items[Utils.CookieListCachePrefix + name] = res;
			return res;
		}

		/// <summary>
		/// Clears the list in the cookie.
		/// </summary>
		/// <param name="name">Cookie name.</param>
		public static void ClearCookieList (string name)
		{
			var c = HttpContext.Current;
			c.Response.Cookies.Add (new HttpCookie (name, string.Empty));
			c.Items[Utils.CookieListCachePrefix + name] = null;
		}

		#endregion

		#region Globalization

		/// <summary>
		/// Parse language info from web config app settings. Returns list of language info.
		/// </summary>
		/// <param name="appConfigKey">App settings key.</param>
		/// <returns>List language info.</returns>
		public static List<LanguageInfo> GetLanguages (string appSettingsKey)
		{
			var res = new List<LanguageInfo> ();

			foreach (Match m in Regex.Matches (ConfigurationManager.AppSettings[appSettingsKey], @"\s*(?<name>\w+)\s*\:\s*(?<culture>[\w\-]+)\s*,?\s*", Utils.RegexDefaultOptions))
			{
				var name = m.Groups["name"].Value.ToLowerInvariant ();
				var culture = m.Groups["culture"].Value;

				res.Add (new LanguageInfo { Name = name, Culture = culture });
			}

			return res;
		}

		#endregion
	}
}
