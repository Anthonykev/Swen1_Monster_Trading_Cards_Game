using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Npgsql;

namespace Monster_Trading_Cards_Game.Network
{
    public class SessionHandler : Handler, IHandler
    {
        private static Dictionary<string, string> ActiveSessions = new();

        public SessionHandler()
        {
            // Entfernen Sie den Aufruf von LoadActiveSessionsFromDatabase aus dem Konstruktor
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
                        (bool Success, string Token) result = User.Logon(username, password);

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
                catch (Exception)
                {
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Invalid request."
                    };
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
                        using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
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
                catch (Exception)
                {
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Invalid request."
                    };
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }

            return false;
        }

        public static void LogoutAllUsers()
        {
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
            {
                connection.Open();
                var command = new NpgsqlCommand("UPDATE Users SET SessionToken = NULL WHERE SessionToken IS NOT NULL", connection);
                command.ExecuteNonQuery();
            }
            ActiveSessions.Clear();
        }

        private static void LoadActiveSessionsFromDatabase()
        {
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
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

        private static void UpdateUserTokenInDatabase(string token, string? newToken)
        {
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
            {
                connection.Open();
                var command = new NpgsqlCommand("UPDATE Users SET SessionToken = @newToken WHERE SessionToken = @token", connection);
                command.Parameters.AddWithValue("@newToken", (object?)newToken ?? DBNull.Value);
                command.Parameters.AddWithValue("@token", token);
                command.ExecuteNonQuery();
            }
        }
    }
}

