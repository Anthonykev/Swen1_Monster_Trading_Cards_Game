using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int Damage { get; set; } = 50; // Standardwert für alle Karten
        public ElementType CardElementType { get; set; }

        public Card(string name)
        {
            Name = name;
            CardElementType = AssignElementType(name); // Zuweisen des Elementtyps basierend auf dem Namen
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

        public double CalculateDamage(Card opponent)
        {
            double effectiveness = 1.0;

            







            double totalDamage = Damage * effectiveness;
            return totalDamage;
        }

        public abstract void PlayCard();
    }
}