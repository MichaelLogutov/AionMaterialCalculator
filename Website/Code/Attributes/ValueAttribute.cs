using System;

namespace Website.Code.Attributes
{
	/// <summary>
	/// <para>Attribute that define a custom value that can be specified for target object.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 21 november 2007</para>
	/// </summary>
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Field)]
	public sealed class ValueAttribute : Attribute
	{
		#region Accessors

		#region Value

		/// <summary>
		/// <see cref="Value"/> internal field.
		/// </summary>
		private readonly object _Value;

		/// <summary>
		/// (GET) Value. Default value is null.
		/// </summary>
		public object Value
		{
			get
			{
				return this._Value;
			}
		}

		#endregion

		#endregion

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="value">Value.</param>
		public ValueAttribute (object value)
		{
			this._Value = value;
		}

		/// <summary>
		/// Converts enum value to related <see cref="ValueAttribute.Value"/>.
		/// </summary>
		/// <param name="value">The value of the enumeration.</param>
		/// <returns><see cref="ValueAttribute.Value"/></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		public static object EnumValueToAttributeValue (object value)
		{
			return ValueAttribute.EnumValueToAttributeValue (value.GetType (), value.ToString ());
		}

		/// <summary>
		/// Converts enum value to related <see cref="ValueAttribute.Value"/>.
		/// </summary>
		/// <param name="type">The type of the enumeration.</param>
		/// <param name="name">The name of the enum value.</param>
		/// <returns><see cref="ValueAttribute.Value"/></returns>
		public static object EnumValueToAttributeValue (Type type, string name)
		{
			var fi = type.GetField (name);
			var attributes = (ValueAttribute[]) fi.GetCustomAttributes (typeof (ValueAttribute), false);

			if (attributes.Length < 1)
				throw new InvalidOperationException ("Enum value \"" + type + "." + name + "\" does not have associated ValueAttribute.");

			return attributes[0].Value;
		}

		/// <summary>
		/// Converts <see cref="ValueAttribute.Value"/> to related enum value.
		/// </summary>
		/// <typeparam name="T">The Enum type.</typeparam>
		/// <param name="value">Enum value or name of the enum element.</param>
		/// <returns>The value, or the passed in description, if it was not found.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		public static T AttributeValueToEnumValue<T> (string value)
		{
			return (T) ValueAttribute.AttributeValueToEnumValue (typeof (T), value);
		}

		/// <summary>
		/// Converts <see cref="ValueAttribute.Value"/> to related enum value.
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
				var attributes = (ValueAttribute[]) fi.GetCustomAttributes (typeof (ValueAttribute), false);

				if (attributes.Length < 1)
					continue;

				for (var k = 0; k < attributes.Length; k++)
				{
					if (attributes[k].Value.ToString () == value)
						return fi.GetValue (fi.Name);
				}
			}

			throw new InvalidOperationException ("Unable to convert \"" + value + "\" to \"" + type + "\" enum representation based on ValueAttribute.");
		}
	}
}
