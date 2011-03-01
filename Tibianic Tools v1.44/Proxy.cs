using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace TibianicTools
{
    /// <summary>
    /// I deeply apologize for the mess in this class, some day I will do a complete re-write of all this
    /// </summary>
    class Proxy
    {
        private static uint[] XTEAKey = new uint[4];
        internal static TcpClient tcpServer = new TcpClient(), tcpClient = new TcpClient();
        internal static TcpListener listenerLocal;
        internal static string MotD = "", MovieFileName = "", ServerIP = "", AutoPlayBackName = "", AutoRecordIP = "";
        internal static int ServerPort = 7171, MinimumRecordedPackets = 5, currentPacket = 0, AutoRecordPort = 7171;
        internal static double TimePlayed = 0, TimeRecorded = 0, TimeTotalPlayback = 0, PlaybackSpeed = 1;
        internal static string[] LoginServersOriginal = new string[1];
        internal static bool Recording = false, Playing = false, Connected = false, doAutoRecord = false, doAutoPlayback = false, Rewind = false;
        internal static List<Packet> RecordedPackets = new List<Packet>();
        internal static List<Addresses.Enums.IncomingPacketTypes> ListPacketFilters = new List<Addresses.Enums.IncomingPacketTypes>();
        private static Stopwatch stopwatchRecordingStarted = new Stopwatch();
        private static Thread listenThread = new Thread(ListenForClients), playbackThread = new Thread(Playback),
                              serverThread = new Thread(HandleServerComm), clientThread = new Thread(HandleClientComm);
        private static ASCIIEncoding ASCIIencoder = new ASCIIEncoding();
        private static Dictionary<string, bool> dictionaryMovieCheck = new Dictionary<string, bool>();
        internal static List<Characterlist.Player> characterList = new List<Characterlist.Player>();

        internal static bool Start(string IP, int Port, bool Playback, bool Record)
        {
            if (Playback && Record) { return false; }

            ServerIP = IP;
            ServerPort = Port;

            if (Record)
            {
                Recording = true;
                ClearTibiaCamBuffer();
                if (!listenThread.IsAlive)
                {
                    listenerLocal = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
                    listenThread = new Thread(new ThreadStart(ListenForClients));
                    listenThread.Start();
                }
                return true;
            }
            else if (Playback)
            {
                Playing = true;
                if (!listenThread.IsAlive)
                {
                    listenerLocal = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
                    listenThread = new Thread(new ThreadStart(ListenForClients));
                    listenThread.Start();
                }

                if (RecordedPackets.Count > MinimumRecordedPackets)
                {
                    UI.SaveTibiaCam(RecordedPackets, "AutoSave-" + Utils.RandomizeInt(1000, 9999));
                }
                ClearTibiaCamBuffer();
                return true;
            }
            return false;
        }

        private static void ListenForClients()
        {
            try
            {
                while (Recording || Playing)
                {
                    listenerLocal.Start();
                    try
                    {
                        if (!listenerLocal.Pending())
                        {
                            Thread.Sleep(500);
                        }
                        //blocks until a client has connected to the server
                        tcpClient = listenerLocal.AcceptTcpClient();
                        //create a thread to handle communication
                        if (Recording && !clientThread.IsAlive)
                        {
                            clientThread = new Thread(HandleClientComm);
                            clientThread.Start();
                            continue;
                        }
                        else if (Playing && !playbackThread.IsAlive)
                        {
                            playbackThread = new Thread(Playback);
                            playbackThread.Start();
                            continue;
                        }
                    }
                    catch { }
                    listenerLocal.Stop();
                }
            }
            catch { listenerLocal.Stop(); }
        }

        private static void HandleClientComm()
        {
            try
            {
                NetworkStream streamClient = tcpClient.GetStream();
                byte[] message = new byte[4096];
                int bytesRead; byte connection = 0;
                while (true)
                {
                    bytesRead = 0;
                    try
                    {
                        //blocks until a client sends a message
                        bytesRead = streamClient.Read(message, 0, 4096);
                    }
                    catch { break; }

                    if (bytesRead > 0)
                    {
                        if (connection != 8)
                        {
                            connection = Memory.ReadByte(Addresses.Client.Connection);
                        }
                        if (connection == 6)
                        {
                            byte charlistIndex = Memory.ReadByte(Addresses.Charlist.SelectedIndex);
                            Characterlist.Player player = characterList[charlistIndex];
                            try
                            {
                                tcpServer = new TcpClient(player.IP, player.Port);
                                serverThread = new Thread(HandleServerComm);
                                serverThread.Start();
                            }
                            catch { }
                        }
                        else if (connection > 0 && !tcpServer.Connected)
                        {
                            TcpClient tc = Utils.Network.Connect(ServerIP, ServerPort);
                            if (tc != null)
                            {
                                tcpServer = tc;
                                serverThread = new Thread(HandleServerComm);
                                serverThread.Start();
                            }
                        }
                        byte[] packetToSend = new byte[bytesRead];
                        Array.Copy(message, packetToSend, bytesRead);
                        Utils.Network.SendPacket(tcpServer, packetToSend);
                    }
                    else if (bytesRead == 0)
                    {
                        //the client has disconnected from the server
                        break;
                    }
                }
                tcpClient.Close();
                if (!listenThread.IsAlive)
                {
                    listenerLocal = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
                    listenThread = new Thread(new ThreadStart(ListenForClients));
                    listenThread.Start();
                }
            }
            catch { }
        }

        private static void HandleServerComm()
        {
            try
            {
                NetworkStream streamServer = tcpServer.GetStream();
                bool ConnectToGameServer = false;
                byte[] message = new byte[4096];
                int bytesRead, connection = 0;
                uint[] XTEAKey = new uint[4];

                while (true)
                {
                    bytesRead = 0;
                    try
                    {
                        //blocks until a server sends a message
                        bytesRead = streamServer.Read(message, 0, 4096);
                    }
                    catch { break; }

                    if (bytesRead > 0)
                    {
                        byte[] packet = new byte[bytesRead];
                        Array.Copy(message, packet, bytesRead);
                        if (connection != 8)
                        {
                            connection = Memory.ReadByte(Addresses.Client.Connection);
                        }
                        if (Recording && connection > 5)
                        {
                            if (RecordedPackets.Count == 3)
                            {
                                stopwatchRecordingStarted.Reset();
                                stopwatchRecordingStarted.Start();
                            }
                            TimeRecorded = stopwatchRecordingStarted.ElapsedMilliseconds;
                            Packet p = new Packet();
                            p.AddUInt32((uint)TimeRecorded);
                            p.AddUInt32((uint)bytesRead);
                            p.AddBytes(packet);
                            RecordedPackets.Add(p);
                        }
                        if (connection == 3)
                        {
                            ConnectToGameServer = true;
                            Memory.WriteByte(Addresses.Charlist.NumberOfCharacters, 0);
                        }
                        Utils.Network.SendPacket(tcpClient, packet);
                    }

                    else if (bytesRead == 0) { break; }
                }
                tcpServer.Close();
                if (tcpClient.Connected) { tcpClient.Close(); }
                if (ConnectToGameServer)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        if (Memory.ReadInt(Addresses.Client.DialogOpen) > 0 &&
                            Memory.ReadByte(Addresses.Charlist.NumberOfCharacters) > 0)
                        {
                            characterList = Characterlist.Players;
                            Client.Charlist.WriteIP("127.0.0.1");
                            if (!listenThread.IsAlive)
                            {
                                listenerLocal = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
                                listenThread = new Thread(new ThreadStart(ListenForClients));
                                listenThread.Start();
                            }
                            break;
                        }
                        else if (Memory.ReadInt(Addresses.Client.DialogOpen) > 0 &&
                                 Client.Misc.DialogTitle == "Enter Game")
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    ConnectToGameServer = false;
                }
                if (Client.Player.Connected && Recording)
                {
                    if (!listenThread.IsAlive)
                    {
                        listenerLocal = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
                        listenThread = new Thread(new ThreadStart(ListenForClients));
                        listenThread.Start();
                    }
                    tcpClient.Close();
                }
            }
            catch { }
        }

        internal static void Stop()
        {
            if (tcpServer != null && tcpServer.Connected)
            {
                tcpServer.Close();
            }
            if (tcpClient != null && tcpClient.Connected)
            {
                tcpClient.Close();
            }
            if (listenerLocal != null)
            {
                listenerLocal.Stop();
            }
            while (listenThread.IsAlive)
            {
                listenThread.Abort();
                Thread.Sleep(100);
            }
            while (playbackThread.IsAlive)
            {
                playbackThread.Abort();
                Thread.Sleep(200);
            }
            if (Proxy.characterList.Count > 0)
            {
                Client.Charlist.WriteIP(Proxy.characterList);
            }
            Recording = false;
            Playing = false;
        }

        internal static void ClearTibiaCamBuffer()
        {
            RecordedPackets.Clear();
            string[] fileVersion = Client.Tibia.MainModule.FileVersionInfo.FileVersion.Split('.');
            byte[] buffer = new byte[] { byte.Parse(fileVersion[0]), byte.Parse(fileVersion[1]) };
            RecordedPackets.Add(new Packet(buffer));
            Process CurrentProcess = Process.GetCurrentProcess();
            fileVersion = CurrentProcess.MainModule.FileVersionInfo.FileVersion.Split('.');
            buffer = new byte[] { byte.Parse(fileVersion[0]), byte.Parse(fileVersion[1]) };
            RecordedPackets.Add(new Packet(buffer));
            RecordedPackets.Add(new Packet(new byte[] { 0, 0, 0, 0 })); // Later to be filled in with total running length (last packet's sleep) 4 BYTES NEEDED
        }

        private static void Playback()
        {
            try
            {
                NetworkStream streamClient = tcpClient.GetStream();
                bool PlayMovie = false;
                string MovieName = "";
                byte[] message = new byte[4096];
                int bytesRead; byte connection = 0;
                while (true)
                {
                    bytesRead = 0;

                    try
                    {
                        //blocks until a client sends a message
                        bytesRead = streamClient.Read(message, 0, 4096);
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionHandler(ex);
                        //a socket error has occured
                        break;
                    }
                    //message has successfully been received
                    if (bytesRead > 0)
                    {
                        byte[] incomingPacket = new byte[bytesRead];
                        Array.Copy(message, incomingPacket, bytesRead);
                        if (connection != 8) { connection = Memory.ReadByte(Addresses.Client.Connection); }
                        if (connection == 3)
                        {
                            string[] TibiaCams = GetTibiaCams();
                            Array.Sort(TibiaCams);
                            if (!Proxy.doAutoPlayback)
                            {
                                Utils.Network.SendPacket(tcpClient, ConstructTibiaCamListPacket(TibiaCams));
                            }
                            else
                            {
                                Utils.Network.SendPacket(tcpClient, ConstructTibiaCamListPacket(new string[] { Proxy.AutoPlayBackName }));
                            }
                            break;
                        }
                        else if (connection == 6)
                        {
                            if (!Proxy.doAutoPlayback)
                            {
                                MovieName = Characterlist.Players[Memory.ReadByte(Addresses.Charlist.SelectedIndex)].Name;
                            }
                            else
                            {
                                MovieName = Proxy.AutoPlayBackName;
                            }
                            if (!File.Exists(MovieName))
                            {
                                System.Windows.Forms.MessageBox.Show("File does not exist");
                                break;
                            }
                            MovieFileName = MovieName.Substring(MovieName.LastIndexOf('\\') + 1);
                            PlayMovie = true;
                            break;
                        }
                    }
                    else if (bytesRead == 0)
                    {
                        //the client has disconnected from the server
                        break;
                    }
                }
                if (PlayMovie)
                {
                    if (!UI.gkh.HookedKeys.Contains(Keys.Up))
                    {
                        UI.gkh.HookedKeys.Add(Keys.Up);
                    }
                    if (!UI.gkh.HookedKeys.Contains(Keys.Down))
                    {
                        UI.gkh.HookedKeys.Add(Keys.Down);
                    }
                    if (!UI.gkh.HookedKeys.Contains(Keys.Left))
                    {
                        UI.gkh.HookedKeys.Add(Keys.Left);
                    }
                    if (!UI.gkh.HookedKeys.Contains(Keys.Right))
                    {
                        UI.gkh.HookedKeys.Add(Keys.Right);
                    }
                    if (!UI.gkh.HookedKeys.Contains(Keys.Back))
                    {
                        UI.gkh.HookedKeys.Add(Keys.Back);
                    }
                    double threadSleep = 0, oldThreadSleep = 0;
                    currentPacket = 3;
                    TimePlayed = 0;
                    string _motd = "";
                    if (Utils.isCamOld(MovieName)) // true for version 1.2 and lower
                    {
                        List<string> packets = Utils.DecompressCam(MovieName);
                        if (doAutoPlayback)
                        {
                            MovieName = AutoPlayBackName.Substring(AutoPlayBackName.LastIndexOf('\\') + 1);
                        }
                        MotD = "Recorded with Tibianic Tools v";
                        _motd = MotD += packets[1].Replace("TibiaCamVersion=", ""); // TibiaCamVersion
                        TimeTotalPlayback = double.Parse(packets[2].Replace("TotalRunningTime=", "")); // TotalRunningTime (ms)
                        while (currentPacket < packets.Count)
                        {
                            try
                            {
                                if (Proxy.PlaybackSpeed < 0 || Proxy.PlaybackSpeed > 200)
                                {
                                    Proxy.PlaybackSpeed = 0;
                                }
                                if (!Playing)
                                {
                                    break;
                                }
                                else if (PlaybackSpeed > 0)
                                {
                                    if (Proxy.Rewind)
                                    {
                                        currentPacket = 3;
                                        TimePlayed = 0;
                                        threadSleep = 0;
                                        oldThreadSleep = 0;
                                        Proxy.Rewind = false;
                                    }
                                    string Packet = packets[currentPacket];
                                    int newThreadSleep = int.Parse(Packet.Substring(0, Packet.IndexOf(':')));
                                    TimePlayed = newThreadSleep;
                                    threadSleep = newThreadSleep - oldThreadSleep;
                                    threadSleep /= PlaybackSpeed;
                                    Packet = Packet.Remove(0, Packet.IndexOf(':') + 1);
                                    string[] strPacket = Packet.Split(' ');
                                    byte[] packet = new byte[strPacket.Length];
                                    for (int j = 0; j < strPacket.Length; j++)
                                    {
                                        packet[j] = byte.Parse(strPacket[j], System.Globalization.NumberStyles.AllowHexSpecifier);
                                    }
                                    if (UI.FastForward && TimePlayed <= UI.timespanFastForward.TotalMilliseconds) { }
                                    else
                                    {
                                        UI.FastForward = false;
                                        Thread.Sleep((int)threadSleep);
                                    }
                                    Utils.Network.SendPacket(tcpClient, packet);
                                    if (currentPacket == 10)
                                    {
                                        Client.Misc.WriteStatusBar(_motd, 10);
                                    }
                                    oldThreadSleep = newThreadSleep;
                                    currentPacket++;
                                }
                                else if (PlaybackSpeed <= 0)
                                {
                                    if (PlaybackSpeed < 0)
                                    {
                                        PlaybackSpeed = 0;
                                    }
                                    Thread.Sleep(400);
                                }
                            }
                            catch
                            {
                                //throw new Exception(ex.Message);
                                break;
                            }
                        }
                        Thread.Sleep(3000);
                        if (Proxy.doAutoPlayback) { Client.Tibia.Kill(); }
                    }
                    else
                    {
                        List<byte[]> packets = Utils.DecompressCamToBytes(MovieName);
                        if (doAutoPlayback)
                        {
                            MovieName = AutoPlayBackName.Substring(AutoPlayBackName.LastIndexOf('\\') + 1);
                        }
                        MotD = "Recorded with Tibianic Tools v";
                        _motd = MotD += packets[1][0] + "." + packets[1][1]; // TibiaCamVersion
                        TimeTotalPlayback = BitConverter.ToUInt32(packets[2], 0); // TotalRunningTime (ms)
                        if (Client.TibiaVersion >= 770)
                        {
                            XTEAKey = Utils.GetXTEAKey();
                        }
                        while (currentPacket < packets.Count)
                        {
                            try
                            {
                                if (Proxy.PlaybackSpeed <= 0 || Proxy.PlaybackSpeed > 200)
                                {
                                    Proxy.PlaybackSpeed = 0;
                                }
                                if (!Playing)
                                {
                                    break;
                                }
                                else if (PlaybackSpeed > 0)
                                {
                                    if (Proxy.Rewind)
                                    {
                                        currentPacket = 3;
                                        TimePlayed = 0;
                                        threadSleep = 0;
                                        oldThreadSleep = 0;
                                        Proxy.Rewind = false;
                                    }
                                    byte[] Packet = packets[currentPacket];
                                    byte[] sleep = new byte[4];
                                    Array.Copy(Packet, sleep, 4);
                                    int newThreadSleep = BitConverter.ToInt32(sleep, 0);
                                    TimePlayed = newThreadSleep;
                                    threadSleep = newThreadSleep - oldThreadSleep;
                                    threadSleep /= PlaybackSpeed;
                                    byte[] RealPacket = new byte[Packet.Length - 8];
                                    Array.Copy(Packet, 8, RealPacket, 0, Packet.Length - 8);
                                    if (UI.FastForward && TimePlayed <= UI.timespanFastForward.TotalMilliseconds) { }
                                    else if (threadSleep > 0)
                                    {
                                        UI.FastForward = false;
                                        Thread.Sleep((int)threadSleep);
                                    }
                                    Utils.Network.SendPacket(tcpClient, RealPacket);
                                    if (currentPacket == 10)
                                    {
                                        Client.Misc.WriteStatusBar(_motd, 10);
                                    }
                                    oldThreadSleep = newThreadSleep;
                                    currentPacket++;
                                }
                                else if (PlaybackSpeed <= 0.0)
                                {
                                    PlaybackSpeed = 0;
                                    Thread.Sleep(400);
                                }
                            }
                            catch { break; }
                        }
                        Thread.Sleep(3000);
                        if (Proxy.doAutoPlayback) { Client.Tibia.Kill(); }
                    }
                    WinApi.SetWindowText(Client.Tibia.MainWindowHandle, "Tibia");
                }
                tcpClient.Close();
                streamClient.Close();
                if (!listenThread.IsAlive)
                {
                    listenerLocal = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
                    listenThread = new Thread(new ThreadStart(ListenForClients));
                    listenThread.Start();
                }
                PlayMovie = false;
                TimePlayed = 0;
                TimeTotalPlayback = 0;
                MovieFileName = "";
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException) { }
                else { Utils.ExceptionHandler(ex); } // is there a prettier way to do this? :(
                WinApi.SetWindowText(Client.Tibia.MainWindowHandle, "Tibia");
            }
        }

        private static string[] GetTibiaCams()
        {
            return Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\", "*.kcam");
        }

        private static byte[] ConstructTibiaCamListPacket(string[] TibiaCams)
        {
            string[] Cams = TibiaCams;
            dictionaryMovieCheck.Clear();
            byte[] TibiaCamListPacket = new byte[4096];
            int pos = 2;
            TibiaCamListPacket[pos] = 0x14; pos++;
            MotD = "Recorded with Tibianic Tools";
            byte[] motd = ASCIIencoder.GetBytes(MotD);
            byte[] motdLength = BitConverter.GetBytes(motd.Length);
            TibiaCamListPacket[pos] = motdLength[0]; pos++;
            TibiaCamListPacket[pos] = motdLength[1]; pos++;
            Array.Copy(motd, 0, TibiaCamListPacket, pos, motd.Length); pos += motd.Length;
            TibiaCamListPacket[pos] = 0x64; pos++;
            int camlengthpos = pos;
            TibiaCamListPacket[pos] = (byte)Cams.Length; pos++;
            byte j = 0;
            for (int i = 0; i < Cams.Length; i++)
            {
                string TibiaVersion = "", CamVersion = "";
                if (Utils.isCamOld(Cams[i]))
                {
                    List<string> stringpackets = Utils.DecompressCam(Cams[i]);
                    if (stringpackets.Count == 0) { continue; }
                    TibiaVersion = stringpackets[0].Replace("TibiaVersion=", "");
                    CamVersion = stringpackets[1].Replace("TibiaCamVersion=", "");
                    dictionaryMovieCheck.Add(Cams[i], true);
                }
                else
                {
                    List<byte[]> packets = Utils.DecompressCamToBytes(Cams[i]);
                    if (packets.Count == 0) { continue; }
                    TibiaVersion = packets[0][0] + "." + packets[0][1];
                    CamVersion = packets[1][0] + "." + packets[1][1];
                    dictionaryMovieCheck.Add(Cams[i], false);
                }
                Cams[i] = Cams[i].Substring(Cams[i].LastIndexOf('\\') + 1);
                byte[] CamLength = BitConverter.GetBytes(Cams[i].Length);
                TibiaCamListPacket[pos] = CamLength[0]; pos++;
                TibiaCamListPacket[pos] = CamLength[1]; pos++;
                byte[] FileName = ASCIIencoder.GetBytes(Cams[i]);
                Array.Copy(FileName, 0, TibiaCamListPacket, pos, FileName.Length); pos += FileName.Length;
                byte[] ServerName = ASCIIencoder.GetBytes(TibiaVersion);
                TibiaCamListPacket[pos] = (byte)ServerName.Length; pos++;
                TibiaCamListPacket[pos] = 0x00; pos++;
                Array.Copy(ServerName, 0, TibiaCamListPacket, pos, ServerName.Length); pos += ServerName.Length;
                TibiaCamListPacket[pos] = 127; pos++;
                TibiaCamListPacket[pos] = 0; pos++;
                TibiaCamListPacket[pos] = 0; pos++;
                TibiaCamListPacket[pos] = 1; pos++;
                byte[] Port = BitConverter.GetBytes(ServerPort);
                TibiaCamListPacket[pos] = Port[0]; pos++;
                TibiaCamListPacket[pos] = Port[1]; pos++;
                j++;
            }
            TibiaCamListPacket[camlengthpos] = j;

            byte[] PremiumDays = BitConverter.GetBytes((ushort)Cams.Length);
            TibiaCamListPacket[pos] = PremiumDays[0]; pos++;
            TibiaCamListPacket[pos] = PremiumDays[1]; pos++;
            byte[] PacketLength = BitConverter.GetBytes(pos - 2);
            TibiaCamListPacket[0] = PacketLength[0];
            TibiaCamListPacket[1] = PacketLength[1];

            byte[] packetToReturn = new byte[pos];
            Array.Copy(TibiaCamListPacket, packetToReturn, pos);

            return packetToReturn;
        }

        private static byte[] RemovePacketFromPackage(byte[] Package, Addresses.Enums.IncomingPacketTypes PacketType)
        {
            try
            {
                int packetLength = 0, packagePos = 0;
                List<byte> newPacket = new List<byte>();

                while (packagePos < Package.Length)
                {
                    packetLength = BitConverter.ToInt16(Package, packagePos);
                    if (Package[packagePos + 2] != (byte)PacketType)
                    {
                        for (int i = 0; i < packetLength + 2; i++)
                        {
                            newPacket.Add(Package[packagePos + i]);
                        }
                    }
                    packagePos += packetLength + 2;
                }

                return newPacket.ToArray();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}
