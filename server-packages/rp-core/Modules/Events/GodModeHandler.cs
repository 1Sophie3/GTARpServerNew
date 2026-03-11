using GTANetworkAPI;
using System.Collections.Generic;

namespace RPCore.Events
{
    /// <summary>
    /// GodMode Handler - Verwaltet Godmode für Spieler
    /// Nutzt effizientes Tracking statt ständiger Shared Data Checks
    /// </summary>
    public class GodModeHandler : Script
    {
        private static HashSet<GTANetworkAPI.Player> _godModePlayers = new HashSet<GTANetworkAPI.Player>();
        private static System.Timers.Timer? _updateTimer;

        public GodModeHandler()
        {
            // Timer für GodMode Update alle 500ms statt bei jedem Frame
            _updateTimer = new System.Timers.Timer(500);
            _updateTimer.Elapsed += UpdateGodModePlayers;
            _updateTimer.Start();

            NAPI.Util.ConsoleOutput("[GODMODE] Handler initialisiert");
        }

        /// <summary>
        /// Aktiviert/Deaktiviert GodMode für einen Spieler
        /// </summary>
        public static void SetGodMode(GTANetworkAPI.Player player, bool enabled)
        {
            if (enabled)
            {
                if (!_godModePlayers.Contains(player))
                {
                    _godModePlayers.Add(player);
                    player.SetSharedData("GodMode", true);
                }
            }
            else
            {
                _godModePlayers.Remove(player);
                player.ResetSharedData("GodMode");
            }
        }

        /// <summary>
        /// Prüft ob ein Spieler GodMode hat
        /// </summary>
        public static bool HasGodMode(GTANetworkAPI.Player player)
        {
            return _godModePlayers.Contains(player);
        }

        /// <summary>
        /// Update-Loop für alle Spieler mit GodMode
        /// </summary>
        private void UpdateGodModePlayers(object? sender, System.Timers.ElapsedEventArgs e)
        {
            // Liste kopieren um ConcurrentModification zu vermeiden
            var players = new List<GTANetworkAPI.Player>(_godModePlayers);

            foreach (var player in players)
            {
                try
                {
                    // Prüfe ob Spieler noch existiert
                    if (player == null || !player.Exists)
                    {
                        _godModePlayers.Remove(player);
                        continue;
                    }

                    // Setze Health und Armor zurück
                    if (player.Health < 100)
                    {
                        player.Health = 100;
                    }

                    if (player.Armor < 100)
                    {
                        player.Armor = 100;
                    }
                }
                catch
                {
                    // Spieler ist nicht mehr gültig, entfernen
                    _godModePlayers.Remove(player);
                }
            }
        }

        /// <summary>
        /// Cleanup wenn Spieler disconnected
        /// </summary>
        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(GTANetworkAPI.Player player, DisconnectionType type, string reason)
        {
            _godModePlayers.Remove(player);
        }

        /// <summary>
        /// Cleanup bei Resource Stop
        /// </summary>
        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            _godModePlayers.Clear();
            NAPI.Util.ConsoleOutput("[GODMODE] Handler gestoppt");
        }
    }
}
