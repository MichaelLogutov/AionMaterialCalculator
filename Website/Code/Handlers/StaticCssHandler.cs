using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Website.Code.Handlers
{
	/// <summary>
	/// <para>Static CSS file handler.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 15 july 2008</para>
	/// </summary>
	public class StaticCssHandler : BaseHandler
	{
		protected override void Initialize ()
		{
			base.Initialize ();

			this.NoCache = (CacheManager.GetConfigurationTimeSpan ("Css").TotalSeconds < 1);
			this.CacheVaryByParams.Add ("n");
			this.CacheVaryByParams.Add ("v");

			this.Context.Response.ContentEncoding = Encoding.UTF8;
		}

		protected override bool Process (DateTime? lastModified, string etag)
		{
			this.CacheExpire = DateTime.Now + CacheManager.GetConfigurationTimeSpan ("Css");
			this.LastModified = DateTime.Today;

			if ((lastModified.HasValue && lastModified.Value == this.LastModified.Value)
				||
				etag == this.GetETag (this.LastModified.Value))
				return false;

			var result = new StringBuilder ();

			var n = this.Context.Request.QueryString["n"];
			if (string.IsNullOrEmpty (n))
				n = string.Empty;

			var config = ConfigurationManager.AppSettings["StaticCss" + n];
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

					var process = (file_path[0] == '!');
					if (process)
						file_path = file_path.Substring (1);

					var css = File.ReadAllText (this.Context.Server.MapPath (file_path), Encoding.UTF8);

					if (process)
					{
						var m = Regex.Match (css, @".*?definitions.*?\{\s*(?<defs>.*?)\s*\}\s*(?<text>.*)", Utils.RegexDefaultOptions);

						if (m.Success)
						{
							var text = new StringBuilder (m.Groups["text"].Value);
							var defs = Regex.Matches (m.Groups["defs"].Value, @"(?<name>\%\w+\%)\s*\=\s*(?<value>.*?)\;", Utils.RegexDefaultOptions);

							foreach (Match def in defs)
								text = text.Replace (def.Groups["name"].Value, def.Groups["value"].Value);

							css = text.ToString ();
						}
					}

					result.AppendLine (css);
				}
			}

			this.Context.Response.Write (result);
			this.Context.Response.ContentType = "text/css";

			return true;
		}
	}
}