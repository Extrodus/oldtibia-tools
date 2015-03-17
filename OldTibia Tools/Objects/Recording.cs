using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Compression;

namespace TibianicTools.Objects
{
    class Recording
    {
        /// <summary>
        /// Constructor for recording.
        /// </summary>
        internal Recording(bool recordMouse)
        {
            this.Type = RecordingType.Record;
            Packets = new List<Recording.Packet>();
            this.TibiaVersion = Client.Tibia.MainModule.FileVersionInfo.FileVersion;
            this.RecorderVersion = Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion;
            this.TibiaVersionNumber = Client.TibiaVersion;
            this.RecorderVersionNumber = Settings.CurrentVersion;
            this.Duration = 0;

            if (recordMouse)
            {
                this.MouseRecording = new MouseRecording();
            }
        }
        /// <summary>
        /// Constructor for playback.
        /// </summary>
        /// <param name="fileName">Relative or absolute path (including file name).</param>
        /// <param name="readMetaData">If true, will read data like Tibia version, recorder version and duration.</param>
        /// <param name="readPackets">If true, will buffer all packets to memory.</param>
        /// <param name="readMouseMovements">If true, will try to read a mouse movement file (*.kcammouse) with the same file name as the recording.</param>
        internal Recording(string fileName, bool readMetaData, bool readPackets, bool readMouseMovements)
        {
            this.Type = RecordingType.Playback;
            if (File.Exists(fileName))
            {
                this.Packets = new List<Recording.Packet>();

                if (fileName.EndsWith(".kcam")) Recorder = Enums.Recorder.TibianicTools;
                else if (fileName.EndsWith(".tmv")) Recorder = Enums.Recorder.TibiaMovie;
                else if (fileName.EndsWith(".cam")) Recorder = Enums.Recorder.IryontCam;
                else { return; }
                FileName = fileName; RecorderVersion = "?"; TibiaVersion = "?"; Duration = 0;
                //if (fileName.Contains("\\")) { FileNameShort = fileName.Substring(fileName.LastIndexOf('\\') + 1); }
                //else { FileNameShort = fileName; }
                if (readMetaData || readPackets || readMouseMovements)
                {
                    switch (this.Recorder)
                    {
                        case Enums.Recorder.IryontCam:
                            try
                            {
                                using (FileStream fstream = File.OpenRead(this.FileName))
                                {
                                    using (BinaryReader reader = new BinaryReader(fstream))
                                    {
                                        if (readPackets)
                                        {
                                            uint tickFirst = 0;
                                            // add first packet
                                            tickFirst = (uint)reader.ReadUInt64();
                                            ushort len = reader.ReadUInt16();
                                            byte[] data = new byte[len + 2];
                                            Array.Copy(BitConverter.GetBytes(len), data, 2);
                                            byte[] packet = reader.ReadBytes(len);
                                            Array.Copy(packet, 0, data, 2, packet.Length);
                                            this.Packets.Add(new Recording.Packet(0, data));
                                            while (reader.BaseStream.Position < reader.BaseStream.Length)
                                            {
                                                uint tick = (uint)reader.ReadUInt64() - tickFirst;
                                                len = reader.ReadUInt16();
                                                data = new byte[len + 2];
                                                Array.Copy(BitConverter.GetBytes(len), data, 2);
                                                packet = reader.ReadBytes(len);
                                                Array.Copy(packet, 0, data, 2, packet.Length);
                                                this.Packets.Add(new Recording.Packet(tick, data));
                                            }
                                            Recording.Packet lastPacket = this.Packets[this.Packets.Count - 1];
                                            this.Duration = lastPacket.Time;
                                        }
                                    }
                                }
                            }
                            catch { this.isCorrupt = true; }
                            break;
                        case Enums.Recorder.TibiaMovie:
                            /*using (FileStream stream = File.OpenRead(fileName))
                            {
                                if (readMetaData)
                                {
                                    byte[] metaData = new byte[8];
                                    stream.Read(metaData, 0, 8);
                                    metaData = Utils.TibiaCam.Zlib.Decompress(metaData);
                                    RecorderVersion = BitConverter.ToUInt16(metaData, 0).ToString();
                                    TibiaVersion = BitConverter.ToUInt16(metaData, 2).ToString();
                                    Duration = BitConverter.ToUInt32(metaData, 4);
                                }
                                if (readPackets)
                                {
                                    byte[] buffer = new byte[stream.Length - 8];
                                    if (!readMetaData) stream.Position += 8;
                                    stream.Read(buffer, 0, buffer.Length);
                                    Utils.TibiaCam.Zlib.Decompress(buffer);
                                    Packets = Utils.TibiaCam.Zlib.BytesToPacket(Utils.TibiaCam.Zlib.Decompress(buffer));
                                }
                                stream.Close();
                            }*/
                            break;
                        case Enums.Recorder.TibianicTools:
                            if (readMouseMovements)
                            {
                                string mouseFileName = fileName + "mouse";
                                if (File.Exists(mouseFileName))
                                {
                                    try { this.MouseRecording = new MouseRecording(mouseFileName); }
                                    catch { this.MouseRecording = null; }
                                }
                            }
                            try
                            {
                                Stream stream = Utils.TibiaCam.DecompressCamToStream(fileName);
                                int firstByte = stream.ReadByte();
                                stream.Position = 0;
                                if (firstByte == (int)'T') // true for 1.2 and older
                                {
                                    isOld = true;
                                    List<string> strings = new List<string>();
                                    StreamReader reader = new StreamReader(stream);
                                    TibiaVersion = reader.ReadLine().Replace("TibiaVersion=", "");
                                    RecorderVersion = reader.ReadLine().Replace("TibiaCamVersion=", "");
                                    Duration = (uint)double.Parse(reader.ReadLine().Replace("TotalRunningTime=", ""));
                                    if (readPackets)
                                    {
                                        while (true)
                                        {
                                            string line = reader.ReadLine();
                                            if (line == null || line.Length == 0) break;
                                            strings.Add(line);
                                        }
                                        stream.Close();
                                        stream.Dispose();
                                        List<Recording.Packet> packets = new List<Recording.Packet>();
                                        foreach (string line in strings)
                                        {
                                            string temp = line;
                                            uint sleep = uint.Parse(temp.Substring(0, temp.IndexOf(':')));
                                            temp = temp.Remove(0, temp.IndexOf(':') + 1);
                                            string[] split = temp.Split(' ');
                                            Objects.Packet packet = new Objects.Packet();
                                            for (int j = 0; j < split.Length; j++)
                                            {
                                                packet.AddByte(byte.Parse(split[j], System.Globalization.NumberStyles.AllowHexSpecifier));
                                            }
                                            packet.GetPosition = 0;
                                            packets.Add(new Recording.Packet(sleep, packet.ToBytes()));
                                        }
                                        Packets = packets;
                                    }
                                }
                                else
                                {
                                    isOld = false;
                                    BinaryReader reader = new BinaryReader(stream);
                                    byte[] metadata = new byte[8]; // 2 bytes TibiaVersion, 2 bytes CamVersion, 4 bytes RunningLength(ms)
                                    reader.Read(metadata, 0, 8); // fill metadata
                                    if (readMetaData)
                                    {
                                        TibiaVersion = metadata[0] + "." + metadata[1];
                                        RecorderVersion = metadata[2] + "." + metadata[3];
                                        Duration = BitConverter.ToUInt32(metadata, 4);
                                    }
                                    if (readPackets)
                                    {
                                        List<Recording.Packet> packets = new List<Recording.Packet>();
                                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                                        {
                                            uint sleep = reader.ReadUInt32();
                                            uint len = reader.ReadUInt32(); // should be changed to UInt16 in future versions as packets never exceed 65k bytes
                                            // merge split packets
                                            ushort packetLen = reader.ReadUInt16();
                                            if (packetLen > len - 2)
                                            {
                                                Objects.Packet p = new Objects.Packet();
                                                reader.BaseStream.Position += 4;
                                                p.AddUInt16(packetLen);
                                                p.AddBytes(reader.ReadBytes((int)len - 2));
                                                uint totalBytesRead = len - 2;
                                                while (totalBytesRead < packetLen)
                                                {
                                                    reader.ReadUInt32(); // sleep, not needed
                                                    len = reader.ReadUInt32();
                                                    p.AddBytes(reader.ReadBytes((int)len));
                                                    totalBytesRead += len;
                                                }
                                                packets.Add(new Recording.Packet(sleep, p.ToBytes()));
                                            }
                                            else
                                            {
                                                reader.BaseStream.Position -= 2;
                                                packets.Add(new Recording.Packet(sleep, reader.ReadBytes((int)len)));
                                            }
                                        }
                                        Packets = packets;
                                        // if duration is 0, get duration from last packet
                                        if (this.Duration == 0)
                                        {
                                            Recording.Packet p = this.Packets[this.Packets.Count - 1];
                                            this.Duration = p.Time;
                                        }
                                    }
                                    reader.Close();
                                    stream.Close();
                                    stream.Dispose();
                                    if (readMouseMovements)
                                    {
                                        string name = fileName.Substring(0, fileName.LastIndexOf('.'));
                                        name += ".kcammouse";
                                        if (File.Exists(name))
                                        {
                                            MouseRecording = new MouseRecording(name);
                                        }
                                        else { MouseRecording = null; }
                                    }
                                }
                            }
                            catch { isCorrupt = true; }
                            break;
                    }
                }
            }
        }

        internal void Save(string fileName)
        {
            if (this.Type != RecordingType.Record) return;
            if (this.Packets.Count == 0) return;
            if (this.MouseRecording != null && this.MouseRecording.IsRecording) this.MouseRecording.StopRecording();
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "Unnamed";
                int i = 0;
                while (File.Exists(fileName + "-" + i + ".kcam")) i++;
                fileName += "-" + i + ".kcam";
            }
            else if (File.Exists(fileName)) File.Delete(fileName);
            Recording.Packet lastPacket = Packets[Packets.Count - 1];
            this.Duration = lastPacket.Time;
            //Utils.TibiaCam.CompressCam(Packets, fileName);
            using (MemoryStream memstream = new MemoryStream())
            {
                string[] split = this.TibiaVersion.Split('.');
                memstream.Write(new byte[] { byte.Parse(split[0]), byte.Parse(split[1]) }, 0, 2); // holy shit this is ugly, fucken backwards compability
                split = this.RecorderVersion.Split('.');
                memstream.Write(new byte[] { byte.Parse(split[0]), byte.Parse(split[1]) }, 0, 2);
                memstream.Write(BitConverter.GetBytes(this.Duration), 0, 4);
                foreach (Recording.Packet p in this.Packets)
                {
                    memstream.Write(BitConverter.GetBytes(p.Time), 0, 4);
                    memstream.Write(BitConverter.GetBytes((uint)p.Data.Length), 0, 4);
                    memstream.Write(p.Data, 0, p.Data.Length);
                }
                memstream.Position = 0;

                using (FileStream fstream = File.Create(fileName + (!fileName.EndsWith(".kcam") ? ".kcam" : string.Empty)))
                {
                    using (DeflateStream compressStream = new DeflateStream(fstream, CompressionMode.Compress))
                    {
                        compressStream.Write(memstream.ToArray(), 0, (int)memstream.Length);
                    }
                }
            }

            if (this.MouseRecording != null)
            {
                string mouseFileName = fileName + "mouse";
                this.MouseRecording.Save(mouseFileName);
                //this.MouseRecording = null;
            }

            this.Packets.Clear();
        }

        internal List<Recording.Packet> Packets { get; set; }
        internal string FileName { get; set; }
        internal string FileNameShort
        {
            get
            {
                if (string.IsNullOrEmpty(FileName)) return string.Empty;
                return this.FileName.Substring(FileName.LastIndexOf("\\") + 1);
            }
        }
        /// <summary>
        /// Returns amount of milliseconds passed so far.
        /// </summary>
        internal uint TimePassed { get; set; }
        /// <summary>
        /// Returns total amount of milliseconds of this recording. Playback only.
        /// </summary>
        internal uint Duration { get; set; }
        internal string RecorderVersion { get; set; }
        internal ushort RecorderVersionNumber { get; set; }
        internal string TibiaVersion { get; set; }
        internal ushort TibiaVersionNumber { get; set; }
        internal Enums.Recorder Recorder { get; set; }
        internal bool isCorrupt { get; set; }
        /// <summary>
        /// Returns true if the recording uses the old (v1.0 to v1.2) string structure.
        /// Needed since newer versions have extra bytes with each packet.
        /// </summary>
        internal bool isOld { get; set; }
        internal MouseRecording MouseRecording { get; set; }
        internal RecordingType Type { get; set; }

        internal bool ContainsData()
        {
            return this.Packets.Count > 3;
        }

        internal enum RecordingType : byte
        {
            Playback = 0,
            Record = 1
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.FileName)) { return FileName; }
            else { return "No filename set"; }
        }

        #region nested packet class
        internal class Packet
        {
            internal Packet(uint time, byte[] data)
            {
                this.Time = time;
                this.Data = data;
            }

            /// <summary>
            /// Amount of time, in milliseconds, since the recording started.
            /// </summary>
            internal uint Time { get; set; }
            /// <summary>
            /// The packet to send.
            /// </summary>
            internal byte[] Data { get; set; }

            internal bool Send(System.Net.Sockets.TcpClient tcpclient)
            {
                if (tcpclient != null && tcpclient.Connected)
                {
                    tcpclient.GetStream().Write(this.Data, 0, this.Data.Length);
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Encrypts the packet with Xtea.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            internal unsafe Recording.Packet XteaEncrypt(uint[] key, int index)
            {
                if (key == null || index >= this.Data.Length) return null;

                byte[] tempbuffer = new byte[this.Data.Length - index];
                Array.Copy(this.Data, index, tempbuffer, 0, tempbuffer.Length);
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
                List<byte> bytes = new List<byte>(buffer);
                bytes.InsertRange(0, BitConverter.GetBytes((ushort)buffer.Length));
                return new Recording.Packet(this.Time, bytes.ToArray());
            }
            internal Recording.Packet Clone() { return new Packet(this.Time, this.Data); }
        }
        #endregion
    }

    class MouseRecording
    {
        /// <summary>
        /// Constructor for recording.
        /// </summary>
        internal MouseRecording() { this.Packets = new List<Objects.Packet>(); }
        /// <summary>
        /// Constructor for playback.
        /// </summary>
        /// <param name="fileName"></param>
        internal MouseRecording(string fileName)
        { 
            this.FileName = fileName;
            this.Packets = new List<Objects.Packet>();
            if (!File.Exists(fileName)) return;
            using (Stream stream = Utils.TibiaCam.DecompressCamToStream(fileName))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    this._RecorderVersion = reader.ReadUInt16();
                    this.Interval = reader.ReadUInt16();

                    WinApi.RECT r = new WinApi.RECT();
                    r.left = 0;
                    r.top = 0;
                    r.right = reader.ReadUInt16();
                    r.bottom = reader.ReadUInt16();
                    this.clientWindowOld = r;

                    WinApi.RECT r2 = new WinApi.RECT();
                    r2.left = reader.ReadUInt16();
                    r2.top = reader.ReadUInt16();
                    r2.right = reader.ReadUInt16();
                    r2.bottom = reader.ReadUInt16();
                    this.gameWindowOld = r2;

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        Objects.Packet p = new Objects.Packet();
                        DataType dt = (DataType)reader.ReadByte();
                        p.AddByte((byte)dt);
                        switch (dt)
                        {
                            case DataType.ClientWindowChange:
                                p.AddUInt16(reader.ReadUInt16()); // width
                                p.AddUInt16(reader.ReadUInt16()); // height
                                break;
                            case DataType.GameWindowChange:
                                p.AddUInt16(reader.ReadUInt16()); // x
                                p.AddUInt16(reader.ReadUInt16()); // y
                                p.AddUInt16(reader.ReadUInt16()); // width
                                p.AddUInt16(reader.ReadUInt16()); // height
                                break;
                            case DataType.MouseXY:
                                p.AddByte(reader.ReadByte()); // mouse %x
                                p.AddByte(reader.ReadByte()); // mouse %y
                                break;
                        }
                        this.Packets.Add(p);
                    }
                }
            }
        }

        /// <summary>
        /// Meta data:
        /// 2 bytes recorder version,
        /// 2 bytes interval,
        /// 2 bytes ClientRect width,
        /// 2 bytes ClientRect height,
        /// 2 bytes GameWindow X,
        /// 2 bytes GameWindow Y,
        /// 2 bytes GameWindow width,
        /// 2 bytes GameWindow height,
        /// </summary>
        internal List<Objects.Packet> Packets { get; set; }
        internal string FileName { get; set; }
        /// <summary>
        /// Determines how often the mouse position is updated.
        /// </summary>
        internal ushort Interval { get; set; }
        internal double PlaybackSpeed { get; set; }
        private ushort _RecorderVersion { get; set; }
        internal ushort RecorderVersion { get { return this._RecorderVersion; } }
        private bool isRecording { get; set; }
        internal bool IsRecording { get { return this.isRecording; } }
        /// <summary>
        /// The current index of Packets. Used only in playback.
        /// </summary>
        internal int CurrentIndex { get; set; }
        private WinApi.RECT gameWindowOld { get; set; }
        private WinApi.RECT gameWindowNew { get; set; }
        private WinApi.RECT clientWindowOld { get; set; }
        private WinApi.RECT clientWindowNew { get; set; }   
        internal enum DataType
        {
            MouseXY = 0,
            GameWindowChange = 1,
            ClientWindowChange = 2
        }

        internal bool StartRecording()
        {
            if (Packets.Count > 0 || isRecording) return false; // already running
            isRecording = true;
            Thread t = new Thread(new ThreadStart(ThreadRecord));
            t.Start();
            return true;
        }
        internal bool StopRecording()
        {
            if (Packets.Count == 0 || !isRecording) return false;
            isRecording = false;
            return true;
        }
        internal void Save(string fileName)
        {
            if (File.Exists(fileName)) File.Delete(fileName);
            Utils.TibiaCam.Compress(Packets, fileName);
            this.Packets = new List<Objects.Packet>();
        }
        /// <summary>
        /// Returns the index of given millisecond.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        internal int Seek(uint millisecond)
        {
            int i = 0;
            while (millisecond > Interval * i)
            {
                i++;
            }
            return i;
        }

        private void ThreadRecord()
        {
            try
            {
                this.Packets = new List<Objects.Packet>();
                Objects.Packet meta = new Objects.Packet();
                meta.AddUInt16((ushort)Settings.CurrentVersion); // recorder version
                meta.AddUInt16(Settings.Tibiacam.Recorder.MouseInterval);
                WinApi.RECT clientRect = new WinApi.RECT();
                WinApi.GetClientRect(Client.Tibia.MainWindowHandle, out clientRect);
                this.clientWindowOld = clientRect;
                //meta.AddUInt16((ushort)clientRect.left);
                //meta.AddUInt16((ushort)clientRect.top);
                meta.AddUInt16((ushort)clientRect.right);
                meta.AddUInt16((ushort)clientRect.bottom);
                WinApi.RECT r = Client.Misc.GameWindow;
                this.gameWindowOld = r;
                meta.AddUInt16((ushort)r.left);
                meta.AddUInt16((ushort)r.top);
                meta.AddUInt16((ushort)r.right);
                meta.AddUInt16((ushort)r.bottom);
                this.Packets.Add(meta);
                this.Interval = Settings.Tibiacam.Recorder.MouseInterval;
                while (isRecording)
                {
                    Objects.Packet p;
                    WinApi.RECT newClientRect = new WinApi.RECT();
                    WinApi.GetClientRect(Client.Tibia.MainWindowHandle, out newClientRect);
                    if (newClientRect.right != this.clientWindowOld.right ||
                        newClientRect.bottom != this.clientWindowOld.bottom)
                    {
                        p = new Objects.Packet();
                        this.clientWindowOld = newClientRect;
                        p.AddByte((byte)DataType.ClientWindowChange);
                        p.AddUInt16((ushort)newClientRect.right);
                        p.AddUInt16((ushort)newClientRect.bottom);
                        this.Packets.Add(p);
                        continue;
                    }
                    WinApi.RECT newRect = Client.Misc.GameWindow;
                    if (this.gameWindowOld.left != newRect.left ||
                        this.gameWindowOld.bottom != newRect.bottom)
                    {
                        p = new Objects.Packet();
                        this.gameWindowOld = newRect;
                        p.AddByte((byte)DataType.GameWindowChange);
                        p.AddUInt16((ushort)newRect.left);
                        p.AddUInt16((ushort)newRect.top);
                        p.AddUInt16((ushort)newRect.right);
                        p.AddUInt16((ushort)newRect.bottom);
                        this.Packets.Add(p);
                        continue;
                    }
                    p = new Objects.Packet();
                    System.Drawing.Point mousePercentage = this.CalculateMousePercentage();
                    p.AddByte((byte)DataType.MouseXY);
                    if (WinApi.GetForegroundWindow() == Client.Tibia.MainWindowHandle)
                    {
                        p.AddByte((byte)mousePercentage.X);
                        p.AddByte((byte)mousePercentage.Y);
                    }
                    else
                    {
                        p.AddByte(0);
                        p.AddByte(0);
                    }
                    this.Packets.Add(p);
                    Thread.Sleep(this.Interval);
                }
            }
            catch (Exception ex) { MessageBox.Show("Mouse recording error:\n" + ex.Message + "\n" + ex.StackTrace); }
            isRecording = false;
        }

        /// <summary>
        /// Calculates a mouse position based on percentage. X-axis is based on WinApi.RECT.right, Y-axis is based on WinApi.RECT.bottom.
        /// </summary>
        /// <returns></returns>
        internal System.Drawing.Point CalculateMousePercentage()
        {
            WinApi.RECT clientBounds = this.GetClientBounds();
            WinApi.RECT gameWindowBounds = Client.Misc.GameWindow;
            gameWindowBounds.top += clientBounds.top;
            gameWindowBounds.left += clientBounds.left;
            gameWindowBounds.right += gameWindowBounds.left;
            gameWindowBounds.bottom += gameWindowBounds.top;
            System.Drawing.Point cursorPos = Cursor.Position;
            //Cursor.Position = new System.Drawing.Point(clientBounds.left, clientBounds.top);
            if (cursorPos.X > gameWindowBounds.left && cursorPos.X < gameWindowBounds.right &&
                cursorPos.Y > gameWindowBounds.top && cursorPos.Y < gameWindowBounds.bottom)
            {
                double percentX = ((double)cursorPos.X - (double)gameWindowBounds.left) / ((double)gameWindowBounds.right - (double)gameWindowBounds.left);
                double percentY = ((double)cursorPos.Y - (double)gameWindowBounds.top) / ((double)gameWindowBounds.bottom - (double)gameWindowBounds.top);
                percentX *= 100;
                percentY *= 100;
                return new System.Drawing.Point((int)Math.Round(percentX, 0), (int)Math.Round(percentY, 0));
            }
            return new System.Drawing.Point(0, 0);
        }
        internal System.Drawing.Point CalculateScreenMouseXY(System.Drawing.Point cursorPercent)
        {
            WinApi.RECT clientBounds = this.GetClientBounds();
            WinApi.RECT gameWindowBounds = Client.Misc.GameWindow;
            gameWindowBounds.top += clientBounds.top;
            gameWindowBounds.left += clientBounds.left;
            gameWindowBounds.right += gameWindowBounds.left;
            gameWindowBounds.bottom += gameWindowBounds.top;
            int x = (int)Math.Round((gameWindowBounds.right - gameWindowBounds.left) * ((double)cursorPercent.X / (double)100), 0),
                y = (int)Math.Round((gameWindowBounds.bottom - gameWindowBounds.top) * ((double)cursorPercent.Y / (double)100), 0);
            x += gameWindowBounds.left;
            y += gameWindowBounds.top;
            return new System.Drawing.Point(x, y);
        }

        private WinApi.RECT GetClientBounds()
        {
            WinApi.WINDOWINFO info = new WinApi.WINDOWINFO();
            info.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(info);
            WinApi.GetWindowInfo(Client.Tibia.MainWindowHandle, ref info);
            return info.rcClient;
        }
    }
}
