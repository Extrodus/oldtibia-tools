using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools
{
    class Enums
    {
        internal enum Recorder : byte
        {
            TibianicTools = 1,
            TibiaMovie = 2,
            IryontCam = 3
        }

        internal enum Skill : byte
        {
            Axe = 0,
            Club = 1,
            Sword = 2,
            Fist = 3,
            Distance = 4,
            MagicLevel = 5,
            Shielding = 6,
            Fishing = 7
        }

        internal enum Connection : byte
        {
            Offline = 0,
            WaitingForCharacterList = 3,
            ConnectingGameServer = 6,
            Online = 8
        }

        internal static class RSAKey
        {
            internal static readonly string OpenTibia = "109120132967399429278860960508995541528237502902798129123468757937266291492576446330739696001110603907230888610072655818825358503429057592827629436413108566029093628212635953836686562675849720620786279431090218017681061521755056710823876476444260558147179707119674283982419152118103759076030616683978566631413";
        }
    }
}
