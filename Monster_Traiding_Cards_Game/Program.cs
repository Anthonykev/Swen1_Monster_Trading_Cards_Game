using System;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Network;
using Monster_Trading_Cards_Game.Interfaces;
using Monster_Traiding_Cards.Database;

namespace Monster_Trading_Cards_Game
{
    /// <summary>This class contains the main entry point of the application.</summary>
    internal class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public constants                                                                                                 //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Determines if debug token ("UserName-debug") will be accepted.</summary>
        public const bool ALLOW_DEBUG_TOKEN = true;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // entry point                                                                                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Application entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            // Verbindungszeichenfolge anpassen
            string connectionString = "Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards";

            // Datenbank-Objekt erstellen
            Database db = new Database(connectionString);

            // Tabelle erstellen
            if (db.CreateTables())
            {
                Console.WriteLine("Tabellen wurden erstellt oder existieren bereits.");
            }
            else
            {
                Console.WriteLine("Fehler beim Erstellen der Tabellen.");
            }

            // Standardkarten zur Datenbank hinzufügen
            if (db.AddDefaultCards())
            {
                Console.WriteLine("Standardkarten wurden hinzugefügt.");
            }
            else
            {
                Console.WriteLine("Fehler beim Hinzufügen der Standardkarten.");
            }

            // Test Registrierung
            Console.WriteLine("Test Registrierung:");
            bool registrationSuccess1 = db.RegisterUser("max", "password123", "Test User", "max@example.com");
            bool registrationSuccess2 = db.RegisterUser("max2", "password123", "Test User 2", "max2@example.com");
            Console.WriteLine(registrationSuccess1 ? "Registrierung von max erfolgreich" : "Registrierung von max fehlgeschlagen");
            Console.WriteLine(registrationSuccess2 ? "Registrierung von max2 erfolgreich" : "Registrierung von max2 fehlgeschlagen");

            // Test Anmeldung
            Console.WriteLine("Test Anmeldung:");
            var (loginSuccess1, token1) = db.AuthenticateUser("max", "password123");
            var (loginSuccess2, token2) = db.AuthenticateUser("max2", "password123");
            Console.WriteLine(loginSuccess1 ? $"Anmeldung von max erfolgreich, Token: {token1}" : "Anmeldung von max fehlgeschlagen");
            Console.WriteLine(loginSuccess2 ? $"Anmeldung von max2 erfolgreich, Token: {token2}" : "Anmeldung von max2 fehlgeschlagen");

            // Test Kartenverwaltung
            if (loginSuccess1 && loginSuccess2)
            {
                User? user1 = User.Get("max");
                User? user2 = User.Get("max2");
                Console.WriteLine(user1 != null ? $"Benutzer max gefunden: {user1.UserName}" : "Benutzer max nicht gefunden");
                Console.WriteLine(user2 != null ? $"Benutzer max2 gefunden: {user2.UserName}" : "Benutzer max2 nicht gefunden");

                if (user1 != null && user2 != null)
                {
                    Console.WriteLine("Test Kartenverwaltung:");
                    user1.AddPackage();
                    user2.AddPackage();
                    Console.WriteLine("Pakete hinzugefügt");

                    user1.ChooseDeck();
                    user2.ChooseDeck();
                    Console.WriteLine("Decks ausgewählt");

                    // Überprüfen, ob die Decks Karten enthalten
                    if (user1.Deck.Count == 0 || user2.Deck.Count == 0)
                    {
                        Console.WriteLine("Einer der Benutzer hat keine Karten im Deck. Kampf kann nicht gestartet werden.");
                        return;
                    }

                    // Test Kämpfe
                    Console.WriteLine("Test Kämpfe:");
                    Battle battle = new Battle(user1, user2);
                    battle.Start();
                    Console.WriteLine(battle.Winner != null ? $"Gewinner: {battle.Winner.UserName}" : "Unentschieden");
                }
            }


            HttpSvr svr = new(); // neue scheibweise. System erkennt automatisch was Sache ist.
            svr.Incoming += Svr_Incoming; //(sender, e) => { Handler.HandleEvent(e); };

            svr.Run();
            Console.WriteLine("Server läuft auf http://127.0.0.1:12000\"");
        }

        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.HandleEvent(e);
        }
    }
}
