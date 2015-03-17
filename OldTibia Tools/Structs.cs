using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools
{
    class Structs
    {
        internal class Skill
        {
            internal string Name;
            internal uint CurrentSkill;
            internal int PercentLeft;
            internal uint PercentGained;
            internal double PercentPerHour;
            internal string TimeLeft;
        }

        internal class Pos
        {
            internal Pos(ushort x, ushort y, byte z)
            {
                X = x; Y = y; Z = z;
            }

            internal Pos() { }

            internal ushort X { get; set; }
            internal ushort Y { get; set; }
            internal byte Z { get; set; }
        }
    }
}
