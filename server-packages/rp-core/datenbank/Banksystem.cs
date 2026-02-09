using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using RPCore.Database;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace RPCore.Database
{
    public class BankSystem : Script
    {
        private const float ATM_INTERACTION_RANGE = 2.5f;

        public BankSystem()
        {
            NAPI.Util.ConsoleOutput("=== Banksystem.cs (v2 mit Transaktionen) wurde geladen ===");
        }

        [RemoteEvent("Bank:RequestOpenUI")]
        public async void OnRequestOpenUI(Player player)
        {
            if (player.IsInVehicle || (player.HasData("ActiveUI") && player.GetData<string>("ActiveUI") != "Bank")) return;

            var atmInRange = Datenbank.Datenbank.BankATMs.FirstOrDefault(atm =>
                player.Position.DistanceTo(new Vector3(atm.PosX, atm.PosY, atm.PosZ)) <= ATM_INTERACTION_RANGE &&
                player.Dimension == atm.Dimension);

            if (atmInRange != null)
            {
                await OpenBankUI(player);
            }
        }

        public async Task OpenBankUI(Player player)
        {
            Accounts playerAccount = player.GetData<Accounts>(Accounts.Account_Key);
            if (playerAccount == null) return;

            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetOrCreatePlayerBankAccount(playerAccount.ID);

            if (bankAccount != null)
            {
                var transactions = await Datenbank.Datenbank.GetTransactionHistoryAsync(bankAccount.Kontonummer);
                string transactionsJson = JsonConvert.SerializeObject(transactions);

                player.SetData("ActiveUI", "Bank");
                // Kontostand immer mit zwei Nachkommastellen formatieren
                string bankBalance = bankAccount.Kontostand.ToString("F2", CultureInfo.InvariantCulture);

                NAPI.ClientEvent.TriggerClientEvent(player, "Bank:ShowMenu",
                    playerAccount.Geld, bankBalance, bankAccount.Kontonummer, transactionsJson);

                NAPI.Util.ConsoleOutput($"[INFO] Bank-UI für {player.Name} geöffnet/aktualisiert.");
            }
            else
            {
                player.SendNotification("~r~Fehler: Ihr Bankkonto konnte nicht geladen werden.");
            }
        }

        [RemoteEvent("Bank:Deposit")]
        public async Task OnDeposit(Player player, int amount)
        {
            Accounts playerAccount = player.GetData<Accounts>(Accounts.Account_Key);
            if (playerAccount == null || amount <= 0) return;

            if (playerAccount.Geld < amount)
            {
                player.SendNotification("~r~Du hast nicht genug Bargeld dabei.");
                return;
            }

            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(playerAccount.ID);
            if (bankAccount == null) return;

            playerAccount.Geld -= amount;
            bankAccount.Kontostand += amount;

            // Datenbank-Updates
            await Datenbank.Datenbank.UpdateAccountMoneyAsync(playerAccount.ID, playerAccount.Geld);
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);
            await Datenbank.Datenbank.LogTransactionAsync(playerAccount.ID, bankAccount.Kontonummer, "deposit", amount, null, "Einzahlung am Automaten");

            player.SendNotification($"~g~{amount}€ erfolgreich eingezahlt.");
            await OpenBankUI(player); // UI mit neuer Transaktion aktualisieren
        }

        [RemoteEvent("Bank:Withdraw")]
        public async Task OnWithdraw(Player player, int amount)
        {
            Accounts playerAccount = player.GetData<Accounts>(Accounts.Account_Key);
            if (playerAccount == null || amount <= 0) return;

            PlayerBankAccount bankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(playerAccount.ID);
            if (bankAccount == null || bankAccount.Kontostand < amount)
            {
                player.SendNotification("~r~Dein Kontostand ist nicht ausreichend.");
                return;
            }

            playerAccount.Geld += amount;
            bankAccount.Kontostand -= amount;

            await Datenbank.Datenbank.UpdateAccountMoneyAsync(playerAccount.ID, playerAccount.Geld);
            await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(bankAccount.Kontonummer, bankAccount.Kontostand);
            await Datenbank.Datenbank.LogTransactionAsync(playerAccount.ID, bankAccount.Kontonummer, "withdraw", amount, null, "Auszahlung am Automaten");

            player.SendNotification($"~g~{amount}€ erfolgreich abgehoben.");
            await OpenBankUI(player);
        }

        [RemoteEvent("Bank:Transfer")]
        public async Task OnTransfer(Player player, string targetKontonummer, int amount, string description)
        {
            Accounts senderAccount = player.GetData<Accounts>(Accounts.Account_Key);
            if (senderAccount == null || amount <= 0 || string.IsNullOrWhiteSpace(targetKontonummer) || targetKontonummer.Length != 9 || !targetKontonummer.All(char.IsDigit) || string.IsNullOrWhiteSpace(description))
            {
                player.SendNotification("~r~Ungültige Überweisungsdaten. Bitte fülle alle Felder korrekt aus.");
                return;
            }

            PlayerBankAccount senderBankAccount = await Datenbank.Datenbank.GetBankAccountByPlayerIdAsync(senderAccount.ID);
            if (senderBankAccount == null || senderBankAccount.Kontostand < amount)
            {
                player.SendNotification("~r~Dein Kontostand ist für diese Überweisung nicht ausreichend.");
                return;
            }

            if (senderBankAccount.Kontonummer == targetKontonummer)
            {
                player.SendNotification("~r~Du kannst kein Geld an dich selbst überweisen.");
                return;
            }

            PlayerBankAccount recipientBankAccount = await Datenbank.Datenbank.GetBankAccountByKontonummerAsync(targetKontonummer);
            if (recipientBankAccount == null)
            {
                player.SendNotification("~r~Das angegebene Empfänger-Konto existiert nicht.");
                return;
            }

            // --- Atomare Transaktion ---
            senderBankAccount.Kontostand -= amount;
            recipientBankAccount.Kontostand += amount;

            bool senderUpdated = await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(senderBankAccount.Kontonummer, senderBankAccount.Kontostand);
            bool recipientUpdated = await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(recipientBankAccount.Kontonummer, recipientBankAccount.Kontostand);
            bool logged = await Datenbank.Datenbank.LogTransactionAsync(senderAccount.ID, senderBankAccount.Kontonummer, "transfer", amount, targetKontonummer, description);

            if (senderUpdated && recipientUpdated && logged)
            {
                player.SendNotification($"~g~{amount}€ erfolgreich an Konto {targetKontonummer} überwiesen.");
                await OpenBankUI(player);

                Player recipientPlayer = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).ID == recipientBankAccount.PlayerId);
                if (recipientPlayer != null)
                {
                    recipientPlayer.SendNotification($"~g~Du hast eine Überweisung über {amount}€ von {senderAccount.Name} erhalten. Grund: {description}");
                    if (recipientPlayer.HasData("ActiveUI") && recipientPlayer.GetData<string>("ActiveUI") == "Bank")
                    {
                        await OpenBankUI(recipientPlayer);
                    }
                }
            }
            else
            {
                // Rollback manuell durchführen (vereinfacht)
                senderBankAccount.Kontostand += amount;
                recipientBankAccount.Kontostand -= amount;
                await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(senderBankAccount.Kontonummer, senderBankAccount.Kontostand);
                await Datenbank.Datenbank.UpdateBankAccountBalanceAsync(recipientBankAccount.Kontonummer, recipientBankAccount.Kontostand);
                player.SendNotification("~r~Ein Datenbankfehler ist aufgetreten. Die Überweisung wurde abgebrochen.");
            }
        }

        [RemoteEvent("Bank:RequestCloseMenu")]
        public void OnRequestCloseMenu(Player player)
        {
            if (player.HasData("ActiveUI") && player.GetData<string>("ActiveUI") == "Bank")
            {
                player.ResetData("ActiveUI");
                NAPI.ClientEvent.TriggerClientEvent(player, "Bank:CloseMenu");
            }
        }
    }
}