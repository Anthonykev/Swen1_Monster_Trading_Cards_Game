using System;
using System.Collections.Generic;
using Monster_Trading_Cards_Game.Repositories;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Models
{
    public class Battle
    {
        public User Player1 { get; private set; }
        public User Player2 { get; private set; }
        public User? Winner { get; private set; }

        private readonly IConfiguration _configuration;

        public Battle(User player1, User player2, IConfiguration configuration)
        {
            Player1 = player1 ?? throw new ArgumentNullException(nameof(player1), "Player1 cannot be null");
            Player2 = player2 ?? throw new ArgumentNullException(nameof(player2), "Player2 cannot be null");
            _configuration = configuration;
        }

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
            int drawCount = 0;
            const int maxRounds = 300;
            const int maxConsecutiveDraws = 10;
            Random random = new Random();

            while (Player1.Deck.Count > 0 && Player2.Deck.Count > 0 && roundCount < maxRounds)
            {
                Console.WriteLine($"\n--- Round {roundCount + 1} ---");
                Round round = new Round(Player1, Player2, _configuration);
                round.Play(random);

                if (round.Winner == null)
                {
                    drawCount++;
                    if (drawCount >= maxConsecutiveDraws)
                    {
                        Console.WriteLine("The battle ended in a draw after 10 consecutive draws!");
                        Winner = null;
                        break;
                    }
                }
                else
                {
                    drawCount = 0;
                }

                roundCount++;
            }

            DetermineWinner();

            // Decks leeren
            Player1.Deck.Clear();
            Player2.Deck.Clear();

            Player1.Save(Player1.UserName, Player1.SessionToken);
            Player2.Save(Player2.UserName, Player2.SessionToken);
        }

        private void DetermineWinner()
        {
            if (Player1.Deck.Count > 0 && Player2.Deck.Count == 0)
            {
                Winner = Player1;
                Console.WriteLine($"{Player1.UserName} gewinnt!");
                Player1.Wins++;
                Player2.Losses++;
            }
            else if (Player2.Deck.Count > 0 && Player1.Deck.Count == 0)
            {
                Winner = Player2;
                Console.WriteLine($"{Player2.UserName} gewinnt!");
                Player2.Wins++;
                Player1.Losses++;
            }
            else
            {
                Winner = null;
                Console.WriteLine("Das Battle endet unentschieden!");
            }

            // Aktualisieren der Gesamtspiele
            Player1.TotalGames++;
            Player2.TotalGames++;

            // Aktualisieren der Elo-Werte (Beispiel)
            UpdateElo(Player1, Player2);
        }

        private void UpdateElo(User player1, User player2)
        {
            // Beispielhafte Elo-Berechnung
            const int kFactor = 32;
            double expectedScore1 = 1.0 / (1.0 + Math.Pow(10, (player2.Elo - player1.Elo) / 400.0));
            double expectedScore2 = 1.0 / (1.0 + Math.Pow(10, (player1.Elo - player2.Elo) / 400.0));

            if (Winner == player1)
            {
                player1.Elo += (int)(kFactor * (1 - expectedScore1));
                player2.Elo += (int)(kFactor * (0 - expectedScore2));
            }
            else if (Winner == player2)
            {
                player1.Elo += (int)(kFactor * (0 - expectedScore1));
                player2.Elo += (int)(kFactor * (1 - expectedScore2));
            }
            else
            {
                player1.Elo += (int)(kFactor * (0.5 - expectedScore1));
                player2.Elo += (int)(kFactor * (0.5 - expectedScore2));
            }
        }
    }
}
