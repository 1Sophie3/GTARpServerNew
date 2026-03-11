//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Linq;
//using GTANetworkAPI;
//using gamemode.Datenbank;
//using Newtonsoft.Json;

//namespace gamemode.Commands
//{
//    class AdminPlayerManagement : Script
//    {
//        // Beispielbefehl: Liste aller Spieler (inklusive Account-ID und AdminRank)
//        [Command("listplayers", "/listplayers zeigt alle Spieler online inkl. AdminRank")]
//        public void ListPlayersCommand(Player admin)
//        {
//            Accounts account = admin.GetData<Accounts>(Accounts.Account_Key);
//            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
//            {
//                admin.SendChatMessage("~r~Keine Berechtigung!");
//                return;
//            }

//            StringBuilder sb = new StringBuilder();
//            foreach (Player p in NAPI.Pools.GetAllPlayers())
//            {
//                Accounts a = p.GetData<Accounts>(Accounts.Account_Key);
//                int adminRank = (a != null) ? a.Adminlevel : 0;
//                sb.AppendLine($"ID: {p.Id} | Name: {p.Name} | AdminRank: {adminRank}");
//            }
//            admin.SendChatMessage(sb.ToString());
//        }

//        // Remote-Event: Sende alle Online-Spieler als JSON (inklusive Account-ID)
//        [RemoteEvent("admin_requestPlayerList")]
//        public void RequestPlayerList(Player admin)
//        {
//            Accounts account = admin.GetData<Accounts>(Accounts.Account_Key);
//            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
//            {
//                admin.SendChatMessage("~r~Keine Berechtigung!");
//                return;
//            }

//            List<object> playerList = new List<object>();
//            Player[] players = NAPI.Pools.GetAllPlayers().ToArray();
//            NAPI.Util.ConsoleOutput("RequestPlayerList invoked. Total players: " + players.Length);

//            foreach (Player p in players)
//            {
//                Accounts a = p.GetData<Accounts>(Accounts.Account_Key);
//                int adminRank = (a != null) ? a.Adminlevel : 0;
//                int accountId = (a != null) ? a.ID : -1;
//                playerList.Add(new { id = p.Id, name = p.Name, adminRank = adminRank, accountId = accountId });
//            }

//            string json = JsonConvert.SerializeObject(playerList);
//            NAPI.Util.ConsoleOutput("PlayerList JSON: " + json);
//            admin.TriggerEvent("admin_showPlayerList", json);
//        }

//        // Teleportiere einen Spieler (Account-ID) zu dir
//        [RemoteEvent("admin_tpToMe")]
//        public void TeleportTargetToAdmin(Player admin, int targetAccountId)
//        {
//            Accounts account = admin.GetData<Accounts>(Accounts.Account_Key);
//            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
//            {
//                admin.SendChatMessage("~r~Keine Berechtigung!");
//                return;
//            }
//            Player target = FindPlayerByAccountId(targetAccountId);
//            if (target == null)
//            {
//                admin.SendChatMessage("~r~Spieler nicht gefunden!");
//                return;
//            }
//            if (target == admin)
//            {
//                admin.SendChatMessage("~r~Du kannst dich nicht zu dir selbst teleportieren!");
//                return;
//            }
//            target.Position = admin.Position;
//            admin.SendChatMessage($"~g~Spieler {target.Name} wurde zu dir teleportiert.");
//            target.SendChatMessage($"~y~Du wurdest zu {admin.Name} teleportiert.");
//        }

//        // Teleportiere dich (Admin) zu einem Spieler (Account-ID)
//        [RemoteEvent("admin_tpMeTo")]
//        public void TeleportAdminToTarget(Player admin, int targetAccountId)
//        {
//            Accounts account = admin.GetData<Accounts>(Accounts.Account_Key);
//            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
//            {
//                admin.SendChatMessage("~r~Keine Berechtigung!");
//                return;
//            }
//            Player target = FindPlayerByAccountId(targetAccountId);
//            if (target == null)
//            {
//                admin.SendChatMessage("~r~Spieler nicht gefunden!");
//                return;
//            }
//            if (target == admin)
//            {
//                admin.SendChatMessage("~r~Du kannst dich nicht zu dir selbst teleportieren!");
//                return;
//            }
//            admin.Position = target.Position;
//            admin.SendChatMessage($"~g~Du wurdest zu {target.Name} teleportiert.");
//        }

//        // ERSETZT: Heile einen Spieler (Account-ID)
//        [RemoteEvent("admin_healTarget")]
//        public void HealTarget(Player admin, int targetAccountId)
//        {
//            Accounts account = admin.GetData<Accounts>(Accounts.Account_Key);
//            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
//            {
//                admin.SendChatMessage("~r~Keine Berechtigung!");
//                return;
//            }
//            Player target = FindPlayerByAccountId(targetAccountId);
//            if (target == null)
//            {
//                admin.SendChatMessage("~r~Spieler nicht gefunden!");
//                return;
//            }
//            if (target == admin)
//            {
//                admin.SendChatMessage("~r~Nutze /heal, um dich selbst zu heilen!");
//                return;
//            }

//            // Spieler heilen
//            target.Health = 100;
//            target.Armor = 100;

//            // WICHTIG: Auch hier den Status zurücksetzen und speichern, um Bugs zu vermeiden!
//            Datenbank.Datenbank.ResetPlayerDeathStatus(target);
//            Datenbank.Datenbank.SavePlayerStatus(target);

//            target.SendChatMessage("~g~Du wurdest von einem Admin geheilt.");
//            admin.SendChatMessage($"~g~Spieler {target.Name} wurde geheilt.");
//        }

//        // ERSETZT: Respawne einen Spieler (Account-ID)
//        [RemoteEvent("admin_respawnTarget")]
//        public void RespawnTarget(Player admin, int targetAccountId)
//        {
//            Accounts account = admin.GetData<Accounts>(Accounts.Account_Key);
//            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
//            {
//                admin.SendChatMessage("~r~Keine Berechtigung!");
//                return;
//            }
//            Player target = FindPlayerByAccountId(targetAccountId);
//            if (target == null)
//            {
//                admin.SendChatMessage("~r~Spieler nicht gefunden!");
//                return;
//            }
//            if (target == admin)
//            {
//                admin.SendChatMessage("~r~Nutze /respawn, um dich selbst zu respawnen!");
//                return;
//            }
//            if (target.Health > 0)
//            {
//                admin.SendChatMessage("~r~Spieler ist bereits am Leben!");
//                return;
//            }

//            // Rufe die zentrale Methode für einen Admin-Revive auf (vor Ort, 100 HP, 100 Armor)
//            Datenbank.Datenbank.RevivePlayer(target, null, 100, 100);

//            target.SendChatMessage("~g~Du wurdest von einem Admin wiederbelebt!");
//            admin.SendChatMessage($"~g~Spieler {target.Name} wurde wiederbelebt.");
//        }

//        // Kicke einen Spieler (Account-ID) – zusätzlich wird das CEF-Fenster geschlossen
//        [RemoteEvent("admin_kickTarget")]
//        public void KickTarget(Player admin, int targetAccountId, string reason)
//        {
//            Accounts account = admin.GetData<Accounts>(Accounts.Account_Key);
//            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.GameMaster))
//            {
//                admin.SendChatMessage("~r~Keine Berechtigung!");
//                return;
//            }
//            Player target = FindPlayerByAccountId(targetAccountId);
//            if (target == null)
//            {
//                admin.SendChatMessage("~r~Spieler nicht gefunden!");
//                return;
//            }
//            if (target == admin)
//            {
//                admin.SendChatMessage("~r~Du kannst dich nicht selbst kicken!");
//                return;
//            }
//            target.Kick($"Gekickt durch Admin. Grund: {reason}");
//            admin.SendChatMessage($"~g~Spieler {target.Name} wurde gekickt. Grund: {reason}");
//            // Schließe das CEF-Fenster, falls vorhanden
//            admin.TriggerEvent("admin_closeCEF");
//        }

//        // Banne einen Spieler (Account-ID) – auch hier wird das CEF-Fenster nach Übermittlung geschlossen
//        [RemoteEvent("admin_banTarget")]
//        public void BanTarget(Player admin, int targetAccountId, string reason)
//        {
//            Accounts account = admin.GetData<Accounts>(Accounts.Account_Key);
//            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.GameMaster))
//            {
//                admin.SendChatMessage("~r~Keine Berechtigung!");
//                return;
//            }
//            Player target = FindPlayerByAccountId(targetAccountId);
//            if (target == null)
//            {
//                admin.SendChatMessage("~r~Spieler nicht gefunden!");
//                return;
//            }
//            if (target == admin)
//            {
//                admin.SendChatMessage("~r~Du kannst dich nicht selbst bannen!");
//                return;
//            }
//            Datenbank.Datenbank.SpielerBannen(targetAccountId, reason);
//            target.Kick($"Gebannt durch Admin. Grund: {reason}");
//            admin.SendChatMessage($"~g~Spieler {target.Name} wurde gebannt. Grund: {reason}");
//            // Schließe das CEF-Fenster
//            admin.TriggerEvent("admin_closeCEF");
//        }

//        // Hilfsmethode: Suche einen Spieler anhand der Account-ID (gespeichert in Accounts.ID)
//        private Player FindPlayerByAccountId(int accountId)
//        {
//            foreach (Player p in NAPI.Pools.GetAllPlayers())
//            {
//                Accounts a = p.GetData<Accounts>(Accounts.Account_Key);
//                if (a != null && a.ID == accountId)
//                {
//                    return p;
//                }
//            }
//            return null;
//        }

//    }
//}