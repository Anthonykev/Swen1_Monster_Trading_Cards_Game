using Npgsql;
using System;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class CreateTablesRepository
    {
        private readonly string _connectionString;

        public CreateTablesRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool CreateTables()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand(@"
                        DROP TABLE IF EXISTS Cards CASCADE;
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
                            Name VARCHAR(100) UNIQUE NOT NULL,
                            Type VARCHAR(50) NOT NULL,
                            Damage INT NOT NULL,
                            ElementType VARCHAR(50) NOT NULL
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
    }
}
