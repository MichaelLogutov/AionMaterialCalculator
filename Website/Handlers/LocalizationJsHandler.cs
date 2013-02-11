using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Resources;
using Website.Code;
using Website.Code.Handlers;

namespace Website.Handlers
{
	/// <summary>
	/// <para>Code generated localization javascript file handler.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 25 november 2009</para>
	/// </summary>
	public class LocalizationJsHandler : BaseHandler
	{
		protected override void Initialize ()
		{
			base.Initialize ();

			this.CacheVaryByParams.Add ("l");
		}

		protected override bool Process (DateTime? lastModified, string etag)
		{
			this.LastModified = File.GetLastWriteTime (this.GetType ().Assembly.Location);

			if ((lastModified.HasValue && lastModified.Value == this.LastModified.Value)
				||
				etag == this.GetETag (this.LastModified.Value))
				return false;

			var js = new List<string> ();

			// localization
			{
				var languages = Utils.GetLanguages ("Languages");
				var lang = languages.SingleOrDefault (x => x.Name == (this.Context.Request.QueryString["l"] ?? string.Empty).ToLowerInvariant ());

				if (lang == null)
					throw new InvalidOperationException ("Invalid arguments");

				var culture = CultureInfo.GetCultureInfo (lang.Culture);
				var culture_en = CultureInfo.GetCultureInfo ("en-US");

				var rs = RecipeCalculator.ResourceManager.GetResourceSet (culture, true, true);
				var rs_en = RecipeCalculator.ResourceManager.GetResourceSet (culture_en, true, true);
				if (rs != null && rs_en != null)
				{
					var e = rs_en.GetEnumerator ();
					while (e.MoveNext ())
					{
						if (e.Value == null)
							continue;

						var key = e.Key.ToString ();
						var value = rs.GetString (key);
						if (string.IsNullOrEmpty (value))
							value = e.Value.ToString ();

						js.Add ("'" + key + "':'" + value.Replace ("'", "\\'") + "'");
					}
				}
			}

			Utils.SetCompressFilter ();
			this.Context.Response.Write ("var Localization = {" + string.Join (",", js.ToArray ()) + "};");
			this.Context.Response.ContentType = "text/javascript";

			return true;
		}
	}
}
