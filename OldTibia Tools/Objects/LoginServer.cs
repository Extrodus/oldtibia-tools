using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools.Objects
{
    class LoginServer
    {
        internal LoginServer(string ip, int port)
        {
            IP = ip; Port = port;
        }

        internal string IP { get; set; }
        internal int Port { get; set; }

        public override string ToString()
        {
            return this.IP + ":" + this.Port;
        }
    }
}
