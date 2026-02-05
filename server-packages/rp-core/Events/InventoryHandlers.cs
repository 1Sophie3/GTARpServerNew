using GTANetworkAPI;
using RPCore.Managers;
using System.Threading.Tasks;
using RPCore.Models.Permission;

namespace RPCore.Events
{
    public class InventoryHandlers : Script
    {
        [RemoteEvent("server:inventoryOpen")]
        public async void OnInventoryOpen(Player player, string category, string ownerType, int ownerId)
        {
            // Basic permission check: player can open own player-inventory or staff can open any
            var account = AccountManager.Instance.GetAccountByPlayer(player);
            var permission = account != null ? AccountManager.Instance.GetPermission(account.Id) : null;

            bool allowed = false;
            if (category == "player" && ownerType == "account" && account != null && account.Id == ownerId)
                allowed = true;
            if (!allowed && permission != null && permission.IsStaff())
                allowed = true;

            if (!allowed)
            {
                player.TriggerEvent("client:updateInventory", false, "Keine Berechtigung zum Öffnen dieses Inventars");
                return;
            }

            // Load or create inventory
            var inv = await InventoryManager.Instance.GetInventoryByOwner(category, ownerType, ownerId);
            if (inv == null)
            {
                player.TriggerEvent("client:updateInventory", false, "Inventar konnte nicht geladen werden");
                return;
            }

            // Send inventory data to client (simple representation)
            player.TriggerEvent("client:inventoryOpened", inv.Id, inv.SlotCount);
            // Also send items
            foreach (var kv in inv.Items)
            {
                var it = kv.Value;
                player.TriggerEvent("client:updateInventoryItem", inv.Id, it.SlotIndex, it.ItemDefId, it.Amount, it.Meta ?? "");
            }
        }

        [RemoteEvent("server:inventoryTransfer")]
        public async void OnInventoryTransfer(Player player, int fromInvId, int fromSlot, int toInvId, int toSlot, int amount)
        {
            if (amount <= 0) { player.TriggerEvent("client:updateInventory", false, "Ungültige Menge"); return; }

            // Basic permission: player must have access to source inventory (own or staff)
            var account = AccountManager.Instance.GetAccountByPlayer(player);
            var permission = account != null ? AccountManager.Instance.GetPermission(account.Id) : null;

            var src = await InventoryManager.Instance.LoadInventory(fromInvId);
            var dst = await InventoryManager.Instance.LoadInventory(toInvId);
            if (src == null || dst == null) { player.TriggerEvent("client:updateInventory", false, "Inventar nicht gefunden"); return; }

            bool srcAllowed = false;
            if (src.Category == "player" && src.OwnerType == "account" && account != null && src.OwnerId == account.Id) srcAllowed = true;
            if (!srcAllowed && permission != null && permission.IsStaff()) srcAllowed = true;

            if (!srcAllowed) { player.TriggerEvent("client:updateInventory", false, "Keine Berechtigung auf Quelle"); return; }

            // Check item exists in fromSlot
            if (!src.Items.ContainsKey(fromSlot)) { player.TriggerEvent("client:updateInventory", false, "Quelle leer"); return; }
            var item = src.Items[fromSlot];
            if (item.Amount < amount) { player.TriggerEvent("client:updateInventory", false, "Nicht genug Items in Quelle"); return; }

            // Perform transfer in-memory
            int itemDefId = item.ItemDefId;

            // Remove from source
            bool removed = await InventoryManager.Instance.RemoveItemFromInventory(fromInvId, fromSlot, amount);
            if (!removed) { player.TriggerEvent("client:updateInventory", false, "Fehler beim Entfernen aus Quelle"); return; }

            // Add to destination
            bool added = await InventoryManager.Instance.AddItemToInventory(toInvId, itemDefId, amount);
            if (!added)
            {
                // Try to rollback (best-effort): add back to source
                await InventoryManager.Instance.AddItemToInventory(fromInvId, itemDefId, amount);
                player.TriggerEvent("client:updateInventory", false, "Kein Platz im Zielinventar");
                return;
            }

            // Notify client to refresh both inventories
            player.TriggerEvent("client:updateInventory", true, "Transfer erfolgreich");
            player.TriggerEvent("client:inventoryRefresh", fromInvId);
            player.TriggerEvent("client:inventoryRefresh", toInvId);
        }
    }
}
