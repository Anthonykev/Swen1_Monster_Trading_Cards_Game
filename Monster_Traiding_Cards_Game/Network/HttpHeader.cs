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
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="header">Raw header string.</param>
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


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the header name.</summary>
        public string Name
        {
            get; protected set;
        }

        /// <summary>Gets the header value.</summary>
        public string Value
        {
            get; protected set;
        }
    }
}
