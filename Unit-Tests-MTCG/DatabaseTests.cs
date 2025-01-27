using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monster_Trading_Cards_Game;
using Monster_Trading_Cards_Game.Database;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Models;
using System;
using System.Linq;
using Npgsql;

namespace Unit_Tests_MTCG
{
    [TestClass]
    public class DatabaseTests
    {
        private const string TestConnectionString = "Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=MTCG_Test";

        private UserRepository _userRepository;
        private CardRepository _cardRepository;
        private PackageRepository _packageRepository;
        private UserStackRepository _userStackRepository;
        private UserDeckRepository _userDeckRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _userRepository = new UserRepository(TestConnectionString);
            _cardRepository = new CardRepository(TestConnectionString);
            _packageRepository = new PackageRepository(TestConnectionString);
            _userStackRepository = new UserStackRepository(TestConnectionString);
            _userDeckRepository = new UserDeckRepository(TestConnectionString);

            CreateTables createTables = new CreateTables(TestConnectionString);
            createTables.Execute_CreateTables();
            SeedTestData();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ClearTestData();
        }

        private void SeedTestData()
        {
            if (!_userRepository.UserExists("testuser1"))
            {
                _userRepository.CreateUser("testuser1", "password123", "Test User 1", "testuser1@example.com");
            }

            if (!_userRepository.UserExists("testuser2"))
            {
                _userRepository.CreateUser("testuser2", "password123", "Test User 2", "testuser2@example.com");
            }

            var user1 = User.Get("testuser1");
            if (user1 == null)
            {
                user1 = User.GetByUsernameAndToken("testuser1", "");
                var cards = _cardRepository.GetAllCards().Take(10).ToList();
                foreach (var card in cards)
                {
                    user1.Stack.Add(card);
                    _userStackRepository.AddCardToUserStack(user1.Id, card.Id);
                }
                user1.Save("testuser1", user1.SessionToken);
            }

            _packageRepository.CreateRandomPackages(3);
        }

        private void ClearTestData()
        {
            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(@"
                    TRUNCATE TABLE Lobby, BattleRounds, Battles, UserDecks, UserStacks, Packages, Cards, Users RESTART IDENTITY CASCADE;", connection);
                command.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void TestUserRegistration()
        {
            var success = _userRepository.CreateUser("testuser111", "password123", "Test User 1", "testuser1@example.com");

            if (!success)
            {
                var userExists = _userRepository.UserExists("testuser111");
                Assert.IsTrue(userExists, "The user already exists in the database.");
                Console.WriteLine("User already exists.");
            }
            else
            {
                var userExists = _userRepository.UserExists("testuser111");
                Assert.IsTrue(userExists, "The new user should exist in the database.");
            }
        }

        [TestMethod]
        public void TestAddDefaultCards()
        {
            var success = _cardRepository.AddDefaultCards();
            Assert.IsTrue(success, "Default cards should be added successfully.");

            using (var connection = new NpgsqlConnection(TestConnectionString))
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
            var (success, token) = _userRepository.AuthenticateUser("testuser1", "password123");
            Assert.IsTrue(success, "User should be able to log in with correct credentials.");
            Assert.IsNotNull(token, "A session token should be generated upon login.");
        }

        [TestMethod]
        public void TestBuyPackage()
        {
            var user = User.GetByUsernameAndToken("testuser1", "some-valid-token");
            Assert.IsNotNull(user, "User should exist.");
            Assert.IsTrue(user.Coins >= 5, "User should have enough coins to buy a package.");

            user.AddPackage("testuser1", user.SessionToken);

            Assert.AreEqual(15, user.Coins, "User should have 15 coins left after buying a package.");
            Assert.IsTrue(user.Stack.Count >= 5, "User should have received 5 cards in their stack.");
        }

        [TestMethod]
        public void TestGetUserCards()
        {
            var user = User.GetByUsernameAndToken("testuser1", "some-valid-token");
            Assert.IsNotNull(user, "User should exist.");

            var userStack = _userStackRepository.GetUserStack(user.Id);
            Assert.IsTrue(userStack.Count > 0, "User should have cards in their stack.");

            var userCards = userStack.Select(cardId => _cardRepository.GetCardById(cardId)).Where(card => card != null).ToList();

            Assert.IsTrue(userCards.Count > 0, "User should have valid cards in their stack.");
        }

        [TestMethod]
        public void TestChooseDeck()
        {
            var user = User.GetByUsernameAndToken("testuser1", "some-valid-token");
            Assert.IsNotNull(user, "User should exist.");

            var userStack = _userStackRepository.GetUserStack(user.Id);
            Assert.IsTrue(userStack.Count >= 4, "User should have at least 4 cards in their stack.");

            var cardIds = userStack.Take(4).ToList();
            user.ChooseDeck("testuser1", user.SessionToken, cardIds);

            var deck = _userDeckRepository.GetUserDeck(user.Id);
            Assert.AreEqual(4, deck.Count, "Deck should contain exactly 4 cards.");
        }
    }
}
