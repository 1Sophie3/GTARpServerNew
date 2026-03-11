using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using System.Timers;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;
using RPCore.Notifications;
using RPCore.Database;
using Newtonsoft.Json;



namespace RPCore.Commands
{
    class Commands : Script
    {
        private static readonly Vector3 RespawnPosition = new Vector3(275.0f, -579.0f, 43.0f); // Beispiel-Koordinaten

        #region Helfermethoden
        // =================================================================================================

        // Findet einen Spieler anhand seines Namens oder seiner Account-ID.
        private Player FindPlayerByIdOrName(string identifier)
        {
            if (int.TryParse(identifier, out int id))
            {
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).ID == id)
                    {
                        return p;
                    }
                }
            }
            return NAPI.Player.GetPlayerFromName(identifier);
        }

        #endregion

        #region Spieler-Befehle
        // =================================================================================================

        // Legt den Sicherheitsgurt an oder ab.
        // In der Klasse 'Commands' in commands.cs

        [Command("seatbelt")]
        public void SeatbeltCommand(Player player)
        {
            var account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account != null && player.IsInVehicle)
            {
                // Kehrt den aktuellen Zustand um (true -> false, false -> true)
                account.Seatbelt = !account.Seatbelt;

                if (account.Seatbelt)
                {
                    player.SendNotification("~g~Sicherheitsgurt angelegt!");
                }
                else
                {
                    player.SendNotification("~y~Sicherheitsgurt abgelegt!");
                }
                // Sendet den neuen Status an den Client
                player.TriggerEvent("updateSeatbelt", account.Seatbelt);
            }
        }

        // Zeigt die aktuellen Koordinaten des Spielers an.
        [Command("pos")]
        public void ShowCoords(Player player)
        {
            var playerPosition = player.Position;
            var playerRotation = player.Rotation;

            player.SendChatMessage($"Deine Position: X: {playerPosition.X}, Y: {playerPosition.Y}, Z: {playerPosition.Z}");
            player.SendChatMessage($"Deine Rotation: X: {playerRotation.X}, Y: {playerRotation.Y}, Z: {playerRotation.Z}");
        }

        // Seilt den Spieler aus einem Helikopter ab.
        [Command("abseilen", "/abseilen, um dich aus einem Helikopter abzuseilen")]
        public void CmdAbseilen(Player player)
        {
            if (!player.IsInVehicle)
            {
                player.SendNotification("~r~Du bist in keinem Fahrzeug!");
                return;
            }

            Vehicle vehicle = player.Vehicle;

            if (vehicle.Model != NAPI.Util.GetHashKey("polmav"))
            {
                player.SendChatMessage("~r~Abseilen ist nur im Polizei-Maverick möglich!");
                return;
            }

            if (player.VehicleSeat == 0 || player.VehicleSeat == 1)
            {
                player.SendChatMessage("~r~Fahrer oder Beifahrer können nicht abseilen!");
                return;
            }

            player.TriggerEvent("client_abseilen");
        }

        // Notfall-Respawn für Spieler.
        [Command("respawn", "Benutze diesen Befehl nur im Notfall, um dich wiederzubeleben.")]
        public void RespawnPlayerCommand(Player player)
        {
            if (player.Health <= 0)
            {
                Datenbank.Datenbank.RevivePlayer(player, RespawnPosition, 65, 0);
                Utils.sendNotification(player, "Du wurdest im Krankenhaus wiederbelebt.", "fas fa-hospital");
            }
            else
            {
                player.SendNotification("~r~Du kannst diesen Befehl nur verwenden, wenn du tot bist!");
            }
        }

        #endregion

        #region Admin- & Debug-Befehle
        // =================================================================================================

        // Speichert die aktuellen Admin-Koordinaten in der Konsole und einer Log-Datei.
        [Command("apos")]
        public void showACoords(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendNotification("~r~Du hast keine Berechtigung, diesen Befehl auszuführen!");
                return;
            }

            Vector3 pos = player.Position;
            Vector3 rot = player.Rotation;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string playerName = player.Name;

            string output = string.Format("[{0}] Spieler: {1} | Position: X: {2:F2}, Y: {3:F2}, Z: {4:F2} | Rotation: X: {5:F2}, Y: {6:F2}, Z: {7:F2}",
                                          timestamp, playerName, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z);

            NAPI.Util.ConsoleOutput(output);

            try
            {
                string filePath = @"C:\RAGEMP\server-files\logs\pos_log.txt";
                System.IO.File.AppendAllText(filePath, output + Environment.NewLine);
                player.SendChatMessage("Position in der Serverkonsole ausgegeben und in pos_log.txt gespeichert.");
            }
            catch (Exception ex)
            {
                player.SendChatMessage("Fehler beim Schreiben der Datei: " + ex.Message);
            }
        }

        // Zeigt die aktuelle Dimension an.
        [Command("cdim")]
        public void CheckDimensionCommand(Player player)
        {
            uint dimension = player.Dimension;
            player.SendNotification($"Du befindest dich in Dimension {dimension}.");
        }

        // Ändert die Dimension des Admins.
        [Command("dim")]
        public void ChangeDimension(Player player, int dimension)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.Admin))
            {
                player.SendNotification("~r~Du hast keine Berechtigung, diesen Befehl auszuführen!");
                return;
            }

            if (dimension < 0)
            {
                player.SendChatMessage("Dimension muss eine positive Zahl sein.");
                return;
            }

            player.Dimension = (uint)dimension;
            player.SendNotification($"Du hast die Dimension zu {dimension} gewechselt.");
        }

        #endregion

        #region Kern-Systeme
        // =================================================================================================

        // Öffnet den Charakter-Editor.
        [Command("charcreator", "")]
        public void CMD_Charcreator(Player player)
        {
            player.TriggerEvent("charcreator-show");
        }

        #endregion

        #region Ausgeklammerte Befehle (veraltet)
        // =================================================================================================
        // Die folgenden Befehle wurden durch das neue Admin-Panel (F5) ersetzt und sind hier
        // nur noch als Referenz auskommentiert.
        // =================================================================================================

        /*
        [Command("av", "/av um ein Shotaro-Motorrad zu spawnen")]
        public void cmd_aveh(Player player)
        {
            uint vehash = NAPI.Util.GetHashKey("shotaro");
            if (vehash <= 0)
            {
                player.SendChatMessage("~r~Ungültige Eingabe");
                return;
            }
            int color1 = 134;
            int color2 = 134;
            Vehicle veh = NAPI.Vehicle.CreateVehicle(vehash, player.Position, player.Heading, color1, color2);
            veh.NumberPlate = "ADMIN";
            veh.Locked = false;
            veh.EngineStatus = true;
            player.SetIntoVehicle(veh, (int)VehicleSeat.Driver);
            Utils.sendNotification(player, "Fahrzeug gespawnt: Shotaro", "fas fa-motorcycle");
        }
        
        [Command("revive", "/revive [DB-ID] - Belebt einen Spieler als Admin wieder.", GreedyArg = true)]
        public void RevivePlayerByIdCommand(Player admin, int databaseId)
        {
            if (!admin.HasData(Accounts.Account_Key))
            {
                admin.SendNotification("~r~Fehler: Account-Daten nicht geladen.");
                return;
            }
            Accounts adminAccount = admin.GetData<Accounts>(Accounts.Account_Key);
            if (!adminAccount.IstSpielerAdmin((int)Accounts.AdminRanks.GameMaster))
            {
                admin.SendNotification("~r~Du hast keine Berechtigung, diesen Befehl auszuführen!");
                return;
            }
            Player targetPlayer = null;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p.HasData(Accounts.Account_Key))
                {
                    Accounts targetAccount = p.GetData<Accounts>(Accounts.Account_Key);
                    if (targetAccount != null && targetAccount.ID == databaseId)
                    {
                        targetPlayer = p;
                        break;
                    }
                }
            }
            if (targetPlayer == null)
            {
                admin.SendChatMessage($"Kein Spieler mit der Datenbank-ID {databaseId} gefunden!");
                return;
            }
            if (targetPlayer.Health <= 0)
            {
                Datenbank.Datenbank.RevivePlayer(targetPlayer, 100, 100);
                targetPlayer.SendNotification("~g~Du wurdest von einem Admin wiederbelebt!");
                admin.SendChatMessage($"~g~Spieler {targetPlayer.Name} wurde erfolgreich wiederbelebt.");
            }
            else
            {
                admin.SendChatMessage($"~r~Spieler {targetPlayer.Name} ist bereits am Leben!");
            }
        }
        
        [Command("veh", "/veh [Fahrzeugname] [Primärfarbe] [Sekundärfarbe]")]
        public void cmd_veh(Player player, string vehname = "", string color1 = "", string color2 = "")
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.Owner))
            {
                player.SendNotification("~r~Befehl nicht gefunden!");
                return;
            }
            if (string.IsNullOrEmpty(vehname))
            {
                player.SendNotification("~r~Du musst einen Fahrzeugnamen eingeben!");
                return;
            }
            if (string.IsNullOrEmpty(color1) || string.IsNullOrEmpty(color2))
            {
                player.SendNotification("~r~Du musst zwei Farben angeben! Beispiel: /veh [Fahrzeugname] [Farbe1] [Farbe2]");
                return;
            }
            if (!int.TryParse(color1, out int parsedColor1) || parsedColor1 < 0 || parsedColor1 > 255)
            {
                player.SendNotification("~r~Du musst eine gültige Zahl (0-255) für die Primärfarbe angeben!");
                return;
            }
            if (!int.TryParse(color2, out int parsedColor2) || parsedColor2 < 0 || parsedColor2 > 255)
            {
                player.SendNotification("~r~Du musst eine gültige Zahl (0-255) für die Sekundärfarbe angeben!");
                return;
            }
            uint vehash = NAPI.Util.GetHashKey(vehname);
            if (vehash == 0)
            {
                player.SendNotification("~r~Ungültiger Fahrzeugname! Bitte gib ein korrektes Fahrzeug an.");
                return;
            }
            Vehicle veh = NAPI.Vehicle.CreateVehicle(vehash, player.Position, player.Heading, parsedColor1, parsedColor2);
            veh.NumberPlate = "X";
            veh.Locked = false;
            veh.EngineStatus = true;
            player.SetIntoVehicle(veh, (int)VehicleSeat.Driver);
            player.SendNotification($"~g~Fahrzeug {vehname} wurde erfolgreich mit den Farben {parsedColor1} und {parsedColor2} gespawnt!");
        }

        [Command("heal", "/heal um dich oder einen Spieler zu heilen")]
        public void cmd_heal(Player player, string targetIdOrName = "")
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            Player target = player;
            if (!string.IsNullOrEmpty(targetIdOrName))
            {
                if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
                {
                    player.SendNotification("~r~Du hast keine Berechtigung, andere zu heilen!");
                    return;
                }
                target = FindPlayerByIdOrName(targetIdOrName);
                if (target == null)
                {
                    player.SendNotification("~r~Spieler nicht gefunden!");
                    return;
                }
            }
            Datenbank.Datenbank.ResetPlayerDeathStatus(target);
            target.Health = 100;
            target.Armor = 100;
            Datenbank.Datenbank.SavePlayerStatus(target);
            if (target == player)
            {
                Utils.sendNotification(player, "Du wurdest vollständig geheilt.", "fas fa-heart");
            }
            else
            {
                Utils.sendNotification(player, $"Spieler {target.Name} wurde geheilt.", "fas fa-user-md");
                Utils.sendNotification(target, "Du wurdest von einem Admin geheilt.", "fas fa-heart");
            }
        }
        
        [Command("setped", "/setped [PedName]")]
        public void CMD_SetPed(Player player, string pedName = "")
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || !account.IstSpielerAdmin((int)Accounts.AdminRanks.Owner))
            {
                player.SendNotification("~r~Du hast keine Berechtigung, diesen Befehl auszuführen!");
                return;
            }
            if (string.IsNullOrEmpty(pedName))
            {
                player.SendNotification("~r~Du musst einen Ped-Namen angeben! Beispiel: /setped [PedName]");
                return;
            }
            uint pedHash = NAPI.Util.GetHashKey(pedName);
            try
            {
                NAPI.Player.SetPlayerSkin(player, pedHash);
                player.SendNotification($"~g~Du hast dich erfolgreich in das Ped '{pedName}' verwandelt!");
            }
            catch (Exception ex)
            {
                player.SendNotification($"~r~Fehler beim Wechseln des Ped-Modells: {ex.Message}");
            }
        }
        
        [Command("weapon")]
        public void GivePistol50(Player player)
        {
            player.GiveWeapon(WeaponHash.Pistol50, 250);
            player.SendNotification("~g~Pistole .50 und Munition erhalten!");
        }

        [Command("tune", "/tune - Tunt dein aktuelles Fahrzeug und speichert die Modifikationen in der Datenbank.")]
        public void cmd_tuneAll(Player player)
        {
            if (!player.IsInVehicle)
            {
                player.SendNotification("~r~Du sitzt in keinem Fahrzeug!");
                return;
            }
            Vehicle vehicle = player.Vehicle;
            for (int i = 0; i < 49; i++)
            {
                try { vehicle.SetMod(i, 3); } catch { continue; }
            }
            vehicle.SetMod(11, 3);
            vehicle.SetMod(12, 2);
            vehicle.SetMod(13, 1);
            vehicle.SetMod(15, 4);
            vehicle.SetMod(16, 4);
            Dictionary<int, int> mods = new Dictionary<int, int>();
            for (int i = 0; i < 49; i++)
            {
                try { int modValue = vehicle.GetMod(i); mods[i] = modValue; } catch { continue; }
            }
            string modsJson = JsonConvert.SerializeObject(mods);
            if (!vehicle.HasData("vehsafe_id"))
            {
                player.SendNotification("~r~Dieses Fahrzeug ist nicht im vehsafe-System registriert!");
                return;
            }
            int vehSafeId = vehicle.GetData<int>("vehsafe_id");
            VehSafeData.UpdateVehicleModifications(vehSafeId, modsJson);
            vehicle.SetData("modifications", modsJson);
            player.SendNotification("~g~Dein Fahrzeug wurde maximal getunt und die Modifikationen wurden gespeichert!");
        }
        
        [Command("noclip", "/noclip toggelt den Noclip-Modus (Admin: TestGamemaster und höher)")]
        public void ToggleNoclipCommand(Player player)
        {
            if (!player.HasData(Accounts.Account_Key))
            {
                player.SendNotification("~r~Account-Daten nicht geladen!");
                return;
            }
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (!account.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendNotification("~r~Du hast keine Berechtigung, diesen Befehl zu verwenden!");
                return;
            }
            bool noclipEnabled = false;
            if (player.HasData("noclip"))
                noclipEnabled = player.GetData<bool>("noclip");
            noclipEnabled = !noclipEnabled;
            player.SetData("noclip", noclipEnabled);
            player.TriggerEvent("toggleNoclip", noclipEnabled);
            if (noclipEnabled)
                player.SendNotification("~g~Noclip aktiviert");
            else
                player.SendNotification("~y~Noclip deaktiviert");
        }
        */

        // nur zum setzen von dealership autos

        public class DealershipAdminCommands : Script
        {
            [Command("addcar", "/addcar <Kategorie> <Preis> <Für Fraktion (true/false)>")]
            public void CMD_AddCarToCatalog(Player player, string category, int price, bool isFactionBuyable)
            {
                Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
                if (acc == null || !acc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin))
                {
                    player.SendNotification("~r~Keine Berechtigung.");
                    return;
                }
                if (player.Vehicle == null)
                {
                    player.SendNotification("~r~Du musst in einem Fahrzeug sitzen.");
                    return;
                }
                string modelName = player.Vehicle.DisplayName;

                string query = "INSERT INTO vehicle_definitions (model, displayName, category, price, isFactionBuyable) " +
                               "VALUES (@model, @displayName, @category, @price, @isFactionBuyable) " +
                               "ON DUPLICATE KEY UPDATE category=@category, price=@price, isFactionBuyable=@isFactionBuyable, displayName=@displayName;";

                DatabaseHelper.ExecuteNonQuery(query,
                    new MySqlParameter("@model", modelName),
                    new MySqlParameter("@displayName", modelName),
                    new MySqlParameter("@category", category),
                    new MySqlParameter("@price", price),
                    new MySqlParameter("@isFactionBuyable", isFactionBuyable)
                );

                player.SendNotification($"~g~Fahrzeug '{modelName}' zum Katalog der Kategorie '{category}' hinzugefügt.");
                NAPI.ClientEvent.TriggerClientEvent(player, "Dealership:ReloadData");
            }

            [Command("adddealerspot", "/adddealerspot <Kategorie> [Name des Autohauses]")]
            public void CMD_AddDealershipSpot(Player player, string category, string name = "Autohaus")
            {
                Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
                if (acc == null || !acc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin))
                {
                    player.SendNotification("~r~Keine Berechtigung.");
                    return;
                }

                Vector3 position = player.Position;
                float heading = player.Heading;

                string query = "INSERT INTO dealerships (name, category, posX, posY, posZ, rotZ) " +
                               "VALUES (@name, @category, @posX, @posY, @posZ, @rotZ);";

                DatabaseHelper.ExecuteNonQuery(query,
                    new MySqlParameter("@name", name),
                    new MySqlParameter("@category", category),
                    new MySqlParameter("@posX", position.X),
                    new MySqlParameter("@posY", position.Y),
                    new MySqlParameter("@posZ", position.Z),
                    new MySqlParameter("@rotZ", heading)
                );

                player.SendNotification($"~g~Neuer Ausstellungsort für Kategorie '{category}' an deiner Position erstellt.");
                NAPI.ClientEvent.TriggerClientEvent(player, "Dealership:ReloadData");
            }

            [Command("setcardesc", "/setcardesc <Beschreibung...>", GreedyArg = true)]
            public void CMD_SetCarDescription(Player player, string description)
            {
                Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
                if (acc == null || !acc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin))
                {
                    player.SendNotification("~r~Keine Berechtigung.");
                    return;
                }
                if (player.Vehicle == null)
                {
                    player.SendNotification("~r~Du musst in einem Fahrzeug sitzen.");
                    return;
                }
                string modelName = player.Vehicle.DisplayName;
                string query = "UPDATE vehicle_definitions SET description = @desc WHERE model = @model";
                DatabaseHelper.ExecuteNonQuery(query,
                    new MySqlParameter("@desc", description),
                    new MySqlParameter("@model", modelName)
                );
                player.SendNotification($"~g~Beschreibung für '{modelName}' erfolgreich gesetzt.");
                NAPI.ClientEvent.TriggerClientEvent(player, "Dealership:ReloadData");
            }
        }

        [Command("veh", "/veh [Fahrzeugname] [Farbe1?] [Farbe2?]", GreedyArg = false)]
        public void CMD_SpawnVehicle(Player player, string modelName, int color1 = 0, int color2 = 0)
        {
            // 1. Berechtigungs-Prüfung
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);

            // Passe das benötigte Admin-Level hier bei Bedarf an (z.B. AdminRanks.Moderator)
            if (acc == null || !acc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin))
            {
                player.SendNotification("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }

            // 2. Eingabe-Validierung
            if (string.IsNullOrWhiteSpace(modelName))
            {
                player.SendNotification("~r~Benutzung: /veh [Fahrzeugname] [Farbe1] [Farbe2]");
                return;
            }

            // 3. Fahrzeug-Modell in einen Hash umwandeln
            uint vehHash = NAPI.Util.GetHashKey(modelName);

            // Prüfen, ob der Fahrzeugname gültig ist
            if (vehHash == 0)
            {
                player.SendNotification($"~r~Ungültiger Fahrzeugname: '{modelName}'");
                return;
            }

            // Optional: Altes Fahrzeug des Admins löschen, um die Welt sauber zu halten
            player.Vehicle?.Delete();

            // 4. Fahrzeug erstellen
            Vehicle veh = NAPI.Vehicle.CreateVehicle(
                vehHash,
                player.Position, // An der aktuellen Position des Spielers
                player.Heading,  // Mit der aktuellen Ausrichtung des Spielers
                color1,
                color2
            );

            // 5. Fahrzeug konfigurieren
            veh.NumberPlate = "ADMIN";
            veh.Locked = false;
            veh.EngineStatus = true;

            // 6. Spieler in das neue Fahrzeug setzen
            NAPI.Task.Run(() => {
                if (player.Exists && veh.Exists)
                {
                    player.SetIntoVehicle(veh, (int)VehicleSeat.Driver);
                }
            }, delayTime: 200);


            // 7. Erfolgs-Nachricht
            player.SendNotification($"~g~Fahrzeug '{modelName}' wurde gespawnt.");
        }

        [Command("tune", "/tune - Tunt dein aktuelles Fahrzeug und speichert die Modifikationen in der Datenbank.")]
        public void cmd_tuneAll(Player player)
        {
            if (!player.IsInVehicle)
            {
                player.SendNotification("~r~Du sitzt in keinem Fahrzeug!");
                return;
            }
            Vehicle vehicle = player.Vehicle;
            for (int i = 0; i < 49; i++)
            {
                try { vehicle.SetMod(i, 3); } catch { continue; }
            }
            vehicle.SetMod(11, 3);
            vehicle.SetMod(12, 2);
            vehicle.SetMod(13, 1);
            vehicle.SetMod(15, 4);
            vehicle.SetMod(16, 4);
            Dictionary<int, int> mods = new Dictionary<int, int>();
            for (int i = 0; i < 49; i++)
            {
                try { int modValue = vehicle.GetMod(i); mods[i] = modValue; } catch { continue; }
            }
            string modsJson = JsonConvert.SerializeObject(mods);
            if (!vehicle.HasData("vehsafe_id"))
            {
                player.SendNotification("~r~Dieses Fahrzeug ist nicht im vehsafe-System registriert!");
                return;
            }
            int vehSafeId = vehicle.GetData<int>("vehsafe_id");
            VehSafeData.UpdateVehicleModifications(vehSafeId, modsJson);
            vehicle.SetData("modifications", modsJson);
            player.SendNotification("~g~Dein Fahrzeug wurde maximal getunt und die Modifikationen wurden gespeichert!");
        }
    }
}



#endregion