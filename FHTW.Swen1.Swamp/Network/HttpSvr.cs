using FHTW.Swen1.Swamp.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            _Listener.Start();

            byte[] buf = new byte[256];

            while (Active)
            {
                Console.WriteLine("Waiting for a connection...");
                TcpClient client = _Listener.AcceptTcpClient();
                Console.WriteLine("Connection accepted!");

                try
                {
                    string data = string.Empty;
                    using (var stream = client.GetStream())
                    {
                        while (stream.DataAvailable || string.IsNullOrWhiteSpace(data))
                        {
                            int n = stream.Read(buf, 0, buf.Length);
                            data += Encoding.ASCII.GetString(buf, 0, n);
                        }

                        //Console.WriteLine($"Received data: {data}");

                        // Trigger the event for incoming data processing
                        Incoming?.Invoke(this, new HttpSvrEventArgs(client, data));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client: {ex.Message}");
                }
                finally
                {
                    client.Close(); // Make sure to close the client connection
                }
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
