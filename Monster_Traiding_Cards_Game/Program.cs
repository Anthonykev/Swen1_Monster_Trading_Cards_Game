﻿using System;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Network;
using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Database;
using Monster_Trading_Cards_Game.Repositories;

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
/*
            // Create 10 random packages at startup
            string connectionString = "Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards";
            PackageRepository packageRepository = new PackageRepository(connectionString);
            packageRepository.CreateRandomPackages(10);
*/

            HttpSvr svr = new();
            svr.Incoming += Svr_Incoming;

            svr.Run();
            Console.WriteLine("Server läuft auf http://127.0.0.1:12000");
        }

        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.HandleEvent(e);
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            // Delete all packages on exit
            string connectionString = "Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards";
            PackageRepository packageRepository = new PackageRepository(connectionString);
            packageRepository.DeleteAllPackages();

            SessionHandler.LogoutAllUsers();
            Console.WriteLine("Alle Benutzer wurden abgemeldet.");
        }
    }
}
