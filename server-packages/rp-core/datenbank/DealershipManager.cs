#nullable enable

using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPCore.Database;
using RPCore.Commands;
using RPCore.FraktionsSystem;
using RPCore.Dealership;

namespace RPCore.Dealership
{
    public class DealershipManager : Script
    {
        private static List<Dealership> _dealerships = new List<Dealership>();
        private static Dictionary<string, VehicleDefinition> _vehicleDefinitions = new Dictionary<string, VehicleDefinition>();

        private static Random _random = new Random();
        private static Dictionary<string, int> _displayVehicleCounts = new Dictionary<string, int>();

      

        [RemoteEvent("Dealership:ReloadData")]
        public void OnReloadData(Player player)
        {
            NAPI.Util.ConsoleOutput("[DEALERSHIP] Lade Daten neu...");
            LoadAllDealershipData();
            if (player != null && player.Exists)
            {
                player.SendNotification("~g~Fahrzeughändler-Daten neu geladen.");
            }
        }

        public static void LoadAllDealershipData()
        {
            _vehicleDefinitions.Clear();
            string defQuery = "SELECT * FROM vehicle_definitions";
            using (var connection = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(defQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var def = new VehicleDefinition
                            {
                                Model = reader.GetString("model"),
                                DisplayName = reader.GetString("displayName"),
                                Category = reader.GetString("category"),
                                Price = reader.GetInt32("price"),
                                IsFactionBuyable = reader.GetBoolean("isFactionBuyable"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "Keine Beschreibung verfügbar." : reader.GetString("description")
                            };
                            _vehicleDefinitions[def.Model] = def;
                        }
                    }
                }
                catch (Exception e) { NAPI.Util.ConsoleOutput($"[DEALERSHIP ERROR] Vehicle Definitions: {e.Message}"); }
            }
            NAPI.Util.ConsoleOutput($"[DEALERSHIP] {_vehicleDefinitions.Count} Fahrzeugdefinitionen geladen.");

            _dealerships.ForEach(d => d.DisplayVehicle?.Delete());
            _dealerships.Clear();
            string dealershipQuery = "SELECT * FROM dealerships";
            using (var connection = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(dealershipQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _dealerships.Add(new Dealership
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Category = reader.GetString("category"),
                                Position = new Vector3(reader.GetFloat("posX"), reader.GetFloat("posY"), reader.GetFloat("posZ")),
                                Rotation = reader.GetFloat("rotZ")
                            });
                        }
                    }
                }
                catch (Exception e) { NAPI.Util.ConsoleOutput($"[DEALERSHIP ERROR] Dealerships: {e.Message}"); }
            }
            NAPI.Util.ConsoleOutput($"[DEALERSHIP] {_dealerships.Count} Autohäuser geladen.");

            SpawnAllDisplayVehicles();
        }

        private static void SpawnAllDisplayVehicles()
        {
            _dealerships.ForEach(d => d.DisplayVehicle?.Delete());
            _displayVehicleCounts.Clear();

            var spotsByCategory = _dealerships.GroupBy(d => d.Category);

            foreach (var categoryGroup in spotsByCategory)
            {
                string category = categoryGroup.Key;

                var availableVehicles = _vehicleDefinitions.Values
                    .Where(v => v.Category == category)
                    .OrderBy(v => _random.Next())
                    .ToList();

                if (!availableVehicles.Any()) continue;

                foreach (var dealershipSpot in categoryGroup)
                {
                    VehicleDefinition? vehicleToSpawn = null;

                    foreach (var potentialVehicle in availableVehicles)
                    {
                        int currentDisplayCount = _displayVehicleCounts.GetValueOrDefault(potentialVehicle.Model, 0);
                        if (currentDisplayCount < 2)
                        {
                            vehicleToSpawn = potentialVehicle;
                            break;
                        }
                    }

                    if (vehicleToSpawn != null)
                    {
                        SpawnSingleVehicleAtSpot(dealershipSpot, vehicleToSpawn);
                        _displayVehicleCounts[vehicleToSpawn.Model] = _displayVehicleCounts.GetValueOrDefault(vehicleToSpawn.Model, 0) + 1;
                    }
                    else
                    {
                        dealershipSpot.DisplayVehicle = null;
                    }
                }
            }
        }

        private static void SpawnSingleVehicleAtSpot(Dealership dealershipSpot, VehicleDefinition vehicleDef)
        {
            var veh = NAPI.Vehicle.CreateVehicle(
                NAPI.Util.GetHashKey(vehicleDef.Model),
                dealershipSpot.Position,
                dealershipSpot.Rotation,
                0, 0, "BUY ME", 255, false, false, 0
            );

            veh.SetData("IS_DEALERSHIP_VEHICLE", true);
            veh.SetData("DEALERSHIP_ID", dealershipSpot.Id);
            veh.SetData("VEHICLE_MODEL", vehicleDef.Model);
            dealershipSpot.DisplayVehicle = veh;
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seat)
        {
            if (!vehicle.HasData("IS_DEALERSHIP_VEHICLE") || seat != (int)VehicleSeat.Driver) return;
            if (player.HasData("DEALERSHIP_MENU_WAS_OPEN")) return;

            int dealershipId = vehicle.GetData<int>("DEALERSHIP_ID");
            string model = vehicle.GetData<string>("VEHICLE_MODEL");

            if (!_vehicleDefinitions.TryGetValue(model, out var vehicleDef)) return;

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            int playerRank = acc != null && acc.Fraktion > 0 ? Fglobalconf.FraktionsCommands.GetMemberRank(acc.ID, acc.Fraktion) : 0;

            var dto = new VehicleInfoDTO
            {
                Model = vehicleDef.Model,
                DisplayName = vehicleDef.DisplayName,
                Price = vehicleDef.Price,
                Description = vehicleDef.Description,
                IsFactionBuyable = vehicleDef.IsFactionBuyable,
                PlayerFactionRank = playerRank
            };

            player.SetData("CURRENT_DEALERSHIP_ID", dealershipId);
            player.SetData("DEALERSHIP_MENU_WAS_OPEN", true);
            player.TriggerEvent("client:dealership:showMenu", JsonConvert.SerializeObject(dto));
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            if (player.HasData("DEALERSHIP_MENU_WAS_OPEN"))
            {
                player.ResetData("DEALERSHIP_MENU_WAS_OPEN");
            }
            player.TriggerEvent("client:dealership:closeMenu");
        }

        [RemoteEvent("server:dealership:buyVehicle")]
        public async Task OnBuyVehicle(Player player, string model, bool forFaction)
        {
            if (!player.HasData("CURRENT_DEALERSHIP_ID")) return;
            int dealershipId = player.GetData<int>("CURRENT_DEALERSHIP_ID");

            var dealership = _dealerships.FirstOrDefault(d => d.Id == dealershipId);
            if (dealership == null || dealership.DisplayVehicle == null || !dealership.DisplayVehicle.Exists)
            {
                player.SendNotification("~r~Das Ausstellungsfahrzeug konnte nicht gefunden werden.");
                return;
            }

            if (!_vehicleDefinitions.TryGetValue(model, out var vehicleDef))
            {
                player.SendNotification("~r~Ein Fehler ist aufgetreten. Fahrzeug nicht im Katalog.");
                return;
            }

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null) return;

            if (forFaction)
            {
                if (!vehicleDef.IsFactionBuyable)
                {
                    player.SendNotification("~r~Dieses Fahrzeug kann nicht für eine Fraktion erworben werden.");
                    return;
                }
                if (acc.Fraktion == 0)
                {
                    player.SendNotification("~r~Du bist in keiner Fraktion.");
                    return;
                }
                if (Fglobalconf.FraktionsCommands.GetMemberRank(acc.ID, acc.Fraktion) < 10)
                {
                    player.SendNotification("~r~Dein Fraktionsrang (mind. 10) ist zu niedrig.");
                    return;
                }
            }

            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(acc.ID);
            if (bankAccount == null || bankAccount.Kontostand < vehicleDef.Price)
            {
                player.SendNotification("~r~Dein Kontostand ist nicht ausreichend gedeckt.");
                return;
            }

            decimal originalBalance = bankAccount.Kontostand;
            decimal newBalance = originalBalance - vehicleDef.Price;
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, newBalance);

            Vehicle vehicleToOwn = dealership.DisplayVehicle;
            var newVehSafe = new VehSafe
            {
                OwnerId = acc.ID,
                IsFactionVehicle = forFaction,
                FactionId = forFaction ? acc.Fraktion : 0,
                ModelName = vehicleDef.Model,
                NumberPlate = "",
                Modifications = "{}",
                PosX = vehicleToOwn.Position.X,
                PosY = vehicleToOwn.Position.Y,
                PosZ = vehicleToOwn.Position.Z,
                Heading = vehicleToOwn.Heading,
                Health = 1000f,
                Status = 0,
                IsInGarage = false,
                GarageId = null,
                ColorPrimary = vehicleToOwn.PrimaryColor,
                ColorSecondary = vehicleToOwn.SecondaryColor
            };

            int newVehicleId = VehSafeData.SaveVehicle(newVehSafe);

            if (newVehicleId > 0)
            {
                vehicleToOwn.ResetData("IS_DEALERSHIP_VEHICLE");
                vehicleToOwn.ResetData("DEALERSHIP_ID");
                vehicleToOwn.SetData("vehsafe_id", newVehicleId);
                vehicleToOwn.NumberPlate = newVehSafe.NumberPlate;
                vehicleToOwn.Locked = false;

                dealership.DisplayVehicle = null;

                player.SetIntoVehicle(vehicleToOwn, (int)VehicleSeat.Driver);

                player.SendNotification($"~g~Du hast erfolgreich einen {vehicleDef.DisplayName} für ${vehicleDef.Price:N0} gekauft!");
                player.TriggerEvent("client:dealership:closeMenu");
            }
            else
            {
                player.SendNotification("~r~Fehler beim Speichern deines neuen Fahrzeugs. Der Kauf wird rückgängig gemacht.");
                await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, originalBalance);

                NAPI.Util.ConsoleOutput($"[DEALERSHIP CRITICAL] Konnte vehsafe für Spieler {acc.Name} nicht erstellen. Kaufpreis wurde zurückgebucht.");
            }
        }

        private string GenerateRandomPlate()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}