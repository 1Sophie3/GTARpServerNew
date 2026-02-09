using RPCore.Configs;
using RPCore.Notifications;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using RPCore.Datenbank; // Sicherstellen, dass Datenbank-Namespace importiert ist

namespace RPCore.Datenbank.connections
{
    class Connections : Script
    {
        [RemoteEvent("Auth.OnLogin")]
        public void OnLogin(Player player, string password)
        {
            // Die Social Club ID des Spielers erhalten
            ulong socialClubId = player.SocialClubId;

            // Jetzt mit socialClubId prüfen, nicht mit player.Name
            if (!Datenbank.IstAccountBereitsVorhanden(socialClubId))
            {
                player.SendNotification("~r~Kein Account gefunden. Bitte registriere dich zuerst!");
                return;
            }

        
            // oder du holst dir die ID zuerst über die SCID. Ich passe sie hier hypothetisch an.
            if (!Datenbank.PasswordCheck(socialClubId, password)) 
            {
                player.SendNotification("~r~Falsches Passwort!");
                return;
            }

            // Account-Objekt deklarieren und initialisieren
            // Übergib die SocialClubId beim Initialisieren, falls dein Accounts-Konstruktor sie verarbeitet.
            // Wenn dein Konstruktor keinen Player oder keine SCID nimmt, dann setze sie danach.
            Accounts account = new Accounts();
            account.SocialClubId = socialClubId; // WICHTIG: SocialClubId im Account-Objekt setzen
            account.Name = player.Name; // Ragename für Anzeigezwecke beibehalten

            // Nun das account-Objekt übergeben (AccountLaden nutzt die bereits gesetzte SocialClubId im Objekt)
            if (!Datenbank.AccountLaden(account))
            {
                player.SendNotification("~r~Fehler: Dein Account konnte nicht geladen werden. Bist du möglicherweise gebannt?");
                player.Kick("Du bist gebannt oder es gibt ein Problem mit deinem Account.");
                return;
            }

            player.SetData(Accounts.Account_Key, account);
            account.Login(player, false);


            // Hardware-ID und Social Club ID auslesen und speichern/aktualisieren
            try
            {
                // Datenbank.UpdateHardwareIdForExistingAccounts(player) kümmert sich bereits um HWID und SCID.
                // Es ist nicht nötig, die Hardware-ID hier manuell zu prüfen, es sei denn, du hast spezifische Anforderungen.
                Datenbank.UpdateHardwareIdForExistingAccounts(player); // Hardware-ID und Social Club ID aktualisieren
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Aktualisieren der Hardware-ID/Social Club ID für Spieler {player.Name}: {ex.Message}");
            }

            // Der Rest des Login-Codes (Gesundheit, Position, etc.) bleibt, da er account.ID verwendet,
            // die wir nach dem Laden des Accounts haben.
            try
            {
                string connectionString = Datenbank.GetConnectionString();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        // Statt @id könntest du hier auch @socialClubId verwenden, aber @id ist ok,
                        // da account.ID nach dem Laden des Accounts gesetzt ist.
                        command.CommandText = "SELECT last_health, last_armor, position_x, position_y, position_z FROM player_tracking WHERE account_id = @id";
                        command.Parameters.AddWithValue("@id", account.ID);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                int lastHealth = reader.GetInt32("last_health");
                                int lastArmor = reader.GetInt32("last_armor");

                                player.Health = Math.Clamp(lastHealth, 0, 100);
                                player.Armor = Math.Clamp(lastArmor, 0, 100);

                                Utils.sendNotification(player, "Willkommen", "");

                                float positionX = reader.GetFloat("position_x");
                                float positionY = reader.GetFloat("position_y");
                                float positionZ = reader.GetFloat("position_z");

                                Vector3 storedPosition = new Vector3(positionX, positionY, positionZ);

                                if (storedPosition != new Vector3(0, 0, 0))
                                {
                                    player.Position = storedPosition;
                                }
                                else
                                {
                                    player.Position = new Vector3(-1036.182, -2729.434, 13.756);
                                    NAPI.Util.ConsoleOutput($"[INFO] Keine gespeicherte Position. Spieler {player.Name} wurde auf die Standardposition gesetzt.");
                                }
                            }
                            else
                            {
                                player.Health = 100;
                                player.Armor = 0;
                                player.Position = new Vector3(-1036.182, -2729.434, 13.756);
                                NAPI.Util.ConsoleOutput($"[WARNUNG] Keine Tracking-Daten für Spieler {player.Name} gefunden. Standardwerte wurden gesetzt.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Abrufen der Gesundheitsdaten für Spieler {player.Name}: {ex.Message}");
                player.Health = 100;
                player.Armor = 0;
                player.Position = new Vector3(-1036.182, -2729.434, 13.756);
            }

            Main.StartOptimizedMonitoring(player);
            Main.StartPlaytimeMonitoring(player);
        }


        [RemoteEvent("Auth.OnRegister")]
        public void OnRegister(Player player, string password)
        {
            ulong socialClubId = player.SocialClubId;

            // Zuerst prüfen, ob der Account bereits existiert (jetzt anhand der Social Club ID)
            if (Datenbank.IstAccountBereitsVorhanden(socialClubId))
            {
                player.SendNotification("~r~Ein Account mit dieser Social Club ID ist bereits registriert!");
                return;
            }

            // Account-Objekt deklarieren und initialisieren
            Accounts account = new Accounts(); // Standard-Konstruktor aufrufen
            account.Name = player.Name; // Ragename für Registrierung und Anzeige
            account.SocialClubId = socialClubId; // WICHTIG: SocialClubId setzen

            // Account erstellen (Registrierung)
            // Register-Methode von Accounts sollte jetzt ebenfalls die SCID nutzen
            account.Register(player.Name, password); // Die Register-Methode von Accounts sollte die SCID aus dem Account-Objekt verwenden

            // Optional: Den soeben erstellten Account laden, um sicherzustellen, dass alle Daten korrekt sind
            // AccountLaden nutzt die bereits gesetzte SocialClubId im Account-Objekt
            if (!Datenbank.AccountLaden(account))
            {
                player.SendNotification("~r~Fehler: Dein Account konnte nicht geladen werden. Bist du möglicherweise gebannt?");
                player.Kick("Du bist gebannt oder es gibt ein Problem mit deinem Account.");
                return;
            }

            player.SetData(Accounts.Account_Key, account);

            // Hardware-ID und Social Club ID auslesen und in die DB speichern/aktualisieren
            try
            {
                // Datenbank.UpdateHardwareIdForExistingAccounts(player) kümmert sich bereits um HWID und SCID.
                // Es ist nicht nötig, die Hardware-ID hier manuell zu prüfen.
                Datenbank.UpdateHardwareIdForExistingAccounts(player); // Hardware-ID und Social Club ID aktualisieren
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Speichern der Hardware-ID/Social Club ID für Spieler {player.Name}: {ex.Message}");
            }
        }
    }
}