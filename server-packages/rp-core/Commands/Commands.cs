using GTANetworkAPI;
using RPCore.Managers;
using RPCore.Models.Permission;
using RPCore.Models.Character;
using RPCore.Models.Faction;
using RPCore.Events;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace RPCore.Commands
{
    /// <summary>
    /// Teleport Location Datenmodell
    /// </summary>
    public class TeleportLocation
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Rotation { get; set; } = 0f;
        public int RequiredPermissionLevel { get; set; } = 0;
    }

    /// <summary>
    /// Basis-Klasse für Commands mit Permission-Checks
    /// </summary>
    public abstract class BaseCommands : Script
    {
        protected static List<TeleportLocation> TeleportLocations { get; private set; } = new List<TeleportLocation>();

        /// <summary>
        /// Lädt Teleport-Locations aus JSON beim ersten Zugriff
        /// </summary>
        protected static void LoadTeleportLocations()
        {
            if (TeleportLocations.Count > 0) return; // Bereits geladen

            string filePath = Path.Combine("configs", "teleportLocations.json");
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    TeleportLocations = JsonConvert.DeserializeObject<List<TeleportLocation>>(json) ?? new List<TeleportLocation>();
                    NAPI.Util.ConsoleOutput($"[TELEPORT] {TeleportLocations.Count} Locations aus teleportLocations.json geladen.");
                }
                catch (System.Exception ex)
                {
                    NAPI.Util.ConsoleOutput($"[FEHLER] teleportLocations.json konnte nicht geladen werden: {ex.Message}");
                    TeleportLocations = new List<TeleportLocation>();
                }
            }
            else
            {
                NAPI.Util.ConsoleOutput($"[WARNUNG] teleportLocations.json nicht gefunden in: {filePath}");
                TeleportLocations = new List<TeleportLocation>();
            }
        }

        protected bool HasPermission(GTANetworkAPI.Player player, PermissionLevel requiredLevel)
        {
            var character = CharacterManager.Instance.GetPlayerCharacter(player);
            if (character == null) return false;

            var permission = AccountManager.Instance.GetPermission(character.AccountId);
            return permission.HasPermission(requiredLevel);
        }

        protected Character GetCharacter(GTANetworkAPI.Player player)
        {
            return CharacterManager.Instance.GetPlayerCharacter(player);
        }
    }

    /// <summary>
    /// Admin Commands - Nur für Staff zugänglich
    /// </summary>
    public class AdminCommands : BaseCommands
    {
        // ========================================
        // FAHRZEUG SPAWN COMMANDS
        // ========================================

        [Command("av")]
        public void CMD_AdminVehicle(GTANetworkAPI.Player player)
        {
            if (!HasPermission(player, PermissionLevel.Supporter))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            // Shotaro spawnen
            VehicleHash hash = (VehicleHash)NAPI.Util.GetHashKey("shotaro");
            Vehicle veh = NAPI.Vehicle.CreateVehicle(hash, player.Position, player.Rotation.Z, 134, 134);
            player.SetIntoVehicle(veh, -1);
            player.SendChatMessage("~g~Shotaro gespawnt!");
        }

        [Command("veh")]
        public void CMD_SpawnVehicle(GTANetworkAPI.Player player, string vehName, int color1 = 0, int color2 = 0)
        {
            if (!HasPermission(player, PermissionLevel.Administrator))
            {
                player.SendChatMessage("~r~Keine Berechtigung! (Admin benötigt)");
                return;
            }

            VehicleHash hash = (VehicleHash)NAPI.Util.GetHashKey(vehName);
            if ((uint)hash == 0)
            {
                player.SendChatMessage("~r~Ungültiger Fahrzeugname!");
                return;
            }

            Vehicle veh = NAPI.Vehicle.CreateVehicle(hash, player.Position, player.Rotation.Z, color1, color2);
            NAPI.Player.SetPlayerIntoVehicle(player, veh, -1);
            player.SendChatMessage($"~g~{vehName} gespawnt! (Farbe: {color1}/{color2})");
        }

        [Command("car")]
        public void CMD_SpawnCar(GTANetworkAPI.Player player, string vehicleName)
        {
            if (!HasPermission(player, PermissionLevel.Supporter))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            VehicleHash hash = (VehicleHash)NAPI.Util.GetHashKey(vehicleName);
            if ((uint)hash == 0)
            {
                player.SendChatMessage("~r~Ungültiger Fahrzeugname!");
                return;
            }

            Vehicle vehicle = NAPI.Vehicle.CreateVehicle(hash, player.Position, player.Rotation.Z, 0, 0);
            NAPI.Player.SetPlayerIntoVehicle(player, vehicle, -1);
            player.SendChatMessage($"~g~Fahrzeug {vehicleName} gespawnt!");
        }

        // ========================================
        // HEAL & REVIVE COMMANDS
        // ========================================

        [Command("heal")]
        public void CMD_Heal(GTANetworkAPI.Player player)
        {
            if (!HasPermission(player, PermissionLevel.Supporter))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            player.Health = 100;
            player.Armor = 100;
            player.SendChatMessage("~g~Du wurdest geheilt und eine Schutzweste wurde angelegt!");
        }

        [Command("aheal")]
        public void CMD_AdminHealTarget(GTANetworkAPI.Player player, string targetName)
        {
            if (!HasPermission(player, PermissionLevel.Moderator))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            var target = NAPI.Pools.GetAllPlayers().FirstOrDefault(p =>
                p.Name.ToLower().Contains(targetName.ToLower()));

            if (target == null)
            {
                player.SendChatMessage("~r~Spieler nicht gefunden!");
                return;
            }

            target.Health = 100;
            target.Armor = 100;

            player.SendChatMessage($"~g~{target.Name} wurde geheilt!");
            target.SendChatMessage($"~g~Du wurdest von {player.Name} geheilt!");
        }

        [Command("revive")]
        public void CMD_Revive(GTANetworkAPI.Player player, string targetName)
        {
            if (!HasPermission(player, PermissionLevel.Moderator))
            {
                player.SendChatMessage("~r~Keine Berechtigung! (Moderator benötigt)");
                return;
            }

            var target = NAPI.Pools.GetAllPlayers().FirstOrDefault(p =>
                p.Name.ToLower().Contains(targetName.ToLower()));

            if (target == null)
            {
                player.SendChatMessage("~r~Spieler nicht gefunden!");
                return;
            }

            // Prüfe ob Spieler tot ist
            if (target.Health > 0)
            {
                player.SendChatMessage("~r~Spieler ist nicht tot!");
                return;
            }

            // Breche automatischen Revive ab
            ReviveHandler.CancelReviveProcess(target);

            // Wiederbeleben
            ReviveHandler.RevivePlayer(target, player);
        }

        // ========================================
        // TELEPORT COMMANDS
        // ========================================

        [Command("tp")]
        public void CMD_Teleport(GTANetworkAPI.Player player, string locationName)
        {
            if (!HasPermission(player, PermissionLevel.Supporter))
            {
                player.SendChatMessage("~r~Keine Berechtigung! (Supporter benötigt)");
                return;
            }

            LoadTeleportLocations();

            TeleportLocation location = TeleportLocations.Find(loc =>
                loc.Name.ToLower() == locationName.ToLower());

            if (location == null)
            {
                player.SendChatMessage("~r~Ort nicht gefunden!");
                player.SendChatMessage("~y~Verfügbare Orte:");

                var availableLocations = TeleportLocations
                    .Select(loc => loc.Name)
                    .ToList();

                if (availableLocations.Count > 0)
                {
                    player.SendChatMessage($"~w~{string.Join(", ", availableLocations)}");
                }
                return;
            }

            player.Position = new Vector3(location.X, location.Y, location.Z);
            player.Rotation = new Vector3(0, 0, location.Rotation);
            player.SendChatMessage($"~g~Teleportiert zu: ~w~{location.Name}");
        }

        [Command("tpcoord")]
        public void CMD_TeleportCoords(GTANetworkAPI.Player player, float x, float y, float z)
        {
            if (!HasPermission(player, PermissionLevel.Administrator))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            player.Position = new Vector3(x, y, z);
            player.SendChatMessage($"~b~Teleportiert zu: {x:F2}, {y:F2}, {z:F2}");
        }

        [Command("vtp")]
        public void CMD_VehicleTeleport(GTANetworkAPI.Player player, string locationName)
        {
            if (!HasPermission(player, PermissionLevel.Supporter))
            {
                player.SendChatMessage("~r~Keine Berechtigung! (Supporter benötigt)");
                return;
            }

            LoadTeleportLocations();

            if (!player.IsInVehicle)
            {
                player.SendChatMessage("~r~Du befindest dich nicht in einem Fahrzeug!");
                return;
            }

            TeleportLocation location = TeleportLocations.Find(loc =>
                loc.Name.ToLower() == locationName.ToLower());

            if (location == null)
            {
                player.SendChatMessage("~r~Ort nicht gefunden!");
                return;
            }

            Vehicle vehicle = player.Vehicle;
            vehicle.Position = new Vector3(location.X, location.Y, location.Z);
            vehicle.Rotation = new Vector3(0, 0, location.Rotation);
            player.SendChatMessage($"~g~Du und dein Fahrzeug wurden zu ~w~{location.Name} ~g~teleportiert.");
        }

        [Command("tpto")]
        public void CMD_TeleportTo(GTANetworkAPI.Player player, string targetName)
        {
            if (!HasPermission(player, PermissionLevel.Moderator))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            var target = NAPI.Pools.GetAllPlayers().FirstOrDefault(p =>
                p.Name.ToLower().Contains(targetName.ToLower()));

            if (target == null)
            {
                player.SendChatMessage("~r~Spieler nicht gefunden!");
                return;
            }

            player.Position = target.Position;
            player.SendChatMessage($"~b~Zu {target.Name} teleportiert!");
        }

        // ========================================
        // WAFFEN COMMANDS
        // ========================================

        [Command("weapon")]
        public void CMD_GiveWeapon(GTANetworkAPI.Player player, string weaponName = "pistol50")
        {
            if (!HasPermission(player, PermissionLevel.Supporter))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            WeaponHash weaponHash = (WeaponHash)NAPI.Util.GetHashKey(weaponName);
            if ((uint)weaponHash == 0)
            {
                player.SendChatMessage("~r~Ungültige Waffe!");
                return;
            }

            player.GiveWeapon(weaponHash, 250);
            player.SendChatMessage($"~g~Waffe {weaponName} mit 250 Schuss erhalten!");
        }

        [Command("removeweapons")]
        public void CMD_RemoveWeapons(GTANetworkAPI.Player player)
        {
            if (!HasPermission(player, PermissionLevel.Supporter))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            player.RemoveAllWeapons();
            player.SendChatMessage("~g~Alle Waffen entfernt!");
        }

        // ========================================
        // PERMISSION COMMANDS
        // ========================================

        [Command("setadmin")]
        public void CMD_SetAdmin(GTANetworkAPI.Player player, string targetName, int level)
        {
            if (!HasPermission(player, PermissionLevel.Owner))
            {
                player.SendChatMessage("~r~Keine Berechtigung! (Owner benötigt)");
                return;
            }

            var target = NAPI.Pools.GetAllPlayers().FirstOrDefault(p =>
                p.Name.ToLower().Contains(targetName.ToLower()));

            if (target == null)
            {
                player.SendChatMessage("~r~Spieler nicht gefunden!");
                return;
            }

            var targetChar = GetCharacter(target);
            if (targetChar == null)
            {
                player.SendChatMessage("~r~Charakter nicht geladen!");
                return;
            }

            if (level < 0 || level > 6)
            {
                player.SendChatMessage("~r~Level muss zwischen 0 und 6 sein!");
                return;
            }

            var adminChar = GetCharacter(player);
            AccountManager.Instance.SetPermission(targetChar.AccountId, (PermissionLevel)level, adminChar.FullName);

            player.SendChatMessage($"~g~{target.Name} ist nun {(PermissionLevel)level}!");
            target.SendChatMessage($"~g~Du wurdest zum {(PermissionLevel)level} ernannt!");
        }

        // ========================================
        // GODMODE COMMAND
        // ========================================

        [Command("godmode")]
        public void CMD_ToggleGodMode(GTANetworkAPI.Player player)
        {
            if (!HasPermission(player, PermissionLevel.Administrator))
            {
                player.SendChatMessage("~r~Keine Berechtigung! (Administrator benötigt)");
                return;
            }

            bool godModeEnabled = GodModeHandler.HasGodMode(player);
            godModeEnabled = !godModeEnabled;

            GodModeHandler.SetGodMode(player, godModeEnabled);

            if (godModeEnabled)
            {
                player.SendChatMessage("~g~Godmode aktiviert!");
                NAPI.Util.ConsoleOutput($"[GODMODE] {player.Name} hat Godmode aktiviert");
            }
            else
            {
                player.SendChatMessage("~y~Godmode deaktiviert!");
                NAPI.Util.ConsoleOutput($"[GODMODE] {player.Name} hat Godmode deaktiviert");
            }
        }

        [Command("agodmode")]
        public void CMD_AdminGodMode(GTANetworkAPI.Player player, string targetName)
        {
            if (!HasPermission(player, PermissionLevel.Administrator))
            {
                player.SendChatMessage("~r~Keine Berechtigung!");
                return;
            }

            var target = NAPI.Pools.GetAllPlayers().FirstOrDefault(p =>
                p.Name.ToLower().Contains(targetName.ToLower()));

            if (target == null)
            {
                player.SendChatMessage("~r~Spieler nicht gefunden!");
                return;
            }

            bool godModeEnabled = GodModeHandler.HasGodMode(target);
            godModeEnabled = !godModeEnabled;

            GodModeHandler.SetGodMode(target, godModeEnabled);

            string status = godModeEnabled ? "aktiviert" : "deaktiviert";
            player.SendChatMessage($"~g~Godmode für {target.Name} {status}!");
            target.SendChatMessage($"~g~Godmode wurde von {player.Name} {status}!");
        }
    }

    /// <summary>
    /// Player Commands - Für alle Spieler zugänglich
    /// </summary>
    public class PlayerCommands : BaseCommands
    {
        // ========================================
        // ALLGEMEINE COMMANDS
        // ========================================

        [Command("stats")]
        public void CMD_Stats(GTANetworkAPI.Player player)
        {
            var character = GetCharacter(player);
            if (character == null)
            {
                player.SendChatMessage("~r~Charakter nicht geladen!");
                return;
            }

            player.SendChatMessage("~b~=== Deine Stats ===");
            player.SendChatMessage($"~w~Name: ~y~{character.FullName}");
            player.SendChatMessage($"~w~Level: ~g~{character.Level}");
            player.SendChatMessage($"~w~Bargeld: ~g~${character.Cash:N0}");
            player.SendChatMessage($"~w~Bank: ~g~${character.BankBalance:N0}");
            player.SendChatMessage($"~w~Job: ~y~{character.Job}");
            player.SendChatMessage($"~w~Spielzeit: ~y~{character.PlaytimeMinutes} Min");

            // Fraktions-Info
            if (character.IsInFaction())
            {
                var faction = FactionManager.Instance.GetFaction(character.FactionId.Value);
                var rank = FactionManager.Instance.GetCharacterRank(character.Id);
                if (faction != null && rank != null)
                {
                    player.SendChatMessage($"~w~Fraktion: ~b~{faction.Name}");
                    player.SendChatMessage($"~w~Rang: ~b~{rank.Name}");
                }
            }

            // Admin-Level anzeigen
            var permission = AccountManager.Instance.GetPermission(character.AccountId);
            if (permission.Level > PermissionLevel.Spieler)
            {
                player.SendChatMessage($"~w~Staff-Level: ~r~{permission.Level}");
            }
        }

        [Command("pos")]
        public void CMD_Position(GTANetworkAPI.Player player)
        {
            Vector3 pos = player.Position;
            Vector3 rot = player.Rotation;
            player.SendChatMessage("~b~=== Position ===");
            player.SendChatMessage($"~w~X: ~y~{pos.X:F2} ~w~Y: ~y~{pos.Y:F2} ~w~Z: ~y~{pos.Z:F2}");
            player.SendChatMessage($"~w~Rotation: ~y~{rot.Z:F2}");
            NAPI.Util.ConsoleOutput($"[POS] {player.Name}: new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f)");
        }

        [Command("help")]
        public void CMD_Help(GTANetworkAPI.Player player)
        {
            player.SendChatMessage("~b~=== Verfügbare Befehle ===");
            player.SendChatMessage("~y~Allgemein:");
            player.SendChatMessage("~w~/stats ~s~- Zeigt deine Statistiken");
            player.SendChatMessage("~w~/pos ~s~- Zeigt deine Position");
            player.SendChatMessage("~w~/time ~s~- Zeigt die Serverzeit");
            player.SendChatMessage("~w~/help ~s~- Zeigt diese Hilfe");

            var character = GetCharacter(player);
            if (character != null)
            {
                var permission = AccountManager.Instance.GetPermission(character.AccountId);

                if (permission.IsStaff())
                {
                    player.SendChatMessage("~y~Staff-Commands:");
                    player.SendChatMessage("~w~/av ~s~- Spawnt Shotaro");
                    player.SendChatMessage("~w~/car [name] ~s~- Spawnt Fahrzeug");
                    player.SendChatMessage("~w~/heal ~s~- Heilt dich");
                    player.SendChatMessage("~w~/weapon [name] ~s~- Gibt Waffe");
                    player.SendChatMessage("~w~/removeweapons ~s~- Entfernt alle Waffen");
                }

                if (permission.HasPermission(PermissionLevel.Moderator))
                {
                    player.SendChatMessage("~y~Moderator-Commands:");
                    player.SendChatMessage("~w~/aheal [name] ~s~- Heilt Spieler");
                    player.SendChatMessage("~w~/revive [name] ~s~- Belebt toten Spieler wieder");
                    player.SendChatMessage("~w~/tpto [name] ~s~- Teleportiert zu Spieler");
                }

                if (permission.HasPermission(PermissionLevel.Administrator))
                {
                    player.SendChatMessage("~y~Admin-Commands:");
                    player.SendChatMessage("~w~/veh [name] [c1] [c2] ~s~- Spawnt Fahrzeug mit Farben");
                    player.SendChatMessage("~w~/tp [ort] ~s~- Teleportiert zu vordefiniertem Ort");
                    player.SendChatMessage("~w~/vtp [ort] ~s~- Teleportiert mit Fahrzeug zu Ort");
                    player.SendChatMessage("~w~/tpcoord [x] [y] [z] ~s~- Teleportiert zu Koordinaten");
                    player.SendChatMessage("~w~/godmode ~s~- Aktiviert/Deaktiviert Godmode");
                    player.SendChatMessage("~w~/agodmode [name] ~s~- Godmode für anderen Spieler");
                }

                if (permission.HasPermission(PermissionLevel.Owner))
                {
                    player.SendChatMessage("~y~Owner-Commands:");
                    player.SendChatMessage("~w~/setadmin [name] [level] ~s~- Setzt Admin-Level");
                }

                // Fraktions-Commands
                if (character.IsInFaction())
                {
                    player.SendChatMessage("~y~Fraktions-Commands:");
                    player.SendChatMessage("~w~/duty ~s~- Dienst an/aus");
                    player.SendChatMessage("~w~/finfo ~s~- Fraktionsinfo");
                }
            }
        }

        [Command("time")]
        public void CMD_Time(GTANetworkAPI.Player player)
        {
            var now = System.DateTime.Now;
            player.SendChatMessage($"~b~Serverzeit: ~w~{now:HH:mm:ss}");
        }
    }

    /// <summary>
    /// Fraktions-Commands - Für Fraktionsmitglieder
    /// </summary>
    public class FactionCommands : BaseCommands
    {
        [Command("duty")]
        public void CMD_Duty(GTANetworkAPI.Player player)
        {
            var character = GetCharacter(player);
            if (character == null || !character.IsInFaction())
            {
                player.SendChatMessage("~r~Du bist in keiner Fraktion!");
                return;
            }

            var member = FactionManager.Instance.GetFactionMember(character.Id);
            if (member == null) return;

            bool newStatus = !member.IsOnDuty;
            FactionManager.Instance.SetDutyStatus(character.Id, newStatus);

            var faction = FactionManager.Instance.GetFaction(member.FactionId);
            string status = newStatus ? "~g~im Dienst" : "~r~außer Dienst";
            player.SendChatMessage($"~w~Du bist nun {status} ~w~bei {faction?.ShortName}");
        }

        [Command("finfo")]
        public void CMD_FactionInfo(GTANetworkAPI.Player player)
        {
            var character = GetCharacter(player);
            if (character == null || !character.IsInFaction())
            {
                player.SendChatMessage("~r~Du bist in keiner Fraktion!");
                return;
            }

            var faction = FactionManager.Instance.GetFaction(character.FactionId.Value);
            var rank = FactionManager.Instance.GetCharacterRank(character.Id);
            var members = FactionManager.Instance.GetFactionMembers(faction.Id);

            player.SendChatMessage($"~b~=== {faction.Name} ===");
            player.SendChatMessage($"~w~Typ: ~y~{faction.GetTypeDescription()}");
            player.SendChatMessage($"~w~Dein Rang: ~y~{rank.Name}");
            player.SendChatMessage($"~w~Gehalt: ~g~${rank.Salary}");
            player.SendChatMessage($"~w~Mitglieder: ~y~{members.Count}/{faction.MaxMembers}");
            player.SendChatMessage($"~w~Fraktionskasse: ~g~${faction.BankBalance:N0}");
        }
    }
}
