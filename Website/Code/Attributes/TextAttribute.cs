using System;

namespace Website.Code.Attributes
{
	/// <summary>
	/// <para>Attribute that define a custom text that can be specified for target object.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 21 november 2007</para>
	/// </summary>
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Field)]
	public sealed class TextAttribute : Attribute
	{
		#region Accessors

		#region Text

		/// <summary>
		/// <see cref="Text"/> internal field.
		/// </summary>
		private readonly string _Text;

		/// <summary>
		/// (GET) Value. Default value is null.
		/// </summary>
		public string Text
		{
			get
			{
				return this._Text;
			}
		}

		#endregion

		#endregion

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="text">Text.</param>
		public TextAttribute (string text)
		{
			this._Text = text;
		}

		/// <summary>
		/// Converts enum value to related <see cref="TextAttribute.Text"/>.
		/// </summary>
		/// <param name="value">The value of the enumeration.</param>
		/// <returns><see cref="TextAttribute.Text"/></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		public static string EnumValueToAttributeValue (object value)
		{
			return TextAttribute.EnumValueToAttributeValue (value.GetType (), value.ToString ());
		}

		/// <summary>
		/// Converts enum value to related <see cref="TextAttribute.Text"/>.
		/// </summary>
		/// <param name="type">The type of the enumeration.</param>
		/// <param name="name">The name of the enum value.</param>
		/// <returns><see cref="TextAttribute.Text"/></returns>
		public static string EnumValueToAttributeValue (Type type, string name)
		{
			var fi = type.GetField (name);
			var attributes = (TextAttribute[]) fi.GetCustomAttributes (typeof (TextAttribute), false);

			if (attributes.Length < 1)
				throw new InvalidOperationException ("Enum value \"" + type + "." + name + "\" does not have associated TextAttribute.");

			return attributes[0].Text;
		}

		/// <summary>
		/// Converts <see cref="TextAttribute.Text"/> to related enum value.
		/// </summary>
		/// <typeparam name="T">The Enum type.</typeparam>
		/// <param name="value">Enum value or name of the enum element.</param>
		/// <returns>The value, or the passed in description, if it was not found.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		public static T AttributeValueToEnumValue<T> (string value)
		{
			return (T) TextAttribute.AttributeValueToEnumValue (typeof (T), value);
		}

		/// <summary>
		/// Converts <see cref="TextAttribute.Text"/> to related enum value.
		/// </summary>
		/// <param name="type">The Enum type.</param>
		/// <param name="value">Enum value or name of the enum element.</param>
		/// <returns>The value, or the passed in description, if it was not found.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		public static object AttributeValueToEnumValue (Type type, string value)
		{
			var fis = type.GetFields ();
			foreach (var fi in fis)
			{
				var attributes = (TextAttribute[]) fi.GetCustomAttributes (typeof (TextAttribute), false);

				if (attributes.Length < 1)
					continue;

				for (var k = 0; k < attributes.Length; k++)
				{
					if (attributes[k].Text == value)
						return fi.GetValue (fi.Name);
				}
			}

			throw new InvalidOperationException ("Unable to convert \"" + value + "\" to \"" + type + "\" enum representation based on TextAttribute.");
		}
	}
}
