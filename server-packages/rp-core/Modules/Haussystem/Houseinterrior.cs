using System.Collections.Generic;
using System.IO;
using GTANetworkAPI;
using Newtonsoft.Json; // Stelle sicher, dass du das Json.NET-Paket installiert hast

public class Houseinterrior : Script
{
    public string ipl { get; set; }
    public Vector3 position { get; set; }
    public Houseinterrior()
    {
    }
    public Houseinterrior(string ipl, Vector3 position)
    {
        this.ipl = ipl;
        this.position = position;
    }

    public static List<Houseinterrior> Interior_Liste = new List<Houseinterrior>();

    public static Vector3 GetHausAusgang(string ipl)
    {
        Vector3 position = new Vector3();
        foreach (Houseinterrior iplModel in Interior_Liste)
        {
            if (iplModel.ipl == ipl)
            {
                position = iplModel.position;
                break;
            }
        }
        return position;
    }

    public static void LoadInteriorsFromJson()
    {
        string filePath = @"C:\RAGEMP\server-files\serverdata\interriors.json";
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Interior_Liste = JsonConvert.DeserializeObject<List<Houseinterrior>>(json);
        }
    }
}
