// bankscript.js
console.log("bankscript.js wird geladen.");

function populateTransactions(transactions) {
  const list = document.getElementById("transactionList");
  if (!list) return;
  list.innerHTML = "";

  if (!transactions || transactions.length === 0) {
    list.innerHTML = "<li>Keine Transaktionen vorhanden.</li>";
    return;
  }

  transactions.forEach(tx => {
    const item = document.createElement("li");
    const amountClass = tx.Amount > 0 ? "amount-positive" : "amount-negative";
    const sign = tx.Amount > 0 ? "+" : "";

    let descriptionHtml = '';
    switch(tx.Type) {
        case 'deposit':
            descriptionHtml = 'Einzahlung';
            break;
        case 'withdraw':
            descriptionHtml = 'Auszahlung';
            break;
        case 'transfer_sent':
            descriptionHtml = `Überweisung an ${tx.TargetKontonummer}:<br><small style="color: #ddd;">${tx.Description}</small>`;
            break;
        case 'transfer_received':
            descriptionHtml = `Überweisung von ${tx.SourceKontonummer}:<br><small style="color: #ddd;">${tx.Description}</small>`;
            break;
        case 'system_charge':
             descriptionHtml = `Systembelastung:<br><small style="color: #ddd;">${tx.Description}</small>`;
             break;
        default:
            descriptionHtml = tx.Description || 'Systemtransaktion';
    }
    
    const date = new Date(tx.TransactionDate).toLocaleString('de-DE');

    item.innerHTML = `
      <div class="transaction-details">
        <div>${descriptionHtml}</div>
        <small style="color: #aaa;">${date}</small>
      </div>
      <div class="transaction-amount ${amountClass}">${sign}${tx.Amount.toFixed(2)}€</div>
    `;
    list.appendChild(item);
  });
}

function showBankData(cash, bankBalance, accountNumber, transactionsJson) {
  document.getElementById("cashBalance").textContent = cash + "€";
  document.getElementById("bankBalance").textContent = bankBalance + "€";
  document.getElementById("accountNumber").textContent = accountNumber || "";

  try {
    const transactions = JSON.parse(transactionsJson);
    populateTransactions(transactions);
  } catch (e) {
    console.error("Fehler beim Parsen der Transaktionen:", e, "JSON:", transactionsJson);
    populateTransactions([]);
  }

  document.getElementById("bankMenu").style.display = "flex";
  console.log("showBankData ausgeführt. UI sichtbar.");
}
window.showBankData = showBankData;

function hideBankUI() {
  const bankMenu = document.getElementById("bankMenu");
  if (bankMenu) {
    bankMenu.style.display = "none";
  }
  console.log("hideBankUI ausgeführt. UI ausgeblendet.");
  if (typeof mp !== "undefined" && mp.events) {
    mp.events.call("CEF:Bank:Close");
  }
}
window.hideBankUI = hideBankUI;

function UpdateBankData(cash, bankBalance) {
  document.getElementById("cashBalance").textContent = cash + "€";
  document.getElementById("bankBalance").textContent = bankBalance + "€";
  console.log("UpdateBankData ausgeführt.");
}
window.UpdateBankData = UpdateBankData;

function depositFunds() {
  const rawVal = document.getElementById("amountInput").value;
  const amount = parseInt(rawVal);
  if (isNaN(amount) || amount <= 0) {
    alert("Bitte geben Sie einen gültigen Betrag ein.");
    return;
  }
  if (typeof mp !== "undefined" && mp.events) {
    mp.events.callRemote("Bank:Deposit", amount);
  }
  document.getElementById("amountInput").value = "";
}
window.depositFunds = depositFunds;

function withdrawFunds() {
  const rawVal = document.getElementById("amountInput").value;
  const amount = parseInt(rawVal);
  if (isNaN(amount) || amount <= 0) {
    alert("Bitte geben Sie einen gültigen Betrag ein.");
    return;
  }
  if (typeof mp !== "undefined" && mp.events) {
    mp.events.callRemote("Bank:Withdraw", amount);
  }
  document.getElementById("amountInput").value = "";
}
window.withdrawFunds = withdrawFunds;

function transferFunds() {
  const transferAccount = document.getElementById("transferAccountInput").value;
  const rawVal = document.getElementById("transferAmountInput").value;
  const description = document.getElementById("transferDescriptionInput").value;
  const amount = parseInt(rawVal);

  if (isNaN(amount) || amount <= 0) {
    alert("Bitte geben Sie einen gültigen Überweisungsbetrag ein.");
    return;
  }
  if (!transferAccount || transferAccount.length !== 9 || !/^\d+$/.test(transferAccount)) {
    alert("Bitte geben Sie eine gültige, 9-stellige Kontonummer ein.");
    return;
  }
  if (!description || description.trim().length === 0) {
      alert("Ein Verwendungszweck ist für die Überweisung erforderlich.");
      return;
  }

  if (typeof mp !== "undefined" && mp.events) {
    mp.events.callRemote("Bank:Transfer", transferAccount, amount, description);
  }
  
  document.getElementById("transferAccountInput").value = "";
  document.getElementById("transferAmountInput").value = "";
  document.getElementById("transferDescriptionInput").value = "";
}
window.transferFunds = transferFunds;

document.addEventListener("DOMContentLoaded", function() {
  console.log("bankscript.js geladen und bereit.");

  // Event Listeners für die normalen Buttons
  document.getElementById("closeBtn").addEventListener("click", hideBankUI);
  document.getElementById("depositBtn").addEventListener("click", depositFunds);
  document.getElementById("withdrawBtn").addEventListener("click", withdrawFunds);
  document.getElementById("transferBtn").addEventListener("click", transferFunds);

  // NEU: Logik für die Tab-Navigation
  const tabButtons = document.querySelectorAll(".tab-btn");
  const tabPanels = document.querySelectorAll(".tab-panel");

  tabButtons.forEach(button => {
    button.addEventListener("click", () => {
      // Entferne 'active' von allen Buttons und Panels
      tabButtons.forEach(btn => btn.classList.remove("active"));
      tabPanels.forEach(panel => panel.classList.remove("active"));

      // Füge 'active' zum geklickten Button und dem zugehörigen Panel hinzu
      button.classList.add("active");
      const targetPanelId = button.getAttribute("data-tab");
      document.getElementById(targetPanelId).classList.add("active");
    });
  });
});