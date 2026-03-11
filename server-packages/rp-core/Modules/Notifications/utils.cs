using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
using System.Security.Policy;
using System.Xml.Linq;
using RPCore.Database;


namespace RPCore.Notifications
{
    class Utils
    {
        public static void sendNotification(Player player, string text, string iconpic)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "showNotification", text, iconpic);
        }

        public static void AdminLog(string text, string username)
        {
            Discordbot.Post("https://ptb.discord.com/api/webhooks/1352652789318418482/Ip27-KFnBwd8NbY612iXc6K7Su9iDarYFg2VJ8I777yoRG7mWx7wB4_-aPrdFNA7pRby", new System.Collections.Specialized.NameValueCollection()
           {
               {
                   "username",
                   username
               },
               {
                   "content",
                   text
               }
           });
        }
    }
}
