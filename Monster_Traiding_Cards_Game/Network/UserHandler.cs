using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Exceptions;
using Monster_Trading_Cards_Game.Network;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Database; // Aktualisieren Sie den Namespace

namespace Monster_Trading_Cards_Game.Network
{
    public class UserHandler : Handler, IHandler
    {
        private readonly CreateTables _createTables;
        private readonly CardRepository _cardRepository;
        private readonly UserRepository _userRepository;
        private readonly UserStackRepository _userStackRepository;
        private readonly UserDeckRepository _userDeckRepository;
        private readonly PackageRepository _packageRepository;
        private readonly TradeRepository _tradeRepository;
        private readonly BattleRepository _battleRepository;
        private readonly BattleRoundRepository _battleRoundRepository;

        public UserHandler()
        {
            string connectionString = "Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards";
            _createTables = new CreateTables(connectionString);
            _cardRepository = new CardRepository(connectionString);
            _userRepository = new UserRepository(connectionString);
            _userStackRepository = new UserStackRepository(connectionString);
            _userDeckRepository = new UserDeckRepository(connectionString);
            _packageRepository = new PackageRepository(connectionString);
            _tradeRepository = new TradeRepository(connectionString);
            _battleRepository = new BattleRepository(connectionString);
            _battleRoundRepository = new BattleRoundRepository(connectionString);
        }

        public override bool Handle(HttpSvrEventArgs e)
        {
            JsonObject? reply = null;
            int status = HttpStatusCode.BAD_REQUEST;

            if ((e.Path.TrimEnd('/', ' ', '\t') == "/create-tables") && (e.Method == "POST"))
            {
                if (_createTables.Execute_CreateTables())
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
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/add-default-cards") && (e.Method == "POST"))
            {
                if (_cardRepository.AddDefaultCards())
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
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/register") && (e.Method == "POST"))
            {
                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    string? username = json?["username"]?.ToString();
                    string? password = json?["password"]?.ToString();
                    string? fullName = json?["fullname"]?.ToString();
                    string? email = json?["email"]?.ToString();

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Missing required fields."
                        }.ToJsonString());
                        return true;
                    }

                    bool success = _userRepository.CreateUser(username, password, fullName ?? string.Empty, email);

                    if (success)
                    {
                        e.Reply(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "Registration successful."
                        }.ToJsonString());
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Registration failed. User might already exist."
                        }.ToJsonString());
                    }
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject
                    {
                        ["success"] = false,
                        ["message"] = "An unexpected error occurred."
                    }.ToJsonString());
                }
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/login") && (e.Method == "POST"))
            {
                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    string? username = json?["username"]?.ToString();
                    string? password = json?["password"]?.ToString();

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Missing username or password."
                        }.ToJsonString());
                        return true;
                    }

                    var (success, token) = _userRepository.AuthenticateUser(username, password);

                    if (success)
                    {
                        e.Reply(HttpStatusCode.OK, new JsonObject()
                        {
                            ["success"] = true,
                            ["token"] = token,
                            ["message"] = "Login successful."
                        }.ToJsonString());
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, new JsonObject()
                        {
                            ["success"] = false,
                            ["message"] = "Invalid username or password."
                        }.ToJsonString());
                    }
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "An unexpected error occurred."
                    }.ToJsonString());
                }
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/buy-package") && (e.Method == "POST"))
            {
                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    string? token = json?["token"]?.ToString();
                    string? username = json?["username"]?.ToString();

                    if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(username))
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Missing token or username."
                        }.ToJsonString());
                        return true;
                    }

                    Console.WriteLine($"Received request to buy package for user: {username} with token: {token}");

                    User? user = User.GetByUsernameAndToken(username, token);
                    if (user == null)
                    {
                        Console.WriteLine($"User not found or invalid token: {username}");
                        e.Reply(HttpStatusCode.UNAUTHORIZED, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid username or token."
                        }.ToJsonString());
                        return true;
                    }

                    user.AddPackage(token);

                    e.Reply(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Package bought successfully."
                    }.ToJsonString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during package purchase: {ex.Message}");
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject
                    {
                        ["success"] = false,
                        ["message"] = $"An unexpected error occurred: {ex.Message}"
                    }.ToJsonString());
                }
                return true;
            }


            return false;
        }
    }
}
