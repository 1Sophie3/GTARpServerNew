const tachoBrowser = mp.browsers.new('package://veh/tachoindex.html'); // Pfad ggf. anpassen
let isTachoActive = false;

setInterval(() => {
    const isPlayerDriving = mp.players.local.vehicle && mp.players.local.vehicle.getPedInSeat(-1) === mp.players.local.handle;

    if (isPlayerDriving && !isTachoActive) {
        isTachoActive = true;
        tachoBrowser.active = true;
        tachoBrowser.execute(`showTacho(true);`);
    } else if (!isPlayerDriving && isTachoActive) {
        isTachoActive = false;
        tachoBrowser.active = false;
        tachoBrowser.execute(`showTacho(false);`);
    }
}, 1000);

// Empfängt das komplette Datenpaket vom Server
mp.events.add('Tacho:Update', (payloadJson) => {
    if (isTachoActive) {
        // payloadJson ist ein String, wir müssen ihn nicht parsen, da er direkt in die Funktion geht
        tachoBrowser.execute(`updateUI(${payloadJson});`);
    }
});