function updateMenu(info) {
    document.getElementById('vehicle-name').innerText = info.DisplayName;
    document.getElementById('vehicle-price').innerText = '$' + info.Price.toLocaleString('de-DE');
    document.getElementById('vehicle-description').innerText = info.Description;

    const factionButton = document.getElementById('buy-faction-btn');
    if (info.IsFactionBuyable && info.PlayerFactionRank >= 10) {
        factionButton.style.display = 'inline-block';
    } else {
        factionButton.style.display = 'none';
    }
}

function buyVehicle(forFaction) {
    // Sende den Kaufwunsch an das client-side script
    mp.trigger('client:dealership:buy', forFaction);
}

function closeMenu() {
    mp.trigger('client:dealership:closeMenu');
}