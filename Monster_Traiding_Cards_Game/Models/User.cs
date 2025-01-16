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
                new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").SaveToDatabase(this);
            }
            else
            {
                throw new AuthenticationException("Not authenticated.");
            }
        }

        /// <summary>Checks if the user is authenticated.</summary>
        /// <param name="token">Token of the session trying to perform the action.</param>
        /// <returns>True if the user is authenticated, otherwise false.</returns>
        public bool IsAuthenticated(string token)
        {
            (bool Success, User? User) auth = Token.Authenticate(token);
            return auth.Success && auth.User!.UserName == UserName;
        }

        /// <summary>Allows the user to buy a package of 5 cards.</summary>
        public void AddPackage(string token)
        {
            if (!IsAuthenticated(token))
            {
                throw new AuthenticationException("User is not authenticated.");
            }

            if (Coins < 5)
            {
                throw new Exception("Not enough coins to buy a package. You need at least 5 coins.");
            }

            Coins -= 5;
            Random randNames = new();
            CardRepository cardRepository = new CardRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
            List<string> cardNames = cardRepository.GetCardNamesFromDatabase();

            for (int i = 0; i < 5; i++)
            {
                string cardName = cardNames[randNames.Next(cardNames.Count)];
                Stack.Add(cardRepository.CreateCard(cardName));
            }

            // Save changes to database
            new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").SaveToDatabase(this);
        }

        /// <summary>Selects the best cards from the stack to add them to the deck.</summary>
        public void ChooseDeck()
        {
            // Clear the current deck in the database
            new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").ClearDeckInDatabase(UserName);

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
            new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").SaveToDatabase(this);
        }

        /// <summary>Returns all cards from the deck to the stack.</summary>
        public void ReturnDeckToStack()
        {
            Stack.AddRange(Deck);
            Deck.Clear();
            new UserRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards").SaveToDatabase(this);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Performs a user logon.</summary>
        public static (bool Success, string Token) Logon(string userName, string password)
        {
            if (_Users.ContainsKey(userName) && VerifyPassword(password, _Users[userName].Password)) // Passwort verifizieren
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
