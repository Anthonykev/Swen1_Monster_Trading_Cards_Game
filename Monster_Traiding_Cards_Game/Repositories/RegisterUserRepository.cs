using Npgsql;
using System;
using System.Text;
using System.Security.Cryptography;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class RegisterUserRepository
    {
        private readonly string _connectionString;

        public RegisterUserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool RegisterUser(string username, string password, string fullName, string email)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Users (Username, Password, FullName, EMail) VALUES (@username, @password, @fullName, @email)", connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", GetPasswordHash(username, password)); // Passwort wird hier gehasht
                    command.Parameters.AddWithValue("@fullName", fullName);
                    command.Parameters.AddWithValue("@email", email);
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
