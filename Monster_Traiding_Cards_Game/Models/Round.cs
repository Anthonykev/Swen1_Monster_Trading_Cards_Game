using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster_Trading_Cards_Game.Models
{
    /// <summary>Represents a single round of a battle.</summary>
    public class Round
    {
        public User Player1 { get; private set; }
        public User Player2 { get; private set; }
        public User? Winner { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="Round"/> class.</summary>
        /// <param name="player1">The first player.</param>
        /// <param name="player2">The second player.</param>
        public Round(User player1, User player2)
        {
            Player1 = player1;
            Player2 = player2;
        }

        /// <summary>Plays the round and determines the winner.</summary>
        public void Play()
        {
            // Assuming both players play with their decks
            var player1Card = Player1.Deck.First();
            var player2Card = Player2.Deck.First();

            double player1Damage = player1Card.CalculateDamage(player2Card);
            double player2Damage = player2Card.CalculateDamage(player1Card);

            Console.WriteLine($"{Player1.UserName} plays {player1Card.Name} with damage {player1Damage}");
            Console.WriteLine($"{Player2.UserName} plays {player2Card.Name} with damage {player2Damage}");

            if (player1Damage > player2Damage)
            {
                Winner = Player1;
                Console.WriteLine($"Round winner: {Player1.UserName}");
                TransferCard(Player2, Player1);
            }
            else if (player2Damage > player1Damage)
            {
                Winner = Player2;
                Console.WriteLine($"Round winner: {Player2.UserName}");
                TransferCard(Player1, Player2);
            }
            else
            {
                Winner = null; // Unentschieden
                Console.WriteLine("The round ended in a draw.");
            }

            // Remove the played cards from the decks
            Player1.Deck.Remove(player1Card);
            Player2.Deck.Remove(player2Card);
        }

        /// <summary>Transfers a card from the loser to the winner.</summary>
        private void TransferCard(User loser, User winner)
        {
            if (loser.Deck.Count > 0)
            {
                var card = loser.Deck.First();
                loser.Deck.Remove(card);
                winner.Deck.Add(card);
            }
        }
    }
}
