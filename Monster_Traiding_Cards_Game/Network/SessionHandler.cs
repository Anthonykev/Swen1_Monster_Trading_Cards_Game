using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Network
{
    public class SessionHandler : Handler, IHandler
    {
        private static Dictionary<string, string> ActiveSessions = new();
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public SessionHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {
                Console.WriteLine("⚠ Fehler: SessionHandler hat keinen ConnectionString erhalten!");
            }
            else
            {
                Console.WriteLine($"✅ SessionHandler ConnectionString: {_connectionString}");
            }
        }

        public void Initialize()
        {
            LoadActiveSessionsFromDatabase();
        }

        public override bool Handle(HttpSvrEventArgs e)
        {
            if ((e.Path.TrimEnd('/', ' ', '\t') == "/sessions") && (e.Method == "POST"))
            {
                JsonObject? reply = null;
                int status = HttpStatusCode.BAD_REQUEST;

                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    if (json != null)
                    {
                        string username = (string)json["username"]!;
                        string password = (string)json["password"]!;
                        (bool Success, string Token) result = User.Logon(username, password, _configuration);

                        if (result.Success)
                        {
                            ActiveSessions[result.Token] = username;
                            status = HttpStatusCode.OK;
                            reply = new JsonObject()
                            {
                                ["success"] = true,
                                ["message"] = "User logged in.",
                                ["token"] = result.Token
                            };

                            var headers = new Dictionary<string, string>
                            {
                                { "Authorization", $"Bearer {result.Token}" }
                            };
                            e.Reply(status, reply?.ToJsonString(), headers);
                            return true;
                        }
                        else
                        {
                            status = HttpStatusCode.UNAUTHORIZED;
                            reply = new JsonObject()
                            {
                                ["success"] = false,
                                ["message"] = "Logon failed."
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Invalid request."
                    };
                    Console.WriteLine($"Error handling login request: {ex.Message}");
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/sessions/logout") && (e.Method == "POST"))
            {
                JsonObject? reply = null;
                int status = HttpStatusCode.BAD_REQUEST;

                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    if (json != null)
                    {
                        string token = (string)json["token"]!;

                        // Prüfen, ob der Token existiert
                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            connection.Open();
                            var command = new NpgsqlCommand("SELECT Username FROM Users WHERE SessionToken = @token", connection);
                            command.Parameters.AddWithValue("@token", token);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string username = reader.GetString(0);
                                    UpdateUserTokenInDatabase(token, null);

                                    status = HttpStatusCode.OK;
                                    reply = new JsonObject()
                                    {
                                        ["success"] = true,
                                        ["message"] = "User logged out."
                                    };
                                    ActiveSessions.Remove(token);
                                }
                                else
                                {
                                    status = HttpStatusCode.UNAUTHORIZED;
                                    reply = new JsonObject()
                                    {
                                        ["success"] = false,
                                        ["message"] = "Invalid token."
                                    };
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Invalid request."
                    };
                    Console.WriteLine($"Error handling logout request: {ex.Message}");
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }

            return false;
        }

        public void LogoutAllUsers()
        {
            int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            var command = new NpgsqlCommand("UPDATE Users SET SessionToken = NULL WHERE SessionToken IS NOT NULL", connection, transaction);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                    ActiveSessions.Clear();
                    Console.WriteLine("All users have been logged out.");
                    break;
                }
                catch (Exception ex)
                {
                    retryCount--;
                    Console.WriteLine($"Error logging out all users: {ex.Message}. Retries left: {retryCount}");
                    if (retryCount == 0)
                    {
                        throw;
                    }
                }
            }
        }

        private void LoadActiveSessionsFromDatabase()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT Username, SessionToken FROM Users WHERE SessionToken IS NOT NULL", connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string username = reader.GetString(0);
                            string token = reader.GetString(1);
                            ActiveSessions[token] = username;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading active sessions from database: {ex.Message}");
            }
        }

        private void UpdateUserTokenInDatabase(string token, string? newToken)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("UPDATE Users SET SessionToken = @newToken WHERE SessionToken = @token", connection);
                    command.Parameters.AddWithValue("@newToken", (object?)newToken ?? DBNull.Value);
                    command.Parameters.AddWithValue("@token", token);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user token in database: {ex.Message}");
            }
        }
    }
}


