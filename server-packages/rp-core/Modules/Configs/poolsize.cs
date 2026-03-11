using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using GTANetworkAPI;

namespace RPCore.Configs
{
    class Poolsize
    {
        public static void Lutscher()
        {
            // Pfad zur gameconfig.xml Datei
            string filePath = "./serverdata/gameconfig.xml";

            // XML-Dokument erstellen oder laden
            XDocument config;
            if (System.IO.File.Exists(filePath))
            {
                config = XDocument.Load(filePath);
                NAPI.Util.ConsoleOutput("gameconfig.xml geladen");
            }
            else
            {
                config = new XDocument(new XElement("Config"));
            }

            // Pool-Größe setzen oder aktualisieren
            XElement poolSizeElement = config.Root.Element("StaticBoundsPoolSize");
            if (poolSizeElement == null)
            {
                poolSizeElement = new XElement("StaticBoundsPoolSize", 100000);
                config.Root.Add(poolSizeElement);
            }
            else
                NAPI.Util.ConsoleOutput("Server gestartet!");            {
                poolSizeElement.Value = "100000"; // oder einen höheren Wert setzen
            }
            // XML-Dokument speichern
            config.Save(filePath);

            Console.WriteLine("gameconfig.xml wurde erfolgreich aktualisiert.");
        }
    }
}