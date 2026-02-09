// #############################################################################
// #                                                                           #
// #      CLIENT-SKRIPT FÜR DAS HAUSSYSTEM (V16.0 - IPL-Management Fix)        #
// #      Behebt das Problem, dass IPLs nicht entladen werden.                 #
// #                                                                           #
// #############################################################################

let houseCef = null;
let inputCef = null;
let currentHousePrompt = null;
let lastHouseData = null; 
let currentInputCallback = null;

// NEU: Variable, um die aktuell geladene IPL zu speichern
let currentLoadedIpl = null; 

// ##### HILFSFUNKTIONEN #####
function closeAllCef() {
    if (houseCef) {
        houseCef.destroy();
        houseCef = null;
    }
    if (inputCef) {
        inputCef.destroy();
        inputCef = null;
    }
    mp.gui.cursor.show(false, false);
    lastHouseData = null; 
    currentInputCallback = null;
}

function openInputCef(title, callbackEventName) {
    if (inputCef || houseCef) return;
    
    currentInputCallback = callbackEventName;

    inputCef = mp.browsers.new('package://housingsystem/house.html');
    mp.gui.cursor.show(true, true);
    setTimeout(() => { if (inputCef) inputCef.execute(`setTitle('${title}');`); }, 500);
}


// ##### EVENTS VOM CEF (BENUTZEROBERFLÄCHE) #####
mp.events.add('CEF:House:Close', closeAllCef);

mp.events.add('CEF:House:Action', (action, data) => {
    switch (action) {
        case 'manage':
            if (houseCef && lastHouseData) {
                houseCef.execute(`handleMenuAction('show_management', ${JSON.stringify(lastHouseData)});`);
            }
            return; 

        case 'back':
            if (houseCef && lastHouseData) {
                houseCef.execute(`handleMenuAction('show_main', ${JSON.stringify(lastHouseData)});`);
            }
            return; 

        case 'removeKey':
            mp.events.callRemote('Server:House:RemoveKey', data);
            return;

        case 'giveKey':
            closeAllCef(); 
            openInputCef("Account-ID des Spielers", "CEF:House:GiveKeyToPlayer");
            return; 

        // --- Aktionen, die das Menü schließen sollen (kein 'return') ---
        case 'buy': mp.events.callRemote('Server:House:Buy'); break;
        case 'rent_1': mp.events.callRemote('Server:House:Rent', 1); break;
        case 'rent_7': mp.events.callRemote('Server:House:Rent', 7); break;
        case 'rent_30': mp.events.callRemote('Server:House:Rent', 30); break;
        case 'enter': mp.events.callRemote('Server:House:Enter'); break;
        case 'toggleLock': mp.events.callRemote('Server:House:ToggleLock'); break;
        case 'changeLocks': mp.events.callRemote('Server:House:ChangeLocks'); break;
        default: break;
    }
    closeAllCef();
});

// Callback für die Schlüsselübergabe
mp.events.add("CEF:House:GiveKeyToPlayer", (targetIdStr) => {
    const targetId = parseInt(targetIdStr);
    if (!isNaN(targetId) && targetId > 0) mp.events.callRemote('Server:House:GiveKey', targetId);
});

// Permanenter Listener für das Eingabefenster
mp.events.add('Client:Cef:InputSubmit', (value) => {
    if (currentInputCallback) {
        const callbackName = currentInputCallback;
        const didSubmit = (value !== null && value !== undefined);
        
        if (didSubmit) {
            mp.events.call(callbackName, value);
        }
    }
    closeAllCef();
});


// ##### EVENTS VOM SERVER #####
mp.events.add("Client:House:ShowInteractionPrompt", (houseDataJson) => {
    currentHousePrompt = JSON.parse(houseDataJson);
});

// MODIFIZIERT: Event-Listener zum Laden von Interiors
mp.events.add("Client:RequestIpl", (ipl) => {
    // 1. Wenn bereits eine IPL geladen ist, entferne sie zuerst.
    if (currentLoadedIpl) {
        mp.game.streaming.removeIpl(currentLoadedIpl);
    }
    
    // 2. Lade die neue IPL.
    mp.game.streaming.requestIpl(ipl);
    
    // 3. Merke dir die neue IPL als die aktuell geladene.
    currentLoadedIpl = ipl;
});

// NEU: Event-Listener zum Entladen einer IPL, wenn man ein Haus verlässt
mp.events.add("Client:House:UnloadIpl", () => {
    if (currentLoadedIpl) {
        mp.game.streaming.removeIpl(currentLoadedIpl);
        currentLoadedIpl = null; // Zurücksetzen, da wir jetzt in der Außenwelt sind
    }
});

mp.events.add("Client:House:HideInteractionPrompt", () => {
    currentHousePrompt = null;
    closeAllCef();
});

mp.events.add("Client:House:ForceMenuClose", closeAllCef);

mp.events.add("Client:House:OpenMenu", (houseDataJson) => {
    if (houseCef || inputCef) return;
    lastHouseData = JSON.parse(houseDataJson); 
    houseCef = mp.browsers.new('package://housingsystem/house_ui.html');
    mp.gui.cursor.show(true, true);
    setTimeout(() => {
        if (houseCef) houseCef.execute(`handleMenuAction('show_main', ${JSON.stringify(lastHouseData)});`);
    }, 500);
});


// ##### TASTENABFRAGE #####
mp.keys.bind(0x45, true, function() { // E-Taste
    if (mp.gui.chat.enabled || mp.gui.cursor.visible || houseCef || inputCef) return;

    if (currentHousePrompt) {
        if (!currentHousePrompt.isLocked && !currentHousePrompt.isOwner && !currentHousePrompt.isRenter && !currentHousePrompt.hasKey) {
            mp.events.callRemote('Server:House:Enter');
        } else {
            mp.events.callRemote('Server:House:RequestMenuData');
        }
    } else if (mp.players.local.dimension !== 0) {
        mp.events.callRemote('Server:House:Exit');
    }
});