#nullable enable

using GTANetworkAPI;

namespace RPCore.Dealership
{
    public class VehicleDefinition
    {
        public string Model { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsFactionBuyable { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class VehicleInfoDTO
    {
        public string Model { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsFactionBuyable { get; set; }
        public int PlayerFactionRank { get; set; }
    }

    public class Dealership
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty; // Wichtig für die 1-zu-1-Zuweisung
        public Vector3 Position { get; set; } = new Vector3();
        public float Rotation { get; set; }
        public Vehicle? DisplayVehicle { get; set; }
    }
}