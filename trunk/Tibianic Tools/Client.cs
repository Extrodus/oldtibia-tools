using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TibianicTools
{
    class Client
    {
        internal static Process Tibia = null;
        internal static IntPtr TibiaHandle = new IntPtr();
        internal static Player Player = null;
        internal static string TibiaPath = "";
        internal static ushort TibiaVersion = 0;
        internal static WinApi.RECT ClientRECT
        {
            get
            {
                WinApi.RECT rect = new WinApi.RECT();
                WinApi.GetClientRect(Client.Tibia.MainWindowHandle, out rect);
                return rect;
            }
        }
        internal static WinApi.RECT ScreenRECT
        {
            get
            {
                WinApi.RECT rect = new WinApi.RECT();
                WinApi.GetWindowRect(Client.Tibia.MainWindowHandle, out rect);
                return rect;
            }
        }

        internal class Charlist
        {
            internal static void WriteIP(string IP)
            {
                int CharacterlistStart = Memory.ReadInt(Addresses.Charlist.Pointer);
                int NumberOfCharacters = Memory.ReadInt(Addresses.Charlist.NumberOfCharacters);
                for (int i = CharacterlistStart; i < CharacterlistStart + (NumberOfCharacters * Addresses.Charlist.Step); i += Addresses.Charlist.Step)
                {
                    Memory.WriteString(i + Addresses.Charlist.DistanceServerIP, IP);
                    Memory.WriteInt32(i + Addresses.Charlist.DistanceServerPort, Proxy.ServerPort);
                }
            }

            internal static void WriteIP(List<CharacterList.Player> Players)
            {
                int CharacterlistStart = Memory.ReadInt(Addresses.Charlist.Pointer);
                int NumberOfCharacters = Memory.ReadInt(Addresses.Charlist.NumberOfCharacters);
                if (NumberOfCharacters != Players.Count)
                {
                    return;
                }
                int j = 0;
                for (int i = CharacterlistStart; i < CharacterlistStart + (NumberOfCharacters * Addresses.Charlist.Step); i += Addresses.Charlist.Step)
                {
                    string currentChar = Memory.ReadString(i + Addresses.Charlist.DistanceCharacter);
                    if (Players[j].Name == currentChar)
                    {
                        Memory.WriteString(i + Addresses.Charlist.DistanceServerIP, Players[j].IP);
                        Memory.WriteInt32(i + Addresses.Charlist.DistanceServerPort, Players[j].Port);
                    }
                    j++;
                }
            }
        }

        internal class Misc
        {
            internal static int DialogPointer { get { return Memory.ReadInt(Addresses.Client.DialogPointer); } }
            internal static string DialogTitle { get { return Memory.ReadString(DialogPointer + Addresses.Client.DialogDistanceTitle); } }

            internal static string[] GetLoginServers()
            {
                string[] LoginServers = new string[6];
                for (int i = 0; i < 5; i++)
                {
                    LoginServers[i] = Memory.ReadString(Addresses.Client.LoginServerStart +
                                                       (Addresses.Client.LoginServerStep * i));
                }
                return LoginServers;
            }

            internal static void SetLoginServers(string IP, int Port)
            {
                for (int i = 0; i < 5; i++)
                {
                    Memory.WriteString(Addresses.Client.LoginServerStart + Addresses.Client.LoginServerStep * i, IP);
                    Memory.WriteInt32(Addresses.Client.LoginServerStart + Addresses.Client.LoginServerDistancePort +
                                     (Addresses.Client.LoginServerStep * i), Port);
                }
            }

            internal static void WriteStatusBar(string text, int seconds)
            {
                Memory.WriteString(Addresses.Client.StatusBarText, text);
                Memory.WriteInt32(Addresses.Client.StatusBarTime, seconds * 10);
            }

            internal static void SetXTEAKey(uint[] XTEAKey)
            {
                Memory.WriteUInt32(Addresses.Client.XTEAKey, XTEAKey[0]);
                Memory.WriteUInt32(Addresses.Client.XTEAKey + 4, XTEAKey[1]);
                Memory.WriteUInt32(Addresses.Client.XTEAKey + 8, XTEAKey[2]);
                Memory.WriteUInt32(Addresses.Client.XTEAKey + 12, XTEAKey[3]);
            }

            internal static double FPS
            {
                get
                {
                    int pointer = Memory.ReadInt(Addresses.Client.FPSPointer);
                    pointer += Addresses.Client.FPSCurrentFPSOffset;
                    return Math.Round(Memory.ReadDouble(pointer), 2);
                }
                set
                {
                    double fps = Math.Round(1000 / FPS, 1);
                    int pointer = Memory.ReadInt(Addresses.Client.FPSPointer);
                    pointer += Addresses.Client.FPSCurrentLimitOffset;
                    Memory.WriteDouble(pointer, fps);
                }
            }

            /// <summary>
            /// Logs in using PostMessage.
            /// </summary>
            /// <param name="accountNumber"></param>
            /// <param name="password"></param>
            /// <param name="character"></param>
            internal static void Login(uint accountNumber, string password, string character)
            {
                if (Memory.ReadByte(Addresses.Client.Connection) == 0)
                {
                    while (WinApi.IsIconic(Client.Tibia.MainWindowHandle))
                    {
                        WinApi.ShowWindow(Client.Tibia.MainWindowHandle, WinApi.SW_SHOW);
                        System.Threading.Thread.Sleep(500);
                    }
                    while (Memory.ReadByte(Addresses.Client.DialogOpen) != 0)
                    {
                        Utils.SendTibiaKeys("escape");
                        System.Threading.Thread.Sleep(300);
                    }
                    Memory.WriteByte(Addresses.Charlist.NumberOfCharacters, 0);
                    WinApi.RECT rect = Client.ClientRECT;
                    bool inputBoxFocused = false;
                    while (!inputBoxFocused)
                    {
                        rect = Client.ClientRECT;
                        Utils.SendMouseClick(120, rect.bottom - 215);
                        for (int i = 0; i < 5; i++)
                        {
                            if (Memory.ReadByte(Addresses.Client.DialogOpen) > 0 &&
                                Client.Misc.DialogTitle == "Enter Game")
                            {
                                inputBoxFocused = true;
                                break;
                            }
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    inputBoxFocused = false;
                    while (!inputBoxFocused)
                    {
                        Utils.SendTibiaString(accountNumber.ToString());
                        System.Threading.Thread.Sleep(100);
                        Utils.SendTibiaKey(Keys.Tab);
                        System.Threading.Thread.Sleep(100);
                        Utils.SendTibiaString(password);
                        System.Threading.Thread.Sleep(100);
                        Utils.SendTibiaKeys("enter");
                        for (int i = 0; i < 10; i++)
                        {
                            if (Memory.ReadByte(Addresses.Client.DialogOpen) > 0 &&
                                Client.Misc.DialogTitle == "Select Character" ||
                                Client.Misc.DialogTitle == "Connecting")
                            {
                                inputBoxFocused = true;
                                break;
                            }
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    while (Client.Misc.DialogTitle == "Connecting")
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    System.Threading.Thread.Sleep(1000);
                    if (Client.Misc.DialogTitle.StartsWith("Message"))
                    {
                        Utils.SendTibiaKeys("enter");
                        System.Threading.Thread.Sleep(1000);
                    }
                    if (Client.Misc.DialogTitle == "Select Character")
                    {
                        foreach (CharacterList.Player _player in CharacterList.Players)
                        {
                            if (_player.Name.ToLower() == character.ToLower())
                            {
                                Utils.SendTibiaKeys("enter");
                                break;
                            }
                            System.Threading.Thread.Sleep(100);
                            Utils.SendTibiaKeys("down");
                        }
                    }
                }
            }
        }
    }
}
