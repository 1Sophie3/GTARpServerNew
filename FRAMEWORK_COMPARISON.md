# Ordnerstruktur-Vergleich: RAGE:MP vs FiveM

## 🎮 RAGE:MP Ordnerstruktur (C#)

```
ragemp-roleplay-project/
│
├── server-packages/              # Server-Side Code
│   ├── rp-core/                  # Haupt-Gamemode (C#)
│   │   ├── rp-core.csproj
│   │   ├── Main.cs
│   │   ├── Database/
│   │   │   └── DatabaseManager.cs
│   │   ├── Player/
│   │   │   ├── PlayerData.cs
│   │   │   └── PlayerManager.cs
│   │   ├── Commands/
│   │   │   └── Commands.cs
│   │   ├── Events/
│   │   │   └── ServerEvents.cs
│   │   ├── Jobs/
│   │   ├── Vehicles/
│   │   └── Factions/
│   │
│   └── rp-database/              # Optionales separates DB-Modul
│       └── DatabaseHelper.cs
│
├── client-packages/              # Client-Side Code
│   └── rp-client/
│       ├── index.js              # Hauptdatei
│       ├── events/
│       ├── ui/
│       └── utils/
│
├── cef/                          # Browser UI (Vue 3)
│   └── rp-ui/
│       ├── package.json
│       ├── vite.config.js
│       ├── login.html
│       ├── hud.html
│       ├── inventory.html
│       └── src/
│           ├── components/
│           │   ├── Login.vue
│           │   ├── Hud.vue
│           │   └── Inventory.vue
│           ├── assets/
│           └── utils/
│
├── database/                     # SQL Dateien
│   ├── schema.sql
│   └── migrations/
│
├── configs/                      # Konfigurationen
│   ├── conf.json                 # Server Config
│   ├── database.json             # DB Config
│   └── database.json.example
│
├── docs/                         # Dokumentation
│
├── .gitignore
└── README.md
```

---

## 🚗 FiveM Ordnerstruktur (Lua + JS)

```
fivem-roleplay-project/
│
├── resources/                    # Alle Ressourcen
│   │
│   ├── [core]/                   # Core-Ressourcen
│   │   ├── rp-core/
│   │   │   ├── fxmanifest.lua
│   │   │   ├── server/
│   │   │   │   ├── main.lua
│   │   │   │   ├── database.lua
│   │   │   │   ├── player.lua
│   │   │   │   └── events.lua
│   │   │   ├── client/
│   │   │   │   ├── main.lua
│   │   │   │   ├── events.lua
│   │   │   │   └── ui.lua
│   │   │   ├── shared/
│   │   │   │   ├── config.lua
│   │   │   │   └── utils.lua
│   │   │   └── html/              # NUI (Browser UI)
│   │   │       ├── index.html
│   │   │       ├── style.css
│   │   │       └── script.js
│   │   │
│   │   └── rp-database/
│   │       ├── fxmanifest.lua
│   │       └── server/
│   │           └── database.lua
│   │
│   ├── [gameplay]/               # Gameplay Features
│   │   ├── rp-jobs/
│   │   │   ├── fxmanifest.lua
│   │   │   ├── server/
│   │   │   └── client/
│   │   │
│   │   ├── rp-vehicles/
│   │   ├── rp-inventory/
│   │   ├── rp-housing/
│   │   └── rp-factions/
│   │
│   ├── [ui]/                     # UI Ressourcen (Vue 3)
│   │   └── rp-ui/
│   │       ├── fxmanifest.lua
│   │       ├── package.json
│   │       ├── vite.config.js
│   │       ├── src/
│   │       │   ├── components/
│   │       │   │   ├── Login.vue
│   │       │   │   ├── Hud.vue
│   │       │   │   └── Inventory.vue
│   │       │   ├── App.vue
│   │       │   └── main.js
│   │       └── dist/             # Build Output
│   │
│   └── [maps]/                   # Custom Maps (optional)
│
├── database/                     # SQL Schema
│   ├── schema.sql
│   └── migrations/
│
├── server-data/                  # Server Konfiguration
│   ├── server.cfg
│   └── resources.cfg
│
├── .gitignore
└── README.md
```

---

## 📊 Direkter Vergleich

| Feature | RAGE:MP | FiveM |
|---------|---------|-------|
| **Server-Sprache** | C# | Lua (oder JS/TS) |
| **Client-Sprache** | JavaScript | Lua (oder JS) |
| **UI System** | CEF (Chromium) | NUI (Chromium) |
| **Ressourcen-System** | Packages | Resources mit fxmanifest.lua |
| **Performance** | ⭐⭐⭐⭐ Sehr gut | ⭐⭐⭐⭐⭐ Hervorragend |
| **Community** | ⭐⭐⭐ Kleiner | ⭐⭐⭐⭐⭐ Sehr groß |
| **Verfügbare Scripts** | ⭐⭐⭐ Weniger | ⭐⭐⭐⭐⭐ Sehr viele |
| **Lernkurve** | Mittel (C# Kenntnisse) | Leicht (Lua einfach) |
| **RP-Geeignet** | ⭐⭐⭐⭐⭐ Ja | ⭐⭐⭐⭐⭐ Ja |
| **Stabilität** | ⭐⭐⭐⭐ Gut | ⭐⭐⭐⭐⭐ Sehr gut |

---

## 💡 Welches Framework für RP-Server?

### **RAGE:MP** ✅
- Wenn du C# bevorzugst
- Kleinere, engere Community
- Etwas bessere Performance bei weniger Spielern
- Weniger fertige Scripts verfügbar

### **FiveM** ✅✅✅ (Empfohlen für RP!)
- **Größte RP-Community weltweit**
- Tausende fertige Scripts (kostenlos & premium)
- Frameworks wie ESX, QBCore direkt verfügbar
- Bessere Dokumentation
- Mehr Support & Tutorials
- OneSync für 500+ Spieler
- Lua ist leichter zu lernen

---

## 🎯 Meine Empfehlung

**Für einen RP-Server → FiveM**

**Gründe:**
1. **90% aller RP-Server laufen auf FiveM**
2. Riesige Auswahl an fertigen Job-Scripts, Inventar-Systemen, etc.
3. Frameworks wie **QBCore** oder **ESX** sparen dir Monate Entwicklungszeit
4. Bessere Performance mit vielen Spielern (OneSync)
5. Einfacher für Anfänger
6. Mehr Entwickler zum Rekrutieren

**RAGE:MP nur wenn:**
- Du unbedingt C# nutzen willst
- Du alles selbst programmieren möchtest
- Du eine kleinere, spezielle Community suchst

---

## 🚀 Schnellstart - Was möchtest du?

**Option 1: RAGE:MP (C#)** → Ich habe dir die Struktur schon erstellt
**Option 2: FiveM (Lua)** → Ich erstelle dir eine komplette FiveM-Struktur mit ESX/QBCore
**Option 3: FiveM (TypeScript)** → Moderne Alternative zu Lua

**Was passt besser zu deinem Projekt?**
