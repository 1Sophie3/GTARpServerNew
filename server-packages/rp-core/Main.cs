using GTANetworkAPI;
using RPCore.Database;
using RPCore.Managers;
using System;
using System.Collections.Generic;

namespace RPCore
{
    public class Main : Script
    {
        public static DatabaseManager Database { get; private set; }
        private static List<Vehicle> _spawnedVehicles = new List<Vehicle>();

        // Konfiguration
        private static readonly Vector3 DefaultSpawnPosition = new Vector3(276.8289f, -580.4714f, 43.113613f);
        private const string WelcomeMessage = "~b~Willkommen auf dem RP-Server!";

        public Main()
        {
            NAPI.Util.ConsoleOutput("[RP-CORE] Initialisiere Roleplay Server...");

            // Datenbank initialisieren und testen
            InitializeDatabase();

            NAPI.Util.ConsoleOutput("[RP-CORE] Server erfolgreich gestartet!");
        }

        /// <summary>
        /// Initialisiert und testet die Datenbankverbindung
        /// </summary>
        private async void InitializeDatabase()
        {
            Database = DatabaseManager.Instance;

            NAPI.Util.ConsoleOutput("[RP-CORE] Teste Datenbankverbindung...");
            bool connected = await DatabaseManager.Instance.TestConnection();

            if (connected)
            {
                NAPI.Util.ConsoleOutput("[RP-CORE] ✓ Datenbankverbindung erfolgreich");
            }
            else
            {
                NAPI.Util.ConsoleOutput("[RP-CORE] ✗ WARNUNG: Datenbankverbindung fehlgeschlagen!");
                NAPI.Util.ConsoleOutput("[RP-CORE]    Überprüfe configs/database.json und MySQL Server");
            }
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            // Server-Einstellungen
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetDefaultSpawnLocation(DefaultSpawnPosition);

            // Spawn Fahrzeuge und andere Entities
            SpawnServerVehicles();

            NAPI.Util.ConsoleOutput("[RP-CORE] Resource gestartet");
            NAPI.Util.ConsoleOutput($"[RP-CORE] {_spawnedVehicles.Count} Fahrzeuge gespawnt");
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            // Cleanup
            foreach (var vehicle in _spawnedVehicles)
            {
                try
                {
                    vehicle?.Delete();
                }
                catch { }
            }
            _spawnedVehicles.Clear();

            NAPI.Util.ConsoleOutput("[RP-CORE] Resource gestoppt");
        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(GTANetworkAPI.Player player)
        {
            NAPI.Util.ConsoleOutput($"[RP-CORE] Spieler verbunden: {player.Name} (IP: {player.Address})");

            // Willkommensnachricht
            player.SendChatMessage(WelcomeMessage);
            NAPI.Chat.SendChatMessageToAll($"~y~{player.Name} ~w~hat den Server betreten!");

            // Spawn Spieler
            SpawnPlayer(player);

            // TODO: Trigger Login UI
            // player.TriggerEvent("client:showLoginUI");
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public async void OnPlayerDisconnected(GTANetworkAPI.Player player, DisconnectionType type, string reason)
        {
            // Speichere Charakter wenn geladen
            var character = CharacterManager.Instance.GetPlayerCharacter(player);
            if (character != null)
            {
                await CharacterManager.Instance.SaveCharacter(character);
            }
            await CharacterManager.Instance.RemovePlayerCharacter(player);

            NAPI.Util.ConsoleOutput($"[RP-CORE] Spieler getrennt: {player.Name} - Grund: {reason}");
            NAPI.Chat.SendChatMessageToAll($"~y~{player.Name} ~w~hat den Server verlassen.");
        }

        /// <summary>
        /// Spawnt einen Spieler an der Standard-Position
        /// </summary>
        private void SpawnPlayer(GTANetworkAPI.Player player)
        {
            player.Position = DefaultSpawnPosition;
            player.Rotation = new Vector3(0, 0, 0);
            player.Health = 100;

            NAPI.Util.ConsoleOutput($"[RP-CORE] {player.Name} gespawnt bei {DefaultSpawnPosition}");
        }

        /// <summary>
        /// Spawnt Server-Fahrzeuge (Test/Spawn-Fahrzeuge)
        /// </summary>
        private void SpawnServerVehicles()
        {
            // Akuma Motorräder am Flughafen (zum Testen)
            VehicleHash akumaHash = NAPI.Util.VehicleNameToModel("akuma");

            var positions = new List<Vector3>
            {
                new Vector3(-1019f, -2692.058f, 13.99043f),
                new Vector3(-1017f, -2688.171f, 13.99043f),
                new Vector3(-1015f, -2684.284f, 13.99043f),
            };

            foreach (var pos in positions)
            {
                Vehicle vehicle = NAPI.Vehicle.CreateVehicle(akumaHash, pos, 59f, 27, 27);
                vehicle.NumberPlate = "SERVER";
                vehicle.Locked = false;
                vehicle.EngineStatus = false;
                _spawnedVehicles.Add(vehicle);
            }

            NAPI.Util.ConsoleOutput($"[SPAWN] {_spawnedVehicles.Count} Server-Fahrzeuge gespawnt");
        }
    }
}
