using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools.Objects
{
    class Packet
    {
        internal Packet(byte[] packet) { listPacket.AddRange(packet); }
        internal Packet(byte[] packet, int length)
        {
            if (length > packet.Length) { listPacket.AddRange(packet); }
            else
            {
                byte[] packetShortened = new byte[length];
                Array.Copy(packet, packetShortened, length);
                listPacket.AddRange(packetShortened);
            }
        }
        internal Packet(byte[] packet, int length, int index)
        {
            if (length > packet.Length || index >= length) { return; }
            else
            {
                byte[] packetShortened = new byte[length];
                Array.Copy(packet, index, packetShortened, 0, length);
                listPacket.AddRange(packetShortened);
            }
        }
        internal Packet() { }

        private List<byte> listPacket = new List<byte>();

        internal List<byte> ToList() { return listPacket; }
        internal byte[] ToBytes() { return listPacket.ToArray(); }
        internal byte[] ToBytes(int index)
        {
            List<byte> templist = new List<byte>();
            templist.AddRange(listPacket.ToArray());
            templist.RemoveRange(0, index);
            return templist.ToArray();
        }
        public override string ToString()
        {
            return BitConverter.ToString(ToBytes());
        }
        internal bool Send(System.Net.Sockets.TcpClient tcpclient)
        {
            return this.Send(tcpclient, 0);
        }
        internal bool Send(System.Net.Sockets.TcpClient tcpclient, int index)
        {
            if (listPacket.Count > index && tcpclient != null && tcpclient.Connected)
            {
                byte[] buffer = this.ToBytes(index);
                tcpclient.GetStream().Write(buffer, 0, buffer.Length);
                return true;
            }
            return false;
        }
        internal int Length { get { return listPacket.Count; } }

        internal void AddByte(byte value) { listPacket.Add(value); }
        internal void AddUInt16(ushort value) { listPacket.AddRange(BitConverter.GetBytes(value)); }
        internal void AddUInt32(uint value) { listPacket.AddRange(BitConverter.GetBytes(value)); }
        internal void AddUInt64(ulong value) { listPacket.AddRange(BitConverter.GetBytes(value)); }
        internal void AddBytes(byte[] value) { listPacket.AddRange(value); }
        internal void AddString(string value) { AddUInt16((ushort)value.Length); listPacket.AddRange(ASCIIEncoding.Default.GetBytes(value)); }
        internal void AddLength() { listPacket.InsertRange(0, BitConverter.GetBytes((ushort)this.Length)); }

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
            string s = ASCIIEncoding.Default.GetString(listPacket.ToArray(), GetPosition, length);
            GetPosition += length;
            return s;
        }
        internal string GetString()
        {
            return GetString(this.GetUInt16());
        }

        /// <summary>
        /// Encrypts the packet with Xtea. Packet length should be re-added after encrypting.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal unsafe bool XteaEncrypt(uint[] key, int index)
        {
            if (key == null || index >= this.Length) return false;

            byte[] tempbuffer = this.ToBytes(index);
            int msgSize = tempbuffer.Length;

            int pad = msgSize % 8;
            if (pad > 0) msgSize += (8 - pad);

            // add filler junk data
            byte[] buffer = new byte[msgSize];
            new Random().NextBytes(buffer);
            Array.Copy(tempbuffer, buffer, tempbuffer.Length);

            fixed (byte* bufferPtr = buffer)
            {
                uint* words = (uint*)(bufferPtr);

                for (int pos = 0; pos < msgSize / 4; pos += 2)
                {
                    uint x_sum = 0, x_delta = 0x9e3779b9, x_count = 32;

                    while (x_count-- > 0)
                    {
                        words[pos] += (words[pos + 1] << 4 ^ words[pos + 1] >> 5) + words[pos + 1] ^ x_sum + key[x_sum & 3];
                        x_sum += x_delta;
                        words[pos + 1] += (words[pos] << 4 ^ words[pos] >> 5) + words[pos] ^ x_sum + key[x_sum >> 11 & 3];
                    }
                }
            }
            this.listPacket.Clear();
            this.listPacket.AddRange(buffer);
            return true;
        }
        /// <summary>
        /// Encrypts the packet with Xtea. Packet length should be re-added after encrypting.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal unsafe bool XteaEncrypt(uint[] key) { return this.XteaEncrypt(key, 0); }
        /// <summary>
        /// Decrypts a packet that is encrypted with Xtea.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <param name="index"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal unsafe Packet XteaDecrypt(uint[] key, int index)
        {
            if (this.Length <= index || (this.Length - index) % 8 > 0 || key == null)
                return null;

            byte[] buffer = this.ToBytes();
            fixed (byte* bufferPtr = buffer)
            {
                uint* words = (uint*)(bufferPtr + index);
                int msgSize = this.Length - index;

                for (int pos = 0; pos < msgSize / 4; pos += 2)
                {
                    uint x_count = 32, x_sum = 0xC6EF3720, x_delta = 0x9E3779B9;

                    while (x_count-- > 0)
                    {
                        words[pos + 1] -= (words[pos] << 4 ^ words[pos] >> 5) + words[pos] ^ x_sum
                            + key[x_sum >> 11 & 3];
                        x_sum -= x_delta;
                        words[pos] -= (words[pos + 1] << 4 ^ words[pos + 1] >> 5) + words[pos + 1] ^ x_sum
                            + key[x_sum & 3];
                    }
                }
            }
            ushort decryptedLen = BitConverter.ToUInt16(buffer, index);
            byte[] decryptedBuffer = new byte[decryptedLen + 2];
            Array.Copy(buffer, index, decryptedBuffer, 0, decryptedBuffer.Length);
            Packet p = new Packet();
            p.AddBytes(decryptedBuffer);
            return p;
        }

        internal Packet Clone() { return new Packet(this.ToBytes()); }

        internal static Packet FromFile(string path)
        {
            if (!System.IO.File.Exists(path)) { return null; }
            byte[] buffer = new byte[1];
            using (System.IO.FileStream fstream = System.IO.File.OpenRead(path))
            {
                buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);
            }
            return new Packet(buffer);
        }
        internal static Packet GetNextPacket(System.Net.Sockets.NetworkStream nstream, ushort length)
        {
            Packet p = new Packet();
            try
            {
                int bytesRead = 0, bytesReadTotal = 0;
                byte[] buffer = new byte[length];
                while (bytesReadTotal < length)
                {
                    try { bytesRead = nstream.Read(buffer, 0, length - bytesReadTotal); }
                    catch { break; }
                    if (bytesRead == 0) break;
                    byte[] tempBytes = new byte[bytesRead];
                    Array.Copy(buffer, tempBytes, bytesRead);
                    p.AddBytes(tempBytes);
                    bytesReadTotal += bytesRead;
                }
                p.AddLength();
            }
            catch { }
            return p;
        }
        internal static Packet GetNextPacket(System.Net.Sockets.NetworkStream nstream)
        {
            Packet p = new Packet();
            try
            {
                int bytesRead = 0;
                ushort length = 0;
                byte[] buffer = new byte[8192];

                // read first 2 bytes (packet length)
                try { bytesRead = nstream.Read(buffer, 0, 2); }
                catch { return p; }
                if (bytesRead == 0) return p;
                length = BitConverter.ToUInt16(buffer, 0);

                return GetNextPacket(nstream, length);
            }
            catch { }
            return p;
        }
    }
}
