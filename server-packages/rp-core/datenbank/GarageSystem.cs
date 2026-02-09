using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Hinzugefügt für asynchrone Operationen
using Newtonsoft.Json;
using RPCore.Database;
using RPCore.Commands;
using RPCore.Notifications;

namespace RPCore.datenbank
{
    public class GarageSystem : Script
    {
        private static Dictionary<int, Blip> _garageBlips = new Dictionary<int, Blip>();
        private static Dictionary<int, Marker> _garageMarkers = new Dictionary<int, Marker>();
        private static Dictionary<int, TextLabel> _garageLabels = new Dictionary<int, TextLabel>();

        public static void CreateAllGarages()
        {
            if (Datenbank.Datenbank.Garages == null || Datenbank.Datenbank.Garages.Count == 0)
            {
                NAPI.Util.ConsoleOutput("[GARAGE][WARN] Keine Garagen in Datenbank.Datenbank.Garages gefunden!");
                return;
            }

            foreach (var garage in Datenbank.Datenbank.Garages)
            {
                Marker marker = NAPI.Marker.CreateMarker(1u, garage.Position - new Vector3(0, 0, 1f),
                                       new Vector3(), new Vector3(), 1f,
                                       new Color(255, 165, 0, 200), true, 0);
                _garageMarkers.Add(garage.Id, marker);

                TextLabel label = NAPI.TextLabel.CreateTextLabel($"~w~{garage.Name}~n~~g~Drücke E zum interagieren",
                                                     garage.Position + new Vector3(0, 0, 0.5f),
                                                     8f, 0.75f, 4, new Color(255, 255, 255));
                _garageLabels.Add(garage.Id, label);
            }
        }

        [RemoteEvent("Client:Garage:RequestOpenUI")]
        public void OnPlayerRequestOpenGarageUI(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null)
            {
                player.SendNotification("~r~Dein Account konnte nicht geladen werden!");
                return;
            }

            Garage nearestGarage = GetNearestGarage(player.Position, 2.5f);

            if (nearestGarage != null)
            {
                if (nearestGarage.FraktionId > 0 && account.Fraktion != nearestGarage.FraktionId)
                {
                    player.SendNotification("~r~Du bist nicht berechtigt, diese Garage zu nutzen.");
                    return;
                }
                OpenGarageUI(player, nearestGarage.Id);
            }
        }

        public void OpenGarageUI(Player player, int garageId)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null)
            {
                player.SendNotification("~r~Dein Account konnte nicht geladen werden!");
                return;
            }

            Garage targetGarage = Datenbank.Datenbank.Garages.Find(g => g.Id == garageId);
            if (targetGarage == null)
            {
                player.SendNotification("~r~Diese Garage existiert nicht!");
                return;
            }

            List<VehSafe> playerVehicles = VehSafeData.LoadVehiclesForPlayer(account.ID, account.Fraktion);
            List<VehSafe> vehiclesInThisGarage = playerVehicles?.Where(v => v.IsInGarage && v.GarageId == garageId).ToList() ?? new List<VehSafe>();
            List<VehSafe> nearbyParkableVehicles = new List<VehSafe>();

            float searchRadius = 10.0f;
            foreach (Vehicle spawnedVeh in NAPI.Pools.GetAllVehicles())
            {
                if (player.Position.DistanceTo(spawnedVeh.Position) < searchRadius && spawnedVeh.HasData("vehsafe_id"))
                {
                    int vehSafeDbId = spawnedVeh.GetData<int>("vehsafe_id");
                    VehSafe parkedVeh = VehSafeData.GetVehicleById(vehSafeDbId);

                    if (parkedVeh != null && !parkedVeh.IsInGarage &&
                        (parkedVeh.OwnerId == account.ID || (parkedVeh.IsFactionVehicle && account.Fraktion == parkedVeh.FactionId)))
                    {
                        nearbyParkableVehicles.Add(parkedVeh);
                    }
                }
            }

            string inGarageJson = JsonConvert.SerializeObject(vehiclesInThisGarage);
            string nearbyParkableVehiclesJson = JsonConvert.SerializeObject(nearbyParkableVehicles);

            player.TriggerEvent("Client:Garage:ShowGarageBrowser", inGarageJson, nearbyParkableVehiclesJson, targetGarage.MaxVehicles);
        }

        [RemoteEvent("Server:Garage:RequestCloseUI")]
        public void OnPlayerRequestCloseGarageUI(Player player)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "Client:Garage:HideGarageBrowser");
        }

        [RemoteEvent("Server:Garage:SpawnVehicle")]
        public void SpawnVehicle(Player player, int vehSafeId)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null)
            {
                player.SendNotification("~r~Fehler: Account nicht geladen.");
                return;
            }

            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null)
            {
                player.SendNotification("~r~Fahrzeugdaten nicht gefunden.");
                return;
            }

            if ((vehData.FactionId > 0 && account.Fraktion != vehData.FactionId) || (vehData.FactionId == 0 && vehData.OwnerId != account.ID))
            {
                player.SendNotification("~r~Du bist nicht berechtigt, dieses Fahrzeug auszuparken.");
                return;
            }

            if (NAPI.Pools.GetAllVehicles().Any(v => v.HasData("vehsafe_id") && v.GetData<int>("vehsafe_id") == vehSafeId))
            {
                player.SendNotification("~r~Dieses Fahrzeug ist bereits in der Welt.");
                return;
            }

            if (!vehData.IsInGarage)
            {
                player.SendNotification("~r~Dieses Fahrzeug ist laut Datenbank nicht in einer Garage.");
                return;
            }

            Garage nearestGarage = GetNearestGarage(player.Position, 5.0f);
            if (nearestGarage == null)
            {
                player.SendNotification("~r~Keine Garage in der Nähe, um das Fahrzeug zu spawnen.");
                return;
            }

            Vector3 spawnPos = (vehData.PosX != 0 || vehData.PosY != 0 || vehData.PosZ != 0)
                ? new Vector3(vehData.PosX, vehData.PosY, vehData.PosZ)
                : nearestGarage.Position;
            float spawnHeading = (vehData.Heading != 0) ? vehData.Heading : nearestGarage.NpcHeading;

            uint vehHash = NAPI.Util.GetHashKey(vehData.ModelName);
            if (vehHash == 0)
            {
                player.SendNotification($"~r~Fehler: Ungültiges Fahrzeugmodell '{vehData.ModelName}'.");
                return;
            }

            Vehicle spawnedVeh = NAPI.Vehicle.CreateVehicle(vehHash, spawnPos, spawnHeading, vehData.ColorPrimary, vehData.ColorSecondary);
            spawnedVeh.SetData("vehsafe_id", vehSafeId);
            spawnedVeh.NumberPlate = vehData.NumberPlate;
            spawnedVeh.Locked = true;
            spawnedVeh.EngineStatus = false;
            spawnedVeh.Health = vehData.Health;

            vehData.IsInGarage = false;
            vehData.GarageId = 0;
            VehSafeData.UpdateVehicle(vehData);

            if (!string.IsNullOrEmpty(vehData.Modifications))
            {
                var mods = JsonConvert.DeserializeObject<Dictionary<int, int>>(vehData.Modifications);
                if (mods != null)
                {
                    foreach (var mod in mods)
                    {
                        spawnedVeh.SetMod(mod.Key, mod.Value);
                    }
                }
            }

            player.SendNotification($"~g~Dein {vehData.ModelName} wurde ausgeparkt!");
            NAPI.ClientEvent.TriggerClientEvent(player, "Client:Garage:HideGarageBrowser");
        }

        // GEÄNDERT: Die Methode ist jetzt "async void", um auf Datenbankabfragen warten zu können.
        [RemoteEvent("Server:Garage:StoreVehicle")]
        public async void StoreVehicle(Player player, int vehSafeId)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account == null)
            {
                player.SendNotification("~r~Fehler: Account nicht geladen.");
                return;
            }

            VehSafe vehData = VehSafeData.GetVehicleById(vehSafeId);
            if (vehData == null)
            {
                player.SendNotification("~r~Fahrzeugdaten nicht gefunden.");
                return;
            }

            if ((vehData.FactionId > 0 && account.Fraktion != vehData.FactionId) || (vehData.FactionId == 0 && vehData.OwnerId != account.ID))
            {
                player.SendNotification("~r~Du bist nicht berechtigt, dieses Fahrzeug einzulagern!");
                return;
            }

            if (vehData.IsInGarage)
            {
                player.SendNotification("~y~Dieses Fahrzeug ist bereits eingelagert.");
                return;
            }

            Vehicle targetVehicle = NAPI.Pools.GetAllVehicles().FirstOrDefault(v => v.HasData("vehsafe_id") && v.GetData<int>("vehsafe_id") == vehSafeId);
            if (targetVehicle == null)
            {
                player.SendNotification("~r~Fahrzeug konnte nicht in der Welt gefunden werden.");
                return;
            }

            Garage nearestGarage = GetNearestGarage(player.Position, 15.0f);
            if (nearestGarage == null)
            {
                player.SendNotification("~r~Keine Garage in der Nähe.");
                return;
            }
            if (player.Position.DistanceTo(targetVehicle.Position) > 15.0f)
            {
                player.SendNotification("~r~Das Fahrzeug ist zu weit entfernt, um es einzulagern.");
                return;
            }

            // --- NEU: Parkgebühr prüfen und abbuchen ---
            const int parkFee = 50;
            var bankAccount = await Datenbank.Datenbank.GetOrCreatePlayerBankAccount(account.ID);

            if (bankAccount == null)
            {
                player.SendNotification("~r~Dein Bankkonto konnte nicht gefunden werden. Einparken fehlgeschlagen.");
                return;
            }

            if (bankAccount.Kontostand < parkFee)
            {
                player.SendNotification($"~r~Du hast nicht genügend Geld auf dem Konto, um die Parkgebühr von {parkFee}$ zu bezahlen.");
                return; // Prozess hier abbrechen
            }

            // Gebühr abziehen und Kontostand in der DB aktualisieren
            bankAccount.Kontostand -= parkFee;
            bool success = await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);

            // Prüfen, ob die Datenbank-Aktualisierung erfolgreich war
            if (!success)
            {
                player.SendNotification("~r~Ein Datenbankfehler ist beim Abbuchen der Gebühr aufgetreten. Bitte versuche es erneut.");
                // WICHTIG: Den abgebuchten Betrag im lokalen Objekt zurücksetzen, da die DB nicht aktualisiert wurde.
                bankAccount.Kontostand += parkFee;
                return; // Prozess hier abbrechen
            }
            // --- ENDE NEUE LOGIK ---

            vehData.IsInGarage = true;
            vehData.GarageId = nearestGarage.Id;
            vehData.PosX = targetVehicle.Position.X;
            vehData.PosY = targetVehicle.Position.Y;
            vehData.PosZ = targetVehicle.Position.Z;
            vehData.Heading = targetVehicle.Heading;
            vehData.Health = targetVehicle.Health;
            vehData.ColorPrimary = targetVehicle.PrimaryColor;
            vehData.ColorSecondary = targetVehicle.SecondaryColor;

            if (targetVehicle.HasData("modifications"))
            {
                vehData.Modifications = targetVehicle.GetData<string>("modifications");
            }

            VehSafeData.UpdateVehicle(vehData);

            targetVehicle.Delete();

            // GEÄNDERT: Benachrichtigung mit Info über die Gebühr
            player.SendNotification($"~g~Dein Fahrzeug wurde eingelagert! {parkFee}$ Parkgebühr wurden von deinem Konto abgebucht.");
            NAPI.ClientEvent.TriggerClientEvent(player, "Client:Garage:HideGarageBrowser");
        }

        private Garage GetNearestGarage(Vector3 playerPos, float maxDistance)
        {
            return Datenbank.Datenbank.Garages?
                .Where(g => playerPos.DistanceTo(g.Position) < maxDistance)
                .OrderBy(g => playerPos.DistanceTo(g.Position))
                .FirstOrDefault();
        }
    }
}