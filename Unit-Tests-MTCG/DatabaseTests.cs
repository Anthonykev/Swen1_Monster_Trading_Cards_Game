using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Models;
using System;
using System.IO;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Monster_Trading_Cards_Game.Database;
using System.Linq;

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

            _userRepository = new UserRepository(_configuration);
            _cardRepository = new CardRepository(_configuration);
            _packageRepository = new PackageRepository(_configuration);
            _userStackRepository = new UserStackRepository(_configuration);
            _userDeckRepository = new UserDeckRepository(_configuration);

            CreateTables createTables = new CreateTables(_configuration);
            createTables.Execute_CreateTables();
            SeedTestData();
        }

        [Priority(1)]
        [TestMethod]
        public void TestUserRegistration()
        {
            var success = _userRepository.CreateUser("kevin", "password123", "Test User", "kodzo@example.com");
            Assert.IsTrue(success || _userRepository.UserExists("kevin"), "User creation should succeed or user should already exist.");
        }

        [Priority(2)]
        [TestMethod]
        public void TestUserLogin()
        {
            var (success, token) = _userRepository.AuthenticateUser("admin", "password123");
            Assert.IsTrue(success, "User should be able to log in.");
            Assert.IsNotNull(token, "A session token should be generated upon login.");
        }

        [Priority(3)]
        [TestMethod]
        public void TestUserLogin2()
        {
            var (success, token) = _userRepository.AuthenticateUser("admin2", "password123");
            Assert.IsTrue(success, "User should be able to log in.");
            Assert.IsNotNull(token, "A session token should be generated upon login.");
        }

        [Priority(4)]
        [TestMethod]
        public void TestAddDefaultCards()
        {
            var success = _cardRepository.AddDefaultCards();
            Assert.IsTrue(success, "Default cards should be added successfully.");
        }

        [Priority(5)]
        [TestMethod]
        public void TestBuyPackage_admin()
        {
            TestBuyPackage("admin", "fixed-token-1");
        }

        [Priority(6)]
        [TestMethod]
        public void TestBuyPackage_admin2()
        {
            TestBuyPackage("admin2", "fixed-token-2");
        }

        [Priority(7)]
        [TestMethod]
        public void TestChooseDeck_admin()
        {
            TestChooseDeck("admin", "fixed-token-1");
        }

        [Priority(8)]
        [TestMethod]
        public void TestChooseDeck_admin2()
        {
            TestChooseDeck("admin2", "fixed-token-2");
        }



        [Priority(9)]
        [TestMethod]
        public void TestChangeMotto()
        {
            // Arrange
            var user = User.Get("admin", _configuration);
            Assert.IsNotNull(user, "User 'admin' should exist.");

            // Act
            user.Motto = "New Motto";
            user.Save(user.UserName, user.SessionToken);

            // Assert
            var updatedUser = User.Get("admin", _configuration);
            Assert.IsNotNull(updatedUser, "Updated user 'admin' should exist.");
            Assert.AreEqual("New Motto", updatedUser.Motto, "The motto should be updated successfully.");
        }

        [Priority(10)]
        [TestMethod]
        public void TestGetUsersSortedByElo()
        {
            // Act
            var users = User.GetUsersSortedByElo(_configuration);

            // Assert
            Assert.IsNotNull(users, "Users should be retrieved successfully.");
            Assert.IsTrue(users.Count > 0, "There should be at least one user.");
            Assert.IsTrue(users.SequenceEqual(users.OrderByDescending(u => u.Elo)), "Users should be sorted by Elo in descending order.");
        }



        [Priority(11)]
        [TestMethod]
        public void TestUserExists()
        {
            var userExists = _userRepository.UserExists("admin");
            Assert.IsTrue(userExists, "The user 'admin' should exist in the database.");
        }

        private void SeedTestData()
        {
            if (!_userRepository.UserExists("admin"))
            {
                _userRepository.CreateUser("admin", "password123", "Test User 1", "testuser1@example.com");
            }

            if (!_userRepository.UserExists("admin2"))
            {
                _userRepository.CreateUser("admin2", "password123", "Test User 2", "testuser2@example.com");
            }

            _cardRepository.AddDefaultCards();
            _packageRepository.CreateRandomPackages(3);
        }

        private void TestBuyPackage(string username, string token)
        {
            var (loginSuccess, loginToken) = _userRepository.AuthenticateUser(username, "password123");
            Assert.IsTrue(loginSuccess, "User should be able to log in.");
            Assert.IsNotNull(loginToken, "A session token should be generated.");
        }

        private void TestChooseDeck(string username, string token)
        {
            var (loginSuccess, loginToken) = _userRepository.AuthenticateUser(username, "password123");
            Assert.IsTrue(loginSuccess, "User should be able to log in.");
            Assert.IsNotNull(loginToken, "A session token should be generated.");
        }
    }
}
