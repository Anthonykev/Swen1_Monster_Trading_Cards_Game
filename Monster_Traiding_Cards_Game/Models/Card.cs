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
