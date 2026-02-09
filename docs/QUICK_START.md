# 🚀 RAGE:MP Roleplay Server - Quick Start

## 📥 Installation auf deinem Entwicklungs-PC

### 1. Projekt herunterladen
Lade den kompletten Ordner `ragemp-roleplay-project` herunter und entpacke ihn.

### 2. Git Repository initialisieren
```bash
cd ragemp-roleplay-project
git init
git add .
git commit -m "Initial commit: RAGE:MP RP Server Structure"
```

### 3. Remote Repository hinzufügen (GitHub/GitLab)
```bash
# GitHub
git remote add origin https://github.com/DEIN-USERNAME/ragemp-rp-server.git
git branch -M main
git push -u origin main

# Oder GitLab
git remote add origin https://gitlab.com/DEIN-USERNAME/ragemp-rp-server.git
git branch -M main
git push -u origin main
```

## 🎨 Vue UI lokal entwickeln

```bash
cd cef/rp-ui
npm install
npm run dev
```
Öffne http://localhost:3000 - Du siehst Login & HUD

## 💻 C# Code entwickeln

**Visual Studio Code:**
```bash
cd server-packages/rp-core
code .
```

**Visual Studio:**
Öffne `server-packages/rp-core/rp-core.csproj`

## 📁 Ordnerstruktur Übersicht

```
ragemp-roleplay-project/
├── server-packages/        # C# Server Code
├── client-packages/        # JS Client Code
├── cef/rp-ui/             # Vue 3 UI
├── database/              # SQL Schema
├── configs/               # Konfigurationen
└── docs/                  # Dokumentation
```

## 🔧 Auf Server deployen

### 1. RAGE:MP Server installieren
Download von rage.mp

### 2. Repository auf Server klonen
```bash
cd /pfad/zu/ragemp/
git clone https://github.com/DEIN-USERNAME/ragemp-rp-server.git packages/rp-server
```

### 3. Datenbank einrichten
```bash
mysql -u root -p < packages/rp-server/database/schema.sql
```

### 4. Config anpassen
```bash
cd packages/rp-server/configs
cp database.json.example database.json
nano database.json  # DB-Zugangsdaten eintragen
```

### 5. Vue UI bauen
```bash
cd packages/rp-server/cef/rp-ui
npm install
npm run build
```

### 6. C# kompilieren
```bash
cd packages/rp-server/server-packages/rp-core
dotnet build -c Release
```

### 7. Server starten
```bash
cd /pfad/zu/ragemp/
./ragemp-server
```

## 📝 Entwicklungs-Workflow

1. Lokal entwickeln (ohne RAGE:MP)
2. Code testen
3. Git commit & push
4. Auf Server: `git pull`
5. Neu kompilieren/bauen
6. Server neustarten

## 🆘 Hilfe & Support

- RAGE:MP Docs: https://wiki.rage.mp/
- C# API: https://wiki.rage.mp/index.php?title=Category:Scripting
- Discord: RAGE:MP Community

Viel Erfolg! 🎮
