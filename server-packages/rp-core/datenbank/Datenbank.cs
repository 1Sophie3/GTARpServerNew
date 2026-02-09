using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using GTANetworkAPI;
using System.Text;
using BCrypt.Net;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using gamemode.datenbank;
using gamemode.Datenbank;
using Newtonsoft.Json;
using System.Linq;
using RPCore.Administration;

namespace RPCore.Datenbank
{
    public class Bankautomat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public int Dimension { get; set; }

        public Bankautomat(int id, string name, float posX, float posY, float posZ, int dimension)
        {
            Id = id;
            Name = name;
            PosX = posX;
            PosY = posY;
            PosZ = posZ;
            Dimension = dimension;
        }
    }

    public class PlayerBankAccount
    {
        public string Kontonummer { get; set; } = string.Empty;
        public decimal Kontostand { get; set; }
        public int PlayerId { get; set; }
    }

    public class TransactionData
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string SourceKontonummer { get; set; } // Für den Empfänger relevant
        public string TargetKontonummer { get; set; } // Für den Sender relevant
        public DateTime TransactionDate { get; set; }
    }


    public class Datenbank
    {
        public static bool DatenbankVerbindung = false;
        public static MySqlConnection Connection = null!; // Kompatibler Fix für die Warnung
        public static readonly object DbLock = new object();
        private static readonly Dictionary<ulong, bool> ActiveTimers = new Dictionary<ulong, bool>();
        private static readonly object TimerLock = new object();
        private static readonly Vector3 AutomaticRespawnPosition = new Vector3(275.0f, -579.0f, 43.0f);

        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }



        public static string GetConnectionString()
        {
            if (DbSettings._Settings == null)
            {
                NAPI.Util.ConsoleOutput("[FATAL ERROR] Datenbank-Einstellungen (DbSettings) wurden nicht geladen, bevor eine Verbindung angefordert wurde!");
                // Wir geben einen leeren String zurück, um einen Folgefehler zu provozieren, der aber klarer ist.
                return "";
            }
            // Greife direkt auf die statischen und geladenen Einstellungen zu.
            return $"SERVER={DbSettings._Settings.Host}; DATABASE={DbSettings._Settings.Database}; UID={DbSettings._Settings.Username}; PASSWORD={DbSettings._Settings.Password}";
        }

        public static void InitConnection()
        {
            string sqlConnection = GetConnectionString();
            using (MySqlConnection testConnection = new MySqlConnection(sqlConnection))
            {
                try
                {
                    testConnection.Open();
                    DatenbankVerbindung = true;
                    NAPI.Util.ConsoleOutput("DATENBANK ONLINE");
                    LoadAllGarages();
                    LoadBankATMs();
                }
                catch (Exception e)
                {
                    DatenbankVerbindung = false;
                    NAPI.Util.ConsoleOutput($"[ERROR] Datenbank konnte nicht gestartet werden: {e.Message}");
                }
                finally
                {
                    if (testConnection.State != System.Data.ConnectionState.Open)
                    {
                        NAPI.Util.ConsoleOutput("[FEHLER] Verbindung konnte nicht hergestellt werden. Überprüfe die Datenbankeinstellungen.");
                    }
                }
            }
        }

        public static bool IstAccountBereitsVorhanden(ulong socialClubId)
        {
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM accounts WHERE socialClubId=@socialClubId LIMIT 1";
                        command.Parameters.AddWithValue("@socialClubId", socialClubId);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
        }

        public static void NeuenAccountErstellen(Accounts account, string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "INSERT INTO accounts (name, password, adminlevel, geld, socialClubId) VALUES (@name, @password, @adminlevel, @geld, @socialClubId)";
                            command.Parameters.AddWithValue("@name", account.Name);
                            command.Parameters.AddWithValue("@password", hashedPassword);
                            command.Parameters.AddWithValue("@adminlevel", account.Adminlevel);
                            command.Parameters.AddWithValue("@geld", account.Geld);
                            command.Parameters.AddWithValue("@socialClubId", account.SocialClubId);
                            command.ExecuteNonQuery();
                            int accountId = Convert.ToInt32(command.LastInsertedId);
                            command.CommandText = "INSERT INTO player_tracking (account_id, last_health, last_armor, is_dead, remaining_time, played_time, position_x, position_y, position_z) VALUES (@id, 100, 0, false, 0, 0, 0, 0, 0)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@id", accountId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Account konnte nicht erstellt werden: {ex.Message}");
            }
        }

        public static bool AccountLaden(Accounts account)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT *, licenses FROM accounts WHERE socialClubId=@socialClubId LIMIT 1";
                            command.Parameters.AddWithValue("@socialClubId", account.SocialClubId);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Read();
                                    account.ID = reader.GetInt32("id");
                                    account.Name = reader.GetString("name");
                                    account.Adminlevel = reader.GetInt16("adminlevel");
                                    account.Geld = reader.GetInt32("geld");
                                    account.Fraktion = reader.GetInt32("fraktion");
                                    account.DutyStart = reader.IsDBNull(reader.GetOrdinal("duty_start")) ? (DateTime?)null : reader.GetDateTime("duty_start");
                                    account.CharacterData = reader.GetString("CharacterData");
                                    account.CharacterData = reader.GetString("CharacterData");
                                    account.Licenses = reader.IsDBNull(reader.GetOrdinal("licenses")) ? "[]" : reader.GetString("licenses"); // Lade die Lizenzen
                                    account.SocialClubId = reader.GetUInt64("socialClubId");
                                    account.LastLogoutDuty = reader.IsDBNull(reader.GetOrdinal("last_logout_duty")) ? (DateTime?)null : reader.GetDateTime("last_logout_duty");
                                    NAPI.Util.ConsoleOutput($"[DEBUG] Spieler {account.Name} ({account.SocialClubId}) geladen. DutyStart: {account.DutyStart}");
                                }
                                else
                                {
                                    NAPI.Util.ConsoleOutput($"[WARNUNG] Kein Account für SCID {account.SocialClubId} in der Tabelle `accounts` gefunden.");
                                    return false;
                                }
                            }
                        }
                        if (!CheckTrackingData(account.ID))
                        {
                            CreateTrackingEntry(account.ID);
                        }
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT played_time FROM player_tracking WHERE account_id=@id LIMIT 1";
                            command.Parameters.AddWithValue("@id", account.ID);
                            object result = command.ExecuteScalar();
                            account.storedPlaytime = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        }
                        account.loginTimestamp = DateTime.Now;
                        account.played_time = account.storedPlaytime;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Laden des Accounts: {ex.Message}");
                return false;
            }
        }

        private static bool CheckTrackingData(int accountId)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT COUNT(*) FROM player_tracking WHERE account_id=@account_id";
                            command.Parameters.AddWithValue("@account_id", accountId);
                            int count = Convert.ToInt32(command.ExecuteScalar());
                            return count > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Überprüfen der Tracking-Daten: {ex.Message}");
                return false;
            }
        }

        private static void CreateTrackingEntry(int accountId)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO player_tracking (account_id, last_health, last_armor, played_time, position_x, position_y, position_z)
                                VALUES (@account_id, @last_health, @last_armor, @played_time, @position_x, @position_y, @position_z)";
                            command.Parameters.AddWithValue("@account_id", accountId);
                            command.Parameters.AddWithValue("@last_health", 100);
                            command.Parameters.AddWithValue("@last_armor", 0);
                            command.Parameters.AddWithValue("@played_time", 0);
                            command.Parameters.AddWithValue("@position_x", 0.0f);
                            command.Parameters.AddWithValue("@position_y", 0.0f);
                            command.Parameters.AddWithValue("@position_z", 0.0f);
                            command.ExecuteNonQuery();
                            NAPI.Util.ConsoleOutput($"[INFO] Neue Tracking-Daten für Account-ID {accountId} erfolgreich erstellt.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Erstellen der Tracking-Daten für Account-ID {accountId}: {ex.Message}");
            }
        }

        public static void UpdateHardwareIdForExistingAccounts(Player player)
        {
            try
            {
                if (!player.HasData(Accounts.Account_Key))
                {
                    NAPI.Util.ConsoleOutput($"[FEHLER] Keine Acnt-Daten für Spieler {player.Name} gefunden.");
                    return;
                }
                Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
                if (account == null)
                {
                    NAPI.Util.ConsoleOutput($"[FEHLER] Account-Objekt ist null für Spieler {player.Name}.");
                    return;
                }
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT hardware_id, socialClubId FROM accounts WHERE id = @account_id";
                            command.Parameters.AddWithValue("@account_id", account.ID);
                            string existingHardwareId = null;
                            ulong existingSocialClubId = 0;
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Read();
                                    existingHardwareId = reader.IsDBNull(0) ? null : reader.GetString(0);
                                    existingSocialClubId = reader.IsDBNull(1) ? 0 : reader.GetUInt64("socialClubId");
                                }
                            }
                            if (string.IsNullOrEmpty(existingHardwareId))
                            {
                                command.CommandText = "UPDATE accounts SET hardware_id = @hardware_id WHERE id = @account_id";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@hardware_id", player.Serial);
                                command.Parameters.AddWithValue("@account_id", account.ID);
                                command.ExecuteNonQuery();
                                NAPI.Util.ConsoleOutput($"[INFO] Hardware-ID erfolgreich ergänzt für Account-ID {account.ID}: {player.Serial}");
                            }
                            if (existingSocialClubId == 0)
                            {
                                if (account.SocialClubId == 0)
                                {
                                    account.SocialClubId = player.SocialClubId;
                                }
                                command.CommandText = "UPDATE accounts SET socialClubId = @socialClubId WHERE id = @account_id";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@socialClubId", account.SocialClubId);
                                command.Parameters.AddWithValue("@account_id", account.ID);
                                command.ExecuteNonQuery();
                                NAPI.Util.ConsoleOutput($"[INFO] Social Club ID erfolgreich ergänzt für Account-ID {account.ID}: {player.SocialClubId}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Aktualisieren der Hardware-ID/Social Club ID: {ex.Message}");
            }
        }

        public static int GetAccountIdBySocialClubId(ulong socialClubId)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT id FROM accounts WHERE socialClubId = @socialClubId LIMIT 1";
                            command.Parameters.AddWithValue("@socialClubId", socialClubId);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows && reader.Read())
                                {
                                    return reader.GetInt32("id");
                                }
                            }
                        }
                        NAPI.Util.ConsoleOutput($"[WARNUNG] Kein Account für Social Club ID {socialClubId} gefunden.");
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Abrufen der Account-ID für SCID {socialClubId}: {ex.Message}");
                return 0;
            }
        }

        public static void LadeSpielerPositionAusDB(Player player, Accounts account)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                            SELECT position_x, position_y, position_z
                            FROM player_tracking
                            WHERE account_id = @id
                            LIMIT 1";
                            command.Parameters.AddWithValue("@id", account.ID);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows && reader.Read())
                                {
                                    float posX = reader.GetFloat("position_x");
                                    float posY = reader.GetFloat("position_y");
                                    float posZ = reader.GetFloat("position_z");
                                    player.Position = new Vector3(posX, posY, posZ);
                                }
                                else
                                {
                                    player.Position = new Vector3(276.828f, -580.471f, 43.113f);
                                    NAPI.Util.ConsoleOutput($"[WARNUNG] Keine gespeicherte Position für Spieler {account.Name} gefunden. Standardposition wird verwendet.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Laden der Position für Spieler {account.Name}: {ex.Message}");
            }
        }

        public static void AccountSpeichern(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Kein Account gefunden für {player.Name}");
                return;
            }
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                        UPDATE accounts 
                        SET adminlevel = @adminlevel, 
                            geld = @geld, 
                            fraktion = @fraktion, 
                            duty_start = @duty_start,
                            licenses = @licenses,
                            duty_offset = @duty_offset,last_logout_duty = @last_logout_duty,
                            CharacterData = @CharacterData
                        WHERE id = @id";
                            command.Parameters.AddWithValue("@adminlevel", account.Adminlevel);
                            command.Parameters.AddWithValue("@geld", account.Geld);
                            command.Parameters.AddWithValue("@fraktion", account.Fraktion);
                            command.Parameters.AddWithValue("@id", account.ID);
                            command.Parameters.AddWithValue("@duty_start", account.DutyStart.HasValue ? (object)account.DutyStart.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@duty_offset", account.DutyOffset);
                            command.Parameters.AddWithValue("@last_logout_duty", account.LastLogoutDuty.HasValue ? (object)account.LastLogoutDuty.Value : DBNull.Value); // NEU

                            command.Parameters.AddWithValue("@CharacterData", account.CharacterData);
                            command.Parameters.AddWithValue("@licenses", account.Licenses); // Speichere die Lizenzen
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Speichern von {player.Name}: {ex.Message}");
            }
        }

        public static void UpdatePlaytimeInDatabase(Accounts account)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            {
                                command.CommandText = "UPDATE player_tracking SET played_time = @played_time WHERE account_id = @id";
                                command.Parameters.AddWithValue("@played_time", account.played_time);
                                command.Parameters.AddWithValue("@id", account.ID);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Aktualisieren der Spielzeit in der Datenbank: {ex.Message}");
            }
        }

        public static void SpeichereSpielerPosition(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Kein Account gefunden für Spieler {player.Name}");
                return;
            }
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE player_tracking SET position_x=@posX, position_y=@posY, position_z=@posZ WHERE account_id=@id";
                            command.Parameters.AddWithValue("@posX", player.Position.X);
                            command.Parameters.AddWithValue("@posY", player.Position.Y);
                            command.Parameters.AddWithValue("@posZ", player.Position.Z);
                            command.Parameters.AddWithValue("@id", account.ID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Speichern der Position für {player.Name}: {ex.Message}");
            }
        }

        public static void SpielerBannen(int accountId, string grund)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE accounts SET is_banned = 1, ban_reason = @reason WHERE id = @account_id";
                            command.Parameters.AddWithValue("@account_id", accountId);
                            command.Parameters.AddWithValue("@reason", grund);
                            command.ExecuteNonQuery();
                            NAPI.Util.ConsoleOutput($"[INFO] Spieler mit Account-ID {accountId} wurde gebannt. Grund: {grund}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Bannen des Accounts mit ID {accountId}: {ex.Message}");
            }
        }

        public static void SpielerEntbannen(int accountId)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE accounts SET is_banned = 0, ban_reason = NULL WHERE id = @account_id";
                            command.Parameters.AddWithValue("@account_id", accountId);
                            command.ExecuteNonQuery();
                            NAPI.Util.ConsoleOutput($"[INFO] Spieler mit Account-ID {accountId} wurde entbannt.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Entbannen des Accounts mit ID {accountId}: {ex.Message}");
            }
        }

        public static bool IstSpielerGesperrt(string hardwareId)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT is_banned FROM accounts WHERE hardware_id = @hardware_id LIMIT 1";
                            command.Parameters.AddWithValue("@hardware_id", hardwareId);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows && reader.Read())
                                    return reader.GetBoolean("is_banned");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Überprüfen des Bannstatus für Hardware-ID {hardwareId}: {ex.Message}");
            }
            return false;
        }

        public static int GetRemainingTime(int accountId)
        {
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT remaining_time FROM player_tracking WHERE account_id = @id";
                        command.Parameters.AddWithValue("@id", accountId);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                                return reader.GetInt32("remaining_time");
                        }
                    }
                }
                return 0;
            }
        }

        public static int GetPlayedTime(int accountId)
        {
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT played_time FROM player_tracking WHERE account_id=@id LIMIT 1";
                        command.Parameters.AddWithValue("@id", accountId);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                                return reader.GetInt32("played_time");
                        }
                    }
                    return 0;
                }
            }
        }

        public static int GetLastPayday(int accountId)
        {
            int lastPayday = 0;
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT last_payday FROM player_tracking WHERE account_id = @id";
                        command.Parameters.AddWithValue("@id", accountId);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                lastPayday = reader.GetInt32("last_payday");
                            }
                        }
                    }
                    return lastPayday;
                }
            }
        }

        public static void UpdateLastPayday(int accountId, int currentSeconds)
        {
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE player_tracking SET last_payday = @currentSeconds WHERE account_id = @id";
                        command.Parameters.AddWithValue("@currentSeconds", currentSeconds);
                        command.Parameters.AddWithValue("@id", accountId);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static int GetLastInsertId()
        {
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT LAST_INSERT_ID()";
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
        }

        public static bool PasswordCheck(ulong socialClubId, string passwordInput)
        {
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                string hashedPassword = null;
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT password FROM accounts WHERE socialClubId = @socialClubId LIMIT 1";
                            command.Parameters.AddWithValue("@socialClubId", socialClubId);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows && reader.Read())
                                {
                                    hashedPassword = reader.GetString("password");
                                }
                            }
                        }
                    }
                    return hashedPassword != null && BCrypt.Net.BCrypt.Verify(passwordInput, hashedPassword);
                }
                catch (Exception ex)
                {
                    NAPI.Util.ConsoleOutput($"[FEHLER] Fehler im PasswordCheck für SCID {socialClubId}: {ex.Message}");
                    return false;
                }
            }
        }

        public static int GetAccountId(string playerName)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT id FROM accounts WHERE name = @name LIMIT 1";
                            command.Parameters.AddWithValue("@name", playerName);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows && reader.Read())
                                {
                                    return reader.GetInt32("id");
                                }
                            }
                        }
                        NAPI.Util.ConsoleOutput($"[WARNUNG] Kein Account für Spieler {playerName} gefunden.");
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Abrufen der Account-ID für {playerName}: {ex.Message}");
                return 0;
            }
        }

        public static void SetPlayerDeathStatus(Player player, int health, int armor)
        {
            try
            {
                if (!player.HasData(Accounts.Account_Key))
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Keine Account-Daten für Spieler {player.Name} gefunden. Todesstatus kann nicht gespeichert werden.");
                    return;
                }
                Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
                int accountId = account.ID;
                if (accountId == 0)
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Account-ID ist 0 für Spieler {player.Name}. Todesstatus kann nicht gespeichert werden.");
                    return;
                }
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                        UPDATE player_tracking
                        SET is_dead = 1,
                            last_health = @health,
                            last_armor = @armor,
                            remaining_time = 600000
                        WHERE account_id = @id";
                            command.Parameters.AddWithValue("@id", accountId);
                            command.Parameters.AddWithValue("@health", Math.Clamp(health, 0, 100));
                            command.Parameters.AddWithValue("@armor", Math.Clamp(armor, 0, 100));
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Setzen des Todesstatus für {player.Name}: {ex.Message}");
            }
        }

        #region Revive Logic

        private static void CoreRevivePlayer(Player player, Vector3 position, int newHealth, int newArmor, bool setPosition)
        {
            try
            {
                if (!player.HasData(Accounts.Account_Key))
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] CoreRevivePlayer: Keine Account-Daten für Spieler {player.Name}.");
                    return;
                }
                Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
                int accountId = account.ID;
                StopDeathTimer(player.SocialClubId);
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE player_tracking SET is_dead = 0, remaining_time = 0, last_health = @health, last_armor = @armor WHERE account_id = @id";
                            command.Parameters.AddWithValue("@id", accountId);
                            command.Parameters.AddWithValue("@health", newHealth);
                            command.Parameters.AddWithValue("@armor", newArmor);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                player.Health = newHealth;
                player.Armor = newArmor;
                player.StopAnimation();
                player.TriggerEvent("stopDeathEffect");
                player.Dimension = 0;
                if (setPosition)
                {
                    player.Position = position;
                }
                AccountSpeichern(player);
                SpeichereSpielerPosition(player);
                NAPI.Util.ConsoleOutput($"[REVIVE] Spieler {player.Name} (ID: {accountId}) wurde mit {newHealth} HP wiederbelebt.");
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Kritischer Fehler in CoreRevivePlayer für {player.Name}: {ex.Message}");
            }
        }

        public static void RevivePlayer(Player player, int newHealth, int newArmor)
        {
            CoreRevivePlayer(player, player.Position, newHealth, newArmor, false);
        }

        public static void RevivePlayer(Player player, Vector3 respawnPosition, int newHealth, int newArmor)
        {
            CoreRevivePlayer(player, respawnPosition, newHealth, newArmor, true);
        }

        #endregion

        public static void UpdatePlayerHealthInDb(int accountId, int newHealth, int newArmor)
        {
            try
            {
                if (accountId == 0) return;

                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE player_tracking SET last_health = @health, last_armor = @armor WHERE account_id = @id";
                            command.Parameters.AddWithValue("@id", accountId);
                            command.Parameters.AddWithValue("@health", newHealth);
                            command.Parameters.AddWithValue("@armor", newArmor);
                            command.ExecuteNonQuery();
                            NAPI.Util.ConsoleOutput($"[DB-FIX] Gesundheit für Account {accountId} auf {newHealth} HP gesetzt.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler bei UpdatePlayerHealthInDb für Account-ID {accountId}: {ex.Message}");
            }
        }
        public static void SavePlayerStatus(Player player)
        {
            try
            {
                if (!player.HasData(Accounts.Account_Key))
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Keine Account-Daten für Spieler {player.Name} gefunden. Status kann nicht gespeichert werden.");
                    return;
                }
                Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
                int accountId = account.ID;
                if (accountId == 0)
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Account-ID ist 0 für Spieler {player.Name}. Status kann nicht gespeichert werden.");
                    return;
                }
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE player_tracking SET last_health = @health, last_armor = @armor WHERE account_id = @id";
                            command.Parameters.AddWithValue("@id", accountId);
                            command.Parameters.AddWithValue("@health", player.Health);
                            command.Parameters.AddWithValue("@armor", player.Armor);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Speichern des Spielerstatus für {player.Name}: {ex.Message}");
            }
        }

        public static int LoadRemainingTimeFromDatabase(Player player)
        {
            int remainingTime = 600000;
            try
            {
                if (!player.HasData(Accounts.Account_Key))
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Keine Account-Daten für Spieler {player.Name} gefunden. Verbleibende Zeit kann nicht geladen werden.");
                    return 0;
                }
                Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
                int accountId = account.ID;
                lock (Datenbank.DbLock)
                {
                    string connectionString = Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT remaining_time FROM player_tracking WHERE account_id = @id";
                            command.Parameters.AddWithValue("@id", accountId);
                            object result = command.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                remainingTime = Convert.ToInt32(result);
                            }
                            NAPI.Util.ConsoleOutput($"[DB] Verbleibende Zeit für {player.Name} (SCID: {player.SocialClubId}) geladen: {remainingTime}ms");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Laden der verbleibenden Zeit für {player.Name}: {ex.Message}");
            }
            return remainingTime;
        }

        public static void StartDeathTimer(Player player, int remainingTime)
        {
            ulong socialClubId = player.SocialClubId;
            string playerNameForLog = player.Name;
            lock (TimerLock)
            {
                if (ActiveTimers.ContainsKey(socialClubId) && ActiveTimers[socialClubId])
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Timer für Spieler {playerNameForLog} (SCID: {socialClubId}) läuft bereits.");
                    return;
                }
                ActiveTimers[socialClubId] = true;
            }
            NAPI.Task.Run(async () =>
            {
                try
                {
                    while (player.Exists && remainingTime > 0)
                    {
                        bool timerActive;
                        lock (TimerLock)
                        {
                            timerActive = ActiveTimers.ContainsKey(socialClubId) && ActiveTimers[socialClubId];
                        }
                        if (!timerActive)
                        {
                            NAPI.Util.ConsoleOutput($"[INFO] Death-Timer für {playerNameForLog} (SCID: {socialClubId}) wurde abgebrochen.");
                            break;
                        }
                        remainingTime -= 1000;
                        SaveRemainingTime(player, remainingTime);
                        if (remainingTime <= 0)
                        {
                            RevivePlayer(player, AutomaticRespawnPosition, 65, 0);
                            break;
                        }
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    NAPI.Util.ConsoleOutput($"[FEHLER] Fehler im Timer von {playerNameForLog} (SCID: {socialClubId}): {ex.Message}");
                }
                finally
                {
                    lock (TimerLock)
                    {
                        if (ActiveTimers.ContainsKey(socialClubId))
                        {
                            ActiveTimers[socialClubId] = false;
                        }
                    }
                }
            });
        }

        public static bool IsPlayerDead(Player player)
        {
            try
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT is_dead FROM player_tracking WHERE account_id = @id LIMIT 1";
                        Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
                        command.Parameters.AddWithValue("@id", account.ID);
                        object result = command.ExecuteScalar();
                        return (result != null && Convert.ToBoolean(result));
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Laden des Todesstatus für {player.Name}: {ex.Message}");
                return false;
            }
        }

        public static void SaveRemainingTime(Player player, int remainingTime)
        {
            try
            {
                if (!player.HasData(Accounts.Account_Key))
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Keine Account-Daten für Spieler {player.Name} gefunden. Verbleibende Zeit kann nicht gespeichert werden.");
                    return;
                }
                Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
                int accountId = account.ID;
                if (accountId == 0)
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Account-ID ist 0 für Spieler {player.Name}. Verbleibende Zeit kann nicht gespeichert werden.");
                    return;
                }
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE player_tracking SET remaining_time = @remaining_time WHERE account_id = @id";
                            command.Parameters.AddWithValue("@id", accountId);
                            command.Parameters.AddWithValue("@remaining_time", remainingTime);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Speichern der verbleibenden Zeit für {player.Name}: {ex.Message}");
            }
        }

        public static void ResetPlayerDeathStatus(Player player)
        {
            try
            {
                if (!player.HasData(Accounts.Account_Key))
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Keine Account-Daten für Spieler {player.Name} gefunden. Todesstatus kann nicht zurückgesetzt werden.");
                    return;
                }
                Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
                int accountId = account.ID;
                if (accountId == 0)
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Account-ID ist 0 für Spieler {player.Name}. Todesstatus kann nicht zurückgesetzt werden.");
                    return;
                }
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE player_tracking SET is_dead = 0, remaining_time = 0 WHERE account_id = @id";
                            command.Parameters.AddWithValue("@id", accountId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Zurücksetzen des Todesstatus für {player.Name}: {ex.Message}");
            }
        }

        public static void StopDeathTimer(ulong socialClubId)
        {
            lock (TimerLock)
            {
                if (ActiveTimers.ContainsKey(socialClubId))
                {
                    ActiveTimers[socialClubId] = false;
                    NAPI.Util.ConsoleOutput($"[INFO] Timer für Spieler (SCID: {socialClubId}) wurde abgebrochen.");
                }
                else
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] Es wurde kein aktiver Timer für Spieler (SCID: {socialClubId}) gefunden, der abgebrochen werden könnte.");
                }
            }
        }

        public static List<Garage> Garages { get; private set; } = new List<Garage>();

        public static void LoadAllGarages()
        {
            Garages.Clear();
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM garages";
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32("id");
                                    string name = reader.GetString("name");
                                    float posX = reader.GetFloat("position_x");
                                    float posY = reader.GetFloat("position_y");
                                    float posZ = reader.GetFloat("position_z");
                                    float npcHeading = reader.GetFloat("npc_heading");
                                    int maxVehicles = reader.GetInt32("max_vehicles");
                                    int blipId = reader.GetInt32("blip_id");
                                    int blipColor = reader.GetInt32("blip_color");
                                    int fraktionId = 0;
                                    if (!reader.IsDBNull(reader.GetOrdinal("fraktionid")))
                                    {
                                        fraktionId = reader.GetInt32("fraktionid");
                                    }
                                    string pedModel = "a_m_m_business_01";
                                    if (!reader.IsDBNull(reader.GetOrdinal("ped_model")))
                                    {
                                        pedModel = reader.GetString("ped_model");
                                    }
                                    Garage garage = new Garage(id, name, posX, posY, posZ, npcHeading, maxVehicles, blipId, blipColor, fraktionId);
                                    Garages.Add(garage);
                                    NAPI.Util.ConsoleOutput($"[GARAGE] Geladen: {garage.Name} an {garage.Position} | Fraktion: {fraktionId}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Laden der Garagen: {ex.Message}");
                    }
                }
            }
        }

        public static List<Bankautomat> BankATMs { get; set; } = new List<Bankautomat>();

        public static void LoadBankATMs()
        {
            BankATMs.Clear();
            lock (DbLock)
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection localConnection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        localConnection.Open();
                        NAPI.Util.ConsoleOutput("[DB_ACCESS] LoadBankATMs: Verbindung geöffnet.");
                        string query = "SELECT id, name, posX, posY, posZ, dimension FROM bankautomaten";
                        using (MySqlCommand command = new MySqlCommand(query, localConnection))
                        {
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    NAPI.Util.ConsoleOutput("[BANKATM] Keine Bankautomaten in der Datenbank gefunden.");
                                }
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32("id");
                                    string name = reader.GetString("name");
                                    float posX = reader.GetFloat("posX");
                                    float posY = reader.GetFloat("posY");
                                    float posZ = reader.GetFloat("posZ");
                                    int dimension = reader.GetInt32("dimension");
                                    BankATMs.Add(new Bankautomat(id, name, posX, posY, posZ, dimension));
                                    NAPI.Util.ConsoleOutput($"[BANKATM] Geladen: {name} (ID: {id}) an Position: ({posX}, {posY}, {posZ}) in Dimension {dimension}");
                                }
                            }
                        }
                        NAPI.Util.ConsoleOutput($"[BANKATM] {BankATMs.Count} Bankautomaten erfolgreich geladen.");
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Laden der Bankautomaten: {ex.Message}");
                    }
                    finally
                    {
                        if (localConnection.State == System.Data.ConnectionState.Open)
                        {
                            localConnection.Close();
                            NAPI.Util.ConsoleOutput("[DB_ACCESS] LoadBankATMs: Verbindung geschlossen.");
                        }
                    }
                }
            }
        }

        // --- NEUE BANKMETHODEN ---

        public static async Task<PlayerBankAccount> GetOrCreatePlayerBankAccount(int playerId)
        {
            PlayerBankAccount acc = await GetBankAccountByPlayerIdAsync(playerId);
            if (acc == null)
            {
                string newKontoNummer = await GenerateUniqueKontonummerAsync();
                // Initiales Guthaben auf 2500 setzen, wie in der alten Logik angedeutet
                bool success = await CreateBankAccountAsync(playerId, newKontoNummer, 2500m);
                if (success)
                {
                    NAPI.Util.ConsoleOutput($"[BANKING] Neues Bankkonto {newKontoNummer} für Spieler-ID {playerId} erstellt.");
                    return new PlayerBankAccount { PlayerId = playerId, Kontonummer = newKontoNummer, Kontostand = 2500m };
                }
                else
                {
                    NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Erstellen des Bankkontos für Spieler-ID {playerId}");
                    return null;
                }
            }
            return acc;
        }

        private static async Task<string> GenerateUniqueKontonummerAsync()
        {
            Random rand = new Random();
            string kontoNummer;
            bool isUnique;
            int attempts = 0;
            do
            {
                kontoNummer = rand.Next(100_000_000, 1_000_000_000).ToString();
                string connectionString = GetConnectionString();
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM bankkonten WHERE kontonummer = @kontonummer";
                        command.Parameters.AddWithValue("@kontonummer", kontoNummer);
                        long count = (long)await command.ExecuteScalarAsync();
                        isUnique = count == 0;
                    }
                }
                attempts++;
                if (attempts > 100) throw new Exception("Konnte nach 100 Versuchen keine eindeutige Kontonummer generieren.");
            } while (!isUnique);
            return kontoNummer;
        }

        public static async Task<PlayerBankAccount> GetBankAccountByPlayerIdAsync(int playerId)
        {
            PlayerBankAccount account = null;
            string connectionString = GetConnectionString();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT kontonummer, kontostand FROM bankkonten WHERE player_id = @playerId LIMIT 1";
                        command.Parameters.AddWithValue("@playerId", playerId);
                        using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                account = new PlayerBankAccount
                                {
                                    PlayerId = playerId,
                                    Kontonummer = reader.GetString("kontonummer"),
                                    Kontostand = reader.GetDecimal("kontostand")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] GetBankAccountByPlayerIdAsync: {ex.Message}");
            }
            return account;
        }

        public static async Task<PlayerBankAccount> GetBankAccountByKontonummerAsync(string kontonummer)
        {
            PlayerBankAccount account = null;
            string connectionString = GetConnectionString();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT player_id, kontostand FROM bankkonten WHERE kontonummer = @kontonummer LIMIT 1";
                        command.Parameters.AddWithValue("@kontonummer", kontonummer);
                        using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                account = new PlayerBankAccount
                                {
                                    PlayerId = reader.GetInt32("player_id"),
                                    Kontonummer = kontonummer,
                                    Kontostand = reader.GetDecimal("kontostand")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] GetBankAccountByKontonummerAsync: {ex.Message}");
            }
            return account;
        }

        public static async Task<bool> CreateBankAccountAsync(int playerId, string kontonummer, decimal initialBalance)
        {
            string connectionString = GetConnectionString();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO bankkonten (player_id, kontonummer, kontostand, creation_date) VALUES (@playerId, @kontonummer, @kontostand, NOW())";
                        command.Parameters.AddWithValue("@playerId", playerId);
                        command.Parameters.AddWithValue("@kontonummer", kontonummer);
                        command.Parameters.AddWithValue("@kontostand", initialBalance);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] CreateBankAccountAsync: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateBankAccountBalanceAsync(string kontonummer, decimal newBalance)
        {
            string connectionString = GetConnectionString();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE bankkonten SET kontostand = @newBalance WHERE kontonummer = @kontonummer";
                        command.Parameters.AddWithValue("@newBalance", newBalance);
                        command.Parameters.AddWithValue("@kontonummer", kontonummer);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] UpdateBankAccountBalanceAsync: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateAccountMoneyAsync(int accountId, int newMoney)
        {
            string connectionString = GetConnectionString();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE accounts SET geld = @newMoney WHERE id = @accountId";
                        command.Parameters.AddWithValue("@newMoney", newMoney);
                        command.Parameters.AddWithValue("@accountId", accountId);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] UpdateAccountMoneyAsync: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> LogTransactionAsync(int playerId, string kontonummer, string type, decimal amount, string targetKontonummer = null, string description = null)
        {
            string connectionString = GetConnectionString();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        INSERT INTO bank_transactions (player_id, kontonummer, type, amount, target_kontonummer, description, transaction_date)
                        VALUES (@player_id, @kontonummer, @type, @amount, @target_kontonummer, @description, NOW())";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@player_id", playerId);
                        command.Parameters.AddWithValue("@kontonummer", kontonummer);
                        command.Parameters.AddWithValue("@type", type);
                        command.Parameters.AddWithValue("@amount", amount);
                        command.Parameters.AddWithValue("@target_kontonummer", (object)targetKontonummer ?? DBNull.Value);
                        command.Parameters.AddWithValue("@description", (object)description ?? DBNull.Value);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] LogTransactionAsync: {ex.Message}");
                return false;
            }
        }

        public static async Task<List<TransactionData>> GetTransactionHistoryAsync(string kontonummer)
        {
            var history = new List<TransactionData>();
            string connectionString = GetConnectionString();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    // Diese Abfrage holt alle Transaktionen, bei denen das Konto entweder Sender oder Empfänger war
                    string query = @"
                        (SELECT 
                            'transfer' as main_type, 
                            -bt.amount as amount, 
                            bt.target_kontonummer, 
                            bt.description, 
                            bt.transaction_date, 
                            bt.kontonummer as source_kontonummer 
                         FROM bank_transactions bt 
                         WHERE bt.kontonummer = @kontonummer AND bt.type = 'transfer')
                        UNION ALL
                        (SELECT 
                            'transfer' as main_type, 
                            bt.amount as amount, 
                            bt.target_kontonummer, 
                            bt.description, 
                            bt.transaction_date, 
                            bt.kontonummer as source_kontonummer 
                         FROM bank_transactions bt 
                         WHERE bt.target_kontonummer = @kontonummer AND bt.type = 'transfer')
                        UNION ALL
                        (SELECT 
                            bt.type as main_type, 
                            IF(bt.type = 'deposit', bt.amount, -bt.amount) as amount, 
                            NULL as target_kontonummer, 
                            bt.description, 
                            bt.transaction_date, 
                            NULL as source_kontonummer 
                         FROM bank_transactions bt 
                         WHERE bt.kontonummer = @kontonummer AND bt.type IN ('deposit', 'withdraw', 'system_charge'))
                        ORDER BY transaction_date DESC
                        LIMIT 15";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@kontonummer", kontonummer);
                        using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var mainType = reader.GetString("main_type");
                                var amount = reader.GetDecimal("amount");
                                var type = mainType;

                                if (mainType == "transfer")
                                {
                                    type = amount < 0 ? "transfer_sent" : "transfer_received";
                                }

                                history.Add(new TransactionData
                                {
                                    Type = type,
                                    Amount = amount,
                                    TargetKontonummer = reader.IsDBNull(reader.GetOrdinal("target_kontonummer")) ? null : reader.GetString("target_kontonummer"),
                                    SourceKontonummer = reader.IsDBNull(reader.GetOrdinal("source_kontonummer")) ? null : reader.GetString("source_kontonummer"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString("description"),
                                    TransactionDate = reader.GetDateTime("transaction_date")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] GetTransactionHistoryAsync: {ex.Message}");
            }
            return history;
        }

        public static async Task<bool> CreateSystemTransaction(string targetKontonummer, decimal amount, string description)
        {
            // Betrag muss positiv sein für die Logik
            if (amount < 0) amount = Math.Abs(amount);

            PlayerBankAccount targetAccount = await GetBankAccountByKontonummerAsync(targetKontonummer);
            if (targetAccount == null)
            {
                NAPI.Util.ConsoleOutput($"[SYSTEM_TX_ERROR] Konto {targetKontonummer} nicht gefunden.");
                return false;
            }

            targetAccount.Kontostand -= amount; // Betrag wird immer abgezogen
            bool balanceUpdated = await UpdateBankAccountBalanceAsync(targetAccount.Kontonummer, targetAccount.Kontostand);

            // Logge diese "System-Transaktion"
            bool logged = await LogTransactionAsync(
                targetAccount.PlayerId,
                targetAccount.Kontonummer,
                "system_charge", // Eigener Typ für System-Belastungen
                amount, // Logge den positiven Betrag der Belastung
                null,
                description
            );

            if (balanceUpdated && logged)
            {
                NAPI.Util.ConsoleOutput($"[SYSTEM_TX] {description} ({amount}€) für Konto {targetKontonummer} erfolgreich verbucht.");

                // Spieler benachrichtigen, wenn online
                Player targetPlayer = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).ID == targetAccount.PlayerId);
                if (targetPlayer != null)
                {
                    targetPlayer.SendNotification($"~r~Ihr Konto wurde mit {amount}€ belastet. Grund: {description}");
                }
                return true;
            }
            return false;
        }
        #region Support System Methods

        public static void CreateSupportTicket(int reporterId, string reporterName, string message, Vector3 position)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "INSERT INTO support_tickets (reporter_id, reporter_name, message, pos_x, pos_y, pos_z, report_date, status) VALUES (@reporter_id, @reporter_name, @message, @pos_x, @pos_y, @pos_z, NOW(), 'Offen')";
                            command.Parameters.AddWithValue("@reporter_id", reporterId);
                            command.Parameters.AddWithValue("@reporter_name", reporterName);
                            command.Parameters.AddWithValue("@message", message);
                            command.Parameters.AddWithValue("@pos_x", position.X);
                            command.Parameters.AddWithValue("@pos_y", position.Y);
                            command.Parameters.AddWithValue("@pos_z", position.Z);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Support-Ticket konnte nicht erstellt werden: {ex.Message}");
            }
        }

        public static List<SupportTicket> GetOpenSupportTickets()
        {
            List<SupportTicket> tickets = new List<SupportTicket>();
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM support_tickets WHERE status != 'Geschlossen' ORDER BY is_priority DESC, report_date ASC";
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    tickets.Add(new SupportTicket
                                    {
                                        Id = reader.GetInt32("id"),
                                        ReporterId = reader.GetInt32("reporter_id"),
                                        ReporterName = reader.GetString("reporter_name"),
                                        Message = reader.GetString("message"),
                                        PosX = reader.GetFloat("pos_x"),
                                        PosY = reader.GetFloat("pos_y"),
                                        PosZ = reader.GetFloat("pos_z"),
                                        ReportDate = reader.GetDateTime("report_date"),
                                        Status = reader.GetString("status"),
                                        IsPriority = reader.GetBoolean("is_priority"),
                                        AdminComment = reader.IsDBNull(reader.GetOrdinal("admin_comment")) ? null : reader.GetString("admin_comment")
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Offene Support-Tickets konnten nicht geladen werden: {ex.Message}");
            }
            return tickets;
        }

        public static void UpdateTicketStatus(int ticketId, string newStatus, int adminId)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE support_tickets SET status = @newStatus, handled_by_admin_id = @adminId, handled_date = NOW() WHERE id = @ticketId";
                            command.Parameters.AddWithValue("@newStatus", newStatus);
                            command.Parameters.AddWithValue("@adminId", adminId);
                            command.Parameters.AddWithValue("@ticketId", ticketId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Support-Ticket-Status konnte nicht aktualisiert werden: {ex.Message}");
            }
        }

        public static void UpdateTicketPriority(int ticketId, bool isPriority)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE support_tickets SET is_priority = @isPriority WHERE id = @ticketId";
                            command.Parameters.AddWithValue("@isPriority", isPriority);
                            command.Parameters.AddWithValue("@ticketId", ticketId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Support-Ticket-Priorität konnte nicht aktualisiert werden: {ex.Message}");
            }
        }

        public static void AddTicketComment(int ticketId, string adminName, string newCommentText)
        {
            if (string.IsNullOrWhiteSpace(newCommentText)) return;

            try
            {
                lock (DbLock)
                {
                    string connectionString = GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        string existingCommentsJson = null;
                        using (MySqlCommand readCommand = connection.CreateCommand())
                        {
                            readCommand.CommandText = "SELECT admin_comment FROM support_tickets WHERE id = @ticketId";
                            readCommand.Parameters.AddWithValue("@ticketId", ticketId);
                            object result = readCommand.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                existingCommentsJson = result.ToString();
                            }
                        }

                        List<AdminCommentEntry> comments;
                        try
                        {
                            // Versuche, bestehende Kommentare als JSON-Liste zu lesen
                            comments = JsonConvert.DeserializeObject<List<AdminCommentEntry>>(existingCommentsJson);
                            if (comments == null) comments = new List<AdminCommentEntry>();
                        }
                        catch
                        {
                            // FEHLER BEIM LESEN? Dann ist es alter Text!
                            // Wir erstellen eine neue Liste und fügen den alten Text als ersten "System"-Kommentar hinzu.
                            comments = new List<AdminCommentEntry>();
                            if (!string.IsNullOrEmpty(existingCommentsJson))
                            {
                                comments.Add(new AdminCommentEntry
                                {
                                    Timestamp = "Früher",
                                    AdminName = "System",
                                    Text = existingCommentsJson
                                });
                            }
                        }

                        // Füge den wirklich neuen Kommentar hinzu
                        comments.Add(new AdminCommentEntry
                        {
                            Timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                            AdminName = adminName,
                            Text = newCommentText
                        });

                        string updatedCommentsJson = JsonConvert.SerializeObject(comments);

                        using (MySqlCommand updateCommand = connection.CreateCommand())
                        {
                            updateCommand.CommandText = "UPDATE support_tickets SET admin_comment = @comment WHERE id = @ticketId";
                            // Explizit den Typ auf JSON setzen, um Treiberprobleme zu vermeiden
                            var param = new MySqlParameter("@comment", MySqlDbType.JSON) { Value = updatedCommentsJson };
                            updateCommand.Parameters.Add(param);
                            updateCommand.Parameters.AddWithValue("@ticketId", ticketId);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Support-Ticket-Kommentar konnte nicht hinzugefügt werden: {ex.Message}");
            }
        }

        #endregion
    }
}
