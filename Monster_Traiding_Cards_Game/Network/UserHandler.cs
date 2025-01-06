using Monster_Trading_Cards_Game.Interfaces;
using Monster_Trading_Cards_Game.Models;
using Monster_Trading_Cards_Game.Exceptions;
using Monster_Trading_Cards_Game.Network;
using System;
using System.Text.Json.Nodes;
using Monster_Traiding_Cards.Database;

namespace Monster_Trading_Cards_Game.Network
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

            // Tabellen erstellen
            if ((e.Path.TrimEnd('/', ' ', '\t') == "/create-tables") && (e.Method == "POST"))
            {
                Database db = new Database("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                if (db.CreateTables())
                {
                    status = HttpStatusCode.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["message"] = "Tabellen wurden erstellt oder existieren bereits."
                    };
                }
                else
                {
                    status = HttpStatusCode.INTERNAL_SERVER_ERROR;
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Fehler beim Erstellen der Tabellen."
                    };
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }
            // Standardkarten zur Datenbank hinzufügen
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/add-default-cards") && (e.Method == "POST"))
            {
                Database db = new Database("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                if (db.AddDefaultCards())
                {
                    status = HttpStatusCode.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["message"] = "Standardkarten wurden hinzugefügt."
                    };
                }
                else
                {
                    status = HttpStatusCode.INTERNAL_SERVER_ERROR;
                    reply = new JsonObject()
                    {
                        ["success"] = false,
                        ["message"] = "Fehler beim Hinzufügen der Standardkarten."
                    };
                }

                e.Reply(status, reply?.ToJsonString());
                return true;
            }
            // Benutzer registrieren
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/register") && (e.Method == "POST"))
            {
                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    if (json != null)
                    {
                        Database db = new Database("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                        bool success = db.RegisterUser((string)json["username"]!, (string)json["password"]!, (string)json["fullname"]!, (string)json["email"]!);
                        status = success ? HttpStatusCode.OK : HttpStatusCode.BAD_REQUEST;
                        reply = new JsonObject()
                        {
                            ["success"] = success,
                            ["message"] = success ? "Registrierung erfolgreich." : "Registrierung fehlgeschlagen."
                        };
                    }
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
            // Benutzer authentifizieren
            else if ((e.Path.TrimEnd('/', ' ', '\t') == "/login") && (e.Method == "POST"))
            {
                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    if (json != null)
                    {
                        Database db = new Database("Host=localhost;Port=5432;Username=kevin;Password=spiel12345;Database=monster_cards");
                        var (success, token) = db.AuthenticateUser((string)json["username"]!, (string)json["password"]!);
                        status = success ? HttpStatusCode.OK : HttpStatusCode.UNAUTHORIZED;
                        reply = new JsonObject()
                        {
                            ["success"] = success,
                            ["token"] = token,
                            ["message"] = success ? "Anmeldung erfolgreich." : "Anmeldung fehlgeschlagen."
                        };
                    }
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
