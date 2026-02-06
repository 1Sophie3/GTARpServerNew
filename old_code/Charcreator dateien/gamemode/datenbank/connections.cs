using gamemode.Configs;
using gamemode.Notifications;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;

namespace gamemode.Datenbank.connections
{
    class Connections : Script
    {
        [RemoteEvent("Auth.OnLogin")]
        // hier müssen sachen angepasst werden - auch mal schauen was man hier lädt und was in account.login 
        public void OnLogin(Player player, string password)
        {
            if (Accounts.IstSpielerEingeloggt(player))
            {
                player.SendNotification("~r~Du bist bereits eingeloggt");
           
            }
            if (!Datenbank.IstAccountBereitsVorhanden(player.Name))
            {
                player.SendNotification("~r~Kein Account gefunden. Bitte registriere dich zuerst!");
                return;
            }

            if (!Datenbank.PasswordCheck(player.Name, password))
            {
                player.SendNotification("~r~Falsches Passwort!");
                return;
            }

            // Account-Objekt deklarieren und initialisieren
            Accounts account = new Accounts(player.Name, player);

            // Nun nur das account-Objekt übergeben (kein zusätzliches player-Argument)
            if (!Datenbank.AccountLaden(account))
            {
                player.SendNotification("~r~Fehler: Dein Account konnte nicht geladen werden. Bist du möglicherweise gebannt?");
                player.Kick("Du bist gebannt oder es gibt ein Problem mit deinem Account.");
                return;
            }

            player.SetData(Accounts.Account_Key, account);

            account.Login(player, false);
            // Hardware-ID auslesen und speichern
            try
            {
                string hardwareId = player.Serial; // Hardware-ID des Spielers
                if (!string.IsNullOrEmpty(hardwareId))
                {

                    Datenbank.UpdateHardwareIdForExistingAccounts(player); // Hardware-ID aktualisieren
                }
                else
                {
                    NAPI.Util.ConsoleOutput($"[FEHLER] Keine gültige Hardware-ID für Spieler {player.Name} verfügbar.");
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Speichern der Hardware-ID für Spieler {player.Name}: {ex.Message}");
            }

            try
            {
                // Spielerwerte aus der `player_tracking`-Tabelle abrufen
                MySqlCommand command = Datenbank.CreateCommand();
                command.CommandText = "SELECT last_health, last_armor, position_x, position_y, position_z FROM player_tracking WHERE account_id = @id";
                command.Parameters.AddWithValue("@id", account.ID);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        // Lade Gesundheits- und Rüstungswerte
                        int lastHealth = reader.GetInt32("last_health");
                        int lastArmor = reader.GetInt32("last_armor");

                        // Setze Gesundheits- und Rüstungswerte, falls gültig
                        player.Health = Math.Clamp(lastHealth, 0, 100);
                        player.Armor = Math.Clamp(lastArmor, 0, 100);

                        // Begrüße Spieler
                        Utils.sendNotification(player, "Willkommen", "");

                        // Position des Spielers setzen
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
                            // Standardposition setzen, falls keine gültige Position existiert
                            player.Position = new Vector3(-1036.182, -2729.434, 13.756);
                            NAPI.Util.ConsoleOutput($"[INFO] Keine gespeicherte Position. Spieler {player.Name} wurde auf die Standardposition gesetzt.");
                        }
                    }
                    else
                    {
                        // Standardwerte setzen, falls keine Tracking-Daten gefunden werden
                        player.Health = 100;
                        player.Armor = 0;
                        player.Position = new Vector3(-1036.182, -2729.434, 13.756);
                        NAPI.Util.ConsoleOutput($"[WARNUNG] Keine Tracking-Daten für Spieler {player.Name} gefunden. Standardwerte wurden gesetzt.");
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Abrufen der Gesundheitsdaten für Spieler {player.Name}: {ex.Message}");
                player.Health = 100;
                player.Armor = 0;
                player.Position = new Vector3(-1036.182, -2729.434, 13.756); // Fallback bei Fehler
            }

            Main.StartOptimizedMonitoring(player);
            Main.StartPlaytimeMonitoring(player);
        }





        [RemoteEvent("Auth.OnRegister")]
        public void OnRegister(Player player, string password)
        {
            // Zuerst prüfen, ob der Account bereits existiert
            if (Datenbank.IstAccountBereitsVorhanden(player.Name))
            {
                player.SendNotification("~r~Accountname bereits vorhanden!");
                return;
            }

            // Account-Objekt deklarieren und initialisieren
            Accounts account = new Accounts(player.Name, player);

            // Account erstellen (Registrierung)
            account.Register(player.Name, password);

            // Optional: Den soeben erstellten Account laden, um sicherzustellen, dass alle Daten korrekt sind
            if (!Datenbank.AccountLaden(account))
            {
                player.SendNotification("~r~Fehler: Dein Account konnte nicht geladen werden. Bist du möglicherweise gebannt?");
                player.Kick("Du bist gebannt oder es gibt ein Problem mit deinem Account.");
                return;
            }

            player.SetData(Accounts.Account_Key, account);

            // Hardware-ID auslesen und in die DB speichern
            try
            {
                string hardwareId = player.Serial; // Hardware-ID des Spielers
                if (!string.IsNullOrEmpty(hardwareId))
                {
                    Datenbank.UpdateHardwareIdForExistingAccounts(player); // Hardware-ID aktualisieren
                }
                else
                {
                    NAPI.Util.ConsoleOutput($"[FEHLER] Keine gültige Hardware-ID für Spieler {player.Name} verfügbar.");
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Speichern der Hardware-ID für Spieler {player.Name}: {ex.Message}");
            }
        }
    }
}

