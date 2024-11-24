using System;
using FHTW.Swen1.Swamp.Interfaces;
using FHTW.Swen1.Swamp.Models;
using FHTW.Swen1.Swamp.Exceptions;
using FHTW.Swen1.Swamp.Network;


using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security;
using System.Text;
using System.Threading.Tasks;


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
        public string UserName
        {
            get; private set;
        } = string.Empty;


        /// <summary>Gets or sets the user's full name.</summary>
        public string FullName
        {
            get; set;
        } = string.Empty;


        /// <summary>Gets or sets the user's e-mail address.</summary>
        public string EMail
        {
            get; set;
        } = string.Empty;



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
            else { new AuthenticationException("Not authenticated."); }
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
    }
}




















































/*{
    internal class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coins { get; set; } = 20;
        //----------------------------------------------------------------------------------------------

        // Alle Karten des Benutzers werden im Stack gespeichert
        public List<Card> Stack { get; set; } = new List<Card>();

        // Verfügbare Kartennamen zur zufälligen Erstellung der Karten
        private List<string> CardNames = new List<string>()
        {
            "Goblins", "Dragons", "Wizzard", "Knights", "Orks", "Kraken", "FireElves", "Lion", "DogMike", "Rocklee",
            "Tetsu", "Amaterasu", "Bankai", "Raijin", "Susanoo", "FighterKevin"
        };

        //-----------------------------------------------------------------------------------------------


        /// <summary>
        /// Erstellt eine neue Karte basierend auf dem Kartennamen
        /// </summary>
        /// <param name="cardName"></param>
        /// <returns></returns>
        private Card CreateCard(string cardName)
        {
            if (cardName == "Dragons" || cardName == "FireElves" || cardName == "Kraken" || cardName == "Lion") //
            {
                return new MonsterCard(cardName);
            }
            else if (cardName == "Wizzard" || cardName == "Tetsu" || cardName == "Amaterasu" ||
                     cardName == "Bankai") // SpellCards
            {
                return new SpellCard(cardName);
            }
            else
            {
                return new NormalCard(cardName);
            }
        }

        /// <summary>
        /// Der Benutzer kauft ein Pack, das aus 5 Karten besteht. Diese Karten werden dann im Stack gespeichert.
        /// </summary>
        public void AddPackage()
        {
            // Überprüfen, ob der Benutzer genug Coins hat, um ein Pack zu kaufen
            if (Coins < 5)
            {
                Console.WriteLine("Not enough coins to buy a package. You need at least 5 coins.");
                return;
            }

            // Reduzieren der Coins des Benutzers um die Kosten für das Pack
            Coins -= 5;

            Random randNames = new Random();

            // Fünf Karten erstellen und zum Stack hinzufügen
            for (int i = 0; i < 5; i++)
            {
                // Zufällige Auswahl eines Kartennamens
                string cardName = CardNames[randNames.Next(CardNames.Count)];

                // Karte basierend auf dem Namen erstellen
                Card newCard = CreateCard(cardName);

                // Hinzufügen der Karte zum Stack
                Stack.Add(newCard);

                Console.WriteLine($"Added new card to stack: {newCard.Name} ({newCard.CardElementType})");
            }
        }

        /// <summary>
        /// Methode, um die besten Karten aus dem Stack auszuwählen und sie dem Deck hinzuzufügen
        /// </summary>
        public void ChooseDeck()
        {
            // ich muss mir das noch überlegen, noch kein plan
        }

        public void printStack()
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("A list of all stored cards!!!");

            foreach (Card item in Stack)
            {
                Console.WriteLine(item);
            }


        }
    }
}*/