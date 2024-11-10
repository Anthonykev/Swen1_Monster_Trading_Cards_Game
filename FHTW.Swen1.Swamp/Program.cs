using System;
using FHTW.Swen1.Swamp.Models;


namespace FHTW.Swen1.Swamp

//1)
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            User player = new User { Username = "Player1", Password = "password123" };
            player.AddPackage();
            
        }
    }
}








/*


Summary
    1)Program.cs is the entry point and eventually will initialize and start the server.
    2)HttpSvr.cs contains the logic for starting and running the server, listening for incoming TCP requests.
    3)HttpSvrEventHandler.cs defines the type of event handler (delegate) that will process incoming data.
    4)HttpSvrEventArgs.cs defines the data that will be provided to event handlers.
    5)HttpHeader.cs is used to parse HTTP headers from incoming messages.
*/