using System;
using System.Net.Http.Headers;
using System.Net.Sockets;


//4)
namespace FHTW.Swen1.Swamp.Network
{
    public class HttpSvrEventArgs : EventArgs  //Defines a class HttpSvrEventArgs which inherits from EventArgs
    {
        protected TcpClient _Client;


        public HttpSvrEventArgs(TcpClient client, string plainMessage)  //A constructor that initializes the class with a TcpClient object and a plain message 
        {
            _Client = client;

        }


        public string PlainMessage
        {
            get; protected set;
        } = string.Empty;


        public virtual string Method
        {
            get; protected set;
        } = string.Empty;


        public virtual string Path
        {
            get; protected set;
        } = string.Empty;


        public virtual HttpHeader[] Headers
        {
            get; protected set;
        } = Array.Empty<HttpHeader>();


        public virtual string Payload
        {
            get; protected set;
        } = string.Empty;

        //The = string.Empty; at the end of each property is an initialization statement.It means that the property is given an initial value of an empty string ("") when an instance of the class is created.
    }
}
