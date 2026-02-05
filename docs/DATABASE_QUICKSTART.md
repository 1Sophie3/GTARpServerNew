# Quick Start: Datenbank Verwendung

## 🚀 Schnellstart

### 1. MySQL Server Setup
```sql
-- Datenbank erstellen
CREATE DATABASE ragemp_rp CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Schema importieren
mysql -u root -p ragemp_rp < database/schema_modular.sql
```

### 2. Konfiguration anpassen
Bearbeite `configs/database.json`:
```json
{
  "database": {
    "host": "localhost",
    "port": 3306,
    "user": "root",
    "password": "dein_passwort",
    "database": "ragemp_rp",
    "charset": "utf8mb4"
  }
}
```

### 3. Server starten
Der DatabaseManager wird automatisch in `Main.cs` initialisiert:
```csharp
✓ Datenbankverbindung erfolgreich  // Wenn alles geklappt hat
✗ Datenbankverbindung fehlgeschlagen  // Bei Fehler
```

## 📝 Code-Beispiele

### Account System

#### Account erstellen (z.B. bei Registration):
```csharp
using RPCore.Managers;
using System.Threading.Tasks;

public async Task RegisterPlayer(string username, string password)
{
    // Password hashen (z.B. mit BCrypt)
    string hashedPassword = HashPassword(password);
    
    // Account erstellen (E-Mail wird nicht mehr bei Registrierung benötigt)
    var account = await AccountManager.Instance.CreateAccount(
        username: username,
        passwordHash: hashedPassword,
        socialClubName: player.SocialClubName,
        hardwareId: player.Serial
    );
    
    if (account != null)
    {
        player.SendChatMessage("~g~Account erfolgreich erstellt!");
    }
    else
    {
        player.SendChatMessage("~r~Fehler: Username existiert bereits!");
    }
}
```

#### Account Login:
```csharp
public async Task LoginPlayer(Player player, string username, string password)
{
    string hashedPassword = HashPassword(password);
    
    // Authentifizierung
    bool success = await AccountManager.Instance.AuthenticateAccount(username, hashedPassword);
    
    if (success)
    {
        var account = await AccountManager.Instance.LoadAccount(username);
        
        // Ban-Check
        if (account.IsBanned)
        {
            player.SendChatMessage($"~r~Du wurdest gebannt: {account.BanReason}");
            player.Kick("Banned");
            return;
        }
        
        // Permission laden
        var permission = AccountManager.Instance.GetPermission(account.Id);
        
        player.SendChatMessage($"~g~Login erfolgreich! Rang: {permission?.Level}");
        
        // Zeige Character-Auswahl
        ShowCharacterSelection(player, account.Id);
    }
    else
    {
        player.SendChatMessage("~r~Falscher Username oder Passwort!");
    }
}
```

### Character System

#### Character erstellen:
```csharp
public async Task CreateNewCharacter(Player player, int accountId, string firstName, string lastName)
{
    var character = await CharacterManager.Instance.CreateCharacter(
        accountId: accountId,
        firstName: firstName,
        lastName: lastName
    );
    
    if (character != null)
    {
        player.SendChatMessage($"~g~Character {character.FullName} erstellt!");
        
        // Character zuweisen
        CharacterManager.Instance.SetPlayerCharacter(player, character);
        player.Spawn(character.LastPosition);
    }
    else
    {
        player.SendChatMessage("~r~Name bereits vergeben!");
    }
}
```

#### Character laden (bei Login):
```csharp
public async Task SelectCharacter(Player player, int accountId)
{
    // Alle Characters des Accounts laden
    var characters = await CharacterManager.Instance.LoadCharactersByAccount(accountId);
    
    if (characters.Count == 0)
    {
        player.SendChatMessage("~y~Du hast noch keinen Character. Erstelle einen!");
        ShowCharacterCreator(player, accountId);
        return;
    }
    
    // Zeige Auswahl oder lade ersten Character
    var character = characters[0];
    CharacterManager.Instance.SetPlayerCharacter(player, character);
    
    player.SendChatMessage($"~g~Willkommen zurück, {character.FullName}!");
}
```

#### Character speichern (bei Disconnect):
```csharp
[ServerEvent(Event.PlayerDisconnected)]
public async void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
{
    var character = CharacterManager.Instance.GetPlayerCharacter(player);
    
    if (character != null)
    {
        // Position/Stats aktualisieren
        character.LastPosition = player.Position;
        character.LastRotation = player.Rotation;
        character.Health = player.Health;
        character.Armor = player.Armor;
        
        // In Datenbank speichern
        await CharacterManager.Instance.SaveCharacter(character);
        
        // Zuordnung entfernen
        await CharacterManager.Instance.RemovePlayerCharacter(player);
        
        NAPI.Util.ConsoleOutput($"[DISCONNECT] {character.FullName} gespeichert");
    }
}
```

### Geld System

#### Geld geben/nehmen:
```csharp
public async Task PayPlayer(Player from, Player to, int amount)
{
    var charFrom = CharacterManager.Instance.GetPlayerCharacter(from);
    var charTo = CharacterManager.Instance.GetPlayerCharacter(to);
    
    if (charFrom == null || charTo == null) return;
    
    if (charFrom.Cash < amount)
    {
        from.SendChatMessage("~r~Du hast nicht genug Geld!");
        return;
    }
    
    // Geld transferieren
    await CharacterManager.Instance.TakeMoney(charFrom, amount);
    await CharacterManager.Instance.GiveMoney(charTo, amount);
    
    from.SendChatMessage($"~g~Du hast ${amount} an {charTo.FullName} bezahlt");
    to.SendChatMessage($"~g~Du hast ${amount} von {charFrom.FullName} erhalten");
}
```

#### Bank-Transfer:
```csharp
[Command("withdraw")]
public async void WithdrawMoney(Player player, int amount)
{
    var character = CharacterManager.Instance.GetPlayerCharacter(player);
    if (character == null) return;
    
    if (character.BankBalance < amount)
    {
        player.SendChatMessage("~r~Nicht genug Geld auf dem Konto!");
        return;
    }
    
    await CharacterManager.Instance.TakeBankMoney(character, amount);
    await CharacterManager.Instance.GiveMoney(character, amount);
    
    player.SendChatMessage($"~g~${amount} abgehoben. Bargeld: ${character.Cash}, Bank: ${character.BankBalance}");
}

[Command("deposit")]
public async void DepositMoney(Player player, int amount)
{
    var character = CharacterManager.Instance.GetPlayerCharacter(player);
    if (character == null) return;
    
    if (character.Cash < amount)
    {
        player.SendChatMessage("~r~Nicht genug Bargeld!");
        return;
    }
    
    await CharacterManager.Instance.TakeMoney(character, amount);
    await CharacterManager.Instance.GiveBankMoney(character, amount);
    
    player.SendChatMessage($"~g~${amount} eingezahlt. Bargeld: ${character.Cash}, Bank: ${character.BankBalance}");
}
```

### Permission System

#### Admin-Rang setzen:
```csharp
[Command("setadmin", GreedyArg = true)]
public async void SetAdminCommand(Player admin, string targetName, int level)
{
    // Permission Check für Admin
    var adminChar = CharacterManager.Instance.GetPlayerCharacter(admin);
    if (adminChar == null) return;
    
    var adminPerm = AccountManager.Instance.GetPermission(adminChar.AccountId);
    if (adminPerm == null || adminPerm.Level < PermissionLevel.Owner)
    {
        admin.SendChatMessage("~r~Keine Berechtigung!");
        return;
    }
    
    // Finde Ziel-Spieler
    var target = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => 
        p.Name.ToLower().Contains(targetName.ToLower()));
    
    if (target == null)
    {
        admin.SendChatMessage("~r~Spieler nicht gefunden!");
        return;
    }
    
    var targetChar = CharacterManager.Instance.GetPlayerCharacter(target);
    if (targetChar == null) return;
    
    // Setze Permission
    var newLevel = (PermissionLevel)level;
    await AccountManager.Instance.SetPermission(
        targetChar.AccountId, 
        newLevel, 
        adminChar.FullName
    );
    
    admin.SendChatMessage($"~g~{targetChar.FullName} ist jetzt {newLevel}");
    target.SendChatMessage($"~g~Dein Rang wurde auf {newLevel} gesetzt!");
}
```

### Level & Experience

#### Erfahrung geben:
```csharp
public async Task GiveJobReward(Player player, int money, int exp)
{
    var character = CharacterManager.Instance.GetPlayerCharacter(player);
    if (character == null) return;
    
    int oldLevel = character.Level;
    
    await CharacterManager.Instance.GiveMoney(character, money);
    await CharacterManager.Instance.GiveExperience(character, exp);
    
    player.SendChatMessage($"~g~+${money} und +{exp} EXP");
    
    // Level-Up benachrichtigung
    if (character.Level > oldLevel)
    {
        player.SendChatMessage($"~y~★ LEVEL UP! Du bist jetzt Level {character.Level}! ★");
        NAPI.ClientEvent.TriggerClientEventForAll("showLevelUp", player, character.Level);
    }
}
```

## 🔍 Debugging

### Datenbank-Verbindung testen:
```csharp
bool connected = await DatabaseManager.Instance.TestConnection();
if (connected)
{
    NAPI.Util.ConsoleOutput("✓ Datenbank läuft");
}
```

### Manuelle Query (für Tests):
```csharp
// SELECT
var reader = await DatabaseManager.Instance.ExecuteReader(
    "SELECT * FROM characters WHERE id = @id",
    DatabaseManager.CreateParameter("@id", characterId)
);

while (await reader.ReadAsync())
{
    string name = reader.GetString("first_name");
    NAPI.Util.ConsoleOutput($"Character: {name}");
}
reader.Close();

// UPDATE
int affected = await DatabaseManager.Instance.ExecuteNonQuery(
    "UPDATE characters SET money = money + @amount WHERE id = @id",
    DatabaseManager.CreateParameter("@amount", 1000),
    DatabaseManager.CreateParameter("@id", characterId)
);
NAPI.Util.ConsoleOutput($"{affected} Zeilen aktualisiert");

// COUNT
var result = await DatabaseManager.Instance.ExecuteScalar(
    "SELECT COUNT(*) FROM accounts"
);
int accountCount = Convert.ToInt32(result);
NAPI.Util.ConsoleOutput($"{accountCount} Accounts in der Datenbank");
```

## ⚠️ Wichtige Hinweise

### Async/Await
Alle Datenbank-Methoden sind **async** und müssen mit **await** aufgerufen werden:
```csharp
✅ var account = await AccountManager.Instance.LoadAccount("Username");
❌ var account = AccountManager.Instance.LoadAccount("Username"); // Fehler!
```

### Connection Pooling
- Der DatabaseManager verwaltet automatisch Connection Pooling
- Keine manuellen Open/Close Calls nötig
- `using`-Statements sorgen für Auto-Cleanup

### Prepared Statements
Verwende **IMMER** Parameter für Benutzereingaben:
```csharp
✅ DatabaseManager.CreateParameter("@name", username)
❌ $"SELECT * FROM accounts WHERE username = '{username}'" // SQL Injection!
```

### Cache
AccountManager und CharacterManager verwenden In-Memory Caches:
- Accounts werden nach erstem Load gecacht
- Characters werden nach Load gecacht
- Bei Updates wird Cache automatisch aktualisiert

### Performance
- Connection Pooling: 5-100 gleichzeitige Verbindungen
- Prepared Statements sind gecacht und schneller
- Reader mit `CommandBehavior.CloseConnection` schließt automatisch

## 📚 Weitere Ressourcen

- [DATABASE_INTEGRATION.md](DATABASE_INTEGRATION.md) - Vollständige Dokumentation
- [MODULARE_ARCHITEKTUR.md](MODULARE_ARCHITEKTUR.md) - Architektur-Übersicht
- [COMMANDS_UEBERSICHT.md](COMMANDS_UEBERSICHT.md) - Alle Commands
- [schema_modular.sql](../database/schema_modular.sql) - Datenbank-Schema
