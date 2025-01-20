using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster_Trading_Cards_Game.Models
{
    /// <summary>Represents a battle between two players.</summary>
    public class Battle
    {
        public User Player1 { get; private set; }
        public User Player2 { get; private set; }
        public User? Winner { get; private set; }

        private List<Round> Rounds { get; set; }

        /// <summary>Initializes a new instance of the <see cref="Battle"/> class.</summary>
        /// <param name="player1">The first player.</param>
        /// <param name="player2">The second player.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the players is null.</exception>
        public Battle(User player1, User player2)
        {
            Player1 = player1 ?? throw new ArgumentNullException(nameof(player1), "Player1 cannot be null");
            Player2 = player2 ?? throw new ArgumentNullException(nameof(player2), "Player2 cannot be null");
            Rounds = new List<Round>();
        }

        /// <summary>Starts the battle between the two players.</summary>
        /// <exception cref="InvalidOperationException">Thrown when any player has an empty deck.</exception>
        public void Start()
        {
            if (Player1.Deck == null || Player1.Deck.Count == 0)
            {
                throw new InvalidOperationException($"{Player1.UserName} has no cards in their deck.");
            }

            if (Player2.Deck == null || Player2.Deck.Count == 0)
            {
                throw new InvalidOperationException($"{Player2.UserName} has no cards in their deck.");
            }

            Console.WriteLine($"Battle started between {Player1.UserName} and {Player2.UserName}!");

            int roundCount = 0;
            while (Player1.Deck.Count > 0 && Player2.Deck.Count > 0 && roundCount < 30)
            {
                Round round = new Round(Player1, Player2);
                round.Play();
                Rounds.Add(round);
                roundCount++;

                if (Player1.Deck.Count == 0 || Player2.Deck.Count == 0)
                {
                    break;
                }
            }

            // Determine the overall winner
            if (Player1.Deck.Count > 0 && Player2.Deck.Count == 0)
            {
                Winner = Player1;
                Console.WriteLine($"The winner of the battle is: {Player1.UserName}");
                Player1.Wins++;
                Player1.Elo += 25;

                Player2.Losses++;
                if (Player1.Elo > 0)
                {
                    Player2.Elo -= 25;
                }

            }
            else if (Player2.Deck.Count > 0 && Player1.Deck.Count == 0)
            {
                Winner = Player2;
                Console.WriteLine($"The winner of the battle is: {Player2.UserName}");
                Player2.Wins++;
                Player2.Elo += 25;

                Player1.Losses++;
                if (Player1.Elo > 0)
                {
                    Player1.Elo -= 25;
                }
                    
            }
            else
            {
                Winner = null;
                Console.WriteLine("The battle ended in a draw!");
            }

            // Update total games
            Player1.TotalGames++;
            Player2.TotalGames++;

            // Return the decks to the stacks
            Player1.ReturnDeckToStack(Player1.UserName, Player1.SessionToken);
            Player2.ReturnDeckToStack(Player2.UserName, Player2.SessionToken);

            // Save changes to database
            Player1.Save(Player1.SessionToken);
            Player2.Save(Player2.SessionToken);
        }
    }
}
