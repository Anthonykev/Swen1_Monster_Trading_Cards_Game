using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//5)

namespace FHTW.Swen1.Swamp.Network
{
    public class HttpHeader  //Declares a class HttpHeader which represents a single HTTP header.
    {
        public HttpHeader(string header)
        {
            Name = Value = string.Empty;

            try
            {
                int n = header.IndexOf(':');  //Finds the index of the : character, which separates the header name and value.
                Name = header.Substring(0, n).Trim(); //Extracts the part of the header string before : and trims any whitespace, assigning it to Name.
                Value = header.Substring(n + 1).Trim();
            }
            catch (Exception) { }
        }


        public string Name
        {
            get; protected set;
        }


        public string Value
        {
            get; protected set;
        }
    }
}
