using FHTW.Swen1.Swamp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// 2)

namespace FHTW.Swen1.Swamp.Network
{
    /// <summary>This class implements a HTTP server.</summary>
    public sealed class HttpSvr
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP listener instance.</summary>
        private TcpListener? _Listener;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public events                                                                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Is raised when incoming data is available.</summary>
        public event HttpSvrEventHandler? Incoming;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets if the server is available.</summary>
        public bool Active { get; private set; } = false;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Runs the server.</summary>
        public void Run()
        {
            if (Active) return;

            Active = true;
            _Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 12000);

            try
            {
                Console.WriteLine("Server is starting...");
                _Listener.Start();
                Console.WriteLine("Server is listening on http://127.0.0.1:12000");

                while (Active)
                {
                    Console.WriteLine("Waiting for a connection...");
                    using (TcpClient client = _Listener.AcceptTcpClient()) // Automatisches Schließen des Clients
                    {
                        Console.WriteLine("Connection accepted!");

                        try
                        {
                            string data = string.Empty;

                            using (var stream = client.GetStream())
                            {
                                // Daten lesen
                                byte[] buf = new byte[256];
                                while (stream.DataAvailable || string.IsNullOrWhiteSpace(data))
                                {
                                    int n = stream.Read(buf, 0, buf.Length);
                                    data += Encoding.ASCII.GetString(buf, 0, n);
                                }

                                Console.WriteLine($"Received data: {data}");

                                // Antwort senden
                                string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, World!";
                                byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                                stream.Write(responseBytes, 0, responseBytes.Length);
                                Console.WriteLine("Response sent.");

                                // Daten für Event vorbereiten
                                string requestData = data; // Hier speichern wir die empfangenen Daten
                                Incoming?.Invoke(this, new HttpSvrEventArgs(null, requestData));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error handling client: {ex.Message}");
                        }
                    } // TcpClient wird hier geschlossen
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }


        /// <summary>Stops the server.</summary>
        public void Stop()
        {
            Console.WriteLine("Stopping server...");
            Active = false;
            _Listener?.Stop();
        }
    }
}
