using GTANetworkAPI;
using System.Collections.Generic;
using System;
using MySql.Data.MySqlClient;
using RPCore.Datenbank; // Ersetze dies ggf. durch den korrekten Namespace deiner Datenbankklasse
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace RPCore.Fishing
{
    public enum ZoneShape
    {
        Circle,
        Rectangle
    }

    public class FishingZone
    {
        public string Name { get; set; }
        public ZoneShape Shape { get; set; }
        public Vector3 Pos1 { get; set; }
        public Vector3 Pos2 { get; set; }
        public float Radius { get; set; }
    }

    public class FishingLootItem
    {
        public string ItemName { get; set; }
        public string ItemDbName { get; set; }
        public float Chance { get; set; }
    }

    public class FishingController : Script
    {
        private const int FISHING_DURATION_MS = 35000;
        private static readonly Dictionary<Player, Timer> _fishingTimers = new Dictionary<Player, Timer>();
        private static List<FishingZone> _fishingZones = new List<FishingZone>();
        private const string ZONES_FILE_PATH = "dotnet/resources/gamemode/fishing_zones.json";
        private static Dictionary<Player, Vector3> _corner1Positions = new Dictionary<Player, Vector3>();
        private const int REQUIRED_ADMIN_LEVEL = 3;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            LoadFishingZones();
        }

        private void LoadFishingZones()
        {
            if (File.Exists(ZONES_FILE_PATH))
            {
                string json = File.ReadAllText(ZONES_FILE_PATH);
                _fishingZones = JsonConvert.DeserializeObject<List<FishingZone>>(json) ?? new List<FishingZone>();
                NAPI.Util.ConsoleOutput($"[FISHING] {_fishingZones.Count} Angel-Zonen erfolgreich geladen.");
            }
            else
            {
                NAPI.Util.ConsoleOutput($"[FISHING] Keine Speicherdatei für Angel-Zonen gefunden.");
            }
        }

        private void SaveFishingZones()
        {
            string json = JsonConvert.SerializeObject(_fishingZones, Formatting.Indented);
            File.WriteAllText(ZONES_FILE_PATH, json);
            NAPI.Util.ConsoleOutput($"[FISHING] Angel-Zonen erfolgreich gespeichert.");
        }

        public static bool IsPlayerInFishingZone(Player player)
        {
            Vector3 playerPos = player.Position;
            foreach (var zone in _fishingZones)
            {
                if (zone.Shape == ZoneShape.Circle)
                {
                    if (playerPos.DistanceTo(zone.Pos1) <= zone.Radius) return true;
                }
                else if (zone.Shape == ZoneShape.Rectangle)
                {
                    float minX = Math.Min(zone.Pos1.X, zone.Pos2.X);
                    float maxX = Math.Max(zone.Pos1.X, zone.Pos2.X);
                    float minY = Math.Min(zone.Pos1.Y, zone.Pos2.Y);
                    float maxY = Math.Max(zone.Pos1.Y, zone.Pos2.Y);
                    if (playerPos.X >= minX && playerPos.X <= maxX && playerPos.Y >= minY && playerPos.Y <= maxY)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region Admin Commands for Zone Management

        // KORRIGIERT: Korrekte Command-Syntax
        [Command("createfishcircle")]
        public void CMD_createfishcircle(Player player, float radius, string name)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || account.Adminlevel < REQUIRED_ADMIN_LEVEL)
            {
                player.SendNotification("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }

            if (radius <= 0)
            {
                player.SendNotification("~r~Der Radius muss größer als 0 sein.");
                return;
            }
            FishingZone newZone = new FishingZone { Name = name, Shape = ZoneShape.Circle, Pos1 = player.Position, Radius = radius };
            _fishingZones.Add(newZone);
            player.SendNotification($"~g~Kreis-Zone '{name}' mit Radius {radius}m erstellt.");
            player.SendNotification("~y~WICHTIG: Benutze /savefishzones zum Speichern!");
        }

        // KORRIGIERT: Korrekte Command-Syntax
        [Command("setcorner1")]
        public void CMD_setcorner1(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || account.Adminlevel < REQUIRED_ADMIN_LEVEL)
            {
                player.SendNotification("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            _corner1Positions[player] = player.Position;
            player.SendNotification($"~g~Ecke 1 für eine neue Rechteck-Zone an deiner Position gesetzt.");
            player.SendNotification("~y~Gehe jetzt zur gegenüberliegenden Ecke und benutze /setcorner2 [ZonenName]");
        }

        // KORRIGIERT: Korrekte Command-Syntax
        [Command("setcorner2")]
        public void CMD_setcorner2(Player player, string name)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || account.Adminlevel < REQUIRED_ADMIN_LEVEL)
            {
                player.SendNotification("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }

            if (!_corner1Positions.ContainsKey(player))
            {
                player.SendNotification("~r~Du musst zuerst mit /setcorner1 die erste Ecke festlegen!");
                return;
            }
            Vector3 corner1 = _corner1Positions[player];
            Vector3 corner2 = player.Position;
            FishingZone newZone = new FishingZone { Name = name, Shape = ZoneShape.Rectangle, Pos1 = corner1, Pos2 = corner2 };
            _fishingZones.Add(newZone);
            _corner1Positions.Remove(player);
            player.SendNotification($"~g~Rechteck-Zone '{name}' erfolgreich erstellt.");
            player.SendNotification("~y~WICHTIG: Benutze /savefishzones zum Speichern!");
        }

        // KORRIGIERT: Korrekte Command-Syntax
        [Command("savefishzones")]
        public void CMD_savefishzones(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || account.Adminlevel < REQUIRED_ADMIN_LEVEL)
            {
                player.SendNotification("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            SaveFishingZones();
            player.SendNotification($"~g~Alle {_fishingZones.Count} Angel-Zonen wurden gespeichert.");
        }

        // KORRIGIERT: Korrekte Command-Syntax
        [Command("reloadfishzones")]
        public void CMD_reloadfishzones(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || account.Adminlevel < REQUIRED_ADMIN_LEVEL)
            {
                player.SendNotification("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            LoadFishingZones();
            player.SendNotification($"~g~Alle Angel-Zonen wurden aus der Datei neu geladen.");
        }

        // KORRIGIERT: Korrekte Command-Syntax
        [Command("listfishzones")]
        public void CMD_listfishzones(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || account.Adminlevel < REQUIRED_ADMIN_LEVEL)
            {
                player.SendNotification("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            player.SendNotification("--- Angel-Zonen ---");
            for (int i = 0; i < _fishingZones.Count; i++)
            {
                var zone = _fishingZones[i];
                player.SendNotification($"ID: {i} | Name: {zone.Name} | Typ: {zone.Shape}");
            }
            player.SendNotification("------------------");
        }

        // KORRIGIERT: Korrekte Command-Syntax
        [Command("delfishzone")]
        public void CMD_delfishzone(Player player, int id)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null || account.Adminlevel < REQUIRED_ADMIN_LEVEL)
            {
                player.SendNotification("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }

            if (id < 0 || id >= _fishingZones.Count)
            {
                player.SendNotification("~r~Ungültige Zonen-ID.");
                return;
            }
            string zoneName = _fishingZones[id].Name;
            _fishingZones.RemoveAt(id);
            player.SendNotification($"~g~Zone '{zoneName}' (ID: {id}) wurde gelöscht.");
            player.SendNotification("~y~WICHTIG: Benutze /savefishzones, um die Löschung dauerhaft zu machen!");
        }

        #endregion

        #region Core Fishing Logic

        [Command("startfish")]
        public void CMD_StartFishing(Player player)
        {
            if (_fishingTimers.ContainsKey(player))
            {
                player.SendNotification("~r~Du angelst bereits!");
                return;
            }
            if (!IsPlayerInFishingZone(player))
            {
                player.SendNotification("~r~Du kannst hier nicht angeln. Suche ein passendes Gewässer.");
                return;
            }
            StartNewFishingSession(player);
        }

        [Command("stopfish")]
        public void CMD_StopFishing(Player player)
        {
            if (!_fishingTimers.ContainsKey(player))
            {
                player.SendNotification("~r~Du angelst doch gar nicht.");
                return;
            }
            StopFishing(player, true);
        }

        private void StartNewFishingSession(Player player)
        {
            player.SendNotification("~g~Du hast deine Angel ausgeworfen...");
            player.TriggerEvent("client:fishing:start");
            StartFishingTimer(player);
        }

        private void StartFishingTimer(Player player)
        {
            if (_fishingTimers.TryGetValue(player, out Timer oldTimer))
            {
                oldTimer.Dispose();
            }
            Timer fishingTimer = new Timer(OnFishingFinished, player, FISHING_DURATION_MS, Timeout.Infinite);
            _fishingTimers[player] = fishingTimer;
        }

        private void OnFishingFinished(object state)
        {
            Player player = (Player)state;
            if (player == null || !player.Exists) return;

            GiveFishingReward(player);
            player.TriggerEvent("client:fishing:showContinuePrompt");
        }

        [RemoteEvent("server:fishing:continue")]
        public void OnContinueFishing(Player player, bool continueFishing)
        {
            if (player == null || !player.Exists) return;

            if (continueFishing)
            {
                if (!IsPlayerInFishingZone(player))
                {
                    player.SendNotification("~r~Du hast dich zu weit vom Wasser entfernt.");
                    StopFishing(player, false);
                    return;
                }
                player.SendNotification("~g~Du wirfst die Leine erneut aus...");
                StartFishingTimer(player);
            }
            else
            {
                StopFishing(player, false);
            }
        }

        public void StopFishing(Player player, bool notify)
        {
            if (_fishingTimers.TryGetValue(player, out Timer timer))
            {
                timer.Dispose();
                _fishingTimers.Remove(player);
            }
            player.TriggerEvent("client:fishing:stop");
            if (notify)
            {
                player.SendNotification("Du hast das Angeln abgebrochen.");
            }
        }

        private void GiveFishingReward(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account != null)
            {
                account.Geld += 1;
                player.SendNotification($"~y~Debug: Du hast 1$ erhalten.");
            }
        }

        private static List<FishingLootItem> GetFishingLoot()
        {
            var lootTable = new List<FishingLootItem>();
            string connectionString = Datenbank.Datenbank.GetConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT itemName, itemDbName, chance FROM fishing_loottable";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lootTable.Add(new FishingLootItem { ItemName = reader.GetString("itemName"), ItemDbName = reader.GetString("itemDbName"), Chance = reader.GetFloat("chance") });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    NAPI.Util.ConsoleOutput($"[FISHING ERROR] Fehler beim Laden der Loot-Tabelle: {ex.Message}");
                }
            }
            return lootTable;
        }

        private static FishingLootItem GetRandomItem(List<FishingLootItem> items)
        {
            if (!items.Any()) return null;

            float totalChance = items.Sum(item => item.Chance);
            Random rand = new Random();
            double randomValue = rand.NextDouble() * totalChance;

            foreach (var item in items)
            {
                if (randomValue < item.Chance)
                {
                    return item;
                }
                randomValue -= item.Chance;
            }
            return items.Last();
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnect(Player player, DisconnectionType type, string reason)
        {
            if (_fishingTimers.ContainsKey(player))
            {
                StopFishing(player, false);
            }
            if (_corner1Positions.ContainsKey(player))
            {
                _corner1Positions.Remove(player);
            }
        }
        #endregion
    }
}