using System;
using System.Globalization;
using System.Linq;
using Program.Properties;

namespace Program
{
	class Program
	{
		public static void Log(string format, params object[] args)
		{
			Console.Write ("[{0}] {1}\n", DateTime.Now.ToString (Settings.Default.DateTimeFormat, CultureInfo.InvariantCulture), string.Format (CultureInfo.InvariantCulture, format, args));
		}

		static void Main(string[] args)
		{
			try
			{
				var app = new App ();
				app.Run (args);
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Program.Log ("{0}", ex);
				Console.ResetColor ();
			}
		}
	}
}
