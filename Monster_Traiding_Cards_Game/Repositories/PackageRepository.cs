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
                        var selectedCardIds = cardIds.OrderBy(x => random.Next()).Take(5).ToList();

                        var command = new NpgsqlCommand("INSERT INTO Packages (Card1Id, Card2Id, Card3Id, Card4Id, Card5Id) VALUES (@card1Id, @card2Id, @card3Id, @card4Id, @card5Id)", connection);
                        command.Parameters.AddWithValue("@card1Id", selectedCardIds[0]);
                        command.Parameters.AddWithValue("@card2Id", selectedCardIds[1]);
                        command.Parameters.AddWithValue("@card3Id", selectedCardIds[2]);
                        command.Parameters.AddWithValue("@card4Id", selectedCardIds[3]);
                        command.Parameters.AddWithValue("@card5Id", selectedCardIds[4]);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating random packages: {ex.Message}");
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

        public (int PackageId, List<int> CardIds)? GetPackage()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT Id, Card1Id, Card2Id, Card3Id, Card4Id, Card5Id FROM Packages LIMIT 1", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int packageId = reader.GetInt32(0);
                            var cardIds = new List<int>
                    {
                        reader.GetInt32(1),
                        reader.GetInt32(2),
                        reader.GetInt32(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5)
                    };
                            return (packageId, cardIds);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting package: {ex.Message}");
            }
            return null;
        }


        public void DeletePackage(int packageId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("DELETE FROM Packages WHERE Id = @packageId", connection);
                    command.Parameters.AddWithValue("@packageId", packageId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting package: {ex.Message}");
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


    }
}
