using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster_Traiding_Cards.Server;

namespace Monster_Traiding_Cards.Handlers
{
    /// <summary>Handlers that handle HTTP requests implement this interface.</summary>
    public interface IHandler
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Tries to handle a HTTP request.</summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>Returns TRUE if the request was handled by this instance,
        ///          otherwise returns FALSE.</returns>
        public bool Handle(HttpSvrEventArgs e);
    }
}