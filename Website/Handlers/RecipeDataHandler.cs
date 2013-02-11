using System;
using System.IO;
using System.Linq;
using Website.Code;
using Website.Code.Handlers;

namespace Website.Handlers
{
	public class RecipeDataHandler : BaseHandler
	{
		protected override void Initialize ()
		{
			base.Initialize ();

			this.CacheVaryByParams.Add ("rid");
			this.CacheVaryByParams.Add ("id");
			this.CacheVaryByParams.Add ("l");
			this.CacheVaryByParams.Add ("v");
		}

		protected override bool Process (DateTime? lastModified, string etag)
		{
			var rid = Utils.ConvertString (this.Context.Request.QueryString["rid"], 0);
			var id = Utils.ConvertString (this.Context.Request.QueryString["id"], 0);


			var languages = Utils.GetLanguages ("Languages");
			var lang = languages.SingleOrDefault (x => x.Name == (this.Context.Request.QueryString["l"] ?? string.Empty).ToLowerInvariant ());

			if (lang == null)
				throw new InvalidOperationException ("Invalid arguments");

			var path = Path.Combine (this.Context.Server.MapPath ("~/Recipes/" + lang.Name), rid + "_" + id + ".js");
			this.LastModified = File.GetLastWriteTime (path);

			if ((lastModified.HasValue && lastModified.Value == this.LastModified.Value)
				||
				etag == this.GetETag (this.LastModified.Value))
				return false;

			if (!File.Exists (path))
			{
				this.Set404 ();
				return true;
			}

			Utils.SetCompressFilter ();
			this.Context.Response.Write (File.ReadAllText (path));
			this.Context.Response.ContentType = "text/plain";

			return true;
		}
	}
}
