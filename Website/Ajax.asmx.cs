using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml.Linq;
using System.Xml.XPath;
using Ionic.Zip;
using Website.Code;

namespace Website
{
	/// <summary>
	/// <para>Proceed all ajax queries.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 13 october 2009</para>
	/// </summary>
	[WebService (Namespace = "http://www.lmstudio.ru/")]
	[WebServiceBinding (ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem (false)]
	[ScriptService]
	public class Ajax : BaseWebService
	{
		[WebMethod]
		public AjaxResult GetRecipeData (int recipeId, int itemId)
		{
			try
			{
				this.HandleCall ();

				var cache_key = "AionItemRecipeData" + itemId;
				var res = new AjaxResult (0, CacheManager.GetItem<string> (cache_key));

				if (string.IsNullOrEmpty (res.Data))
				{
					res.Data = File.ReadAllText (HttpContext.Current.Server.MapPath ("~/Aion/Recipes/" + recipeId + "_" + itemId + ".js"));
					CacheManager.AddItem (cache_key, res.Data, 60);
				}

				Utils.SetCompressFilter ();

				return res;
			}
			catch (Exception ex)
			{
				return this.HandleException (ex);
			}
		}

		[WebMethod]
		public AjaxResult Feedback (string email, string message, string additionalInfo)
		{
			try
			{
				this.HandleCall ();

				if (!Utils.ValidateRequired (email, message) || !Utils.ValidateEmail (email))
					return AjaxResult.WrongParametersOrHackAttempt;

				var xml = new XDocument (
						new XElement ("Root",
							new XElement ("Email", Utils.RemoveHtmlTagsAndTrim (email, false)),
							new XElement ("Message", Utils.RemoveHtmlTagsAndTrim (message, true)),
							new XElement ("AdditionalInfo", Utils.RemoveHtmlTagsAndTrim (additionalInfo, true))
						)
					);

				if (this.Context.Request.Browser != null)
					xml.Root.Add (new XElement ("Browser", this.Context.Request.Browser.Platform + " " + this.Context.Request.Browser.Browser + " " + this.Context.Request.Browser.Version));

				if (this.Context.Request.UserAgent != null)
					xml.Root.Add (new XElement ("UserAgent", this.Context.Request.UserAgent));

				if (this.Context.Request.UserHostAddress != null)
					xml.Root.Add (new XElement ("UserHostAddress", this.Context.Request.UserHostAddress));

				Utils.SendMailXslt (xml.CreateNavigator (), "~/Xslt/EmailFeedback.xslt", ConfigurationManager.AppSettings["SupportEmail"]);

				return AjaxResult.Success;
			}
			catch (Exception ex)
			{
				return this.HandleException (ex);
			}
		}

		private void ProceedNewDataThread (object target)
		{
			try
			{
				var id = (Guid) target;

				var dir = this.Server.MapPath ("~/Aion/Recipes");
				var path = Path.Combine (dir, "data.zip");
				using (var zip = ZipFile.Read (path))
				{
					var total_files = zip.Count;
					var k = 0;
					foreach (var f in zip)
					{
						var fname = Path.Combine (dir, f.FileName);
						if (File.Exists (fname))
							File.Delete (fname);

						f.ZipErrorAction = ZipErrorAction.Throw;
						for (var i = 0; i < 5; i++)
						{
							try
							{
								f.Extract (dir, ExtractExistingFileAction.OverwriteSilently);
								break;
							}
							catch
							{
							}
						}

						k++;
						this.Application["Aion.DataUpdateProgress." + id] = Math.Round (100.0 * k / total_files);
					}
				}

				this.Application.Remove ("Aion.DataUpdateProgress." + id);
				File.Delete (path);
			}
			catch
			{
			}
		}

		[WebMethod]
		public AjaxResult ProceedNewData ()
		{
			try
			{
				this.HandleCall ();
				this.CheckAuthenticated ();

				if (this.User.Identity.Name != "admin")
					throw new InvalidOperationException ("You don't have a permission to access this page");

				var process_id = Guid.NewGuid ();
				this.Application["Aion.DataUpdateProgress." + process_id] = 0;

				var t = new Thread (this.ProceedNewDataThread);
				t.Start (process_id);

				return new AjaxResult (0, process_id.ToString ());
			}
			catch (Exception ex)
			{
				return this.HandleException (ex);
			}
		}

		[WebMethod]
		public AjaxResult GetProceedNewDataProgress (string id)
		{
			try
			{
				this.HandleCall ();
				this.CheckAuthenticated ();

				if (this.User.Identity.Name != "admin")
					throw new InvalidOperationException ("You don't have a permission to access this page");

				var process_id = new Guid (id);
				var progress = this.Application["Aion.DataUpdateProgress." + process_id];
				if (progress == null)
					return AjaxResult.Success;

				return new AjaxResult (0, progress.ToString ());
			}
			catch (Exception ex)
			{
				return this.HandleException (ex);
			}
		}
	}
}
