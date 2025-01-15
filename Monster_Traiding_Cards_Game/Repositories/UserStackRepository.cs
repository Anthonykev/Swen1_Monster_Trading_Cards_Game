using Npgsql;
using System;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class UserStackRepository
    {
        private readonly string _connectionString;

        public UserStackRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddCardToUserStack(int userId, int cardId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO UserStacks (UserId, CardId) VALUES (@userId, @cardId) ON CONFLICT (UserId, CardId) DO NOTHING", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@cardId", cardId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding card to user stack: {ex.Message}");
                return false;
            }
        }
    }
}

