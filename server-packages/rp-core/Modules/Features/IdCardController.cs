// -----------------------------------------------------------------------------
// Modul: IdCardController
//
// Beschreibung:
// Der Controller kapselt die Logik für das Ausweis-System und steuert die
// Kommunikation zwischen Datenbank, Ausweis-Modul und Frontend. Er ist modular
// aufgebaut und kann einfach erweitert werden (z.B. für REST-API, Events, etc.).
//
// Aufrufkette:
// - Ein Server-Handler (z.B. Command, Event, API-Endpunkt) ruft den Controller auf.
// - Der Controller holt die CharacterInfo aus der Datenbank.
// - Der Controller nutzt das Ausweis-Modul (IdCardCommands) zur Datenaufbereitung.
// - Der Controller gibt das JSON für das Frontend zurück.
// -----------------------------------------------------------------------------
using System;
using RPCore.Database;
using Newtonsoft.Json;

namespace RPCore.Features
{
    public class IdCardController
    {
        private IdCardCommands _idCardModule = new IdCardCommands();

        /// <summary>
        /// Holt und verarbeitet die Ausweis-Daten für einen Spieler.
        /// </summary>
        /// <param name="accountId">Die Account-ID des Spielers</param>
        /// <returns>JSON mit Ausweis-Daten für das Frontend</returns>
        public string GetIdCardForPlayer(int accountId)
        {
            // 1. CharacterInfo aus der Datenbank holen
            // (Hier Beispiel: Accounts.GetById(accountId) - muss ggf. angepasst werden)
            Accounts account = Accounts.GetById(accountId); // <-- Datenbank-Modul
            if (account == null || string.IsNullOrEmpty(account.CharacterData)) return null;

            // 2. Ausweis-Daten mit dem Modul erzeugen
            string idCardJson = _idCardModule.GetSelfIdCardJson(
                account.CharacterData,
                account.ID,
                account.CreationDate
            );

            // 3. Rückgabe für das Frontend (z.B. TriggerEvent, API, WebSocket)
            return idCardJson;
        }

        /// <summary>
        /// Holt und verarbeitet die Ausweis-Daten zum Zeigen an einen anderen Spieler.
        /// </summary>
        /// <param name="sourceAccountId">Account-ID des Zeigenden</param>
        /// <param name="targetAccountId">Account-ID des Zielspielers</param>
        /// <returns>JSON mit Ausweis-Daten für das Frontend</returns>
        public string GetIdCardForOther(int sourceAccountId, int targetAccountId)
        {
            // 1. Source-Account aus der Datenbank holen
            Accounts sourceAccount = Accounts.GetById(sourceAccountId); // <-- Datenbank-Modul
            if (sourceAccount == null || string.IsNullOrEmpty(sourceAccount.CharacterData)) return null;

            // 2. Target-Account aus der Datenbank holen (optional für Chat-Feedback)
            Accounts targetAccount = Accounts.GetById(targetAccountId); // <-- Datenbank-Modul
            // Hier könnte z.B. ein Chat-Feedback erzeugt werden

            // 3. Ausweis-Daten mit dem Modul erzeugen
            string idCardJson = _idCardModule.GetIdCardJsonForOther(
                sourceAccount.CharacterData,
                sourceAccount.ID,
                sourceAccount.CreationDate
            );

            // 4. Rückgabe für das Frontend (z.B. TriggerEvent, API, WebSocket)
            return idCardJson;
        }
    }
}
