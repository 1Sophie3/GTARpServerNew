const localPlayer = mp.players.local;
const maxSpeed = 10.0;
const minHeight = 15.0;
const maxHeight = 45.0;
const maxAngle = 15.0;

mp.events.add("client_abseilen", () => {
    const vehicle = localPlayer.vehicle;

    // Überprüfen, ob der Spieler in einem Fahrzeug ist
    if (!vehicle) {
        mp.gui.chat.push("~r~Du bist in keinem Fahrzeug!");
        return;
    }

    // Debugging: Aktuelle Höhe ausgeben
    const curHeight = vehicle.getHeightAboveGround();
    mp.gui.chat.push(`~o~Aktuelle Höhe: ${curHeight}`);

    // Höhenüberprüfung
    if (curHeight < minHeight || curHeight > maxHeight) {
        mp.gui.chat.push("~r~Das Fahrzeug ist zu tief oder zu hoch.");
        return;
    }

    // Überprüfen, ob das Fahrzeug aufrecht steht
    if (!vehicle.isUpright(maxAngle) || vehicle.isUpsidedown()) {
        mp.gui.chat.push("~r~Halte den Helikopter ruhig!");
        return;
    }

    // Geschwindigkeit überprüfen
    if (vehicle.getSpeed() > maxSpeed) {
        mp.gui.chat.push("~r~Ihr seid zu schnell!");
        return;
    }

    // Tasks des Spielers zurücksetzen (falls blockiert)
    localPlayer.clearTasksImmediately();

    // Überprüfen, ob der Spieler bereits abseilt
    const taskStatus = localPlayer.getScriptTaskStatus(-275944640);
    mp.gui.chat.push(`~o~Abseil-Task-Status: ${taskStatus}`); // Debugging
    if (taskStatus === 0 || taskStatus === 1) {
        mp.gui.chat.push("~r~Du seilst dich bereits ab.");
        return;
    }

    // Abseilen starten
    localPlayer.taskRappelFromHeli(10.0);
    mp.gui.chat.push("~g~Du seilst dich jetzt ab.");
});
