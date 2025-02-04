using Npgsql;
using System;
using System.Collections.Generic;
using Monster_Trading_Cards_Game.Models;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game.Repositories
{
    public class CardRepository
    {
        private readonly string _connectionString;

        public CardRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool AddDefaultCards()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // �berpr�fen, ob bereits Karten in der Datenbank vorhanden sind
                    var checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Cards", connection);
                    int cardCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (cardCount > 0)
                    {
                        // Karten sind bereits in der Datenbank vorhanden, keine weiteren Aktionen erforderlich
                        return true;
                    }

                    // Karten sind nicht vorhanden, Standardkarten hinzuf�gen
                    var command = new NpgsqlCommand(@"
                    INSERT INTO Cards (Name, Type, Damage, ElementType) VALUES
                    ('Dragons', 'Monster-Card', 70, 'Fire'),
                    ('FireElves', 'Monster-Card', 70, 'Fire'),
                    ('Amaterasu', 'Spell-Card', 70, 'Fire'),
                    ('Raijin', 'Spell-Card', 70, 'Fire'),
                    ('Tetsu', 'Monster-Card', 70, 'Fire'),
                    ('Kraken', 'Monster-Card', 70, 'Water'),
                    ('DogMike', 'Monster-Card', 70, 'Water'),
                    ('Susanoo', 'Spell-Card', 70, 'Water'),
                    ('Bankai', 'Spell-Card', 70, 'Water'),
                    ('Wizzard', 'Spell-Card', 70, 'Water'),
                    ('Goblins', 'Monster-Card', 50, 'Normal'),
                    ('Knights', 'Monster-Card', 50, 'Normal'),
                    ('Orks', 'Monster-Card', 50, 'Normal'),
                    ('Rocklee', 'Monster-Card', 50, 'Normal'),
                    ('FighterKevin', 'Monster-Card', 50, 'Normal'),
                    ('Inferno', 'Spell-Card', 70, 'Fire'),
                    ('Tsunami', 'Spell-Card', 70, 'Water'),
                    ('Earthquake', 'Spell-Card', 50, 'Normal'),
                    ('Blizzard', 'Spell-Card', 70, 'Water'),
                    ('Lightning', 'Spell-Card', 70, 'Fire')
                    ON CONFLICT (Name) DO NOTHING", connection);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding default cards: {ex.Message}");
                return false;
            }
        }

        public Card? GetCardById(int cardId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Id, Name, Damage, ElementType, Type FROM Cards WHERE Id = @id", connection);
                command.Parameters.AddWithValue("@id", cardId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        int damage = reader.GetInt32(2);
                        ElementType elementType = Enum.Parse<ElementType>(reader.GetString(3));
                        string type = reader.GetString(4);

                        Console.WriteLine($"Loaded-card: ID={id}, Name={name}, Damage={damage}, ElementType={elementType}, Type={type}");

                        if (type == "Monster-Card")
                        {
                            return new MonsterCard(id, name, damage, elementType);
                        }
                        else if (type == "Spell-Card")
                        {
                            return new SpellCard(id, name, damage, elementType);
                        }
                        else
                        {
                            throw new Exception("Unknown card type");
                        }
                    }
                }
            }
            return null;
        }
    }
}
