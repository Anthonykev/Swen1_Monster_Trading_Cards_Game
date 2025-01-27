using Npgsql;
using System;
using System.Collections.Generic;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Database;


namespace Monster_Trading_Cards_Game.Repositories
{
    public class UserStackRepository
    {
        private readonly string _connectionString;

        public UserStackRepository(string connectionString)
        {
            _connectionString = connectionString;
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
                    //Console.WriteLine($"Added card {cardId} to user {userId}'s stack in the database");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding card to user stack: {ex.Message}");
            }
        }


        public List<int> GetUserStack(int userId)
        {
            var stack = new List<int>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand(@"
                        SELECT CardId FROM UserStacks WHERE UserId = @userId
                    ", connection);
                    command.Parameters.AddWithValue("userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stack.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user stack: {ex.Message}");
                throw;
            }

            return stack;
        }
    }
}
