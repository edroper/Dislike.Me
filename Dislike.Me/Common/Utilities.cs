using System;
using System.Net;

namespace Dislike.Me.Common
{
    public static class Utilities
    {
        public static string GetJsonFromWeb(string url)
        {
            string requestResult;

            using (var w = new WebClient())
            {
                try
                {
                    requestResult = w.DownloadString(url);
                }
                catch (Exception) { return ""; }
            }

            return requestResult;
        }
    }
}