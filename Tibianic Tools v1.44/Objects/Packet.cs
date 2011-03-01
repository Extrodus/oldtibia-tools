using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools
{
    class Packet
    {
        internal Packet(byte[] packet) { listPacket.AddRange(packet); }
        internal Packet(byte[] packet, int length)
        {
            if (length >= packet.Length) { listPacket.AddRange(packet); }
            else
            {
                byte[] packetShortened = new byte[length];
                Array.Copy(packet, packetShortened, length);
                listPacket.AddRange(packetShortened);
            }
        }
        internal Packet() { }

        private List<byte> listPacket = new List<byte>();

        internal byte[] ToBytes() { return listPacket.ToArray(); }
        public override string ToString()
        {
            return BitConverter.ToString(ToBytes());
        }
        internal bool Send(System.Net.Sockets.TcpClient tcpclient)
        {
            return Utils.Network.SendPacket(tcpclient, this);
        }

        internal void AddByte(byte value) { listPacket.Add(value); }
        internal void AddUInt16(ushort value) { listPacket.AddRange(BitConverter.GetBytes(value)); }
        internal void AddUInt32(uint value) { listPacket.AddRange(BitConverter.GetBytes(value)); }
        internal void AddUInt64(ulong value) { listPacket.AddRange(BitConverter.GetBytes(value)); }
        internal void AddBytes(byte[] value) { listPacket.AddRange(value); }
        internal void AddString(string value) { listPacket.AddRange(ASCIIEncoding.ASCII.GetBytes(value)); }

        internal int GetPosition = 0;
        internal byte GetByte()
        {
            byte val = listPacket[GetPosition];
            GetPosition++;
            return val;
        }
        internal ushort GetUInt16()
        {
            ushort val = BitConverter.ToUInt16(listPacket.ToArray(), GetPosition);
            GetPosition += 2;
            return val;
        }
        internal uint GetUInt32()
        {
            uint val = BitConverter.ToUInt32(listPacket.ToArray(), GetPosition); 
            GetPosition += 4;
            return val;
        }
        internal ulong GetUInt64()
        {
            ulong val = BitConverter.ToUInt64(listPacket.ToArray(), GetPosition);
            GetPosition += 8;
            return val;
        }
        internal byte[] GetBytes(int length)
        {
            byte[] b = new byte[length];
            Array.Copy(listPacket.ToArray(), GetPosition, b, 0, length);
            GetPosition += length;
            return b;
        }
        internal string GetString(int length)
        {
            string s = ASCIIEncoding.ASCII.GetString(listPacket.ToArray(), GetPosition, length);
            GetPosition += length;
            return s;
        }
    }
}
