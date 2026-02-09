using RPCore.Configs;
using RPCore.Database;
using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace RPCore.Commands
{
    // HINWEIS: Die Logik dieser Klasse wurde vollständig in AdminPanel.cs verschoben.
    // Du kannst diese Datei entweder löschen oder den Inhalt auskommentieren, um sie zu deaktivieren.
    public class TeleportCommand : Script
    {
        /*
        private List<TeleportLocation> locations;

        public TeleportCommand()
        {
            LoadLocations();
        }

        private void LoadLocations()
        {
            // ... Logik jetzt in AdminPanel.cs
        }

        public void TeleportPlayerCommand(Player player, string locationName)
        {
            // ... Logik jetzt in AdminPanel.cs
        }

        public void TeleportPlayerAndVehicleCommand(Player admin, string locationName)
        {
            // ... Logik jetzt in AdminPanel.cs
        }

        public void TeleportToPlayerCommand(Player admin, string identifier)
        {
            // ... Logik jetzt in AdminPanel.cs
        }
        */
    }

    public class TeleportLocation
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}