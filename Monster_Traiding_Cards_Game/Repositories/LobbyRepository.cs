using Npgsql;
using System;
using Monster_Trading_Cards_Game.Repositories;

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

        public bool AddUserToLobby(int userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Überprüfen, ob der Benutzer bereits in der Lobby ist
                    var checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Lobby WHERE UserId = @UserId", connection);
                    checkCommand.Parameters.AddWithValue("UserId", userId);
                    var count = (long)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        return false; // Benutzer ist bereits in der Lobby
                    }

                    // Benutzer zur Lobby hinzufügen
                    var insertCommand = new NpgsqlCommand("INSERT INTO Lobby (UserId) VALUES (@UserId)", connection);
                    insertCommand.Parameters.AddWithValue("UserId", userId);
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

                // Battle-Eintrag erstellen
                var battleCreated = _battleRepository.AddBattle(user1Id, user2Id, 0, 0); // WinnerId und LoserId sind 0, da der Battle noch nicht entschieden ist

                if (battleCreated)
                {
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
