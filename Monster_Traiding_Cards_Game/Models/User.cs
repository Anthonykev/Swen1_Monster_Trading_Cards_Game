﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Monster_Trading_Cards_Game.Exceptions;
using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Network;

namespace Monster_Trading_Cards_Game.Models
{
    /// <summary>This class represents a user.</summary>
    public sealed class User
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static members                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Currently holds the system users.</summary>
        /// <remarks>Is to be removed by database implementation later.</remarks>
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

        /// <summary>Holds the user's card stack.</summary>
        public List<Card> Stack { get; set; } = new();

        /// <summary>Holds the user's deck of selected cards.</summary>
        public List<Card> Deck { get; set; } = new();

        /// <summary>Holds the current session token of the user.</summary>
        public string? SessionToken { get; set; } = null;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Available card names for random card creation.</summary>
        private List<string> CardNames = new()
        {
            "Goblins", "Dragons", "Wizzard", "Knights", "Orks", "Kraken", "FireElves", "Lion", "DogMike", "Rocklee",
            "Tetsu", "Amaterasu", "Bankai", "Raijin", "Susanoo", "FighterKevin"
        };

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
                // Save data.
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

            for (int i = 0; i < 5; i++)
            {
                string cardName = CardNames[randNames.Next(CardNames.Count)];
                Stack.Add(CreateCard(cardName));
            }
        }

        /// <summary>Selects the best cards from the stack to add them to the deck.</summary>
        public void ChooseDeck()
        {
            var sortedStack = Stack
                .OrderByDescending(card => card.Damage)
                .ThenBy(card => card.CardElementType == ElementType.Water ? 1 :
                    card.CardElementType == ElementType.Fire ? 2 : 3)
                .ThenByDescending(card => card.GetType().Name) // Optional: prioritize card type
                .ToList();

            Deck.Clear();
            Deck = sortedStack.Take(4).ToList();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private methods                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new card based on the card name.</summary>
        /// <param name="cardName">The name of the card.</param>
        /// <returns>The created card.</returns>
        private Card CreateCard(string cardName)
        {
            if (cardName == "Dragons" || cardName == "FireElves" || cardName == "Kraken" || cardName == "Lion")
            {
                return new MonsterCard(cardName);
            }
            else if (cardName == "Wizzard" || cardName == "Tetsu" || cardName == "Amaterasu" || cardName == "Bankai")
            {
                return new SpellCard(cardName);
            }
            else
            {
                return new NormalCard(cardName);
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
                FullName = fullName,
                EMail = eMail
            };

            _Users.Add(user.UserName, user);
        }

        /// <summary>Performs a user logon.</summary>
        public static (bool Success, string Token) Logon(string userName, string password)
        {
            if (_Users.ContainsKey(userName))
            {
                string token = Token._CreateTokenFor(_Users[userName]);
                _Users[userName].SessionToken = token;
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
            if (_Users.TryGetValue(userName, out User? user))
            {
                return user;
            }
            return null;
        }

        public static IEnumerable<User> GetAllUsers()
        {
            return _Users.Values;
        }
    }
}