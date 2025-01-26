using Monster_Trading_Cards_Game.Models;
using Npgsql;
using System;
using System.Collections.Generic;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class UserDeckRepository
    {
        public string ConnectionString { get; }

        public UserDeckRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public bool AddCardToUserDeck(int userId, int cardId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO UserDecks (UserId, CardId) VALUES (@userId, @cardId)", connection);

                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@cardId", cardId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding card to user deck: {ex.Message}");
                return false;
            }
        }



        public bool AddCardToUserDeck(int userId, int cardId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            try
            {
                var command = new NpgsqlCommand("INSERT INTO UserDecks (UserId, CardId) VALUES (@userId, @cardId)", connection, transaction);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@cardId", cardId);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding card to user deck: {ex.Message}");
                return false;
            }
        }

        public bool RemoveCardFromUserDeck(int userId, int cardId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("DELETE FROM UserDecks WHERE UserId = @userId AND CardId = @cardId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@cardId", cardId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing card from user deck: {ex.Message}");
                return false;
            }
        }

        public bool RemoveCardFromUserDeck(int userId, int cardId, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            try
            {
                var command = new NpgsqlCommand("DELETE FROM UserDecks WHERE UserId = @userId AND CardId = @cardId", connection, transaction);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@cardId", cardId);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing card from user deck: {ex.Message}");
                return false;
            }
        }

        public List<Card> GetUserDeck(int userId)
        {
            var deck = new List<Card>();
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT CardId FROM UserDecks WHERE UserId = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var cardId = reader.GetInt32(0);
                            var cardRepository = new CardRepository(ConnectionString);
                            var card = cardRepository.GetCardById(cardId);
                            if (card != null)
                            {
                                deck.Add(card);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user deck: {ex.Message}");
            }
            return deck;
        }

        public bool ClearUserDeck(int userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    Console.WriteLine($"Connected to database. Clearing deck for user {userId}");
                    var command = new NpgsqlCommand("DELETE FROM UserDecks WHERE UserId = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    var result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        Console.WriteLine($"ClearUserDeck: UserId={userId}, Result=True");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"ClearUserDeck: UserId={userId}, Deck is already clear");
                        return true; // Return true even if the deck was already clear
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing user deck: {ex.Message}");
                return false;
            }
        }

        public void ClearAllUserDecks()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var clearDecksCommand = new NpgsqlCommand("DELETE FROM UserDecks", connection);
                    clearDecksCommand.ExecuteNonQuery();
                    Console.WriteLine("Alle Benutzerdecks wurden geleert.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Leeren der Benutzerdecks: {ex.Message}");
            }
        }
    }
}


