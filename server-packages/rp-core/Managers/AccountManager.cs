using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using RPCore.Models.Account;
using RPCore.Models.Permission;
using RPCore.Database;
using MySql.Data.MySqlClient;

namespace RPCore.Managers
{
    /// <summary>
    /// Account Manager - Verwaltet Spieler-Accounts (Login, Registration, etc.)
    /// HINWEIS: 1 Charakter pro Account - Charakter-Daten sind im Account integriert
    /// </summary>
    public class AccountManager
    {
        private static AccountManager? _instance;
        public static AccountManager Instance => _instance ??= new AccountManager();

        private Dictionary<string, Account> _loadedAccounts;
        private Dictionary<int, PlayerPermission> _permissions;

        // Player -> Account Mapping
        private Dictionary<GTANetworkAPI.Player, Account> _playerAccounts;

        private AccountManager()
        {
            _loadedAccounts = new Dictionary<string, Account>();
            _permissions = new Dictionary<int, PlayerPermission>();
            _playerAccounts = new Dictionary<GTANetworkAPI.Player, Account>();
        }

        /// <summary>
        /// Lädt einen Account (inkl. Charakter-Daten) aus der Datenbank
        /// </summary>
        public async Task<Account?> LoadAccount(string username)
        {
            // Cache-Check
            if (_loadedAccounts.ContainsKey(username.ToLower()))
            {
                return _loadedAccounts[username.ToLower()];
            }

            // Datenbank-Abfrage mit Prepared Statement
            string query = "SELECT * FROM accounts WHERE username = @username LIMIT 1";
            var reader = await DatabaseManager.Instance.ExecuteReader(query,
                DatabaseManager.CreateParameter("@username", username));

            if (reader == null || !await reader.ReadAsync())
            {
                reader?.Close();
                NAPI.Util.ConsoleOutput($"[AccountManager] Account '{username}' nicht gefunden");
                return null;
            }

            var account = new Account
            {
                // Account Daten
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                PasswordHash = reader.GetString("password_hash"),
                SocialClubName = reader.IsDBNull(reader.GetOrdinal("social_club_name")) ? "" : reader.GetString("social_club_name"),
                HardwareId = reader.IsDBNull(reader.GetOrdinal("hardware_id")) ? "" : reader.GetString("hardware_id"),

                // Charakter Daten (1 Charakter pro Account)
                FirstName = reader.GetString("first_name"),
                LastName = reader.GetString("last_name"),
                Cash = reader.GetInt32("cash"),
                BankMoney = reader.GetInt32("bank_money"),
                Level = reader.GetInt32("level"),
                Experience = reader.GetInt32("experience"),
                PlayTimeMinutes = reader.GetInt32("play_time"),
                Job = reader.IsDBNull(reader.GetOrdinal("job")) ? "Arbeitslos" : reader.GetString("job"),
                FactionId = reader.IsDBNull(reader.GetOrdinal("faction_id")) ? null : reader.GetInt32("faction_id"),
                FactionRank = reader.GetInt32("faction_rank"),

                // Position
                LastPosition = new Vector3(
                    reader.GetFloat("last_position_x"),
                    reader.GetFloat("last_position_y"),
                    reader.GetFloat("last_position_z")
                ),
                LastRotation = reader.GetFloat("last_rotation"),
                Dimension = reader.GetInt32("dimension"),

                // Status
                Health = reader.GetInt32("health"),
                Armor = reader.GetInt32("armor"),
                IsAlive = reader.GetBoolean("is_alive"),
                IsInjured = reader.GetBoolean("is_injured"),
                Appearance = reader.IsDBNull(reader.GetOrdinal("appearance")) ? "" : reader.GetString("appearance"),

                // Admin & Ban
                AdminLevel = reader.GetInt32("admin_level"),
                IsBanned = reader.GetBoolean("is_banned"),
                BanReason = reader.IsDBNull(reader.GetOrdinal("ban_reason")) ? "" : reader.GetString("ban_reason"),
                BanExpiry = reader.IsDBNull(reader.GetOrdinal("ban_expiry")) ? null : reader.GetDateTime("ban_expiry"),

                // Zeitstempel
                CreatedAt = reader.GetDateTime("created_at"),
                LastLogin = reader.IsDBNull(reader.GetOrdinal("last_login")) ? DateTime.Now : reader.GetDateTime("last_login")
            };

            reader.Close();

            _loadedAccounts[username.ToLower()] = account;
            NAPI.Util.ConsoleOutput($"[AccountManager] Account '{username}' ({account.FullName}) geladen (ID: {account.Id})");
            return account;
        }

        /// <summary>
        /// Lädt Permission für einen Account (Legacy - jetzt in Account.AdminLevel integriert)
        /// </summary>
        [Obsolete("Permission ist jetzt im Account.AdminLevel integriert")]
        private async Task LoadPermission(int accountId)
        {
            // Legacy-Unterstützung für alte player_permissions Tabelle
            string query = "SELECT * FROM player_permissions WHERE account_id = @accountId LIMIT 1";
            var reader = await DatabaseManager.Instance.ExecuteReader(query,
                DatabaseManager.CreateParameter("@accountId", accountId));

            if (reader != null && await reader.ReadAsync())
            {
                var permission = new PlayerPermission
                {
                    Id = reader.GetInt32("id"),
                    AccountId = reader.GetInt32("account_id"),
                    Level = (PermissionLevel)reader.GetInt32("permission_level")
                };

                _permissions[accountId] = permission;
                reader.Close();
            }
            else
            {
                reader?.Close();
                // Erstelle Standard-Permission wenn keine existiert
                await CreateDefaultPermission(accountId);
            }
        }

        /// <summary>
        /// Erstellt Standard-Permission (Spieler)
        /// </summary>
        private async Task CreateDefaultPermission(int accountId)
        {
            string query = "INSERT INTO player_permissions (account_id, permission_level) VALUES (@accountId, @level)";
            long permId = await DatabaseManager.Instance.ExecuteInsert(query,
                DatabaseManager.CreateParameter("@accountId", accountId),
                DatabaseManager.CreateParameter("@level", (int)PermissionLevel.Spieler));

            if (permId > 0)
            {
                _permissions[accountId] = new PlayerPermission
                {
                    Id = (int)permId,
                    AccountId = accountId,
                    Level = PermissionLevel.Spieler
                };
                NAPI.Util.ConsoleOutput($"[AccountManager] Standard-Permission erstellt für Account {accountId}");
            }
        }

        /// <summary>
        /// Erstellt einen neuen Account (inkl. Charakter - 1 Charakter pro Account)
        /// </summary>
        public async Task<Account?> CreateAccount(string username, string passwordHash,
            string socialClubName, string hardwareId, string firstName, string lastName)
        {
            // Überprüfung ob Username bereits existiert
            if (await AccountExists(username))
            {
                NAPI.Util.ConsoleOutput($"[AccountManager] Account '{username}' existiert bereits!");
                return null;
            }

            // Nur prüfen, wenn ein Vor- und Nachname übergeben wurde (Charakter-Erstellung später)
            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                if (await CharacterNameExists(firstName, lastName))
                {
                    NAPI.Util.ConsoleOutput($"[AccountManager] Charaktername '{firstName} {lastName}' existiert bereits!");
                    return null;
                }
            }

            string query = @"INSERT INTO accounts 
                (username, password_hash, social_club_name, hardware_id, 
                 first_name, last_name, created_at) 
                VALUES 
                (@username, @password, @socialclub, @hwid, 
                 @firstName, @lastName, @created)";

            long accountId = await DatabaseManager.Instance.ExecuteInsert(query,
                DatabaseManager.CreateParameter("@username", username),
                DatabaseManager.CreateParameter("@password", passwordHash),
                DatabaseManager.CreateParameter("@socialclub", socialClubName),
                DatabaseManager.CreateParameter("@hwid", hardwareId),
                DatabaseManager.CreateParameter("@firstName", firstName),
                DatabaseManager.CreateParameter("@lastName", lastName),
                DatabaseManager.CreateParameter("@created", DateTime.Now));

            if (accountId == 0)
            {
                NAPI.Util.ConsoleOutput($"[AccountManager] Fehler beim Erstellen von Account '{username}'");
                return null;
            }

            var account = new Account
            {
                Id = (int)accountId,
                Username = username,
                PasswordHash = passwordHash,
                SocialClubName = socialClubName,
                HardwareId = hardwareId,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.Now
            };

            _loadedAccounts[username.ToLower()] = account;

            NAPI.Util.ConsoleOutput($"[AccountManager] Account '{username}' ({account.FullName}) wurde erstellt (ID: {accountId})");
            return account;
        }

        /// <summary>
        /// Legacy CreateAccount ohne Charakter-Daten (ruft neue Methode auf)
        /// </summary>
        [Obsolete("Account-Erstellung setzt keine Charakternamen mehr; Character-Creation übernimmt später")]
        public async Task<Account?> CreateAccount(string username, string passwordHash, string socialClubName, string hardwareId)
        {
            // Legacy-Aufruf: lege Account ohne Charakter-Namen an (weder speichern noch vorausfüllen)
            return await CreateAccount(username, passwordHash, socialClubName, hardwareId, string.Empty, string.Empty);
        }

        /// <summary>
        /// Überprüft ob ein Charaktername bereits existiert
        /// </summary>
        public async Task<bool> CharacterNameExists(string firstName, string lastName)
        {
            string query = "SELECT COUNT(*) FROM accounts WHERE first_name = @firstName AND last_name = @lastName";
            var result = await DatabaseManager.Instance.ExecuteScalar(query,
                DatabaseManager.CreateParameter("@firstName", firstName),
                DatabaseManager.CreateParameter("@lastName", lastName));

            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Überprüft ob ein Account existiert
        /// </summary>
        public async Task<bool> AccountExists(string username)
        {
            string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
            var result = await DatabaseManager.Instance.ExecuteScalar(query,
                DatabaseManager.CreateParameter("@username", username));

            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Authentifiziert einen Account mit Passwort
        /// </summary>
        public async Task<bool> AuthenticateAccount(string username, string passwordHash)
        {
            var account = await LoadAccount(username);
            if (account == null) return false;

            bool success = account.PasswordHash == passwordHash;

            if (success)
            {
                await UpdateLastLogin(account.Id);
            }

            return success;
        }

        /// <summary>
        /// Aktualisiert den LastLogin Timestamp
        /// </summary>
        public async Task UpdateLastLogin(int accountId)
        {
            var account = _loadedAccounts.Values.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
            {
                account.LastLogin = DateTime.Now;

                string query = "UPDATE accounts SET last_login = @lastLogin WHERE id = @accountId";
                await DatabaseManager.Instance.ExecuteNonQuery(query,
                    DatabaseManager.CreateParameter("@lastLogin", DateTime.Now),
                    DatabaseManager.CreateParameter("@accountId", accountId));
            }
        }

        /// <summary>
        /// Bannt einen Account
        /// </summary>
        public async Task BanAccount(int accountId, string reason, DateTime? expiry = null)
        {
            var account = _loadedAccounts.Values.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
            {
                account.IsBanned = true;
                account.BanReason = reason;
                account.BanExpiry = expiry;

                string query = "UPDATE accounts SET is_banned = 1, ban_reason = @reason, ban_expiry = @expiry WHERE id = @accountId";
                await DatabaseManager.Instance.ExecuteNonQuery(query,
                    DatabaseManager.CreateParameter("@reason", reason),
                    DatabaseManager.CreateParameter("@expiry", expiry),
                    DatabaseManager.CreateParameter("@accountId", accountId));

                NAPI.Util.ConsoleOutput($"[AccountManager] Account {account.Username} wurde gebannt: {reason}");
            }
        }

        /// <summary>
        /// Entbannt einen Account
        /// </summary>
        public async Task UnbanAccount(int accountId)
        {
            var account = _loadedAccounts.Values.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
            {
                account.IsBanned = false;
                account.BanReason = "";
                account.BanExpiry = null;

                string query = "UPDATE accounts SET is_banned = 0, ban_reason = NULL, ban_expiry = NULL WHERE id = @accountId";
                await DatabaseManager.Instance.ExecuteNonQuery(query,
                    DatabaseManager.CreateParameter("@accountId", accountId));

                NAPI.Util.ConsoleOutput($"[AccountManager] Account {account.Username} wurde entbannt");
            }
        }

        /// <summary>
        /// Holt die Permission für einen Account
        /// </summary>
        public PlayerPermission? GetPermission(int accountId)
        {
            if (_permissions.ContainsKey(accountId))
            {
                return _permissions[accountId];
            }

            return null;
        }

        /// <summary>
        /// Setzt die Permission für einen Account
        /// </summary>
        public async Task SetPermission(int accountId, PermissionLevel level, string grantedBy)
        {
            // Neues System: Permission ist in Account.AdminLevel integriert
            var account = _loadedAccounts.Values.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
            {
                account.AdminLevel = (int)level;

                string query = "UPDATE accounts SET admin_level = @level WHERE id = @accountId";
                await DatabaseManager.Instance.ExecuteNonQuery(query,
                    DatabaseManager.CreateParameter("@level", (int)level),
                    DatabaseManager.CreateParameter("@accountId", accountId));

                NAPI.Util.ConsoleOutput($"[AccountManager] AdminLevel für Account {account.Username} auf {level} gesetzt");
            }
        }

        /// <summary>
        /// Speichert einen Account (inkl. alle Charakter-Daten) in die Datenbank
        /// </summary>
        public async Task SaveAccount(Account account)
        {
            string query = @"UPDATE accounts SET 
                -- Charakter Daten
                cash = @cash, 
                bank_money = @bankMoney, 
                level = @level, 
                experience = @experience, 
                play_time = @playTime,
                job = @job,
                faction_id = @factionId,
                faction_rank = @factionRank,
                
                -- Position
                last_position_x = @posX, 
                last_position_y = @posY, 
                last_position_z = @posZ,
                last_rotation = @rotation,
                dimension = @dimension,
                
                -- Status
                health = @health,
                armor = @armor,
                is_alive = @isAlive,
                is_injured = @isInjured,
                appearance = @appearance,
                
                -- Timestamps
                last_login = @lastLogin
                
                WHERE id = @accountId";

            await DatabaseManager.Instance.ExecuteNonQuery(query,
                // Charakter Daten
                DatabaseManager.CreateParameter("@cash", account.Cash),
                DatabaseManager.CreateParameter("@bankMoney", account.BankMoney),
                DatabaseManager.CreateParameter("@level", account.Level),
                DatabaseManager.CreateParameter("@experience", account.Experience),
                DatabaseManager.CreateParameter("@playTime", account.PlayTimeMinutes),
                DatabaseManager.CreateParameter("@job", account.Job),
                DatabaseManager.CreateParameter("@factionId", account.FactionId),
                DatabaseManager.CreateParameter("@factionRank", account.FactionRank),

                // Position
                DatabaseManager.CreateParameter("@posX", account.LastPosition.X),
                DatabaseManager.CreateParameter("@posY", account.LastPosition.Y),
                DatabaseManager.CreateParameter("@posZ", account.LastPosition.Z),
                DatabaseManager.CreateParameter("@rotation", account.LastRotation),
                DatabaseManager.CreateParameter("@dimension", account.Dimension),

                // Status
                DatabaseManager.CreateParameter("@health", account.Health),
                DatabaseManager.CreateParameter("@armor", account.Armor),
                DatabaseManager.CreateParameter("@isAlive", account.IsAlive),
                DatabaseManager.CreateParameter("@isInjured", account.IsInjured),
                DatabaseManager.CreateParameter("@appearance", account.Appearance ?? ""),

                // Timestamps
                DatabaseManager.CreateParameter("@lastLogin", DateTime.Now),
                DatabaseManager.CreateParameter("@accountId", account.Id));

            NAPI.Util.ConsoleOutput($"[AccountManager] Account '{account.Username}' ({account.FullName}) gespeichert");
        }

        /// <summary>
        /// Holt einen Account für einen Spieler
        /// </summary>
        public Account? GetAccountByPlayer(GTANetworkAPI.Player player)
        {
            if (_playerAccounts.ContainsKey(player))
            {
                return _playerAccounts[player];
            }
            return null;
        }

        /// <summary>
        /// Setzt den Account für einen Spieler
        /// </summary>
        public void SetPlayerAccount(GTANetworkAPI.Player player, Account account)
        {
            _playerAccounts[player] = account;
        }

        /// <summary>
        /// Entfernt einen Spieler aus dem Mapping
        /// </summary>
        public void RemovePlayerAccount(GTANetworkAPI.Player player)
        {
            if (_playerAccounts.ContainsKey(player))
            {
                _playerAccounts.Remove(player);
            }
        }
    }
}
