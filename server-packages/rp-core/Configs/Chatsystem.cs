using GTANetworkAPI;
using System;
using System.IO;

namespace RPCore.Configs
{
    public class ChatHandler : Script
    {
        // Dieses Event wird ausgelöst, wenn ein Spieler eine Nachricht im Chat sendet.
        [ServerEvent(Event.ChatMessage)]
        public void OnChatMessage(Player player, string message)
        {
            // Falls die Nachricht nicht mit "/" beginnt, wird sie nicht global an alle Spieler gesendet.
            if (!message.StartsWith("/"))
            {
                // Logge normale Chatnachrichten, die nicht verarbeitet werden.
                NAPI.Util.ConsoleOutput($"[GlobalChat deaktiviert] {player.Name}: {message}");
                return;
            }

            // Nachrichten, die mit "/" beginnen, gelten als Befehle.
            // --- NEU: Start des Logging-Blocks ---

            // 1. Daten für den Log-Eintrag sammeln
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string playerName = player.Name;

            // 2. Den Log-Eintrag formatieren, genau wie in deinem Beispiel
            string logEntry = string.Format("[{0}] Spieler: {1} | Befehl: {2}",
                                              timestamp, playerName, message);

            // 3. In die Konsole ausgeben
            NAPI.Util.ConsoleOutput(logEntry);

            // 4. In die Datei schreiben (mit Fehlerbehandlung)
            try
            {
                string filePath = @"C:\RAGEMP\server-files\logs\commandlog.txt";
                string directoryPath = Path.GetDirectoryName(filePath);

                // Sicherstellen, dass der Ordner "logs" existiert
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Den Log-Eintrag an die Datei anhängen
                File.AppendAllText(filePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Command-Log konnte nicht geschrieben werden: {ex.Message}");
            }

            // --- ENDE: Ende des Logging-Blocks ---

         
        }

        // Implementierung des /ooc Befehls
        [Command("ooc")]
        public void CMD_OOC(Player player, params string[] messageParts)
        {
            // Fasse alle übergebenen Teile zu einer kompletten Nachricht zusammen.
            string message = string.Join(" ", messageParts);

            if (string.IsNullOrWhiteSpace(message))
            {
                player.SendChatMessage("Bitte gib eine Nachricht ein. Syntax: /ooc [Nachricht]");
                return;
            }

            // Logge den /ooc-Befehl in der Konsole
            NAPI.Util.ConsoleOutput($"[OOC] {player.Name}: {message}");

            // Definiere den Radius (z. B. 50 Meter), in dem andere Spieler die Nachricht erhalten sollen.
            float radius = 25.0f;

            // Sende ein Push-Notification-Event an alle Spieler im Umkreis.
            foreach (Player other in NAPI.Pools.GetAllPlayers())
            {
                if (other.Position.DistanceTo(player.Position) < radius)
                {
                    other.TriggerEvent("showPushNotification", $"{player.Name}: {message}");
                }
            }

            // Bestätige dem Absender, dass seine Nachricht versendet und geloggt wurde.
            player.SendChatMessage("Deine OOC Nachricht wurde gesendet und geloggt.");
        }
    }
}
