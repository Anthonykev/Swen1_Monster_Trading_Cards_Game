﻿using Microsoft.Extensions.Configuration;
using Monster_Trading_Cards_Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monster_Trading_Cards_Game.Network
{
    /// <summary>This class provides an abstract implementation of the
    /// <see cref="IHandler"/> interface. It also implements static methods
    /// that handles an incoming HTTP request by discovering and calling
    /// available handlers.</summary>
    public abstract class Handler : IHandler
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>List of available handlers.</summary>
        private static List<IHandler>? _Handlers = null;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Discovers and returns all available handler implementations.</summary>
        /// <returns>Returns a list of available handlers.</returns>
        private static List<IHandler> _GetHandlers(IConfiguration configuration)
        {
            List<IHandler> rval = new();

            foreach (Type i in Assembly.GetExecutingAssembly().GetTypes()
                                      .Where(m => m.IsAssignableTo(typeof(IHandler)) && !m.IsAbstract))
            {
                if (i == typeof(SessionHandler) || i == typeof(UserHandler))
                {
                    // Explizite Instanziierung von SessionHandler und UserHandler mit der Konfiguration
                    IHandler? h = (IHandler?)Activator.CreateInstance(i, configuration);
                    if (h != null)
                    {
                        rval.Add(h);
                    }
                }
                else
                {
                    // Andere Handler ohne Konfiguration
                    IHandler? h = (IHandler?)Activator.CreateInstance(i);
                    if (h != null)
                    {
                        rval.Add(h);
                    }
                }
            }

            return rval;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Handles an incoming HTTP request.</summary>
        /// <param name="e">Event arguments.</param>
        /// <param name="configuration">Configuration instance.</param>
        public static void HandleEvent(HttpSvrEventArgs e, IConfiguration configuration)
        {
            _Handlers ??= _GetHandlers(configuration);

            foreach (IHandler i in _Handlers)
            {
                if (i.Handle(e)) return;
            }
            e.Reply(HttpStatusCode.BAD_REQUEST);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // [interface] IHandler                                                                                             //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Tries to handle a HTTP request.</summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>Returns TRUE if the request was handled by this instance,
        ///          otherwise returns FALSE.</returns>
        public abstract bool Handle(HttpSvrEventArgs e);
    }
}
