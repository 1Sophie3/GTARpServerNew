using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RPCore.Commands;
using RPCore.Database;
using RPCore.Haussystem;
using Newtonsoft.Json;

namespace RPCore.Admin
{
    public class ClothingData
    {
        public int Drawable { get; set; }
        public int Texture { get; set; }
    }

    public class AdminOutfit
    {
        public Dictionary<int, int[]> Male { get; set; } = new Dictionary<int, int[]>();
        public Dictionary<int, int[]> Female { get; set; } = new Dictionary<int, int[]>();
    }

    class AdminSystem : Script
    {
        private static readonly Dictionary<Player, Vector3> adminGoBackPosition = new Dictionary<Player, Vector3>();
        private static readonly Dictionary<string, AdminOutfit> AdminOutfits = new Dictionary<string, AdminOutfit>();
        private static List<TeleportLocation> teleportLocations = new List<TeleportLocation>();

        public AdminSystem()
        {
            LoadTeleportLocations();
            InitializeAdminOutfits();
        }

        private void InitializeAdminOutfits()
        {
            AdminOutfits["support"] = new AdminOutfit { Male = new Dictionary<int, int[]> { { 1, new[] { 135, 5 } }, { 11, new[] { 287, 5 } }, { 8, new[] { 15, 0 } }, { 4, new[] { 114, 5 } }, { 6, new[] { 78, 5 } }, { 3, new[] { 3, 0 } } }, Female = new Dictionary<int, int[]> { { 1, new[] { 153, 5 } }, { 11, new[] { 300, 5 } }, { 8, new[] { 15, 0 } }, { 4, new[] { 121, 5 } }, { 6, new[] { 82, 5 } }, { 3, new[] { 8, 0 } } } };
            AdminOutfits["mod"] = new AdminOutfit { Male = new Dictionary<int, int[]> { { 1, new[] { 135, 2 } }, { 11, new[] { 287, 2 } }, { 8, new[] { 15, 0 } }, { 4, new[] { 114, 2 } }, { 6, new[] { 78, 2 } }, { 3, new[] { 3, 0 } } }, Female = new Dictionary<int, int[]> { { 1, new[] { 153, 2 } }, { 11, new[] { 300, 2 } }, { 8, new[] { 15, 0 } }, { 4, new[] { 121, 2 } }, { 6, new[] { 82, 2 } }, { 3, new[] { 8, 0 } } } };
            AdminOutfits["admin"] = new AdminOutfit { Male = new Dictionary<int, int[]> { { 1, new[] { 135, 3 } }, { 11, new[] { 287, 3 } }, { 8, new[] { 15, 0 } }, { 4, new[] { 114, 3 } }, { 6, new[] { 78, 3 } }, { 3, new[] { 3, 0 } } }, Female = new Dictionary<int, int[]> { { 1, new[] { 153, 3 } }, { 11, new[] { 300, 3 } }, { 8, new[] { 15, 0 } }, { 4, new[] { 121, 3 } }, { 6, new[] { 82, 3 } }, { 3, new[] { 8, 0 } } } };
            AdminOutfits["superadmin"] = new AdminOutfit { Male = new Dictionary<int, int[]> { { 1, new[] { 135, 2 } }, { 11, new[] { 287, 2 } }, { 8, new[] { 15, 0 } }, { 4, new[] { 114, 2 } }, { 6, new[] { 78, 2 } }, { 3, new[] { 3, 0 } } }, Female = new Dictionary<int, int[]> { { 1, new[] { 153, 2 } }, { 11, new[] { 300, 2 } }, { 8, new[] { 15, 0 } }, { 4, new[] { 121, 2 } }, { 6, new[] { 82, 2 } }, { 3, new[] { 8, 0 } } } };
            NAPI.Util.ConsoleOutput($"[AdminSystem] {AdminOutfits.Count} Admin-Outfits geladen.");
        }

        private void LoadTeleportLocations()
        {
            string filePath = "dotnet/resources/gamemode/gamemode/Commands/teleportLocations.json";
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    teleportLocations = JsonConvert.DeserializeObject<List<TeleportLocation>>(json);
                    NAPI.Util.ConsoleOutput($"[AdminSystem] {teleportLocations.Count} Teleport-Orte erfolgreich geladen.");
                }
                else
                {
                    NAPI.Util.ConsoleOutput($"[AdminSystem-FEHLER] Die Datei teleportLocations.json wurde unter dem Pfad '{filePath}' NICHT gefunden.");
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Laden von teleportLocations.json: {ex.Message}");
            }
        }

        [RemoteEvent("adminPanel:requestOpen")]
        public void EVENT_RequestAdminPanelData(Player admin)
        {
            Accounts adminAccount = admin.GetData<Accounts>(Accounts.Account_Key);
            if (adminAccount == null || !adminAccount.IstSpielerAdmin((int)Accounts.AdminRanks.TestGamemaster)) return;

            var playerList = NAPI.Pools.GetAllPlayers()
                .Where(p => p.HasData(Accounts.Account_Key))
                .Select(p => new { acc = p.GetData<Accounts>(Accounts.Account_Key), player = p })
                .OrderByDescending(p => p.acc.Adminlevel)
                .ThenBy(p => p.acc.ID)
                .Select(p => new { id = p.acc.ID, name = p.acc.GetFullName() })
                .ToList();

            object houseList = new List<object>();
            if (adminAccount.IstSpielerAdmin((int)Accounts.AdminRanks.Admin))
            {
                houseList = HouseManager.AllHouses.Select(h => new { id = h.Id, name = h.Name, owner = h.OwnerName ?? "STAAT" }).ToList();
            }
            var tpLocations = teleportLocations.Select(loc => loc.Name).ToList();

            var supportTickets = Datenbank.Datenbank.GetOpenSupportTickets();

            admin.TriggerEvent("adminPanel:show",
                adminAccount.Adminlevel,
                adminAccount.ID,
                JsonConvert.SerializeObject(playerList),
                JsonConvert.SerializeObject(houseList),
                JsonConvert.SerializeObject(tpLocations),
                JsonConvert.SerializeObject(supportTickets));
        }

        #region Aduty & Invisibility
        [RemoteEvent("adminPanel:toggleAduty")]
        public void EVENT_ToggleAduty(Player admin)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return;
            bool isOnAduty = admin.GetData<bool>("ON_ADUTY");
            if (!isOnAduty)
            {
                var originalClothes = new Dictionary<int, ClothingData>();
                for (int i = 0; i <= 11; i++) { originalClothes[i] = new ClothingData { Drawable = admin.GetClothesDrawable(i), Texture = admin.GetClothesTexture(i) }; }
                admin.SetData("ADUTY_ORIGINAL_CLOTHES", JsonConvert.SerializeObject(originalClothes));
                SetAdminOutfit(admin);
                admin.SetSharedData("GODMODE", true);
                admin.SetData("ON_ADUTY", true);
                admin.SendNotification("~g~Du bist jetzt im Admin-Dienst.");
            }
            else
            {
                RestoreOriginalOutfit(admin);
                admin.SetSharedData("GODMODE", false);
                admin.ResetData("ON_ADUTY");
                admin.SendNotification("~y~Du hast den Admin-Dienst beendet.");
            }
        }

        [RemoteEvent("adminPanel:toggleInvisibility")]
        public void EVENT_ToggleInvisibility(Player admin)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return;
            if (admin.Transparency == 255)
            {
                admin.Transparency = 0;
                admin.SendNotification("~y~Du bist jetzt unsichtbar.");
            }
            else
            {
                admin.Transparency = 255;
                admin.SendNotification("~g~Du bist jetzt wieder sichtbar.");
            }
        }

        private void SetAdminOutfit(Player admin)
        {
            Accounts acc = admin.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null) return;
            string outfitKey;
            if (acc.Adminlevel >= 5) outfitKey = "superadmin"; else if (acc.Adminlevel >= 3) outfitKey = "admin"; else if (acc.Adminlevel == 2) outfitKey = "mod"; else outfitKey = "support";
            if (AdminOutfits.TryGetValue(outfitKey, out AdminOutfit outfit))
            {
                var clothes = (admin.Model == (uint)PedHash.FreemodeMale01) ? outfit.Male : outfit.Female;
                foreach (var clothingItem in clothes) { admin.SetClothes(clothingItem.Key, clothingItem.Value[0], clothingItem.Value[1]); }
            }
        }

        private void RestoreOriginalOutfit(Player admin)
        {
            if (admin.HasData("ADUTY_ORIGINAL_CLOTHES"))
            {
                var clothesJson = admin.GetData<string>("ADUTY_ORIGINAL_CLOTHES");
                var originalClothes = JsonConvert.DeserializeObject<Dictionary<int, ClothingData>>(clothesJson);
                foreach (var entry in originalClothes) { admin.SetClothes(entry.Key, entry.Value.Drawable, entry.Value.Texture); }
                admin.ResetData("ADUTY_ORIGINAL_CLOTHES");
            }
        }
        #endregion

        #region Player Actions
        [RemoteEvent("adminPanel:kick")]
        public void EVENT_Kick(Player admin, int targetAccountId, string reason)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.GameMaster)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            string characterName = GetFullNameFromTarget(target);
            NAPI.Chat.SendChatMessageToAll($"~p~[SERVER]~w~ {characterName} wurde von einem Admin gekickt. Grund: {reason}");
            target.Kick(reason);
        }

        [RemoteEvent("adminPanel:ban")]
        public void EVENT_Ban(Player admin, int targetAccountId, string reason)
        {
            // 1. Berechtigungen prüfen (bleibt gleich)
            if (!HasPermission(admin, Accounts.AdminRanks.GameMaster)) return;

            Accounts targetAccount = GetAccountData(targetAccountId);
            if (targetAccount == null)
            {
                admin.SendNotification("~r~Account nicht gefunden.");
                return;
            }

            // 2. Ban in der Datenbank speichern (bleibt gleich)
            string characterName = targetAccount.GetFullName();
            Datenbank.Datenbank.SpielerBannen(targetAccountId, reason);

            
          

            // NEU: Schleife, die nur Admins eine Benachrichtigung sendet
            string adminMessage = $"~r~{characterName} wurde von {admin.Name} gebannt. Grund: {reason}";
            foreach (Player onlinePlayer in NAPI.Pools.GetAllPlayers())
            {
                // Wir gehen davon aus, dass Supporter der niedrigste Admin-Rang ist, der dies sehen soll.
                if (HasPermission(onlinePlayer, Accounts.AdminRanks.TestGamemaster))
                {
                    onlinePlayer.SendNotification(adminMessage);
                }
            }

            // 4. Spieler kicken (bleibt gleich, ist bereits unmittelbar)
            Player targetPlayer = FindPlayerByAccountId(targetAccountId);
            if (targetPlayer != null)
            {
                targetPlayer.Kick($"Du wurdest permanent gebannt. Grund: {reason}");
            }
        }

        [RemoteEvent("adminPanel:unban")]
        public void EVENT_Unban(Player admin, int targetAccountId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;
            Datenbank.Datenbank.SpielerEntbannen(targetAccountId);
            admin.SendNotification($"~g~Account-ID {targetAccountId} wurde entbannt.");
        }

        [RemoteEvent("adminPanel:goto")]
        public void EVENT_Goto(Player admin, int targetAccountId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            string characterName = GetFullNameFromTarget(target);
            adminGoBackPosition[admin] = admin.Position;
            admin.Position = target.Position;
            admin.Dimension = target.Dimension;
            admin.SendNotification($"~g~Teleportiert zu {characterName}.");
        }

        [RemoteEvent("adminPanel:gethere")]
        public void EVENT_Gethere(Player admin, int targetAccountId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.GameMaster)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            string characterName = GetFullNameFromTarget(target);
            target.Position = admin.Position;
            target.Dimension = admin.Dimension;
            admin.SendNotification($"~g~{characterName} wurde zu dir teleportiert.");
            target.SendNotification($"~y~Du wurdest von einem Admin teleportiert.");
        }

        [RemoteEvent("adminPanel:heal")]
        public void EVENT_Heal(Player admin, int targetAccountId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            string characterName = GetFullNameFromTarget(target);
            target.Health = 100;
            target.Armor = 100;
            admin.SendNotification($"~g~{characterName} geheilt.");
            if (admin != target) target.SendNotification("~g~Du wurdest von einem Admin geheilt.");
        }

        [RemoteEvent("adminPanel:revivePlayer")]
        public void EVENT_RevivePlayer(Player admin, int targetAccountId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            if (target.Health > 0) { admin.SendNotification("~r~Dieser Spieler ist nicht tot."); return; }
            string characterName = GetFullNameFromTarget(target);
            Datenbank.Datenbank.RevivePlayer(target, 100, 100);
            admin.SendNotification($"~g~Spieler {characterName} wurde wiederbelebt.");
            target.SendNotification("~g~Du wurdest von einem Admin wiederbelebt.");
        }

        [RemoteEvent("adminPanel:toggleSpectate")]
        public void EVENT_ToggleSpectate(Player admin, int targetAccountId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.GameMaster)) return;
            if (admin.GetData<bool>("IS_SPECTATING") == true)
            {
                admin.TriggerEvent("spectate:stop");
                admin.SendNotification("~y~Spectate-Modus beendet.");
            }
            else
            {
                Player target = FindPlayerByAccountId(targetAccountId);
                if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
                if (target == admin) { admin.SendNotification("~r~Du kannst dich nicht selbst beobachten."); return; }
                string characterName = GetFullNameFromTarget(target);
                adminGoBackPosition[admin] = admin.Position;
                admin.SetData("IS_SPECTATING", true);
                admin.TriggerEvent("spectate:start", target);
                admin.SendNotification($"~g~Beobachtest nun {characterName}. Nochmal klicken zum Beenden.");
            }
        }

        [RemoteEvent("adminPanel:toggleFreeze")]
        public void EVENT_ToggleFreeze(Player admin, int targetAccountId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            string characterName = GetFullNameFromTarget(target);
            bool isFrozen = target.GetData<bool>("IS_FROZEN_BY_ADMIN");
            target.SetData("IS_FROZEN_BY_ADMIN", !isFrozen);
            target.TriggerEvent("admin:doFreeze", !isFrozen);
            admin.SendNotification($"~y~Spieler {characterName} wurde {(!isFrozen ? "eingefroren" : "aufgetaut")}.");
        }

        [RemoteEvent("adminPanel:setPlayerDimension")]
        public void EVENT_SetPlayerDimension(Player admin, int targetAccountId, int dimension)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            string characterName = GetFullNameFromTarget(target);
            target.Dimension = (uint)dimension;
            admin.SendNotification($"~g~Dimension von {characterName} auf {dimension} gesetzt.");
            target.SendNotification($"~g~Deine Dimension wurde von einem Admin auf {dimension} gesetzt.");
        }

        [RemoteEvent("adminPanel:giveMoney")]
        public async void EVENT_GiveMoney(Player admin, int targetAccountId, int amount, string type)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            Accounts targetAcc = target.GetData<Accounts>(Accounts.Account_Key);
            if (targetAcc == null) return;
            string characterName = targetAcc.GetFullName();
            if (type == "cash")
            {
                targetAcc.Geld += amount;
                admin.SendNotification($"~g~Du hast {characterName} ${amount:N0} Bargeld gegeben.");
                target.SendNotification($"~g~Ein Admin hat dir ${amount:N0} Bargeld gegeben.");
                Datenbank.Datenbank.AccountSpeichern(target);
            }
            else if (type == "bank")
            {
                PlayerBankAccount bank = await Datenbank.Datenbank.GetOrCreatePlayerBankAccount(targetAcc.ID);
                if (bank != null)
                {
                    bank.Kontostand += amount;
                    bool success = await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bank.Kontonummer, bank.Kontostand);
                    if (success)
                    {
                        await Datenbank.Datenbank.LogTransactionAsync(targetAcc.ID, bank.Kontonummer, "deposit", amount, null, "Admin-Gutschrift");
                        admin.SendNotification($"~g~Du hast ${amount:N0} auf das Konto von {characterName} überwiesen.");
                        target.SendNotification($"~g~Ein Admin hat dir ${amount:N0} auf dein Konto überwiesen.");
                    }
                }
            }
        }

        [RemoteEvent("adminPanel:giveWeapon")]
        public void EVENT_GiveWeapon(Player admin, int targetAccountId, string weaponName)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;
            Player target = FindPlayerByAccountId(targetAccountId);
            if (target == null) { admin.SendNotification("~r~Spieler nicht online."); return; }
            string characterName = GetFullNameFromTarget(target);
            try
            {
                uint weaponHash = NAPI.Util.GetHashKey(weaponName);
                if (weaponHash == 0) { admin.SendNotification("~r~Ungültige Waffe."); return; }
                target.GiveWeapon((WeaponHash)weaponHash, 250);
                admin.SendNotification($"~g~Waffe '{weaponName}' an {characterName} gegeben.");
                target.SendNotification($"~g~Du hast eine '{weaponName}' von einem Admin erhalten.");
            }
            catch { admin.SendNotification("~r~Waffe konnte nicht gegeben werden."); }
        }
        #endregion

        #region Self & Teleport Actions
        [RemoteEvent("adminPanel:spawnAdminVehicle")]
        public void EVENT_SpawnAdminVehicle(Player admin) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; Vehicle veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Shotaro, admin.Position.Around(3f), admin.Heading, 134, 134); veh.NumberPlate = "ADMIN"; admin.SetIntoVehicle(veh, (int)VehicleSeat.Driver); }
        [RemoteEvent("adminPanel:goBack")]
        public void EVENT_GoBack(Player admin) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; if (adminGoBackPosition.TryGetValue(admin, out Vector3 pos)) { admin.Position = pos; adminGoBackPosition.Remove(admin); admin.SendNotification("~g~Zurückteleportiert."); } else { admin.SendNotification("~r~Keine Rückkehrposition gespeichert."); } }
        [RemoteEvent("adminPanel:toggleGodMode")]
        public void EVENT_ToggleGodMode(Player admin) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; bool godMode = admin.GetSharedData<bool>("GODMODE"); admin.SetSharedData("GODMODE", !godMode); admin.SendNotification(!godMode ? "~g~Godmode aktiviert." : "~r~Godmode deaktiviert."); }
        [RemoteEvent("adminPanel:toggleNoClip")]
        public void EVENT_ToggleNoClip(Player admin) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; admin.TriggerEvent("noclip:toggle"); }
        [RemoteEvent("adminPanel:teleportToLocation")]
        public void EVENT_TeleportToLocation(Player admin, string locationName, bool withVehicle) { var rank = withVehicle ? Accounts.AdminRanks.GameMaster : Accounts.AdminRanks.TestGamemaster; if (!HasPermission(admin, rank)) return; var location = teleportLocations.FirstOrDefault(l => l.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase)); if (location == null) { admin.SendNotification("~r~Ort nicht gefunden."); return; } Vector3 targetPos = new Vector3(location.X, location.Y, location.Z); if (withVehicle && admin.IsInVehicle) { admin.Vehicle.Position = targetPos; } else { admin.Position = targetPos; } admin.SendNotification($"~g~Teleportiert zu: {location.Name}"); }
        [RemoteEvent("adminPanel:teleportToCoords")]
        public void EVENT_TeleportToCoords(Player admin, float x, float y, float z) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; admin.Position = new Vector3(x, y, z); admin.SendNotification($"~g~Zu Koordinaten teleportiert: {x:F1}, {y:F1}, {z:F1}"); }

        [ServerEvent(Event.PlayerDamage)]
        public void OnPlayerDamage(Player player, float healthLoss, float armorLoss) { if (player.GetSharedData<bool>("GODMODE")) { NAPI.Task.Run(() => { if (NAPI.Entity.DoesEntityExist(player)) { player.Health = 100; player.Armor = 100; } }, 50); } }
        #endregion

        #region Vehicle Actions
        [RemoteEvent("adminPanel:spawnTempVehicle")]
        public void EVENT_SpawnTempVehicle(Player admin, string modelName) { if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return; uint vehHash = NAPI.Util.GetHashKey(modelName); if (vehHash == 0) { admin.SendNotification("~r~Ungültiges Modell."); return; } Vehicle veh = NAPI.Vehicle.CreateVehicle(vehHash, admin.Position.Around(3f), admin.Heading, 0, 0); veh.NumberPlate = "ADMIN"; admin.SetIntoVehicle(veh, (int)VehicleSeat.Driver); }
        [RemoteEvent("adminPanel:createPersVehicle")]
        public void EVENT_CreatePersVehicle(Player admin, string modelName, int ownerId, int color1, int color2, string plate)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;
            Accounts ownerAcc = GetAccountData(ownerId);
            if (ownerAcc == null) { admin.SendNotification("~r~Besitzer-Account-ID nicht gefunden."); return; }
            uint vehHash = NAPI.Util.GetHashKey(modelName);
            if (vehHash == 0) { admin.SendNotification($"~r~Ungültiges Fahrzeugmodell: {modelName}"); return; }

            VehSafe newVeh = new VehSafe()
            {
                OwnerId = ownerAcc.ID,
                ModelName = modelName,
                NumberPlate = plate,
                ColorPrimary = color1,
                ColorSecondary = color2,
                Modifications = "{}",
                PosX = admin.Position.X,
                PosY = admin.Position.Y,
                PosZ = admin.Position.Z,
                Heading = admin.Heading,

                // --- HIER DIE NEUEN ZEILEN EINFÜGEN ---
                Fuel = 100.0f,      // Startet mit vollem Tank
                Mileage = 0.0f      // Startet mit 0 Kilometern
            };

            int recordId = VehSafeData.SaveVehicle(newVeh);
            Vehicle spawnedVeh = NAPI.Vehicle.CreateVehicle((VehicleHash)vehHash, new Vector3(newVeh.PosX, newVeh.PosY, newVeh.PosZ), newVeh.Heading, newVeh.ColorPrimary, newVeh.ColorSecondary, newVeh.NumberPlate);
            spawnedVeh.SetData("vehsafe_id", recordId);
            admin.SendNotification($"~g~Persistentes Fahrzeug '{modelName}' für {ownerAcc.GetFullName()} (DB-ID: {recordId}) erstellt.");
        }
        [RemoteEvent("adminPanel:createFactionVehicle")]
        public void EVENT_CreateFactionVehicle(Player admin, string modelName, int factionId, string plate, int color1, int color2)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;
            uint vehHash = NAPI.Util.GetHashKey(modelName);
            if (vehHash == 0) { admin.SendNotification("~r~Ungültiges Modell."); return; }
            if (factionId <= 0) { admin.SendNotification("~r~Ungültige Fraktions-ID."); return; }

            VehSafe newVeh = new VehSafe()
            {
                OwnerId = 0,
                IsFactionVehicle = true,
                FactionId = factionId,
                ModelName = modelName,
                NumberPlate = plate,
                ColorPrimary = color1,
                ColorSecondary = color2,
                Modifications = "{}",
                PosX = admin.Position.X,
                PosY = admin.Position.Y,
                PosZ = admin.Position.Z,
                Heading = admin.Heading,

                // --- HIER DIE NEUEN ZEILEN EINFÜGEN ---
                Fuel = 100.0f,
                Mileage = 0.0f
            };

            int recordId = VehSafeData.SaveVehicle(newVeh);
            if (recordId > 0)
            {
                Vehicle spawnedVeh = NAPI.Vehicle.CreateVehicle((VehicleHash)vehHash, new Vector3(newVeh.PosX, newVeh.PosY, newVeh.PosZ), newVeh.Heading, newVeh.ColorPrimary, newVeh.ColorSecondary, newVeh.NumberPlate);
                spawnedVeh.SetData("vehsafe_id", recordId);
                spawnedVeh.SetData("faction_id", newVeh.FactionId);
            }
            admin.SendNotification($"~g~Fraktionsfahrzeug '{modelName}' für Fraktion {factionId} erstellt (DB-ID: {recordId}).");
        }
        [RemoteEvent("adminPanel:tptoVehicle")]
        public void EVENT_TeleportToVehicle(Player admin, int vehDbId) { if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return; VehSafe vehData = VehSafeData.GetVehicleById(vehDbId); if (vehData == null) { admin.SendNotification("~r~Fahrzeug mit dieser ID in der DB nicht gefunden."); return; } Vehicle targetVehicle = NAPI.Pools.GetAllVehicles().FirstOrDefault(v => v.HasData("vehsafe_id") && v.GetData<int>("vehsafe_id") == vehDbId); Vector3 pos = (targetVehicle != null) ? targetVehicle.Position : new Vector3(vehData.PosX, vehData.PosY, vehData.PosZ); admin.Position = pos; admin.SendNotification($"~g~Teleportiert zu Fahrzeug-ID {vehDbId}."); }
        [RemoteEvent("adminPanel:deleteVehicleDB")]
        public void EVENT_DeleteVehicleFromDB(Player admin, int vehDbId) { if (!HasPermission(admin, Accounts.AdminRanks.Owner)) return; Vehicle targetVehicle = NAPI.Pools.GetAllVehicles().FirstOrDefault(v => v.HasData("vehsafe_id") && v.GetData<int>("vehsafe_id") == vehDbId); if (targetVehicle != null) { targetVehicle.Delete(); } VehSafeData.DeleteVehicleRecord(vehDbId); admin.SendNotification($"~g~Fahrzeug mit DB-ID {vehDbId} komplett gelöscht."); }
        [RemoteEvent("adminPanel:parkVehicleInAlta")]
        public void EVENT_ParkVehicleInAlta(Player admin, int vehDbId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;

            // Lade die Fahrzeugdaten aus der DB
            VehSafe vehData = VehSafeData.GetVehicleById(vehDbId);
            if (vehData == null)
            {
                admin.SendNotification("~r~Fahrzeug mit dieser ID in der DB nicht gefunden.");
                return;
            }

            // Finde das Fahrzeug in der Spielwelt, falls es gespawnt ist
            Vehicle targetVehicle = NAPI.Pools.GetAllVehicles().FirstOrDefault(v => v.HasData("vehsafe_id") && v.GetData<int>("vehsafe_id") == vehDbId);
            if (targetVehicle != null)
            {
                // Lösche das Fahrzeug aus der Welt, damit es nicht dupliziert wird
                targetVehicle.Delete();
            }

            // Setze den Garagen-Status in den Daten
            vehData.IsInGarage = true;
            vehData.GarageId = 1; // Wir nehmen an, Garage 1 ist die "Alta Garage". Passe die ID bei Bedarf an.

            // Speichere die Änderungen in der Datenbank
            VehSafeData.UpdateVehicle(vehData);
            admin.SendNotification($"~g~Fahrzeug mit DB-ID {vehDbId} wurde in der Alta Garage geparkt.");
        }
        [RemoteEvent("adminPanel:fetchVehicle")]
        public void EVENT_FetchVehicle(Player admin, int vehDbId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;

            VehSafe vehData = VehSafeData.GetVehicleById(vehDbId);
            if (vehData == null)
            {
                admin.SendNotification("~r~Fahrzeug mit dieser ID in der DB nicht gefunden.");
                return;
            }

            // Altes Fahrzeug in der Welt finden und zerstören, um Duplikate zu vermeiden
            Vehicle oldVehicle = NAPI.Pools.GetAllVehicles().FirstOrDefault(v => v.HasData("vehsafe_id") && v.GetData<int>("vehsafe_id") == vehDbId);
            if (oldVehicle != null)
            {
                oldVehicle.Delete();
            }

            // Fahrzeugdaten für den neuen Spawn vorbereiten
            vehData.PosX = admin.Position.X;
            vehData.PosY = admin.Position.Y;
            vehData.PosZ = admin.Position.Z;
            vehData.Heading = admin.Heading;
            vehData.IsInGarage = false;
            vehData.GarageId = null;

            // Fahrzeug spawnen (Logik aus deinem GarageSystem übernommen)
            Vehicle newVehicle = NAPI.Vehicle.CreateVehicle(
                NAPI.Util.GetHashKey(vehData.ModelName),
                admin.Position.Around(2f),
                admin.Heading,
                vehData.ColorPrimary,
                vehData.ColorSecondary
            );

            newVehicle.NumberPlate = vehData.NumberPlate;
            newVehicle.SetData("vehsafe_id", vehData.Id);
            newVehicle.Health = vehData.Health;
            newVehicle.Locked = true;
            newVehicle.EngineStatus = false;

            // Mods anwenden mit unserer Helfer-Methode
            if (!string.IsNullOrEmpty(vehData.Modifications))
            {
                var mods = JsonConvert.DeserializeObject<Dictionary<int, int>>(vehData.Modifications);
                if (mods != null)
                {
                    foreach (var mod in mods)
                    {
                        newVehicle.SetMod(mod.Key, mod.Value);
                    }
                }

                // Tacho-Daten setzen
                newVehicle.SetSharedData("VEHICLE_FUEL", vehData.Fuel);
                newVehicle.SetSharedData("VEHICLE_MILEAGE", vehData.Mileage);

                // Datenbank aktualisieren
                VehSafeData.UpdateVehicle(vehData);

                admin.SendNotification($"~g~Fahrzeug mit DB-ID {vehDbId} wurde zu dir geholt.");
            }
        }


        [RemoteEvent("adminPanel:repairVehicle")]
        public void EVENT_RepairVehicle(Player admin, int vehDbId)
        {
            if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return;

            // Finde das Fahrzeug in der Spielwelt
            Vehicle targetVehicle = NAPI.Pools.GetAllVehicles().FirstOrDefault(v => v.HasData("vehsafe_id") && v.GetData<int>("vehsafe_id") == vehDbId);
            if (targetVehicle != null)
            {
                // Repariere es visuell
                targetVehicle.Repair();
                admin.SendNotification("~g~Fahrzeug  repariert.");
            }

            // Lade die Fahrzeugdaten aus der DB
            VehSafe vehData = VehSafeData.GetVehicleById(vehDbId);
            if (vehData == null)
            {
                admin.SendNotification("~r~Fahrzeug mit dieser ID in der DB nicht gefunden.");
                return;
            }

            // Repariere die Daten in der Datenbank
            vehData.Health = 1000f;
            vehData.Status = 0; // Setze den Status auf "nicht zerstört"

            // Speichere die Änderungen
            VehSafeData.UpdateVehicle(vehData);
         
        }

        [RemoteEvent("adminPanel:forceToggleLock")]
        public void EVENT_ForceToggleLock(Player admin, bool isLocked) { if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return; var veh = GetClosestVehicle(admin); if (veh == null) { admin.SendNotification("~r~Kein Fahrzeug in der Nähe."); return; } veh.Locked = isLocked; admin.SendNotification($"~g~Fahrzeug {(isLocked ? "abgeschlossen" : "aufgeschlossen")}."); }
        [RemoteEvent("adminPanel:forceToggleEngine")]
        public void EVENT_ForceToggleEngine(Player admin, bool status) { if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return; var veh = GetClosestVehicle(admin); if (veh == null) { admin.SendNotification("~r~Kein Fahrzeug in der Nähe."); return; } veh.EngineStatus = status; admin.SendNotification($"~g~Motor {(status ? "eingeschaltet" : "ausgeschaltet")}."); }
        #endregion

        #region World & Communication Actions
        [RemoteEvent("adminPanel:teleportToHouse")]
        public void EVENT_TeleportToHouse(Player admin, int houseId) { if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return; var house = HouseManager.GetHouseById(houseId); if (house == null) { admin.SendNotification("~r~Haus nicht gefunden."); return; } admin.Position = house.Position; admin.SendNotification($"~g~Teleportiert zu Haus {house.Name} (ID: {house.Id})."); }
        [RemoteEvent("adminPanel:setHouseOwner")]
        public async void EVENT_SetHouseOwner(Player admin, int houseId, int targetAccountId) { if (!HasPermission(admin, Accounts.AdminRanks.Admin)) return; var house = HouseManager.GetHouseById(houseId); if (house == null) { admin.SendNotification("~r~Haus nicht gefunden."); return; } Accounts targetAcc = GetAccountData(targetAccountId); if (targetAcc == null && targetAccountId != 0) { admin.SendNotification("~r~Ziel-Account nicht gefunden."); return; } house.OwnerAccountId = targetAccountId == 0 ? (int?)null : targetAccountId; string ownerName = targetAccountId == 0 ? "STAAT" : targetAcc.GetFullName(); house.OwnerName = ownerName; house.Status = (byte)(targetAccountId == 0 ? 0 : 1); await HouseDatabase.SpeichereHausInDB(house); house.UpdateTextLabelAndMarker(); admin.SendNotification($"~g~Besitzer von Haus {house.Id} auf '{ownerName}' gesetzt."); }
        [RemoteEvent("adminPanel:sendAdminChat")]
        public void EVENT_SendAdminChat(Player admin, string message) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; string adminName = GetFullNameFromTarget(admin); foreach (var p in NAPI.Pools.GetAllPlayers()) { if (p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).Adminlevel > 0) { p.SendChatMessage($"~p~[ADMIN-CHAT] {adminName}:~w~ {message}"); } } }
        [RemoteEvent("adminPanel:sendAnnouncement")]
        public void EVENT_SendAnnouncement(Player admin, string message) { if (!HasPermission(admin, Accounts.AdminRanks.GameMaster)) return; NAPI.Chat.SendChatMessageToAll($"~y~[SERVER-INFO]~w~ {message}"); }
        #endregion

        #region Support System Events
        [RemoteEvent("adminPanel:support:updateStatus")]
        public void EVENT_SupportUpdateStatus(Player admin, int ticketId, string status) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; Accounts adminAccount = admin.GetData<Accounts>(Accounts.Account_Key); Datenbank.Datenbank.UpdateTicketStatus(ticketId, status, adminAccount.ID); admin.SendNotification($"Ticket {ticketId} wurde auf '{status}' gesetzt."); }
        [RemoteEvent("adminPanel:support:setPriority")]
        public void EVENT_SupportSetPriority(Player admin, int ticketId, bool isPriority) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; Datenbank.Datenbank.UpdateTicketPriority(ticketId, isPriority); admin.SendNotification($"Ticket {ticketId} Priorität wurde " + (isPriority ? "erhöht." : "entfernt.")); }
        [RemoteEvent("adminPanel:support:addComment")]
        public void EVENT_SupportAddComment(Player admin, int ticketId, string comment) { if (!HasPermission(admin, Accounts.AdminRanks.TestGamemaster)) return; string adminName = GetFullNameFromTarget(admin); Datenbank.Datenbank.AddTicketComment(ticketId, adminName, comment); admin.SendNotification("~g~Kommentar wurde zum Ticket hinzugefügt."); }
        #endregion

        #region Helper Methods
        private bool HasPermission(Player player, Accounts.AdminRanks requiredRank)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account != null && account.Adminlevel >= (int)requiredRank) return true;
            player.SendChatMessage("~r~Keine Berechtigung.");
            return false;
        }

        private Player FindPlayerByAccountId(int accountId)
        {
            return NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).ID == accountId);
        }

        private Accounts GetAccountData(int accountId)
        {
            var player = FindPlayerByAccountId(accountId);
            return player?.GetData<Accounts>(Accounts.Account_Key);
        }

        private string GetFullNameFromTarget(Player target)
        {
            if (target == null) return "Unbekannt";
            Accounts targetAccount = target.GetData<Accounts>(Accounts.Account_Key);
            return (targetAccount != null) ? targetAccount.GetFullName() : target.Name;
        }

        private Vehicle GetClosestVehicle(Player player, float distance = 4.0f)
        {
            return NAPI.Pools.GetAllVehicles().OrderBy(v => player.Position.DistanceTo(v.Position)).FirstOrDefault(v => player.Position.DistanceTo(v.Position) < distance);
        }
        #endregion
    }
}