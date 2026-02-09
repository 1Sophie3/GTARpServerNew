using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace RPCore.datenbank
{
    class Discordbot
    {
        public static byte[] Post(string url, NameValueCollection pairs)
        {
            using (WebClient webClient = new WebClient())
            {
                return webClient.UploadValues(url, pairs);
            }
        }
    }
          
}
