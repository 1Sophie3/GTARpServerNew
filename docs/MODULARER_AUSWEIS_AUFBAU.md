# Modularer Ausweis-Aufbau für GTA-Projekte

Dieses Dokument beschreibt den empfohlenen modularen Aufbau für das Ausweis-System und die Kommunikation zwischen Client, Server und Frontend. Es dient als Leitfaden für die Strukturierung und Erweiterung deines Projekts.

---

## 1. Übersicht

Das System besteht aus klar getrennten Modulen:
- **Client**: Sendet Anfragen und zeigt Ausweis-Daten im Frontend an.
- **Server-Handler**: Empfängt Anfragen vom Client und steuert die Abläufe.
- **Controller**: Kapselt die Logik und ruft das Ausweis-Modul auf.
- **Ausweis-Modul**: Verarbeitet und bereitet die Ausweis-Daten auf.
- **Frontend (CEF/Vue/HTML)**: Zeigt die Ausweis-Daten an.

---

## 2. Kommunikation

### Client → Server
- Der Client sendet eine Anfrage (z.B. Event, API-Call) an den Server, um einen Ausweis anzuzeigen.

### Server-Handler
- Der Handler nimmt die Anfrage entgegen und ruft den Controller auf.

### Controller
- Holt die CharacterInfo aus der Datenbank.
- Nutzt das Ausweis-Modul zur Aufbereitung der Daten.
- Gibt die Ausweis-Daten als JSON zurück.

### Server → Client
- Der Server sendet das JSON mit den Ausweis-Daten zurück an den Client.

### Client → Frontend
- Der Client öffnet die CEF/Vue-Seite und übergibt die Ausweis-Daten (z.B. per setData-Funktion).
- Das Frontend zeigt die Daten an.

---

## 3. Modularität & Erweiterbarkeit

- **Trennung der Logik**: Jede Funktion hat ihren eigenen Aufgabenbereich.
- **Klare Schnittstellen**: Kommunikation erfolgt über Events oder APIs.
- **Erweiterbarkeit**: Neue Features (z.B. weitere Ausweis-Typen, Validierungen) können einfach ergänzt werden.
- **Wiederverwendbarkeit**: Module können in anderen Projekten oder Systemen genutzt werden.

---

## 4. Beispiel-Aufrufkette

1. Client sendet Event: `showIdCard` mit AccountId.
2. Server-Handler empfängt das Event und ruft `IdCardController.GetIdCardForPlayer(accountId)` auf.
3. Controller holt Daten aus der Datenbank und ruft das Ausweis-Modul auf.
4. Controller gibt JSON zurück.
5. Server sendet JSON an Client.
6. Client öffnet CEF/Vue-Seite und übergibt die Daten.
7. Frontend zeigt Ausweis an.

---

## 5. Tipps für die Praxis

- Halte die Module klein und fokussiert.
- Dokumentiere Schnittstellen und Abläufe.
- Nutze Events oder APIs für die Kommunikation.
- Trenne Datenverarbeitung und Darstellung.
- Plane Erweiterungen von Anfang an modular.

---

## 6. Plattformintegration

Die Kernlogik bleibt plattformunabhängig. Für jede Plattform (z.B. RAGE, FiveM) baust du einen eigenen Handler/Adapter, der die Verbindung herstellt und die Controller-Funktionen nutzt.

### Vorgehen:
- Die plattformspezifischen Handler übernehmen die Kommunikation mit dem Spieler (z.B. TriggerEvent, Callback).
- Sie rufen die modularen Controller/Module auf und leiten die Ausweis-Daten weiter.
- So bleibt die Logik sauber getrennt und kann für jede Plattform angepasst werden.

### Beispiel: RAGE C# Handler
```csharp
// Handler für RAGE: Empfängt Command und ruft Controller auf
[Command("id")]
public void CMD_ShowSelfIdCard(Player player)
{
    int accountId = player.GetData<Accounts>(Accounts.Account_Key).ID;
    string idCardJson = IdCardController.GetIdCardForPlayer(accountId);
    player.TriggerEvent("showPlayerIdCard", idCardJson);
}
```

### Beispiel: FiveM Lua Handler
```lua
-- Handler für FiveM: Empfängt Event und ruft Server-API auf
RegisterServerEvent('showIdCard')
AddEventHandler('showIdCard', function(playerId)
    local accountId = GetAccountIdFromPlayer(playerId)
    local idCardJson = GetIdCardForPlayer(accountId) -- Server-API/Controller
    TriggerClientEvent('showPlayerIdCard', playerId, idCardJson)
end)
```

---

## 7. Clientseite & Frontend

Die Clientseite (Frontend) ist ebenfalls modular aufgebaut und kann unabhängig von der Server-Logik entwickelt und erweitert werden.

### Ablauf:
- Der Server sendet die Ausweis-Daten als JSON an den Client (z.B. TriggerEvent, API-Response).
- Der Client empfängt das JSON und öffnet das Frontend (z.B. CEF/Vue/HTML-Seite).
- Die Ausweis-Daten werden per Funktion (z.B. setData) an das Frontend übergeben.
- Das Frontend zeigt die Daten an und kann beliebig erweitert werden (Design, Felder, Interaktionen).

### Vorteile:
- Klare Schnittstelle zwischen Server und Client.
- Frontend kann unabhängig von der Server-Logik angepasst werden.
- Erweiterungen (z.B. neue Ausweis-Typen, UI-Features) sind einfach möglich.

---

**Dieses Dokument dient als Fahrplan für die weitere Entwicklung und sorgt für eine flexible, skalierbare und wartbare Projektstruktur!**
