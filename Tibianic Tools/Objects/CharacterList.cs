using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools
{
    class CharacterList
    {
        internal CharacterList() { }

        internal static List<CharacterList.Player> Players
        {
            get
            {
                List<CharacterList.Player> players = new List<CharacterList.Player>();
                byte NumberOfCharacters = Memory.ReadByte(Addresses.Charlist.NumberOfCharacters);
                int CharacterlistStart = Memory.ReadInt(Addresses.Charlist.Pointer);
                byte j = 0;
                for (int i = CharacterlistStart; i < CharacterlistStart + (NumberOfCharacters * Addresses.Charlist.Step); i += Addresses.Charlist.Step)
                {
                    players.Add(new Player(j, Memory.ReadString(i),
                                              Memory.ReadString(i + Addresses.Charlist.DistanceServerName),
                                              Memory.ReadString(i + Addresses.Charlist.DistanceServerIP),
                                              Memory.ReadInt(i + Addresses.Charlist.DistanceServerPort)));
                    j++;
                }
                return players;
            }
        }

        internal class Player
        {
            private int _port;
            private string _ip, _serverName;

            internal Player(byte index, string name, string serverName, string ip, int port)
            {
                Index = index;
                Name = name;
                _serverName = serverName;
                _ip = ip;
                _port = port;
            }

            internal byte Index { get; set; }
            internal string Name { get; set; }

            internal string ServerName
            {
                get { return _serverName; }
                set 
                { 
                    int charlistStart = Memory.ReadInt(Addresses.Charlist.Pointer);
                    if (Memory.ReadString(charlistStart + (Index * Addresses.Charlist.Step)) == Name)
                    {
                        Memory.WriteString(charlistStart + Addresses.Charlist.DistanceServerName + (Index * Addresses.Charlist.Step), value);
                    }
                    _serverName = value;
                }
            }

            internal string IP
            {
                get { return _ip; }
                set
                {
                    int charlistStart = Memory.ReadInt(Addresses.Charlist.Pointer);
                    string asd = Memory.ReadString(charlistStart + (Index * Addresses.Charlist.Step));
                    if (Memory.ReadString(charlistStart + (Index * Addresses.Charlist.Step)) == Name)
                    {
                        Memory.WriteString(charlistStart + Addresses.Charlist.DistanceServerIP + (Index * Addresses.Charlist.Step), value);
                    }
                    _ip = value;
                }
            }

            internal int Port
            {
                get { return _port; }
                set
                {
                    int charlistStart = Memory.ReadInt(Addresses.Charlist.Pointer);
                    if (Memory.ReadString(charlistStart + (Index * Addresses.Charlist.Step)) == Name)
                    {
                        Memory.WriteInt32(charlistStart + Addresses.Charlist.DistanceServerPort + (Index * Addresses.Charlist.Step), value);
                    }
                    _port = value;
                }
            }
        }
    }
}
