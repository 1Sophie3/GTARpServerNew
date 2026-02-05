// Client-Side JavaScript für RAGE:MP

// Browser (CEF) Instanz
let loginBrowser = null;
let hudBrowser = null;

// Event: Zeige Login UI
mp.events.add('client:showLoginUI', () => {
    if (loginBrowser) return;

    loginBrowser = mp.browsers.new('package://rp-ui/login.html');
    mp.gui.cursor.visible = true;
    mp.gui.chat.show(false);

    console.log('[CLIENT] Login UI geöffnet');
});

// Event: Login erfolgreich
mp.events.add('client:loginSuccess', (playerData) => {
    if (loginBrowser) {
        loginBrowser.destroy();
        loginBrowser = null;
    }

    mp.gui.cursor.visible = false;
    mp.gui.chat.show(true);

    // JSON-String parsen falls nötig
    let pd = playerData;
    try { pd = (typeof playerData === 'string') ? JSON.parse(playerData) : playerData; } catch (e) { }

    // HUD laden
    loadHUD(pd);

    console.log('[CLIENT] Login erfolgreich');
});

// Resultate von Registrierung anzeigen (CEFs SetWarning/SetSuccess aufrufen)
mp.events.add('client:registerResult', (success, message) => {
    if (!loginBrowser) return;
    const fn = success ? 'SetSuccess' : 'SetWarning';
    loginBrowser.execute(`${fn}(${JSON.stringify(message)})`);
});

mp.events.add('client:loginResult', (success, message) => {
    if (!loginBrowser) return;
    const fn = success ? 'SetSuccess' : 'SetWarning';
    loginBrowser.execute(`${fn}(${JSON.stringify(message)})`);
});

// HUD laden
function loadHUD(playerData) {
    if (hudBrowser) return;

    hudBrowser = mp.browsers.new('package://rp-ui/hud.html');

    // Spielerdaten an HUD senden
    hudBrowser.execute(`updatePlayerData(${JSON.stringify(playerData)})`);

    console.log('[CLIENT] HUD geladen');
}

// Event: HUD Update
mp.events.add('client:updateHUD', (data) => {
    if (hudBrowser) {
        hudBrowser.execute(`updateHUD(${JSON.stringify(data)})`);
    }
});

// CEF -> Client Events
mp.events.add('cef:login', (username, password) => {
    // An Server senden
    mp.events.callRemote('server:login', username, password);
});

mp.events.add('cef:register', (username, password) => {
    // An Server senden (Vor- und Nachname werden serverseitig gesetzt)
    mp.events.callRemote('server:register', username, password);
});

// CEF -> Client: Inventory öffnen / Transfer
mp.events.add('cef:inventoryOpen', (category, targetId) => {
    mp.events.callRemote('server:inventoryOpen', category, targetId || '');
});

// CEF asks to close inventory UI
mp.events.add('cef:inventoryClose', () => {
    if (inventoryBrowser) {
        inventoryBrowser.destroy();
        inventoryBrowser = null;
        mp.gui.cursor.visible = false;
    }
});

mp.events.add('cef:inventoryTransfer', (category, fromInvId, toType, toId, slotIdx, amount) => {
    mp.events.callRemote('server:inventoryTransfer', category, fromInvId, toType, toId, slotIdx, amount);
});

// Server -> Client: Inventory-Update empfangen und an CEF weiterreichen
let inventoryBrowser = null;
mp.events.add('client:updateInventory', (inventoryJson) => {
    try {
        const data = (typeof inventoryJson === 'string') ? JSON.parse(inventoryJson) : inventoryJson;
        if (!inventoryBrowser) {
            inventoryBrowser = mp.browsers.new('package://rp-ui/inventory.html');
            mp.gui.cursor.visible = true;
        }
        // Rufe ein Event in der CEF auf
        inventoryBrowser.execute(`window.dispatchEvent(new CustomEvent('updateInventory',{detail: ${JSON.stringify(data)}}))`);
    } catch (e) {
        console.error('Failed to forward inventory update to CEF', e);
    }
});

mp.events.add('client:inventoryTransferResult', (success, message) => {
    if (inventoryBrowser) {
        inventoryBrowser.execute(`window.dispatchEvent(new CustomEvent('inventoryTransferResult',{detail: ${JSON.stringify({ success, message })}}))`);
    }
});

// Tastatur Events
mp.keys.bind(0x75, true, () => { // F6 - Spieler Menü
    if (hudBrowser) {
        hudBrowser.execute('togglePlayerMenu()');
        mp.gui.cursor.visible = !mp.gui.cursor.visible;
    }
});

mp.keys.bind(0x74, true, () => { // F5 - Inventar
    if (hudBrowser) {
        hudBrowser.execute('toggleInventory()');
        mp.gui.cursor.visible = !mp.gui.cursor.visible;
    }
});

// Taste 'I' öffnen Inventar im Spiel (0x49 = 'I')
mp.keys.bind(0x49, true, () => {
    // Öffne Spieler-Inventar
    mp.events.callRemote('server:inventoryOpen', 'player', '');
});

// Spieler Tod
mp.events.add('playerDeath', (player, reason, killer) => {
    mp.game.graphics.startScreenEffect('DeathFailOut', 0, true);

    setTimeout(() => {
        mp.game.graphics.stopScreenEffect('DeathFailOut');
    }, 5000);
});

console.log('[CLIENT] RP-Client erfolgreich geladen!');
