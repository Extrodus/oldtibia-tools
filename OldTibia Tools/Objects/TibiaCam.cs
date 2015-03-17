using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TibianicTools.Objects
{
    class TibiaCam
    {
        /// <summary>
        /// Constructor for playback.
        /// </summary>
        internal TibiaCam()
        {
            this.Mode = TibiaCamMode.Playback;
            this.PlaybackSpeed = 1.0;
        }
        /// <summary>
        /// Constructor for recording.
        /// </summary>
        /// <param name="loginServer">The login server to connect to.</param>
        internal TibiaCam(LoginServer loginServer)
        {
            this.Mode = TibiaCamMode.Record;
            this.ServerInfo = loginServer;
            this.RecordingStopwatch = new System.Diagnostics.Stopwatch();
            this.CachedCharacterList = new List<CharacterList.Player>();
        }

        #region get-set properties
        internal uint[] XteaKey { get; set; }
        internal Recording CurrentRecording { get; set; }
        internal LoginServer ServerInfo { get; set; }
        internal TibiaCamMode Mode { get; set; }
        internal ushort ListenerPort { get; set; }
        internal string PreferredFileName { get; set; }
        internal double PlaybackSpeed { get; set; }
        internal bool AutoRecord { get; set; }
        internal bool AutoPlayback { get; set; }
        internal bool DoKillAfterPlayback { get; set; }
        private TcpClient TcpClientLocal { get; set; }
        private TcpClient TcpClientServer { get; set; }
        private bool isRunning { get; set; }
        internal bool IsRunning { get { return this.isRunning; } }
        /// <summary>
        /// Used to reset character list when closing.
        /// </summary>
        private List<CharacterList.Player> CachedCharacterList { get; set; }
        private System.Diagnostics.Stopwatch RecordingStopwatch { get; set; }
        private int CurrentPacket { get; set; }
        private TimeSpan FastForwardTo { get; set; }
        private bool DoFastForward { get; set; }
        /// <summary>
        /// Used to reset the login servers when closing.
        /// </summary>
        private List<LoginServer> ServerInfoOld = new List<LoginServer>();
        private Thread threadPlaybackSend { get; set; }
        private Thread threadPlaybackSendMouse { get; set; }
        #endregion

        #region methods
        internal void Start()
        {
            this.isRunning = true;
            switch (this.Mode)
            {
                case TibiaCamMode.Playback:
                    break;
                case TibiaCamMode.Record:
                    this.CurrentRecording = new Recording(Settings.Tibiacam.Recorder.doRecordMouse);
                    break;
            }
            this.ServerInfoOld = Client.Misc.GetLoginServers();
            Thread t = new Thread(new ThreadStart(HandleTcpRequests));
            t.Start();
            if (this.AutoPlayback)
            {
                Thread tLogin = new Thread(new ThreadStart(delegate() { Client.Misc.Login(0, string.Empty, "TibiaCam"); }));
                tLogin.Start();
            }
        }

        internal void Stop()
        {
            this.isRunning = false;
            if (this.ServerInfoOld.Count > 0) Client.Misc.SetLoginServers(this.ServerInfoOld);
            if (this.CachedCharacterList != null && this.CachedCharacterList.Count > 0) Client.Charlist.WriteIP(this.CachedCharacterList);
            switch (this.Mode)
            {
                case TibiaCamMode.Playback:
                    if (this.TcpClientLocal != null) this.TcpClientLocal.Close();
                    if (this.threadPlaybackSend != null && this.threadPlaybackSend.IsAlive) this.threadPlaybackSend.Abort();
                    if (this.DoKillAfterPlayback && !Client.Tibia.HasExited) Client.Tibia.Kill();
                    break;
                case TibiaCamMode.Record:
                    if (this.CurrentRecording != null && this.CurrentRecording.ContainsData()) this.CurrentRecording.Save(this.PreferredFileName);
                    this.RecordingStopwatch.Stop();
                    break;
            }
        }

        /// <summary>
        /// Handles incoming TCP connection requests and sets the login servers. Should be run on its own thread.
        /// </summary>
        private void HandleTcpRequests()
        {
            TcpListener listener = null;
            for (ListenerPort = 7000; ListenerPort < 7500; ListenerPort++)
            {
                try
                {
                    listener = new TcpListener(IPAddress.Loopback, ListenerPort);
                    listener.Start();
                    break;
                }
                catch { continue; }
            }
            if (listener == null || this.ListenerPort == 7500) return;
            Client.Misc.SetLoginServers(new List<LoginServer>() { new LoginServer(IPAddress.Loopback.ToString(), ListenerPort) });
            while (isRunning)
            {
                while (!listener.Pending()) Thread.Sleep(100);
                TcpClientLocal = listener.AcceptTcpClient();
                Thread t;
                switch (this.Mode)
                {
                    case TibiaCamMode.Playback:
                        t = new Thread(new ThreadStart(PlaybackRecv));
                        t.Start();
                        break;
                    case TibiaCamMode.Record:
                        t = new Thread(new ThreadStart(HandleTcpLocal));
                        t.Start();
                        break;
                }
            }
            listener.Stop();
        }

        #region Recording
        /// <summary>
        /// Handles the local Tibia client connection. Should be run on its own thread.
        /// </summary>
        private void HandleTcpLocal()
        {
            try
            {
                if (TcpClientLocal == null) return;
                NetworkStream nstream = TcpClientLocal.GetStream();
                int connection = 0;
                bool obtainedXteaKey = false;

                while (true)
                {
                    Objects.Packet p = Objects.Packet.GetNextPacket(nstream);
                    if (p.Length == 0) break;

                    if (!obtainedXteaKey)
                    {
                        if (Client.TibiaVersion >= 770)
                        {
                            this.XteaKey = new uint[4];
                            for (int i = 0; i < 4; i++) this.XteaKey[i] = Memory.ReadUInt(Addresses.Client.XTEAKey + i * 4);
                        }
                        obtainedXteaKey = true;
                    }

                    if (connection != 8)
                    {
                        connection = Memory.ReadByte(Addresses.Client.Connection);
                        if (connection == 6) // connecting to gameserver
                        {
                            try
                            {
                                byte index = Memory.ReadByte(Addresses.Charlist.SelectedIndex);
                                if (index < 0 || index >= this.CachedCharacterList.Count) break; // invalid index
                                CharacterList.Player player = this.CachedCharacterList[index];
                                TcpClientServer = new TcpClient(player.IP, player.Port);
                                Thread t = new Thread(new ThreadStart(HandleTcpServer));
                                t.Start();
                                Thread.Sleep(20); // give thread time to start
                            }
                            catch { break; }
                        }
                        else if (connection > 0 && connection < 6) // connecting to login server, not sure exactly what value it should be
                        {
                            try
                            {
                                this.TcpClientServer = new TcpClient(this.ServerInfo.IP, this.ServerInfo.Port);
                                Thread t = new Thread(new ThreadStart(HandleTcpServer));
                                t.Start();
                                Thread.Sleep(20); // give thread time to start
                            }
                            catch { break; } // couldn't connect to server
                        }
                    }
                    p.Send(this.TcpClientServer);

                    try
                    {
                        if (Client.TibiaVersion >= 770) p = p.XteaDecrypt(this.XteaKey, 2);
                        ushort len = p.GetUInt16();
                        byte packetType = p.GetByte();
                        if (!Settings.Tibiacam.Recorder.Filters.OutgoingPrivateMessages && packetType == 0x96 && Client.TibiaVersion == 740)
                        {
                            if (p.GetByte() != 0x04) continue;
                            Objects.Packet pm = new Objects.Packet();
                            pm.AddByte((byte)Addresses.Enums.IncomingPacketTypes.CreatureSpeech);
                            string recipient = p.GetString();
                            pm.AddString(recipient);
                            pm.AddByte(0x04);
                            string msg = p.GetString();
                            pm.AddString("» " + msg);
                            pm.AddLength();
                            //Objects.Packet temp = new Objects.Packet();
                            //temp.AddUInt32((uint)this.RecordingStopwatch.ElapsedMilliseconds);
                            //temp.AddUInt32((uint)pm.Length);
                            //temp.AddBytes(pm.ToBytes());
                            CurrentRecording.Packets.Add(new Recording.Packet((uint)this.RecordingStopwatch.ElapsedMilliseconds, pm.ToBytes()));
                        }
                    }
                    catch { }
                }
                if (this.TcpClientLocal != null) this.TcpClientLocal.Close();
                if (this.TcpClientServer != null) this.TcpClientServer.Close();
            }
            catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace); }
        }

        private void HandleTcpServer()
        {
            try
            {
                if (TcpClientServer == null) return;
                NetworkStream nstream = TcpClientServer.GetStream();
                int connection = 0;
                bool changeCharacterList = false, sentAnimatedTextPacket = false;
                while (true)
                {
                    Objects.Packet p = Objects.Packet.GetNextPacket(nstream);
                    if (p.Length == 0) break;

                    if (connection != 8) connection = Memory.ReadByte(Addresses.Client.Connection);
                    if (connection == 3)
                    {
                        changeCharacterList = true;
                        Memory.WriteByte(Addresses.Charlist.NumberOfCharacters, 0);
                    }
                    else if (connection > 5 && this.IsRunning)
                    {
                        if (connection == 8 && !sentAnimatedTextPacket)
                        {
                            Objects.Packet animPacket = new Objects.Packet();
                            animPacket.AddByte((byte)Addresses.Enums.IncomingPacketTypes.AnimatedText);
                            animPacket.AddUInt16(Client.Player.X);
                            animPacket.AddUInt16(Client.Player.Y);
                            animPacket.AddByte(Client.Player.Z);
                            animPacket.AddByte(181);
                            animPacket.AddString("Recording!");
                            animPacket.AddLength();
                            if (Client.TibiaVersion >= 770) { animPacket.XteaEncrypt(this.XteaKey); animPacket.AddLength(); }
                            animPacket.Send(TcpClientLocal);
                            sentAnimatedTextPacket = true;
                        }
                        if (!this.RecordingStopwatch.IsRunning) this.RecordingStopwatch.Start();
                        if (this.CurrentRecording.MouseRecording != null && !this.CurrentRecording.MouseRecording.IsRecording)
                        {
                            this.CurrentRecording.MouseRecording.StartRecording();
                        }
                        Objects.Packet temp = new Objects.Packet();
                        //temp.AddUInt32((uint)this.RecordingStopwatch.ElapsedMilliseconds);
                        if (Client.TibiaVersion >= 770)
                        {
                            Objects.Packet decryptedPacket = p.XteaDecrypt(this.XteaKey, 2);
                            //temp.AddUInt32((uint)decryptedPacket.Length);
                            temp.AddBytes(decryptedPacket.ToBytes());
                        }
                        else
                        {
                            //temp.AddUInt32((uint)p.Length);
                            temp.AddBytes(p.ToBytes());
                        }
                        this.CurrentRecording.Packets.Add(new Recording.Packet((uint)this.RecordingStopwatch.ElapsedMilliseconds, temp.ToBytes()));
                        this.CurrentRecording.TimePassed = (uint)this.RecordingStopwatch.ElapsedMilliseconds;
                    }
                    p.Send(this.TcpClientLocal);
                }

                if (changeCharacterList)
                {
                    changeCharacterList = false;
                    for (int i = 0; i < 100; i++)
                    {
                        if (Memory.ReadInt(Addresses.Client.DialogOpen) > 0 &&
                            Memory.ReadByte(Addresses.Charlist.NumberOfCharacters) > 0)
                        {
                            this.CachedCharacterList = CharacterList.GetPlayers();
                            Client.Charlist.WriteIP("127.0.0.1", this.ListenerPort);
                            break;
                        }
                        else if (Memory.ReadInt(Addresses.Client.DialogOpen) > 0 &&
                                 Client.Misc.DialogTitle == "Enter Game")
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }

                if (this.TcpClientLocal != null) this.TcpClientLocal.Close();
                if (this.TcpClientServer != null) this.TcpClientServer.Close();
            }
            catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace); }
        }
        #endregion

        #region Playback
        internal void FastForward(TimeSpan ts)
        {
            if (this.CurrentRecording == null) return;
            if (this.threadPlaybackSend == null || !this.threadPlaybackSend.IsAlive) return;
            this.FastForwardTo = ts;
            this.DoFastForward = true;
            if (this.threadPlaybackSend.ThreadState == ThreadState.WaitSleepJoin) this.threadPlaybackSend.Interrupt();
        }

        private void PlaybackSend()
        {
            try
            {
                Recording.Packet p;
                this.CurrentPacket = 0;
                this.DoFastForward = false;
                uint timeOld = 0, timeNew = 0;
                bool hasShownInfo = false;
                this.CurrentRecording.TimePassed = 0;
                if (this.CurrentRecording.MouseRecording != null)
                {
                    while (this.threadPlaybackSendMouse != null && this.threadPlaybackSendMouse.IsAlive) { this.threadPlaybackSendMouse.Abort(); Thread.Sleep(100); }
                    this.threadPlaybackSendMouse = new Thread(new ThreadStart(this.PlaybackSendMouse));
                    threadPlaybackSendMouse.Start();
                }

                try
                {
                    while (this.CurrentPacket < this.CurrentRecording.Packets.Count)
                    {
                        if (!this.TcpClientLocal.Connected) return;

                        if (this.DoFastForward)
                        {
                            if (TimeSpan.FromMilliseconds(this.CurrentRecording.TimePassed) > this.FastForwardTo) { this.CurrentPacket = 0; this.CurrentRecording.TimePassed = 0; }
                            while (this.CurrentPacket < this.CurrentRecording.Packets.Count &&
                                   this.CurrentRecording.TimePassed < this.FastForwardTo.TotalMilliseconds)
                            {
                                p = this.CurrentRecording.Packets[this.CurrentPacket].Clone();
                                this.CurrentRecording.TimePassed = p.Time;
                                //if (this.CurrentRecording.Recorder == Enums.Recorder.TibianicTools) p.GetPosition += 4;
                                //else if (this.CurrentRecording.Recorder == Enums.Recorder.TibiaMovie) p.GetPosition += 2;
                                if (Client.TibiaVersion >= 770) p = p.XteaEncrypt(this.XteaKey, 0);
                                p.Send(this.TcpClientLocal);
                                this.CurrentPacket++;
                            }
                            this.DoFastForward = false;
                            hasShownInfo = false;
                        }
                        else if (this.PlaybackSpeed <= 0) { Thread.Sleep(200); continue; }

                        if (/*Client.TibiaVersion == 740 && */!hasShownInfo && this.CurrentPacket > 3 && Client.Player.Connected)
                        {
                            Thread.Sleep(100);
                            Objects.Packet packet = new Objects.Packet();
                            packet.AddByte(0xb4); // text message
                            packet.AddByte(16); // orange color
                            packet.AddString("Welcome to a Tibianic Tools recording!\nKeyboard hotkeys:\nArrow keys - change playback speed\nBackspace - rewind 1 minute");
                            packet.AddByte(0xb4);
                            packet.AddByte(16);
                            packet.AddString("Text commands:\ninfo - show recording information\ngoto hh:mm:ss - rewinds/fast forwards to given time\npause - pauses playback\nresume - resumes playback");
                            packet.AddLength();
                            if (Client.TibiaVersion >= 770) { packet.XteaEncrypt(this.XteaKey, packet.GetPosition); packet.AddLength(); }
                            packet.Send(this.TcpClientLocal);

                            hasShownInfo = true;
                            Thread.Sleep(20);
                        }

                        p = this.CurrentRecording.Packets[this.CurrentPacket].Clone();
                        this.CurrentRecording.TimePassed = p.Time;
                        timeNew = this.CurrentRecording.TimePassed;
                        int sleep = (int)((timeNew - timeOld) / this.PlaybackSpeed);
                        //if (this.CurrentRecording.Recorder == Enums.Recorder.TibianicTools) p.GetPosition += 4; // length, not needed
                        //else if (this.CurrentRecording.Recorder == Enums.Recorder.TibiaMovie) p.GetPosition += 2; // -||-
                        // try-catch because Thread.Interrupt() will throw an exception
                        try { if (sleep > 0) Thread.Sleep(sleep); }
                        catch { }
                        if (!this.TcpClientLocal.Connected) return;
                        if (Client.TibiaVersion >= 770)
                        {
                            p = p.XteaEncrypt(this.XteaKey, 0);
                        }
                        p.Send(this.TcpClientLocal);
                        if (this.CurrentRecording.MouseRecording != null)
                        {
                            MouseRecording mouseRec = this.CurrentRecording.MouseRecording;
                            //int currentTime = mouseRec.Seek(this.CurrentRecording.TimePassed);
                            int mouseTime = mouseRec.CurrentIndex * mouseRec.Interval;
                            int timeMarginMax = mouseTime + 200, timeMarginMin = mouseTime - 200;
                            if (timeNew >= timeMarginMax || timeNew <= timeMarginMin)
                            {
                                this.CurrentRecording.MouseRecording.CurrentIndex = this.CurrentRecording.MouseRecording.Seek(timeNew);
                            }
                        }
                        timeOld = timeNew;
                        this.CurrentPacket++;
                    }
                }
                catch { }
                Thread.Sleep(3000);
                this.PlaybackSpeed = 1;
                WinApi.SetWindowText(Client.Tibia.MainWindowHandle, "Tibia");
                if (TcpClientLocal != null) TcpClientLocal.Close();
                if (this.DoKillAfterPlayback && !Client.Tibia.HasExited) Client.Tibia.Kill();
                else this.AutoPlayback = false;
            }
            catch (Exception ex) { /*System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);*/ }
        }
        private void PlaybackSendMouse()
        {
            try
            {
                Recording rec = this.CurrentRecording;
                if (rec == null) return;
                MouseRecording mouseRec = null;
                if (this.CurrentRecording.MouseRecording != null) mouseRec = this.CurrentRecording.MouseRecording;
                else return;

                mouseRec.CurrentIndex = 0;
                while (this.IsRunning && this.TcpClientLocal.Connected)
                {
                    if (this.threadPlaybackSend == null || !this.threadPlaybackSend.IsAlive) return;
                    while (this.PlaybackSpeed <= 0) { Thread.Sleep(200); continue; }
                    //int currentTime = mouseRec.Seek(this.CurrentRecording.TimePassed);
                    //int mouseTime = i * mouseRec.Interval;

                    Objects.Packet p = mouseRec.Packets[mouseRec.CurrentIndex];
                    p.GetPosition = 0;
                    MouseRecording.DataType dt = (MouseRecording.DataType)p.GetByte();
                    switch (dt)
                    {
                        case MouseRecording.DataType.ClientWindowChange:
                            break;
                        case MouseRecording.DataType.GameWindowChange:
                            break;
                        case MouseRecording.DataType.MouseXY:
                            int percentX = p.GetByte(), percentY = p.GetByte();
                            if (percentX == 0 && percentY == 0) break;
                            if (WinApi.GetForegroundWindow() != Client.Tibia.MainWindowHandle) break;
                            System.Drawing.Point point = mouseRec.CalculateScreenMouseXY(new System.Drawing.Point(percentX, percentY));
                            System.Windows.Forms.Cursor.Position = point;
                            break;
                    }
                    p.GetPosition = 0;
                    mouseRec.CurrentIndex++;
                    Thread.Sleep((int)(mouseRec.Interval / this.PlaybackSpeed));
                }
            }
            catch { }
        }

        private void PlaybackRecv()
        {
            try
            {
                NetworkStream nstream = TcpClientLocal.GetStream();
                int connection = 0;
                bool doLoop = true, obtainedXteaKey = false;

                while (doLoop)
                {
                    Objects.Packet p = Objects.Packet.GetNextPacket(nstream);
                    if (p.Length == 0) break;

                    if (!obtainedXteaKey)
                    {
                        if (Client.TibiaVersion >= 770)
                        {
                            this.XteaKey = new uint[4];
                            for (int i = 0; i < 4; i++) this.XteaKey[i] = Memory.ReadUInt(Addresses.Client.XTEAKey + i * 4);
                        }
                        obtainedXteaKey = true;
                    }

                    connection = Memory.ReadByte(Addresses.Client.Connection);
                    switch ((Enums.Connection)connection)
                    {
                        case Enums.Connection.WaitingForCharacterList:
                            if (this.AutoPlayback)
                            {
                                p = new Objects.Packet();
                                p.AddByte(0x64);
                                p.AddByte(1);
                                p.AddString("TibiaCam");
                                p.AddString(CurrentRecording.TibiaVersion);
                                p.AddBytes(IPAddress.Loopback.GetAddressBytes());
                                p.AddUInt16(this.ListenerPort);
                                p.AddUInt16(0); // premium days
                                p.AddLength();
                                if (Client.TibiaVersion >= 770) { p.XteaEncrypt(this.XteaKey); p.AddLength(); }
                                p.Send(this.TcpClientLocal);
                                doLoop = false;
                                break;
                            }

                            List<string> files = new List<string>();
                            foreach (string kcam in System.IO.Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\", "*.kcam"))
                            {
                                files.Add(kcam);
                            }
                            foreach (string iryontcam in System.IO.Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\", "*.cam"))
                            {
                                files.Add(iryontcam);
                            }
                            if (Settings.Tibiacam.Playback.SupportTibiaMovies)
                            {
                                foreach (string tmv in System.IO.Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\", "*.tmv"))
                                {
                                    files.Add(tmv);
                                }
                            }
                            files.Sort();
                            string warning = string.Empty;
                            int count = files.Count;
                            if (files.Count > 255) { warning = "\n\nWarning! There are " + files.Count + " recordings.\nOnly the first 255 recordings are listed."; count = 255; }
                            p = new Objects.Packet();
                            p.AddByte(0x14); // motd type
                            // motd id is 0-255, which is followed by a newline (\n)
                            p.AddString((byte)new Random().Next(255) + "\nThank you for using Tibianic Tools.\nhttp://code.google.com/p/tibianic-tools/\nhttp://tibianic.org/" + warning);
                            p.AddByte(0x64); // character list type
                            List<Recording> recordings = new List<Recording>();
                            for (int i = 0; i < count; i++)
                            {
                                Recording r = new Recording(files[i], Settings.Tibiacam.Playback.ReadMetaData, false, false);
                                if (!r.isCorrupt) recordings.Add(r);
                            }
                            p.AddByte((byte)count); // amount of characters
                            foreach (Recording r in recordings)
                            {
                                p.AddString(r.FileNameShort); // character name
                                p.AddString(r.TibiaVersion); // server name
                                p.AddBytes(IPAddress.Loopback.GetAddressBytes()); // server ipv4 (in this case: 127.0.0.1)
                                p.AddUInt16(this.ListenerPort); // server port
                            }
                            p.AddUInt16((ushort)count); // premium days
                            p.AddLength();
                            if (Client.TibiaVersion >= 770) { p.XteaEncrypt(this.XteaKey); p.AddLength(); }
                            p.Send(TcpClientLocal);
                            doLoop = false;
                            break;
                        case Enums.Connection.ConnectingGameServer:
                            if (this.AutoPlayback)
                            {
                                this.threadPlaybackSend = new Thread(new ThreadStart(PlaybackSend));
                                this.threadPlaybackSend.Start();
                                break;
                            }
                            CharacterList.Player player = CharacterList.GetPlayers()[Memory.ReadByte(Addresses.Charlist.SelectedIndex)];
                            if (!System.IO.File.Exists(player.Name)) { System.Windows.Forms.MessageBox.Show(player.Name + "\ndoes not exist", "Error"); doLoop = false; break; }
                            Recording rec = new Recording(player.Name, true, true, Settings.Tibiacam.Playback.doPlayMouse);
                            if (rec.isCorrupt || !rec.ContainsData()) { System.Windows.Forms.MessageBox.Show(player.Name + "\nappears to be corrupt or it doesn't contain any data", "Error"); doLoop = false; break; }
                            this.CurrentRecording = rec;
                            while (this.threadPlaybackSend != null && this.threadPlaybackSend.IsAlive) { this.threadPlaybackSend.Abort(); Thread.Sleep(100); }
                            while (this.threadPlaybackSendMouse != null && this.threadPlaybackSendMouse.IsAlive) { this.threadPlaybackSendMouse.Abort(); Thread.Sleep(100); }
                            this.threadPlaybackSend = new Thread(new ThreadStart(PlaybackSend));
                            this.threadPlaybackSend.Start();
                            break;
                        case Enums.Connection.Online:
                            ushort len = p.GetUInt16();
                            if (Client.TibiaVersion >= 770)
                            {
                                p = p.XteaDecrypt(this.XteaKey, 2);
                                if (p == null) break;
                                p.GetPosition = 2;
                            }
                            byte type = p.GetByte();
                            switch (type)
                            {
                                case 0x14: // logout
                                    while (this.threadPlaybackSend != null && this.threadPlaybackSend.IsAlive) { this.threadPlaybackSend.Abort(); Thread.Sleep(100); }
                                    this.TcpClientLocal.Close();
                                    return;
                                case 0x96: // player speech
                                    byte speechType = p.GetByte();
                                    if (speechType < 1 || speechType > 3) break;
                                    string msg = p.GetString().ToLower();
                                    string[] msgSplit = msg.Split(' ');
                                    p = new Objects.Packet();
                                    switch (msgSplit[0])
                                    {
                                        case "info":
                                            p.AddByte(0xb4);
                                            p.AddByte(22);
                                            p.AddString("Tibia version: " + this.CurrentRecording.TibiaVersion + "\nRecorder version: " + this.CurrentRecording.RecorderVersion + "\n# of packets: " + this.CurrentRecording.Packets.Count);
                                            p.AddLength();
                                            if (Client.TibiaVersion >= 770) { p.XteaEncrypt(this.XteaKey, p.GetPosition); p.AddLength(); }
                                            p.Send(this.TcpClientLocal);
                                            break;
                                        case "goto":
                                            TimeSpan ts;
                                            if (!TimeSpan.TryParse(msgSplit[1], out ts)) break;
                                            this.FastForward(ts);
                                            break;
                                        case "pause":
                                            this.PlaybackSpeed = 0;
                                            break;
                                        case "resume":
                                            this.PlaybackSpeed = 1;
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                }
                if (TcpClientLocal != null) TcpClientLocal.Close();
            }
            catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace); if (TcpClientLocal != null) TcpClientLocal.Close(); }
        }
        #endregion

        #endregion

        internal enum TibiaCamMode
        {
            Undefined = 0,
            Playback = 1,
            Record = 2
        }
    }
}
