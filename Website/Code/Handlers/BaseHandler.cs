using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Code.Handlers
{
	/// <summary>
	/// <para>Base class for all HTTP handlers.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 05 june 2009</para>
	/// </summary>
	public abstract class BaseHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return false; }
		}

		protected HttpContext Context { get; set; }
		protected bool NoCache { get; set; }
		protected DateTime? CacheExpire { get; set; }
		protected DateTime? LastModified { get; set; }
		protected HttpCacheability Cachability { get; set; }
		protected bool CacheValidUntilExpires { get; set; }
		protected List<string> CacheVaryByCustom { get; set; }
		protected List<string> CacheVaryByParams { get; set; }
		protected bool UseCompression { get; set; }

		/// <summary>
		/// Gets ETag for current request.
		/// </summary>
		/// <param name="lastModified">Last modified date.</param>
		/// <returns>ETag string.</returns>
		public string GetETag (DateTime lastModified)
		{
			return Utils.GetETag (this.Context.Request.RawUrl, lastModified, this.CacheVaryByCustom, this.CacheVaryByParams);
		}

		/// <summary>
		/// Sets the response code and response status to 404.
		/// </summary>
		public void Set404 ()
		{
			this.Context.Response.StatusCode = 404;
			this.Context.Response.StatusDescription = "Not Found";
			this.NoCache = true;
		}

		/// <summary>
		/// Sets the response code and response status to 500.
		/// </summary>
		public void Set500 ()
		{
			this.Context.Response.StatusCode = 500;
			this.Context.Response.StatusDescription = "Internal server error";
			this.NoCache = true;
		}

		/// <summary>
		/// Sets the response code and response status to 500.
		/// </summary>
		public void Set200 ()
		{
			this.Context.Response.StatusCode = 200;
			this.Context.Response.StatusDescription = "OK";
		}

		/// <summary>
		/// Handles HTTP request.
		/// </summary>
		/// <param name="context">Current context.</param>
		public void ProcessRequest (HttpContext context)
		{
			try
			{
				this.Context = context;
				this.Set200 ();

				this.Initialize ();

				// get etag and if-modified-since
				DateTime? dt_if_modified_since = null;
				string etag = null;

				if (CacheManager.IsEnabled && !this.NoCache)
				{
					etag = this.Context.Request.Headers["If-None-Match"];
					dt_if_modified_since = Utils.GetIfModifiedSince ();
				}

				// process
				if (!this.Process (dt_if_modified_since, etag))
				{
					this.Context.Response.StatusCode = 304;
					this.Context.Response.StatusDescription = "Not Modified";
				}
				else
				{
					if (this.UseCompression)
						Utils.SetCompressFilter ();
				}

				// set cache headers
				if (CacheManager.IsEnabled && !this.NoCache)
				{
					if (this.CacheExpire.HasValue)
						this.Context.Response.Cache.SetExpires (this.CacheExpire.Value);

					this.Context.Response.Cache.SetCacheability (this.Cachability);
					this.Context.Response.Cache.SetValidUntilExpires (this.CacheValidUntilExpires);

					if (this.LastModified.HasValue)
						this.Context.Response.Cache.SetLastModified (this.LastModified.Value);

					this.Context.Response.Cache.SetOmitVaryStar (true);
					this.Context.Response.Cache.SetVaryByCustom (Utils.ConvertToString (this.CacheVaryByCustom, ";"));
					foreach (var p in this.CacheVaryByParams)
						this.Context.Response.Cache.VaryByParams[p] = true;

					if (this.LastModified.HasValue)
						this.Context.Response.Cache.SetETag (this.GetETag (this.LastModified.Value));
				}
			}
			catch (Exception ex)
			{
				this.Set500 ();
				this.HandleError (ex);
			}
		}

		#region Methods to be overriden

		/// <summary>
		/// Change handler default configuration options if neccessary.
		/// </summary>
		protected virtual void Initialize ()
		{
			this.NoCache = false;
			this.UseCompression = true;

			this.CacheExpire = DateTime.Today.AddDays (1).AddSeconds (-1);
			this.Cachability = HttpCacheability.ServerAndPrivate;
			this.CacheValidUntilExpires = false;

			this.CacheVaryByCustom = new List<string> ();
			this.CacheVaryByCustom.Add ("ClearCache");

			this.CacheVaryByParams = new List<string> ();
		}

		/// <summary>
		/// Process handler request. Returns true if the current request is successfully proceed or false if content is not modified.
		/// </summary>
		/// <param name="lastModified">Date and time of the last modification (if sended from client).</param>
		/// <param name="etag">ETag from client.</param>
		/// <returns>True if the current request is successfully proceed or false if content is not modified.</returns>
		protected abstract bool Process (DateTime? lastModified, string etag);

		/// <summary>
		/// Handles handler processing error.
		/// </summary>
		/// <param name="ex">Exception that was thrown.</param>
		protected virtual void HandleError (Exception ex)
		{
#if DEBUG
			this.Context.Response.Write (ex.ToString ());
#else
			this.Context.Response.Write (ex.Message);
#endif
		}

		#endregion
	}
}
