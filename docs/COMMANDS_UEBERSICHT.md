# 📋 Commands Übersicht - RolePlay Server

## Inhaltsverzeichnis

- [Admin Commands](#admin-commands)
- [Player Commands](#player-commands)
- [Faction Commands](#faction-commands)
- [Teleport Commands](#teleport-commands)

---

## 🛡️ Admin Commands

### Fahrzeug-Commands

| Command                         | Permission    | Beschreibung                      | Beispiel          |
| ------------------------------- | ------------- | --------------------------------- | ----------------- |
| `/av`                           | Supporter     | Spawnt Shotaro Motorrad           | `/av`             |
| `/veh <name> [color1] [color2]` | Administrator | Spawnt beliebiges Fahrzeug        | `/veh elegy 12 0` |
| `/car <name>`                   | Supporter     | Spawnt Fahrzeug (Standard-Farben) | `/car zentorno`   |

### Heal & Revive

| Command          | Permission | Beschreibung                       | Beispiel       |
| ---------------- | ---------- | ---------------------------------- | -------------- |
| `/heal`          | Supporter  | Heilt dich selbst (100 HP + Armor) | `/heal`        |
| `/aheal <name>`  | Moderator  | Heilt anderen Spieler              | `/aheal John`  |
| `/revive <name>` | Moderator  | Belebt toten Spieler wieder        | `/revive John` |

### Teleport

| Command                | Permission    | Beschreibung                   | Beispiel          |
| ---------------------- | ------------- | ------------------------------ | ----------------- |
| `/tp <location>`       | Variabel      | Teleport zu vordefiniertem Ort | `/tp LSPD`        |
| `/vtp <location>`      | Variabel      | Teleport mit Fahrzeug zu Ort   | `/vtp Airport`    |
| `/tpcoord <x> <y> <z>` | Administrator | Teleport zu Koordinaten        | `/tpcoord 0 0 72` |
| `/tpto <name>`         | Moderator     | Teleportiert zu Spieler        | `/tpto John`      |

### Waffen

| Command          | Permission | Beschreibung              | Beispiel           |
| ---------------- | ---------- | ------------------------- | ------------------ |
| `/weapon [name]` | Supporter  | Gibt Waffe mit 250 Schuss | `/weapon pistol50` |
| `/removeweapons` | Supporter  | Entfernt alle Waffen      | `/removeweapons`   |

### Verwaltung

| Command                    | Permission    | Beschreibung                   | Beispiel           |
| -------------------------- | ------------- | ------------------------------ | ------------------ |
| `/setadmin <name> <level>` | Owner         | Setzt Admin-Level (0-6)        | `/setadmin John 3` |
| `/godmode`                 | Administrator | Toggle Godmode für sich selbst | `/godmode`         |
| `/agodmode <name>`         | Administrator | Toggle Godmode für anderen     | `/agodmode John`   |

---

## 👤 Player Commands

### Allgemeine Commands

| Command  | Permission | Beschreibung                       | Beispiel |
| -------- | ---------- | ---------------------------------- | -------- |
| `/stats` | Spieler    | Zeigt deine Statistiken            | `/stats` |
| `/pos`   | Spieler    | Zeigt aktuelle Position + Rotation | `/pos`   |
| `/help`  | Spieler    | Zeigt verfügbare Commands          | `/help`  |
| `/time`  | Spieler    | Zeigt aktuelle Serverzeit          | `/time`  |

**Stats-Ausgabe zeigt:**
- Name, Level, Spielzeit
- Bargeld und Bank
- Job (bei Zivilisten)
- Fraktion und Rang (bei Mitgliedern)
- Staff-Level (bei Staff)

---

## 🏢 Faction Commands

### Mitglieder-Commands

| Command  | Permission     | Beschreibung                 | Beispiel |
| -------- | -------------- | ---------------------------- | -------- |
| `/duty`  | Faction Member | Toggle Dienst an/aus         | `/duty`  |
| `/finfo` | Faction Member | Zeigt Fraktionsinformationen | `/finfo` |

**Finfo zeigt:**
- Fraktionsname und Typ (Staat/Criminal/Neutral)
- Dein Rang und Gehalt
- Online-Mitglieder / Max. Mitglieder
- Fraktionskasse

---

## 🗺️ Teleport Commands

### Öffentliche Locations (Level 0)

Alle Spieler können zu diesen Orten teleportieren:

| Location Name | Beschreibung                     | Koordinaten        |
| ------------- | -------------------------------- | ------------------ |
| `LSPD`        | Los Santos Police Department     | Mission Row        |
| `LSMD`        | Pillbox Hill Medical Center      | Zentrum            |
| `Airport`     | Los Santos International Airport | Süden              |
| `Grove`       | Grove Street                     | Families Territory |
| `Bank`        | Maze Bank Filiale                | Zentrum            |
| `Ammunation`  | Waffenladen                      | Zentrum            |
| `Beach`       | Vespucci Beach                   | Strandpromenade    |
| `Pier`        | Del Perro Pier                   | Riesenrad          |
| `Docks`       | Los Santos Docks                 | Hafen              |
| `Vinewood`    | Vinewood Hills                   | Hollywood          |
| `Casino`      | Diamond Casino & Resort          | Vinewood           |
| `Paleto`      | Paleto Bay                       | Norden             |
| `Sandy`       | Sandy Shores                     | Wüste              |
| `Farmhouse`   | Grapeseed Farmhouse              | Farmgebiet         |
| `Chumash`     | Chumash Beach                    | Westküste          |

### Moderator Locations (Level 2+)

| Location Name | Beschreibung             |
| ------------- | ------------------------ |
| `Prison`      | Bolingbroke Penitentiary |

### Administrator Locations (Level 3+)

| Location Name  | Beschreibung               |
| -------------- | -------------------------- |
| `FIB`          | FIB Headquarters (Rooftop) |
| `MazeBank`     | Maze Bank Tower (Rooftop)  |
| `MilitaryBase` | Fort Zancudo Haupttor      |
| `Zancudo`      | Fort Zancudo Inneres       |

### Teleport-Verwendung

```bash
# Spieler zu Ort teleportieren
/tp LSPD

# Falsche Eingabe → zeigt verfügbare Orte
/tp wrongname
> Ort nicht gefunden!
> Verfügbare Orte: LSPD, Bank, Airport, Grove...

# Mit Fahrzeug teleportieren
/vtp Airport

# Koordinaten-Teleport (Admin only)
/tpcoord 123.4 567.8 90.1
```

---

## 🔐 Permission Levels

| Level | Name           | Rechte                                                           |
| ----- | -------------- | ---------------------------------------------------------------- |
| 0     | Spieler        | Basis-Commands, öffentliche Teleports                            |
| 1     | Supporter      | `/av`, `/car`, `/heal`, `/weapon`, `/removeweapons`              |
| 2     | Moderator      | `/aheal`, `/revive`, `/tpto`, Prison-Teleport                    |
| 3     | Administrator  | `/veh`, `/tpcoord`, `/godmode`, `/agodmode`, spezielle Teleports |
| 4     | HeadAdmin      | Alle Administrator-Rechte                                        |
| 5     | Projektleitung | Alle Administrator-Rechte                                        |
| 6     | Owner          | **Alle Rechte** + `/setadmin`                                    |

---

## 💡 Tipps & Tricks

### Position für neue Locations finden

```bash
# Ingame Position anzeigen
/pos
> X: 123.45 Y: 567.89 Z: 90.12
> Rotation: 45.67

# Console Output (für Entwicklung)
[POS] PlayerName: new Vector3(123.45f, 567.89f, 90.12f)
```

### Fahrzeug-Namen

Häufig verwendete Fahrzeugnamen für `/veh` und `/car`:

**Super Cars:**
- `t20`, `zentorno`, `entityxf`, `adder`, `osiris`, `xa21`

**Sports:**
- `elegy`, `sultan`, `kuruma`, `jester`, `massacro`

**Bikes:**
- `shotaro`, `hakuchou`, `bati`, `akuma`, `double`

**Police:**
- `police`, `police2`, `police3`, `policeb` (Motorrad)

**Emergency:**
- `ambulance`, `firetruk`, `lguard` (Lifeguard)

**Helicopters:**
- `polmav`, `maverick`, `frogger`, `buzzard`

### Waffen-Namen

Häufig verwendete Waffennamen für `/weapon`:

**Pistols:**
- `pistol`, `pistol50`, `combatpistol`, `heavypistol`

**SMGs:**
- `microsmg`, `smg`, `assaultsmg`

**Rifles:**
- `assaultrifle`, `carbinerifle`, `advancedrifle`

**Heavy:**
- `mg`, `combatmg`, `minigun`

**Shotguns:**
- `pumpshotgun`, `sawnoffshotgun`, `assaultshotgun`

**Sniper:**
- `sniperrifle`, `heavysniper`, `marksmanrifle`

---

## 📝 Neue Commands hinzufügen

### 1. Command in Commands.cs erstellen

```csharp
[Command("mycommand")]
public void CMD_MyCommand(GTANetworkAPI.Player player, string arg)
{
    if (!HasPermission(player, PermissionLevel.Moderator))
    {
        player.SendChatMessage("~r~Keine Berechtigung!");
        return;
    }

    // Command-Logik hier
    player.SendChatMessage($"~g~Command ausgeführt: {arg}");
}
```

### 2. Command in /help eintragen

```csharp
if (permission.HasPermission(PermissionLevel.Moderator))
{
    player.SendChatMessage("~w~/mycommand [arg] ~s~- Beschreibung");
}
```

### 3. Dokumentation aktualisieren

Trage den Command in diese Datei ein!

---

## 🔄 Locations bearbeiten

**Datei:** `configs/teleportLocations.json`

```json
{
  "Name": "NeuerOrt",
  "X": 123.4,
  "Y": 567.8,
  "Z": 90.1,
  "Rotation": 45.0,
  "RequiredPermissionLevel": 0
}
```

- **Name**: Command-Name (Case-Insensitive)
- **X, Y, Z**: Koordinaten (mit /pos ermitteln)
- **Rotation**: Blickrichtung (0-360°)
- **RequiredPermissionLevel**: 
  - `0` = Alle Spieler
  - `1` = Supporter+
  - `2` = Moderator+
  - `3` = Administrator+

Nach Speichern: Server neu laden oder nächster `/tp` lädt automatisch

---

**Letzte Aktualisierung:** 5. Februar 2026
