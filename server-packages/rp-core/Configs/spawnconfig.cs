using RPCore.Commands;
using RPCore.Database;
using RPCore.FraktionsSystem;
using RPCore.Haussystem;
using RPCore.Notifications;
using RPCore.Database;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RPCore.Dealership;
using RPCore.Features;

namespace RPCore.Configs
{
    public class Main : Script
    {
        private List<VehicleData> _vehicles = new List<VehicleData>();
        public const float MIN_ENGINE_HEALTH = 300f;
      

      
        
        
        public Main()
        {
            //
            

        }
        [ServerEvent(Event.ResourceStart)]
        public async void OnResourceStart()
        {
            if (DbSettings.LoadServerSettings())
            {
                gamemode.Datenbank.Datenbank.InitConnection();
            }

            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.ClientEvent.TriggerClientEventForAll("Heal");

            NAPI.Util.ConsoleOutput("[INFO] Resource startet: Spawning aller gespeicherten Fahrzeuge...");
            LoadVehicleData();
            VehicleSpawn();
            SpawnNewbieCars();

            StartFullSaveTimer();

            gamemode.FraktionsSystem.DutyManager.StartGlobalDutyTimer();
            gamemode.Dealership.DealershipManager.LoadAllDealershipData();

            NAPI.Server.SetDefaultSpawnLocation(new Vector3(-1036.182, -2729.434, 13.75665));

            await HouseManager.InitializeHouses();
            Houseinterrior.LoadInteriorsFromJson();

            IplManager.LoadAllCustomIpls();





            // --- Fallschirm-Pickup erstellen ---

            FallschirmWorkaround.Instance?.CreateAllParachuteSpawns();
            




            CreateBlips();
            LoadIPL();
           

            NAPI.Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var player in NAPI.Pools.GetAllPlayers())
                    {
                        var account = player.GetData<Accounts>(Accounts.Account_Key);
                        if (account != null)
                        {
                            await ProcessPlayerPayday(player, account);
                        }
                    }
                    await Task.Delay(60000);
                }
            });
        }

        public void VehicleSpawn()
        {
            CleanupVehicles();
            GarageSystem.CreateAllGarages();
            VehSafeData.SpawnAllVehicles();

            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
            {
                if (veh.HasData("vehsafe_id"))
                {
                    veh.Health = 1000;
                    StartOptimizedVehicleMonitoring(veh);
                }
            }
            VPDES.PreventDamagedEngineStart();
            //
            NAPI.Util.ConsoleOutput("Server gestartet!");
        }
        
        public void SpawnNewbieCars()
        {
            try
            {
                string filePath = @"C:\RAGEMP\server-files\client_packages\gameconf\NewbieCars.json";

                if (!File.Exists(filePath))
                {
                    NAPI.Util.ConsoleOutput($"[WARNUNG] NewbieCars.json nicht gefunden: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);
                List<VehicleData> newbieCars = JsonConvert.DeserializeObject<List<VehicleData>>(json);

                if (newbieCars == null || newbieCars.Count == 0)
                {
                    NAPI.Util.ConsoleOutput("[INFO] Es wurden keine NewbieCars in der JSON-Datei gefunden.");
                    return;
                }

                foreach (var car in newbieCars)
                {
                    uint vehHash = NAPI.Util.GetHashKey(car.Vehicle);
                    Vector3 pos = new Vector3(car.Position.X, car.Position.Y, car.Position.Z);
                    Vehicle veh = NAPI.Vehicle.CreateVehicle(vehHash, pos, car.Rotation.Z, car.Color1, car.Color2);
                    veh.NumberPlate = car.NumberPlate;
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Spawnen der NewbieCars: {ex.Message}");
            }
        }

        private void CleanupVehicles()
        {
            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
            {
                if (veh.HasData("vehsafe_id"))
                {
                    veh.Delete();
                }
            }
            NAPI.Util.ConsoleOutput("[GARAGE] Alle instanziierten vehsafe Fahrzeuge wurden entfernt.");
        }

        private static async Task ProcessPlayerPayday(Player player, Accounts account)
        {
            try
            {
                int sessionSeconds = (int)(DateTime.Now - account.loginTimestamp).TotalSeconds;
                int totalPlayedTime = account.storedPlaytime + sessionSeconds;

                int lastPayday = await Task.Run(() => gamemode.Datenbank.Datenbank.GetLastPayday(account.ID));

                const int oneHourInSeconds = 3600;
                int elapsedSinceLastPayday = totalPlayedTime - lastPayday;
                int intervals = elapsedSinceLastPayday / oneHourInSeconds;

                if (intervals > 0)
                {
                    PlayerBankAccount bankAccount = await gamemode.Datenbank.Datenbank.GetOrCreatePlayerBankAccount(account.ID);

                    if (bankAccount == null)
                    {
                        NAPI.Util.ConsoleOutput($"[FEHLER] Payday: Konnte Bankkonto für Spieler {player.Name} (ID: {account.ID}) nicht finden oder erstellen.");
                        return;
                    }

                    int totalPaydayAmount = 0;
                    for (int i = 0; i < intervals; i++)
                    {
                        int gehalt = 500;
                        switch (account.Adminlevel)
                        {
                            case 1: gehalt += 50; break;
                            case 2: gehalt += 100; break;
                            case 3: gehalt += 150; break;
                            case 4: gehalt += 200; break;
                            case 5: gehalt += 250; break;
                        }
                        bankAccount.Kontostand += gehalt;
                        totalPaydayAmount += gehalt;
                    }

                    bool success = await gamemode.Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);

                    if (success)
                    {
                        player.SendNotification($"~g~Dein Gehalt von {totalPaydayAmount}€ wurde auf dein Bankkonto überwiesen.");
                        int newLastPayday = lastPayday + intervals * oneHourInSeconds;
                        await Task.Run(() => gamemode.Datenbank.Datenbank.UpdateLastPayday(account.ID, newLastPayday));
                        gamemode.Datenbank.Datenbank.AccountSpeichern(player);
                        NAPI.Util.ConsoleOutput($"[PAYDAY] {player.Name} hat {totalPaydayAmount}€ auf das Konto überwiesen bekommen. Neuer Kontostand: {bankAccount.Kontostand}");
                    }
                    else
                    {
                        NAPI.Util.ConsoleOutput($"[FEHLER] Payday: Update des Bankkontos für {player.Name} fehlgeschlagen.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEHLER] Fehler bei der Payday-Verarbeitung von {player.Name}: {ex.Message}");
            }
        }

        public void LoadVehicleData()
        {
            string path = "C:\\RAGEMP\\server-files\\dotnet\\resources\\gamemode\\gamemode\\VehicleData.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _vehicles = JsonConvert.DeserializeObject<List<VehicleData>>(json);
            }
        }

        public void LoadIPL()
        {
            Console.WriteLine("Interiordaten aus der JSON-Datei geladen.");
        }

        private void CreateBlips()
        {
            BlipManager.CreateAllStaticBlips();
        }

        [ServerEvent(Event.PlayerSpawn)]
        public void OnPlayerSpawn(Player player)
        {
            if (!player.HasData(Accounts.Account_Key))
                return;

            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);

            if (account.DutyStart != null)
            {
                DutyManager.OnDutyPlayers[account.ID] = account.DutyStart.Value;
                player.SendNotification("~y~Dein Dienst wurde automatisch fortgesetzt. Setze deine Arbeit fort.");
                NAPI.Util.ConsoleOutput($"[DutyManager] Dienst für {player.Name} nach Relog fortgesetzt.");
            }

            bool isDead = Datenbank.Datenbank.IsPlayerDead(player);

            if (isDead)
            {
                int remainingTime = Datenbank.Datenbank.LoadRemainingTimeFromDatabase(player);
                Datenbank.Datenbank.StartDeathTimer(player, remainingTime);
                player.TriggerEvent("showDeathScreen");
                return;
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "loadCayoPericoMap");

                Datenbank.Datenbank.LadeSpielerPositionAusDB(player, account);
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            var account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account != null)
            {
                DateTime disconnectTime = DateTime.Now;
                int sessionSeconds = (int)(disconnectTime - account.loginTimestamp).TotalSeconds;
                account.storedPlaytime += sessionSeconds;
                account.played_time = account.storedPlaytime;

                DutyManager.HandlePlayerDisconnect(player);

                gamemode.Datenbank.Datenbank.AccountSpeichern(player);
                Datenbank.Datenbank.SpeichereSpielerPosition(player);
                if (player.Vehicle != null && player.Vehicle.HasData("vehsafe_id"))
                {
                    SaveVehicleData(player.Vehicle);
                }
            }
        }

        [ServerEvent(Event.PlayerDamage)]
        public void OnPlayerDamage(Player player, float healthLoss, float armorLoss)
        {
            if (player.HasData(Accounts.Account_Key))
            {
                gamemode.Datenbank.Datenbank.AccountSpeichern(player);
                if (player.Vehicle != null && player.Vehicle.HasData("vehsafe_id"))
                {
                    SaveVehicleData(player.Vehicle);
                }
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            player.TriggerEvent("startDeathEffect");
            gamemode.Datenbank.Datenbank.SetPlayerDeathStatus(player, 0, 0);
            int remainingTime = gamemode.Datenbank.Datenbank.LoadRemainingTimeFromDatabase(player);
            gamemode.Datenbank.Datenbank.StartDeathTimer(player, remainingTime);

            gamemode.Datenbank.Datenbank.AccountSpeichern(player);
            gamemode.Datenbank.Datenbank.SpeichereSpielerPosition(player);
            if (player.Vehicle != null && player.Vehicle.HasData("vehsafe_id"))
            {
                SaveVehicleData(player.Vehicle);
            }
        }

        [ServerEvent(Event.VehicleDamage)]
        public void OnVehicleDamage(Vehicle veh, float oldHealth, float currentHealth)
        {
            veh.SetData("ServerVehicleHealth", currentHealth);
            SaveVehicleData(veh);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatId)
        {
            var account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account != null)
            {
                account.Seatbelt = false;
                player.TriggerEvent("updateSeatbelt", false);

                if (vehicle.HasData("vehsafe_id"))
                {
                    if (!vehicle.HasData("engine_status"))
                    {
                        vehicle.SetData("engine_status", false);
                        vehicle.EngineStatus = false;
                    }
                    else
                    {
                        vehicle.EngineStatus = vehicle.GetData<bool>("engine_status");
                    }

                    if (vehicle.Health < 300)
                    {
                        vehicle.EngineStatus = false;
                        player.SendNotification("~r~Dieses Fahrzeug ist zu beschädigt, um den Motor zu starten!");
                    }

                    SaveVehicleData(vehicle);
                    gamemode.Datenbank.Datenbank.AccountSpeichern(player);
                    gamemode.Datenbank.Datenbank.SpeichereSpielerPosition(player);
                }
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            var account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account != null)
            {
                account.Seatbelt = false;
                player.TriggerEvent("updateSeatbelt", false);
                if (vehicle.HasData("vehsafe_id"))
                {
                    SaveVehicleData(vehicle);
                    gamemode.Datenbank.Datenbank.AccountSpeichern(player);
                    Datenbank.Datenbank.SpeichereSpielerPosition(player);
                }
            }
        }

        [ServerEvent(Event.Update)]
        public void OnUpdate_AdminSystem()
        {
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p.HasData("IS_FROZEN_BY_ADMIN") && p.GetData<bool>("IS_FROZEN_BY_ADMIN"))
                {
                    p.Velocity = new Vector3(0, 0, 0);
                }
            }
        }

        public static void SaveVehicleData(Vehicle veh)
        {
            if (veh == null || !veh.Exists || !veh.HasData("vehsafe_id"))
                return;

            int vehSafeId = veh.GetData<int>("vehsafe_id");
            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null)
                return;

            vehData.PosX = veh.Position.X;
            vehData.PosY = veh.Position.Y;
            vehData.PosZ = veh.Position.Z;
            vehData.Heading = veh.Rotation.Z;
            vehData.ColorPrimary = veh.PrimaryColor;
            vehData.ColorSecondary = veh.SecondaryColor;

            float finalHealth = veh.HasData("ServerVehicleHealth") ? veh.GetData<float>("ServerVehicleHealth") : veh.Health;
            vehData.Health = finalHealth;

            vehData.Status = (finalHealth < VehSafe.MIN_ENGINE_HEALTH) ? 1 : 0;

            VehSafeData.UpdateVehicle(vehData);
        }

        // --- WIEDERHERGESTELLTE METHODEN ---
        public static void StartOptimizedMonitoring(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Monitoring: Kein Account gefunden für Spieler {player.Name}");
                return;
            }

            Vector3 lastPosition = player.Position;
            int lastHealth = player.Health;
            int lastArmor = player.Armor;

            NAPI.Task.Run(async () =>
            {
                while (player.Exists)
                {
                    try
                    {
                        bool hasChanged = player.Position != lastPosition ||
                                          player.Health != lastHealth ||
                                          player.Armor != lastArmor;

                        if (hasChanged)
                        {
                            lastPosition = player.Position;
                            lastHealth = player.Health;
                            lastArmor = player.Armor;
                            gamemode.Datenbank.Datenbank.AccountSpeichern(player);
                            Datenbank.Datenbank.SpeichereSpielerPosition(player);
                        }

                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput($"[ERROR] Monitoring-Fehler für Spieler {player.Name}: {ex.Message}");
                    }
                }
            });
        }

        public static void StartPlaytimeMonitoring(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Playtime Monitoring: Kein Account für Spieler {player.Name} gefunden.");
                return;
            }

            if (account.loginTimestamp == default(DateTime))
                account.loginTimestamp = DateTime.Now;

            NAPI.Task.Run(async () =>
            {
                while (player.Exists)
                {
                    try
                    {
                        int sessionSeconds = (int)(DateTime.Now - account.loginTimestamp).TotalSeconds;
                        int totalPlayedTime = account.storedPlaytime + sessionSeconds;
                        account.played_time = totalPlayedTime;
                        gamemode.Datenbank.Datenbank.UpdatePlaytimeInDatabase(account);
                        await Task.Delay(30000);
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Überwachen der Spielzeit für Spieler {player.Name}: {ex.Message}");
                    }
                }
            });
        }

        public static void StartOptimizedVehicleMonitoring(Vehicle vehicle)
        {
            if (!vehicle.HasData("vehsafe_id"))
            {
                NAPI.Util.ConsoleOutput("[ERROR] Fahrzeug-Monitoring: Dieses Fahrzeug hat keine vehsafe_id.");
                return;
            }

            Vector3 lastPosition = vehicle.Position;
            float lastHealth = vehicle.Health;
            float lastHeading = vehicle.Rotation.Z;

            NAPI.Task.Run(async () =>
            {
                while (vehicle.Exists)
                {
                    try
                    {
                        bool hasChanged = vehicle.Position != lastPosition ||
                                          vehicle.Health != lastHealth ||
                                          vehicle.Rotation.Z != lastHeading;

                        if (hasChanged)
                        {
                            lastPosition = vehicle.Position;
                            lastHealth = vehicle.Health;
                            lastHeading = vehicle.Rotation.Z;

                            SaveVehicleData(vehicle);
                        }

                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput($"[ERROR] Fahrzeug-Monitoring-Fehler: {ex.Message}");
                    }
                }
            });
        }
        // --- ENDE WIEDERHERGESTELLTE METHODEN ---

        public void StartFullSaveTimer()
        {
            NAPI.Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                        {
                            if (veh.HasData("vehsafe_id"))
                            {
                                SaveVehicleData(veh);
                            }
                        }

                        foreach (Player player in NAPI.Pools.GetAllPlayers())
                        {
                            if (player.HasData(Accounts.Account_Key))
                            {
                                gamemode.Datenbank.Datenbank.AccountSpeichern(player);
                                Datenbank.Datenbank.SpeichereSpielerPosition(player);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput($"[ERROR] Fehler beim Vollsave: {ex.Message}");
                    }
                    await Task.Delay(1200000);
                }
            });
        }

        public class PositionData { public float X { get; set; } public float Y { get; set; } public float Z { get; set; } }
        public class RotationData { public float X { get; set; } public float Y { get; set; } public float Z { get; set; } }
        public class VehicleData { public string Vehicle { get; set; } public PositionData Position { get; set; } public RotationData Rotation { get; set; } public int Color1 { get; set; } public int Color2 { get; set; } public string NumberPlate { get; set; } }
    }
}