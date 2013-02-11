using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Website.Code
{
	/// <summary>
	/// <para>Cache manager class that will handle all common cache routines.</para>
	/// <para><b>Author:</b> Logutov Michael<br />
	/// <b>Creation date:</b> 05 august 2009</para>
	/// </summary>
	public class CacheManager
	{
		#region Accessors

		#region Static: event ClearCache

		/// <summary>
		/// Event that will be fired when cache needs to be cleared.
		/// </summary>
		public static event EventHandler ClearCache;

		#endregion

		#region Static: IsEnabled

		/// <summary>
		/// <see cref="IsEnabled"/> internal field.
		/// </summary>
		private static bool _IsEnabled = true;

		/// <summary>
		/// (GET, SET) True if cache manager enabled. Set in application start only - not thread safe! Default value is true.
		/// </summary>
		public static bool IsEnabled
		{
			get
			{
				return CacheManager._IsEnabled;
			}
			set
			{
				CacheManager._IsEnabled = value;
			}
		}

		#endregion

		#endregion

		#region Cache items methods

		/// <summary>
		/// Gets items from cache by specified <paramref name="key"/>.
		/// </summary>
		/// <typeparam name="T">Type of the object.</typeparam>
		/// <param name="key">Key name.</param>
		/// <returns>Object instance or null.</returns>
		public static T GetItem<T> (string key)
			where T : class
		{
			if (!CacheManager.IsEnabled)
				return null;

			return HttpContext.Current.Cache[key] as T;
		}

		/// <summary>
		/// Gets items from cache by specified <paramref name="key"/>.
		/// </summary>
		/// <typeparam name="T">Type of the object.</typeparam>
		/// <param name="key">Key name.</param>
		/// <param name="defaultValue">Default value if item does not exists.</param>
		/// <returns>Object instance or null.</returns>
		public static T GetItem<T> (string key, T defaultValue)
			where T : struct
		{
			if (!CacheManager.IsEnabled)
				return defaultValue;

			var v = HttpContext.Current.Cache[key];
			if (v == null)
				return defaultValue;

			return (T) v;
		}

		/// <summary>
		/// Adds object to the cache setting expiration when current day ends.
		/// </summary>
		/// <param name="key">Key name.</param>
		/// <param name="item">Object.</param>
		public static void AddItemForTooday (string key, object item)
		{
			CacheManager.AddItem (key, item, null, DateTime.Today.AddDays (1).AddSeconds (-1), Cache.NoSlidingExpiration,
				CacheItemPriority.Normal, null);
		}

		/// <summary>
		/// Adds object to the cache.
		/// </summary>
		/// <param name="key">Key name.</param>
		/// <param name="item">Object.</param>
		/// <param name="minutes">Time for this object to be saved in cache.</param>
		public static void AddItem (string key, object item, int minutes)
		{
			CacheManager.AddItem (key, item, minutes, CacheItemPriority.Default);
		}

		/// <summary>
		/// Adds object to the cache (with sliding expiration).
		/// </summary>
		/// <param name="key">Key name.</param>
		/// <param name="item">Object.</param>
		/// <param name="minutes">Time for this object to be saved in cache after the last retrieving.</param>
		public static void AddItemSliding (string key, object item, int minutes)
		{
			CacheManager.AddItem (key, item, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes (minutes), CacheItemPriority.Default, null);
		}

		/// <summary>
		/// Adds object to the cache.
		/// </summary>
		/// <param name="key">Key name.</param>
		/// <param name="item">Object.</param>
		/// <param name="minutes">Time for this object to be saved in cache.</param>
		/// <param name="priority">Priority.</param>
		public static void AddItem (string key, object item, int minutes, CacheItemPriority priority)
		{
			CacheManager.AddItem (key, item, null, DateTime.Now.AddMinutes (minutes), Cache.NoSlidingExpiration, priority, null);
		}

		/// <summary>
		/// Adds object to the cache.
		/// </summary>
		/// <param name="key">Key name.</param>
		/// <param name="item">Object.</param>
		/// <param name="dep">Cache dependency object instance.</param>
		/// <param name="absoluteExpiration">Absolute expiration date.</param>
		/// <param name="slidingExpiration">Sliding expiration period.</param>
		/// <param name="priority">Priority.</param>
		/// <param name="removeCallback">Callback on deletion object from the cache.</param>
		public static void AddItem (string key, object item, CacheDependency dep, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback removeCallback)
		{
			HttpContext.Current.Cache.Remove (key);

			if (!CacheManager.IsEnabled)
				return;

			if (absoluteExpiration.Ticks == 0 && slidingExpiration.Ticks == 0)
				return;

			if (item != null)
				HttpContext.Current.Cache.Add (key, item, dep, absoluteExpiration, slidingExpiration, priority, removeCallback);
		}

		/// <summary>
		/// Fires <see cref="ClearCache"/> event and clears cache.
		/// </summary>
		public static void Clear ()
		{
			if (CacheManager.ClearCache != null)
				CacheManager.ClearCache (null, null);

			var cache_items_enumerator = HttpContext.Current.Cache.GetEnumerator ();
			var remove_keys = new List<string> ();
			while (cache_items_enumerator.MoveNext ())
				remove_keys.Add ((string) cache_items_enumerator.Key);

			foreach (var key in remove_keys)
				HttpContext.Current.Cache.Remove (key);
		}

		/// <summary>
		/// Removes item with specified key from the cache.
		/// </summary>
		/// <param name="key">Item key.</param>
		public static void Remove (string key)
		{
			HttpContext.Current.Cache.Remove (key);
		}

		/// <summary>
		/// Removes all items from cache which have their key started with specified <paramref name="prefix"/>.
		/// </summary>
		/// <param name="prefix">Key name prefix.</param>
		public static void RemoveByKeyPrefix (string prefix)
		{
			var cache_items_enumerator = HttpContext.Current.Cache.GetEnumerator ();
			var remove_keys = new List<string> ();
			while (cache_items_enumerator.MoveNext ())
			{
				var key = (string) cache_items_enumerator.Key;
				if (key.StartsWith (prefix))
					remove_keys.Add (key);
			}

			foreach (var key in remove_keys)
				HttpContext.Current.Cache.Remove (key);
		}

		#endregion

		#region Custom string cache items

		/// <summary>
		/// Sets the value for the custom string cache item.
		/// </summary>
		/// <param name="name">Item name.</param>
		/// <param name="value">New value.</param>
		public static void SetVaryByCustomStringValue (string name, string value)
		{
			if (string.IsNullOrEmpty (value))
				HttpContext.Current.Application.Remove (name);
			else
				HttpContext.Current.Application["VaryByCustomString." + name] = value;
		}

		/// <summary>
		/// Gets the value for the custom string cache item.
		/// </summary>
		/// <param name="name">Item name.</param>
		/// <returns>Item value.</returns>
		public static string GetVaryByCustomStringValue (string name)
		{
			var res = HttpContext.Current.Application["VaryByCustomString." + name];
			return res != null ? res.ToString () : null;
		}

		/// <summary>
		/// Increase number for the custom string cache item value.
		/// </summary>
		/// <param name="name">Item name.</param>
		public static void IncreaseVaryByCustomStringValue (string name)
		{
			HttpContext.Current.Application["VaryByCustomString." + name] = Utils.ConvertString (CacheManager.GetVaryByCustomStringValue (name), -1) + 1;
		}

		#endregion

		#region Utilities

		/// <summary>
		/// Gets <see cref="TimeSpan"/> from settings["CacheManager." + <paramref name="key"/>].
		/// </summary>
		/// <param name="key">Partial key name (without "CacheManager." prefix).</param>
		/// <returns><see cref="TimeSpan"/> value.</returns>
		public static TimeSpan GetConfigurationTimeSpan (string key)
		{
			TimeSpan res;
			if (!TimeSpan.TryParse (ConfigurationManager.AppSettings["CacheManager." + key], out res))
				res = new TimeSpan ();

			return res;
		}

		/// <summary>
		/// Gets <see cref="DateTime.Now"/> + app settings["CacheManager." + <paramref name="key"/>] minutes.
		/// </summary>
		/// <param name="key">Partial key name (without "CacheManager." prefix).</param>
		/// <returns>Minutes value.</returns>
		public static int GetConfigurationMinutes (string key)
		{
			try
			{
				return int.Parse (ConfigurationManager.AppSettings["CacheManager." + key], NumberStyles.Integer, CultureInfo.InvariantCulture);
			}
			catch
			{
				throw new InvalidOperationException ("\"CacheManager." + key + "\" app config settings contains no or invalid value.");
			}
		}

		/// <summary>
		/// Gets <see cref="Boolean"/> + app settings["CacheManager." + <paramref name="key"/>].
		/// </summary>
		/// <param name="key">Partial key name (without "CacheManager." prefix).</param>
		/// <returns><see cref="Boolean"/> value.</returns>
		public static bool GetConfigurationBoolean (string key)
		{
			return string.Compare (ConfigurationManager.AppSettings["CacheManager." + key], "true", StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		#endregion
	}
}
