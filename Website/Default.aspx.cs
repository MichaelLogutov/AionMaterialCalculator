using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.UI;
using Resources;
using Website.Code;

namespace Website
{
	public partial class Default : Page
	{
		protected string version;
		protected List<LanguageInfo> languages;
		protected LanguageInfo currentLanguage;

		#region Culture setup

		/// <Summary>
		/// Sets the current UICulture and CurrentCulture based on the arguments.
		/// </Summary>
		/// <param name="locale">New locale.</param>
		/// <param name="name">New culture.</param>
		protected void SetCulture (string name, string locale)
		{
			this.UICulture = name;
			this.Culture = locale;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture (name);
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture (locale);

			this.Session["CurrentUICulture"] = Thread.CurrentThread.CurrentUICulture;
			this.Session["CurrentCulture"] = Thread.CurrentThread.CurrentCulture;
		}

		/// <summary>
		/// Handles culture initialization.
		/// </summary>
		protected override void InitializeCulture ()
		{
			try
			{
				// language bar
				{
					this.languages = Utils.GetLanguages ("Languages");
					var lang = (this.Request.QueryString["lang"] ?? string.Empty).Trim ().ToLowerInvariant ();

					this.currentLanguage = this.languages.SingleOrDefault (x => x.Name == lang);
					if (this.currentLanguage == null)
						this.currentLanguage = this.languages[0];

					this.SetCulture (this.currentLanguage.Culture, this.currentLanguage.Culture);
				}

				if (this.Session["CurrentUICulture"] != null && this.Session["CurrentCulture"] != null)
				{
					Thread.CurrentThread.CurrentUICulture = (CultureInfo) this.Session["CurrentUICulture"];
					Thread.CurrentThread.CurrentCulture = (CultureInfo) this.Session["CurrentCulture"];
				}
			}
			catch
			{
			}

			base.InitializeCulture ();
		}

		#endregion

		protected override void OnLoad (EventArgs e)
		{
			// language bar
			{
				var sb = new StringBuilder ();
				foreach (var lang in this.languages)
				{
					if (sb.Length > 0)
						sb.Append (" | ");

					sb.Append ("<a href='");
					sb.Append (Utils.UrlSetParameter (this.Request.Url.OriginalString, "lang", lang.Name));
					sb.Append ("'");

					if (lang.Name == this.currentLanguage.Name)
						sb.Append (" class='sel'");

					sb.Append ('>');
					sb.Append (lang.Name.ToUpperInvariant ());
					sb.Append ("</a>");
				}

				this.langLinks.Text = sb.ToString ();
			}

			this.Title = RecipeCalculator.s46;
			this.Page.Items["MetaDescription"] = RecipeCalculator.s46;
			this.Page.Items["MetaKeywords"] = "MMORPG, Aion, NCSoft";
			this.Page.Items["AdditionalHeaderIncludes"] = "<link rel='stylesheet' type='text/css' href='/Css/RecipeCalculator.css' />";

			this.version = Utils.GetCurrentVersion (true);

			base.OnLoad (e);
		}
	}
}
