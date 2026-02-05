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
    
    // HUD laden
    loadHUD(playerData);
    
    console.log('[CLIENT] Login erfolgreich');
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

mp.events.add('cef:register', (username, email, password) => {
    // An Server senden
    mp.events.callRemote('server:register', username, email, password);
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

// Spieler Tod
mp.events.add('playerDeath', (player, reason, killer) => {
    mp.game.graphics.startScreenEffect('DeathFailOut', 0, true);
    
    setTimeout(() => {
        mp.game.graphics.stopScreenEffect('DeathFailOut');
    }, 5000);
});

console.log('[CLIENT] RP-Client erfolgreich geladen!');
