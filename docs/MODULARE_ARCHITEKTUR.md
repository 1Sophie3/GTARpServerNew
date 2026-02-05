# Modulare Architektur - RolePlay Server

## 📋 Übersicht

Diese Dokumentation beschreibt die modulare Architektur des RolePlay Servers, die eine klare Trennung zwischen verschiedenen Systemen ermöglicht.

## 🏗️ Architektur-Prinzipien

### 1. **Separation of Concerns**
Jedes System hat seine eigene Verantwortlichkeit:
- **Account** → Login, Authentifizierung
- **Character** → IC (In-Character) Daten
- **Permission** → OOC (Out-of-Character) Rechte
- **Faction** → Fraktions- und Organisationssystem

### 2. **Modular & Erweiterbar**
Neue Features können einfach hinzugefügt werden, ohne bestehenden Code zu ändern.

### 3. **Singleton Manager Pattern**
Jedes System hat einen Manager, der zentral alle Operationen steuert.

---

## 📁 Ordnerstruktur

```
server-packages/rp-core/
├── Models/
│   ├── Account/
│   │   └── Account.cs                    # Login-Daten, Bans
│   ├── Character/
│   │   └── Character.cs                  # IC Charakter-Daten
│   ├── Permission/
│   │   └── PermissionLevel.cs            # OOC Admin-Rechte
│   └── Faction/
│       ├── Faction.cs                    # Fraktion
│       ├── FactionType.cs                # Fraktionstypen (Staat, Gang, etc.)
│       └── FactionRank.cs                # Ränge & Mitglieder
├── Managers/
│   ├── AccountManager.cs                 # Account-Verwaltung
│   ├── CharacterManager.cs               # Charakter-Verwaltung
│   └── FactionManager.cs                 # Fraktions-Verwaltung
├── Database/
│   └── DatabaseManager.cs                # DB-Verbindung
├── Commands/
│   └── Commands.cs                       # Spieler-Commands (+ Teleport-System)
├── Events/
│   ├── GodModeHandler.cs                 # Godmode-System
│   └── ReviveHandler.cs                  # Death/Revive-System
└── Main.cs                               # Entry Point
```

---

## 🔧 Systemübersicht

### **1. Account System**

**Zweck:** Verwaltet die Login-Daten eines Spielers (OOC)

**Datei:** `Models/Account/Account.cs`

**Eigenschaften:**
- Username (für Login)
- PasswordHash
- Email
- Social Club Name
- Hardware ID
- Ban-Informationen

**Manager:** `AccountManager.cs`

**Wichtige Methoden:**
```csharp
// Account erstellen
AccountManager.Instance.CreateAccount(username, passwordHash, email, socialClub, hwid);

// Account laden
var account = AccountManager.Instance.LoadAccount(username);

// Authentifizierung
bool isValid = AccountManager.Instance.AuthenticateAccount(username, passwordHash);

// Account bannen
AccountManager.Instance.BanAccount(accountId, "Cheating", DateTime.Now.AddDays(7));
```

---

### **2. Permission System**

**Zweck:** Verwaltet OOC Administrationsrechte

**Datei:** `Models/Permission/PermissionLevel.cs`

**Permission Levels:**
```
0 = Spieler           (keine Rechte)
1 = Supporter         (Support-Tickets, kleine Commands)
2 = Moderator         (Kick, temporäre Bans)
3 = Administrator     (permanente Bans, größere Rechte)
4 = HeadAdmin         (Leitung Admin-Team)
5 = Projektleitung    (Management)
6 = Owner             (Vollzugriff)
```

**Verwendung:**
```csharp
// Permission holen
var perm = AccountManager.Instance.GetPermission(accountId);

// Rechte prüfen
if (perm.HasPermission(PermissionLevel.Administrator))
{
    // Nur Admins können das
}

// Permission setzen
AccountManager.Instance.SetPermission(accountId, PermissionLevel.Moderator, "Owner");
```

---

### **3. Character System**

**Zweck:** Verwaltet IC (In-Character) Charaktere

**Datei:** `Models/Character/Character.cs`

**Ein Account kann mehrere Charaktere haben!**

**Eigenschaften:**
- FirstName + LastName (IC Name)
- Cash (Bargeld)
- BankBalance (Bankkonto)
- Level, Experience
- Position, Dimension
- Health, Armor
- Faction (optional)
- Job (für Zivilisten)

**Manager:** `CharacterManager.cs`

**Wichtige Methoden:**
```csharp
// Charakter erstellen
var character = CharacterManager.Instance.CreateCharacter(accountId, "John", "Doe");

// Alle Charaktere eines Accounts laden
var characters = CharacterManager.Instance.LoadCharactersByAccount(accountId);

// Aktiven Charakter setzen
CharacterManager.Instance.SetPlayerCharacter(player, character);

// Aktuellen Charakter holen
var currentChar = CharacterManager.Instance.GetPlayerCharacter(player);

// Geld geben/nehmen
CharacterManager.Instance.GiveMoney(character, 1000, toBank: false);
CharacterManager.Instance.TakeMoney(character, 500, fromBank: true);
```

---

### **4. Faction System**

**Zweck:** Verwaltet Fraktionen, Ränge und Mitglieder

**Dateien:**
- `Models/Faction/Faction.cs`
- `Models/Faction/FactionType.cs`
- `Models/Faction/FactionRank.cs`

**Fraktionstypen:**

| Wert    | Kategorie        | Beispiele                               |
| ------- | ---------------- | --------------------------------------- |
| 1-99    | Staatsfraktionen | LSPD (1), Medics (2), FIB (3), LSCS (4) |
| 100-199 | Kriminelle       | Vagos (100), LCN (101), MC (102)        |
| 200-999 | Neutrale/Firmen  | Taxi (200), News (201)                  |

**Manager:** `FactionManager.cs`

**Wichtige Methoden:**
```csharp
// Alle Fraktionen laden
var allFactions = FactionManager.Instance.GetAllFactions();

// Bestimmte Fraktion holen
var lspd = FactionManager.Instance.GetFaction(1);

// Charakter zu Fraktion hinzufügen
FactionManager.Instance.AddCharacterToFaction(characterId, factionId, rankLevel: 0, "John Doe");

// Charakter befördern/degradieren
FactionManager.Instance.PromoteCharacter(characterId);
FactionManager.Instance.DemoteCharacter(characterId);

// Duty-Status setzen
FactionManager.Instance.SetDutyStatus(characterId, onDuty: true);

// Rang-Informationen
var rank = FactionManager.Instance.GetCharacterRank(characterId);
if (rank.CanInvite) { /* Spieler kann einladen */ }
```

---

## 🔄 Workflow-Beispiele

### **Spieler verbindet sich:**

```csharp
[ServerEvent(Event.PlayerConnected)]
public void OnPlayerConnected(GTANetworkAPI.Player player)
{
    // 1. Zeige Login-Screen
    player.TriggerEvent("client:showLoginUI");
}
```

### **Spieler loggt sich ein:**

```csharp
// 1. Account authentifizieren
var account = AccountManager.Instance.LoadAccount(username);
if (!AccountManager.Instance.AuthenticateAccount(username, passwordHash))
{
    return; // Login fehlgeschlagen
}

// 2. Prüfe Ban
if (account.IsBanned)
{
    player.Kick($"Gebannt: {account.BanReason}");
    return;
}

// 3. Permission laden
var permission = AccountManager.Instance.GetPermission(account.Id);

// 4. Zeige Charakter-Auswahl
var characters = CharacterManager.Instance.LoadCharactersByAccount(account.Id);
player.TriggerEvent("client:showCharacterSelection", characters);
```

### **Spieler wählt Charakter:**

```csharp
// 1. Charakter setzen
CharacterManager.Instance.SetPlayerCharacter(player, selectedCharacter);

// 2. Lade Fraktions-Daten falls vorhanden
if (selectedCharacter.IsInFaction())
{
    var faction = FactionManager.Instance.GetFaction(selectedCharacter.FactionId.Value);
    var rank = FactionManager.Instance.GetCharacterRank(selectedCharacter.Id);
    
    player.SendChatMessage($"Du bist {rank.Name} bei {faction.Name}");
}

// 3. Spawn Spieler
player.Position = selectedCharacter.LastPosition;
player.TriggerEvent("client:characterLoaded");
```

### **Admin befördert Fraktionsmitglied:**

```csharp
[Command("promote")]
public void CMD_Promote(GTANetworkAPI.Player admin, GTANetworkAPI.Player target)
{
    // 1. Prüfe Admin-Rechte
    var adminChar = CharacterManager.Instance.GetPlayerCharacter(admin);
    var adminRank = FactionManager.Instance.GetCharacterRank(adminChar.Id);
    
    if (!adminRank.CanPromote)
    {
        admin.SendChatMessage("~r~Keine Berechtigung!");
        return;
    }
    
    // 2. Befördern
    var targetChar = CharacterManager.Instance.GetPlayerCharacter(target);
    if (FactionManager.Instance.PromoteCharacter(targetChar.Id))
    {
        var newRank = FactionManager.Instance.GetCharacterRank(targetChar.Id);
        admin.SendChatMessage($"~g~{targetChar.FullName} zu {newRank.Name} befördert!");
        target.SendChatMessage($"~g~Du wurdest zu {newRank.Name} befördert!");
    }
}
```

---

## 🗄️ Datenbank

**Schema:** `database/schema_modular.sql`

**Tabellen:**
- `accounts` - Account Login-Daten
- `player_permissions` - OOC Rechte
- `characters` - IC Charaktere
- `factions` - Fraktionen
- `faction_ranks` - Ränge
- `faction_members` - Mitgliedschaften

---

## ✅ Vorteile dieser Architektur

### 1. **Klare Trennung**
- Account ≠ Character (ein Account kann mehrere Chars haben)
- OOC Rechte (Admin) ≠ IC Rechte (Fraktionsrang)

### 2. **Einfach erweiterbar**
Neue Features können hinzugefügt werden ohne bestehenden Code zu ändern:
- Neues System → Neues Model + Manager
- Neue Properties → Einfach zum Model hinzufügen

### 3. **Wiederverwendbar**
Alle Manager-Methoden sind zentral und können überall verwendet werden.

### 4. **Testbar**
Jedes System kann unabhängig getestet werden.

### 5. **Skalierbar**
Die Struktur funktioniert für kleine und große Server gleichermaßen.

---

## �️ Location/Teleport System

**Zweck:** Verwaltet Teleport-Locations aus JSON-Datei

**Dateien:**
- `Commands/Commands.cs` (TeleportLocation Model + LoadTeleportLocations)
- `configs/teleportLocations.json` (Location-Datenbank)

### **Location-Struktur**

```json
{
  "Name": "LSPD",
  "X": 425.1,
  "Y": -979.5,
  "Z": 30.7,
  "Rotation": 180.0,
  "RequiredPermissionLevel": 0
}
```

**Properties:**
- `Name` - Location-Name für Command
- `X, Y, Z` - Koordinaten
- `Rotation` - Blickrichtung beim Spawn
- `RequiredPermissionLevel` - Minimales Admin-Level (0 = alle, 1+ = Staff)

### **Verfügbare Commands**

#### `/tp <locationName>`
Teleportiert Spieler zu vordefinierter Location
- Zeigt verfügbare Locations wenn Name falsch
- Filtert nach Permission Level
- Setzt Position + Rotation

**Beispiele:**
```
/tp LSPD         → Teleport zur LSPD
/tp Airport      → Teleport zum Flughafen
/tp MilitaryBase → Nur für Admin Level 3+
```

#### `/vtp <locationName>`
Teleportiert Spieler **mit Fahrzeug** zu Location
- Prüft ob Spieler in Fahrzeug sitzt
- Teleportiert Fahrzeug + Spieler
- Gleiche Permission-Checks wie /tp

**Beispiel:**
```
/vtp Airport → Fahrzeug wird zum Flughafen teleportiert
```

#### `/tpcoord <x> <y> <z>`
Teleportiert zu exakten Koordinaten (Admin only)
- Erfordert Administrator-Rechte
- Für Entwicklung und präzise Positions-Tests

### **Location-Kategorien**

**Öffentliche Locations** (RequiredPermissionLevel: 0)
- LSPD, LSMD, Bank, Airport, Grove, Beach, Pier
- Ammunation, Casino, Vinewood, Paleto, Sandy, Docks
- Farmhouse, Chumash

**Staff-Locations** (RequiredPermissionLevel: 2-3)
- Prison (Moderator+)
- FIB, MazeBank, MilitaryBase, Zancudo (Administrator+)

### **Locations hinzufügen**

1. Öffne `configs/teleportLocations.json`
2. Füge neue Location hinzu:
```json
{
  "Name": "MeinOrt",
  "X": 123.4,
  "Y": 567.8,
  "Z": 90.1,
  "Rotation": 45.0,
  "RequiredPermissionLevel": 0
}
```
3. Speichern → Server automatisch beim nächsten Load

### **Tipps für Position/Rotation**

```csharp
// Ingame mit /pos aktuelle Position anzeigen
/pos
// Output: X: 123.45 Y: 567.89 Z: 90.12 Rotation: 45.67

// Für JSON kopieren und einfügen
```

### **Integration in Commands**

```csharp
// Location-Liste wird beim ersten /tp automatisch geladen
LoadTeleportLocations();

// Locations sind in BaseCommands verfügbar
protected static List<TeleportLocation> TeleportLocations;

// Alle Commands (Admin/Player/Faction) haben Zugriff
```

### **Automatisches Permission-Filtering**

```csharp
// Spieler sieht nur Locations mit erlaubtem Level
/tp wrongname
// Output: 
// Ort nicht gefunden!
// Verfügbare Orte: LSPD, Bank, Airport, Grove... (nur Level 0 Orte)

// Admin sieht alle Locations
/tp wrongname (als Admin)
// Verfügbare Orte: LSPD, Bank, FIB, MazeBank, MilitaryBase... (alle)
```

---

## �🚀 Nächste Schritte

Weitere Module die hinzugefügt werden können:

1. **Inventory System** (`Models/Inventory/`)
   - Item.cs
   - InventoryManager.cs

2. **Vehicle System** (`Models/Vehicle/`)
   - Vehicle.cs (persönliche Fahrzeuge)
   - VehicleManager.cs

3. **Housing System** (`Models/Housing/`)
   - House.cs
   - HouseManager.cs

4. **Business System** (`Models/Business/`)
   - Business.cs
   - BusinessManager.cs

5. **Banking System** (`Models/Banking/`)
   - BankAccount.cs
   - Transaction.cs
   - BankingManager.cs

Jedes neue System folgt dem gleichen Pattern: **Model + Manager**

---

## 📝 Best Practices

1. **Niemals direkten Datenbankzugriff in Models**
   → Immer über Manager

2. **Validation in Managern**
   → Manager prüfen ob Operationen erlaubt sind

3. **Alle IDs sind int**
   → Einfache Referenzen zwischen Tabellen

4. **DateTime für alle Zeitstempel**
   → Konsistente Zeitverwaltung

5. **NULL-Handling**
   → Nullable Properties (?) für optionale Werte

---

**Viel Erfolg mit dem modularen Aufbau! 🎮**
