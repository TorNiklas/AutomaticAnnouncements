//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.ComponentModel;
//using System.Drawing;
////using System.Windows.Forms;
//using System.Net;
//using Newtonsoft.Json.Linq;
//using System.Runtime.InteropServices;
//using System.Security;
//using System.Security.Permissions;

//namespace AutomaticAnnouncements
//{
//	class Browser
//	{
//		public static dynamic GetPatreonPosts(string url)
//		{
//			try
//			{
//				//Patreon makes things hard
//				//Step 1: Get cookies
//				var cookies = FullWebBrowserCookie.GetCookieInternal(new Uri("https://www.patreon.com"), false);

//				//Step 2: Use gotten cookes to download the JSON string for the posts
//				WebClient wc = new WebClient();
//				wc.Headers.Add("Cookie: " + cookies);
//				wc.Headers.Add("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
//				wc.Headers.Add("Accept-Language: en-US,en;q=0.5");
//				wc.Headers.Add("Cache-Control: no-cache");
//				wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:99.0) Gecko/20100101 Firefox/99.0");

//				//Step 3: Convert JSON string to JSON object and return
//				string jsonString = wc.DownloadString(url);
//				dynamic content = JObject.Parse(jsonString);
//				return content;
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine("getCookies failed: " + ex.Message);
//				return null;
//			}
//		}
//	}

//	internal sealed class NativeMethods
//	{
//		#region enums

//		public enum ErrorFlags
//		{
//			ERROR_INSUFFICIENT_BUFFER = 122,
//			ERROR_INVALID_PARAMETER = 87,
//			ERROR_NO_MORE_ITEMS = 259
//		}

//		public enum InternetFlags
//		{
//			INTERNET_COOKIE_HTTPONLY = 8192, //Requires IE 8 or higher   
//			INTERNET_COOKIE_THIRD_PARTY = 131072,
//			INTERNET_FLAG_RESTRICTED_ZONE = 16
//		}

//		#endregion

//		#region DLL Imports

//		[SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("wininet.dll", EntryPoint = "InternetGetCookieExW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
//		internal static extern bool InternetGetCookieEx([In] string Url, [In] string cookieName, [Out] StringBuilder cookieData, [In, Out] ref uint pchCookieData, uint flags, IntPtr reserved);

//		#endregion
//	}


//	/// <SUMMARY></SUMMARY>   
//	/// WebBrowserCookie?   
//	/// webBrowser1.Document.CookieHttpOnlyCookie   
//	///    
//	public class FullWebBrowserCookie : WebBrowser
//	{

//		[SecurityCritical]
//		public static string GetCookieInternal(Uri uri, bool throwIfNoCookie)
//		{
//			uint pchCookieData = 0;
//			string url = UriToString(uri);
//			uint flag = (uint)NativeMethods.InternetFlags.INTERNET_COOKIE_HTTPONLY;

//			//Gets the size of the string builder   
//			if (NativeMethods.InternetGetCookieEx(url, null, null, ref pchCookieData, flag, IntPtr.Zero))
//			{
//				pchCookieData++;
//				StringBuilder cookieData = new StringBuilder((int)pchCookieData);

//				//Read the cookie   
//				if (NativeMethods.InternetGetCookieEx(url, null, cookieData, ref pchCookieData, flag, IntPtr.Zero))
//				{
//					DemandWebPermission(uri);
//					return cookieData.ToString();
//				}
//			}

//			int lastErrorCode = Marshal.GetLastWin32Error();

//			if (throwIfNoCookie || (lastErrorCode != (int)NativeMethods.ErrorFlags.ERROR_NO_MORE_ITEMS))
//			{
//				throw new Win32Exception(lastErrorCode);
//			}

//			return null;
//		}

//		private static void DemandWebPermission(Uri uri)
//		{
//			string uriString = UriToString(uri);

//			if (uri.IsFile)
//			{
//				string localPath = uri.LocalPath;
//				new FileIOPermission(FileIOPermissionAccess.Read, localPath).Demand();
//			}
//			else
//			{
//				new WebPermission(NetworkAccess.Connect, uriString).Demand();
//			}
//		}

//		private static string UriToString(Uri uri)
//		{
//			if (uri == null)
//			{
//				throw new ArgumentNullException("uri");
//			}

//			UriComponents components = (uri.IsAbsoluteUri ? UriComponents.AbsoluteUri : UriComponents.SerializationInfoString);
//			return new StringBuilder(uri.GetComponents(components, UriFormat.SafeUnescaped), 2083).ToString();
//		}
//	}
//}

