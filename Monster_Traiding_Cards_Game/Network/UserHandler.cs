using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Exceptions;
using Monster_Trading_Cards_Game.Network;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Monster_Traiding_Cards.Database;

namespace Monster_Trading_Cards_Game.Network
{
    /// <summary>This class implements a handler for user-specific requests.</summary>
    public class UserHandler : Handler, IHandler
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // [override] Handler                                                                                               //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Handles an incoming HTTP request.</summary>
        /// <param name="e">Event arguments.</param>
        public override bool Handle(HttpSvrEventArgs e)
        {
            JsonObject? reply = null;
            int status = HttpStatusCode.BAD_REQUEST;

            // Tabellen erstellen
            if ((e.Path.TrimEnd('/', ' ', '\t') == "/create-tables") && (e.Method == "POST"))
            {
                Database db = new Database("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                if (db.CreateTables())
                {
                    status = HttpStatusCode.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["message"] = "Tabellen wurden erstellt oder existieren bereits."
                    };
                }
                else
                {
                    status = HttpStatusCode.INTERNAL_SERVER_ERROR;
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Fehler beim Erstellen der Tabellen."
                    };
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }
            // Standardkarten zur Datenbank hinzufügen
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/add-default-cards") && (e.Method == "POST"))
            {
                Database db = new Database("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                if (db.AddDefaultCards())
                {
                    status = HttpStatusCode.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["message"] = "Standardkarten wurden hinzugefügt."
                    };
                }
                else
                {
                    status = HttpStatusCode.INTERNAL_SERVER_ERROR;
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Fehler beim Hinzufügen der Standardkarten."
                    };
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }
            // Benutzer registrieren
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/register") && (e.Method == "POST"))
            {
                try
                {
                    Console.WriteLine($"[Handler] Raw payload: {e.Payload}");

                    JsonNode? json = null;
                    try
                    {
                        json = JsonNode.Parse(e.Payload);
                        Console.WriteLine("[Handler] JSON parsed successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Handler] JSON parsing failed: {ex.Message}");
                        Console.WriteLine($"[Handler] StackTrace: {ex.StackTrace}");
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid JSON format."
                        }.ToJsonString());
                        return true;
                    }

                    string? username = json?["username"]?.ToString();
                    string? password = json?["password"]?.ToString();
                    string? fullName = json?["fullname"]?.ToString();
                    string? email = json?["email"]?.ToString();

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
                    {
                        Console.WriteLine("[Handler] Error: Missing required fields in JSON.");
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Missing required fields."
                        }.ToJsonString());
                        return true;
                    }

                    Console.WriteLine($"[Handler] Parsed JSON: username={username}, password={password}, fullname={fullName}, email={email}");

                    Database db = new Database("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                    bool success = db.RegisterUser(username, password, fullName ?? string.Empty, email);

                    if (success)
                    {
                        Console.WriteLine($"[Handler] User {username} registered successfully.");
                        e.Reply(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "Registration successful."
                        }.ToJsonString());
                    }
                    else
                    {
                        Console.WriteLine($"[Handler] Failed to register user {username}.");
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Registration failed. User might already exist."
                        }.ToJsonString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Handler] Unexpected error: {ex.Message}");
                    Console.WriteLine($"[Handler] StackTrace: {ex.StackTrace}");
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject
                    {
                        ["success"] = false,
                        ["message"] = "An unexpected error occurred."
                    }.ToJsonString());
                }
                return true;
            }
            // Benutzer authentifizieren
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/login") && (e.Method == "POST"))
            {
                try
                {
                    Console.WriteLine($"[Handler] Raw payload: {e.Payload}");

                    JsonNode? json = null;
                    try
                    {
                        json = JsonNode.Parse(e.Payload);
                        Console.WriteLine("[Handler] JSON parsed successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Handler] JSON parsing failed: {ex.Message}");
                        Console.WriteLine($"[Handler] StackTrace: {ex.StackTrace}");
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid JSON format."
                        }.ToJsonString());
                        return true;
                    }

                    string? username = json?["username"]?.ToString();
                    string? password = json?["password"]?.ToString();

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        Console.WriteLine("[Handler] Error: Missing required fields in JSON.");
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Missing username or password."
                        }.ToJsonString());
                        return true;
                    }

                    Console.WriteLine($"[Handler] Parsed JSON: username={username}, password={password}");

                    Database db = new Database("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                    var (success, token) = db.AuthenticateUser(username, password);

                    if (success)
                    {
                        Console.WriteLine($"[Handler] User {username} authenticated successfully.");
                        e.Reply(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["token"] = token,
                            ["message"] = "Login successful."
                        }.ToJsonString());
                    }
                    else
                    {
                        Console.WriteLine($"[Handler] Failed to authenticate user {username}.");
                        e.Reply(HttpStatusCode.UNAUTHORIZED, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid username or password."
                        }.ToJsonString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Handler] Unexpected error: {ex.Message}");
                    Console.WriteLine($"[Handler] StackTrace: {ex.StackTrace}");
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject
                    {
                        ["success"] = false,
                        ["message"] = "An unexpected error occurred."
                    }.ToJsonString());
                }
                return true;
            }

            return false;
        }
    }
}
