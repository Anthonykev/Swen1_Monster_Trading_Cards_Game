using Npgsql;
using System;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class LobbyRepository
    {
        private readonly string _connectionString;

        public LobbyRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool AddUserToLobby(string username, string token)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Lobby (UserId) SELECT Id FROM Users WHERE Username = @username AND SessionToken = @token", connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@token", token);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                Console.WriteLine($"User already in lobby: {username}. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user to lobby: {ex.Message}");
                return false;
            }
        }

        public bool RemoveUserFromLobby(string username)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("DELETE FROM Lobby WHERE UserId = (SELECT Id FROM Users WHERE Username = @username)", connection);
                    command.Parameters.AddWithValue("@username", username);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing user from lobby: {ex.Message}");
                return false;
            }
        }

        public bool IsUserInLobby(string username)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT COUNT(*) FROM Lobby WHERE UserId = (SELECT Id FROM Users WHERE Username = @username)", connection);
                    command.Parameters.AddWithValue("@username", username);
                    var count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if user is in lobby: {ex.Message}");
                return false;
            }
        }

        public void ClearLobby()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("DELETE FROM Lobby", connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing lobby: {ex.Message}");
            }
        }
    }
}

