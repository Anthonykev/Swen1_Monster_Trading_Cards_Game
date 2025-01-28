using Microsoft.Extensions.Configuration;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Network;
using Monster_Trading_Cards_Game.Repositories;
using Npgsql;
using System.Security.Authentication;
using System.Security.Cryptography; // Hinzugefügt
using System.Text;

public sealed class User
{
    private static Dictionary<string, User> _Users = new();
    private readonly IConfiguration _configuration;

    private User(IConfiguration configuration)
    {
        _configuration = configuration;
    }

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

    public void Save(string username, string token)
    {
        if (!IsAuthenticated(username, token))
        {
            throw new AuthenticationException("Not authenticated.");
        }

        new UserRepository(_configuration).SaveToDatabase(this);
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
        CardRepository cardRepository = new CardRepository(_configuration);
        PackageRepository packageRepository = new PackageRepository(_configuration);
        UserStackRepository userStackRepository = new UserStackRepository(_configuration);

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
        Save(username, token);
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

        var userStackRepository = new UserStackRepository(_configuration);
        var userDeckRepository = new UserDeckRepository(_configuration);
        var cardRepository = new CardRepository(_configuration);

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
        Save(username, token);
    }

    public static (bool Success, string Token) Logon(string userName, string password, IConfiguration configuration)
    {
        if (_Users.ContainsKey(userName) && VerifyPassword(password, _Users[userName].Password))
        {
            string token = Token._CreateTokenFor(_Users[userName]);
            _Users[userName].SessionToken = token;
            new UserRepository(configuration).SaveToDatabase(_Users[userName]);
            return (true, token);
        }

        return (false, string.Empty);
    }

    public static bool Exists(string userName)
    {
        return _Users.ContainsKey(userName);
    }

    public static User? Get(string userName, IConfiguration configuration)
    {
        using (var connection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT Id, Username, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames FROM Users WHERE Username = @username", connection);
            command.Parameters.AddWithValue("@username", userName);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new User(configuration)
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

    public static User? GetById(int userId, IConfiguration configuration)
    {
        using (var connection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT Id, Username, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames FROM Users WHERE Id = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new User(configuration)
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

    public static User? GetByUsernameAndToken(string username, string token, IConfiguration configuration)
    {
        try
        {
            using (var connection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Id, Username, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames FROM Users WHERE Username = @username AND SessionToken = @token", connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@token", token);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User(configuration)
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
    public static List<User> GetUsersSortedByElo(IConfiguration configuration)
    {
        List<User> users = new List<User>();

        using (var connection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT Id, Username, FullName, EMail, Coins, Password, SessionToken, Elo, Wins, Losses, TotalGames FROM Users ORDER BY Elo DESC", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new User(configuration)
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
                    });
                }
            }
        }

        return users;
    }


}

