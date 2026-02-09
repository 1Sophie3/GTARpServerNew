using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using RPCore.Datenbank;
using System.Linq;

namespace RPCore.FraktionsSystem
{
    public static class DutyManager
    {
        public static Dictionary<int, DateTime> OnDutyPlayers = new Dictionary<int, DateTime>();

        public static Dictionary<int, Dictionary<int, int>> FactionSalaryTables = new Dictionary<int, Dictionary<int, int>>()
        {
             { 1, new Dictionary<int, int>() { { 1, 850 },  { 2, 1200 }, { 3, 1400 }, { 4, 1850 }, { 5, 2050 }, { 6, 2550 }, { 7, 3200 }, { 8, 4000 }, { 9, 4700 }, { 10, 6000 }, { 11,6500 }, { 12,7000 } } },
             { 2, new Dictionary<int, int>() { { 1, 800 },  { 2, 1150 }, { 3, 1300 }, { 4, 1750 }, { 5, 1850 }, { 6, 2450 }, { 7, 2900 }, { 8, 3500 }, { 9, 4300 }, { 10, 5500 }, { 11,6000 }, { 12,6500 } } },
             { 3, new Dictionary<int, int>() { { 1, 800 },  { 2, 1150 }, { 3, 1300 }, { 4, 1750 }, { 5, 1850 }, { 6, 2200 }, { 7, 2500 }, { 8, 3200 }, { 9, 3900 }, { 10, 5000 }, { 11,5500 }, { 12,6000 } } },
             { 4, new Dictionary<int, int>() { { 1, 2000 }, { 2, 2750 }, { 12, 3500 } } }

        };

        public static void StartGlobalDutyTimer()
        {
            NAPI.Task.Run(async () => await PaydayTick(), delayTime: 60000);
        }

        private static async Task PaydayTick()
        {
            List<int> accountIds = new List<int>(OnDutyPlayers.Keys);
            DateTime now = DateTime.Now;
            foreach (int accountId in accountIds)
            {
                Player player = GetPlayerByAccountId(accountId);
                if (player == null || !player.Exists) { OnDutyPlayers.Remove(accountId); continue; }
                Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
                if (acc == null) continue;
                TimeSpan elapsed = now - OnDutyPlayers[accountId];
                if (elapsed.TotalMinutes >= 60)
                {
                    int fullHours = (int)Math.Floor(elapsed.TotalMinutes / 60);
                    int rank = GetMemberRank(acc.ID, acc.Fraktion);
                    int hourlyWage = GetFactionSalary(acc.Fraktion, rank);
                    int payment = fullHours * hourlyWage;
                    if (payment > 0)
                    {
                        var bankAccount = await Datenbank.Datenbank.GetOrCreatePlayerBankAccount(acc.ID);
                        if (bankAccount != null)
                        {
                            bankAccount.Kontostand += payment;
                            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);
                            await Datenbank.Datenbank.LogTransactionAsync(acc.ID, bankAccount.Kontonummer, "deposit", payment, null, "Staatliches Gehalt");
                            double remainderMinutes = elapsed.TotalMinutes - (fullHours * 60);
                            OnDutyPlayers[accountId] = now.Subtract(TimeSpan.FromMinutes(remainderMinutes));
                            player.SendNotification($"~g~[PAYDAY] Dein Gehalt von {payment}$ für {fullHours} Stunde(n) wurde auf dein Konto überwiesen.");
                        }
                    }
                }
            }
            NAPI.Task.Run(async () => await PaydayTick(), delayTime: 60000);
        }

        public static void BeginDuty(Player player, bool isAutoRelog = false)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null || acc.Fraktion == 0) return;
            if (acc.DutyStart.HasValue && OnDutyPlayers.ContainsKey(acc.ID)) return;
            int offsetMinutes = acc.DutyOffset;
            DateTime referenceTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(offsetMinutes));
            acc.DutyStart = DateTime.Now;
            acc.DutyOffset = 0;
            player.SetData(Accounts.Account_Key, acc);
            OnDutyPlayers[acc.ID] = referenceTime;
            Datenbank.Datenbank.AccountSpeichern(player);
            if (isAutoRelog)
            {
                string frakName = GetFactionName(acc.Fraktion);
                player.SendNotification($"~g~Willkommen zurück im {frakName} Dienst!");
            }
            else
            {
                player.SendNotification("~g~[Dienst] Du hast deinen Dienst begonnen.");
            }
        }

        public static async Task EndDuty(Player player)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null || !acc.DutyStart.HasValue) return;
            DateTime referenceTime = OnDutyPlayers.ContainsKey(acc.ID) ? OnDutyPlayers[acc.ID] : acc.DutyStart.Value;
            TimeSpan totalDutyTime = DateTime.Now - referenceTime;
            int fullHours = (int)Math.Floor(totalDutyTime.TotalMinutes / 60);
            if (fullHours > 0)
            {
                int rank = GetMemberRank(acc.ID, acc.Fraktion);
                int hourlyWage = GetFactionSalary(acc.Fraktion, rank);
                int payment = fullHours * hourlyWage;
                if (payment > 0)
                {
                    var bankAccount = await Datenbank.Datenbank.GetOrCreatePlayerBankAccount(acc.ID);
                    if (bankAccount != null)
                    {
                        bankAccount.Kontostand += payment;
                        await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);
                        await Datenbank.Datenbank.LogTransactionAsync(acc.ID, bankAccount.Kontonummer, "deposit", payment, null, "Dienst-Abschlusszahlung");
                        player.SendNotification($"~g~[Dienst] {payment}$ für {fullHours} volle Stunde(n) wurden auf dein Bankkonto überwiesen.");
                    }
                }
            }
            double remainderMinutes = totalDutyTime.TotalMinutes % 60;
            acc.DutyOffset = (int)Math.Floor(remainderMinutes);
            acc.DutyStart = null;
            acc.LastLogoutDuty = null;
            player.SetData(Accounts.Account_Key, acc);
            if (OnDutyPlayers.ContainsKey(acc.ID))
            {
                OnDutyPlayers.Remove(acc.ID);
            }
            Datenbank.Datenbank.AccountSpeichern(player);
            player.SendNotification($"~y~[Dienst] Dienst beendet. {acc.DutyOffset} Minute(n) Restzeit gutgeschrieben.");
        }

        public static void HandlePlayerDisconnect(Player player)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null || !acc.DutyStart.HasValue || !OnDutyPlayers.ContainsKey(acc.ID)) return;
            DateTime referenceTime = OnDutyPlayers[acc.ID];
            TimeSpan totalDutyTime = DateTime.Now - referenceTime;
            double remainderMinutes = totalDutyTime.TotalMinutes % 60;
            acc.DutyOffset = (int)Math.Floor(remainderMinutes);
            acc.LastLogoutDuty = DateTime.Now;
            OnDutyPlayers.Remove(acc.ID);
            Datenbank.Datenbank.AccountSpeichern(player);
        }

        public static void CheckAutoDutyOnLogin(Player player)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc != null && acc.DutyStart.HasValue && acc.LastLogoutDuty.HasValue)
            {
                if ((DateTime.Now - acc.LastLogoutDuty.Value).TotalMinutes < 10)
                {
                    BeginDuty(player, isAutoRelog: true);
                }
                else
                {
                    acc.DutyStart = null;
                }
                acc.LastLogoutDuty = null;
                Datenbank.Datenbank.AccountSpeichern(player);
            }
        }

        public static int GetFactionSalary(int factionId, int rank)
        {
            if (FactionSalaryTables.TryGetValue(factionId, out var salaryTable) && salaryTable.TryGetValue(rank, out int salary)) return salary;
            return 0;
        }

        public static int GetMemberRank(int accountId, int fraktionId)
        {
            int rank = 0;
            using (var connection = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT rank FROM fraktionsmitglieder WHERE account_id = @account AND fraktion_id = @fraktion LIMIT 1";
                        command.Parameters.AddWithValue("@account", accountId);
                        command.Parameters.AddWithValue("@fraktion", fraktionId);
                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value) rank = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex) { NAPI.Util.ConsoleOutput($"[ERROR] GetMemberRank: {ex.Message}"); }
            }
            return rank;
        }

        // HIER IST DIE FEHLENDE METHODE, DIE DEN COMPILERFEHLER VERURSACHT HAT
        public static string GetFactionName(int factionId)
        {
            string frakName = "unbekannten";
            using (var connection = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT name FROM fraktion WHERE id = @id LIMIT 1";
                        command.Parameters.AddWithValue("@id", factionId);
                        var result = command.ExecuteScalar();
                        if (result != null && !string.IsNullOrEmpty(result.ToString())) frakName = result.ToString();
                    }
                }
                catch (Exception ex) { NAPI.Util.ConsoleOutput($"[ERROR] GetFactionName: {ex.Message}"); }
            }
            return frakName;
        }

        public static Player GetPlayerByAccountId(int accountId)
        {
            return NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).ID == accountId);
        }
    }
}