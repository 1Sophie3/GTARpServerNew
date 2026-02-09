const speedoBrowser = mp.browsers.new("package://speedometer/CEF/speedometer.html");
const player = mp.players.local;
let isTachoVisible = false;

// Eine Hilfsvariable, damit der Chat nicht zugespammt wird
let lastMessageTime = 0;

setInterval(() => {
    const vehicle = player.vehicle;
    
    if (vehicle && mp.vehicles.exists(vehicle) && vehicle.getPedInSeat(-1) === player.handle) {
        if (!isTachoVisible) {
            speedoBrowser.execute("showSpeedo();");
            isTachoVisible = true;
        }

        const speed = vehicle.getSpeed() * 3.6;
        const rpm = vehicle.rpm * 1000;
        
        const fuel = vehicle.getVariable('Vehicle_Fuel') || 0;
const mileage = vehicle.getVariable('Vehicle_Mileage') || 0;
        
        // Der Tacho wird weiterhin mit den Daten versorgt
        speedoBrowser.execute(`update(${speed}, ${rpm}, ${fuel}, ${mileage});`);

        // ======================= DEBUG-ZEILE =======================
        // Diese Zeile schreibt die empfangenen Werte alle 2 Sekunden in den Chat.
        const currentTime = Date.now();
        if (currentTime - lastMessageTime > 2000) {
           
            lastMessageTime = currentTime;
        }
        // =========================================================

    } else {
        if (isTachoVisible) {
            speedoBrowser.execute("hideSpeedo();");
            isTachoVisible = false;
        }
    }
}, 200);