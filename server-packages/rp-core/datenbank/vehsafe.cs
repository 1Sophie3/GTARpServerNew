using RPCore.Database;
using RPCore.Notifications;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPCore.Commands
{
    // Repräsentiert ein gespeichertes Fahrzeug im vehsafe-System.
    public class VehSafe
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public bool IsFactionVehicle { get; set; }
        public int FactionId { get; set; }
        public string ModelName { get; set; }
        public string NumberPlate { get; set; }
        public string Modifications { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float Heading { get; set; }
        public float Health { get; set; }
        public const float MIN_ENGINE_HEALTH = 300f;
        public int Status { get; set; }
        public bool IsInGarage { get; set; }
        public int? GarageId { get; set; }
        public int ColorPrimary { get; set; }
        public int ColorSecondary { get; set; }
        //Tacho & Tank
        public float Fuel { get; set; }
        public float Mileage { get; set; }
        public string FuelType { get; set; }
    }
    public static class VehSafeData
    {
        private static readonly object DbLock = new object();

        // Speichert ein neues Fahrzeug in der DB inklusive Position und Health.
        public static int SaveVehicle(VehSafe vehicle)
        {
            int insertedId = 0;
            try
            {
                lock (DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
    INSERT INTO vehsafe
    (owner, is_faction_vehicle, faction_id, model_name, number_plate, modifications, pos_x, pos_y, pos_z, heading, health, status, garage_id, is_in_garage, color_primary, color_secondary, fuel, mileage, fuel_type)
    VALUES
    (@owner, @isFaction, @factionId, @modelName, @numberPlate, @modifications, @posX, @posY, @posZ, @heading, @health, @status, @garageId, @isInGarage, @colorPrimary, @colorSecondary, @fuel, @mileage, @fuel_type)";
                            command.Parameters.AddWithValue("@status", vehicle.Status);
                            command.Parameters.AddWithValue("@owner", vehicle.OwnerId);
                            command.Parameters.AddWithValue("@isFaction", vehicle.IsFactionVehicle ? 1 : 0);
                            command.Parameters.AddWithValue("@factionId", vehicle.FactionId);
                            command.Parameters.AddWithValue("@modelName", vehicle.ModelName);
                            command.Parameters.AddWithValue("@numberPlate", vehicle.NumberPlate);
                            command.Parameters.AddWithValue("@modifications", vehicle.Modifications);
                            command.Parameters.AddWithValue("@posX", vehicle.PosX);
                            command.Parameters.AddWithValue("@posY", vehicle.PosY);
                            command.Parameters.AddWithValue("@posZ", vehicle.PosZ);
                            command.Parameters.AddWithValue("@heading", vehicle.Heading);
                            command.Parameters.AddWithValue("@health", vehicle.Health);
                            command.Parameters.AddWithValue("@garageId", vehicle.GarageId);
                            command.Parameters.AddWithValue("@isInGarage", vehicle.IsInGarage ? 1 : 0);
                            command.Parameters.AddWithValue("@colorPrimary", vehicle.ColorPrimary);
                            command.Parameters.AddWithValue("@colorSecondary", vehicle.ColorSecondary);
                            command.Parameters.AddWithValue("@fuel", vehicle.Fuel);
                            command.Parameters.AddWithValue("@mileage", vehicle.Mileage);
                            command.Parameters.AddWithValue("@fuelType", vehicle.FuelType);

                            command.ExecuteNonQuery();
                            insertedId = (int)command.LastInsertedId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ConsoleOutput für Fehler beim Speichern des Fahrzeugs
            }
            return insertedId;
        }

        // Aktualisiert einen bestehenden Fahrzeugdatensatz (inklusive Position und Health).
        public static void UpdateVehicle(VehSafe vehicle)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
    UPDATE vehsafe
    SET owner = @owner,
        is_faction_vehicle = @isFaction,
        faction_id = @factionId,
        model_name = @modelName,
        number_plate = @numberPlate,
        modifications = @modifications,
        pos_x = @posX,
        pos_y = @posY,
        pos_z = @posZ,
        heading = @heading,
        health = @health,
        status = @status,
        garage_id = @garageId,
        is_in_garage = @isInGarage,
        color_primary = @colorPrimary,
        color_secondary = @colorSecondary,
 fuel = @fuel, 
    mileage = @mileage, 
    fuel_type = @fuelType
    WHERE id = @id";
                            command.Parameters.AddWithValue("@status", vehicle.Status);
                            command.Parameters.AddWithValue("@owner", vehicle.OwnerId);
                            command.Parameters.AddWithValue("@isInGarage", vehicle.IsInGarage ? 1 : 0);
                            command.Parameters.AddWithValue("@factionId", vehicle.FactionId);
                            command.Parameters.AddWithValue("@modelName", vehicle.ModelName);
                            command.Parameters.AddWithValue("@numberPlate", vehicle.NumberPlate);
                            command.Parameters.AddWithValue("@modifications", vehicle.Modifications);
                            command.Parameters.AddWithValue("@posX", vehicle.PosX);
                            command.Parameters.AddWithValue("@posY", vehicle.PosY);
                            command.Parameters.AddWithValue("@posZ", vehicle.PosZ);
                            command.Parameters.AddWithValue("@heading", vehicle.Heading);
                            command.Parameters.AddWithValue("@health", vehicle.Health);
                            command.Parameters.AddWithValue("@garageId", vehicle.GarageId);
                            command.Parameters.AddWithValue("@isFaction", vehicle.IsFactionVehicle ? 1 : 0);
                            command.Parameters.AddWithValue("@colorPrimary", vehicle.ColorPrimary);
                            command.Parameters.AddWithValue("@colorSecondary", vehicle.ColorSecondary);
                            command.Parameters.AddWithValue("@fuel", vehicle.Fuel);
                            command.Parameters.AddWithValue("@mileage", vehicle.Mileage);
                            command.Parameters.AddWithValue("@fuelType", vehicle.FuelType);
                            command.Parameters.AddWithValue("@id", vehicle.Id);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //ConsoleOutput für Fehler beim Aktualisieren des Fahrzeugs
            }
        }


        // Aktualisiert nur Position, Heading und Health.
        public static void UpdateVehiclePosition(int vehSafeId, Vector3 position, float heading, float health)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                UPDATE vehsafe
                                SET pos_x = @posX, pos_y = @posY, pos_z = @posZ, heading = @heading, health = @health
                                WHERE id = @id";

                            command.Parameters.AddWithValue("@posX", position.X);
                            command.Parameters.AddWithValue("@posY", position.Y);
                            command.Parameters.AddWithValue("@posZ", position.Z);
                            command.Parameters.AddWithValue("@heading", heading);
                            command.Parameters.AddWithValue("@health", health);
                            command.Parameters.AddWithValue("@id", vehSafeId);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ConsoleOutput für Fehler beim Aktualisieren der Fahrzeugposition
                // NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Aktualisieren der Fahrzeugposition: {ex.Message}");
            }
        }


        // Löscht den Fahrzeugdatensatz aus der DB.
        public static void DeleteVehicleRecord(int vehSafeId)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM vehsafe WHERE id = @id";
                            command.Parameters.AddWithValue("@id", vehSafeId);
                            command.ExecuteNonQuery();
                        }
                        NAPI.Util.ConsoleOutput($"[INFO] Fahrzeug mit ID {vehSafeId} aus vehsafe gelöscht.");
                    }
                }
            }
            catch (Exception ex)
            {
                // ConsoleOutput für Fehler beim Löschen des Fahrzeugs
                // NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Löschen des Fahrzeugs (ID {vehSafeId}): {ex.Message}");
            }
        }



        // Liest einen Fahrzeugdatensatz anhand seiner ID.
        public static VehSafe GetVehicleById(int id)
        {
            VehSafe veh = null;
            try
            {
                lock (DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM vehsafe WHERE id = @id";
                            command.Parameters.AddWithValue("@id", id);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    veh = new VehSafe
                                    {
                                        Id = reader.GetInt32("id"),
                                        OwnerId = reader.GetInt32("owner"),
                                        IsFactionVehicle = reader.GetBoolean("is_faction_vehicle"),
                                        FactionId = reader.GetInt32("faction_id"),
                                        ModelName = reader.GetString("model_name"),
                                        NumberPlate = reader.GetString("number_plate"),
                                        Modifications = reader.GetString("modifications"),
                                        PosX = reader.IsDBNull(reader.GetOrdinal("pos_x")) ? 0.0f : reader.GetFloat("pos_x"),
                                        PosY = reader.IsDBNull(reader.GetOrdinal("pos_y")) ? 0.0f : reader.GetFloat("pos_y"),
                                        PosZ = reader.IsDBNull(reader.GetOrdinal("pos_z")) ? 0.0f : reader.GetFloat("pos_z"),
                                        Heading = reader.IsDBNull(reader.GetOrdinal("heading")) ? 0.0f : reader.GetFloat("heading"),
                                        Health = reader.IsDBNull(reader.GetOrdinal("health")) ? 1000f : reader.GetFloat("health"),
                                        Status = reader.IsDBNull(reader.GetOrdinal("status")) ? 0 : reader.GetInt32("status"),
                                        IsInGarage = !reader.IsDBNull(reader.GetOrdinal("is_in_garage")) && reader.GetBoolean("is_in_garage"),
                                        GarageId = reader.IsDBNull(reader.GetOrdinal("garage_id")) ? (int?)null : reader.GetInt32("garage_id"),
                                        ColorPrimary = reader.IsDBNull(reader.GetOrdinal("color_primary")) ? 0 : reader.GetInt32("color_primary"),
                                        ColorSecondary = reader.IsDBNull(reader.GetOrdinal("color_secondary")) ? 0 : reader.GetInt32("color_secondary"),
                                        Fuel = reader.IsDBNull(reader.GetOrdinal("fuel")) ? 100.0f : reader.GetFloat("fuel"),
                                        Mileage = reader.IsDBNull(reader.GetOrdinal("mileage")) ? 0.0f : reader.GetFloat("mileage"),
                                        FuelType = reader.IsDBNull(reader.GetOrdinal("fuel_type")) ? "benzin" : reader.GetString("fuel_type")
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Abrufen des Fahrzeugs (ID {id}): {ex.Message}");
            }
            return veh;
        }

        public static List<VehSafe> LoadVehiclesForPlayer(int ownerId, int fraktionId)
        {
            List<VehSafe> vehicles = new List<VehSafe>();
            string query = "SELECT * FROM vehsafe WHERE owner = @OwnerId OR (is_faction_vehicle = 1 AND faction_id = @FraktionId)";

            using (MySqlConnection connection = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                        cmd.Parameters.AddWithValue("@FraktionId", fraktionId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                vehicles.Add(new VehSafe
                                {
                                    Id = reader.GetInt32("id"),
                                    OwnerId = reader.GetInt32("owner"),
                                    IsFactionVehicle = reader.GetBoolean("is_faction_vehicle"),
                                    FactionId = reader.GetInt32("faction_id"),
                                    ModelName = reader.GetString("model_name"),
                                    NumberPlate = reader.GetString("number_plate"),
                                    Modifications = reader.GetString("modifications"),
                                    PosX = reader.IsDBNull(reader.GetOrdinal("pos_x")) ? 0.0f : reader.GetFloat("pos_x"),
                                    PosY = reader.IsDBNull(reader.GetOrdinal("pos_y")) ? 0.0f : reader.GetFloat("pos_y"),
                                    PosZ = reader.IsDBNull(reader.GetOrdinal("pos_z")) ? 0.0f : reader.GetFloat("pos_z"),
                                    Heading = reader.IsDBNull(reader.GetOrdinal("heading")) ? 0.0f : reader.GetFloat("heading"),
                                    Health = reader.IsDBNull(reader.GetOrdinal("health")) ? 1000f : reader.GetFloat("health"),
                                    Status = reader.IsDBNull(reader.GetOrdinal("status")) ? 0 : reader.GetInt32("status"),
                                    GarageId = reader.IsDBNull(reader.GetOrdinal("garage_id")) ? (int?)null : reader.GetInt32("garage_id"),
                                    IsInGarage = !reader.IsDBNull(reader.GetOrdinal("is_in_garage")) && reader.GetBoolean("is_in_garage"),
                                    ColorPrimary = reader.IsDBNull(reader.GetOrdinal("color_primary")) ? 0 : reader.GetInt32("color_primary"),
                                    ColorSecondary = reader.IsDBNull(reader.GetOrdinal("color_secondary")) ? 0 : reader.GetInt32("color_secondary"),
                                    Fuel = reader.IsDBNull(reader.GetOrdinal("fuel")) ? 100.0f : reader.GetFloat("fuel"),
                                    Mileage = reader.IsDBNull(reader.GetOrdinal("mileage")) ? 0.0f : reader.GetFloat("mileage"),
                                    FuelType = reader.IsDBNull(reader.GetOrdinal("fuel_type")) ? "benzin" : reader.GetString("fuel_type")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Laden der VehSafe für Spieler {ownerId}: {ex.Message}");
                }
            }
            return vehicles;
        }

        public static List<VehSafe> GetVehiclesByOwner(int ownerId)
        {
            List<VehSafe> vehicles = new List<VehSafe>();
            try
            {
                lock (DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM vehsafe WHERE owner = @owner";
                            command.Parameters.AddWithValue("@owner", ownerId);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    VehSafe veh = new VehSafe
                                    {
                                        Id = reader.GetInt32("id"),
                                        OwnerId = reader.GetInt32("owner"),
                                        IsFactionVehicle = reader.GetBoolean("is_faction_vehicle"),
                                        FactionId = reader.GetInt32("faction_id"),
                                        ModelName = reader.GetString("model_name"),
                                        NumberPlate = reader.GetString("number_plate"),
                                        Modifications = reader.GetString("modifications"),
                                        PosX = reader.IsDBNull(reader.GetOrdinal("pos_x")) ? 0.0f : reader.GetFloat("pos_x"),
                                        PosY = reader.IsDBNull(reader.GetOrdinal("pos_y")) ? 0.0f : reader.GetFloat("pos_y"),
                                        PosZ = reader.IsDBNull(reader.GetOrdinal("pos_z")) ? 0.0f : reader.GetFloat("pos_z"),
                                        Heading = reader.IsDBNull(reader.GetOrdinal("heading")) ? 0.0f : reader.GetFloat("heading"),
                                        Health = reader.IsDBNull(reader.GetOrdinal("health")) ? 1000f : reader.GetFloat("health"),
                                        Status = reader.IsDBNull(reader.GetOrdinal("status")) ? 0 : reader.GetInt32("status"),
                                        IsInGarage = !reader.IsDBNull(reader.GetOrdinal("is_in_garage")) && reader.GetBoolean("is_in_garage"),
                                        GarageId = reader.IsDBNull(reader.GetOrdinal("garage_id")) ? (int?)null : reader.GetInt32("garage_id"),
                                        ColorPrimary = reader.IsDBNull(reader.GetOrdinal("color_primary")) ? 0 : reader.GetInt32("color_primary"),
                                        ColorSecondary = reader.IsDBNull(reader.GetOrdinal("color_secondary")) ? 0 : reader.GetInt32("color_secondary"),
                                        Fuel = reader.IsDBNull(reader.GetOrdinal("fuel")) ? 100.0f : reader.GetFloat("fuel"),
                                        Mileage = reader.IsDBNull(reader.GetOrdinal("mileage")) ? 0.0f : reader.GetFloat("mileage"),
                                        FuelType = reader.IsDBNull(reader.GetOrdinal("fuel_type")) ? "benzin" : reader.GetString("fuel_type")
                                    };
                                    vehicles.Add(veh);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Abrufen der Fahrzeuge für Besitzer {ownerId}: {ex.Message}");
            }
            return vehicles;
        }

        public static void UpdateVehicleModifications(int vehSafeId, string modifications)
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE vehsafe SET modifications = @modifications WHERE id = @id";
                            command.Parameters.AddWithValue("@modifications", modifications);
                            command.Parameters.AddWithValue("@id", vehSafeId);
                            command.ExecuteNonQuery();
                        }
                        NAPI.Util.ConsoleOutput($"[INFO] Fahrzeugmodifications aktualisiert für Fahrzeug-ID {vehSafeId}");
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Aktualisieren der Fahrzeugmodifications: {ex.Message}");
            }
        }

        public static void SpawnAllVehicles()
        {
            try
            {
                lock (DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM vehsafe WHERE is_in_garage = 0 AND (garage_id IS NULL OR garage_id = 0)"; //
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int currentVehicleId = -1;
                                    try
                                    {
                                        currentVehicleId = reader.GetInt32("id"); //
                                        string modelName = reader.IsDBNull(reader.GetOrdinal("model_name")) ? string.Empty : reader.GetString("model_name"); //
                                        uint vehHash = NAPI.Util.GetHashKey(modelName); //
                                        if (vehHash == 0)
                                        {
                                            NAPI.Util.ConsoleOutput($"[WARN] Fahrzeug ID {currentVehicleId} mit ungültigem Modell: '{modelName}' übersprungen"); //
                                            continue;
                                        }
                                        Vector3 position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z")); //
                                        float heading = reader.GetFloat("heading"); //
                                        float health = reader.IsDBNull(reader.GetOrdinal("health")) ? 1000f : reader.GetFloat("health"); //
                                        int color1 = reader.IsDBNull(reader.GetOrdinal("color_primary")) ? 0 : reader.GetInt32("color_primary"); //
                                        int color2 = reader.IsDBNull(reader.GetOrdinal("color_secondary")) ? 0 : reader.GetInt32("color_secondary"); //
                                        string numberPlate = reader.IsDBNull(reader.GetOrdinal("number_plate")) ? "UNKNOWN" : reader.GetString("number_plate"); //

                                        Vehicle veh = NAPI.Vehicle.CreateVehicle(vehHash, position, heading, color1, color2); //
                                        veh.SetData("vehsafe_id", currentVehicleId); //
                                        veh.NumberPlate = numberPlate; //
                                        veh.Locked = true; //
                                        veh.EngineStatus = false; //
                                        veh.Health = health; //
                                        veh.SetSharedData("VEH_ENGINE_STATE", false); //

                                        // --- NEUE DATEN AUSLESEN UND SETZEN ---
                                        float fuel = reader.IsDBNull(reader.GetOrdinal("fuel")) ? 100.0f : reader.GetFloat("fuel");
                                        float mileage = reader.IsDBNull(reader.GetOrdinal("mileage")) ? 0.0f : reader.GetFloat("mileage");
                                        string fuelType = reader.IsDBNull(reader.GetOrdinal("fuel_type")) ? "benzin" : reader.GetString("fuel_type");

                                        veh.SetSharedData("VEHICLE_FUEL", fuel);
                                        veh.SetSharedData("VEHICLE_MILEAGE", mileage);
                                        veh.SetSharedData("VEHICLE_FUEL_TYPE", fuelType);
                                        // --- ENDE DER NEUEN DATEN ---

                                        string modsJson = ""; //
                                        if (!reader.IsDBNull(reader.GetOrdinal("modifications"))) //
                                        {
                                            modsJson = reader.GetString("modifications"); //
                                        }
                                        NAPI.Task.Run(async () => //
                                        {
                                            await Task.Delay(1000); //
                                            if (!string.IsNullOrEmpty(modsJson)) //
                                            {
                                                try
                                                {
                                                    Dictionary<int, int> mods = JsonConvert.DeserializeObject<Dictionary<int, int>>(modsJson); //
                                                    if (mods != null) //
                                                    {
                                                        foreach (KeyValuePair<int, int> mod in mods) //
                                                        {
                                                            try { veh.SetMod(mod.Key, mod.Value); } //
                                                            catch (Exception exMod) { NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Setzen von Mod {mod.Key} für Fahrzeug {currentVehicleId}: {exMod.Message}"); } //
                                                        }
                                                    }
                                                }
                                                catch (Exception exJson) { NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Deserialisieren der Mods für Fahrzeug {currentVehicleId}: {exJson.Message}"); } //
                                            }
                                        });
                                    }
                                    catch (Exception innerEx)
                                    {
                                        NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Spawnen von Fahrzeug ID {currentVehicleId}: {innerEx.Message}"); //
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Schwerwiegender Fehler beim Fahrzeug-Spawn: {ex.Message}"); //
            }
            NAPI.Util.ConsoleOutput("[INFO] Alle gespeicherten Fahrzeuge wurden gespawnt."); //
        }

        public static void ResetVehicleHealth(Vehicle veh)
        {
            if (veh == null || !veh.HasData("vehsafe_id")) return;
            veh.Health = 1000;
            VehSafe vehData = GetVehicleById(veh.GetData<int>("vehsafe_id"));
            if (vehData != null)
            {
                UpdateVehicle(vehData);
            }
        }

       
       

        // +++ NEUE METHODE ZUM SPEICHERN VON TANK/KM +++
        public static void UpdateVehicleFuelAndMileage(int vehSafeId, float fuel, float mileage)
        {
            try
            {
                lock (Datenbank.Datenbank.DbLock)
                {
                    string connectionString = gamemode.Datenbank.Datenbank.GetConnectionString();
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE vehsafe SET fuel = @fuel, mileage = @mileage WHERE id = @id";
                            command.Parameters.AddWithValue("@fuel", fuel);
                            command.Parameters.AddWithValue("@mileage", mileage);
                            command.Parameters.AddWithValue("@id", vehSafeId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Aktualisieren von Tank/KM für Fahrzeug {vehSafeId}: {ex.Message}");
            }
        }


    }

    public class VehSafeCommands : Script
    {
        public const float MIN_ENGINE_HEALTH = 300f;

        private bool CanControlVehicle(Player player, VehSafe vehData)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null) return false;
            if (acc.ID == vehData.OwnerId) return true;
            if (vehData.IsFactionVehicle && acc.Fraktion == vehData.FactionId) return true;
            return false;
        }

        private Vehicle GetTargetVehicle(Player player)
        {
            if (player.Vehicle != null) return player.Vehicle;
            Vehicle nearestVeh = null;
            float minDistance = 5.0f;
            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
            {
                if (!veh.HasData("vehsafe_id")) continue;
                float dist = player.Position.DistanceTo(veh.Position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestVeh = veh;
                }
            }
            return nearestVeh;
        }

        



        #region Spieler-Befehle (Aktiv)

        [Command("vlock")]
        public void LockVehicleCommand(Player player)
        {
            Vehicle veh = player.Vehicle ?? GetTargetVehicle(player);
            if (veh == null) { player.SendNotification("~r~Kein Fahrzeug gefunden!"); return; }
            if (!veh.HasData("vehsafe_id")) { player.SendNotification("~r~Dieses Fahrzeug ist nicht registriert!"); return; }

            int vehSafeId = veh.GetData<int>("vehsafe_id");
            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null) { player.SendNotification("~r~Fahrzeugdatensatz nicht gefunden!"); return; }
            if (!CanControlVehicle(player, vehData)) { player.SendNotification("~r~Du bist nicht berechtigt!"); return; }

            veh.Locked = true;
            player.SendNotification("~g~Fahrzeug abgeschlossen!");
        }

        [Command("vunlock")]
        public void UnlockVehicleCommand(Player player)
        {
            Vehicle veh = player.Vehicle ?? GetTargetVehicle(player);
            if (veh == null) { player.SendNotification("~r~Kein Fahrzeug gefunden!"); return; }
            if (!veh.HasData("vehsafe_id")) { player.SendNotification("~r~Dieses Fahrzeug ist nicht registriert!"); return; }

            int vehSafeId = veh.GetData<int>("vehsafe_id");
            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null) { player.SendNotification("~r~Fahrzeugdatensatz nicht gefunden!"); return; }
            if (!CanControlVehicle(player, vehData)) { player.SendNotification("~r~Du bist nicht berechtigt!"); return; }

            veh.Locked = false;
            Utils.sendNotification(player, "Fahrzeug aufgeschlossen!!", "");
        }

        [Command("engineon")]
        public void EngineOnCommand(Player player)
        {
            if (player.Vehicle == null) { player.SendNotification("~r~Du sitzt in keinem Fahrzeug!"); return; }
            if (!player.Vehicle.HasData("vehsafe_id")) { player.SendNotification("~r~Dieses Fahrzeug ist nicht registriert!"); return; }

            int vehSafeId = player.Vehicle.GetData<int>("vehsafe_id");
            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null) { player.SendNotification("~r~Fahrzeugdatensatz nicht gefunden!"); return; }
            if (!CanControlVehicle(player, vehData)) { player.SendNotification("~r~Du bist nicht berechtigt!"); return; }
            if (vehData.Health < MIN_ENGINE_HEALTH || vehData.Status == 1) { player.SendNotification("~r~Der Motor ist zu beschädigt!"); return; }

            player.Vehicle.EngineStatus = true;
            player.Vehicle.SetSharedData("VEH_ENGINE_STATE", true);
            Utils.sendNotification(player, "Motor eingeschaltet!", "");
        }

        [Command("engineoff")]
        public void EngineOffCommand(Player player)
        {
            if (player.Vehicle == null) { player.SendNotification("~r~Du sitzt in keinem Fahrzeug!"); return; }
            if (!player.Vehicle.HasData("vehsafe_id")) { player.SendNotification("~r~Dieses Fahrzeug ist nicht registriert!"); return; }

            int vehSafeId = player.Vehicle.GetData<int>("vehsafe_id");
            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null) { player.SendNotification("~r~Fahrzeugdatensatz nicht gefunden!"); return; }
            if (!CanControlVehicle(player, vehData)) { player.SendNotification("~r~Du bist nicht berechtigt!"); return; }

            player.Vehicle.EngineStatus = false;
            player.Vehicle.SetSharedData("VEH_ENGINE_STATE", false);
            Utils.sendNotification(player, "Motor ausgeschaltet!", "");
        }

        [Command("myvehicles")]
        public void MyVehiclesCommand(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null) { player.SendNotification("~r~Dein Account konnte nicht geladen werden!"); return; }

            List<VehSafe> vehicles = VehSafeData.GetVehiclesByOwner(account.ID);
            if (vehicles.Count == 0) { player.SendNotification("~y~Du hast keine Fahrzeuge."); return; }

            player.SendNotification("~y~Deine Fahrzeuge:");
            foreach (VehSafe veh in vehicles)
            {
                player.SendNotification($"~w~ID: {veh.Id} | Modell: {veh.ModelName} | Kennzeichen: {veh.NumberPlate}");
            }
        }

        [Command("vehhelp")]
        public void VehHelpCommand(Player player)
        {
            player.SendChatMessage("~y~----- Fahrzeug Hilfe -----");
            player.SendChatMessage("~y~/vlock & /vunlock ~w~- Schließt dein Fahrzeug auf/ab.");
            player.SendChatMessage("~y~/engineon & /engineoff ~w~- Schaltet den Motor ein/aus.");
            player.SendChatMessage("~y~/myvehicles ~w~- Zeigt deine Fahrzeuge an.");
            player.SendChatMessage("~y~/setnumberplate <plate> ~w~- Setzt dein Nummernschild (Kosten: $500).");
            player.SendChatMessage("~y~/setcolor <color1> <color2> ~w~- Ändert die Fahrzeugfarben (Kosten: $500).");
        }

        [Command("setnumberplate")]
        public void SetNumberPlateCommand(Player player, string plate)
        {
            if (player.Vehicle == null) { player.SendNotification("~r~Du sitzt in keinem Fahrzeug!"); return; }
            if (!player.Vehicle.HasData("vehsafe_id")) { player.SendNotification("~r~Dieses Fahrzeug ist nicht registriert!"); return; }

            int vehSafeId = player.Vehicle.GetData<int>("vehsafe_id");
            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null) { player.SendNotification("~r~Fahrzeugdatensatz nicht gefunden!"); return; }
            if (!CanControlVehicle(player, vehData)) { player.SendNotification("~r~Du bist nicht berechtigt!"); return; }

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null) { player.SendNotification("~r~Dein Account konnte nicht geladen werden!"); return; }
            if (acc.Geld < 500) { player.SendNotification("~r~Nicht genug Geld! Du benötigst $500."); return; }

            acc.Geld -= 500;
            gamemode.Datenbank.Datenbank.AccountSpeichern(player);

            vehData.NumberPlate = plate;
            VehSafeData.UpdateVehicle(vehData);

            player.Vehicle.NumberPlate = plate;
            player.SendNotification($"~g~Nummernschild auf {plate} geändert! (Kosten: $500)");
        }

        [Command("setcolor")]
        public void SetColorCommand(Player player, int color1, int color2)
        {
            if (player.Vehicle == null) { player.SendNotification("~r~Du sitzt in keinem Fahrzeug!"); return; }
            if (!player.Vehicle.HasData("vehsafe_id")) { player.SendNotification("~r~Fahrzeug nicht registriert!"); return; }

            int vehSafeId = player.Vehicle.GetData<int>("vehsafe_id");
            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null) { player.SendNotification("~r~Fahrzeugdatensatz nicht gefunden!"); return; }

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null || acc.ID != vehData.OwnerId) { player.SendNotification("~r~Du bist nicht der Besitzer!"); return; }
            if (acc.Geld < 500) { player.SendNotification("~r~Nicht genug Geld! ($500 benötigt)"); return; }

            acc.Geld -= 500;
            Datenbank.Datenbank.AccountSpeichern(player);

            try
            {
                string connectionString = Datenbank.Datenbank.GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE vehsafe SET color_primary=@color1, color_secondary=@color2 WHERE id=@id";
                        command.Parameters.AddWithValue("@color1", color1);
                        command.Parameters.AddWithValue("@color2", color2);
                        command.Parameters.AddWithValue("@id", vehSafeId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[ERROR] Fehler beim Speichern der Farben: " + ex.Message);
            }

            player.Vehicle.PrimaryColor = color1;
            player.Vehicle.SecondaryColor = color2;
            player.SendNotification($"~g~Fahrzeugfarben geändert! (Kosten: $500)");
        }

        #endregion

        #region Remote Events & System Logic

        [RemoteEvent("updateVehicleHealth")]
        public void UpdateVehicleHealthEvent(Player player, float clientHealth)
        {
            if (player.Vehicle == null || !player.Vehicle.HasData("vehsafe_id")) return;
            Vehicle veh = player.Vehicle;
            int vehSafeId = veh.GetData<int>("vehsafe_id");
            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null) return;
            vehData.Health = clientHealth;
            VehSafeData.UpdateVehicle(vehData);
            veh.SetData("ServerVehicleHealth", clientHealth);
        }

        [RemoteEvent("handlePlayerCrash")]
        public void HandlePlayerCrash(Player player, float updatedVehicleHealth)
        {
            if (player == null) return;
            if (player.Vehicle != null)
            {
                player.WarpOutOfVehicle();
                if (player.Vehicle.HasData("vehsafe_id"))
                {
                    int vehSafeId = player.Vehicle.GetData<int>("vehsafe_id");
                    VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
                    if (vehData != null)
                    {
                        vehData.Health = updatedVehicleHealth;
                        vehData.Status = 1;
                        VehSafeData.UpdateVehicle(vehData);
                        player.Vehicle.SetData("ServerVehicleHealth", updatedVehicleHealth);
                    }
                }
            }
            player.Health = 0;
            player.Armor = 0;
            gamemode.Datenbank.Datenbank.AccountSpeichern(player);
        }

        #endregion
    }

    public class VPDES : Script
    {
        public const float MIN_ENGINE_HEALTH = 300f;
        public static void PreventDamagedEngineStart()
        {
            NAPI.Task.Run(async () =>
            {
               
                {
                    try
                    {
                        foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                        {
                            if (veh.HasData("vehsafe_id"))
                            {
                                int vehSafeId = veh.GetData<int>("vehsafe_id");
                                VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
                                if (vehData != null)
                                {
                                    if (vehData.Health < MIN_ENGINE_HEALTH && veh.EngineStatus)
                                    {
                                        veh.EngineStatus = false;
                                        //NAPI.Util.ConsoleOutput($"[INFO] Fahrzeug {vehSafeId} ist beschädigt – Motor bleibt aus.");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput($"[ERROR] PreventDamagedEngineStart: {ex.Message}");
                    }
                    await Task.Delay(2000);
                }
            });
        }
    }
}