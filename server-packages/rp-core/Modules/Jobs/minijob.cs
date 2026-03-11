using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RPCore.Datenbank;

namespace RPCore.Jobs
{
    // Definiert eine Busroute mit Namen, Belohnung und einer Liste von Haltestellen
    public class BusJobRoute
    {
        public string RouteName { get; }
        public int Reward { get; }
        public List<Vector3> Stops { get; }

        public BusJobRoute(string routeName, int reward, List<Vector3> stops)
        {
            RouteName = routeName;
            Reward = reward;
            Stops = stops;
        }
    }

    public class BusJob : Script
    {
        // Konfiguration
        private static readonly Vector3 BusDriverNPCPosition = new Vector3(449.991f, -658.367f, 28.443f);
        private static readonly Vector3 BusSpawnPosition = new Vector3(433.916f, -607.706f, 28.496f);
        private static readonly float BusSpawnHeading = 176.52296f;

        private static readonly List<BusJobRoute> BusRoutes = new List<BusJobRoute>
        {
            new BusJobRoute("Stadt-Ring West", 1250, new List<Vector3>
            {
                new Vector3(401.187f, -733.555f, 28.5f),
                new Vector3(233.15f, -788.5f, 32.5f),
                new Vector3(-58.5f, -1030.5f, 27.5f),
                new Vector3(-415.5f, -865.5f, 29f),
                new Vector3(-620.5f, -635.5f, 31f)
            }),
            new BusJobRoute("Zentrum-Tour", 1750, new List<Vector3>
            {
                new Vector3(395.376f, -995.417f, 29.3f),
                new Vector3(135.5f, -1280.5f, 29.3f),
                new Vector3(-160.5f, -1670.5f, 34f),
                new Vector3(10.5f, -1855.5f, 27f),
                new Vector3(325.5f, -1990.5f, 24.5f)
            }),
            new BusJobRoute("Vinewood-Express", 2500, new List<Vector3>
            {
                new Vector3(783.860f, -1112.211f, 25.5f),
                new Vector3(825.5f, -450.5f, 37.5f),
                new Vector3(420.5f, 160.5f, 103f),
                new Vector3(-195.5f, 430.5f, 100f),
                new Vector3(-705.5f, 275.5f, 82f)
            })
        };

        // Container für Busjob-Daten eines Spielers
        private class BusJobData
        {
            public Vehicle Bus { get; set; }
            public BusJobRoute Route { get; set; }
            public int CurrentStopIndex { get; set; }
            public DateTime StartTime { get; set; }
            public CancellationTokenSource TimerToken { get; set; }
        }
        private static readonly Dictionary<string, BusJobData> ActiveBusJobs = new Dictionary<string, BusJobData>();
        private static readonly Dictionary<string, DateTime> PenaltyTimes = new Dictionary<string, DateTime>();

        // ----- Busjob starten -----
        [RemoteEvent("OnPlayerPressF")]
        public void OnPlayerPressF(Player player)
        {
            if (player.Position.DistanceTo(BusDriverNPCPosition) > 3.0f) return;

            if (ActiveBusJobs.ContainsKey(player.Name))
            {
                player.SendNotification("Du hast bereits einen aktiven Busjob!");
                return;
            }

            if (PenaltyTimes.TryGetValue(player.Name, out DateTime penaltyEnd) && DateTime.Now < penaltyEnd)
            {
                TimeSpan remaining = penaltyEnd - DateTime.Now;
                player.SendNotification($"Du kannst noch keinen neuen Busjob annehmen. Warte {remaining.Minutes} Minuten, {remaining.Seconds} Sekunden.");
                return;
            }

            Random rnd = new Random();
            BusJobRoute chosenRoute = BusRoutes[rnd.Next(BusRoutes.Count)];

            Vehicle bus = NAPI.Vehicle.CreateVehicle(VehicleHash.Bus, BusSpawnPosition, BusSpawnHeading, 0, 0, "BUSJOB");
            if (bus == null)
            {
                player.SendNotification("Fehler: Bus konnte nicht gespawnt werden! Versuch es erneut.");
                return;
            }

            BusJobData jobData = new BusJobData()
            {
                Bus = bus,
                Route = chosenRoute,
                CurrentStopIndex = 0,
                StartTime = DateTime.Now,
                TimerToken = new CancellationTokenSource()
            };

            ActiveBusJobs[player.Name] = jobData;
            player.SendNotification($"Busjob gestartet! Route: {chosenRoute.RouteName}. Folge den Wegpunkten. Du hast maximal 20 Minuten Zeit.");

            StartJobTimer(player, jobData.TimerToken.Token);
            player.TriggerEvent("createBusJobBlip", bus.Position.X, bus.Position.Y, bus.Position.Z);
        }

        private void StartJobTimer(Player player, CancellationToken token)
        {
            Task.Run(async () =>
            {
                try
                {
                    // 15-Minuten-Warnung
                    await Task.Delay(TimeSpan.FromMinutes(15), token);
                    if (player.Exists && ActiveBusJobs.ContainsKey(player.Name))
                        NAPI.Task.Run(() => player.SendNotification("Warnung: Noch 5 Minuten Zeit für deinen Busjob!"));

                    // 20-Minuten-Timeout
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                    if (player.Exists && ActiveBusJobs.ContainsKey(player.Name))
                    {
                        NAPI.Task.Run(() => {
                            player.SendNotification("Busjob abgelaufen! Du erhältst nun eine 15 Minuten Sperre.");
                            CancelJobWithPenalty(player);
                        });
                    }
                }
                catch (TaskCanceledException) { /* Timer wurde erfolgreich abgebrochen */ }
            }, token);
        }

        // ----- Spieler steigt in den Bus ein -----
        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seat)
        {
            if (!ActiveBusJobs.TryGetValue(player.Name, out BusJobData jobData) || vehicle != jobData.Bus) return;

            player.SendNotification("Du bist in deinem Bus! Fahre zur nächsten Haltestelle.");
            player.TriggerEvent("removeBusJobBlip");
            SetNextStop(player);
        }

        // ----- Nächste Bushaltestelle setzen -----
        private void SetNextStop(Player player)
        {
            if (!ActiveBusJobs.TryGetValue(player.Name, out BusJobData jobData)) return;

            if (jobData.CurrentStopIndex >= jobData.Route.Stops.Count)
            {
                CompleteBusDepot(player);
                return;
            }
            Vector3 nextStop = jobData.Route.Stops[jobData.CurrentStopIndex];
            player.SendNotification($"Fahre zur Haltestelle {jobData.CurrentStopIndex + 1} von {jobData.Route.Stops.Count}.");
            player.TriggerEvent("setBusStopWaypoint", nextStop.X, nextStop.Y);
        }

        // ----- Bushaltestelle erreicht -----
        [RemoteEvent("BusStopReached")]
        public void OnBusStopReached(Player player)
        {
            if (!ActiveBusJobs.TryGetValue(player.Name, out BusJobData jobData) || player.Vehicle != jobData.Bus) return;

            Vector3 currentStopPos = jobData.Route.Stops[jobData.CurrentStopIndex];
            if (player.Position.DistanceTo(currentStopPos) > 10.0f) return;

            player.SendNotification("Haltestelle erreicht. Warte 5 Sekunden...");

            jobData.Bus.EngineStatus = false;

            NAPI.Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (!player.Exists || !ActiveBusJobs.ContainsKey(player.Name)) return;

                jobData.Bus.EngineStatus = true;
                jobData.CurrentStopIndex++;
                SetNextStop(player);
            });
        }

        // ----- Busjob abbrechen -----
        [Command("cancelbusjob")]
        public void CancelBusJobCommand(Player player)
        {
            if (!ActiveBusJobs.ContainsKey(player.Name))
            {
                player.SendNotification("Kein aktiver Busjob gefunden.");
                return;
            }
            CancelJobWithPenalty(player);
            player.SendNotification("Busjob abgebrochen. Du erhältst eine 15 Minuten Sperre.");
        }

        private void CancelJobWithPenalty(Player player)
        {
            if (!ActiveBusJobs.TryGetValue(player.Name, out BusJobData jobData)) return;

            jobData.TimerToken.Cancel();

            if (jobData.Bus != null && jobData.Bus.Exists)
                jobData.Bus.Delete();

            ActiveBusJobs.Remove(player.Name);
            PenaltyTimes[player.Name] = DateTime.Now.AddMinutes(15);
            player.TriggerEvent("clearBusJob");
        }

        // ----- Busjob abschließen -----
        private async void CompleteBusDepot(Player player)
        {
            if (!ActiveBusJobs.TryGetValue(player.Name, out BusJobData jobData)) return;

            int reward = jobData.Route.Reward;

            // GEÄNDERT: Benachrichtigung angepasst
            player.SendNotification($"Busjob abgeschlossen! ${reward} wurden auf dein Bankkonto überwiesen.");

            jobData.TimerToken.Cancel();

            if (jobData.Bus != null && jobData.Bus.Exists)
                jobData.Bus.Delete();

            // GEÄNDERT: Asynchroner Aufruf zur Aktualisierung des Bankkontos
            await UpdatePlayerBankAccountAsync(player, reward);

            ActiveBusJobs.Remove(player.Name);
            player.TriggerEvent("clearBusJob");
        }

        // ----- Kontostand aktualisieren (jetzt für Bankkonto) -----
        private async Task UpdatePlayerBankAccountAsync(Player player, int amount)
        {
            try
            {
                var account = player.GetData<Accounts>(Accounts.Account_Key);
                if (account == null)
                {
                    player.SendNotification("~r~Fehler: Dein Account konnte nicht geladen werden.");
                    return;
                }

                var bankAccount = await Datenbank.Datenbank.GetOrCreatePlayerBankAccount(account.ID);
                if (bankAccount == null)
                {
                    player.SendNotification("~r~Fehler: Dein Bankkonto konnte nicht gefunden werden.");
                    return;
                }

                bankAccount.Kontostand += amount;
                bool success = await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);

                if (!success)
                {
                    player.SendNotification("~r~Ein Fehler ist bei der Überweisung aufgetreten.");
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"Fehler beim Aktualisieren des Bankkontos für {player.Name}: {ex.Message}");
            }
        }
    }
}