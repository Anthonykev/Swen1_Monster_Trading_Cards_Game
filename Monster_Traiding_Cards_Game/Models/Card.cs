using System;

namespace Monster_Trading_Cards_Game.Models
{
    public enum ElementType
    {
        Water,
        Fire,
        Normal
    }

    public abstract class Card
    {
        public int Id { get; set; } // Eindeutige ID für die Karte
        public string Name { get; set; }
        public int Damage { get; private set; } // Schaden ist nun abhängig vom Typ und wird nicht verändert
        public ElementType CardElementType { get; private set; }

        public Card(int id, string name, int damage, ElementType elementType)
        {
            Id = id;
            Name = name;
            Damage = damage;
            CardElementType = elementType;
        }

        // Berechnung des Schadens basierend auf dem Gegner
        public double CalculateDamage(Card opponent)
        {
            // Spezialregeln
            if (Name == "Goblins" && opponent.Name == "Dragons")
            {
                Console.WriteLine("Special Rule: Goblins do not attack Dragons. No damage!!");
                return 0; // Goblins greifen keine Drachen an
            }
            if (Name == "Orks" && opponent.Name == "Wizzard")
            { 
                Console.WriteLine("Special Rule: Orks deal no damage to Wizards. No damage dealt.");
          
                return 0; // Orks richten keinen Schaden an
            }

            if (Name == "Tsunami" && opponent.Name == "Knights")
            {
                Console.WriteLine("Special Rule: Knights instantly lose to WaterSpells. Immediate loss.");
                return double.MaxValue; // Knights ertrinken sofort
            }
            if (Name.Contains("Spell") && opponent.Name == "Kraken")
            {
                Console.WriteLine("Special Rule: Kraken are immune to Spells. No damage dealt.");
                return 0; // Kraken sind immun gegen Spells
            }
            if (Name == "Dragons" && opponent.Name == "FireElves")
            {
                Console.WriteLine("Special Rule: FireElves evade attacks from Dragons. No damage dealt.");
                return 0; // FireElves weichen Drachenangriffen aus
            }

            double effectiveness = 1.0;

            // Effektivitätsregel basierend auf den Typen
            if ((CardElementType == ElementType.Water && opponent.CardElementType == ElementType.Fire) ||
                (CardElementType == ElementType.Fire && opponent.CardElementType == ElementType.Normal) ||
                (CardElementType == ElementType.Normal && opponent.CardElementType == ElementType.Water))
            {
                effectiveness = 2.0; // Doppelter Schaden
            }
            else if ((CardElementType == ElementType.Fire && opponent.CardElementType == ElementType.Water) ||
                     (CardElementType == ElementType.Normal && opponent.CardElementType == ElementType.Fire) ||
                     (CardElementType == ElementType.Water && opponent.CardElementType == ElementType.Normal))
            {
                effectiveness = 0.5; // Halber Schaden
            }
            // Keine Änderung bei gleichen Typen oder nicht definierten Regeln

            double totalDamage = Damage * effectiveness;
            return totalDamage;
        }

        public abstract void PlayCard();
    }
}
