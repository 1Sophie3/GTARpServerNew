// garagescript.js
// Dies ist das JavaScript, das im RAGE:MP CEF-Browserfenster läuft.

console.log('garagescript.js wird geladen.'); // Debug-Log, sollte in localhost:9222 Konsole erscheinen

// Funktion zum Anzeigen/Verbergen von Tabs
function openTab(evt, tabName) {
    console.log(`openTab aufgerufen für ${tabName}.`);
    let i, tabcontent, tabbuttons;

    // Alle Tab-Inhalte ausblenden
    tabcontent = document.getElementsByClassName("tab-content");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }

    // Alle Tab-Buttons auf inaktiv setzen
    tabbuttons = document.getElementsByClassName("tab-button");
    for (i = 0; i < tabbuttons.length; i++) {
        tabbuttons[i].className = tabbuttons[i].className.replace(" active", "");
    }

    // Den ausgewählten Tab-Inhalt anzeigen
    document.getElementById(tabName).style.display = "block";

    // Den aktiven Tab-Button markieren
    if (evt && evt.currentTarget) { // Prüfen, ob ein Event-Objekt und ein currentTarget existieren
        evt.currentTarget.className += " active";
        console.log('Tab-Button über Event aktiviert.');
    } else {
        // Wenn openTab ohne Event aufgerufen wird (z.B. initial beim Laden),
        // den passenden Button über seinen onclick-Handler finden und aktivieren.
        const defaultButton = document.querySelector(`.tab-button[onclick*="${tabName}"]`);
        if (defaultButton) {
            defaultButton.classList.add('active');
            console.log('Tab-Button über QuerySelector aktiviert (initialer Aufruf).');
        }
    }
}

// Funktion, die vom Server aufgerufen wird, um Garagendaten anzuzeigen
function showGarageData(inGarageVehicles, nearbyParkableVehicles, maxGarageVehicles) {
    console.log('showGarageData aufgerufen. Empfangene Daten verarbeiten.');
    const inGarageList = document.getElementById('inGarageVehiclesList');
    const currentVehicleInfoDiv = document.getElementById('currentVehicleInfo'); // Das Element für den Einlagerungs-Tab

    inGarageList.innerHTML = ''; // Vorherige Einträge löschen
    currentVehicleInfoDiv.innerHTML = ''; // Vorherige Einträge löschen

    // Fahrzeuge in der Garage (zum Spawnen) anzeigen
    if (inGarageVehicles.length === 0) {
        inGarageList.innerHTML = '<p style="text-align: center; color: #ccc; padding: 20px;">Du hast keine Fahrzeuge in dieser Garage.</p>';
    } else {
        inGarageVehicles.forEach(veh => {
            const vehicleItem = document.createElement('div');
            vehicleItem.classList.add('vehicle-item');
            vehicleItem.innerHTML = `
                <div class="vehicle-details">
                    <p class="model-name">${veh.ModelName}</p>
                    <p class="plate">Kennzeichen: ${veh.NumberPlate}</p>
                    <p>Gesundheit: ${Math.round(veh.Health / 10)}%</p>
                </div>
                <div class="vehicle-actions">
                    <button onclick="requestSpawnVehicle(${veh.Id})">Spawnen</button>
                </div>
            `; // KORREKTUR: Die Gesundheitsanzeige
            inGarageList.appendChild(vehicleItem);
        });
    }

    // Kapazitätsanzeige aktualisieren
    document.getElementById('currentCapacity').textContent = inGarageVehicles.length;
    document.getElementById('maxCapacity').textContent = maxGarageVehicles;


    // Parkbare Fahrzeuge im Umkreis (zum Einlagern) anzeigen
    if (nearbyParkableVehicles.length === 0) {
        currentVehicleInfoDiv.innerHTML = '<p style="text-align: center; color: #ccc; padding: 20px;">Keine einlagerbaren Fahrzeuge in deiner Nähe.</p>';
    } else {
        nearbyParkableVehicles.forEach(veh => {
            const vehicleItem = document.createElement('div');
            vehicleItem.classList.add('vehicle-item');
            vehicleItem.innerHTML = `
                <div class="vehicle-details">
                    <p class="model-name">${veh.ModelName}</p>
                    <p class="plate">Kennzeichen: ${veh.NumberPlate}</p>
                    <p>Zustand: ${Math.round(veh.Health / 10)}%</p>
                </div>
                <div class="vehicle-actions">
                    <button class="red-button" onclick="requestStoreVehicle(${veh.Id})">Einlagern</button>
                </div>
            `;
            currentVehicleInfoDiv.appendChild(vehicleItem);
        });
    }

    // Stelle sicher, dass der Browser sichtbar ist
    document.querySelector('.garage-container').style.display = 'block';
    console.log('Garage UI sollte jetzt sichtbar sein.');
}


// Funktion zum Schließen des Garagen-UI
function closeGarageUI() {
    console.log('closeGarageUI aufgerufen. Sende Event an Server.');
    if (typeof mp !== 'undefined' && mp.trigger) {
        mp.trigger('Server:Garage:RequestCloseUI');
    }
    document.querySelector('.garage-container').style.display = 'none'; // UI lokal ausblenden
}

// Funktionen zum Senden von Anfragen an den Server
function requestSpawnVehicle(vehSafeId) {
    console.log(`Anfrage zum Spawnen von Fahrzeug ID: ${vehSafeId}`);
    if (typeof mp !== 'undefined' && mp.trigger) {
        mp.trigger('Server:Garage:SpawnVehicle', vehSafeId);
    }
}

function requestStoreVehicle(vehSafeId) {
    console.log(`Anfrage zum Einlagern von Fahrzeug ID: ${vehSafeId}`);
    if (typeof mp !== 'undefined' && mp.trigger) {
        mp.trigger('Server:Garage:StoreVehicle', vehSafeId); // <--- HIER WIRD vehSafeId GESENDET!
    }
}

// Initialer Aufruf beim Laden der Seite, um den ersten Tab zu aktivieren
document.addEventListener('DOMContentLoaded', () => {
    openTab(null, 'spawnTab'); // null, da kein Event-Objekt beim initialen Laden existiert
});

// Debug-Log, um zu bestätigen, dass das Skript läuft
console.log('garagescript.js geladen und bereit.');