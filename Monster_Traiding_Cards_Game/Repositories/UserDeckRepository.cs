using Monster_Trading_Cards_Game.Models;
using Npgsql;
using System;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class UserDeckRepository
    {
        private readonly string _connectionString;

        public UserDeckRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddCardToUserDeck(int userId, int cardId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine($"Connected to database. Adding card {cardId} to deck for user {userId}");
                    var command = new NpgsqlCommand("INSERT INTO UserDecks (UserId, CardId) VALUES (@userId, @cardId)", connection);

                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@cardId", cardId);
                    var result = command.ExecuteNonQuery() > 0;
                    Console.WriteLine($"AddCardToUserDeck: UserId={userId}, CardId={cardId}, Result={result}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding card to user deck: {ex.Message}");
                return false;
            }
        }

        public List<Card> GetUserDeck(int userId)
        {
            var deck = new List<Card>();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT CardId FROM UserDecks WHERE UserId = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var cardId = reader.GetInt32(0);
                            var cardRepository = new CardRepository(_connectionString);
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
                using (var connection = new NpgsqlConnection(_connectionString))
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
                using (var connection = new NpgsqlConnection(_connectionString))
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
