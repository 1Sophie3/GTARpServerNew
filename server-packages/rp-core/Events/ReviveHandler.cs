using GTANetworkAPI;
using RPCore.Managers;
using System.Collections.Generic;

namespace RPCore.Events
{
    /// <summary>
    /// Revive Handler - Verwaltet Tod und Wiederbelebung von Spielern
    /// </summary>
    public class ReviveHandler : Script
    {
        private static readonly Vector3 HospitalPosition = new Vector3(-1036.182f, -2729.434f, 13.75665f);
        private static HashSet<GTANetworkAPI.Player> _revivingPlayers = new HashSet<GTANetworkAPI.Player>();

        // Konfigurierbare Werte
        private const int DEATH_SCREEN_TIME = 8000;      // 8 Sekunden "Tot"-Anzeige
        private const int REVIVE_HEALTH = 70;             // Health nach Revive
        private const int REVIVE_ARMOR = 0;               // Armor nach Revive

        public ReviveHandler()
        {
            NAPI.Util.ConsoleOutput("[REVIVE] Handler initialisiert");
        }

        /// <summary>
        /// Event wenn ein Spieler stirbt
        /// </summary>
        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(GTANetworkAPI.Player player, GTANetworkAPI.Player killer, uint reason)
        {
            if (player == null) return;

            // Prüfe ob Spieler bereits wiederbelebt wird
            if (_revivingPlayers.Contains(player))
            {
                return;
            }

            _revivingPlayers.Add(player);

            // Logge Tod
            var character = CharacterManager.Instance.GetPlayerCharacter(player);
            if (character != null)
            {
                character.IsAlive = false;
                character.IsInjured = true;

                string killerName = killer != null ? killer.Name : "Unbekannt";
                NAPI.Util.ConsoleOutput($"[REVIVE] {player.Name} ist gestorben (Killer: {killerName}, Grund: {reason})");
            }

            // Starte Revive-Prozess
            StartReviveProcess(player);
        }

        /// <summary>
        /// Startet den automatischen Revive-Prozess
        /// </summary>
        private void StartReviveProcess(GTANetworkAPI.Player player)
        {
            // Warte während "Tot"-Screen
            NAPI.Task.Run(() =>
            {
                if (player == null || !player.Exists)
                {
                    _revivingPlayers.Remove(player);
                    return;
                }

                // Teleportiere zum Krankenhaus
                player.Position = HospitalPosition;
                player.Rotation = new Vector3(0, 0, 0);
                player.SendChatMessage("~y~Du wirst ins Krankenhaus gebracht...");

                // Warte kurz nach Teleport
                NAPI.Task.Run(() =>
                {
                    if (player == null || !player.Exists)
                    {
                        _revivingPlayers.Remove(player);
                        return;
                    }

                    // Wiederbeleben
                    RevivePlayer(player, null);

                }, delayTime: 2000);

            }, delayTime: DEATH_SCREEN_TIME);
        }

        /// <summary>
        /// Belebt einen Spieler wieder
        /// </summary>
        public static void RevivePlayer(GTANetworkAPI.Player player, GTANetworkAPI.Player? revivedBy)
        {
            if (player == null || !player.Exists) return;

            player.Health = REVIVE_HEALTH;
            player.Armor = REVIVE_ARMOR;

            // Aktualisiere Character-Daten
            var character = CharacterManager.Instance.GetPlayerCharacter(player);
            if (character != null)
            {
                character.IsAlive = true;
                character.IsInjured = false;
                character.Health = REVIVE_HEALTH;
                character.Armor = REVIVE_ARMOR;
            }

            // Entferne aus Revive-Liste
            _revivingPlayers.Remove(player);

            // Benachrichtigungen
            if (revivedBy != null)
            {
                player.SendChatMessage($"~g~Du wurdest von {revivedBy.Name} wiederbelebt!");
                revivedBy.SendChatMessage($"~g~Du hast {player.Name} wiederbelebt!");
                NAPI.Util.ConsoleOutput($"[REVIVE] {player.Name} wurde von {revivedBy.Name} wiederbelebt");
            }
            else
            {
                player.SendChatMessage("~g~Du wurdest im Krankenhaus behandelt und wiederbelebt!");
                NAPI.Util.ConsoleOutput($"[REVIVE] {player.Name} wurde automatisch wiederbelebt");
            }
        }

        /// <summary>
        /// Prüft ob ein Spieler gerade wiederbelebt wird
        /// </summary>
        public static bool IsReviving(GTANetworkAPI.Player player)
        {
            return _revivingPlayers.Contains(player);
        }

        /// <summary>
        /// Bricht den Revive-Prozess ab (z.B. für Admin-Revive)
        /// </summary>
        public static void CancelReviveProcess(GTANetworkAPI.Player player)
        {
            _revivingPlayers.Remove(player);
        }

        /// <summary>
        /// Cleanup bei Disconnect
        /// </summary>
        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(GTANetworkAPI.Player player, DisconnectionType type, string reason)
        {
            _revivingPlayers.Remove(player);
        }

        /// <summary>
        /// Cleanup bei Resource Stop
        /// </summary>
        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            _revivingPlayers.Clear();
            NAPI.Util.ConsoleOutput("[REVIVE] Handler gestoppt");
        }
    }
}
