using GTANetworkAPI;
using RPCore.Managers;
using RPCore.Models.Account;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace RPCore.Events
{
    public class AuthHandlers : Script
    {
        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [RemoteEvent("server:register")]
        public async void OnRegister(Player player, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                player.TriggerEvent("client:registerResult", false, "Bitte Benutzername und Passwort angeben.");
                return;
            }

            string hashed = ComputeSha256Hash(password);

            // Nutze Legacy-Overload ohne Vor-/Nachname (setzt "Unbekannt Spieler")
            var account = await AccountManager.Instance.CreateAccount(username, hashed, player.SocialClubName ?? string.Empty, player.Serial ?? string.Empty);

            if (account == null)
            {
                player.TriggerEvent("client:registerResult", false, "Fehler: Benutzername existiert bereits oder Charaktername nicht verfügbar.");
            }
            else
            {
                player.TriggerEvent("client:registerResult", true, "Account erstellt. Bitte melde dich an.");
            }
        }

        [RemoteEvent("server:login")]
        public async void OnLogin(Player player, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                player.TriggerEvent("client:loginResult", false, "Bitte Benutzername und Passwort angeben.");
                return;
            }

            string hashed = ComputeSha256Hash(password);

            bool success = await AccountManager.Instance.AuthenticateAccount(username, hashed);
            if (!success)
            {
                player.TriggerEvent("client:loginResult", false, "Falscher Benutzername oder Passwort.");
                return;
            }

            var account = await AccountManager.Instance.LoadAccount(username);
            if (account == null)
            {
                player.TriggerEvent("client:loginResult", false, "Account konnte nicht geladen werden.");
                return;
            }

            // Sende Account-Daten als JSON an den Client (ohne Vor-/Nachnamen beim Login)
            var accountData = new
            {
                id = account.Id,
                username = account.Username,
                cash = account.Cash,
                bank = account.BankMoney,
                level = account.Level,
                experience = account.Experience,
                job = account.Job,
                lastPosition = new { x = account.LastPosition.X, y = account.LastPosition.Y, z = account.LastPosition.Z }
            };

            string json = JsonConvert.SerializeObject(accountData);
            player.TriggerEvent("client:loginSuccess", json);
        }
    }
}
