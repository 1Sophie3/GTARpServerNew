using System;

namespace RPCore.Models.Inventory
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public int SlotIndex { get; set; }
        public int ItemDefId { get; set; }
        public int Amount { get; set; }
        public string? Meta { get; set; }
    }
}
