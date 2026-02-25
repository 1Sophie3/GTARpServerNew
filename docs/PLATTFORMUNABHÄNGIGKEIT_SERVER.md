# Plattformunabhängige Serverstruktur für GTA-Server (FiveM/RAGE)

## Ziel
Die Kernlogik des Servers (z.B. Ausweis-System, Charakterverwaltung, Datenbankzugriffe) soll unabhängig von der gewählten Plattform (FiveM, RAGE) entwickelt werden. Die Anbindung an die Plattform erfolgt später über eigene Adapter/Handler.

---

## 1. Strukturprinzip
- **Kernmodule**: Enthalten die gesamte Spiellogik, Datenverarbeitung und Datenbankzugriffe. Keine Abhängigkeit zu FiveM oder RAGE.
- **Schnittstellen (Interfaces)**: Definieren die Kommunikation (z.B. IPlayer, IEventDispatcher), aber keine Implementierung.
- **Plattform-Adapter**: Implementieren die Schnittstellen für FiveM oder RAGE und verbinden die Plattform mit den Kernmodulen.

---

## 2. Beispielstruktur

```
server/
  core/
    CharacterManager.cs
    IdCardModule.cs
    ...
  interfaces/
    IPlayer.cs
    IEventDispatcher.cs
  adapters/
    fivem/
      FiveMPlayerAdapter.cs
      FiveMEventDispatcher.cs
    rage/
      RagePlayerAdapter.cs
      RageEventDispatcher.cs
```

---

## 3. Beispiel: Interface & Adapter

### IPlayer.cs
```csharp
public interface IPlayer
{
    int Id { get; }
    string Name { get; }
    void SendMessage(string message);
    // Weitere Methoden nach Bedarf
}
```

### FiveMPlayerAdapter.cs
```csharp
public class FiveMPlayerAdapter : IPlayer
{
    private readonly dynamic _fivemPlayer;
    public FiveMPlayerAdapter(dynamic fivemPlayer) { _fivemPlayer = fivemPlayer; }
    public int Id => _fivemPlayer.source;
    public string Name => _fivemPlayer.name;
    public void SendMessage(string message) => _fivemPlayer.TriggerEvent("chat:addMessage", message);
}
```

### RagePlayerAdapter.cs
```csharp
public class RagePlayerAdapter : IPlayer
{
    private readonly GTANetworkAPI.Player _ragePlayer;
    public RagePlayerAdapter(GTANetworkAPI.Player ragePlayer) { _ragePlayer = ragePlayer; }
    public int Id => _ragePlayer.Id;
    public string Name => _ragePlayer.Name;
    public void SendMessage(string message) => _ragePlayer.SendChatMessage(message);
}
```

---

## 4. Nutzung in der Kernlogik

Die Kernlogik arbeitet nur mit IPlayer, nicht mit FiveM- oder RAGE-spezifischen Klassen:

```csharp
public class IdCardController
{
    public void ShowIdCard(IPlayer player)
    {
        var idCardData = ...; // Datenbankabfrage
        player.SendMessage($"Dein Ausweis: {idCardData}");
    }
}
```

---

## 5. Plattform-Handler

Die Handler/Adapter verbinden die Plattform mit der Kernlogik:

```csharp
// Beispiel für FiveM
RegisterCommand("id", (source, args, raw) => {
    var player = new FiveMPlayerAdapter(GetPlayer(source));
    idCardController.ShowIdCard(player);
}, false);

// Beispiel für RAGE
[Command("id")]
public void CMD_ShowSelfIdCard(Player player)
{
    var corePlayer = new RagePlayerAdapter(player);
    idCardController.ShowIdCard(corePlayer);
}
```

---

## 6. Vorteile
- **Portabilität**: Kernlogik kann für beide Plattformen genutzt werden.
- **Wartbarkeit**: Änderungen an der Plattformintegration betreffen nur die Adapter.
- **Testbarkeit**: Kernlogik kann unabhängig getestet werden.

---


---

## 7. Rolle des Frontends

Das Frontend (z.B. CEF, Vue, HTML) ist ebenfalls modular und unabhängig von der Serverplattform. Es kommuniziert ausschließlich über klar definierte Schnittstellen (z.B. Events, JSON-Daten) mit dem Client-Code.

### Prinzipien:
- Das Frontend erhält alle anzuzeigenden Daten als JSON vom Server (über den Client/Adapter).
- Die Darstellung und Logik im Frontend sind unabhängig von FiveM oder RAGE.
- Änderungen am Server oder der Plattform erfordern keine Anpassungen am Frontend, solange die Schnittstelle gleich bleibt.

### Beispiel-Ablauf:
1. Server bereitet die Daten (z.B. Ausweis) in der Kernlogik auf.
2. Plattform-Adapter sendet die Daten als JSON an den Client.
3. Der Client öffnet das Frontend (z.B. CEF/Vue) und übergibt die Daten per Event/Funktion (z.B. setData).
4. Das Frontend zeigt die Daten an und verarbeitet Benutzerinteraktionen (z.B. Buttons).
5. Aktionen des Nutzers (z.B. Button-Klick) werden als Event zurück an den Client/Server gesendet.

### Vorteile:
- Das Frontend kann unabhängig entwickelt und gestaltet werden.
- UI/UX-Änderungen sind ohne Serveranpassungen möglich.
- Die Schnittstelle zwischen Server und Frontend bleibt klar und einfach testbar.

---

## 8. Fazit
Entwickle alle Spielfunktionen plattformunabhängig. Die Anbindung an FiveM oder RAGE erfolgt erst später über schlanke Adapter. Das Frontend bleibt modular und kommuniziert nur über Schnittstellen mit dem Server/Client. So bleibt dein Projekt flexibel und zukunftssicher.