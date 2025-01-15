using Npgsql;
using System;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class PackageRepository
    {
        private readonly string _connectionString;

        public PackageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddPackage(int card1Id, int card2Id, int card3Id, int card4Id, int card5Id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Packages (Card1Id, Card2Id, Card3Id, Card4Id, Card5Id) VALUES (@card1Id, @card2Id, @card3Id, @card4Id, @card5Id)", connection);
                    command.Parameters.AddWithValue("@card1Id", card1Id);
                    command.Parameters.AddWithValue("@card2Id", card2Id);
                    command.Parameters.AddWithValue("@card3Id", card3Id);
                    command.Parameters.AddWithValue("@card4Id", card4Id);
                    command.Parameters.AddWithValue("@card5Id", card5Id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding package: {ex.Message}");
                return false;
            }
        }
    }
}

