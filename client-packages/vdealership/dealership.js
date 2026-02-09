let dealershipBrowser = null;
let currentVehicleInfo = null;

// KORREKTUR: Die Funktionalität zum Schließen des Menüs mit der physischen 'X'-Taste wurde entfernt.
// Das Schließen funktioniert jetzt nur noch über den 'X'-Button im Menü oder durch Aussteigen.

// Event vom Server, um das Menü zu öffnen.
mp.events.add('client:dealership:showMenu', (vehicleInfoJson) => {
    if (dealershipBrowser) {
        dealershipBrowser.destroy();
    }
    
    currentVehicleInfo = JSON.parse(vehicleInfoJson);

    // WICHTIG: Der Pfad muss zu deiner HTML-Datei im client_packages-Ordner passen.
    dealershipBrowser = mp.browsers.new('package://vdealership/index.html');
    mp.gui.cursor.show(true, true);

    setTimeout(() => {
        if (dealershipBrowser) {
            dealershipBrowser.execute(`updateMenu(${JSON.stringify(currentVehicleInfo)});`);
        }
    }, 500);
});

// Diese Funktion wird jetzt vom CEF-Button oder vom Server (beim Aussteigen) aufgerufen.
function closeMenu() {
    if (dealershipBrowser) {
        dealershipBrowser.destroy();
        dealershipBrowser = null;
    }
    mp.gui.cursor.show(false, false);
    currentVehicleInfo = null;
}

// Event vom Server (z.B. beim Aussteigen) oder vom CEF-Button, um das Menü zu schließen.
mp.events.add('client:dealership:closeMenu', closeMenu);

// Event, das vom CEF-Browser aufgerufen wird, wenn ein Kauf-Button geklickt wird.
mp.events.add('client:dealership:buy', (forFaction) => {
    if (currentVehicleInfo && currentVehicleInfo.Model) {
        mp.events.callRemote('server:dealership:buyVehicle', currentVehicleInfo.Model, forFaction);
    }
});