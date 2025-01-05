using System;
using System.Collections.Generic;
using Npgsql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster_Traiding_Cards.Database
{
    internal class Database
    {
        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public bool CreateTable()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new NpgsqlCommand(@"
                        CREATE TABLE IF NOT EXISTS Cards (
                            Id SERIAL PRIMARY KEY,
                            Name VARCHAR(100) NOT NULL,
                            Type VARCHAR(50) NOT NULL,
                            Damage INT NOT NULL
                        )", connection);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}");
                return false;
            }
        }

        public bool AddCard(string cardName, string cardType, int damage)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Cards (Name, Type, Damage) VALUES (@name, @type, @damage)", connection);
                    command.Parameters.AddWithValue("@name", cardName);
                    command.Parameters.AddWithValue("@type", cardType);
                    command.Parameters.AddWithValue("@damage", damage);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding card: {ex.Message}");
                return false;
            }
        }

        public bool UpdateCard(int cardId, string cardName, string cardType, int damage)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new NpgsqlCommand("UPDATE Cards SET Name = @name, Type = @type, Damage = @damage WHERE Id = @id", connection);
                    command.Parameters.AddWithValue("@id", cardId);
                    command.Parameters.AddWithValue("@name", cardName);
                    command.Parameters.AddWithValue("@type", cardType);
                    command.Parameters.AddWithValue("@damage", damage);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating card: {ex.Message}");
                return false;
            }
        }

        public bool DeleteCard(int cardId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new NpgsqlCommand("DELETE FROM Cards WHERE Id = @id", connection);
                    command.Parameters.AddWithValue("@id", cardId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting card: {ex.Message}");
                return false;
            }
        }

        public List<(int Id, string Name, string Type, int Damage)> GetAllCards()
        {
            var cards = new List<(int Id, string Name, string Type, int Damage)>();

            using (var connection = GetConnection())
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Id, Name, Type, Damage FROM Cards", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cards.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3)));
                    }
                }
            }

            return cards;
        }
    }
}
