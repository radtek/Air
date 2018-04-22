using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;

namespace AirFlight.Models
{
    public class ActiveSessions
    {
        private static List<string> _sessionInfo;
        private static readonly object padlock = new object();

        public static List<string> Sessions
        {
            get
            {
                lock (padlock)
                {
                    if (_sessionInfo == null)
                    {
                        _sessionInfo = new List<string>();
                    }

                    return _sessionInfo;
                }
            }
        }

        public static int Count
        {
            get
            {
                lock (padlock)
                {
                    if (_sessionInfo == null)
                    {
                        _sessionInfo = new List<string>();
                    }
                    return _sessionInfo.Count;
                }
            }
        }       
    
    }
}