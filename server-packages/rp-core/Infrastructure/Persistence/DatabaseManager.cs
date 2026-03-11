using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RPCore.Database
{
    /// <summary>
    /// Verwaltet die MySQL-Datenbankverbindung mit Connection Pooling
    /// </summary>
    public class DatabaseManager
    {
        private static DatabaseManager? _instance;
        public static DatabaseManager Instance => _instance ??= new DatabaseManager();

        private string _connectionString = "";
        private bool _isInitialized = false;

        private DatabaseManager()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Lädt Datenbank-Config aus database.json
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                string configPath = Path.Combine("configs", "database.json");
                if (!File.Exists(configPath))
                {
                    Console.WriteLine($"[DATABASE] FEHLER: database.json nicht gefunden in: {configPath}");
                    return;
                }

                string json = File.ReadAllText(configPath);
                JObject config = JObject.Parse(json);

                var dbConfig = config["database"];
                if (dbConfig == null)
                {
                    Console.WriteLine("[DATABASE] FEHLER: 'database' Sektion fehlt in config!");
                    return;
                }

                string host = dbConfig["host"]?.ToString() ?? "localhost";
                int port = dbConfig["port"]?.ToObject<int>() ?? 3306;
                string user = dbConfig["user"]?.ToString() ?? "root";
                string password = dbConfig["password"]?.ToString() ?? "";
                string database = dbConfig["database"]?.ToString() ?? "ragemp_rp";
                string charset = dbConfig["charset"]?.ToString() ?? "utf8mb4";

                // Connection String mit Pooling
                _connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};" +
                                  $"Charset={charset};Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;" +
                                  $"ConnectionTimeout=30;DefaultCommandTimeout=30;";

                _isInitialized = true;
                Console.WriteLine($"[DATABASE] Konfiguration geladen: {host}:{port}/{database}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] Fehler beim Laden der Config: {ex.Message}");
            }
        }

        /// <summary>
        /// Testet die Datenbankverbindung
        /// </summary>
        public async Task<bool> TestConnection()
        {
            if (!_isInitialized)
            {
                Console.WriteLine("[DATABASE] Datenbank nicht initialisiert!");
                return false;
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                Console.WriteLine("[DATABASE] ✓ Verbindung erfolgreich!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] ✗ Verbindungsfehler: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Führt eine SELECT-Query aus und gibt DataReader zurück
        /// </summary>
        public async Task<MySqlDataReader?> ExecuteReader(string query, params MySqlParameter[] parameters)
        {
            if (!_isInitialized) return null;

            try
            {
                var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new MySqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] ExecuteReader Fehler: {ex.Message}");
                Console.WriteLine($"[DATABASE] Query: {query}");
                return null;
            }
        }

        /// <summary>
        /// Führt INSERT/UPDATE/DELETE aus und gibt betroffene Zeilen zurück
        /// </summary>
        public async Task<int> ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            if (!_isInitialized) return 0;

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] ExecuteNonQuery Fehler: {ex.Message}");
                Console.WriteLine($"[DATABASE] Query: {query}");
                return 0;
            }
        }

        /// <summary>
        /// Führt INSERT aus und gibt die neue Auto-Increment ID zurück
        /// </summary>
        public async Task<long> ExecuteInsert(string query, params MySqlParameter[] parameters)
        {
            if (!_isInitialized) return 0;

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                await command.ExecuteNonQueryAsync();
                return command.LastInsertedId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] ExecuteInsert Fehler: {ex.Message}");
                Console.WriteLine($"[DATABASE] Query: {query}");
                return 0;
            }
        }

        /// <summary>
        /// Führt Scalar-Query aus (COUNT, SUM, etc.)
        /// </summary>
        public async Task<object?> ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            if (!_isInitialized) return null;

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                return await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] ExecuteScalar Fehler: {ex.Message}");
                Console.WriteLine($"[DATABASE] Query: {query}");
                return null;
            }
        }

        /// <summary>
        /// Hilfsmethode: Erstellt MySqlParameter
        /// </summary>
        public static MySqlParameter CreateParameter(string name, object? value)
        {
            return new MySqlParameter(name, value ?? DBNull.Value);
        }
    }
}
