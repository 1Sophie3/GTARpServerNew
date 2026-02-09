let wheelCEF = null;
let isWheelActive = false; // Ein simpler Schalter, der uns sagt, ob das Menü gerade aktiv ist
let hoveredAction = null;
let currentMenuItems = [];

// Hilfsfunktion, um das nächstgelegene Fahrzeug zu finden
function getTargetVehicle() {
    const player = mp.players.local;
    if (player.vehicle) {
        return player.vehicle;
    }

    let closestVehicle = null;
    let minDistance = 5.0; // Maximale Reichweite, um ein Fahrzeug zu finden
    mp.vehicles.forEachInRange(player.position, minDistance, (v) => {
        const distance = mp.game.system.vdist(player.position.x, player.position.y, player.position.z, v.position.x, v.position.y, v.position.z);
        if (distance < minDistance) {
            minDistance = distance;
            closestVehicle = v;
        }
    });
    return closestVehicle;
}

// Funktion zum Schließen des Menüs (wird jetzt von mehreren Stellen aufgerufen)
function hideWheelMenu() {
    if (wheelCEF && isWheelActive) {
        wheelCEF.active = false;
        isWheelActive = false;
        mp.gui.cursor.show(false, false);
        hoveredAction = null;
    }
}

// =======================================================================
// NEUE, ROBUSTE KONTROLL-SCHLEIFE IM RENDER-EVENT
// =======================================================================
mp.events.add('render', () => {
    const isXKeyDown = mp.keys.isDown(0x58); // Prüft, ob 'X' (Keycode 88) GEDRÜCKT GEHALTEN wird
    const targetVehicle = getTargetVehicle();

    // FALL 1: 'X' wird gedrückt, ein Fahrzeug ist in Reichweite und das Menü ist noch NICHT aktiv
    if (isXKeyDown && targetVehicle && !isWheelActive) {
        isWheelActive = true; // Zustand auf "aktiv" setzen, um doppeltes Öffnen zu verhindern
        mp.events.callRemote('requestVehicleWheelMenu', targetVehicle);
    }
    // FALL 2: 'X' wird NICHT mehr gedrückt (oder es gibt kein Fahrzeug mehr) und das Menü ist noch aktiv
    else if ((!isXKeyDown || !targetVehicle) && isWheelActive) {
        // Logik zum Auswählen einer Option beim Loslassen der Taste
        const actionItem = hoveredAction ? currentMenuItems.find(item => item.Action === hoveredAction) : null;
        if (hoveredAction && actionItem && !actionItem.RequiresClick) {
            mp.events.call('clientVehicleWheelAction', hoveredAction);
        }
        // Schließe das Menü in jedem Fall
        hideWheelMenu();
    }
});

// Event vom Server, um das Menü mit den richtigen Optionen zu füllen
mp.events.add('showVehicleWheelMenu', (menuItemsJson, centerText) => {
    // Nur anzeigen, wenn der 'render'-Loop es erlaubt hat
    if (!isWheelActive) return;

    if (wheelCEF === null) {
        wheelCEF = mp.browsers.new('package://veh/vehicleWheel.html'); // Pfad an deine Struktur angepasst
    }
    
    currentMenuItems = JSON.parse(menuItemsJson);
    wheelCEF.execute(`populateWheel(${menuItemsJson}, '${centerText}');`);
    
    wheelCEF.active = true;
    mp.gui.cursor.show(true, true);
});

// Diese Events werden direkt vom CEF-Browser (aus der HTML) aufgerufen
mp.events.add('clientVehicleWheelSetHover', (action) => {
    hoveredAction = action;
});

mp.events.add('clientVehicleWheelAction', (action) => {
    if (!isWheelActive) return;
    const targetVehicle = getTargetVehicle();
    if (targetVehicle) {
        mp.events.callRemote('vehicleWheelMenuAction', targetVehicle, action);
    }
    hideWheelMenu();
});

// Escape-Taste zum Schließen
mp.keys.bind(0x1B, true, () => { 
    if (isWheelActive) {
        hideWheelMenu();
    }
});