//using GTANetworkAPI;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace gamemode.Commands
//{

//    public class GodModeScript : Script
//    {
//        public GodModeScript()
//        {
//            // Hier könnten weitere Initialisierungen hinzugefügt werden, falls benötigt.
//        }

//        [Command("godmode")]
//        public void ToggleGodMode(Player player)
//        {
//            bool godModeEnabled = player.HasSharedData("GodMode") && player.GetSharedData<bool>("GodMode");
//            godModeEnabled = !godModeEnabled;

//            player.SetSharedData("GodMode", godModeEnabled);

//            if (godModeEnabled)
//            {
//                player.SendChatMessage("Godmode ist aktiviert.");
//            }
//            else
//            {
//                player.SendChatMessage("Godmode ist deaktiviert.");
//            }
//        }

//        [ServerEvent(Event.Update)]
//        public void OnServerUpdate()
//        {
//            foreach (Player player in NAPI.Pools.GetAllPlayers())
//            {
//                if (player.HasSharedData("GodMode") && player.GetSharedData<bool>("GodMode"))
//                {
//                    // Setzt Gesundheit und Rüstung zurück
//                    if (player.Health < 100)
//                    {
//                        player.Health = 100;
//                    }

//                    if (player.Armor < 100)
//                    {
//                        player.Armor = 100;
//                    }
//                }
//            }
//        }
//    }
//}
