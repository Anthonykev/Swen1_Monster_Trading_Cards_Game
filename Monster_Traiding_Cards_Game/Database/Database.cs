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

        public bool CreateTables()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new NpgsqlCommand(@"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id SERIAL PRIMARY KEY,
                            Username VARCHAR(50) UNIQUE NOT NULL,
                            Password VARCHAR(255) NOT NULL,
                            FullName VARCHAR(100),
                            EMail VARCHAR(100),
                            Coins INT DEFAULT 20,
                            SessionToken VARCHAR(255),
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );

                        CREATE TABLE IF NOT EXISTS Cards (
                            Id SERIAL PRIMARY KEY,
                            Name VARCHAR(100) NOT NULL,
                            Type VARCHAR(50) NOT NULL,
                            Damage INT NOT NULL,
                            ElementType VARCHAR(50) NOT NULL,
                            UserId INT REFERENCES Users(Id)
                        );

                        CREATE TABLE IF NOT EXISTS UserStacks (
                            UserId INT REFERENCES Users(Id),
                            CardId INT REFERENCES Cards(Id),
                            PRIMARY KEY (UserId, CardId)
                        );

                        CREATE TABLE IF NOT EXISTS UserDecks (
                            UserId INT REFERENCES Users(Id),
                            CardId INT REFERENCES Cards(Id),
                            PRIMARY KEY (UserId, CardId)
                        );

                        CREATE TABLE IF NOT EXISTS Packages (
                            Id SERIAL PRIMARY KEY,
                            Card1Id INT REFERENCES Cards(Id),
                            Card2Id INT REFERENCES Cards(Id),
                            Card3Id INT REFERENCES Cards(Id),
                            Card4Id INT REFERENCES Cards(Id),
                            Card5Id INT REFERENCES Cards(Id)
                        );

                        CREATE TABLE IF NOT EXISTS Trades (
                            Id SERIAL PRIMARY KEY,
                            UserId INT REFERENCES Users(Id),
                            OfferedCardId INT REFERENCES Cards(Id),
                            RequestedCardType VARCHAR(50),
                            RequestedCardElementType VARCHAR(50)
                        );

                        CREATE TABLE IF NOT EXISTS Battles (
                            Id SERIAL PRIMARY KEY,
                            User1Id INT REFERENCES Users(Id),
                            User2Id INT REFERENCES Users(Id),
                            WinnerId INT REFERENCES Users(Id),
                            LoserId INT REFERENCES Users(Id)
                        );
                    ", connection);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tables: {ex.Message}");
                return false;
            }
        }

        public bool RegisterUser(string username, string password, string fullName, string email)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Users (Username, Password, FullName, EMail) VALUES (@username, @password, @fullName, @email)", connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password); // In einer echten Anwendung sollten Sie das Passwort hashen
                    command.Parameters.AddWithValue("@fullName", fullName);
                    command.Parameters.AddWithValue("@email", email);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user: {ex.Message}");
                return false;
            }
        }

        public (bool Success, string Token) AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT Id, Password FROM Users WHERE Username = @username", connection);
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var storedPassword = reader.GetString(1);
                            if (storedPassword == password) // In einer echten Anwendung sollten Sie das Passwort hashen und vergleichen
                            {
                                var userId = reader.GetInt32(0);
                                var token = Guid.NewGuid().ToString(); // Generieren eines einfachen Tokens
                                UpdateUserToken(userId, token);
                                return (true, token);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error authenticating user: {ex.Message}");
            }
            return (false, string.Empty);
        }

        private void UpdateUserToken(int userId, string token)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new NpgsqlCommand("UPDATE Users SET SessionToken = @token WHERE Id = @userId", connection);
                    command.Parameters.AddWithValue("@token", token);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user token: {ex.Message}");
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

