using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHTW.Swen1.Swamp.Models
{
    internal class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coins { get; set; } = 20;
        //----------------------------------------------------------------------------------------------

        // Alle Karten des Benutzers werden im Stack gespeichert
        public List<Card> Stack { get; set; } = new List<Card>();

        // Verfügbare Kartennamen zur zufälligen Erstellung der Karten
        private List<string> CardNames = new List<string>()
        {
            "Goblins", "Dragons", "Wizzard", "Knights", "Orks", "Kraken", "FireElves", "Lion", "DogMike", "Rocklee",
            "Tetsu", "Amaterasu", "Bankai", "Raijin", "Susanoo", "FighterKevin"
        };

        //-----------------------------------------------------------------------------------------------


        /// <summary>
        /// Erstellt eine neue Karte basierend auf dem Kartennamen
        /// </summary>
        /// <param name="cardName"></param>
        /// <returns></returns>
        private Card CreateCard(string cardName)
        {
            if (cardName == "Dragons" || cardName == "FireElves" || cardName == "Kraken" || cardName == "Lion") //
            {
                return new MonsterCard(cardName);
            }
            else if (cardName == "Wizzard" || cardName == "Tetsu" || cardName == "Amaterasu" ||
                     cardName == "Bankai") // SpellCards
            {
                return new SpellCard(cardName);
            }
            else
            {
                return new NormalCard(cardName);
            }
        }

        /// <summary>
        /// Der Benutzer kauft ein Pack, das aus 5 Karten besteht. Diese Karten werden dann im Stack gespeichert.
        /// </summary>
        public void AddPackage()
        {
            // Überprüfen, ob der Benutzer genug Coins hat, um ein Pack zu kaufen
            if (Coins < 5)
            {
                Console.WriteLine("Not enough coins to buy a package. You need at least 5 coins.");
                return;
            }

            // Reduzieren der Coins des Benutzers um die Kosten für das Pack
            Coins -= 5;

            Random randNames = new Random();

            // Fünf Karten erstellen und zum Stack hinzufügen
            for (int i = 0; i < 5; i++)
            {
                // Zufällige Auswahl eines Kartennamens
                string cardName = CardNames[randNames.Next(CardNames.Count)];

                // Karte basierend auf dem Namen erstellen
                Card newCard = CreateCard(cardName);

                // Hinzufügen der Karte zum Stack
                Stack.Add(newCard);

                Console.WriteLine($"Added new card to stack: {newCard.Name} ({newCard.CardElementType})");
            }
        }

        /// <summary>
        /// Methode, um die besten Karten aus dem Stack auszuwählen und sie dem Deck hinzuzufügen
        /// </summary>
        public void ChooseDeck()
        {
            // ich muss mir das noch überlegen, noch kein plan
        }

        public void printStack()
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("A list of all stored cards!!!");

            foreach (Card item in Stack)
            {
                Console.WriteLine(item);
            }


        }
    }
}