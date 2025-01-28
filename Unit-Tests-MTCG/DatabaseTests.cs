using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Models;
using System;
using System.IO;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Monster_Trading_Cards_Game.Database;

namespace Unit_Tests_MTCG
{
    [TestClass]
    public class DatabaseTests
    {
        private IConfiguration _configuration;
        private UserRepository _userRepository;
        private CardRepository _cardRepository;
        private PackageRepository _packageRepository;
        private UserStackRepository _userStackRepository;
        private UserDeckRepository _userDeckRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();

            // Initialisiere Repositories mit der Default-Datenbank
            _userRepository = new UserRepository(_configuration);
            _cardRepository = new CardRepository(_configuration);
            _packageRepository = new PackageRepository(_configuration);
            _userStackRepository = new UserStackRepository(_configuration);
            _userDeckRepository = new UserDeckRepository(_configuration);

            // Tabellen erstellen und Testdaten hinzufügen
            CreateTables createTables = new CreateTables(_configuration);
            createTables.Execute_CreateTables();
            SeedTestData();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Entferne die Bereinigung, um Daten zu behalten
            //ClearTestData();
        }

        private void SeedTestData()
        {
            // Überprüfen und erstellen, wenn Benutzer nicht existieren
            if (!_userRepository.UserExists("admin"))
            {
                _userRepository.CreateUser("admin", "password123", "Test User 1", "testuser1@example.com");
            }

            if (!_userRepository.UserExists("admin2"))
            {
                _userRepository.CreateUser("admin2", "password123", "Test User 2", "testuser2@example.com");
            }

            // Karten hinzufügen, falls noch nicht vorhanden
            _cardRepository.AddDefaultCards();

            // Pakete vorbereiten
            _packageRepository.CreateRandomPackages(3);
        }

        private void AddTestPackage(string username, string token)
        {
            string defaultConnectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new NpgsqlConnection(defaultConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(@"
                    UPDATE Users 
                    SET Coins = Coins - 5 
                    WHERE Username = @username AND SessionToken = @token AND Coins >= 5;
                ", connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@token", token);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception("Test package purchase failed: Not enough coins or invalid user.");
                }

                Console.WriteLine($"Test package added for user '{username}' in database '{connection.Database}'.");
            }
        }

        private void ClearTestData()
        {
            string defaultConnectionString = _configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine("Clearing test data...");
            using (var connection = new NpgsqlConnection(defaultConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("TRUNCATE Users, Cards, Packages, UserStacks, UserDecks RESTART IDENTITY CASCADE;", connection);
                command.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void TestUserRegistration()
        {
            // Versuche, den Benutzer zu erstellen
            var success = _userRepository.CreateUser("kevin", "password123", "Test User", "kodzo@example.com");

            if (!success)
            {
                // Überprüfen, ob der Benutzer bereits existiert
                var userExists = _userRepository.UserExists("kevin");

                if (userExists)
                {
                    // Benutzer existiert bereits, kein Fehler
                    Console.WriteLine("User already exists.");
                    return; // Test erfolgreich abgeschlossen
                }
                else
                {
                    // Benutzer existiert nicht, aber die Erstellung ist fehlgeschlagen
                    Assert.Fail("User creation failed unexpectedly, and the user does not exist.");
                }
            }

            // Überprüfen, ob der Benutzer erfolgreich erstellt wurde
            var userCreated = _userRepository.UserExists("kevin");
            Assert.IsTrue(userCreated, "The new user should exist in the database.");
        }

        [TestMethod]
        public void TestAddDefaultCards()
        {
            // Füge Standardkarten hinzu
            var success = _cardRepository.AddDefaultCards();
            Assert.IsTrue(success, "Default cards should be added successfully.");

            // Überprüfe, ob Karten in der Datenbank vorhanden sind
            string defaultConnectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new NpgsqlConnection(defaultConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT COUNT(*) FROM Cards", connection);
                int cardCount = Convert.ToInt32(command.ExecuteScalar());
                Assert.IsTrue(cardCount > 0, "There should be default cards in the database.");
            }
        }

        [TestMethod]
        public void TestUserLogin()
        {
            var (success, token) = _userRepository.AuthenticateUser("admin", "password123");
            Assert.IsTrue(success, "User should be able to log in with correct credentials.");
            Assert.IsNotNull(token, "A session token should be generated upon login.");
        }


        [TestMethod]
        public void TestUserLogin2()
        {
            var (success, token) = _userRepository.AuthenticateUser("admin2", "password123");
            Assert.IsTrue(success, "User should be able to log in with correct credentials.");
            Assert.IsNotNull(token, "A session token should be generated upon login.");
        }


        [TestMethod]
        public void TestBuyPackage_admin()
        {
            string token = "fixed-token-1";
            string username = "admin";

            Console.WriteLine($"Starting TestBuyPackage for user '{username}'.");

            // Benutzer authentifizieren
            var (loginSuccess, loginToken) = _userRepository.AuthenticateUser(username, "password123");
            Assert.IsTrue(loginSuccess, "User should be able to log in.");
            Assert.IsNotNull(loginToken, "A session token should be generated.");

            // Benutzer abrufen
            var user = User.Get(username, _configuration);
            Assert.IsNotNull(user, "User should exist.");
            Console.WriteLine($"User '{user.UserName}' retrieved.");

            // **Anzahl der Karten im Stack vor dem Kauf speichern**
            var userCardsBefore = _userStackRepository.GetUserStack(user.Id);
            int initialCardCount = userCardsBefore.Count;
            Console.WriteLine($"User '{username}' has {initialCardCount} cards before buying a package.");

            // **Überprüfen, ob der Benutzer genug Coins hat**
            if (user.Coins < 5)
            {
                Console.WriteLine($"Skipping package purchase test because user '{username}' has only {user.Coins} coins.");
                Assert.Inconclusive("User does not have enough coins to buy a package.");
                return;
            }

            // **Paket kaufen**
            try
            {
                user.AddPackage(username, loginToken);
                Console.WriteLine($"Package successfully added for user '{username}'.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"AddPackage() failed with exception: {ex.Message}");
            }

            // **Anzahl der Karten nach dem Kauf abrufen**
            var userCardsAfter = _userStackRepository.GetUserStack(user.Id);
            int finalCardCount = userCardsAfter.Count;
            Console.WriteLine($"User '{username}' now has {finalCardCount} cards after buying a package.");

            // **Prüfen, ob genau 5 neue Karten hinzugefügt wurden**
            Assert.AreEqual(initialCardCount + 5, finalCardCount, "User's stack should have exactly 5 more cards.");
        }

        [TestMethod]
        public void TestBuyPackage_admin2()
        {
            string token = "fixed-token-2";
            string username = "admin2";

            Console.WriteLine($"Starting TestBuyPackage for user '{username}'.");

            // Benutzer authentifizieren
            var (loginSuccess, loginToken) = _userRepository.AuthenticateUser(username, "password123");
            Assert.IsTrue(loginSuccess, "User should be able to log in.");
            Assert.IsNotNull(loginToken, "A session token should be generated.");

            // Benutzer abrufen
            var user = User.Get(username, _configuration);
            Assert.IsNotNull(user, "User should exist.");
            Console.WriteLine($"User '{user.UserName}' retrieved.");

            // **Anzahl der Karten im Stack vor dem Kauf speichern**
            var userCardsBefore = _userStackRepository.GetUserStack(user.Id);
            int initialCardCount = userCardsBefore.Count;
            Console.WriteLine($"User '{username}' has {initialCardCount} cards before buying a package.");

            // **Überprüfen, ob der Benutzer genug Coins hat**
            if (user.Coins < 5)
            {
                Console.WriteLine($"Skipping package purchase test because user '{username}' has only {user.Coins} coins.");
                Assert.Inconclusive("User does not have enough coins to buy a package.");
                return;
            }

            // **Paket kaufen**
            try
            {
                user.AddPackage(username, loginToken);
                Console.WriteLine($"Package successfully added for user '{username}'.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"AddPackage() failed with exception: {ex.Message}");
            }

            // **Anzahl der Karten nach dem Kauf abrufen**
            var userCardsAfter = _userStackRepository.GetUserStack(user.Id);
            int finalCardCount = userCardsAfter.Count;
            Console.WriteLine($"User '{username}' now has {finalCardCount} cards after buying a package.");

            // **Prüfen, ob genau 5 neue Karten hinzugefügt wurden**
            Assert.AreEqual(initialCardCount + 5, finalCardCount, "User's stack should have exactly 5 more cards.");
        }
        [TestMethod]
        public void TestChooseDeck_admin()
        {
            string token = "fixed-token-1";
            string username = "admin";

            Console.WriteLine($"Starting TestChooseDeck for user '{username}'.");

            // Benutzer authentifizieren
            var (loginSuccess, loginToken) = _userRepository.AuthenticateUser(username, "password123");
            Assert.IsTrue(loginSuccess, "User should be able to log in.");
            Assert.IsNotNull(loginToken, "A session token should be generated.");

            // Benutzer abrufen
            var user = User.Get(username, _configuration);
            Assert.IsNotNull(user, "User should exist.");
            Console.WriteLine($"User '{user.UserName}' retrieved.");

            // **Alle Karten des Benutzers abrufen**
            var userCards = _userStackRepository.GetUserStack(user.Id);
            int totalCards = userCards.Count;
            Console.WriteLine($"User '{username}' has {totalCards} cards in stack before choosing a deck.");

            // **Überprüfen, ob der Benutzer mindestens 4 Karten besitzt**
            if (totalCards < 4)
            {
                Console.WriteLine($"Skipping ChooseDeck test because user '{username}' has only {totalCards} cards.");
                Assert.Inconclusive("User does not have enough cards to choose a deck.");
                return;
            }

            // **Die ersten 4 Karten aus dem Stack für das Deck wählen**
            var selectedCardIds = userCards.Take(4).ToList();

            // **Deck wählen**
            try
            {
                user.ChooseDeck(username, loginToken, selectedCardIds);
                Console.WriteLine($"Deck successfully chosen for user '{username}' with cards: {string.Join(", ", selectedCardIds)}");
            }
            catch (Exception ex)
            {
                Assert.Fail($"ChooseDeck() failed with exception: {ex.Message}");
            }

            // **Deck nach der Auswahl abrufen**
            var userDeck = _userDeckRepository.GetUserDeck(user.Id);
            int deckSize = userDeck.Count;
            Console.WriteLine($"User '{username}' now has {deckSize} cards in deck.");

            // **Prüfen, ob genau 4 Karten im Deck sind**
            Assert.AreEqual(4, deckSize, "User's deck should contain exactly 4 cards.");
        }


        [TestMethod]
        public void TestChooseDeck_admin2()
        {
            string token = "fixed-token-2";
            string username = "admin2";

            Console.WriteLine($"Starting TestChooseDeck for user '{username}'.");

            // Benutzer authentifizieren
            var (loginSuccess, loginToken) = _userRepository.AuthenticateUser(username, "password123");
            Assert.IsTrue(loginSuccess, "User should be able to log in.");
            Assert.IsNotNull(loginToken, "A session token should be generated.");

            // Benutzer abrufen
            var user = User.Get(username, _configuration);
            Assert.IsNotNull(user, "User should exist.");
            Console.WriteLine($"User '{user.UserName}' retrieved.");

            // **Alle Karten des Benutzers abrufen**
            var userCards = _userStackRepository.GetUserStack(user.Id);
            int totalCards = userCards.Count;
            Console.WriteLine($"User '{username}' has {totalCards} cards in stack before choosing a deck.");

            // **Überprüfen, ob der Benutzer mindestens 4 Karten besitzt**
            if (totalCards < 4)
            {
                Console.WriteLine($"Skipping ChooseDeck test because user '{username}' has only {totalCards} cards.");
                Assert.Inconclusive("User does not have enough cards to choose a deck.");
                return;
            }

            // **Die ersten 4 Karten aus dem Stack für das Deck wählen**
            var selectedCardIds = userCards.Take(4).ToList();

            // **Deck wählen**
            try
            {
                user.ChooseDeck(username, loginToken, selectedCardIds);
                Console.WriteLine($"Deck successfully chosen for user '{username}' with cards: {string.Join(", ", selectedCardIds)}");
            }
            catch (Exception ex)
            {
                Assert.Fail($"ChooseDeck() failed with exception: {ex.Message}");
            }

            // **Deck nach der Auswahl abrufen**
            var userDeck = _userDeckRepository.GetUserDeck(user.Id);
            int deckSize = userDeck.Count;
            Console.WriteLine($"User '{username}' now has {deckSize} cards in deck.");

            // **Prüfen, ob genau 4 Karten im Deck sind**
            Assert.AreEqual(4, deckSize, "User's deck should contain exactly 4 cards.");
        }



        [TestMethod]
        public void TestUserExists()
        {
            // Überprüfe, ob der Benutzer existiert
            var userExists = _userRepository.UserExists("admin");
            Assert.IsTrue(userExists, "The user 'admin' should exist in the database.");
        }

        

    }
}
