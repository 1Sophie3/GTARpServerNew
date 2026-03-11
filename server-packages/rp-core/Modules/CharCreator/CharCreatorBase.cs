// -----------------------------------------------------------------------------
// CharCreatorBase: Abstrakte Basisklasse für plattformübergreifende Charakter-Erstellung
//
// Ziel: Gemeinsame Logik und Schnittstellen für CharCreator-Implementierungen (RAGE, FiveM)
//
// Jede Plattform erhält eine eigene Implementierung, die die plattformspezifischen
// Methoden (z.B. Skin setzen, Kleidung, Overlays) bereitstellt.
// -----------------------------------------------------------------------------
using System;

namespace RPCore.CharCreator
{
    public abstract class CharCustomizationLogicBase
    {
        public abstract void ApplyGender(object player, string gender);
        public abstract void ApplyCustomization(object player, string characterDataJson);
    }
}
