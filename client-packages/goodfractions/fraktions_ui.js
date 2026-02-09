document.addEventListener('DOMContentLoaded', () => {
    const factionMenu = document.getElementById('faction-menu');
    const closeBtn = document.getElementById('close-btn');
    const startDutyBtn = document.getElementById('startDutyBtn');
    const endDutyBtn = document.getElementById('endDutyBtn');
    const factionTitle = document.getElementById('faction-title');
    const tabs = document.querySelectorAll('.tab-btn');
    const panels = document.querySelectorAll('.tab-panel');

    // Funktion zum Aktualisieren des Menüs mit Daten vom Client
    window.updateMenu = (factionName, isOnDuty) => {
        factionTitle.textContent = `${factionName} Dienstmenü`;
        
        // Zeige den richtigen Button an
        startDutyBtn.style.display = isOnDuty ? 'none' : 'inline-block';
        endDutyBtn.style.display = isOnDuty ? 'inline-block' : 'none';

        factionMenu.style.display = 'flex';
    };

    // Event Listener für die Buttons
    startDutyBtn.addEventListener('click', () => {
        if (typeof mp !== 'undefined') {
            mp.trigger('Faction:CEF:StartDuty');
        }
    });

    endDutyBtn.addEventListener('click', () => {
        if (typeof mp !== 'undefined') {
            mp.trigger('Faction:CEF:EndDuty');
        }
    });

    closeBtn.addEventListener('click', () => {
        if (typeof mp !== 'undefined') {
            mp.trigger('Faction:CEF:CloseMenu');
        }
        factionMenu.style.display = 'none';
    });

    // Tab-Logik
    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            // Deaktiviere alle Tabs und Panels
            tabs.forEach(item => item.classList.remove('active'));
            panels.forEach(panel => panel.classList.remove('active'));

            // Aktiviere den geklickten Tab und das zugehörige Panel
            tab.classList.add('active');
            document.getElementById(tab.dataset.tab).classList.add('active');
        });
    });

    // Schließen mit ESC-Taste
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeBtn.click();
        }
    });
});