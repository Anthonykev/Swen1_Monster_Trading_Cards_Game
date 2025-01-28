using Npgsql;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Monster_Trading_Cards_Game.Models;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class UserDeckRepository
    {
        public string ConnectionString { get; }

        public UserDeckRepository(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
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
            var cards = new List<Card>();
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
                            var cardRepository = new CardRepository(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
                            var card = cardRepository.GetCardById(cardId);
                            if (card != null)
                            {
                                cards.Add(card);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user deck: {ex.Message}");
            }
            return cards;
        }

        public bool ClearUserDeck(int userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("DELETE FROM UserDecks WHERE UserId = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    return command.ExecuteNonQuery() > 0;
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
                    var command = new NpgsqlCommand("DELETE FROM UserDecks", connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing all user decks: {ex.Message}");
            }
        }
    }
}

