using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TibianicTools
{
    class Memory
    {
        internal static byte[] ReadBytes(long address, uint bytesToRead)
        {
            try
            {
                IntPtr ptrBytesRead;
                byte[] buffer = new byte[bytesToRead];
                WinApi.ReadProcessMemory(Client.TibiaHandle, new IntPtr(address), buffer, bytesToRead, out ptrBytesRead);
                return buffer;
            }
            catch
            {
                if (Client.Tibia.HasExited) { System.Windows.Forms.Application.Exit(); }
                return new byte[bytesToRead];
            }
        }

        internal static int ReadInt(long address)
        {
            return BitConverter.ToInt32(ReadBytes(address, 4), 0);
        }

        internal static ushort ReadUShort(long address)
        {
            return BitConverter.ToUInt16(ReadBytes(address, 2), 0);
        }

        internal static byte ReadByte(long address)
        {
            return ReadBytes(address, 1)[0];
        }

        internal static uint ReadUInt(long address)
        {
            return BitConverter.ToUInt32(ReadBytes(address, 4), 0);
        }

        internal static double ReadDouble(long address)
        {
            return BitConverter.ToDouble(ReadBytes(address, 8), 0);
        }

        internal static string ReadString(long address)
        {
            string stringRead = Encoding.ASCII.GetString(ReadBytes(address, 32));
            return stringRead.Substring(0, stringRead.IndexOf('\0'));
        }
        internal static string ReadString(long address, int length)
        {
            return Encoding.ASCII.GetString(ReadBytes(address, (uint)length));
        }

        internal static bool WriteBytes(long address, byte[] bytes, uint length)
        {
            try
            {
                IntPtr bytesWritten;
                int result = WinApi.WriteProcessMemory(Client.TibiaHandle, new IntPtr(address), bytes, length, out bytesWritten);
                return result != 0;
            }
            catch { return false; }
        }

        internal static bool WriteByte(long address, byte value)
        {
            return WriteBytes(address, new byte[] { value }, 1);
        }

        internal static bool WriteUShort(long address, ushort value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value), 2);
        }

        internal static bool WriteInt32(long address, int value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value), 4);
        }

        internal static bool WriteUInt32(long address, uint value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value), 4);
        }

        internal static bool WriteDouble(long address, double value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value), 8);
        }

        internal static bool WriteString(long address, string text)
        {
            return WriteBytes(address, Encoding.ASCII.GetBytes(text + '\0'), (uint)text.Length + 1);
        }
    }
}