using GTANetworkAPI;
using RPCore.Database;
using RPCore.Player;
using System;

namespace RPCore
{
    public class Main : Script
    {
        public static DatabaseManager Database { get; private set; }

        public Main()
        {
            NAPI.Util.ConsoleOutput("[RP-CORE] Initialisiere Roleplay Server...");
            
            // Datenbank initialisieren
            Database = new DatabaseManager();
            
            NAPI.Util.ConsoleOutput("[RP-CORE] Server erfolgreich gestartet!");
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Util.ConsoleOutput("[RP-CORE] Resource gestartet");
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            NAPI.Util.ConsoleOutput("[RP-CORE] Resource gestoppt");
            Database?.Disconnect();
        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(GTANetworkAPI.Player player)
        {
            NAPI.Util.ConsoleOutput($"[RP-CORE] Spieler verbunden: {player.Name}");
            
            // Spawn Spieler am Flughafen
            player.Position = new Vector3(-1037.7, -2738.5, 13.8);
            player.Rotation = new Vector3(0, 0, 0);
            
            // Trigger CEF UI
            player.TriggerEvent("client:showLoginUI");
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(GTANetworkAPI.Player player, DisconnectionType type, string reason)
        {
            NAPI.Util.ConsoleOutput($"[RP-CORE] Spieler getrennt: {player.Name} - Grund: {reason}");
        }
    }
}
