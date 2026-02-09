let licenseBrowser = null;

mp.events.add('showPlayerLicense', (jsonData) => {
    if (licenseBrowser) {
        licenseBrowser.destroy();
    }
    licenseBrowser = mp.browsers.new('package://cards/licenseui.html');
    
    setTimeout(() => {
        if (licenseBrowser) {
            licenseBrowser.execute(`setData(${jsonData});`);
        }
    }, 100);

    setTimeout(() => {
        if (licenseBrowser) {
            licenseBrowser.destroy();
            licenseBrowser = null;
        }
    }, 5000);
});

// Der playerCommand-Block wurde entfernt, da alle Befehle jetzt in C# sind.