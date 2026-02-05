# RAGE:MP Roleplay Server - Entwicklungsumgebung

## 📁 Projektstruktur

Dieses Projekt ist so aufgebaut, dass du lokal ohne Admin-Rechte entwickeln kannst und per Git synchronisierst.

## 🚀 Setup ohne RAGE:MP Installation

1. Entwickle in diesem Ordner (kein RAGE:MP Server nötig)
2. Nutze Git für Versionskontrolle
3. Deploye auf deinen Server mit RAGE:MP Installation

## 📦 Ordnerstruktur

```
ragemp-roleplay-project/
├── server-packages/          # Server-Side C# Code
│   ├── rp-core/              # Haupt-Gamemode
│   └── rp-database/          # Datenbank-Handler
├── client-packages/          # Client-Side JavaScript
│   └── rp-client/            # Client Logik
├── cef/                      # CEF/Browser UI (Vue 3)
│   └── rp-ui/                # Vue 3 Frontend
├── database/                 # SQL Schemas & Migrations
├── configs/                  # Server Konfigurationen
└── docs/                     # Dokumentation
```

## 🔧 Technologie-Stack

- **Server:** C# (.NET 6+)
- **Client:** JavaScript
- **CEF/UI:** Vue 3 + Vite
- **Datenbank:** MySQL/MariaDB
- **Versionskontrolle:** Git

## 📝 Entwicklungs-Workflow

1. Lokal in diesem Ordner entwickeln
2. Testen mit Mock-Daten (ohne RAGE:MP)
3. Git Commit & Push
4. Auf Server mit RAGE:MP pullen und deployen
