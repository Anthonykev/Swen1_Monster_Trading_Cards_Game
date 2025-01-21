using Npgsql;
using System;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Trading_Cards_Game.Models;

namespace Monster_Traiding_Cards.Repositories
{
    internal class LobbyRepository
    {
        private readonly string _connectionString;
        private readonly BattleRepository _battleRepository;

        public LobbyRepository(string connectionString)
        {
            _connectionString = connectionString;
            _battleRepository = new BattleRepository(connectionString);
        }

        public bool AddUserToLobby(string username, string token)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Benutzer-Objekt abrufen
                    var user = User.Get(username);
                    if (user == null)
                    {
                        throw new Exception("User not found");
                    }

                    // Überprüfen, ob der Benutzer bereits in der Lobby ist
                    var checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Lobby WHERE UserId = @UserId", connection);
                    checkCommand.Parameters.AddWithValue("UserId", user.Id);
                    var count = (long)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        return false; // Benutzer ist bereits in der Lobby
                    }

                    // Überprüfen, ob der Benutzer ein Deck ausgewählt hat
                    if (user.Deck == null || user.Deck.Count == 0)
                    {
                        Console.WriteLine($"User {username} has not selected a deck.");
                        return false; // Benutzer hat kein Deck ausgewählt
                    }

                    // Benutzer zur Lobby hinzufügen
                    var insertCommand = new NpgsqlCommand("INSERT INTO Lobby (UserId) VALUES (@UserId)", connection);
                    insertCommand.Parameters.AddWithValue("UserId", user.Id);
                    insertCommand.ExecuteNonQuery();

                    // Überprüfen, ob zwei Benutzer in der Lobby sind
                    var lobbyCountCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Lobby", connection);
                    var lobbyCount = (long)lobbyCountCommand.ExecuteScalar();

                    if (lobbyCount >= 2)
                    {
                        // Zwei Benutzer in der Lobby, Battle starten
                        StartBattle(connection);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user to lobby: {ex.Message}");
                return false;
            }
        }



        private void StartBattle(NpgsqlConnection connection)
        {
            try
            {
                // Zwei Benutzer aus der Lobby holen
                var getUsersCommand = new NpgsqlCommand("SELECT UserId FROM Lobby LIMIT 2", connection);
                var reader = getUsersCommand.ExecuteReader();

                int user1Id = 0, user2Id = 0;
                if (reader.Read())
                {
                    user1Id = reader.GetInt32(0);
                }
                if (reader.Read())
                {
                    user2Id = reader.GetInt32(0);
                }
                reader.Close();

                // Benutzer-Objekte abrufen
                var player1 = User.GetById(user1Id);
                var player2 = User.GetById(user2Id);

                if (player1 != null && player2 != null)
                {
                    // Battle starten
                    var battle = new Battle(player1, player2);
                    battle.Start();

                    // Benutzer aus der Lobby entfernen
                    var deleteCommand = new NpgsqlCommand("DELETE FROM Lobby WHERE UserId = @User1Id OR UserId = @User2Id", connection);
                    deleteCommand.Parameters.AddWithValue("User1Id", user1Id);
                    deleteCommand.Parameters.AddWithValue("User2Id", user2Id);
                    deleteCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting battle: {ex.Message}");
            }
        }
    }
}
