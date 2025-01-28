using System;
using System.Linq;
using Monster_Trading_Cards_Game.Repositories;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Models
{
    /// <summary>Represents a single round of a battle.</summary>
    public class Round
    {
        public User Player1 { get; private set; }
        public User Player2 { get; private set; }
        public User? Winner { get; private set; }

        private readonly UserDeckRepository _userDeckRepository;
        private readonly IConfiguration _configuration;

        /// <summary>Initializes a new instance of the <see cref="Round"/> class.</summary>
        /// <param name="player1">The first player.</param>
        /// <param name="player2">The second player.</param>
        /// <param name="configuration">The configuration instance.</param>
        public Round(User player1, User player2, IConfiguration configuration)
        {
            Player1 = player1;
            Player2 = player2;
            _configuration = configuration;
            _userDeckRepository = new UserDeckRepository(configuration);
        }

        /// <summary>Plays the round and determines the winner.</summary>
        public void Play(Random random)
        {
            // Select random cards from each player's deck
            var player1Card = GetRandomCard(Player1.Deck, random);
            var player2Card = GetRandomCard(Player2.Deck, random);

            string player1Reason, player2Reason;
            double player1Damage = player1Card.CalculateDamage(player2Card, out player1Reason);
            double player2Damage = player2Card.CalculateDamage(player1Card, out player2Reason);

            Console.WriteLine($"{Player1.UserName} plays {player1Card.Name} with damage {player1Damage}");
            Console.WriteLine($"{Player2.UserName} plays {player2Card.Name} with damage {player2Damage}");

            if (player1Damage > player2Damage)
            {
                Winner = Player1;
                Console.WriteLine($"Round winner: {Player1.UserName}");
                Console.WriteLine($"Reason: {player1Reason}");
                TransferCardDuringBattle(Player2, player2Card, Player1);
            }
            else if (player2Damage > player1Damage)
            {
                Winner = Player2;
                Console.WriteLine($"Round winner: {Player2.UserName}");
                Console.WriteLine($"Reason: {player2Reason}");
                TransferCardDuringBattle(Player1, player1Card, Player2);
            }
            else
            {
                Winner = null; // Unentschieden
                Console.WriteLine("The round ended in a draw.");
            }

            // Remove the played cards from the decks
            Player1.Deck.Remove(player1Card);
            Player2.Deck.Remove(player2Card);

            // Ausgabe der verbleibenden Karten
            Console.WriteLine($"{Player1.UserName} has {Player1.Deck.Count} cards left.");
            Console.WriteLine($"{Player2.UserName} has {Player2.Deck.Count} cards left.");
        }

        /// <summary>Transfers a card from the loser to the winner temporarily during the battle.</summary>
        private void TransferCardDuringBattle(User loser, Card card, User winner)
        {
            loser.Deck.Remove(card);
            winner.Deck.Add(card);

            // Update the database
            using (var connection = new NpgsqlConnection(_userDeckRepository.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        _userDeckRepository.RemoveCardFromUserDeck(loser.Id, card.Id, connection, transaction);
                        _userDeckRepository.AddCardToUserDeck(winner.Id, card.Id, connection, transaction);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error transferring card: {ex.Message}");
                        transaction.Rollback();
                    }
                }
            }
        }

        /// <summary>Selects a random card from the deck.</summary>
        private Card GetRandomCard(System.Collections.Generic.List<Card> deck, Random random)
        {
            int randomIndex = random.Next(deck.Count);
            return deck[randomIndex];
        }
    }
}
