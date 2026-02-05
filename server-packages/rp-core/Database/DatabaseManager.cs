using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace RPCore.Database
{
    public class DatabaseManager
    {
        private MySqlConnection _connection;
        private readonly string _connectionString;

        public DatabaseManager()
        {
            // Wird später aus config.json geladen
            _connectionString = "Server=localhost;Database=ragemp_rp;Uid=root;Pwd=;";
            Connect();
        }

        public void Connect()
        {
            try
            {
                _connection = new MySqlConnection(_connectionString);
                _connection.Open();
                Console.WriteLine("[DATABASE] Verbindung zur Datenbank hergestellt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] Fehler bei Verbindung: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                Console.WriteLine("[DATABASE] Verbindung geschlossen");
            }
        }

        public async Task<MySqlDataReader> ExecuteQuery(string query)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                return await cmd.ExecuteReaderAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] Query-Fehler: {ex.Message}");
                return null;
            }
        }

        public async Task<int> ExecuteNonQuery(string query)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(query, _connection);
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATABASE] NonQuery-Fehler: {ex.Message}");
                return 0;
            }
        }
    }
}
