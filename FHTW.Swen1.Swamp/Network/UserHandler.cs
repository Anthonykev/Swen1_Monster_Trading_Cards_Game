using FHTW.Swen1.Swamp.Interfaces;
using FHTW.Swen1.Swamp.Models;
using FHTW.Swen1.Swamp.Exceptions;
using FHTW.Swen1.Swamp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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
            else if (e.Path.StartsWith("/users"))
            {
                // Abrufen aller Benutzer
                if (e.Method == "GET" && e.Path == "/users")
                {
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
                // Abrufen eines bestimmten Benutzers
                else if (e.Method == "GET" && e.Path.StartsWith("/users/"))
                {
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
                        else
                        {
                            status = HttpStatusCode.NOT_FOUND;
                            reply = new JsonObject()
                            {
                                ["success"] = false,
                                ["message"] = "User not found."
                            };
                        }
                    }
                    else
                    {
                        status = HttpStatusCode.NOT_FOUND;
                        reply = new JsonObject()
                        {
                            ["success"] = false,
                            ["message"] = "User not found."
                        };
                    }

                    e.Reply(status, reply?.ToJsonString());
                    return true;
                }
                // Hinzufügen eines Kartenpakets zum Stack des Benutzers
                else if (e.Method == "POST" && e.Path.EndsWith("/stack/add-package"))
                {
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
                // Auswählen eines neuen Decks aus dem Stack
                else if (e.Method == "POST" && e.Path.EndsWith("/deck/choose"))
                {
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
            }

            return false;
        }
    }
}