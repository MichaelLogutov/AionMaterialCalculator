using System;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace Website.Code
{
	#region class AjaxResult

	/// <summary>
	/// <para>Represent typical ajax webservice call result.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 14 november 2008</para>
	/// </summary>
	public class AjaxResult
	{
		public static AjaxResult WrongParametersOrHackAttempt = new AjaxResult (1, "Переданы неверные параметры или зарегистрирована попытка взлома. Повторите попытку позже");
		public static AjaxResult Success = new AjaxResult (0, null);
		public static AjaxResult Error = new AjaxResult (1, null);

		/// <summary>
		/// Result code.
		/// </summary>
		public int Code { get; set; }

		/// <summary>
		/// Result data.
		/// </summary>
		public string Data { get; set; }

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="code"><see cref="Code"/>.</param>
		/// <param name="data"><see cref="Data"/>.</param>
		public AjaxResult (int code, string data)
		{
			this.Code = code;
			this.Data = data;
		}
	}

	#endregion

	/// <summary>
	/// <para>Base class for all webservices on the site.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 04 june 2009</para>
	/// </summary>
	public class BaseWebService : WebService
	{
		/// <summary>
		/// Handles start of webservice's call.
		/// </summary>
		protected void HandleCall ()
		{
		}

		/// <summary>
		/// Checks if current user is authenticated and throw an exception if not (exception will not be reported to administrator).
		/// </summary>
		protected void CheckAuthenticated ()
		{
			this.CheckAuthenticated ("Вы не авторизированны");
		}

		/// <summary>
		/// Checks if current user is authenticated and throw an exception if not (exception will not be reported to administrator).
		/// </summary>
		/// <param name="message">Exception message.</param>
		protected void CheckAuthenticated (string message)
		{
			if (!HttpContext.Current.User.Identity.IsAuthenticated)
				throw new InvalidOperationExceptionNoReport (message);
		}

		/// <summary>
		/// Handles webservice exception.
		/// </summary>
		/// <param name="ex">Exception instance.</param>
		/// <returns>Result of the operation.</returns>
		protected AjaxResult HandleException (Exception ex)
		{
			if (!(ex is InvalidOperationExceptionNoReport))
				Utils.ReportError (ex);

			return new AjaxResult (1, ex.Message);
		}
	}
}
