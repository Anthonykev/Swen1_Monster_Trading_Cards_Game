using Npgsql;
using System;
using System.Collections.Generic;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class PackageRepository
    {
        private readonly string _connectionString;

        public PackageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateRandomPackages(int numberOfPackages)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var cardIds = GetCardIdsFromDatabase(connection);

                    for (int i = 0; i < numberOfPackages; i++)
                    {
                        var random = new Random();
                        var card1Id = cardIds[random.Next(cardIds.Count)];
                        var card2Id = cardIds[random.Next(cardIds.Count)];
                        var card3Id = cardIds[random.Next(cardIds.Count)];
                        var card4Id = cardIds[random.Next(cardIds.Count)];
                        var card5Id = cardIds[random.Next(cardIds.Count)];

                        var command = new NpgsqlCommand("INSERT INTO Packages (Card1Id, Card2Id, Card3Id, Card4Id, Card5Id) VALUES (@card1Id, @card2Id, @card3Id, @card4Id, @card5Id)", connection);
                        command.Parameters.AddWithValue("@card1Id", card1Id);
                        command.Parameters.AddWithValue("@card2Id", card2Id);
                        command.Parameters.AddWithValue("@card3Id", card3Id);
                        command.Parameters.AddWithValue("@card4Id", card4Id);
                        command.Parameters.AddWithValue("@card5Id", card5Id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating random packages: {ex.Message}");
            }
        }

        public void DeleteAllPackages()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("DELETE FROM Packages", connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all packages: {ex.Message}");
            }
        }

        public bool ArePackagesAvailable()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT COUNT(*) FROM Packages", connection);
                    var count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking package availability: {ex.Message}");
                return false;
            }
        }

        private List<int> GetCardIdsFromDatabase(NpgsqlConnection connection)
        {
            var cardIds = new List<int>();
            var command = new NpgsqlCommand("SELECT Id FROM Cards", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    cardIds.Add(reader.GetInt32(0));
                }
            }
            return cardIds;
        }
        public int GetPackageCount()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT COUNT(*) FROM Packages", connection);
                    var count = (long)command.ExecuteScalar();
                    return (int)count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting package count: {ex.Message}");
                return 0;
            }
        }


    }
}
