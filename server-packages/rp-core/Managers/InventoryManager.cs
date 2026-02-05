using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using RPCore.Database;
using RPCore.Models.Inventory;
using GTANetworkAPI;

namespace RPCore.Managers
{
    public class InventoryManager
    {
        private static InventoryManager? _instance;
        public static InventoryManager Instance => _instance ??= new InventoryManager();

        private Dictionary<int, Inventory> _cache = new Dictionary<int, Inventory>();
        private Dictionary<string, ItemDefinition> _itemDefsByKey = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<int, ItemDefinition> _itemDefsById = new Dictionary<int, ItemDefinition>();

        private InventoryManager()
        {
            // Load item definitions on startup
            Task.Run(async () => await LoadItemDefinitions());
        }

        public async Task LoadItemDefinitions()
        {
            string query = "SELECT * FROM item_definitions";
            var reader = await DatabaseManager.Instance.ExecuteReader(query);
            if (reader == null) return;

            while (await reader.ReadAsync())
            {
                var def = new ItemDefinition
                {
                    Id = reader.GetInt32("id"),
                    Key = reader.GetString("key"),
                    Name = reader.GetString("name"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                    Stackable = reader.GetBoolean("stackable"),
                    MaxStack = reader.GetInt32("max_stack"),
                    Weight = reader.GetFloat("weight"),
                    MetaSchema = reader.IsDBNull(reader.GetOrdinal("meta_schema")) ? null : reader.GetString("meta_schema")
                };
                _itemDefsByKey[def.Key] = def;
                _itemDefsById[def.Id] = def;
            }
            reader.Close();
            NAPI.Util.ConsoleOutput($"[InventoryManager] {_itemDefsById.Count} item definitions loaded.");
        }

        public ItemDefinition? GetItemDefinitionByKey(string key)
        {
            return _itemDefsByKey.TryGetValue(key, out var d) ? d : null;
        }

        public ItemDefinition? GetItemDefinitionById(int id)
        {
            return _itemDefsById.TryGetValue(id, out var d) ? d : null;
        }

        public async Task<Inventory?> GetInventoryByOwner(string category, string ownerType, int ownerId)
        {
            string query = "SELECT * FROM inventories WHERE category = @cat AND owner_type = @otype AND owner_id = @oid LIMIT 1";
            var reader = await DatabaseManager.Instance.ExecuteReader(query,
                DatabaseManager.CreateParameter("@cat", category),
                DatabaseManager.CreateParameter("@otype", ownerType),
                DatabaseManager.CreateParameter("@oid", ownerId));

            if (reader == null) return null;
            if (!await reader.ReadAsync())
            {
                reader.Close();
                // create default inventory
                int defaultSlots = category == "player" ? 20 : (category == "wardrobe" ? 40 : 50);
                long newId = await CreateInventory(category, ownerType, ownerId, defaultSlots, null);
                if (newId == 0) return null;
                return await LoadInventory((int)newId);
            }

            var inv = new Inventory
            {
                Id = reader.GetInt32("id"),
                Category = reader.GetString("category"),
                OwnerType = reader.GetString("owner_type"),
                OwnerId = reader.GetInt32("owner_id"),
                SlotCount = reader.GetInt32("slot_count"),
                MaxWeight = reader.IsDBNull(reader.GetOrdinal("max_weight")) ? (float?)null : reader.GetFloat("max_weight")
            };
            reader.Close();

            var loaded = await LoadInventory(inv.Id);
            return loaded;
        }

        public async Task<long> CreateInventory(string category, string ownerType, int ownerId, int slotCount, float? maxWeight)
        {
            string query = @"INSERT INTO inventories (category, owner_type, owner_id, slot_count, max_weight, created_at)
                VALUES (@cat, @otype, @oid, @slots, @maxw, NOW())";
            long id = await DatabaseManager.Instance.ExecuteInsert(query,
                DatabaseManager.CreateParameter("@cat", category),
                DatabaseManager.CreateParameter("@otype", ownerType),
                DatabaseManager.CreateParameter("@oid", ownerId),
                DatabaseManager.CreateParameter("@slots", slotCount),
                DatabaseManager.CreateParameter("@maxw", maxWeight));

            return id;
        }

        public async Task<Inventory?> LoadInventory(int inventoryId)
        {
            if (_cache.ContainsKey(inventoryId)) return _cache[inventoryId];

            string qInv = "SELECT * FROM inventories WHERE id = @id LIMIT 1";
            var rInv = await DatabaseManager.Instance.ExecuteReader(qInv, DatabaseManager.CreateParameter("@id", inventoryId));
            if (rInv == null || !await rInv.ReadAsync())
            {
                rInv?.Close();
                return null;
            }

            var inv = new Inventory
            {
                Id = rInv.GetInt32("id"),
                Category = rInv.GetString("category"),
                OwnerType = rInv.GetString("owner_type"),
                OwnerId = rInv.GetInt32("owner_id"),
                SlotCount = rInv.GetInt32("slot_count"),
                MaxWeight = rInv.IsDBNull(rInv.GetOrdinal("max_weight")) ? (float?)null : rInv.GetFloat("max_weight")
            };
            rInv.Close();

            // Load items
            string qItems = "SELECT * FROM inventory_items WHERE inventory_id = @invId";
            var rItems = await DatabaseManager.Instance.ExecuteReader(qItems, DatabaseManager.CreateParameter("@invId", inventoryId));
            if (rItems != null)
            {
                while (await rItems.ReadAsync())
                {
                    var item = new InventoryItem
                    {
                        Id = rItems.GetInt32("id"),
                        InventoryId = rItems.GetInt32("inventory_id"),
                        SlotIndex = rItems.GetInt32("slot_index"),
                        ItemDefId = rItems.GetInt32("item_def_id"),
                        Amount = rItems.GetInt32("amount"),
                        Meta = rItems.IsDBNull(rItems.GetOrdinal("meta")) ? null : rItems.GetString("meta")
                    };
                    inv.Items[item.SlotIndex] = item;
                }
                rItems.Close();
            }

            _cache[inv.Id] = inv;
            return inv;
        }

        public async Task<bool> SaveInventory(Inventory inv)
        {
            if (inv == null) return false;

            // Update inventory meta
            string qUpdate = "UPDATE inventories SET slot_count = @slots, max_weight = @maxw WHERE id = @id";
            await DatabaseManager.Instance.ExecuteNonQuery(qUpdate,
                DatabaseManager.CreateParameter("@slots", inv.SlotCount),
                DatabaseManager.CreateParameter("@maxw", inv.MaxWeight),
                DatabaseManager.CreateParameter("@id", inv.Id));

            // Persist items: simple approach = delete existing and re-insert
            await DatabaseManager.Instance.ExecuteNonQuery("DELETE FROM inventory_items WHERE inventory_id = @invId", DatabaseManager.CreateParameter("@invId", inv.Id));

            foreach (var kv in inv.Items)
            {
                var it = kv.Value;
                string qIns = @"INSERT INTO inventory_items (inventory_id, slot_index, item_def_id, amount, meta, created_at)
                    VALUES (@invId, @slot, @defId, @amount, @meta, NOW())";
                await DatabaseManager.Instance.ExecuteInsert(qIns,
                    DatabaseManager.CreateParameter("@invId", inv.Id),
                    DatabaseManager.CreateParameter("@slot", it.SlotIndex),
                    DatabaseManager.CreateParameter("@defId", it.ItemDefId),
                    DatabaseManager.CreateParameter("@amount", it.Amount),
                    DatabaseManager.CreateParameter("@meta", it.Meta));
            }

            return true;
        }

        /// <summary>
        /// Versucht ein Item in ein Inventar zu legen (berücksichtigt Stapelbarkeit)
        /// </summary>
        public async Task<bool> AddItemToInventory(int inventoryId, int itemDefId, int amount)
        {
            var inv = await LoadInventory(inventoryId);
            if (inv == null) return false;

            var def = GetItemDefinitionById(itemDefId);
            if (def == null) return false;

            // Try to stack into existing slots
            if (def.Stackable)
            {
                foreach (var kv in inv.Items)
                {
                    var it = kv.Value;
                    if (it.ItemDefId == itemDefId && it.Amount < def.MaxStack)
                    {
                        int space = def.MaxStack - it.Amount;
                        int take = Math.Min(space, amount);
                        it.Amount += take;
                        amount -= take;
                        if (amount == 0) break;
                    }
                }
            }

            // Fill empty slots
            for (int slot = 0; slot < inv.SlotCount && amount > 0; slot++)
            {
                if (!inv.Items.ContainsKey(slot))
                {
                    int put = def.Stackable ? Math.Min(def.MaxStack, amount) : 1;
                    var newItem = new InventoryItem { InventoryId = inv.Id, SlotIndex = slot, ItemDefId = itemDefId, Amount = put };
                    inv.Items[slot] = newItem;
                    amount -= put;
                }
            }

            if (amount > 0)
            {
                // Not enough space
                return false;
            }

            await SaveInventory(inv);
            return true;
        }

        /// <summary>
        /// Entfernt eine Menge eines Items aus einem Inventar-Slot
        /// </summary>
        public async Task<bool> RemoveItemFromInventory(int inventoryId, int slotIndex, int amount)
        {
            var inv = await LoadInventory(inventoryId);
            if (inv == null) return false;

            if (!inv.Items.ContainsKey(slotIndex)) return false;

            var it = inv.Items[slotIndex];
            if (amount <= 0) return false;

            if (it.Amount > amount)
            {
                it.Amount -= amount;
            }
            else
            {
                // remove slot
                inv.Items.Remove(slotIndex);
            }

            await SaveInventory(inv);
            return true;
        }
    }
}
