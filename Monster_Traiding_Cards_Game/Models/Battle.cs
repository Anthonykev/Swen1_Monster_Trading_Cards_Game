using System;
using System.Collections.Generic;
using System.Linq;
using Monster_Trading_Cards_Game.Repositories;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Models
{
    /// <summary>Represents a battle between two players.</summary>
    public class Battle
    {
        public User Player1 { get; private set; }
        public User Player2 { get; private set; }
        public User? Winner { get; private set; }

        private List<Round> Rounds { get; set; }
        private Dictionary<int, List<Card>> OriginalDecks { get; set; }
        private readonly IConfiguration _configuration;

        /// <summary>Initializes a new instance of the <see cref="Battle"/> class.</summary>
        /// <param name="player1">The first player.</param>
        /// <param name="player2">The second player.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the players is null.</exception>
        public Battle(User player1, User player2, IConfiguration configuration)
        {
            Player1 = player1 ?? throw new ArgumentNullException(nameof(player1), "Player1 cannot be null");
            Player2 = player2 ?? throw new ArgumentNullException(nameof(player2), "Player2 cannot be null");
            Rounds = new List<Round>();
            OriginalDecks = new Dictionary<int, List<Card>>
            {
                { player1.Id, new List<Card>(player1.Deck) },
                { player2.Id, new List<Card>(player2.Deck) }
            };
            _configuration = configuration;
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

            // Remove players from the lobby
            RemovePlayersFromLobby();

            Console.WriteLine($"Battle started between {Player1.UserName} and {Player2.UserName}!");

            int roundCount = 0;
            int drawCount = 0;
            Random random = new Random();

            while (Player1.Deck.Count > 0 && Player2.Deck.Count > 0 && roundCount < 100)
            {
                Round round = new Round(Player1, Player2, _configuration);
                round.Play(random);
                Rounds.Add(round);

                if (round.Winner == null)
                {
                    drawCount++;
                }
                else
                {
                    drawCount = 0;
                }

                roundCount++;

                if (drawCount >= 20)
                {
                    Console.WriteLine("The battle ended in a draw after 20 consecutive draws!");
                    Winner = null;
                    break;
                }
            }

            // Determine the overall winner if not already set
            if (Winner == null)
            {
                if (Player1.Deck.Count > 0 && Player2.Deck.Count == 0)
                {
                    Winner = Player1;
                    Console.WriteLine($"The winner of the battle is: {Player1.UserName}");
                    Player1.Wins++;
                    Player1.Elo += 25;

                    Player2.Losses++;
                    if (Player2.Elo > 0)
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
            }

            // Update total games
            Player1.TotalGames++;
            Player2.TotalGames++;

            // Return the decks to the original owners
            ReturnCardsToOriginalOwners();

            // Save changes to database
            Player1.Save(Player1.UserName, Player1.SessionToken);
            Player2.Save(Player2.UserName, Player2.SessionToken);
        }

        /// <summary>Removes the players from the lobby.</summary>
        private void RemovePlayersFromLobby()
        {
            var lobbyRepository = new LobbyRepository(_configuration);

            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var deleteCommand = new NpgsqlCommand("DELETE FROM Lobby WHERE UserId = @User1Id OR UserId = @User2Id", connection, transaction);
                        deleteCommand.Parameters.AddWithValue("User1Id", Player1.Id);
                        deleteCommand.Parameters.AddWithValue("User2Id", Player2.Id);
                        deleteCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error removing players from lobby: {ex.Message}");
                        transaction.Rollback();
                    }
                }
            }
        }

        /// <summary>Returns the cards to the original owners after the battle.</summary>
        private void ReturnCardsToOriginalOwners()
        {
            var userDeckRepository = new UserDeckRepository(_configuration);

            using (var connection = new NpgsqlConnection(userDeckRepository.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var card in Player1.Deck.ToList())
                        {
                            if (!OriginalDecks[Player1.Id].Contains(card))
                            {
                                Player1.Deck.Remove(card);
                                OriginalDecks[Player2.Id].Add(card);
                                userDeckRepository.RemoveCardFromUserDeck(Player1.Id, card.Id, connection, transaction);
                                userDeckRepository.AddCardToUserDeck(Player2.Id, card.Id, connection, transaction);
                            }
                        }

                        foreach (var card in Player2.Deck.ToList())
                        {
                            if (!OriginalDecks[Player2.Id].Contains(card))
                            {
                                Player2.Deck.Remove(card);
                                OriginalDecks[Player1.Id].Add(card);
                                userDeckRepository.RemoveCardFromUserDeck(Player2.Id, card.Id, connection, transaction);
                                userDeckRepository.AddCardToUserDeck(Player1.Id, card.Id, connection, transaction);
                            }
                        }

                        Player1.Deck = OriginalDecks[Player1.Id];
                        Player2.Deck = OriginalDecks[Player2.Id];

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error returning cards to original owners: {ex.Message}");
                        transaction.Rollback();
                    }
                }
            }
        }
    }
}
