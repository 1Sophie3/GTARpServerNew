const speedText = document.getElementById('speed-text');
const gearText = document.getElementById('gear-text');
const rpmArc = document.getElementById('rpm-arc');
const needle = document.getElementById('needle');
const fuelBar = document.getElementById('fuel-bar');
const mileageText = document.getElementById('mileage-text');
const tachoContainer = document.getElementById('tacho-container');

const MAX_SPEED = 340; // Maximale Geschwindigkeit für die Nadel-Anzeige
const RPM_ARC_LENGTH = rpmArc.getTotalLength();

function updateUI(payload) {
    // Visuelle Elemente
    speedText.textContent = Math.floor(payload.Speed);
    gearText.textContent = payload.Gear > 0 ? payload.Gear : 'N';
    
    // Nadel rotieren (-90 bis +90 Grad)
    const speedPercent = Math.min(payload.Speed / MAX_SPEED, 1);
    const angle = -90 + (speedPercent * 180);
    needle.style.transform = `rotate(${angle}deg)`;

    // RPM-Bogen
    rpmArc.style.strokeDashoffset = RPM_ARC_LENGTH - (payload.Rpm * RPM_ARC_LENGTH);

    // Daten-Elemente
    fuelBar.style.width = `${payload.Fuel}%`;
    mileageText.textContent = `${payload.Mileage.toFixed(1)}`;
}

function showTacho(visible) {
    tachoContainer.style.display = visible ? 'block' : 'none';
}