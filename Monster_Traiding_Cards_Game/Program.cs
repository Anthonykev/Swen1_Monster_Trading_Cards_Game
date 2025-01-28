using System;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Network;
using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Database;
using Monster_Trading_Cards_Game.Repositories;
using Microsoft.Extensions.Configuration;

namespace Monster_Trading_Cards_Game
{
    internal class Program
    {
        public const bool ALLOW_DEBUG_TOKEN = true;

        static void Main(string[] args)
        {
            // Konfiguration laden
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            // Prüfe, ob der ConnectionString korrekt geladen wird
            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("⚠ Fehler: ConnectionString wurde nicht geladen!");
            }
            else
            {
                Console.WriteLine($"✅ ConnectionString erfolgreich geladen: {connectionString}");
            }

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            // Direkte Instanziierung des SessionHandlers mit expliziter Konfiguration
            SessionHandler sessionHandler = new SessionHandler(configuration);
            sessionHandler.Initialize();

            HttpSvr svr = new();
            svr.Incoming += (sender, e) => Svr_Incoming(sender, e, configuration);

            svr.Run();
            Console.WriteLine("Server läuft auf http://127.0.0.1:12000");
        }

        private static void Svr_Incoming(object sender, HttpSvrEventArgs e, IConfiguration configuration)
        {
            Handler.HandleEvent(e, configuration);
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("OnProcessExit aufgerufen.");

                // Konfiguration erneut laden
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfiguration configuration = builder.Build();

                // Direkte Instanziierung des SessionHandlers mit der Konfiguration
                SessionHandler sessionHandler = new SessionHandler(configuration);
                sessionHandler.LogoutAllUsers();
                Console.WriteLine("Alle Benutzer wurden abgemeldet.");

                // Lobby und Benutzerdecks leeren
                var lobbyRepository = new LobbyRepository(configuration);
                lobbyRepository.ClearLobby();

                var userDeckRepository = new UserDeckRepository(configuration);
                userDeckRepository.ClearAllUserDecks();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abmelden aller Benutzer: {ex.Message}");
            }
        }
    }
}
