using Npgsql;
using System;
using System.Text;
using System.Security.Cryptography;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class AuthenticateUserRepository
    {
        private readonly string _connectionString;

        public AuthenticateUserRepository(string connectionString)
        {
            _connectionString = connectionString;
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
    }
}
