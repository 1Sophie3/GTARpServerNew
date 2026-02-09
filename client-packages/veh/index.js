// client_packages/garage/index.js
// Dies ist das Haupt-Client-Side-Skript für dein Garagen-System

let garageBrowser = null; // Globale Variable, um den Browser zu speichern
let isGarageOpen = false; // NEU: Flag, um den Zustand des Browsers zu verfolgen

// Event-Handler, der vom Server ausgelöst wird, um den Garagen-Browser anzuzeigen
mp.events.add("Client:Garage:ShowGarageBrowser", (inGarageJson, nearbyParkableVehiclesJson, maxVehicles) => {
    // Sicherstellung: Nur öffnen, wenn nicht bereits offen
    if (isGarageOpen) {
        console.log("[CLIENT_SIDE] Garagen-Browser ist bereits offen. Ignoriere erneuten Öffnungsversuch.");
        return;
    }

    // Falls bereits ein Browser-Objekt existiert (unerwartet, aber zur Sicherheit), zerstöre es
    if (mp.browsers.exists(garageBrowser)) {
        garageBrowser.destroy();
        garageBrowser = null;
        console.log("[CLIENT_SIDE] Vorhandener (unerwarteter) Garagen-Browser zerstört.");
    }

    // Erstelle einen neuen Browser und lade die HTML-Datei
    garageBrowser = mp.browsers.new("package://veh/garage.html");
    isGarageOpen = true; // Setze das Flag auf true

    // Zeige den Mauscursor an und deaktiviere HUD/Radar im Spiel
    mp.gui.cursor.show(true, true);
    mp.game.ui.displayRadar(false);
    mp.game.ui.displayHud(false);
    console.log("[CLIENT_SIDE] Garagen-Browser erstellt und Cursor/HUD angepasst.");

    // Wichtig: Eine kleine Verzögerung geben, damit der Browser vollständig geladen ist,
    // bevor JavaScript im Browser ausgeführt wird.
    setTimeout(() => {
        if (mp.browsers.exists(garageBrowser)) {
            // Daten an die JavaScript-Funktion im Browser senden
            garageBrowser.execute(`
                const inGarageVehicles = ${inGarageJson};
                const nearbyParkableVehicles = ${nearbyParkableVehiclesJson}; // NEU: Daten für parkbare Fahrzeuge
                const maxGarageVehicles = ${maxVehicles};
                // Stelle sicher, dass die Funktion im Browser-JS geladen ist
                if (typeof showGarageData === 'function') {
                    showGarageData(inGarageVehicles, nearbyParkableVehicles, maxGarageVehicles);
                    console.log('Browser: showGarageData im Browser aufgerufen.');
                } else {
                    console.error('Browser: showGarageData ist im Browser nicht definiert! Ist garagescript.js korrekt geladen?');
                }
                // Debug-Logs im Browser-Konsole für CEF-Debugging
                console.log('Browser: typeof mp:', typeof mp);
                console.log('Browser: typeof mp.trigger:', typeof mp.trigger);
            `);
        } else {
            console.error("[CLIENT_SIDE] Garagen-Browser existiert nicht beim Versuch, execute auszuführen.");
        }
    }, 200); // 200 ms Verzögerung hat sich bewährt
});

// Event-Handler, der vom Server ausgelöst wird, um den Browser zu verstecken
// UND/ODER der "X"-Button im Browser ruft diese Funktion auch auf (indirekt über Server-Event)
mp.events.add("Client:Garage:HideGarageBrowser", () => {
    // Sicherstellung: Nur schließen, wenn es auch offen ist
    if (mp.browsers.exists(garageBrowser)) {
        garageBrowser.destroy();
        garageBrowser = null;
        isGarageOpen = false; // Setze das Flag auf false
        
        // Immer Cursor und HUD zurücksetzen, wenn der Browser geschlossen wird
        mp.gui.cursor.show(false, false);
        mp.game.ui.displayRadar(true);
        mp.game.ui.displayHud(true);
        console.log("[CLIENT_SIDE] Garagen-Browser versteckt und Cursor/HUD zurückgesetzt.");
    } else {
        console.log("[CLIENT_SIDE] 'Client:Garage:HideGarageBrowser' aufgerufen, aber Browser war nicht offen.");
        // Auch wenn der Browser nicht existiert, stellen wir sicher, dass Cursor/HUD zurückgesetzt sind
       
        
    }
});

// Tastendruck-Handler für die "E"-Taste
mp.keys.bind(0x45, true, () => { // E-Taste (0x45 ist der Keycode für 'E')
    // Zusätzliche Prüfungen, um Konflikte zu vermeiden (z.B. wenn Chat offen ist)
    if (mp.gui.chat.enabled || mp.gui.cursor.visible) {
        console.log("[CLIENT_SIDE] E-Taste gedrückt, aber Chat/Cursor aktiv. Aktion blockiert.");
        return;
    }

    // Wenn der Browser NICHT offen ist, sende Anfrage an den Server
    if (!isGarageOpen) { // Prüfe unser eigenes Flag
        console.log("[CLIENT_SIDE] E-Taste gedrückt. Sende RequestOpenUI an Server.");
        mp.events.callRemote("Client:Garage:RequestOpenUI");
    } else {
        // Wenn der Browser bereits offen ist, schließe ihn bei erneutem Drücken der E-Taste
        console.log("[CLIENT_SIDE] E-Taste gedrückt, Browser ist offen. Sende RequestCloseUI an Server.");
        // Löse das Schließ-Event über den Server aus, damit die Logik zentral bleibt
        mp.events.callRemote("Server:Garage:RequestCloseUI");
    }
});

// --- WICHTIG: Event-Listener für Events, die VOM BROWSER (CEF) kommen ---
// Diese Events werden von mp.trigger() im Browser ausgelöst und hier empfangen.
// Von hier leiten wir sie dann mit mp.events.callRemote() an den C#-Server weiter.

// Listener für das Spawnen eines Fahrzeugs aus dem UI
mp.events.add("Server:Garage:SpawnVehicle", (vehSafeId) => {
    console.log(`[CLIENT_SIDE] Empfangen von Browser 'Server:Garage:SpawnVehicle' für ID: ${vehSafeId}. Leite an Server weiter.`);
    mp.events.callRemote("Server:Garage:SpawnVehicle", vehSafeId);
});

// Listener für das Einlagern eines Fahrzeugs aus dem UI
mp.events.add("Server:Garage:StoreVehicle", (vehSafeId) => { // NEU: vehSafeId hier empfangen
    console.log(`[CLIENT_SIDE] Empfangen von Browser 'Server:Garage:StoreVehicle' für ID: ${vehSafeId}. Leite an Server weiter.`);
    mp.events.callRemote("Server:Garage:StoreVehicle", vehSafeId); // NEU: vehSafeId weiterleiten
});

// Listener für die Anfrage zum Schließen des UI aus dem Browser (z.B. durch den "X"-Button)
mp.events.add("Server:Garage:RequestCloseUI", () => {
    console.log(`[CLIENT_SIDE] Empfangen von Browser 'Server:Garage:RequestCloseUI'. Leite an Server weiter.`);
    // Der Server verarbeitet dieses Event und triggert dann "Client:Garage:HideGarageBrowser" zurück.
    // Dies stellt sicher, dass die Schließlogik (Cursor, HUD, etc.) immer über das zentrale Hide-Event läuft.
    mp.events.callRemote("Server:Garage:RequestCloseUI");
});

console.log('client_packages/garage/index.js geladen.');