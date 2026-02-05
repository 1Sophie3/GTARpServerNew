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
    /// </summary>
    public class AccountManager
    {
        private static AccountManager? _instance;
        public static AccountManager Instance => _instance ??= new AccountManager();

        private Dictionary<string, Account> _loadedAccounts;
        private Dictionary<int, PlayerPermission> _permissions;

        private AccountManager()
        {
            _loadedAccounts = new Dictionary<string, Account>();
            _permissions = new Dictionary<int, PlayerPermission>();
        }

        /// <summary>
        /// Lädt einen Account aus der Datenbank
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
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                PasswordHash = reader.GetString("password_hash"),
                Email = reader.GetString("email"),
                SocialClubName = reader.IsDBNull(reader.GetOrdinal("socialclub_name")) ? "" : reader.GetString("socialclub_name"),
                HardwareId = reader.IsDBNull(reader.GetOrdinal("hardware_id")) ? "" : reader.GetString("hardware_id"),
                IsBanned = reader.GetBoolean("is_banned"),
                BanReason = reader.IsDBNull(reader.GetOrdinal("ban_reason")) ? "" : reader.GetString("ban_reason"),
                BanExpiry = reader.IsDBNull(reader.GetOrdinal("ban_expiry")) ? null : reader.GetDateTime("ban_expiry"),
                CreatedAt = reader.GetDateTime("created_at"),
                LastLogin = reader.IsDBNull(reader.GetOrdinal("last_login")) ? DateTime.Now : reader.GetDateTime("last_login")
            };

            reader.Close();

            // Lade Permission
            await LoadPermission(account.Id);

            _loadedAccounts[username.ToLower()] = account;
            NAPI.Util.ConsoleOutput($"[AccountManager] Account '{username}' geladen (ID: {account.Id})");
            return account;
        }

        /// <summary>
        /// Lädt Permission für einen Account
        /// </summary>
        private async Task LoadPermission(int accountId)
        {
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
        /// Erstellt einen neuen Account
        /// </summary>
        public async Task<Account?> CreateAccount(string username, string passwordHash, string email, string socialClubName, string hardwareId)
        {
            // Überprüfung ob Username bereits existiert
            if (await AccountExists(username))
            {
                NAPI.Util.ConsoleOutput($"[AccountManager] Account '{username}' existiert bereits!");
                return null;
            }

            string query = "INSERT INTO accounts (username, password_hash, email, socialclub_name, hardware_id, created_at) " +
                          "VALUES (@username, @password, @email, @socialclub, @hwid, @created)";

            long accountId = await DatabaseManager.Instance.ExecuteInsert(query,
                DatabaseManager.CreateParameter("@username", username),
                DatabaseManager.CreateParameter("@password", passwordHash),
                DatabaseManager.CreateParameter("@email", email),
                DatabaseManager.CreateParameter("@socialclub", socialClubName),
                DatabaseManager.CreateParameter("@hwid", hardwareId),
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
                Email = email,
                SocialClubName = socialClubName,
                HardwareId = hardwareId,
                CreatedAt = DateTime.Now
            };

            _loadedAccounts[username.ToLower()] = account;

            // Erstelle Standard-Permission
            await CreateDefaultPermission(account.Id);

            NAPI.Util.ConsoleOutput($"[AccountManager] Account '{username}' wurde erstellt (ID: {accountId})");
            return account;
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
            var permission = GetPermission(accountId);
            if (permission == null)
            {
                // Erstelle neue Permission
                string query = "INSERT INTO player_permissions (account_id, permission_level) VALUES (@accountId, @level)";
                long permId = await DatabaseManager.Instance.ExecuteInsert(query,
                    DatabaseManager.CreateParameter("@accountId", accountId),
                    DatabaseManager.CreateParameter("@level", (int)level));

                if (permId > 0)
                {
                    permission = new PlayerPermission
                    {
                        Id = (int)permId,
                        AccountId = accountId,
                        Level = level
                    };
                    _permissions[accountId] = permission;
                }
            }
            else
            {
                // Update existierende Permission
                permission.Level = level;
                permission.GrantedBy = grantedBy;
                permission.GrantedAt = DateTime.Now;

                string query = "UPDATE player_permissions SET permission_level = @level WHERE account_id = @accountId";
                await DatabaseManager.Instance.ExecuteNonQuery(query,
                    DatabaseManager.CreateParameter("@level", (int)level),
                    DatabaseManager.CreateParameter("@accountId", accountId));
            }

            NAPI.Util.ConsoleOutput($"[AccountManager] Permission für Account {accountId} auf {level} gesetzt");
        }
    }
}
