using Npgsql;
using System;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class BattleRoundRepository
    {
        private readonly string _connectionString;

        public BattleRoundRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool AddBattleRound(int battleId, int roundNumber, int player1CardId, int player2CardId, int winnerId, int loserId, string log)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO BattleRounds (BattleId, RoundNumber, Player1CardId, Player2CardId, WinnerId, LoserId, Log) VALUES (@battleId, @roundNumber, @player1CardId, @player2CardId, @winnerId, @loserId, @log)", connection);
                    command.Parameters.AddWithValue("@battleId", battleId);
                    command.Parameters.AddWithValue("@roundNumber", roundNumber);
                    command.Parameters.AddWithValue("@player1CardId", player1CardId);
                    command.Parameters.AddWithValue("@player2CardId", player2CardId);
                    command.Parameters.AddWithValue("@winnerId", winnerId);
                    command.Parameters.AddWithValue("@loserId", loserId);
                    command.Parameters.AddWithValue("@log", log);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding battle round: {ex.Message}");
                return false;
            }
        }
    }
}
