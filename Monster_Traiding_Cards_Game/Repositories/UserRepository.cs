using Npgsql;
using System;
using System.Text;
using System.Security.Cryptography;
using Monster_Trading_Cards_Game.Models;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool CreateUser(string username, string password, string fullName, string email)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Users (Username, Password, FullName, EMail, Coins, Elo, Wins, Losses, TotalGames) VALUES (@username, @password, @fullName, @Email, @coins, @elo, @wins, @losses, @totalgames)", connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", GetPasswordHash(username, password)); // Passwort wird hier gehasht
                    command.Parameters.AddWithValue("@fullName", fullName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@coins", 20);
                    command.Parameters.AddWithValue("@elo", 100);
                    command.Parameters.AddWithValue("@wins", 0);
                    command.Parameters.AddWithValue("@losses", 0);
                    command.Parameters.AddWithValue("@totalgames", 0);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                Console.WriteLine($"Duplicate username detected: {username}. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
        }

        public (bool Success, string Token) AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT Id, Password FROM Users WHERE Username = @username", connection);
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var storedPassword = reader.GetString(1);
                            if (storedPassword == GetPasswordHash(username, password)) // Passwort wird hier verifiziert
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
                using (var connection = new NpgsqlConnection(_connectionString))
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

        private string GetPasswordHash(string username, string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] buf = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(username + password));

                StringBuilder rval = new StringBuilder();
                foreach (byte b in buf) { rval.Append(b.ToString("x2")); }

                return rval.ToString();
            }
        }

        public void SaveToDatabase(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(@"
                INSERT INTO Users (UserName, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames)
                VALUES (@username, @fullname, @email, @coins, @password, @sessiontoken, @elo, @wins, @losses, @totalgames)
                ON CONFLICT (UserName) DO UPDATE
                SET FullName = @fullname, EMail = @email, Coins = @coins, Password = @password, SessionToken = @sessiontoken, Elo = @elo, Wins = @wins, Losses = @losses, TotalGames = @totalgames", connection);
                command.Parameters.AddWithValue("@username", user.UserName);
                command.Parameters.AddWithValue("@fullname", user.FullName);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@email", user.EMail);
                command.Parameters.AddWithValue("@coins", user.Coins);
                command.Parameters.AddWithValue("@sessiontoken", user.SessionToken ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@elo", user.Elo);
                command.Parameters.AddWithValue("@wins", user.Wins);
                command.Parameters.AddWithValue("@losses", user.Losses);
                command.Parameters.AddWithValue("@totalgames", user.TotalGames);
                command.ExecuteNonQuery();

                // Save the user's stack to the database
                foreach (var card in user.Stack)
                {
                    var cardCommand = new NpgsqlCommand(@"
                    INSERT INTO UserStacks (UserId, CardId)
                    VALUES ((SELECT Id FROM Users WHERE UserName = @username), @cardid)
                    ON CONFLICT (UserId, CardId) DO NOTHING", connection);
                    cardCommand.Parameters.AddWithValue("@username", user.UserName);
                    cardCommand.Parameters.AddWithValue("@cardid", card.Id);
                    cardCommand.ExecuteNonQuery();
                }

                // Save the user's deck to the database
                foreach (var card in user.Deck)
                {
                    var cardCommand = new NpgsqlCommand(@"
                    INSERT INTO UserDecks (UserId, CardId)
                    VALUES ((SELECT Id FROM Users WHERE UserName = @username), @cardid)
                    ON CONFLICT (UserId, CardId) DO NOTHING", connection);
                    cardCommand.Parameters.AddWithValue("@username", user.UserName);
                    cardCommand.Parameters.AddWithValue("@cardid", card.Id);
                    cardCommand.ExecuteNonQuery();
                }
            }
        }

        public void ClearDeckInDatabase(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(@"
                DELETE FROM UserDecks
                WHERE UserId = (SELECT Id FROM Users WHERE UserName = @username)", connection);
                command.Parameters.AddWithValue("@username", username);
                command.ExecuteNonQuery();
            }
        }

        public List<string> GetCardNamesFromDatabase()
        {
            List<string> cardNames = new();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Name FROM Cards", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cardNames.Add(reader.GetString(0));
                    }
                }
            }
            return cardNames;
        }

        public Card CreateCard(string cardName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Id, Name, Damage, ElementType, Type FROM Cards WHERE Name = @name", connection);
                command.Parameters.AddWithValue("@name", cardName);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        int damage = reader.GetInt32(2);
                        ElementType elementType = Enum.Parse<ElementType>(reader.GetString(3));
                        string type = reader.GetString(4);

                        if (type == "Monster-Card")
                        {
                            return new MonsterCard(id, name, damage, elementType);
                        }
                        else if (type == "Spell-Card")
                        {
                            return new SpellCard(id, name, damage, elementType);
                        }
                        else
                        {
                            throw new Exception("Unknown card type");
                        }
                    }
                }
            }
            throw new Exception("Card not found in database");
        }
    }

}
