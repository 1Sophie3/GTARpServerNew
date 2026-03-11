using System;

namespace RPCore.Models.Inventory
{
    public class ItemDefinition
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Stackable { get; set; } = true;
        public int MaxStack { get; set; } = 64;
        public float Weight { get; set; } = 0.0f;
        public string? MetaSchema { get; set; }
    }
}
