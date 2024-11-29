using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using FHTW.Swen1.Swamp.Exceptions;
using FHTW.Swen1.Swamp.Interfaces;
using FHTW.Swen1.Swamp.Network;

namespace FHTW.Swen1.Swamp.Models
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
        private User()
        { }

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
        public List<Card> Stack { get; set; } = new List<Card>();

        /// <summary>Holds the user's deck of selected cards.</summary>
        public List<Card> Deck { get; set; } = new List<Card>();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Available card names for random card creation.</summary>
        private List<string> CardNames = new List<string>
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
            // Check if the user has enough coins to buy a package
            if (Coins < 5)
            {
                Console.WriteLine("Not enough coins to buy a package. You need at least 5 coins.");
                return;
            }

            // Deduct coins for the package
            Coins -= 5;
            Random randNames = new Random();

            // Create five cards and add them to the stack
            for (int i = 0; i < 5; i++)
            {
                // Select a random card name
                string cardName = CardNames[randNames.Next(CardNames.Count)];
                // Create a card based on the name
                Card newCard = CreateCard(cardName);
                // Add the card to the stack
                Stack.Add(newCard);
                Console.WriteLine($"Added new card to stack: {newCard.Name} ({newCard.CardElementType})");
            }
        }

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

        /// <summary>Selects the best cards from the stack to add them to the deck.</summary>
        public void ChooseDeck()
        {
           

            // Sort the stack by Damage (descending), then by Element Type (Water > Fire > Normal), and optionally by Card Type.
            var sortedStack = Stack.OrderByDescending(card => card.Damage)
                .ThenBy(card => card.CardElementType == ElementType.Water ? 1 :
                    card.CardElementType == ElementType.Fire ? 2 : 3) // Water first, then Fire, then Normal
                .ThenByDescending(card => card.GetType().Name) // Optional: prioritize card type
                .ToList();

            // Clear the current deck to ensure it contains only the newest selection
            Deck.Clear();

            // Select the top 4 cards for the deck
            Deck = sortedStack.Take(4).ToList();

            // Print the selected deck
            Console.WriteLine("Selected Deck:");
            foreach (var card in Deck)
            {
                Console.WriteLine($"Card: {card.Name}, Damage: {card.Damage}, Element: {card.CardElementType}, Type: {card.GetType().Name}");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a user.</summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <param name="fullName">Full name.</param>
        /// <param name="eMail">E-mail addresss.</param>
        /// <exception cref="UserException">Thrown when the user name already exists.</exception>
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
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <returns>Returns a tuple of success flag and token.
        ///          If successful, the success flag is TRUE and the token contains a token string,
        ///          otherwise success flag is FALSE and token is empty.</returns>
        public static (bool Success, string Token) Logon(string userName, string password)
        {
            if (_Users.ContainsKey(userName))
            {
                return (true, Token._CreateTokenFor(_Users[userName]));
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