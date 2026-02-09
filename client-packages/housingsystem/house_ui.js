// Globale UI-Elemente
const houseMenu = document.getElementById('house-menu');
const houseNameElem = document.getElementById('house-name');
const houseInfoElem = document.getElementById('house-info');
const menuOptionsElem = document.getElementById('menu-options');
const closeBtn = document.getElementById('closeBtn');

// Funktion zum Leeren der Button-Liste
function clearOptions() {
    menuOptionsElem.innerHTML = '';
}

// Funktion zum Erstellen eines Buttons
function createButton(text, action, cssClass = '', data = null) {
    const button = document.createElement('button');
    button.innerText = text;
    button.className = `menu-button ${cssClass}`;
    button.onclick = () => {
        if (typeof mp !== 'undefined') {
            mp.trigger('CEF:House:Action', action, data);
        }
    };
    menuOptionsElem.appendChild(button);
}

// Hauptfunktion, die das initiale Menü aufbaut
function showHouseMenu(data) {
    clearOptions();
    houseNameElem.innerText = data.name;

    // Szenario 1: Haus steht zum Verkauf
    if (data.isForSale) {
        houseInfoElem.innerHTML = `<p>Preis: <span>$${data.price.toLocaleString()}</span></p>`;
        createButton('Haus kaufen', 'buy');
    }
    // Szenario 2: Mietobjekt ist frei
    else if (data.isRentable && !data.isRenter) {
        houseInfoElem.innerHTML = `<p>Miete: <span>$${data.rentPrice.toLocaleString()} / Tag</span></p>`;
        createButton('Für 1 Tag mieten', 'rent_1');
        createButton('Für 7 Tage mieten', 'rent_7');
        createButton('Für 30 Tage mieten', 'rent_30');
    }
    // Szenario 3: Spieler ist Besitzer, Mieter oder hat Schlüssel
    else {
        let ownerText = data.isRentable ? `Mieter: <span>${data.renterName}</span>` : `Besitzer: <span>${data.ownerName}</span>`;
        houseInfoElem.innerHTML = `<p>${ownerText}</p>`;
        
        if (data.isOwner || data.isRenter || data.hasKey) {
            createButton('Haus betreten', 'enter');
        }
        
        if (data.isOwner || data.isRenter) {
            const lockStateText = data.isLocked ? "Aufschließen" : "Abschließen";
            createButton(lockStateText, 'toggleLock');
        }

        // NEU: Verwaltungsoptionen nur für den Besitzer (nicht für Mieter)
        if (data.isOwner) {
            createButton('Immobilie verwalten', 'manage');
        }
    }

    houseMenu.style.display = 'block';
}

// NEU: Funktion, die das Verwaltungs-Menü anzeigt
function showManagementMenu(data) {
    clearOptions();
    houseNameElem.innerText = "Immobilienverwaltung";
    houseInfoElem.innerHTML = `<p>Verwalte deine Schlüssel und Schlösser.</p>`;

    // Button zum Übergeben eines Schlüssels
    createButton('Schlüssel übergeben (2.500$)', 'giveKey');
    
    // Button zum Wechseln der Schlösser
    createButton('Schlösser wechseln (25.000$)', 'changeLocks', 'sell');

    // Liste der Spieler, die einen Schlüssel haben
    if (data.KeyHolders && data.KeyHolders.length > 0) {
        houseInfoElem.innerHTML += `<p style="margin-top:15px; font-weight:bold;">Schlüsselbesitzer:</p>`;
        data.KeyHolders.forEach(holder => {
            const keyEntry = document.createElement('div');
            keyEntry.className = 'key-holder-entry';
            keyEntry.innerHTML = `<span>${holder.TargetPlayerName} (ID: ${holder.TargetAccountId})</span>`;
            
            const removeBtn = document.createElement('button');
            removeBtn.innerText = 'Entziehen';
            removeBtn.className = 'remove-key-btn';
            removeBtn.onclick = () => {
                if (typeof mp !== 'undefined') {
                    // Bestätigung einholen
                    if (confirm(`Möchtest du den Schlüssel von ${holder.TargetPlayerName} wirklich entziehen?`)) {
                        mp.trigger('CEF:House:Action', 'removeKey', holder.TargetAccountId);
                    }
                }
            };
            keyEntry.appendChild(removeBtn);
            menuOptionsElem.appendChild(keyEntry);
        });
    }

    createButton('Zurück zum Hauptmenü', 'back');
}


// Event-Listener für Aktionen vom Client
// Wird jetzt vom Client aufgerufen, um das passende Menü zu zeigen
function handleMenuAction(action, data) {
    if (action === 'show_main') {
        showHouseMenu(data);
    } else if (action === 'show_management') {
        showManagementMenu(data);
    }
}

// Schließen-Logik
function closeMenu() {
    houseMenu.style.display = 'none';
    if (typeof mp !== 'undefined') {
        mp.trigger('CEF:House:Close');
    }
}

closeBtn.onclick = closeMenu;
window.handleMenuAction = handleMenuAction;