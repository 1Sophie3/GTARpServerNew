using System;
using System.Collections.Generic;
using GTANetworkAPI;
using RPCore.Database;
using RPCore.Commands;
using Newtonsoft.Json;
using RPCore.FraktionsSystem;
using RPCore.Notifications;

namespace RPCore.Features
{
    public class WheelMenuItem
    {
        public string Name { get; set; }
        public string Action { get; set; }
        public bool RequiresClick { get; set; } = false;
    }

    public class VehicleWheelMenu : Script
    {
        [RemoteEvent("requestVehicleWheelMenu")]
        public void OnRequestVehicleWheelMenu(Player player, Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists) return;

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null) return;

            VehSafe vehData = null;
            if (vehicle.HasData("vehsafe_id"))
            {
                vehData = VehSafeData.GetVehicleById(vehicle.GetData<int>("vehsafe_id"));
            }

            // Admins können immer interagieren, normale Spieler nur mit registrierten Fahrzeugen
            if (vehData == null && !acc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin))
            {
                // Ausnahme für LSPD/LSCS, damit sie auch mit nicht-registrierten Fahrzeugen interagieren können
                if (acc.Fraktion != 1 && acc.Fraktion != 3)
                {
                    return;
                }
            }

            var menuItems = new List<WheelMenuItem>();
            string centerText;

            // Variablen für die Logik definieren
            bool isAdminOnDuty = player.HasData("ON_ADUTY") && player.GetData<bool>("ON_ADUTY") && acc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin);
            bool isOnFactionDuty = acc.DutyStart != null;
            bool hasKeys = vehData != null && CanControlVehicle(acc, vehData);
            bool isInVehicle = player.IsInVehicle && player.Vehicle == vehicle;
            bool isLspd = acc.Fraktion == 1;
            bool isLscs = acc.Fraktion == 3;


            // ======================= SAUBERE LOGIK-STRUKTUR =======================

            // 1. ZUERST: Entscheiden, wer die Details (ID, Kennzeichen) sehen darf.
            //    Das sind Admins, Besitzer ODER Fraktionsmitglieder im Dienst.
            bool darfDetailsSehen = isAdminOnDuty || hasKeys || (isOnFactionDuty && (isLspd || isLscs));

            // 2. DANN: Den Text basierend auf dieser einen Bedingung setzen.
            if (darfDetailsSehen && vehData != null)
            {
                // Wenn ja, zeige die Details.
                centerText = $"ID: {vehData.Id}<br>Kennzeichen: {vehData.NumberPlate}";
            }
            else
            {
                // Wenn nein, zeige nur den Modellnamen oder einen Standardtext.
                centerText = vehData?.ModelName ?? "Fahrzeug";
            }

            // 3. JETZT: Die Menüpunkte hinzufügen.

            // Menüpunkte für Besitzer oder Admins
            if (hasKeys || isAdminOnDuty)
            {
                menuItems.Add(new WheelMenuItem { Name = vehicle.Locked ? "Aufschließen" : "Abschließen", Action = "toggle_lock" });

                if (isInVehicle)
                {
                    // Wir nutzen hier die SharedData Variable, um den korrekten Status anzuzeigen!
                    bool engineIsOn = vehicle.HasSharedData("VEH_ENGINE_STATE") && vehicle.GetSharedData<bool>("VEH_ENGINE_STATE");
                    menuItems.Add(new WheelMenuItem { Name = engineIsOn ? "Motor aus" : "Motor an", Action = "toggle_engine" });
                    menuItems.Add(new WheelMenuItem { Name = acc.Seatbelt ? "Abschnallen" : "Anschnallen", Action = "toggle_seatbelt" });
                }

                menuItems.Add(new WheelMenuItem { Name = "Kofferraum", Action = "toggle_trunk" });
            }
            else // Menüpunkte für "Fremde"
            {
                menuItems.Add(new WheelMenuItem { Name = "Aufschließen (Versuch)", Action = "toggle_lock" });
            }

            // Zusätzliche Menüpunkte für Fraktionen im Dienst
            if (!isAdminOnDuty && vehData != null && isOnFactionDuty)
            {
                if (isLspd)
                {
                    int lspdRank = DutyManager.GetMemberRank(acc.ID, acc.Fraktion);
                    if (lspdRank >= 2 && vehData.Status != 1)
                    {
                        menuItems.Add(new WheelMenuItem { Name = "Stilllegen", Action = "lspd_impound", RequiresClick = true });
                    }
                    if (lspdRank >= 3 && vehData.Status == 1)
                    {
                        menuItems.Add(new WheelMenuItem { Name = "Stillegung aufheben", Action = "lspd_unimpound", RequiresClick = true });
                    }
                }

                if (isLscs)
                {
                    if (vehicle.Health < 999)
                    {
                        menuItems.Add(new WheelMenuItem { Name = "Reparieren", Action = "lscs_repair", RequiresClick = true });
                    }
                    menuItems.Add(new WheelMenuItem { Name = "Abschleppen", Action = "lscs_tow", RequiresClick = true });
                }
            }

            // ======================= ENDE DER NEUEN LOGIK =======================

            if (menuItems.Count > 0)
            {
                player.TriggerEvent("showVehicleWheelMenu", JsonConvert.SerializeObject(menuItems), centerText);
            }
        }

        [RemoteEvent("vehicleWheelMenuAction")]
        public void OnVehicleWheelMenuAction(Player player, Vehicle vehicle, string action)
        {
            if (vehicle == null || !vehicle.Exists || !player.Exists) return;

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null) return;

            VehSafe vehData = null;
            if (vehicle.HasData("vehsafe_id"))
            {
                vehData = VehSafeData.GetVehicleById(vehicle.GetData<int>("vehsafe_id"));
            }

            bool isAdminOnDuty = player.HasData("ON_ADUTY") && player.GetData<bool>("ON_ADUTY") && acc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin);
            bool isOnFactionDuty = acc.DutyStart != null;
            bool hasKeys = vehData != null && CanControlVehicle(acc, vehData);

            switch (action)
            {
                case "toggle_lock":
                    if (hasKeys || isAdminOnDuty)
                    {
                        vehicle.Locked = !vehicle.Locked;
                        player.PlayAnimation("anim@mp_player_intmenu@key_fob", "fob_click", 1);
                        Utils.sendNotification(player, vehicle.Locked ? "Fahrzeug abgeschlossen." : "Fahrzeug aufgeschlossen.", "fas fa-key");
                    }
                    else
                    {
                        player.SendNotification("~r~Du hast keinen Schlüssel für dieses Fahrzeug.");
                    }
                    break;

                case "toggle_engine":
                    if (player.IsInVehicle && player.Vehicle == vehicle)
                    {
                        if (hasKeys || isAdminOnDuty)
                        {
                            if (vehicle.Health < VehSafe.MIN_ENGINE_HEALTH)
                            {
                                player.SendNotification("~r~Der Motor ist zu stark beschädigt!");
                                return;
                            }

                            // 1. Schalte den Motorstatus wie gewohnt um
                            vehicle.EngineStatus = !vehicle.EngineStatus;

                            // 2. KORREKTUR: Synchronisiere den neuen Zustand mit dem Client
                            vehicle.SetSharedData("VEH_ENGINE_STATE", vehicle.EngineStatus);

                            Utils.sendNotification(player, vehicle.EngineStatus ? "Motor an." : "Motor aus.", "fas fa-power-off");
                        }
                    }
                    break;

                case "toggle_trunk":
                    if (hasKeys || isAdminOnDuty)
                    {
                        player.TriggerEvent("client:vehicle:toggleTrunk", vehicle);
                        Utils.sendNotification(player, "Kofferraum betätigt.", "fas fa-suitcase");
                    }
                    else
                    {
                        player.SendNotification("~r~Du hast keinen Schlüssel für dieses Fahrzeug.");
                    }
                    break;

                case "toggle_seatbelt":
                    if (player.IsInVehicle)
                    {
                        acc.Seatbelt = !acc.Seatbelt;
                        player.TriggerEvent("updateSeatbelt", acc.Seatbelt);
                        Utils.sendNotification(player, acc.Seatbelt ? "Sicherheitsgurt angelegt." : "Sicherheitsgurt abgelegt.", "fas fa-user-shield");
                    }
                    break;

                case "lspd_impound":
                    if (acc.Fraktion == 1 && isOnFactionDuty && DutyManager.GetMemberRank(acc.ID, acc.Fraktion) >= 2)
                    {
                        if (vehData != null)
                        {
                            vehData.Health = 250f;
                            vehData.Status = 1;
                            VehSafeData.UpdateVehicle(vehData);
                            vehicle.Health = 250;
                            vehicle.EngineStatus = false;
                            vehicle.SetSharedData("VEH_ENGINE_STATE", false);
                            Utils.sendNotification(player, $"Fahrzeug mit ID {vehData.Id} wurde stillgelegt.", "fas fa-ban");
                        }
                    }
                    break;
                case "lspd_unimpound":
                    if (acc.Fraktion == 1 && isOnFactionDuty && DutyManager.GetMemberRank(acc.ID, acc.Fraktion) >= 3)
                    {
                        if (vehData != null && vehData.Status == 1)
                        {
                            vehData.Status = 0; // "normal"
                            VehSafeData.UpdateVehicle(vehData);
                            Utils.sendNotification(player, $"Die Stillegung für Fahrzeug-ID {vehData.Id} wurde aufgehoben.", "fas fa-check-circle");
                        }
                    }
                    break;

                case "lscs_repair":
                    if (acc.Fraktion == 3 && isOnFactionDuty)
                    {
                        if (vehData != null)
                        {
                            vehData.Health = 1000f;
                            VehSafeData.UpdateVehicle(vehData);
                        }
                        vehicle.Repair();
                        Utils.sendNotification(player, "Fahrzeug erfolgreich repariert.", "fas fa-tools");
                    }
                    break;

                case "lscs_tow":
                    if (acc.Fraktion == 3 && isOnFactionDuty)
                    {
                        Utils.sendNotification(player, "Fahrzeug zum Abschleppen markiert (Funktion in Entwicklung).", "fas fa-truck-pickup");
                    }
                    break;
            }
        }

        private bool CanControlVehicle(Accounts acc, VehSafe vehData)
        {
            if (vehData == null || acc == null) return false;
            if (acc.ID == vehData.OwnerId) return true;
            if (vehData.IsFactionVehicle && acc.Fraktion == vehData.FactionId) return true;
            return false;
        }
    }
}