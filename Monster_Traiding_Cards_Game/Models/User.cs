using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Monster_Trading_Cards_Game.Exceptions;
using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Network;
using Npgsql;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Database;

namespace Monster_Trading_Cards_Game.Models
{
    public sealed class User
    {
        private static Dictionary<string, User> _Users = new();

        private User() { }

        public int Id { get; private set; }
        public string UserName { get; private set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string EMail { get; set; } = string.Empty;
        public int Coins { get; set; } = 20;
        public string Password { get; private set; } = string.Empty;
        public List<Card> Stack { get; set; } = new();
        public List<Card> Deck { get; set; } = new();
        public string? SessionToken { get; set; } = null;
        public int Elo { get; set; } = 100;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int TotalGames { get; set; } = 0;

        public void Save(string token)
        {
            (bool Success, User? User) auth = Token.Authenticate(token);
            if (auth.Success)
            {
                if (auth.User!.UserName != UserName)
                {
                    throw new SecurityException("Trying to change other user's data.");
                }
                new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").SaveToDatabase(this);
            }
            else
            {
                throw new AuthenticationException("Not authenticated.");
            }
        }

        public bool IsAuthenticated(string username, string token)
        {
            return UserName == username && SessionToken == token;
        }

        public void AddPackage(string username, string token)
        {
            if (!IsAuthenticated(username, token))
            {
                throw new AuthenticationException("User is not authenticated.");
            }

            if (Coins < 5)
            {
                throw new Exception("Not enough coins to buy a package. You need at least 5 coins.");
            }

            Coins -= 5;
            CardRepository cardRepository = new CardRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
            PackageRepository packageRepository = new PackageRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
            UserStackRepository userStackRepository = new UserStackRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");

            if (!packageRepository.ArePackagesAvailable())
            {
                packageRepository.CreateRandomPackages(10);
            }

            var package = packageRepository.GetPackage();
            if (package == null)
            {
                throw new Exception("No packages available.");
            }

            // Sicherstellen, dass das Paket genau 5 Karten enthält
            if (package.Value.CardIds.Count != 5)
            {
                throw new Exception("Package does not contain exactly 5 cards.");
            }

            Console.WriteLine($"Adding package with {package.Value.CardIds.Count} cards to user {username}");

            foreach (var cardId in package.Value.CardIds)
            {
                var card = cardRepository.GetCardById(cardId);
                if (card != null)
                {
                    Stack.Add(card);
                    userStackRepository.AddCardToUserStack(Id, cardId);
                    Console.WriteLine($"Added card {cardId} to user {username}'s stack");
                }
            }

            packageRepository.DeletePackage(package.Value.PackageId);

            if (packageRepository.GetPackageCount() <= 5)
            {
                packageRepository.CreateRandomPackages(5);
            }

            // Speichere nur die Benutzeränderungen
            new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").SaveToDatabase(this);
            Console.WriteLine("Package added successfully.");
        }


        public void ChooseDeck(string username, string token, List<int> cardIds)
        {
            if (!IsAuthenticated(username, token))
            {
                throw new AuthenticationException("User is not authenticated.");
            }

            if (cardIds.Count != 4)
            {
                throw new ArgumentException("You must select exactly 4 cards for the deck.");
            }

            var userStackRepository = new UserStackRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
            var userDeckRepository = new UserDeckRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
            var cardRepository = new CardRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");

            // Alle Karten-IDs des Benutzers abrufen
            var userCardIds = userStackRepository.GetUserStack(Id);

            // Prüfen, ob die ausgewählten Karten zur Sammlung des Benutzers gehören
            if (!cardIds.All(cardId => userCardIds.Contains(cardId)))
            {
                throw new InvalidOperationException("One or more selected cards do not belong to the user.");
            }

            // Bestehendes Deck löschen
            userDeckRepository.ClearUserDeck(Id);

            // Neues Deck speichern
            foreach (var cardId in cardIds)
            {
                userDeckRepository.AddCardToUserDeck(Id, cardId);
            }

            // Karteninformationen für das neue Deck laden
            Deck = cardIds.Select(cardId => cardRepository.GetCardById(cardId)).Where(card => card != null).ToList();
        }












        public void ReturnDeckToStack(string username, string token)
        {
            if (!IsAuthenticated(username, token))
            {
                throw new AuthenticationException("User is not authenticated.");
            }

            Stack.AddRange(Deck);
            Deck.Clear();
            new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").SaveToDatabase(this);
        }

        public static (bool Success, string Token) Logon(string userName, string password)
        {
            if (_Users.ContainsKey(userName) && VerifyPassword(password, _Users[userName].Password))
            {
                string token = Token._CreateTokenFor(_Users[userName]);
                _Users[userName].SessionToken = token;
                new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").SaveToDatabase(_Users[userName]);
                return (true, token);
            }

            return (false, string.Empty);
        }

        public static bool Exists(string userName)
        {
            return _Users.ContainsKey(userName);
        }

        public static User? Get(string userName)
        {
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Id, Username, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames FROM Users WHERE Username = @username", connection);
                command.Parameters.AddWithValue("@username", userName);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            FullName = reader.GetString(2),
                            EMail = reader.GetString(3),
                            Coins = reader.GetInt32(4),
                            Password = reader.GetString(5),
                            SessionToken = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Elo = reader.GetInt32(7),
                            Wins = reader.GetInt32(8),
                            Losses = reader.GetInt32(9),
                            TotalGames = reader.GetInt32(10)
                        };
                    }
                }
            }
            return null;
        }

        public static User? GetById(int userId)
        {
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Id, Username, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames FROM Users WHERE Id = @userId", connection);
                command.Parameters.AddWithValue("@userId", userId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            FullName = reader.GetString(2),
                            EMail = reader.GetString(3),
                            Coins = reader.GetInt32(4),
                            Password = reader.GetString(5),
                            SessionToken = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Elo = reader.GetInt32(7),
                            Wins = reader.GetInt32(8),
                            Losses = reader.GetInt32(9),
                            TotalGames = reader.GetInt32(10)
                        };
                    }
                }
            }
            return null;
        }

        public static IEnumerable<User> GetAllUsers()
        {
            return _Users.Values;
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        public static User? GetByUsernameAndToken(string username, string token)
        {
            try
            {
                using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT Id, Username, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames FROM Users WHERE Username = @username AND SessionToken = @token", connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@token", token);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                FullName = reader.GetString(2),
                                EMail = reader.GetString(3),
                                Coins = reader.GetInt32(4),
                                Password = reader.GetString(5),
                                SessionToken = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Elo = reader.GetInt32(7),
                                Wins = reader.GetInt32(8),
                                Losses = reader.GetInt32(9),
                                TotalGames = reader.GetInt32(10)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by username and token: {ex.Message}");
            }
            return null;
        }
    }
}
