using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Website.Code;

namespace Website
{
	public partial class Main : MasterPage
	{
		protected override void OnPreRender (EventArgs e)
		{
			Utils.SetCompressFilter ();

			this.Page.ClientScript.RegisterClientScriptBlock (this.GetType (), "reset_page_load", "var IsPageLoaded = false;", true);
			this.Page.ClientScript.RegisterStartupScript (this.GetType (), "set_page_load", "IsPageLoaded = true;", true);
			this.Page.ClientScript.RegisterOnSubmitStatement (this.GetType (), "global_submit", "if(!IsPageLoaded){alert('Пожалуйста, дождитесь завершения загрузки страницы.');return false;}");

			this.strOldBrowserWarning.Text = Resources.MasterPage.OldBrowserWarning;
			this.strNoJavaScriptWarning.Text = Resources.MasterPage.NoJavaScriptWarning;

			this.panelOldBrowser.Visible = Utils.IsOldBrowser ();
			var v = Utils.GetCurrentVersion (true);

			this.headIncludes.Text =
				"<link href='/StaticCss.axd?v=" + v + "' type='text/css' rel='stylesheet' />" +
				"<script type='text/javascript' src='/StaticJs.axd?v=" + v + "'></script>" +
				(this.Page.Items["AdditionalHeaderIncludes"] ?? string.Empty);

			// set meta "description"
			var meta_description = this.Page.Items["MetaDescription"] as string;
			if (!string.IsNullOrEmpty (meta_description))
			{
				var meta = new HtmlMeta
				{
					Name = "description",
					Content = meta_description
				};

				this.Page.Header.Controls.Add (meta);
			}

			// set meta "keywords"
			var meta_keywords = this.Page.Items["MetaKeywords"] as string;
			if (!string.IsNullOrEmpty (meta_keywords))
			{
				var meta = new HtmlMeta
				{
					Name = "keywords",
					Content = meta_keywords
				};

				this.Page.Header.Controls.Add (meta);
			}

			base.OnPreRender (e);
		}
	}
}
