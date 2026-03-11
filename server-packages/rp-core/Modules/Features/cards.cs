// -----------------------------------------------------------------------------
// Modul: Ausweis-System (IdCard)
//
// Beschreibung:
// Dieses Modul stellt die Kernlogik für das Ausweis-System bereit. Es ist bewusst
// plattformunabhängig gehalten und enthält keine direkte Verbindung zu GTA-Servern
// (z.B. RAGE, FiveM). Die Methoden liefern die Ausweis-Daten als JSON zurück und
// können von beliebigen Server-Handlern (Events, Commands, API-Endpunkte) genutzt
// werden.
//
// Modularer Aufbau:
// - Die Logik ist in einzelne Funktionen gekapselt, die nur Daten verarbeiten und
//   zurückgeben.
// - Plattformabhängige Handler (z.B. GTA-Events, Web-APIs) können diese Funktionen
//   aufrufen und die Daten an das Frontend oder andere Systeme weiterleiten.
// - Das Modul kann jederzeit erweitert werden (z.B. weitere Ausweis-Typen,
//   Validierungen, Logging, neue Felder), ohne die Kernlogik zu verändern.
//
// Erweiterbarkeit:
// - Neue Funktionen können einfach hinzugefügt werden.
// - Die Rückgabe als JSON ermöglicht flexible Integration in verschiedene Systeme.
// - Die Trennung von Logik und Plattform erleichtert Wartung und Skalierung.
//
// Beispiel-Erweiterungen:
// - Zusätzliche Felder (Adresse, Staatsangehörigkeit, etc.)
// - Validierung von Ausweis-Daten
// - Logging von Ausweis-Aktionen
// - Integration mit anderen Modulen (z.B. Polizei, Fraktionen)
// -----------------------------------------------------------------------------
// Speicherort: Features/cards.cs

using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Linq;
using RPCore.Database;

public class IdCardCommands
{
    // NEU: Direkter Befehl, um den eigenen Ausweis zu zeigen.
    // Gibt die Ausweis-Daten für den eigenen Charakter zurück
    public string GetSelfIdCardJson(string characterDataJson, int accountId, DateTime creationDate)
    {
        if (string.IsNullOrEmpty(characterDataJson)) return null;
        try
        {
            CharacterData charData = JsonConvert.DeserializeObject<CharacterData>(characterDataJson);
            if (charData == null) return null;

            var dataForClient = new
            {
                firstname = charData.Firstname,
                lastname = charData.Lastname,
                birth = charData.Birth,
                gender = charData.Gender,
                accountId = accountId,
                creationDate = creationDate.ToString("dd.MM.yyyy")
            };
            return JsonConvert.SerializeObject(dataForClient);
        }
        catch (JsonException) { return null; }
    }

    // Gibt die Ausweis-Daten für das Zeigen an einen anderen zurück
    public string GetIdCardJsonForOther(string characterDataJson, int accountId, DateTime creationDate)
    {
        // Logik identisch, aber für andere Spieler verwendbar
        return GetSelfIdCardJson(characterDataJson, accountId, creationDate);
    }

    // NEU: Direkter Befehl, um den Ausweis anderen zu zeigen.
    // Die Logik zum Zeigen des Ausweises an andere Spieler bleibt als Datenfunktion erhalten.

    // Keine GTA-spezifischen Events oder Commands mehr enthalten.
    // Ein plattformabhängiger Handler ruft diese Methoden auf und übernimmt die Weiterleitung.

    // Keine Player-Logik mehr enthalten.
}