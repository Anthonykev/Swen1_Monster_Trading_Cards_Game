using System;
using FHTW.Swen1.Swamp.Models;
using FHTW.Swen1.Swamp.Network;
using FHTW.Swen1.Swamp.Interfaces;



namespace FHTW.Swen1.Swamp
{
    /// <summary>This class contains the main entry point of the application.</summary>
    internal class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // entry point                                                                                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Application entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            HttpSvr svr = new(); // neue scheibweise. System erkennt automatisch was Sache ist.
            svr.Incoming += Svr_Incoming; //(sender, e) => { Handler.HandleEvent(e); };

            svr.Run();
            Console.WriteLine("Server läuft auf http://127.0.0.1:12000\"");
        }



        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.HandleEvent(e);

            /*
            Console.WriteLine(e.Method);
            Console.WriteLine(e.Path);
            Console.WriteLine();
            foreach(HttpHeader i in e.Headers)
            {
                Console.WriteLine(i.Name + ": " + i.Value);
            }
            Console.WriteLine();
            Console.WriteLine(e.Payload);

            e.Reply(HttpStatusCode.OK, "Yo Baby!");
            */
        }
    }
}




//Eigene Klasse für Deck erstellen und dort die Logig schon mal einbauen 


/*


Summary
   

   1. `Program.cs` – Einstiegspunkt, initialisiert und startet den Server.
   2. `HttpSvr.cs` – Server-Logik, verarbeitet TCP-Anfragen.
   3. `HttpSvrEventHandler.cs` – Delegat, verbindet Ereignisse mit Methoden.
   4. `HttpSvrEventArgs.cs` – Stellt Daten für Ereignisse bereit.
   5. `HttpHeader.cs` – Analysiert und speichert HTTP-Header.
   6. `HttpStatusCodes.cs` – Definiert HTTP-Statuscodes.
   7. `IHandler.cs`– Schnittstelle für Handler-Klassen.
   8. `Handler.cs` – Abstrakte Klasse, die Handler-Logik koordiniert.
   9. `UserHandler.cs` – Bearbeitet Benutzer-bezogene Anfragen.
   10. `User.cs`– Repräsentiert Benutzer, enthält Methoden für Benutzerverwaltung.
   11. `Token.cs` – Sicherheitslogik, erstellt und überprüft Tokens.
   12. `SessionHandler.cs` – Verarbeitet Sitzungs-bezogene Anfragen (z. B. Login).
   13. `UserException.cs` – Behandelt benutzerdefinierte Ausnahmen.
   
 

*/


