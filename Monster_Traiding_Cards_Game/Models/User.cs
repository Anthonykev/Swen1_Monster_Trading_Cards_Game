﻿using System;
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

namespace Monster_Trading_Cards_Game.Models
{
    /// <summary>This class represents a user.</summary>
    public sealed class User
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static members                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Currently holds the system users.</summary>
        private static Dictionary<string, User> _Users = new();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        private User() { }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the user name.</summary>
        public string UserName { get; private set; } = string.Empty;

        /// <summary>Gets or sets the user's full name.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's e-mail address.</summary>
        public string EMail { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's coins.</summary>
        public int Coins { get; set; } = 20;

        /// <summary>Gets or sets the user's password.</summary>
        public string Password { get; private set; } = string.Empty;

        /// <summary>Holds the user's card stack.</summary>
        public List<Card> Stack { get; set; } = new();

        /// <summary>Holds the user's deck of selected cards.</summary>
        public List<Card> Deck { get; set; } = new();

        /// <summary>Holds the current session token of the user.</summary>
        public string? SessionToken { get; set; } = null;

        /// <summary>Gets or sets the user's Elo rating.</summary>
        public int Elo { get; set; } = 100;

        /// <summary>Gets or sets the user's number of wins.</summary>
        public int Wins { get; set; } = 0;

        /// <summary>Gets or sets the user's number of losses.</summary>
        public int Losses { get; set; } = 0;

        /// <summary>Gets or sets the user's total number of games.</summary>
        public int TotalGames { get; set; } = 0;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Saves changes to the user object.</summary>
        /// <param name="token">Token of the session trying to modify the object.</param>
        /// <exception cref="SecurityException">Thrown in case of an unauthorized attempt to modify data.</exception>
        /// <exception cref="AuthenticationException">Thrown when the token is invalid.</exception>
        public void Save(string token)
        {
            (bool Success, User? User) auth = Token.Authenticate(token);
            if (auth.Success)
            {
                if (auth.User!.UserName != UserName)
                {
                    throw new SecurityException("Trying to change other user's data.");
                }
                // Save data to database
                SaveToDatabase();
            }
            else
            {
                throw new AuthenticationException("Not authenticated.");
            }
        }

        /// <summary>Allows the user to buy a package of 5 cards.</summary>
        public void AddPackage()
        {
            if (Coins < 5)
            {
                throw new Exception("Not enough coins to buy a package. You need at least 5 coins.");
            }

            Coins -= 5;
            Random randNames = new();
            List<string> cardNames = GetCardNamesFromDatabase();

            for (int i = 0; i < 5; i++)
            {
                string cardName = cardNames[randNames.Next(cardNames.Count)];
                Stack.Add(CreateCard(cardName));
            }

            // Save changes to database
            SaveToDatabase();
        }

        /// <summary>Selects the best cards from the stack to add them to the deck.</summary>
        public void ChooseDeck()
        {
            // Clear the current deck in the database
            ClearDeckInDatabase();

            // Select the best cards from the stack
            var sortedStack = Stack
                .OrderByDescending(card => card.Damage)
                .ThenBy(card => card.CardElementType == ElementType.Water ? 1 :
                    card.CardElementType == ElementType.Fire ? 2 : 3)
                .ThenByDescending(card => card.GetType().Name) // Optional: prioritize card type
                .ToList();

            // Add the selected cards to the deck
            Deck = sortedStack.Take(4).ToList();

            // Save changes to database
            SaveToDatabase();
        }

        /// <summary>Returns all cards from the deck to the stack.</summary>
        public void ReturnDeckToStack()
        {
            Stack.AddRange(Deck);
            Deck.Clear();
            SaveToDatabase();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private methods                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new card based on the card name.</summary>
        /// <param name="cardName">The name of the card.</param>
        /// <returns>The created card.</returns>
        private Card CreateCard(string cardName)
        {
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
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

        /// <summary>Gets the available card names from the database.</summary>
        /// <returns>A list of card names.</returns>
        private List<string> GetCardNamesFromDatabase()
        {
            List<string> cardNames = new();
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
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

        /// <summary>Clears the user's deck in the database.</summary>
        private void ClearDeckInDatabase()
        {
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
            {
                connection.Open();
                var command = new NpgsqlCommand(@"
                    DELETE FROM UserDecks
                    WHERE UserId = (SELECT Id FROM Users WHERE UserName = @username)", connection);
                command.Parameters.AddWithValue("@username", UserName);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>Saves the user data to the database.</summary>
        private void SaveToDatabase()
        {
            using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards"))
            {
                connection.Open();
                var command = new NpgsqlCommand(@"
                    INSERT INTO Users (UserName, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames)
                    VALUES (@username, @fullname, @email, @coins, @password, @sessiontoken, @elo, @wins, @losses, @totalgames)
                    ON CONFLICT (UserName) DO UPDATE
                    SET FullName = @fullname, EMail = @email, Coins = @coins, Password = @password, SessionToken = @sessiontoken, Elo = @elo, Wins = @wins, Losses = @losses, TotalGames = @totalgames", connection);
                command.Parameters.AddWithValue("@username", UserName);
                command.Parameters.AddWithValue("@fullname", FullName);
                command.Parameters.AddWithValue("@password", Password); 
                command.Parameters.AddWithValue("@email", EMail);
                command.Parameters.AddWithValue("@coins", Coins);
                command.Parameters.AddWithValue("@sessiontoken", SessionToken ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@elo", Elo);
                command.Parameters.AddWithValue("@wins", Wins);
                command.Parameters.AddWithValue("@losses", Losses);
                command.Parameters.AddWithValue("@totalgames", TotalGames);
                command.ExecuteNonQuery();

                // Save the user's stack to the database
                foreach (var card in Stack)
                {
                    var cardCommand = new NpgsqlCommand(@"
                        INSERT INTO UserStacks (UserId, CardId)
                        VALUES ((SELECT Id FROM Users WHERE UserName = @username), @cardid)
                        ON CONFLICT (UserId, CardId) DO NOTHING", connection);
                    cardCommand.Parameters.AddWithValue("@username", UserName);
                    cardCommand.Parameters.AddWithValue("@cardid", card.Id);
                    cardCommand.ExecuteNonQuery();
                }

                // Save the user's deck to the database
                foreach (var card in Deck)
                {
                    var cardCommand = new NpgsqlCommand(@"
                        INSERT INTO UserDecks (UserId, CardId)
                        VALUES ((SELECT Id FROM Users WHERE UserName = @username), @cardid)
                        ON CONFLICT (UserId, CardId) DO NOTHING", connection);
                    cardCommand.Parameters.AddWithValue("@username", UserName);
                    cardCommand.Parameters.AddWithValue("@cardid", card.Id);
                    cardCommand.ExecuteNonQuery();
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a user.</summary>
        public static void Create(string userName, string password, string fullName = "", string eMail = "")
        {
            if (_Users.ContainsKey(userName))
            {
                throw new UserException("User name already exists.");
            }

            User user = new()
            {
                UserName = userName,
                Password = HashPassword(password), // Passwort hashen
                FullName = fullName,
                EMail = eMail,
                Elo = 100 // Initial Elo rating
            };

            _Users.Add(user.UserName, user);

            // Save user to database
            user.SaveToDatabase();
        }

        /// <summary>Performs a user logon.</summary>

        public static (bool Success, string Token) Logon(string userName, string password)
        {
            if (_Users.ContainsKey(userName) && VerifyPassword(password, _Users[userName].Password)) // Passwort verifizieren
            {
                string token = Token._CreateTokenFor(_Users[userName]);
                _Users[userName].SessionToken = token;
                _Users[userName].SaveToDatabase();
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
                var command = new NpgsqlCommand("SELECT Username, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames FROM Users WHERE Username = @username", connection);
                command.Parameters.AddWithValue("@username", userName);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            UserName = reader.GetString(0),
                            FullName = reader.GetString(1),
                            EMail = reader.GetString(2),
                            Coins = reader.GetInt32(3),
                            Password = reader.GetString(4),
                            SessionToken = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Elo = reader.GetInt32(6),
                            Wins = reader.GetInt32(7),
                            Losses = reader.GetInt32(8),
                            TotalGames = reader.GetInt32(9)
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Hashes a password using SHA256.</summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password.</returns>
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>Verifies a password against a hashed password.</summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>True if the password matches the hashed password, otherwise false.</returns>
        private static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}
