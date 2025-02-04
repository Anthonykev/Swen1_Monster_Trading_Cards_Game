﻿using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Exceptions;
using Monster_Trading_Cards_Game.Network;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Database;
using Microsoft.Extensions.Configuration;

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
        private readonly IConfiguration _configuration;

        public UserHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            _createTables = new CreateTables(configuration);
            _cardRepository = new CardRepository(configuration);
            _userRepository = new UserRepository(configuration);
            _userStackRepository = new UserStackRepository(configuration);
            _userDeckRepository = new UserDeckRepository(configuration);
            _packageRepository = new PackageRepository(configuration);
            //_tradeRepository = new TradeRepository(configuration);
            _battleRepository = new BattleRepository(configuration);
            _battleRoundRepository = new BattleRoundRepository(configuration);
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

                    User? user = User.GetByUsernameAndToken(username, token, _configuration);
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

                    user.AddPackage(username, token);

                    e.Reply(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Package bought successfully."
                    }.ToJsonString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during package purchase: {ex.Message}");
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = $"An unexpected error occurred: {ex.Message}"
                    }.ToJsonString());
                }
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/seed-users") && (e.Method == "POST"))
            {
                try
                {
                    SeedUsers.Seed(_userRepository);
                    status = HttpStatusCode.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["message"] = "Benutzer mit festen Tokens wurden erstellt."
                    };
                }
                catch (Exception ex)
                {
                    status = HttpStatusCode.INTERNAL_SERVER_ERROR;
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = $"Fehler beim Erstellen der Benutzer: {ex.Message}"
                    };
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/battle-request") && (e.Method == "POST"))
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

                    User? user = User.GetByUsernameAndToken(username, token, _configuration);
                    if (user == null)
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid username or token."
                        }.ToJsonString());
                        return true;
                    }

                    var lobbyRepository = new LobbyRepository(_configuration);
                    if (lobbyRepository.AddUserToLobby(username, token))
                    {
                        e.Reply(HttpStatusCode.OK, new JsonObject
                        {
                            ["success"] = true,
                            ["message"] = "User added to lobby successfully."
                        }.ToJsonString());
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "User is already in the lobby."
                        }.ToJsonString());
                    }
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = $"An unexpected error occurred: {ex.Message}"
                    }.ToJsonString());
                }
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/get-user-cards") && (e.Method == "POST"))
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
                            ["message"] = "Missing username or token."
                        }.ToJsonString());
                        return true;
                    }

                    User? user = User.GetByUsernameAndToken(username, token, _configuration);
                    if (user == null)
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid username or token."
                        }.ToJsonString());
                        return true;
                    }

                    var userStackRepository = new UserStackRepository(_configuration);
                    var cardRepository = new CardRepository(_configuration);

                    // Alle Karten-IDs des Benutzers holen
                    var userCardIds = userStackRepository.GetUserStack(user.Id);

                    // Details der Karten aus der Cards-Tabelle holen
                    var userCards = new List<object>();
                    foreach (var cardId in userCardIds)
                    {
                        var card = cardRepository.GetCardById(cardId);
                        if (card != null)
                        {
                            userCards.Add(new
                            {
                                CardId = card.Id,
                                Name = card.Name,
                                Damage = card.Damage,
                                ElementType = card.CardElementType.ToString()
                            });
                        }
                    }

                    e.Reply(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["cards"] = JsonSerializer.Serialize(userCards)
                    }.ToJsonString());
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject
                    {
                        ["success"] = false,
                        ["message"] = $"An unexpected error occurred: {ex.Message}"
                    }.ToJsonString());
                }
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/choose-deck") && (e.Method == "POST"))
            {
                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    string? token = json?["token"]?.ToString();
                    string? username = json?["username"]?.ToString();
                    JsonArray? cardIdsArray = json?["cardIds"]?.AsArray();

                    if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(username) || cardIdsArray == null || cardIdsArray.Count != 4)
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid request. Provide username, token, and exactly 4 card IDs."
                        }.ToJsonString());
                        return true;
                    }

                    List<int> cardIds = cardIdsArray.Select(c => c.GetValue<int>()).ToList();

                    User? user = User.GetByUsernameAndToken(username, token, _configuration);
                    if (user == null)
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid username or token."
                        }.ToJsonString());
                        return true;
                    }

                    user.ChooseDeck(username, token, cardIds);

                    e.Reply(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Deck updated successfully."
                    }.ToJsonString());
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject
                    {
                        ["success"] = false,
                        ["message"] = $"An unexpected error occurred: {ex.Message}"
                    }.ToJsonString());
                }
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/get-elo-ranking") && (e.Method == "GET"))
            {
                try
                {
                    var users = User.GetUsersSortedByElo(_configuration);
                    var userRankings = users.Select(user => new
                    {
                        user.UserName,
                        user.Elo
                    });

                    // Ausgabe der ELO-Rangliste auf dem Server
                    Console.WriteLine("ELO-Rangliste:");
                    foreach (var user in userRankings)
                    {
                        Console.WriteLine($"Benutzername: {user.UserName}, ELO: {user.Elo}");
                    }

                    e.Reply(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["rankings"] = JsonSerializer.Serialize(userRankings)
                    }.ToJsonString());
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject
                    {
                        ["success"] = false,
                        ["message"] = $"An unexpected error occurred: {ex.Message}"
                    }.ToJsonString());
                }
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/set-motto") && (e.Method == "POST"))
            {
                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    string? token = json?["token"]?.ToString();
                    string? username = json?["username"]?.ToString();
                    string? motto = json?["motto"]?.ToString();

                    if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(motto))
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Missing token, username, or motto."
                        }.ToJsonString());
                        return true;
                    }

                    User? user = User.GetByUsernameAndToken(username, token, _configuration);
                    if (user == null)
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, new JsonObject
                        {
                            ["success"] = false,
                            ["message"] = "Invalid username or token."
                        }.ToJsonString());
                        return true;
                    }

                    user.Motto = motto;
                    user.Save(username, token);

                    e.Reply(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Motto updated successfully."
                    }.ToJsonString());
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, new JsonObject
                    {
                        ["success"] = false,
                        ["message"] = $"An unexpected error occurred: {ex.Message}"
                    }.ToJsonString());
                }
                return true;
            }
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/get-all-users-mottos") && (e.Method == "GET"))
            {
                try
                {
                    // Benutzer und deren Mottos abrufen
                    var usersWithMottos = User.GetAllUsersWithMottos(_configuration).ToList();

                    // Ausgabe der Benutzer und deren Mottos auf dem Server
                    Console.WriteLine("Benutzer und deren Mottos:");
                    foreach (var user in usersWithMottos)
                    {
                        Console.WriteLine($"Benutzername: {user.GetType().GetProperty("UserName")?.GetValue(user)}, Motto: {user.GetType().GetProperty("Motto")?.GetValue(user)}");
                    }

                    e.Reply(HttpStatusCode.OK, new JsonObject
                    {
                        ["success"] = true,
                        ["message"] = "Benutzer und deren Mottos wurden ausgegeben.",
                        ["users"] = JsonSerializer.Serialize(usersWithMottos)
                    }.ToJsonString());
                }
                catch (Exception ex)
                {
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

