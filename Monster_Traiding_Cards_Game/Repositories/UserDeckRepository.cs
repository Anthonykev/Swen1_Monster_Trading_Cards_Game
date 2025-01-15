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
                    var command = new NpgsqlCommand("INSERT INTO UserDecks (UserId, CardId) VALUES (@userId, @cardId) ON CONFLICT (UserId, CardId) DO NOTHING", connection);
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

        public bool ClearUserDeck(int userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
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
    }
}

