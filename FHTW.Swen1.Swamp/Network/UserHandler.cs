using FHTW.Swen1.Swamp.Interfaces;
using FHTW.Swen1.Swamp.Models;
using FHTW.Swen1.Swamp.Exceptions;
using FHTW.Swen1.Swamp.Network;
using System;
using System.Text.Json.Nodes;

namespace FHTW.Swen1.Swamp.Network
{
    /// <summary>This class implements a handler for user-specific requests.</summary>
    public class UserHandler : Handler, IHandler
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // [override] Handler                                                                                               //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Handles an incoming HTTP request.</summary>
        /// <param name="e">Event arguments.</param>
        public override bool Handle(HttpSvrEventArgs e)
        {
            JsonObject? reply = null;
            int status = HttpStatusCode.BAD_REQUEST;

            // Benutzer erstellen
            if ((e.Path.TrimEnd('/', ' ', '\t') == "/users") && (e.Method == "POST"))
            {
                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    if (json != null)
                    {
                        User.Create((string)json["username"]!,
                            (string)json["password"]!,
                            (string?)json["fullname"] ?? "",
                            (string?)json["email"] ?? "");
                        status = HttpStatusCode.OK;
                        reply = new JsonObject()
                        {
                            ["success"] = true,
                            ["message"] = "User created."
                        };
                    }
                }
                catch (UserException ex)
                {
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = ex.Message
                    };
                }
                catch (Exception)
                {
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Invalid request."
                    };
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }
            // Informationen über den eingeloggten Benutzer abrufen
            else if ((e.Path == "/users/me") && (e.Method == "GET"))
            {
                (bool Success, User? User) ses = Token.Authenticate(e);

                if (ses.Success)
                {
                    status = HttpStatusCode.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["username"] = ses.User!.UserName,
                        ["fullname"] = ses.User!.FullName,
                        ["email"] = ses.User!.EMail
                    };
                }
                else
                {
                    status = HttpStatusCode.UNAUTHORIZED;
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Unauthorized."
                    };
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }
            // Weitere Benutzer-bezogene Anfragen
            else if (e.Path.StartsWith("/users"))
            {
                if (e.Method == "GET" && e.Path == "/users")
                {
                    return HandleGetAllUsers(e);
                }
                else if (e.Method == "GET" && e.Path.StartsWith("/users/"))
                {
                    return HandleGetUser(e);
                }
                else if (e.Method == "POST" && e.Path.EndsWith("/stack/add-package"))
                {
                    return HandleAddPackage(e);
                }
                else if (e.Method == "POST" && e.Path.EndsWith("/deck/choose"))
                {
                    return HandleChooseDeck(e);
                }
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Private Methoden                                                                                                 //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Handles retrieving all users.</summary>
        private bool HandleGetAllUsers(HttpSvrEventArgs e)
        {
            JsonObject? reply = null;
            int status = HttpStatusCode.UNAUTHORIZED;

            (bool Success, User? User) ses = Token.Authenticate(e);

            if (ses.Success)
            {
                JsonArray usersArray = new JsonArray();
                foreach (var user in User.GetAllUsers())
                {
                    usersArray.Add(new JsonObject()
                    {
                        ["username"] = user.UserName,
                        ["fullname"] = user.FullName,
                        ["email"] = user.EMail
                    });
                }

                status = HttpStatusCode.OK;
                reply = new JsonObject()
                {
                    ["success"] = true,
                    ["users"] = usersArray
                };
            }
            else
            {
                reply = new JsonObject()
                {
                    ["success"] = false,
                    ["message"] = "Unauthorized."
                };
            }

            e.Reply(status, reply?.ToJsonString());
            return true;
        }

        /// <summary>Handles retrieving a specific user.</summary>
        private bool HandleGetUser(HttpSvrEventArgs e)
        {
            JsonObject? reply = null;
            int status = HttpStatusCode.NOT_FOUND;

            string requestedUser = e.Path.Substring("/users/".Length);

            if (User.Exists(requestedUser))
            {
                User? user = User.Get(requestedUser);
                if (user != null)
                {
                    status = HttpStatusCode.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["username"] = user.UserName,
                        ["fullname"] = user.FullName,
                        ["email"] = user.EMail
                    };
                }
            }
            else
            {
                reply = new JsonObject()
                {
                    ["success"] = false,
                    ["message"] = "User not found."
                };
            }

            e.Reply(status, reply?.ToJsonString());
            return true;
        }

        /// <summary>Handles adding a package to the user's stack.</summary>
        private bool HandleAddPackage(HttpSvrEventArgs e)
        {
            JsonObject? reply = null;
            int status = HttpStatusCode.UNAUTHORIZED;

            (bool Success, User? User) ses = Token.Authenticate(e);

            if (ses.Success)
            {
                ses.User!.AddPackage();
                status = HttpStatusCode.OK;
                reply = new JsonObject()
                {
                    ["success"] = true,
                    ["message"] = "Package added to user's stack."
                };
            }
            else
            {
                reply = new JsonObject()
                {
                    ["success"] = false,
                    ["message"] = "Unauthorized."
                };
            }

            e.Reply(status, reply?.ToJsonString());
            return true;
        }

        /// <summary>Handles selecting a new deck from the user's stack.</summary>
        private bool HandleChooseDeck(HttpSvrEventArgs e)
        {
            JsonObject? reply = null;
            int status = HttpStatusCode.UNAUTHORIZED;

            (bool Success, User? User) ses = Token.Authenticate(e);

            if (ses.Success)
            {
                ses.User!.ChooseDeck();
                status = HttpStatusCode.OK;
                reply = new JsonObject()
                {
                    ["success"] = true,
                    ["message"] = "Deck selected from user's stack."
                };
            }
            else
            {
                reply = new JsonObject()
                {
                    ["success"] = false,
                    ["message"] = "Unauthorized."
                };
            }

            e.Reply(status, reply?.ToJsonString());
            return true;
        }
    }
}