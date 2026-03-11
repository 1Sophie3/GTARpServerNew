using System;
using System.Collections.Generic;

namespace RPCore.Models.Inventory
{
    public class Inventory
    {
        public int Id { get; set; }
        public string Category { get; set; } = "player"; // player, vehicle, faction, house, wardrobe
        public string OwnerType { get; set; } = "account"; // account, vehicle, faction, house
        public int OwnerId { get; set; }
        public int SlotCount { get; set; } = 20;
        public float? MaxWeight { get; set; }

        // In-memory items keyed by slot index
        public Dictionary<int, InventoryItem> Items { get; set; } = new Dictionary<int, InventoryItem>();
    }
}
