using System;

namespace FHTW.Swen1.Swamp.Models
{
    public enum ElementType
    {
        Water,
        Fire,
        Normal
    }

    public abstract class Card
    {
        public string Name { get; set; }
        public int Damage { get; private set; } // Schaden ist nun abhängig vom Typ und wird nicht verändert
        public ElementType CardElementType { get; private set; }

        public Card(string name)
        {
            Name = name;
            CardElementType = AssignElementType(name); // Elementtyp basierend auf dem Namen zuweisen
            AssignDamage(); // Schaden basierend auf dem Typ zuweisen
        }

        // Methode zur Zuweisung des richtigen Elementtyps basierend auf dem Kartennamen
        private ElementType AssignElementType(string name)
        {
            switch (name)
            {
                case "Dragons":
                case "FireElves":
                case "Amaterasu":
                case "Raijin":
                case "Tetsu":
                    return ElementType.Fire; // Fire Typ

                case "Kraken":
                case "DogMike":
                case "Susanoo":
                case "Bankai":
                case "Wizzard":
                    return ElementType.Water; // Water Typ

                case "Goblins":
                case "Knights":
                case "Orks":
                case "Rocklee":
                case "FighterKevin":
                    return ElementType.Normal; // Normal Typ

                default:
                    return ElementType.Normal; // Standardwert
            }
        }

        // Methode zur Zuweisung des Schadens basierend auf dem Typ
        private void AssignDamage()
        {
            switch (CardElementType)
            {
                case ElementType.Normal:
                    Damage = 50; // Schaden für Normalkarten
                    break;
                case ElementType.Fire:
                    Damage = 70; // Schaden for Type Fire
                    break;
                case ElementType.Water:
                    Damage = 70; // Schaden für Water
                    break;
                default:
                    Damage = 50; // Standardwert für den Fall eines unbekannten Typs
                    break;
            }
        }

        // Berechnung des Schadens basierend auf dem Gegner
        public double CalculateDamage(Card opponent)
        {
            double effectiveness = 1.0;

            // Beispiel: Abhängigkeit des Schadens basierend auf den Typen (später erweiterbar)
            if ((CardElementType == ElementType.Fire && opponent.CardElementType == ElementType.Normal) ||
                (CardElementType == ElementType.Water && opponent.CardElementType == ElementType.Fire))
                
            {
                effectiveness = 2.0; // Doppelter Schaden
            }
            else if ((CardElementType == ElementType.Fire && opponent.CardElementType == ElementType.Water))  
            {
                effectiveness = 0.5; // Halber Schaden
            }
            

            double totalDamage = Damage * effectiveness;
            return totalDamage;
        }

        public abstract void PlayCard();
    }
}
