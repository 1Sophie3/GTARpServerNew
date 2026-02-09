using GTANetworkAPI;

namespace RPCore.datenbank
{
    public class Garage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float NpcHeading { get; set; }
        public int MaxVehicles { get; set; }
        public int BlipId { get; set; }
        public int BlipColor { get; set; }
        public int FraktionId { get; set; }

      
        public Garage(int id, string name, float posX, float posY, float posZ, float npcHeading, int maxVehicles, int blipId, int blipColor, int fraktionId = 0)
        {
            Id = id;
            Name = name;
            Position = new Vector3(posX, posY, posZ);
            NpcHeading = npcHeading;
            MaxVehicles = maxVehicles;
            BlipId = blipId;
            BlipColor = blipColor;
            FraktionId = fraktionId;
            
        }
    }
}