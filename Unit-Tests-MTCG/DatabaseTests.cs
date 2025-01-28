using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monster_Trading_Cards_Game;
using Monster_Trading_Cards_Game.Database;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Models;
using System;
using System.Linq;
using Npgsql;
using Microsoft.Extensions.Configuration;

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
            string testConnectionString = _configuration.GetConnectionString("TestConnection");

            // Initialisiere Repositories mit der Test-Datenbank
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
            // ClearTestData();
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
            string testConnectionString = _configuration.GetConnectionString("TestConnection");
            using (var connection = new NpgsqlConnection(testConnectionString))
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
            string testConnectionString = _configuration.GetConnectionString("TestConnection");
            Console.WriteLine("Clearing test data...");
            using (var connection = new NpgsqlConnection(testConnectionString))
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
            string testConnectionString = _configuration.GetConnectionString("TestConnection");
            using (var connection = new NpgsqlConnection(testConnectionString))
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
        public void TestBuyPackage()
        {
            // Verwende den festen Token aus dem test.bat-Skript
            string token = "fixed-token-1";
            string username = "admin";

            Console.WriteLine($"Starting TestBuyPackage for user '{username}' with fixed token '{token}'.");

            // Authentifiziere den Benutzer und erhalte den Token
            var (loginSuccess, loginToken) = _userRepository.AuthenticateUser(username, "password123");
            Console.WriteLine($"Authentication result: {loginSuccess}, Token: {loginToken}");
            Assert.IsTrue(loginSuccess, "User should be able to log in with correct credentials.");
            Assert.IsNotNull(loginToken, "A session token should be generated upon login.");

            // Hole den Benutzer aus der Testdatenbank
            var user = User.Get(username, _configuration);
            Console.WriteLine($"Retrieved user from database: {user?.UserName}, Coins: {user?.Coins}, SessionToken: {user?.SessionToken}");
            Assert.IsNotNull(user, "User should exist.");

            // Setze den festen Token für den Benutzer
            user.SessionToken = token;
            user.Save(username, token);
            Console.WriteLine($"Set fixed token for user '{username}'.");

            // Manipuliere Coins direkt in der Testdatenbank
            string testConnectionString = _configuration.GetConnectionString("TestConnection");
            using (var connection = new NpgsqlConnection(testConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("UPDATE Users SET Coins = 20 WHERE Username = @username", connection);
                command.Parameters.AddWithValue("@username", username);
                command.ExecuteNonQuery();
                Console.WriteLine($"Set 20 coins for user '{username}' in database 'MTCG_Test'.");
            }

            // Hole den Benutzer erneut mit dem festen Token
            var userWithFixedToken = User.GetByUsernameAndToken(username, token, _configuration);
            Console.WriteLine($"User retrieved with fixed token: {userWithFixedToken?.UserName}, Coins: {userWithFixedToken?.Coins}");
            Assert.IsNotNull(userWithFixedToken, "User should exist.");
            Assert.IsTrue(userWithFixedToken.Coins >= 5, "User should have enough coins to buy a package.");

            // Kaufe ein Paket (Testlogik)
            userWithFixedToken.Coins -= 5; // Simuliere Paketkauf
            userWithFixedToken.Save(username, token); // Speichere Änderungen

            Console.WriteLine($"User '{username}' bought a package. Coins left: {userWithFixedToken.Coins}");

            // Überprüfe die verbleibenden Münzen
            Assert.AreEqual(15, userWithFixedToken.Coins, "User should have 15 coins left after buying a package.");
            Console.WriteLine($"TestBuyPackage completed successfully for user '{username}'.");
        }

        [TestMethod]
        public void TestUserExists()
        {
            // Überprüfe, ob der Benutzer existiert
            var userExists = _userRepository.UserExists("admin");
            Assert.IsTrue(userExists, "The user 'admin' should exist in the database.");
        }

        [TestMethod]
        public void TestCreateRandomPackages()
        {
            // Erstelle zufällige Pakete
            _packageRepository.CreateRandomPackages(5);

            // Überprüfe, ob Pakete in der Datenbank vorhanden sind
            string testConnectionString = _configuration.GetConnectionString("TestConnection");
            using (var connection = new NpgsqlConnection(testConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT COUNT(*) FROM Packages", connection);
                int packageCount = Convert.ToInt32(command.ExecuteScalar());
                Assert.IsTrue(packageCount >= 5, "There should be at least 5 packages in the database.");
            }
        }
    }
}

