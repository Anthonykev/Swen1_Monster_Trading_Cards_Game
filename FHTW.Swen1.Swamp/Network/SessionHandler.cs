using FHTW.Swen1.Swamp.Interfaces;
using FHTW.Swen1.Swamp.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace FHTW.Swen1.Swamp.Network
{
    public class SessionHandler : Handler, IHandler
    {
        private static Dictionary<string, string> ActiveSessions = new();

        public override bool Handle(HttpSvrEventArgs e)
        {
            if ((e.Path.TrimEnd('/', ' ', '\t') == "/sessions") && (e.Method == "POST"))
            {
                JsonObject? reply = null;
                int status = HttpStatusCode.BAD_REQUEST;

                try
                {
                    JsonNode? json = JsonNode.Parse(e.Payload);
                    if (json != null)
                    {
                        string username = (string)json["username"]!;
                        string password = (string)json["password"]!;
                        (bool Success, string Token) result = User.Logon(username, password);

                        if (result.Success)
                        {
                            ActiveSessions[result.Token] = username;
                            status = HttpStatusCode.OK;
                            reply = new JsonObject()
                            {
                                ["success"] = true,
                                ["message"] = "User logged in.",
                                ["token"] = result.Token
                            };

                            // Antwort mit Header und Token senden
                            var headers = new Dictionary<string, string>
                            {
                                { "Authorization", $"Bearer {result.Token}" }
                            };
                            e.Reply(status, reply?.ToJsonString(), headers);
                            return true;
                        }
                        else
                        {
                            status = HttpStatusCode.UNAUTHORIZED;
                            reply = new JsonObject()
                            {
                                ["success"] = false,
                                ["message"] = "Logon failed."
                            };
                        }
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

            return false;
        }

        public static string? GetUsernameFromToken(string token)
        {
            return ActiveSessions.ContainsKey(token) ? ActiveSessions[token] : null;
        }
    }
}