using Npgsql;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class UserStackRepository
    {
        private readonly string _connectionString;

        public UserStackRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void AddCardToUserStack(int userId, int cardId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO UserStacks (UserId, CardId) VALUES (@userId, @cardId)", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@cardId", cardId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding card to user stack: {ex.Message}");
            }
        }

        public List<int> GetUserStack(int userId)
        {
            var cardIds = new List<int>();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT CardId FROM UserStacks WHERE UserId = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cardIds.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user stack: {ex.Message}");
            }
            return cardIds;
        }
    }
}
