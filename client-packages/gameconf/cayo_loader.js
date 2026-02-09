// In client_packages/cayo_loader.js

mp.events.add('loadCayoPericoMap', () => {

    // Schritt 1: Aktiviere die Cayo Perico Insel.
    // Dies ist der Befehl, der das Problem verursacht, weil er Los Santos "vergisst".
    mp.game.invoke('0x9A9D1BA639675CF1', 'HeistIsland', true);

    // Schritt 2: DER WICHTIGSTE TRICK - Aktiviere sofort danach die Hauptkarte wieder.
    // Der Name "Isleofluck" bezieht sich intern auf das Casino-DLC.
    // Die Aktivierung dieses Teils erzwingt das Neuladen der gesamten Hauptkarte von Los Santos.
    mp.game.invoke('0x9A9D1BA639675CF1', 'Isleofluck', true);

    mp.console.logInfo('Forcing Cayo Perico AND Los Santos maps to be enabled.');
});