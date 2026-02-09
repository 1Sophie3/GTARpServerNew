using MySql.Data.MySqlClient;
using System;
using GTANetworkAPI;
using RPCore.Datenbank;

public static class DatabaseHelper
{
    public static void ExecuteNonQuery(string commandText, params MySqlParameter[] parameters)
    {
        try
        {
            string connectionString = Datenbank.GetConnectionString();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            NAPI.Util.ConsoleOutput($"[DATABASE HELPER ERROR] {ex.Message}");
        }
    }
}