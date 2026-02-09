using GTANetworkInternals;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GTANetworkAPI;

namespace RPCore.Datenbank
{
    class DbSettings
    {
        public static DbSettings _Settings;
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }

        public static bool LoadServerSettings()
        {
            string directory = "./serverdata/DbSettings.json";
            if (File.Exists(directory))
            {
                string settings = File.ReadAllText(directory);
                _Settings = NAPI.Util.FromJson<DbSettings>(settings);
                NAPI.Util.ConsoleOutput("DB Settings geladen!");
                return true;
            }
            else
            {
                NAPI.Util.ConsoleOutput("DB Settings konnten nicht geladen werden!");
                NAPI.Task.Run(() =>
                {
                    Environment.Exit(0);
                }, delayTime: 5000);
                return false;

            }
         }
     }
  }

