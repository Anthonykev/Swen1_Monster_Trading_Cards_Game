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
        public Battle(User player1, User player2)
        {
            Player1 = player1;
            Player2 = player2;
            Rounds = new List<Round>();
        }

        /// <summary>Starts the battle between the two players.</summary>
        public void Start()
        {
            Console.WriteLine($"Battle started between {Player1.UserName} and {Player2.UserName}!");

            int player1Wins = 0;
            int player2Wins = 0;

            // Example: Conduct 3 rounds
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"Starting Round {i + 1}...");
                Round round = new Round(Player1, Player2);
                round.Play();
                Rounds.Add(round);

                if (round.Winner == Player1)
                {
                    player1Wins++;
                }
                else if (round.Winner == Player2)
                {
                    player2Wins++;
                }
            }

            // Determine the overall winner
            if (player1Wins > player2Wins)
            {
                Winner = Player1;
                Console.WriteLine($"The winner of the battle is: {Player1.UserName}");
            }
            else if (player2Wins > player1Wins)
            {
                Winner = Player2;
                Console.WriteLine($"The winner of the battle is: {Player2.UserName}");
            }
            else
            {
                Winner = null;
                Console.WriteLine("The battle ended in a draw!");
            }
        }
    }
}