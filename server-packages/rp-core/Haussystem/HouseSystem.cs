// #############################################################################
// #                                                                           #
// #         FINALES HAUSSYSTEM (V17.3 - Vollständigkeits-Fix)                 #
// #      Fehlende Klassendefinitionen wieder hinzugefügt.                     #
// #                                                                           #
// #############################################################################

using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RPCore.Datenbank;
using System.Data;

namespace RPCore.Haussystem
{
    #region 1. Daten-Modelle
    public class HouseKey
    {
        public int Id { get; set; }
        public int HouseId { get; set; }
        public int TargetAccountId { get; set; }
        public string TargetPlayerName { get; set; }
        public byte KeyType { get; set; }
    }

    public class House
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IPL { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 InteriorPosition { get; set; }
        public float InteriorRotationZ { get; set; }
        public int Price { get; set; }
        public int? OwnerAccountId { get; set; }
        public string OwnerName { get; set; }
        public byte Status { get; set; }
        public bool IsLocked { get; set; }
        public int Dimension { get; set; }
        public bool IsRentable { get; set; }
        public int RentPrice { get; set; }
        public int? RenterAccountId { get; set; }
        public string RenterName { get; set; }
        public DateTime? RentPaidUntil { get; set; }

        public ColShape EntranceShape { get; set; }
        public TextLabel InfoLabel { get; set; }
        public Marker EntranceMarker { get; set; }
        public List<HouseKey> Keys { get; set; } = new List<HouseKey>();

        public bool IsOwner(Player p)
        {
            if (!p.HasData(Accounts.Account_Key)) return false;
            return OwnerAccountId.HasValue && OwnerAccountId.Value == p.GetData<Accounts>(Accounts.Account_Key).ID;
        }

        public bool HasKey(Player p)
        {
            if (!p.HasData(Accounts.Account_Key)) return false;
            return Keys.Any(k => k.TargetAccountId == p.GetData<Accounts>(Accounts.Account_Key).ID);
        }

        public async Task<bool> IsRenter(Player p)
        {
            if (!p.HasData(Accounts.Account_Key)) return false;
            if (RentPaidUntil.HasValue && RentPaidUntil.Value < DateTime.Now)
            {
                RenterAccountId = null; RenterName = null; RentPaidUntil = null; IsLocked = true;
                await HouseDatabase.SpeichereHausInDB(this);
                UpdateTextLabelAndMarker();
                return false;
            }
            return RenterAccountId.HasValue && RenterAccountId.Value == p.GetData<Accounts>(Accounts.Account_Key).ID;
        }

        public void UpdateTextLabelAndMarker()
        {
            if (InfoLabel == null || !InfoLabel.Exists) return;
            string labelText;
            Color markerColor;
            if (IsRentable)
            {
                if (RenterAccountId.HasValue)
                {
                    labelText = $"Mieter: ~c~{RenterName}~w~\nStatus: {(IsLocked ? "~r~Abgeschlossen" : "~g~Aufgeschlossen")}";
                    markerColor = new Color(52, 152, 219);
                }
                else
                {
                    labelText = $"~g~Zu Mieten~w~\nPreis: ${RentPrice:N0} / Tag";
                    markerColor = new Color(26, 188, 156);
                }
            }
            else
            {
                switch (Status)
                {
                    case 0: labelText = $"~g~Zu Verkaufen~w~\nPreis: ${Price:N0}"; markerColor = new Color(38, 230, 0); break;
                    case 1: labelText = $"Eigentümer: ~c~{OwnerName ?? "STAAT"}~w~\nStatus: {(IsLocked ? "~r~Abgeschlossen" : "~g~Aufgeschlossen")}"; markerColor = new Color(255, 255, 255); break;
                    default: labelText = "~r~Admin-Gesperrt~w~"; markerColor = new Color(255, 0, 0); break;
                }
            }
            InfoLabel.Text = $"~b~{Name}~w~\n{labelText}";
            if (EntranceMarker?.Exists == true) EntranceMarker.Color = new Color(markerColor.Red, markerColor.Green, markerColor.Blue, 150);
        }
    }
    #endregion

    #region 2. Datenbank-Logik
    public static class HouseDatabase
    {
        private static async Task ExecuteNonQueryAsync(string query, params MySqlParameter[] parameters)
        {
            using (var conn = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task<List<House>> LadeAlleHaeuser()
        {
            var list = new List<House>();
            using (var c = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                await c.OpenAsync();
                var query = "SELECT h.*, a.name AS owner_name, r.name as renter_name FROM houses h LEFT JOIN accounts a ON h.besitzer_acc_id = a.id LEFT JOIN accounts r ON h.mieter_acc_id = r.id";
                using (var cmd = new MySqlCommand(query, c))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new House { Id = r.GetInt32("id"), Name = r.GetString("name"), IPL = r.IsDBNull(r.GetOrdinal("ipl")) ? "apa_v_mp_h_01_a" : r.GetString("ipl"), Position = new Vector3(r.GetFloat("posX"), r.GetFloat("posY"), r.GetFloat("posZ")), InteriorPosition = new Vector3(r.GetFloat("interiorPosX"), r.GetFloat("interiorPosY"), r.GetFloat("interiorPosZ")), InteriorRotationZ = r.GetFloat("interiorRotZ"), Price = r.GetInt32("preis"), OwnerAccountId = r.IsDBNull(r.GetOrdinal("besitzer_acc_id")) ? (int?)null : r.GetInt32("besitzer_acc_id"), OwnerName = r.IsDBNull(r.GetOrdinal("owner_name")) ? "STAAT" : r.GetString("owner_name"), Status = r.GetByte("status"), IsLocked = r.GetBoolean("abgeschlossen"), Dimension = r.GetInt32("dimension"), IsRentable = r.GetBoolean("is_rentable"), RentPrice = r.GetInt32("mietpreis"), RenterAccountId = r.IsDBNull(r.GetOrdinal("mieter_acc_id")) ? (int?)null : r.GetInt32("mieter_acc_id"), RenterName = r.IsDBNull(r.GetOrdinal("renter_name")) ? null : r.GetString("renter_name"), RentPaidUntil = r.IsDBNull(r.GetOrdinal("miete_bezahlt_bis")) ? (DateTime?)null : r.GetDateTime("miete_bezahlt_bis") });
                    }
                }
            }
            return list;
        }

        public static async Task<List<HouseKey>> LadeHausSchluessel(int houseId)
        {
            var list = new List<HouseKey>();
            using (var c = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                await c.OpenAsync();
                using (var cmd = new MySqlCommand("SELECT * FROM house_keys WHERE house_id = @id", c))
                {
                    cmd.Parameters.AddWithValue("@id", houseId);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new HouseKey { Id = r.GetInt32("id"), HouseId = r.GetInt32("house_id"), TargetAccountId = r.GetInt32("target_acc_id"), TargetPlayerName = r.GetString("target_player_name"), KeyType = r.GetByte("key_type") });
                        }
                    }
                }
            }
            return list;
        }

        public static async Task<int> ErstelleHausInDB(House h)
        {
            using (var conn = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                await conn.OpenAsync();
                string query = "INSERT INTO houses (name, ipl, posX, posY, posZ, interiorPosX, interiorPosY, interiorPosZ, interiorRotZ, preis, status, abgeschlossen, dimension, is_rentable, mietpreis, besitzer_acc_id) VALUES (@name, @ipl, @posX, @posY, @posZ, @intX, @intY, @intZ, @intRotZ, @preis, @status, @abgeschlossen, @dimension, @is_rentable, @mietpreis, NULL); SELECT LAST_INSERT_ID();";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", h.Name);
                    cmd.Parameters.AddWithValue("@ipl", h.IPL);
                    cmd.Parameters.AddWithValue("@posX", h.Position.X);
                    cmd.Parameters.AddWithValue("@posY", h.Position.Y);
                    cmd.Parameters.AddWithValue("@posZ", h.Position.Z);
                    cmd.Parameters.AddWithValue("@intX", h.InteriorPosition.X);
                    cmd.Parameters.AddWithValue("@intY", h.InteriorPosition.Y);
                    cmd.Parameters.AddWithValue("@intZ", h.InteriorPosition.Z);
                    cmd.Parameters.AddWithValue("@intRotZ", h.InteriorRotationZ);
                    cmd.Parameters.AddWithValue("@preis", h.Price);
                    cmd.Parameters.AddWithValue("@status", h.Status);
                    cmd.Parameters.AddWithValue("@abgeschlossen", h.IsLocked);
                    cmd.Parameters.AddWithValue("@dimension", h.Dimension);
                    cmd.Parameters.AddWithValue("@is_rentable", h.IsRentable);
                    cmd.Parameters.AddWithValue("@mietpreis", h.RentPrice);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public static async Task SpeichereHausInDB(House h)
        {
            string query = "UPDATE houses SET name = @name, ipl = @ipl, preis = @preis, posX = @posX, posY = @posY, posZ = @posZ, interiorPosX = @intX, interiorPosY = @intY, interiorPosZ = @intZ, interiorRotZ = @intRotZ, besitzer_acc_id = @owner, status = @status, abgeschlossen = @locked, mieter_acc_id = @renter, miete_bezahlt_bis = @rent_until, mietpreis = @rent_price WHERE id = @id";
            await ExecuteNonQueryAsync(query,
                new MySqlParameter("@id", h.Id), new MySqlParameter("@name", h.Name), new MySqlParameter("@ipl", h.IPL),
                new MySqlParameter("@preis", h.Price), new MySqlParameter("@posX", h.Position.X), new MySqlParameter("@posY", h.Position.Y),
                new MySqlParameter("@posZ", h.Position.Z), new MySqlParameter("@intX", h.InteriorPosition.X), new MySqlParameter("@intY", h.InteriorPosition.Y),
                new MySqlParameter("@intZ", h.InteriorPosition.Z), new MySqlParameter("@intRotZ", h.InteriorRotationZ), new MySqlParameter("@owner", h.OwnerAccountId),
                new MySqlParameter("@status", h.Status), new MySqlParameter("@locked", h.IsLocked), new MySqlParameter("@renter", h.RenterAccountId),
                new MySqlParameter("@rent_until", h.RentPaidUntil), new MySqlParameter("@rent_price", h.RentPrice));
        }

        public static async Task SpeichereNeuenSchluessel(HouseKey k) => await ExecuteNonQueryAsync("INSERT INTO house_keys (house_id, target_acc_id, target_player_name, key_type) VALUES (@h_id, @acc_id, @name, @type)", new MySqlParameter("@h_id", k.HouseId), new MySqlParameter("@acc_id", k.TargetAccountId), new MySqlParameter("@name", k.TargetPlayerName), new MySqlParameter("@type", k.KeyType));

        public static async Task LoescheAlleSchluessel(int houseId) => await ExecuteNonQueryAsync("DELETE FROM house_keys WHERE house_id = @id", new MySqlParameter("@id", houseId));

        public static async Task LoescheEinzelnenSchluessel(int houseId, int targetAccountId)
        {
            await ExecuteNonQueryAsync("DELETE FROM house_keys WHERE house_id = @h_id AND target_acc_id = @acc_id",
                new MySqlParameter("@h_id", houseId),
                new MySqlParameter("@acc_id", targetAccountId));
        }
    }
    #endregion

    #region 3. Core-Logik (HouseManager)
    public static class HouseManager
    {
        public static List<House> AllHouses { get; private set; } = new List<House>();
        private static int nextAvailableDimension = 1000;

        public static async Task InitializeHouses()
        {
            AllHouses = await HouseDatabase.LadeAlleHaeuser();
            foreach (var h in AllHouses)
            {
                h.Keys = await HouseDatabase.LadeHausSchluessel(h.Id);
                CreateHouseEntities(h);
            }
            if (AllHouses.Any()) { int maxDimension = AllHouses.Max(h => h.Dimension); nextAvailableDimension = (maxDimension < 1000) ? 1000 : maxDimension + 1; }
            NAPI.Util.ConsoleOutput($"[Haussystem] {AllHouses.Count} Häuser geladen. Nächste freie Dimension: {nextAvailableDimension}");
        }

        public static int GetNextAvailableDimension() { return nextAvailableDimension++; }

        public static void CreateHouseEntities(House house)
        {
            const uint mainWorldDimension = 0;
            house.EntranceShape = NAPI.ColShape.CreateSphereColShape(house.Position, 2.0f, mainWorldDimension);
            house.EntranceShape.SetData("HOUSE_ID", house.Id);
            house.InfoLabel = NAPI.TextLabel.CreateTextLabel("", house.Position + new Vector3(0, 0, 1.0f), 12f, 0.7f, 4, new Color(255, 255, 255), false, mainWorldDimension);
            house.EntranceMarker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, house.Position - new Vector3(0, 0, 0.9f), new Vector3(), new Vector3(), 1.5f, new Color(255, 255, 255, 100), false, mainWorldDimension);
            house.UpdateTextLabelAndMarker();

            var exitShape = NAPI.ColShape.CreateSphereColShape(house.InteriorPosition, 2.5f, (uint)house.Dimension);
            exitShape.SetData("HOUSE_EXIT_ID", house.Id);
        }

        public static House GetHouseById(int id) => AllHouses.FirstOrDefault(h => h.Id == id);
        public static House GetHousePlayerIsAt(Player p) => p.HasData("AT_HOUSE_ENTRANCE_ID") ? GetHouseById(p.GetData<int>("AT_HOUSE_ENTRANCE_ID")) : null;
        public static House GetHousePlayerIsIn(Player p) => AllHouses.FirstOrDefault(h => h.Dimension == p.Dimension && p.Dimension != 0);
    }
    #endregion

    #region 4. Events & Befehle
    public class HouseEventsAndCommands : Script
    {
        [ServerEvent(Event.PlayerEnterColshape)]
        public async void OnPlayerEnterColshape(ColShape shape, Player player)
        {
            if (shape.HasData("HOUSE_ID"))
            {
                var house = HouseManager.GetHouseById(shape.GetData<int>("HOUSE_ID"));
                if (house == null) return;
                player.SetData("AT_HOUSE_ENTRANCE_ID", house.Id);

                var promptData = new
                {
                    isLocked = house.IsLocked,
                    isOwner = house.IsOwner(player),
                    isRenter = await house.IsRenter(player),
                    hasKey = house.HasKey(player)
                };
                player.TriggerEvent("Client:House:ShowInteractionPrompt", NAPI.Util.ToJson(promptData));
            }
            else if (shape.HasData("HOUSE_EXIT_ID"))
            {
                player.SetData("CAN_EXIT_HOUSE", true);
                player.SendChatMessage("~y~Drücke E, um das Haus zu verlassen.");
            }
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void OnPlayerExitColshape(ColShape shape, Player player)
        {
            if (shape.HasData("HOUSE_ID")) { player.ResetData("AT_HOUSE_ENTRANCE_ID"); player.TriggerEvent("Client:House:HideInteractionPrompt"); }
            else if (shape.HasData("HOUSE_EXIT_ID")) { player.ResetData("CAN_EXIT_HOUSE"); }
        }

        [RemoteEvent("Server:House:RequestMenuData")]
        public async void OnRequestMenuData(Player player)
        {
            var h = HouseManager.GetHousePlayerIsAt(player);
            if (h == null) return;

            var houseData = new
            {
                id = h.Id,
                name = h.Name,
                price = h.Price,
                rentPrice = h.RentPrice,
                isLocked = h.IsLocked,
                isOwner = h.IsOwner(player),
                ownerName = h.OwnerName,
                isRenter = await h.IsRenter(player),
                renterName = h.RenterName,
                hasKey = h.HasKey(player),
                isRentable = h.IsRentable,
                isForSale = (h.Status == 0 && !h.IsRentable),
                KeyHolders = h.Keys.Select(k => new { k.TargetAccountId, k.TargetPlayerName }).ToList()
            };
            player.TriggerEvent("Client:House:OpenMenu", NAPI.Util.ToJson(houseData));
        }

        [RemoteEvent("Server:House:Enter")]
        public async void OnEnter(Player player)
        {
            var h = HouseManager.GetHousePlayerIsAt(player);
            if (h == null) return;

            if (h.IsLocked)
            {
                player.SendChatMessage("~r~Das Haus ist abgeschlossen. Du musst es zuerst aufschließen.");
                OnRequestMenuData(player);
                return;
            }

            player.TriggerEvent("Client:RequestIpl", h.IPL);
            await Task.Delay(250);

            player.Dimension = (uint)h.Dimension;
            player.Position = h.InteriorPosition;
            player.Rotation = new Vector3(0, 0, h.InteriorRotationZ);
        }

        [RemoteEvent("Server:House:Exit")]
        public void OnAttemptExit(Player player)
        {
            if (!player.GetData<bool>("CAN_EXIT_HOUSE")) return;
            var h = HouseManager.GetHousePlayerIsIn(player);

            // Wenn kein Haus gefunden wurde (Sicherheitscheck), setze den Spieler zurück.
            if (h == null)
            {
                if (player.Dimension != 0)
                {
                    player.Position = new Vector3(276.828f, -580.471f, 43.113f);
                    player.Dimension = 0;
                    // NEU: Auch hier die IPL entladen lassen
                    player.TriggerEvent("Client:House:UnloadIpl");
                }
                return;
            }

            player.Position = h.Position;
            player.Dimension = 0;
            player.ResetData("CAN_EXIT_HOUSE");

            // NEU: Löse das Event aus, damit der Client die alte IPL entfernt.
            player.TriggerEvent("Client:House:UnloadIpl");
        }

        [RemoteEvent("Server:House:Buy")]
        public async void OnBuy(Player player)
        {
            var h = HouseManager.GetHousePlayerIsAt(player);
            if (h == null || h.Status != 0 || h.IsRentable || !player.HasData(Accounts.Account_Key)) return;

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(acc.ID);

            if (bankAccount == null || bankAccount.Kontostand < h.Price)
            {
                player.SendChatMessage("~r~Dein Bankkonto ist nicht ausreichend gedeckt.");
                return;
            }

            bankAccount.Kontostand -= h.Price;
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);

            h.OwnerAccountId = acc.ID;
            h.OwnerName = acc.Name;
            h.Status = 1;
            h.IsLocked = true;
            h.UpdateTextLabelAndMarker();
            await HouseDatabase.SpeichereHausInDB(h);

            player.SendChatMessage($"~g~Haus '{h.Name}' für ${h.Price:N0} vom Bankkonto gekauft.");
            player.TriggerEvent("Bank:UpdateBalances", acc.Geld, bankAccount.Kontostand.ToString(CultureInfo.InvariantCulture));
            player.TriggerEvent("Client:House:ForceMenuClose");
        }

        [RemoteEvent("Server:House:Sell")]
        public async void OnSell(Player player)
        {
            var h = HouseManager.GetHousePlayerIsAt(player);
            if (h == null || !h.IsOwner(player) || !player.HasData(Accounts.Account_Key)) return;

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(acc.ID);

            if (bankAccount == null)
            {
                player.SendChatMessage("~r~Fehler: Dein Bankkonto konnte nicht gefunden werden.");
                return;
            }

            int sellPrice = (int)(h.Price * 0.75);
            bankAccount.Kontostand += sellPrice;
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);

            h.OwnerAccountId = null;
            h.OwnerName = "STAAT";
            h.Status = 0;
            h.Keys.Clear();
            await HouseDatabase.LoescheAlleSchluessel(h.Id);
            h.UpdateTextLabelAndMarker();
            await HouseDatabase.SpeichereHausInDB(h);

            player.SendChatMessage($"~y~Haus für ${sellPrice:N0} an den Staat verkauft. Betrag wurde dem Konto gutgeschrieben.");
            player.TriggerEvent("Bank:UpdateBalances", acc.Geld, bankAccount.Kontostand.ToString(CultureInfo.InvariantCulture));
            player.TriggerEvent("Client:House:ForceMenuClose");
        }

        [RemoteEvent("Server:House:Rent")]
        public async void OnRent(Player player, int days)
        {
            var h = HouseManager.GetHousePlayerIsAt(player);
            if (h == null || !h.IsRentable || h.RenterAccountId.HasValue || !player.HasData(Accounts.Account_Key)) return;
            if (days != 1 && days != 7 && days != 30) { player.SendChatMessage("~r~Ungültige Mietdauer."); return; }

            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(acc.ID);

            int cost = h.RentPrice * days;
            if (bankAccount == null || bankAccount.Kontostand < cost)
            {
                player.SendChatMessage("~r~Dein Bankkonto ist nicht ausreichend gedeckt.");
                return;
            }

            bankAccount.Kontostand -= cost;
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);

            h.RenterAccountId = acc.ID;
            h.RenterName = acc.Name;
            h.RentPaidUntil = DateTime.Now.AddDays(days);
            h.IsLocked = true;
            h.UpdateTextLabelAndMarker();
            await HouseDatabase.SpeichereHausInDB(h);

            player.SendChatMessage($"~g~'{h.Name}' für {days} Tag(e) für ${cost:N0} vom Bankkonto gemietet.");
            player.TriggerEvent("Bank:UpdateBalances", acc.Geld, bankAccount.Kontostand.ToString(CultureInfo.InvariantCulture));
            player.TriggerEvent("Client:House:ForceMenuClose");
        }

        [RemoteEvent("Server:House:ToggleLock")]
        public async void OnToggleLock(Player player)
        {
            var h = HouseManager.GetHousePlayerIsAt(player) ?? HouseManager.GetHousePlayerIsIn(player);
            if (h == null || (!h.IsOwner(player) && !await h.IsRenter(player))) return;

            h.IsLocked = !h.IsLocked;
            h.UpdateTextLabelAndMarker();
            await HouseDatabase.SpeichereHausInDB(h);
            player.SendChatMessage($"~y~Haus {(h.IsLocked ? "abgeschlossen" : "aufgeschlossen")}.");
            player.TriggerEvent("Client:House:ForceMenuClose");
        }

        [RemoteEvent("Server:House:GiveKey")]
        public async void OnGiveKey(Player player, int targetId)
        {
            var h = HouseManager.GetHousePlayerIsAt(player);
            if (h == null || !h.IsOwner(player)) return;

            const int GIVE_KEY_COST = 2500;
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(acc.ID);

            if (bankAccount == null || bankAccount.Kontostand < GIVE_KEY_COST)
            {
                player.SendChatMessage($"~r~Dein Bankkonto ist nicht gedeckt. Es werden ${GIVE_KEY_COST:N0} benötigt.");
                return;
            }

            Player target = NAPI.Pools.GetAllPlayers().FirstOrDefault(x => x.HasData(Accounts.Account_Key) && x.GetData<Accounts>(Accounts.Account_Key).ID == targetId);
            if (target == null) { player.SendChatMessage("~r~Spieler nicht online."); return; }
            if (h.HasKey(target) || h.IsOwner(target)) { player.SendChatMessage("~r~Spieler hat bereits einen Schlüssel."); return; }

            bankAccount.Kontostand -= GIVE_KEY_COST;
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);
            player.TriggerEvent("Bank:UpdateBalances", acc.Geld, bankAccount.Kontostand.ToString(CultureInfo.InvariantCulture));

            var key = new HouseKey { HouseId = h.Id, TargetAccountId = targetId, TargetPlayerName = target.Name, KeyType = 1 };
            h.Keys.Add(key);
            await HouseDatabase.SpeichereNeuenSchluessel(key);

            player.SendChatMessage($"~g~Schlüssel für '{h.Name}' an {target.Name} gegeben. Kosten: ${GIVE_KEY_COST:N0}.");
            target.SendChatMessage($"~g~{player.Name} hat dir einen Schlüssel für '{h.Name}' gegeben.");
        }

        [RemoteEvent("Server:House:RemoveKey")]
        public async void OnRemoveKey(Player player, int targetAccountId)
        {
            var h = HouseManager.GetHousePlayerIsAt(player);
            if (h == null || !h.IsOwner(player)) return;

            var keyToRemove = h.Keys.FirstOrDefault(k => k.TargetAccountId == targetAccountId);
            if (keyToRemove == null)
            {
                player.SendChatMessage("~r~Diese Person hat keinen Schlüssel für dieses Haus.");
                return;
            }

            h.Keys.Remove(keyToRemove);
            await HouseDatabase.LoescheEinzelnenSchluessel(h.Id, targetAccountId);

            player.SendChatMessage($"~g~Schlüssel von {keyToRemove.TargetPlayerName} wurde erfolgreich entzogen.");
            Player targetPlayer = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).ID == targetAccountId);
            if (targetPlayer != null)
            {
                targetPlayer.SendChatMessage($"~r~Dein Schlüssel für das Haus '{h.Name}' wurde vom Besitzer entzogen.");
            }

            OnRequestMenuData(player);
        }

        [RemoteEvent("Server:House:ChangeLocks")]
        public async void OnChangeLocks(Player player)
        {
            var h = HouseManager.GetHousePlayerIsAt(player);
            if (h == null || !h.IsOwner(player)) return;

            const int CHANGE_LOCKS_COST = 25000;
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(acc.ID);

            if (bankAccount == null || bankAccount.Kontostand < CHANGE_LOCKS_COST)
            {
                player.SendChatMessage($"~r~Dein Bankkonto ist nicht gedeckt. Es werden ${CHANGE_LOCKS_COST:N0} benötigt.");
                return;
            }

            bankAccount.Kontostand -= CHANGE_LOCKS_COST;
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);
            player.TriggerEvent("Bank:UpdateBalances", acc.Geld, bankAccount.Kontostand.ToString(CultureInfo.InvariantCulture));

            foreach (var key in h.Keys)
            {
                Player targetPlayer = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).ID == key.TargetAccountId);
                if (targetPlayer != null)
                {
                    targetPlayer.SendChatMessage($"~r~Die Schlösser des Hauses '{h.Name}' wurden ausgetauscht. Dein Schlüssel ist ungültig.");
                }
            }

            h.Keys.Clear();
            await HouseDatabase.LoescheAlleSchluessel(h.Id);

            player.SendChatMessage($"~g~Schlösser erfolgreich für ${CHANGE_LOCKS_COST:N0} ausgetauscht. Alle alten Schlüssel sind ungültig.");
            OnRequestMenuData(player);
        }

        [Command("createhouse", GreedyArg = true)]
        public async void CMD_CreateHouse(Player player, string args)
        {
            if (!player.HasData(Accounts.Account_Key) || player.GetData<Accounts>(Accounts.Account_Key).Adminlevel < 4) { player.SendChatMessage("~r~Keine Rechte."); return; }

            string[] argList = args.Split(' ');
            if (argList.Length < 2) { player.SendChatMessage("~y~SYNTAX: /createhouse [IPL] [Preis] [Name]"); return; }

            string ipl = argList[0];
            if (!int.TryParse(argList[1], out int price)) { player.SendChatMessage("~r~Ungültiger Preis."); return; }

            string name = argList.Length > 2 ? string.Join(" ", argList.Skip(2)) : "Wohnhaus";

            Vector3 interiorPos = Houseinterrior.GetHausAusgang(ipl);

            House newHouse = new House
            {
                Name = name,
                IPL = ipl,
                Price = price,
                Position = player.Position,
                InteriorPosition = interiorPos,
                Status = 0,
                IsLocked = true,
                IsRentable = (price == 0),
                RentPrice = (price == 0) ? 500 : 0,
                Dimension = HouseManager.GetNextAvailableDimension()
            };

            newHouse.Id = await HouseDatabase.ErstelleHausInDB(newHouse);
            if (newHouse.Id > 0)
            {
                HouseManager.AllHouses.Add(newHouse);
                HouseManager.CreateHouseEntities(newHouse);
                player.SendChatMessage($"~g~Haus '{name}' (ID {newHouse.Id}, Dim {newHouse.Dimension}) erstellt.");

                if (interiorPos.X == 0 && interiorPos.Y == 0)
                {
                    player.SendChatMessage("~y~WARNUNG: Die IPL wurde nicht in interriors.json gefunden. Bitte setze den Spawnpunkt manuell mit /sethouseinterior " + newHouse.Id);
                }
                else
                {
                    player.SendChatMessage("~g~Der Interior-Spawnpunkt wurde automatisch gesetzt.");
                }
            }
            else { player.SendChatMessage("~r~DB-Fehler beim Erstellen."); }
        }

        [Command("sethouseinterior")]
        public async void CMD_SetHouseInterior(Player player, int houseId)
        {
            if (!player.HasData(Accounts.Account_Key) || player.GetData<Accounts>(Accounts.Account_Key).Adminlevel < 4) { player.SendChatMessage("~r~Keine Rechte."); return; }
            var house = HouseManager.GetHouseById(houseId);
            if (house == null) { player.SendChatMessage("~r~Haus-ID nicht gefunden."); return; }
            house.InteriorPosition = player.Position;
            house.InteriorRotationZ = player.Heading;
            await HouseDatabase.SpeichereHausInDB(house);
            player.SendChatMessage($"~g~Interior-Spawnpunkt für Haus ID {houseId} manuell gesetzt.");
        }

        [Command("goipl")]
        public async void CMD_GoToIpl(Player player, string ipl)
        {
            if (!player.HasData(Accounts.Account_Key) || player.GetData<Accounts>(Accounts.Account_Key).Adminlevel < 4) { player.SendChatMessage("~r~Keine Rechte."); return; }
            if (string.IsNullOrEmpty(ipl)) { player.SendChatMessage("~y~SYNTAX: /goipl [IPL-Name]"); return; }

            Vector3 interiorPosition = Houseinterrior.GetHausAusgang(ipl);
            if (interiorPosition.X != 0 || interiorPosition.Y != 0)
            {
                player.TriggerEvent("Client:RequestIpl", ipl);
                await Task.Delay(250);

                player.Position = interiorPosition;
                player.SendChatMessage($"~g~Teleportiert zu '{ipl}'.");
            }
            else
            {
                player.SendChatMessage($"~r~Keine Position für '{ipl}' in interriors.json gefunden.");
            }
        }

        [Command("sethouserent")]
        public async void CMD_SetHouseRent(Player player, int houseId, int rentPrice)
        {
            if (!player.HasData(Accounts.Account_Key) || player.GetData<Accounts>(Accounts.Account_Key).Adminlevel < 4) { player.SendChatMessage("~r~Keine Rechte."); return; }
            var house = HouseManager.GetHouseById(houseId);
            if (house == null) { player.SendChatMessage("~r~Haus-ID nicht gefunden."); return; }
            if (!house.IsRentable) { player.SendChatMessage("~r~Dieses Haus ist kein Mietobjekt."); return; }
            if (rentPrice < 0) { player.SendChatMessage("~r~Der Mietpreis kann nicht negativ sein."); return; }
            house.RentPrice = rentPrice;
            await HouseDatabase.SpeichereHausInDB(house);
            house.UpdateTextLabelAndMarker();
            player.SendChatMessage($"~g~Du hast den Mietpreis für Haus '{house.Name}' (ID: {house.Id}) auf ${rentPrice:N0} pro Tag gesetzt.");
        }

        [Command("createmotelroom", GreedyArg = true)]
        public async void CMD_CreateMotelRoom(Player player, string roomName)
        {
            if (!player.HasData(Accounts.Account_Key) || player.GetData<Accounts>(Accounts.Account_Key).Adminlevel < 4)
            {
                player.SendChatMessage("~r~Keine Rechte.");
                return;
            }

            const int MOTEL_SETUP_COST = 2;
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(acc.ID);

            if (bankAccount == null || bankAccount.Kontostand < MOTEL_SETUP_COST)
            {
                player.SendChatMessage($"~r~Dein Bankkonto ist nicht gedeckt. Es werden ${MOTEL_SETUP_COST:N0} benötigt.");
                return;
            }

            bankAccount.Kontostand -= MOTEL_SETUP_COST;
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);
            player.TriggerEvent("Bank:UpdateBalances", acc.Geld, bankAccount.Kontostand.ToString(CultureInfo.InvariantCulture));

            House newHouse = new House
            {
                Name = roomName,
                IPL = "v_motel_mp",
                Price = 0,
                Position = player.Position,
                InteriorPosition = new Vector3(151.587f, -1007.008f, -99f),
                Status = 0,
                IsLocked = true,
                IsRentable = true,
                RentPrice = 200,
                Dimension = HouseManager.GetNextAvailableDimension()
            };

            newHouse.Id = await HouseDatabase.ErstelleHausInDB(newHouse);
            if (newHouse.Id > 0)
            {
                HouseManager.AllHouses.Add(newHouse);
                HouseManager.CreateHouseEntities(newHouse);
                player.SendChatMessage($"~g~Motelzimmer '{roomName}' (ID {newHouse.Id}) für ${MOTEL_SETUP_COST:N0} erstellt. Miete: ${newHouse.RentPrice:N0}/Tag.");
                player.SendChatMessage("~g~Der Interior-Spawnpunkt wurde automatisch gesetzt.");
            }
            else
            {
                player.SendChatMessage("~r~Ein Fehler ist beim Erstellen des Hauses in der Datenbank aufgetreten.");
                bankAccount.Kontostand += MOTEL_SETUP_COST;
                await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);
            }
        }
    }
    #endregion
}