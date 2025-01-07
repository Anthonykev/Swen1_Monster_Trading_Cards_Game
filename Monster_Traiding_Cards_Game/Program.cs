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

            HttpSvr svr = new();
            svr.Incoming += Svr_Incoming;

            svr.Run();
            Console.WriteLine("Server läuft auf http://127.0.0.1:12000");
        }

        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.HandleEvent(e);
        }
    }
}
