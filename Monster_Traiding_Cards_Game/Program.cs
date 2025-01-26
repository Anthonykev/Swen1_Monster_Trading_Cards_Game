using System;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Network;
using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Database;
using Monster_Trading_Cards_Game.Repositories;
using Monster_Traiding_Cards.Repositories;

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
        /// <param name="args">Command line arguments.</summary>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            HttpSvr svr = new();
            svr.Incoming += Svr_Incoming;

            // Initialisieren Sie den SessionHandler
            SessionHandler sessionHandler = new();
            sessionHandler.Initialize();

            svr.Run();
            Console.WriteLine("Server läuft auf http://127.0.0.1:12000");
        }

        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.HandleEvent(e);
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("OnProcessExit aufgerufen.");
                SessionHandler.LogoutAllUsers();
                Console.WriteLine("Alle Benutzer wurden abgemeldet.");

                // Lobby und Benutzerdecks leeren
                var lobbyRepository = new LobbyRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                lobbyRepository.ClearLobby();

                var userDeckRepository = new UserDeckRepository("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                userDeckRepository.ClearAllUserDecks();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abmelden aller Benutzer: {ex.Message}");
            }
        }


    }
}


