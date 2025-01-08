using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

using Monster_Traiding_Cards.Security;
using Monster_Traiding_Cards.Server;

namespace Monster_Traiding_Cards.Handlers
{
    public sealed class SessionHandler : Handler, IHandler
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // [override] Handler                                                                                               //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Handles an incoming HTTP request.</summary>
        /// <param name="e">Event arguments.</param>
        public override bool Handle(HttpSvrEventArgs e)
        {
            if (e.Path.TrimEnd('/', ' ', '\t') == "/sessions" && e.Method == "POST")
            {                                                                   // POST /sessions will create a new session
                return _CreateSession(e);
            }

            return false;
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a session.</summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>Returns TRUE.</returns>
        public static bool _CreateSession(HttpSvrEventArgs e)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
            int status = HttpStatusCode.BAD_REQUEST;                            // initialize response

            try
            {
                JsonNode? json = JsonNode.Parse(e.Payload);                     // parse payload
                if (json != null)
                {
                    Session ses = Session.Create((string?)json["username"] ?? "", (string?)json["password"] ?? "");

                    if (ses.Valid)
                    {
                        status = HttpStatusCode.OK;
                        reply = new JsonObject()
                        {
                            ["success"] = true,
                            ["token"] = ses.Token
                        };
                    }
                    else
                    {
                        reply = new JsonObject()
                        {
                            ["success"] = false,
                            ["message"] = "Invalid user name or password"
                        };
                    }
                }
            }
            catch (Exception)
            {                                                                   // handle unexpected exception
                reply = new JsonObject() { ["success"] = false, ["message"] = "Unexpected error." };
            }

            e.Reply(status, reply?.ToJsonString());                             // send response
            return true;
        }
    }
}
