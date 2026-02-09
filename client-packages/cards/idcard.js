// Speicherort: client_packages/idcard.js

let idCardBrowser = null;

// Dieses Event wird vom Server (cards.cs) ausgelöst, um den Ausweis anzuzeigen.
// Diese Logik bleibt unverändert.
mp.events.add('showPlayerIdCard', (jsonData) => {
    if (idCardBrowser) {
        idCardBrowser.destroy();
        idCardBrowser = null;
    }

    idCardBrowser = mp.browsers.new('package://cards/index.html');
    const data = JSON.parse(jsonData);

    setTimeout(() => {
        if (idCardBrowser && data) {
            idCardBrowser.execute(`setData('${data.firstname}', '${data.lastname}', '${data.birth}', '${data.gender}', ${data.accountId}, '${data.creationDate}');`);
        }
    }, 100);

    setTimeout(() => {
        if (idCardBrowser) {
            idCardBrowser.destroy();
            idCardBrowser = null;
        }
    }, 5000);
});

// Der komplette 'mp.events.add('playerCommand', ...)' Block wurde entfernt.