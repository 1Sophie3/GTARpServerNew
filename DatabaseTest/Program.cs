using System;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== RAGE MP Datenbank Test ===\n");

        // 1. Config laden
        Console.WriteLine("1. Lade database.json...");
        string configPath = Path.Combine("..", "configs", "database.json");
        
        if (!File.Exists(configPath))
        {
            Console.WriteLine($"✗ FEHLER: {configPath} nicht gefunden!");
            return;
        }

        string json = File.ReadAllText(configPath);
        JObject config = JObject.Parse(json);
        var dbConfig = config["database"];

        string host = dbConfig?["host"]?.ToString() ?? "localhost";
        int port = dbConfig?["port"]?.ToObject<int>() ?? 3306;
        string user = dbConfig?["user"]?.ToString() ?? "root";
        string password = dbConfig?["password"]?.ToString() ?? "";
        string database = dbConfig?["database"]?.ToString() ?? "ragemp_rp";

        Console.WriteLine($"✓ Config geladen: {user}@{host}:{port}/{database}\n");

        // 2. Verbindung testen
        Console.WriteLine("2. Teste Verbindung...");
        string connStr = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";

        try
        {
            using var connection = new MySqlConnection(connStr);
            await connection.OpenAsync();
            Console.WriteLine("✓ Verbindung erfolgreich!\n");

            // 3. Tabellen prüfen
            Console.WriteLine("3. Prüfe Tabellen...");
            using var cmd = new MySqlCommand("SHOW TABLES", connection);
            using var reader = await cmd.ExecuteReaderAsync();
            
            int tableCount = 0;
            while (await reader.ReadAsync())
            {
                string tableName = reader.GetString(0);
                Console.WriteLine($"   - {tableName}");
                tableCount++;
            }
            reader.Close();
            Console.WriteLine($"✓ {tableCount} Tabellen gefunden\n");

            // 4. Fraktionen laden
            Console.WriteLine("4. Lade Fraktionen...");
            using var cmdFactions = new MySqlCommand("SELECT id, name, type FROM factions", connection);
            using var readerFactions = await cmdFactions.ExecuteReaderAsync();
            
            while (await readerFactions.ReadAsync())
            {
                int id = readerFactions.GetInt32(0);
                string name = readerFactions.GetString(1);
                int type = readerFactions.GetInt32(2);
                Console.WriteLine($"   [{id}] {name} (Type: {type})");
            }
            readerFactions.Close();
            Console.WriteLine("✓ Fraktionen geladen\n");

            // 5. Test: Account erstellen
            Console.WriteLine("5. Teste Account-Erstellung...");
            string testUsername = "TestUser_" + DateTime.Now.Ticks;
            
            using var cmdInsert = new MySqlCommand(
                "INSERT INTO accounts (username, password_hash, email, created_at) VALUES (@user, @pass, @email, @created)",
                connection);
            cmdInsert.Parameters.AddWithValue("@user", testUsername);
            cmdInsert.Parameters.AddWithValue("@pass", "test_hash_123");
            cmdInsert.Parameters.AddWithValue("@email", "test@example.com");
            cmdInsert.Parameters.AddWithValue("@created", DateTime.Now);
            
            await cmdInsert.ExecuteNonQueryAsync();
            long accountId = cmdInsert.LastInsertedId;
            Console.WriteLine($"✓ Account erstellt mit ID: {accountId}\n");

            // 6. Account wieder laden
            Console.WriteLine("6. Lade Account...");
            using var cmdSelect = new MySqlCommand(
                "SELECT * FROM accounts WHERE id = @id",
                connection);
            cmdSelect.Parameters.AddWithValue("@id", accountId);
            
            using var readerAccount = await cmdSelect.ExecuteReaderAsync();
            if (await readerAccount.ReadAsync())
            {
                string username = readerAccount.GetString(readerAccount.GetOrdinal("username"));
                string email = readerAccount.GetString(readerAccount.GetOrdinal("email"));
                DateTime created = readerAccount.GetDateTime(readerAccount.GetOrdinal("created_at"));
                Console.WriteLine($"✓ Account geladen: {username} ({email})");
                Console.WriteLine($"  Erstellt: {created:dd.MM.yyyy HH:mm:ss}\n");
            }
            readerAccount.Close();

            // 7. Aufräumen
            Console.WriteLine("7. Räume Test-Account auf...");
            using var cmdDelete = new MySqlCommand(
                "DELETE FROM accounts WHERE id = @id",
                connection);
            cmdDelete.Parameters.AddWithValue("@id", accountId);
            await cmdDelete.ExecuteNonQueryAsync();
            Console.WriteLine("✓ Test-Account gelöscht\n");

            Console.WriteLine("=== ALLE TESTS ERFOLGREICH ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FEHLER: {ex.Message}");
            Console.WriteLine($"\nDetails:\n{ex}");
        }
    }
}
