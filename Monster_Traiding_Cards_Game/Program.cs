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
                Console.WriteLine("Tabelle 'Cards' wurde erstellt oder existiert bereits.");
            }
            else
            {
                Console.WriteLine("Fehler beim Erstellen der Tabelle 'Cards'.");
            }

          /*  // Testkarte hinzufügen
            if (db.AddCard("Dragon", "Fire", 70))
            {
                Console.WriteLine("Karte erfolgreich hinzugefügt.");
            }

            // Alle Karten ausgeben
            var cards = db.GetAllCards();
            foreach (var card in cards)
            {
                Console.WriteLine($"Id: {card.Id}, Name: {card.Name}, Typ: {card.Type}, Schaden: {card.Damage}");
            }
*/



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
