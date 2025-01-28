using Npgsql;
using System;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class BattleRepository
    {
        private readonly string _connectionString;

        public BattleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool AddBattle(int user1Id, int user2Id, int winnerId, int loserId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Battles (User1Id, User2Id, WinnerId, LoserId) VALUES (@user1Id, @user2Id, @winnerId, @loserId)", connection);
                    command.Parameters.AddWithValue("@user1Id", user1Id);
                    command.Parameters.AddWithValue("@user2Id", user2Id);
                    command.Parameters.AddWithValue("@winnerId", winnerId);
                    command.Parameters.AddWithValue("@loserId", loserId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding battle: {ex.Message}");
                return false;
            }
        }
    }
}
