// Deaktiviert die automatische Gesundheitsregeneration dauerhaft
function disableHealthRegen() {
    // Setzt den Regenerationsmultiplikator auf 0, um automatische Heilung zu unterbinden.
    mp.game.player.setHealthRechargeMultiplier(0.0);
}

// Überprüft und setzt validierte Werte für Gesundheit und Rüstung
function setPlayerState(health, armor) {
    try {
        // Debug: Übergebene Werte anzeigen
        mp.gui.chat.push(`[DEBUG] Empfangen: Health=${health}, Armor=${armor}`);

        const parsedHealth = parseInt(health, 10);
        const parsedArmor = parseInt(armor, 10);

        if (isNaN(parsedHealth) || isNaN(parsedArmor)) {
            throw new Error(`Ungültige Eingabewerte: Health=${health}, Armor=${armor}`);
        }

        const clampedHealth = Math.max(0, Math.min(parsedHealth, 100));
        const clampedArmor = Math.max(0, Math.min(parsedArmor, 100));

        mp.players.local.setHealth(clampedHealth);
        mp.players.local.setArmour(clampedArmor);

        mp.gui.chat.push(`[DEBUG] Spielerzustand gesetzt: Health=${clampedHealth}, Armor=${clampedArmor}`);
    }
    catch (error) {
        mp.gui.chat.push(`[ERROR] Fehler in setPlayerState: ${error.message}`);
    }
}

// Initialisiert die Health-Regeneration-Deaktivierung, sobald der Spieler bereit ist.
mp.events.add("playerReady", () => {
    try {
        disableHealthRegen();
        setInterval(disableHealthRegen, 1000); // Wiederholt alle 1 Sekunde
    }
    catch (error) {
        mp.gui.chat.push(`[ERROR] Fehler beim Initialisieren der Regeneration: ${error.message}`);
    }
});

// Wiederherstellen des Zustands, basierend auf vom Server gesendeten Werten
mp.events.add("restorePlayerState", (health, armor) => {
    try {
        setPlayerState(health, armor);
    }
    catch (error) {
        mp.gui.chat.push(`[ERROR] Fehler beim Wiederherstellen: ${error.message}`);
    }
});

// Variable, um den CEF-Browser (Deathscreen) zu speichern
let deathBrowser = null;
// Aktualisiert den Countdown-Wert im Deathscreen, basierend auf Server-Updates.
mp.events.add("updateDeathTimer", (secondsRemaining) => {
    try {
        if (deathBrowser) {
            // Ruft in der CEF-Webseite die JS-Funktion "updateTimer" auf.
            // Stelle sicher, dass deine death.html eine solche Funktion definiert,
            // die beispielsweise den innerText eines Countdown-Elements aktualisiert.
            deathBrowser.execute(`updateTimer(${secondsRemaining});`);
            mp.gui.chat.push(`[DEBUG] Countdown updated to ${secondsRemaining} seconds`);
        }
    }
    catch (error) {
        mp.gui.chat.push(`[ERROR] Fehler beim Aktualisieren des Timers: ${error.message}`);
    }
});

// Aktiviert den Todesbildschirm und öffnet den CEF-Browser
mp.events.add("startDeathEffect", () => {
    try {
        // Starte den GTA-Native Effekt "DeathFailMPDark"
        mp.game.graphics.startScreenEffect("DeathFailMPDark", 0, true);
        mp.gui.chat.push("[INFO] Todesbildschirm aktiviert.");
        
        // Öffne den CEF-Browser, falls er noch nicht existiert. Dieser zeigt den transparenten, zentrierten Countdown.
        if (!deathBrowser) {
            deathBrowser = mp.browsers.new("package://death/death.html"); 
        }
    }
    catch (error) {
        mp.gui.chat.push(`[ERROR] Fehler beim Aktivieren des Todesbildschirms: ${error.message}`);
    }
});

// Stoppt den Todesbildschirm-Effekt und schließt den CEF-Browser
mp.events.add("stopDeathEffect", () => {
    try {
        mp.game.graphics.stopScreenEffect("DeathFailMPDark");
        mp.gui.chat.push("[INFO] Todesbildschirm deaktiviert.");
        
        if (deathBrowser) {
            deathBrowser.destroy();
            deathBrowser = null;
        }
    }
    catch (error) {
        mp.gui.chat.push(`[ERROR] Fehler beim Deaktivieren des Todesbildschirms: ${error.message}`);
    }
});
