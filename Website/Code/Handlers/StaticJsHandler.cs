using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace Website.Code.Handlers
{
	/// <summary>
	/// <para>Static javascript file handler.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 20 february 2009</para>
	/// </summary>
	public class StaticJsHandler : BaseHandler
	{
		protected override void Initialize ()
		{
			base.Initialize ();

			this.NoCache = (ConfigurationManager.AppSettings["CacheManager.CacheStaticJs"] != "true");
			this.CacheVaryByParams.Add ("n");
			this.CacheVaryByParams.Add ("v");

			this.Context.Response.ContentEncoding = Encoding.UTF8;
		}

		protected override bool Process (DateTime? lastModified, string etag)
		{
			this.LastModified = DateTime.Today;

			if ((lastModified.HasValue && lastModified.Value == this.LastModified.Value)
				||
				etag == this.GetETag (this.LastModified.Value))
				return false;

			var result = new StringBuilder ();

			var n = this.Context.Request.QueryString["n"];
			if (string.IsNullOrEmpty (n))
				n = string.Empty;

			var config = ConfigurationManager.AppSettings["StaticJs" + n];
			if (!string.IsNullOrEmpty (config))
			{
				var paths = config.Split (new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var path in paths)
				{
					var file_path = path.Trim ();

					if (string.IsNullOrEmpty (file_path))
						continue;

					if (file_path[0] == '/')
						file_path = "~" + file_path;

					var str = File.ReadAllText (this.Context.Server.MapPath (file_path), Encoding.UTF8);
					result.AppendLine (str);
				}
			}

			this.Context.Response.Write (result);
			this.Context.Response.ContentType = "text/javascript";

			return true;
		}
	}
}