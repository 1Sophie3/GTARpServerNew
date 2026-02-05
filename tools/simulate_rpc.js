// Lokale Simulation von mp.events, Client und Server (Registration + Login)
const fs = require('fs');

// Einfacher In-Memory Account-Store
const accounts = {};

// Helper: SHA256 Hash
const crypto = require('crypto');
function sha256(s) { return crypto.createHash('sha256').update(s).digest('hex'); }

// Mock mp API
global.mp = {
    events: (function () {
        const handlers = {};
        return {
            add: (name, fn) => {
                if (!handlers[name]) handlers[name] = [];
                handlers[name].push(fn);
            },
            callRemote: (name, ...args) => {
                console.log('[mp] callRemote ->', name, args);
                // Simuliere Server-Handler
                if (name === 'server:register') {
                    serverRegister(args[0], args[1]);
                } else if (name === 'server:login') {
                    serverLogin(args[0], args[1]);
                }
            },
            // Hilfs-Funktion: aus dem Server Events zum Client senden
            _callClient: (name, ...args) => {
                const list = handlers[name] || [];
                for (const fn of list) {
                    try { fn(...args); } catch (e) { console.error('handler error', e); }
                }
            }
        };
    })(),
    browsers: {
        new: (url) => {
            console.log('[mp.browsers] new(', url, ')');
            return {
                execute: (s) => console.log('[CEF EXECUTE]', s),
                destroy: () => console.log('[CEF] destroy')
            };
        }
    },
    gui: { cursor: { visible: false }, chat: { show: (v) => console.log('[gui.chat.show]', v) } },
    keys: { bind: () => { } },
    game: { graphics: { startScreenEffect: () => { }, stopScreenEffect: () => { } } },
    pools: { GetAllPlayers: () => [] }
};

// Server simulation functions
function serverRegister(username, password) {
    console.log('[server] register called', username);
    if (!username || !password) {
        global.mp.events._callClient('client:registerResult', false, 'Bitte Benutzername und Passwort angeben.');
        return;
    }
    if (accounts[username]) {
        global.mp.events._callClient('client:registerResult', false, 'Benutzername existiert bereits.');
        return;
    }
    const hashed = sha256(password);
    // Account anlegen ohne Charakternamen (Charakter wird später erstellt)
    accounts[username] = { username, passwordHash: hashed };
    console.log('[server] created account', accounts[username]);
    global.mp.events._callClient('client:registerResult', true, 'Account erstellt. Bitte melde dich an.');
}

function serverLogin(username, password) {
    console.log('[server] login called', username);
    if (!username || !password) {
        global.mp.events._callClient('client:loginResult', false, 'Bitte Benutzername und Passwort angeben.');
        return;
    }
    const acc = accounts[username];
    if (!acc) {
        global.mp.events._callClient('client:loginResult', false, 'Falscher Benutzername oder Passwort.');
        return;
    }
    const hashed = sha256(password);
    if (hashed !== acc.passwordHash) {
        global.mp.events._callClient('client:loginResult', false, 'Falscher Benutzername oder Passwort.');
        return;
    }
    // Sende login success mit playerData JSON (ohne Vor-/Nachname)
    const playerData = { id: 1, username: acc.username, cash: 500, bank: 5000 };
    global.mp.events._callClient('client:loginSuccess', JSON.stringify(playerData));
}

// Simuliere server:inventoryOpen
function serverInventoryOpen(category, targetId) {
    console.log('[server] inventoryOpen', category, targetId);
    // Beispiel-Inventardaten
    const slots = [];
    const slotCount = (category === 'wardrobe') ? 18 : (category === 'vehicle' ? 20 : 30);
    for (let i = 0; i < slotCount; i++) {
        if (i === 0) slots.push({ name: 'Wasserflasche', amount: 2 });
        else if (i === 2) slots.push({ name: 'Pistole', amount: 1 });
        else slots.push(null);
    }
    const payload = { category, id: targetId || 'local', slots };
    global.mp.events._callClient('client:updateInventory', JSON.stringify(payload));
}

// Simuliere server:inventoryTransfer
function serverInventoryTransfer(category, fromInvId, toType, toId, slotIdx, amount) {
    console.log('[server] inventoryTransfer', { category, fromInvId, toType, toId, slotIdx, amount });
    // Einfaches Erfolgsszenario
    global.mp.events._callClient('client:inventoryTransferResult', true, 'Transfer simuliert');
}

// Lade client script
console.log('Loading client script...');
require('../client-packages/rp-client/index.js');

// Simuliere: Show Login UI
global.mp.events._callClient('client:showLoginUI');

// Simuliere: CEF ruft cef:register -> in real app CEF would call mp.events.add('cef:register'...), in our client script mp.events.add('cef:register') is defined
console.log('\n-- Simuliere Registrierung --');
global.mp.events._callClient('cef:register', 'testuser', 'password123');

// Warte kurz und simuliere Login
setTimeout(() => {
    console.log('\n-- Simuliere Login --');
    global.mp.events._callClient('cef:login', 'testuser', 'password123');

    // Simuliere: Inventory öffnen nach Login
    setTimeout(() => {
        console.log('\n-- Simuliere Inventory Open --');
        serverInventoryOpen('player', 'local');
    }, 200);
}, 500);

// Ende nach kurzer Zeit
setTimeout(() => {
    console.log('\nAccounts store:', accounts);
    process.exit(0);
}, 1500);
