using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace Dislike.Me.Common
{

    public static class Utilities
    {

        public static string GetJSONFromWeb(string url)
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