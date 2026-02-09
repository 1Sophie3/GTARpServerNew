// Kamera-Einstellungen
let fov_max = 80.0;
let fov_min = 10.0;
let zoom_speed = 2.0;
let pan_speed = 3.0;
let helicam_active = false; // Kamera ein-/ausgeschaltet
let fov = (fov_max + fov_min) / 2;
let vision_mode = 0; // Vision-Modus: 0 = Normal, 1 = Nacht, 2 = Thermal
let camera = null;
let scaleform = null;

// Hilfsfunktion: Überprüfen, ob das Fahrzeug ein Polizei-Maverick ist
const isPoliceMaverick = (vehicle) => {
    const policeMaverickHash = mp.game.joaat("polmav");
    return vehicle && vehicle.model === policeMaverickHash;
};

// Command zum Umschalten der Kamera
mp.events.addCommand("helicam", () => {
    const player = mp.players.local;

    // Debugging: Command wurde erkannt
    mp.gui.chat.push("~o~Command /helicam wurde ausgeführt!");

    if (helicam_active) {
        // Kamera deaktivieren
        if (camera) {
            camera.destroy(true);
            camera = null;
        }
        if (scaleform) {
            mp.game.graphics.setScaleformMovieAsNoLongerNeeded(scaleform);
        }
        mp.game.cam.renderScriptCams(false, false, 0, true, false);
        mp.game.graphics.setSeethrough(false);
        mp.game.graphics.setNightvision(false);
        vision_mode = 0;
        helicam_active = false;
        mp.gui.chat.push("~r~Helikopterkamera deaktiviert.");
    } else {
        const vehicle = player.vehicle;

        // Debugging: Check Fahrzeug
        if (!vehicle) {
            mp.gui.chat.push("~r~Du bist in keinem Fahrzeug!");
            return;
        }

        // Debugging: Check Fahrzeugtyp
        if (!isPoliceMaverick(vehicle)) {
            mp.gui.chat.push("~r~Die Kamera kann nur im Polizei-Maverick verwendet werden.");
            return;
        }

        // Debugging: Check Sitzposition
        if (vehicle.getPedInSeat(1) !== player.handle) {
            mp.gui.chat.push("~r~Nur der Beifahrer kann die Kamera steuern.");
            return;
        }

        // Kamera aktivieren
        scaleform = mp.game.graphics.requestScaleformMovie("HELI_CAM");
        while (!mp.game.graphics.hasScaleformMovieLoaded(scaleform))
            mp.game.wait(0);

        camera = mp.cameras.new(
            "DEFAULT_SCRIPTED_FLY_CAMERA",
            player.position,
            new mp.Vector3(0, 0, 0),
            60
        );

        camera.attachTo(vehicle.handle, 0, 0, -1.5, true);
        camera.setActive(true);
        mp.game.cam.renderScriptCams(true, false, 0, true, false);
        helicam_active = true;
        mp.gui.chat.push("~g~Helikopterkamera aktiviert.");
    }
});

// Vision-Modus umschalten
mp.events.addCommand("vision", () => {
    if (!helicam_active) {
        mp.gui.chat.push("~r~Die Kamera ist nicht aktiv!");
        return;
    }

    vision_mode = (vision_mode + 1) % 3;

    if (vision_mode === 0) {
        mp.game.graphics.setNightvision(false);
        mp.game.graphics.setSeethrough(false);
        mp.gui.chat.push("~g~Vision-Modus: Normal");
    } else if (vision_mode === 1) {
        mp.game.graphics.setNightvision(true);
        mp.game.graphics.setSeethrough(false);
        mp.gui.chat.push("~g~Vision-Modus: Nachtmodus");
    } else if (vision_mode === 2) {
        mp.game.graphics.setSeethrough(true);
        mp.gui.chat.push("~g~Vision-Modus: Thermalmodus");
    }
});

// Kamera Steuerung und Logik
const handleHeliCam = () => {
    if (!helicam_active || !camera) return;

    mp.game.controls.disableAllControlActions(2); // Deaktivieren aller Eingaben

    const panX = mp.game.controls.getDisabledControlNormal(0, 1) * pan_speed;
    const panY = mp.game.controls.getDisabledControlNormal(0, 2) * pan_speed;
    const currentRotation = camera.getRot(2);
    camera.setRot(currentRotation.x - panY, 0, currentRotation.z - panX, 2);

    const zoomIn = mp.game.controls.getDisabledControlNormal(2, 40) * zoom_speed;
    const zoomOut = mp.game.controls.getDisabledControlNormal(2, 41) * zoom_speed;

    fov -= zoomIn - zoomOut;
    if (fov < fov_min) fov = fov_min;
    if (fov > fov_max) fov = fov_max;
    camera.setFov(fov);

    // Anzeige
    mp.game.graphics.pushScaleformMovieFunction(scaleform, "SET_ALT_FOV_HEADING");
    mp.game.graphics.pushScaleformMovieFunctionParameterFloat(mp.players.local.vehicle.position.z); // Höhe
    mp.game.graphics.pushScaleformMovieFunctionParameterFloat(fov); // Sichtfeld
    mp.game.graphics.pushScaleformMovieFunctionParameterFloat(camera.getRot(2).z); // Richtung
    mp.game.graphics.popScaleformMovieFunctionVoid();

    mp.game.graphics.drawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, false);
};

// Ereignis: Helikopterkamera steuern
mp.events.add("render", () => {
    if (helicam_active) {
        handleHeliCam();
    }
});
