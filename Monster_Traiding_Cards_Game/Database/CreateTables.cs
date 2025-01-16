using Npgsql;
using System;

namespace Monster_Trading_Cards_Game.Database
{
    public class CreateTables
    {
        private readonly string _connectionString;

        public CreateTables(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool Execute_CreateTables()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
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
                    Elo INT DEFAULT 100,
                    Wins INT DEFAULT 0,
                    Losses INT DEFAULT 0,
                    TotalGames INT DEFAULT 0,
                    SessionToken VARCHAR(255),
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Cards (
                    Id SERIAL PRIMARY KEY,
                    Name VARCHAR(100) UNIQUE NOT NULL,
                    Type VARCHAR(50) NOT NULL,
                    Damage INT NOT NULL,
                    ElementType VARCHAR(50) NOT NULL,
                    Abilities JSONB
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
                    RequestedCardElementType VARCHAR(50),
                    MinimumDamage INT
                );

                CREATE TABLE IF NOT EXISTS Battles (
                    Id SERIAL PRIMARY KEY,
                    User1Id INT REFERENCES Users(Id),
                    User2Id INT REFERENCES Users(Id),
                    WinnerId INT REFERENCES Users(Id),
                    LoserId INT REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS BattleRounds (
                    Id SERIAL PRIMARY KEY,
                    BattleId INT REFERENCES Battles(Id),
                    RoundNumber INT NOT NULL,
                    Player1CardId INT REFERENCES Cards(Id),
                    Player2CardId INT REFERENCES Cards(Id),
                    WinnerId INT REFERENCES Users(Id),
                    LoserId INT REFERENCES Users(Id),
                    Log TEXT
                );
                CREATE TABLE IF NOT EXISTS Lobby (
                    Id SERIAL PRIMARY KEY,
                    UserId INT REFERENCES Users(Id) UNIQUE NOT NULL,
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
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
