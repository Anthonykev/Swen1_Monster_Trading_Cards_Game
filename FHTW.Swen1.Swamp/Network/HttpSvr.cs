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
    public sealed class HttpSvr
    {
        private TcpListener? _Listener;  // "?" steht dafür das der _Listener auch ein Null Wert enthalten kann. 


        public event HttpSvrEventHandler? Incoming; //"Incoming" ist der Namen von event


        public bool Active
        {
            get; set;
        } = false;


        public void Run()
        {
            if (Active) return;

            Active = true;
            _Listener = new(IPAddress.Parse("127.0.0.1"), 12000);  //Wir wandeln die IP Adresse so um das es gut lesbar ist.
            _Listener.Start();

            byte[] buf = new byte[256];

            while (Active)
            {
                TcpClient client = _Listener.AcceptTcpClient(); //Accepts a pending connection request and returns a TcpClient to represent the connection.
                string data = string.Empty;

                while (client.GetStream().DataAvailable || string.IsNullOrWhiteSpace(data))
                {
                    int n = client.GetStream().Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);  //Converts the bytes read into a string and appends it to data.
                }

                Incoming?.Invoke(this, new(client, data));
            }
        }
    }
}


