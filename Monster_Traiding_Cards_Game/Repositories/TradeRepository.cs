using Npgsql;
using System;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class TradeRepository
    {
        private readonly string _connectionString;

        public TradeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddTrade(int userId, int offeredCardId, string requestedCardType, string requestedCardElementType, int minimumDamage)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Trades (UserId, OfferedCardId, RequestedCardType, RequestedCardElementType, MinimumDamage) VALUES (@userId, @offeredCardId, @requestedCardType, @requestedCardElementType, @minimumDamage)", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@offeredCardId", offeredCardId);
                    command.Parameters.AddWithValue("@requestedCardType", requestedCardType);
                    command.Parameters.AddWithValue("@requestedCardElementType", requestedCardElementType);
                    command.Parameters.AddWithValue("@minimumDamage", minimumDamage);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding trade: {ex.Message}");
                return false;
            }
        }
    }
}

