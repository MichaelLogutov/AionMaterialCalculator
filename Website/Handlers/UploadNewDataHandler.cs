using System;
using System.IO;
using System.Linq;
using Website.Code.Handlers;

namespace Website.Handlers
{
	public class UploadNewDataHandler : BaseHandler
	{
		protected override void Initialize ()
		{
			base.Initialize ();
			this.NoCache = true;
		}

		protected override bool Process (DateTime? lastModified, string etag)
		{
			if (this.Context.Request.Files.Count < 1)
				throw new InvalidOperationException ("No files to upload");

			var file = this.Context.Request.Files[0];
			var path = Path.Combine (this.Context.Server.MapPath ("~/Recipes"), "data.zip");

			if (File.Exists (path))
				File.Delete (path);

			file.SaveAs (path);
			return true;
		}
	}
}
