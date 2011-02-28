using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Drawing.Imaging;
using System.Drawing;
using System.Management;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace TibianicTools
{
    public class Utils
    {
        internal class Network
        {
            /// <summary>
            /// Returns a connected TcpClient if successful, returns null if unsuccessful. Should be run on its own thread.
            /// </summary>
            /// <param name="ip">IP or DNS to connect to.</param>
            /// <param name="port">The port to connect to.</param>
            /// <param name="isDNS">True if ip is a DNS</param>
            /// <returns></returns>
            internal static TcpClient Connect(string ip, int port)
            {
                try
                {
                    IPAddress IP = null;
                    if (!IPAddress.TryParse(ip, out IP))
                    {
                        try { IP = Dns.GetHostAddresses(ip)[0]; }
                        catch { return null; }
                    }
                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(IP, port);
                    if (tcpClient.Connected) { return tcpClient; }
                    else { return null; }
                }
                catch { return null; }
            }

            internal static bool SendPacket(TcpClient client, byte[] packet)
            {
                if (client.Connected)
                {
                    NetworkStream netStream = client.GetStream();
                    netStream.Write(packet, 0, packet.Length);
                    netStream.Flush();
                    return true;
                }
                return false;
            }

            internal static bool SendPacket(TcpClient client, Packet packet)
            {
                return Utils.Network.SendPacket(client, packet.ToBytes());
            }
        }

        /// <summary>
        /// Credits: http://forums.devx.com/showthread.php?t=159027
        /// </summary>
        internal class ThreadSafe
        {
            private delegate void SetTextDelegate(Control control, string text);
            internal static void SetText(Control control, string text)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new SetTextDelegate(SetText), new object[] { control, text });
                }
                else
                {
                    control.Text = text;
                }
            }

            private delegate void SetVisibleDelegate(Control control, bool visible);
            internal static void SetVisible(Control control, bool visible)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new SetVisibleDelegate(SetVisible), new object[] { control, visible });
                }
                else
                {
                    control.Visible = visible;
                }
            }

            private delegate void SetEnabledDelegate(Control control, bool enabled);
            internal static void SetEnabled(Control control, bool enabled)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new SetEnabledDelegate(SetEnabled), new object[] { control, enabled });
                }
                else { control.Enabled = enabled; }
            }

            private delegate void SetValueDelegate(NumericUpDown control, decimal value);
            internal static void SetValue(NumericUpDown control, decimal value)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new SetValueDelegate(SetValue), new object[] { control, value });
                }
                else { control.Value = value; }
            }
        }

        #region APIs and Consts
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, int lParam);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        const int VK_ESCAPE = 0x1B;
        const int VK_RETURN = 0x0D;
        public const int F1 = 0x70;
        public const int F2 = 0x71;
        public const int F3 = 0x72;
        public const int F4 = 0x73;
        public const int F5 = 0x74;
        public const int F6 = 0x75;
        public const int F7 = 0x76;
        public const int F8 = 0x77;
        public const int F9 = 0x78;
        public const int F10 = 0x79;
        public const int F11 = 0x7A;
        public const int F12 = 0x7B;
        const int VK_LCONTROL = 162;
        const int VK_RCONTROL = 163;
        const int VK_LSHIFT = 160;
        const int VK_RSHIFT = 161;
        public const int WM_CHAR = 0x0102;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SETTEXT = 0x0C;
        public const int CONTROL = 0x11;
        public const int SHIFT = 0x10;
        public const int VK_LEFT = 0x25;
        public const int VK_UP = 0x26;
        public const int VK_RIGHT = 0x27;
        public const int VK_DOWN = 0x28;
        #endregion

        internal static void SendTibiaKeys(string s)
        {
            IntPtr hWnd = Client.Tibia.MainWindowHandle;
            switch (s.ToUpper())
            {
                #region Send Keys
                case "ESCAPE":
                    PostMessage(hWnd, WM_KEYDOWN, VK_ESCAPE, 0);
                    PostMessage(hWnd, WM_KEYUP, VK_ESCAPE, 0);
                    break;
                case "ENTER":
                    PostMessage(hWnd, WM_KEYDOWN, VK_RETURN, 0);
                    PostMessage(hWnd, WM_KEYUP, VK_RETURN, 0);
                    break;
                case "UP":
                    PostMessage(hWnd, WM_KEYDOWN, VK_UP, 0);
                    PostMessage(hWnd, WM_KEYUP, VK_UP, 0);
                    break;
                case "LEFT":
                    PostMessage(hWnd, WM_KEYDOWN, VK_LEFT, 0);
                    PostMessage(hWnd, WM_KEYUP, VK_LEFT, 0);
                    break;
                case "DOWN":
                    PostMessage(hWnd, WM_KEYDOWN, VK_DOWN, 0);
                    PostMessage(hWnd, WM_KEYUP, VK_DOWN, 0);
                    break;
                case "RIGHT":
                    PostMessage(hWnd, WM_KEYDOWN, VK_RIGHT, 0);
                    PostMessage(hWnd, WM_KEYUP, VK_RIGHT, 0);
                    break;
                case "DOWNLEFT":
                    PostMessage(hWnd, WM_KEYDOWN, 35, 0);
                    PostMessage(hWnd, WM_KEYUP, 35, 0);
                    break;
                case "DOWNRIGHT":
                    PostMessage(hWnd, WM_KEYDOWN, 34, 0);
                    PostMessage(hWnd, WM_KEYUP, 34, 0);
                    break;
                case "UPLEFT":
                    PostMessage(hWnd, WM_KEYDOWN, 36, 0);
                    PostMessage(hWnd, WM_KEYUP, 36, 0);
                    break;
                case "UPRIGHT":
                    PostMessage(hWnd, WM_KEYDOWN, 33, 0);
                    PostMessage(hWnd, WM_KEYUP, 33, 0);
                    break;
                case "F1":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F1, 0);
                    PostMessage(hWnd, WM_KEYUP, F1, 0);
                    break;
                case "F2":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F2, 0);
                    PostMessage(hWnd, WM_KEYUP, F2, 0);
                    break;
                case "F3":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F3, 0);
                    PostMessage(hWnd, WM_KEYUP, F3, 0);
                    break;
                case "F4":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F4, 0);
                    PostMessage(hWnd, WM_KEYUP, F4, 0);
                    break;
                case "F5":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F5, 0);
                    PostMessage(hWnd, WM_KEYUP, F5, 0);
                    break;
                case "F6":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F6, 0);
                    PostMessage(hWnd, WM_KEYUP, F6, 0);
                    break;
                case "F7":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F7, 0);
                    PostMessage(hWnd, WM_KEYUP, F7, 0);
                    break;
                case "F8":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F8, 0);
                    PostMessage(hWnd, WM_KEYUP, F8, 0);
                    break;
                case "F9":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F9, 0);
                    PostMessage(hWnd, WM_KEYUP, F9, 0);
                    break;
                case "F10":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F10, 0);
                    PostMessage(hWnd, WM_KEYUP, F10, 0);
                    break;
                case "F11":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F11, 0);
                    PostMessage(hWnd, WM_KEYUP, F11, 0);
                    break;
                case "F12":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F12, 0);
                    PostMessage(hWnd, WM_KEYUP, F12, 0);
                    break;
                case "SHIFT+F1":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F1, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F1, 0);
                    break;
                case "SHIFT+F2":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F2, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F2, 0);
                    break;
                case "SHIFT+F3":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F3, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F3, 0);
                    break;
                case "SHIFT+F4":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F4, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F4, 0);
                    break;
                case "SHIFT+F5":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F5, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F5, 0);
                    break;
                case "SHIFT+F6":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F6, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F6, 0);
                    break;
                case "SHIFT+F7":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F7, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F7, 0);
                    break;
                case "SHIFT+F8":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F8, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F8, 0);
                    break;
                case "SHIFT+F9":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F9, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F9, 0);
                    break;
                case "SHIFT+F10":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F10, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F10, 0);
                    break;
                case "SHIFT+F11":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F11, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F11, 0);
                    break;
                case "SHIFT+F12":
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F12, 0);
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYUP, F12, 0);
                    break;
                case "CTRL+F1":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F1, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F1, 0);
                    break;
                case "CTRL+F2":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F2, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F2, 0);
                    break;
                case "CTRL+F3":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F3, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F3, 0);
                    break;
                case "CTRL+F4":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F4, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F4, 0);
                    break;
                case "CTRL+F5":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F5, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F5, 0);
                    break;
                case "CTRL+F6":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F6, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F6, 0);
                    break;
                case "CTRL+F7":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F7, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F7, 0);
                    break;
                case "CTRL+F8":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F8, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F8, 0);
                    break;
                case "CTRL+F9":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F9, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F9, 0);
                    break;
                case "CTRL+F10":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F10, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F10, 0);
                    break;
                case "CTRL+F11":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F11, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F11, 0);
                    break;
                case "CTRL+F12":
                    PostMessage(hWnd, WM_KEYUP, SHIFT, 0);
                    PostMessage(hWnd, WM_KEYDOWN, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYDOWN, F12, 0);
                    PostMessage(hWnd, WM_KEYUP, CONTROL, 0);
                    PostMessage(hWnd, WM_KEYUP, F12, 0);
                    break;
                #endregion
                default:
                    if (isStringNumeric(s))
                    {
                        PostMessage(hWnd, WM_KEYDOWN, uint.Parse(s), 0);
                        PostMessage(hWnd, WM_KEYUP, uint.Parse(s), 0);
                    }
                    break;
            }
        }

        internal static void SendTibiaKey(Keys key)
        {
            PostMessage(Client.Tibia.MainWindowHandle, WM_CHAR, (uint)key, 0);
        }

        internal static void SendTibiaString(string s)
        {
            foreach (char c in s)
            {
                PostMessage(Client.Tibia.MainWindowHandle, WM_CHAR, (uint)c, 0);
            }
        }

        internal static void SendMouseClick(int x, int y)
        {
            uint WM_LBUTTONDOWN = 0x201; //Left mousebutton down
            uint WM_LBUTTONUP = 0x202;  //Left mousebutton up

            int LParam = MakeLParam(x, y);
            WinApi.SendMessage(Client.Tibia.MainWindowHandle, WM_LBUTTONDOWN, 0, LParam);
            WinApi.SendMessage(Client.Tibia.MainWindowHandle, WM_LBUTTONUP, 0, LParam);
        }

        internal static ImageFormat ConvertStringToImageFormat(string s)
        {
            switch (s)
            {
                case "BMP":
                    return ImageFormat.Bmp;
                case "JPG":
                case "JPEG":
                    return ImageFormat.Jpeg;
                case "GIF":
                    return ImageFormat.Gif;
                case "PNG":
                    return ImageFormat.Png;
                default:
                    return null;
            }
        }

        internal static void Screenshot(string targetFolder, string fileName, ImageFormat imgFormat, bool activeWindowOnly)
        {
            try
            {
                if (targetFolder.Length > 0 && fileName.Length > 0 && imgFormat != null)
                {
                    Bitmap bitmap = new Bitmap(1, 1);
                    Rectangle bounds;
                    if (activeWindowOnly)
                    {
                        WinApi.RECT srcRect;
                        if (WinApi.GetWindowRect(Client.Tibia.MainWindowHandle, out srcRect) != null)
                        {
                            int width = srcRect.right - srcRect.left;
                            int height = srcRect.bottom - srcRect.top;

                            bitmap = new Bitmap(width, height);
                            Graphics gr = Graphics.FromImage(bitmap);
                            gr.CopyFromScreen(srcRect.left, srcRect.top,
                                            0, 0, new Size(width, height),
                                            CopyPixelOperation.SourceCopy);
                        }

                    }
                    else
                    {
                        bounds = Screen.GetBounds(Point.Empty);
                        bitmap = new Bitmap(bounds.Width, bounds.Height);
                        using (Graphics gr = Graphics.FromImage(bitmap))
                        {
                            gr.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                        }
                    }
                    bitmap.Save(targetFolder + fileName + DateTime.Now.ToString("-yyyy-MM-dd~HH-mm-ss") + "." + imgFormat.ToString().ToLower(), imgFormat);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionHandler(ex);
            }
        }

        internal static bool isStringNumeric(string s)
        {
            if (s.Length > 0)
            {
                try
                {
                    int.Parse(s);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        internal static bool isStringBool(string s)
        {
            try
            {
                bool.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static int RandomizeInt(int Min, int Max)
        {
            Random randomizer = new Random();
            if (Min > Max)
            {
                int tempMin = Min, tempMax = Max;
                Min = tempMax;
                Max = tempMin;
            }
            return randomizer.Next(Min, Max);
        }

        /// <summary>
        /// Compresses a *.kcam file. Version 1.2 and below.
        /// </summary>
        /// <param name="Packets"></param>
        /// <param name="FileName"></param>
        internal static void CompressCam(List<string> Packets, string FileName)
        {
            MemoryStream stream = new MemoryStream();
            byte[] buffer = new byte[4096];
            foreach (string line in Packets)
            {
                byte[] temp = ASCIIEncoding.ASCII.GetBytes(line + "\n");
                stream.Write(temp, 0, temp.Length);
            }
            stream.Position = 0;
            if (!FileName.EndsWith(".kcam"))
            {
                FileName += ".kcam";
            }
            // Create the compressed file.
            using (FileStream outFile =
                    File.Create(FileName))
            {
                using (DeflateStream Compress =
                    new DeflateStream(outFile,
                    CompressionMode.Compress))
                {
                    // Copy the source file into 
                    // the compression stream.
                    buffer = new byte[4096];
                    int numRead;
                    while ((numRead = stream.Read(buffer,
                            0, buffer.Length)) != 0)
                    {
                        Compress.Write(buffer, 0, numRead);
                    }
                }
            }
        }

        /// <summary>
        /// Compresses a *.kcam file. Version 1.21 and higher.
        /// </summary>
        /// <param name="Packets"></param>
        /// <param name="FileName"></param>
        internal static void CompressCam(List<byte[]> Packets, string FileName)
        {
            MemoryStream stream = new MemoryStream();
            byte[] buffer = new byte[4096];
            for (int i = 0; i < Packets.Count; i++)
            {
                stream.Write(Packets[i], 0, Packets[i].Length);
            }
            stream.Position = 0;
            if (!FileName.EndsWith(".kcam"))
            {
                FileName += ".kcam";
            }
            // Create the compressed file.
            using (FileStream outFile =
                    File.Create(FileName))
            {
                using (DeflateStream Compress =
                    new DeflateStream(outFile,
                    CompressionMode.Compress))
                {
                    // Copy the source file into 
                    // the compression stream.
                    buffer = new byte[4096];
                    int numRead;
                    while ((numRead = stream.Read(buffer,
                            0, buffer.Length)) != 0)
                    {
                        Compress.Write(buffer, 0, numRead);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true for v1.2 and lower.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        internal static bool isCamOld(string Path)
        {
            try
            {
                MemoryStream uncompressedStream = new MemoryStream();
                FileInfo fi = new FileInfo(Path);
                using (FileStream inFile = fi.OpenRead())
                {
                    using (DeflateStream Decompress = new DeflateStream(inFile,
                        CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int numRead = 0;
                        while ((numRead =
                            Decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            uncompressedStream.Write(buffer, 0, numRead);
                        }
                    }
                }
                uncompressedStream.Position = 0;
                StreamReader streamReader = new StreamReader(uncompressedStream);
                if (streamReader.ReadLine().StartsWith("Tibia"))
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Decompresses a *.kcam file. Version 1.2 and lower.
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        internal static List<string> DecompressCam(string FilePath)
        {
            try
            {
                MemoryStream uncompressedStream = new MemoryStream();
                FileInfo fi = new FileInfo(FilePath);
                using (FileStream inFile = fi.OpenRead())
                {
                    using (DeflateStream Decompress = new DeflateStream(inFile,
                        CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int numRead = 0;
                        while ((numRead =
                            Decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            uncompressedStream.Write(buffer, 0, numRead);
                        }
                    }
                }
                uncompressedStream.Position = 0;
                List<string> DecompressedList = new List<string>();
                StreamReader streamReader = new StreamReader(uncompressedStream);
                while (true)
                {
                    string currentLine = streamReader.ReadLine();
                    if (currentLine == null)
                    {
                        break;
                    }
                    DecompressedList.Add(currentLine);
                }
                return DecompressedList;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Decompresses a *.kcam file. Version 1.21 and higher.
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        internal static List<byte[]> DecompressCamToBytes(string FilePath)
        {
            try
            {
                MemoryStream uncompressedStream = new MemoryStream();
                FileInfo fi = new FileInfo(FilePath);
                using (FileStream inFile = fi.OpenRead())
                {
                    using (DeflateStream Decompress = new DeflateStream(inFile,
                        CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int numRead = 0;
                        while ((numRead =
                            Decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            uncompressedStream.Write(buffer, 0, numRead);
                        }
                    }
                }
                uncompressedStream.Position = 0;
                List<byte[]> DecompressedList = new List<byte[]>();
                BinaryReader binReader = new BinaryReader(uncompressedStream);
                byte[] tempBuffer = binReader.ReadBytes(8); // 2 bytes TibiaVersion, 2 bytes CamVersion, 4 bytes RunningLength(ms)
                DecompressedList.Add(new byte[] { tempBuffer[0], tempBuffer[1] });
                DecompressedList.Add(new byte[] { tempBuffer[2], tempBuffer[3] });
                DecompressedList.Add(new byte[] { tempBuffer[4], tempBuffer[5], tempBuffer[6], tempBuffer[7] });
                while (binReader.BaseStream.Length > binReader.BaseStream.Position)
                {
                    byte[] sleep = binReader.ReadBytes(4);
                    byte[] packageLength = binReader.ReadBytes(4);
                    byte[] packet = binReader.ReadBytes(BitConverter.ToInt32(packageLength, 0));
                    byte[] fullPacket = new byte[8 + packet.Length];
                    Array.Copy(sleep, 0, fullPacket, 0, 4);
                    Array.Copy(packageLength, 0, fullPacket, 4, 4);
                    Array.Copy(packet, 0, fullPacket, 8, packet.Length);
                    DecompressedList.Add(fullPacket);
                }
                return DecompressedList;
            }
            catch (Exception ex)
            {
                //Utils.ExceptionHandler(ex);
            }
            return new List<byte[]>();
        }

        internal static bool FileWrite(string fileName, string text, bool append)
        {
            try
            {
                string Text = "";
                if (File.Exists(fileName))
                {
                    if (append)
                    {
                        StreamReader streamReader = new StreamReader(fileName);
                        Text = streamReader.ReadToEnd();
                        streamReader.Close();
                        streamReader.Dispose();
                    }
                    else { File.Delete(fileName); }
                }
                StreamWriter streamWriter = new StreamWriter(fileName);
                if (Text != "")
                {
                    streamWriter.Write(Text);
                }
                streamWriter.WriteLine(text.Trim('\n'));
                streamWriter.Close();
                streamWriter.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Utils.ExceptionHandler(ex);
                return false;
            }
        }

        internal static bool FileWrite(string fileName, string[] text, bool append)
        {
            try
            {
                string Text = "";
                if (File.Exists(fileName))
                {
                    if (append)
                    {
                        StreamReader streamReader = new StreamReader(fileName);
                        Text = streamReader.ReadToEnd();
                        streamReader.Close();
                        streamReader.Dispose();
                    }
                    else { File.Delete(fileName); }
                }
                StreamWriter streamWriter = new StreamWriter(fileName);
                if (Text != "")
                {
                    streamWriter.Write(Text);
                }
                foreach (string line in text)
                {
                    streamWriter.WriteLine(line.Replace("\r", "").Replace("\n", ""));
                }
                streamWriter.Close();
                streamWriter.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Utils.ExceptionHandler(ex);
                return false;
            }
        }

        internal static string[] FileRead(string FileName)
        {
            if (File.Exists(FileName))
            {
                StreamReader streamReader = new StreamReader(FileName);
                string[] result = streamReader.ReadToEnd().Split('\n');
                streamReader.Close();
                streamReader.Dispose();
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = result[i].TrimEnd('\r');
                }
                return result;
            }
            return new string[] { string.Empty };
        }

        internal class ExperienceCounter
        {
            #region Experience Counter
            internal static Stopwatch stopWatch = new Stopwatch();
            private static int LevelPercentOld = 100 - Memory.ReadInt(Addresses.Player.LevelPercent);
            private static int LevelPercentNew = 0,
                                LevelOld = Memory.ReadInt(Addresses.Player.Level), LevelNew = 0;
            private static long ExperienceOld = Memory.ReadUInt(Addresses.Player.Exp), ExperienceNew = 0;
            private static double GainedExp = 0, GainedLevelPercent = 0, GainedLevelsInPercent = 0,
                                  TempLevelPercent = 0;
            private static double ExpPerHour = 0, LevelPercentPerHour = 0;
            internal static int[] ExperienceTable = new int[] 
            #region ExperienceTable 
            {0,
            0,
            100,
            200,
            400,
            800,
            1500,
            2600,
            4200,
            6400,
            9300,
            13000,
            17600,
            23200,
            29900,
            37800,
            47000,
            57600,
            69700,
            83400,
            98800,
            116000,
            135100,
            156200,
            179400,
            204800,
            232500,
            262600,
            295200,
            330400,
            368300,
            409000,
            452600,
            499200,
            548900,
            601800,
            658000,
            717600,
            780700,
            847400,
            917800,
            992000,
            1070100,
            1152200,
            1238400,
            1328800,
            1423500,
            1522600,
            1626200,
            1734400,
            1847300,
            1965000,
            2087600,
            2215200,
            2347900,
            2485800,
            2629000,
            2777600,
            2931700,
            3091400,
            3256800,
            3428000,
            3605400,
            3788800,
            3978600,
            4175200,
            4379000,
            4590400,
            4809800,
            5037600,
            5274200,
            5520000,
            5775400,
            6040800,
            6316600,
            6603200,
            6901000,
            7210400,
            7531800,
            7865600,
            8212200,
            8572000,
            8945400,
            9332800,
            9734600,
            10151200,
            10583000,
            11030400,
            11493800,
            11973600,
            12470200,
            12984000,
            13515400,
            14064800,
            14632600,
            15219200,
            15825000,
            16450400,
            17095800,
            17761600,
            18448200,
            19156000,
            19885400,
            20636800,
            21410600,
            22207200,
            23027000,
            23870400,
            24737800,
            25629600,
            26546200,
            27488000,
            28455400,
            29448800,
            30468600,
            31515200,
            32589000,
            33690400,
            34819800,
            35977600,
            37164200};
#endregion

            internal static long GetExperienceTNL()
            {
                uint Level = Client.Player.Level;
                long ExperienceTNL = (50 * (Level) * (Level) * (Level) - 150 * (Level) * (Level) + 400 * (Level)) / 3;
                ExperienceTNL -= Client.Player.Experience;
                /*if (ExperienceTable.Length >= Level + 1)
                {
                    return ExperienceTable[Level + 1] - Client.Player.Experience;
                }*/
                return ExperienceTNL;
            }

            internal static int GetLevelPercentTNL()
            {
                return 100 - Memory.ReadInt(Addresses.Player.LevelPercent);
            }

            internal static void Start()
            {
                Reset();
                stopWatch.Start();
            }

            internal static void Stop()
            {
                Reset();
                stopWatch.Stop();
            }

            internal static void Reset()
            {
                LevelPercentOld = 100 - Memory.ReadInt(Addresses.Player.LevelPercent);
                GainedLevelPercent = 0;
                ExperienceOld = Memory.ReadUInt(Addresses.Player.Exp);
                GainedLevelPercent = 0;
                TempLevelPercent = 0;
                GainedLevelsInPercent = 0;
                GainedExp = 0;
                stopWatch.Reset();
                stopWatch.Start();
            }

            internal static void Pause()
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }
            }

            internal static void Resume()
            {
                if (!stopWatch.IsRunning)
                {
                    stopWatch.Start();
                }
            }

            internal static double GetExperiencePerHour()
            {
                if (GetGainedExperience() > 0)
                {
                    ExpPerHour = GainedExp / Math.Ceiling(GetTotalRunningTime().TotalSeconds) * 3600;
                    ExpPerHour = Math.Ceiling(ExpPerHour);
                    return ExpPerHour;
                }
                else
                {
                    ExpPerHour = 0;
                    return 0;
                }
            }

            internal static double GetLevelPercentPerHour()
            {
                if (GetGainedLevelPercent() > 0)
                {
                    LevelPercentPerHour = GainedLevelPercent / GetTotalRunningTime().TotalSeconds * 3600;
                    LevelPercentPerHour = Math.Ceiling(LevelPercentPerHour);
                    return LevelPercentPerHour;
                }

                return 0;
            }

            internal static string GetTimeLeftTNL()
            {
                if (GetExperiencePerHour() > 0)
                {
                    TimeSpan timespanTimeLeft = new TimeSpan();
                    if (Memory.ReadUInt(Addresses.Player.Level) < 120)
                    {
                        double TimeLeft = (GetExperienceTNL() * 3600) / ExpPerHour;
                        timespanTimeLeft = TimeSpan.FromSeconds(TimeLeft);
                        return string.Format("{0:D2}:{1:D2}:{2:D2}", timespanTimeLeft.Hours, timespanTimeLeft.Minutes, timespanTimeLeft.Seconds);
                    }
                    else if (GetGainedLevelPercent() > 0)
                    {
                        LevelPercentNew = 100 - Memory.ReadInt(Addresses.Player.LevelPercent);
                        double TimeLeftBasedOnLevelPercent = (LevelPercentNew * 3600) / GetLevelPercentPerHour();
                        timespanTimeLeft = TimeSpan.FromSeconds(TimeLeftBasedOnLevelPercent);
                        return string.Format("{0:D2}:{1:D2}:{2:D2}", timespanTimeLeft.Hours, timespanTimeLeft.Minutes, timespanTimeLeft.Seconds);
                    }
                }
                return "infinity";
            }

            internal static TimeSpan GetTotalRunningTime()
            {
                return stopWatch.Elapsed;
            }

            internal static string GetTotalRunningTimeString()
            {
                TimeSpan elapsed = stopWatch.Elapsed;
                return string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds);
            }

            internal static double GetGainedExperience()
            {
                ExperienceNew = Memory.ReadUInt(Addresses.Player.Exp);
                GainedExp = ExperienceNew - ExperienceOld;
                if (GainedExp < 0)
                {
                    GainedExp = 0;
                    ExperienceOld = ExperienceNew;
                }
                else if (GainedExp == ExperienceNew)
                {
                    GainedExp = 0;
                    ExperienceOld = ExperienceNew;
                }
                return GainedExp;
            }

            internal static double GetGainedLevelPercent()
            {
                LevelPercentNew = 100 - Memory.ReadInt(Addresses.Player.LevelPercent);
                LevelNew = Memory.ReadInt(Addresses.Player.Level);
                if (LevelOld < LevelNew)
                {
                    int LevelDifference = LevelNew - LevelOld - 1;
                    TempLevelPercent = GainedLevelPercent;
                    GainedLevelsInPercent = LevelDifference * 100;
                    LevelOld = LevelNew;
                    LevelDifference = 0;
                    LevelPercentOld = LevelPercentNew;
                }
                if (LevelOld == LevelNew)
                {
                    GainedLevelPercent = GainedLevelsInPercent + TempLevelPercent + (LevelPercentOld - LevelPercentNew);
                    GainedLevelPercent = Math.Round(GainedLevelPercent, 0);
                    if (GainedLevelPercent > 0)
                    {
                        return GainedLevelPercent;
                    }
                }
                return 0;
            }
        #endregion
        }

        internal class SkillCounter
        {
            #region Skill Counter
            struct GainedPercent
            {
                public int SkillOld;
                public int SkillNew;
                public int PercentOld;
                public int PercentNew;
            }
            private static GainedPercent gainedMLVL = new GainedPercent();
            private static GainedPercent gainedFist = new GainedPercent();
            private static GainedPercent gainedClub = new GainedPercent();
            private static GainedPercent gainedSword = new GainedPercent();
            private static GainedPercent gainedAxe = new GainedPercent();
            private static GainedPercent gainedDistance = new GainedPercent();
            private static GainedPercent gainedShielding = new GainedPercent();
            private static GainedPercent gainedFishing = new GainedPercent();

            private static Stopwatch stopWatch = new Stopwatch();
            private static Structs.Skill MLVL = new Structs.Skill();
            private static Structs.Skill Fist = new Structs.Skill();
            private static Structs.Skill Club = new Structs.Skill();
            private static Structs.Skill Sword = new Structs.Skill();
            private static Structs.Skill Axe = new Structs.Skill();
            private static Structs.Skill Distance = new Structs.Skill();
            private static Structs.Skill Shielding = new Structs.Skill();
            private static Structs.Skill Fishing = new Structs.Skill();

            internal static TimeSpan GetTotalRunningTime()
            {
                return stopWatch.Elapsed;
            }

            internal static string GetTotalRunningTimeString()
            {
                TimeSpan elapsed = stopWatch.Elapsed;
                return string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds);
            }

            internal static void Pause()
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }
            }

            internal static void Resume()
            {
                if (!stopWatch.IsRunning)
                {
                    stopWatch.Start();
                }
            }

            internal static void Start()
            {
                if (!stopWatch.IsRunning)
                {
                    Reset();
                    stopWatch.Start();
                }
            }

            internal static void Stop()
            {
                stopWatch.Stop();
                stopWatch.Reset();
            }

            internal static void Reset()
            {
                stopWatch.Reset();
                stopWatch.Start();
                Fist.PercentGained = 0;
                Axe.PercentGained = 0;
                Sword.PercentGained = 0;
                Club.PercentGained = 0;
                Distance.PercentGained = 0;
                Shielding.PercentGained = 0;
                Fishing.PercentGained = 0;
                gainedAxe.PercentOld = Memory.ReadInt(Addresses.Player.AxePercent);
                gainedAxe.SkillOld = Memory.ReadInt(Addresses.Player.Axe);
                gainedClub.PercentOld = Memory.ReadInt(Addresses.Player.ClubPercent);
                gainedClub.SkillOld = Memory.ReadInt(Addresses.Player.Club);
                gainedDistance.PercentOld = Memory.ReadInt(Addresses.Player.DistancePercent);
                gainedDistance.SkillOld = Memory.ReadInt(Addresses.Player.Distance);
                gainedFishing.PercentOld = Memory.ReadInt(Addresses.Player.FishingPercent);
                gainedFishing.SkillOld = Memory.ReadInt(Addresses.Player.Fishing);
                gainedFist.PercentOld = Memory.ReadInt(Addresses.Player.FistPercent);
                gainedFist.SkillOld = Memory.ReadInt(Addresses.Player.Fist);
                gainedMLVL.PercentOld = Memory.ReadInt(Addresses.Player.MagicLevelPercent);
                gainedMLVL.SkillOld = Memory.ReadInt(Addresses.Player.MagicLevel);
                gainedShielding.PercentOld = Memory.ReadInt(Addresses.Player.ShieldingPercent);
                gainedShielding.SkillOld = Memory.ReadInt(Addresses.Player.Shielding);
                gainedSword.PercentOld = Memory.ReadInt(Addresses.Player.SwordPercent);
                gainedSword.SkillOld = Memory.ReadInt(Addresses.Player.Sword);
            }

            internal static Structs.Skill GetSkillInfo(Enums.Skill Skill)
            {
                switch (Skill)
                {
                    case Enums.Skill.MagicLevel:
                        MLVL.Name = "MLVL";
                        MLVL.CurrentSkill = Memory.ReadUInt(Addresses.Player.MagicLevel);
                        MLVL.PercentLeft = 100 - Memory.ReadInt(Addresses.Player.MagicLevelPercent);
                        MLVL.PercentGained = GetGainedPercent("MLVL");
                        MLVL.PercentPerHour = GetPercentPerHour("MLVL");
                        MLVL.TimeLeft = GetTimeLeft("MLVL", MLVL.PercentGained, MLVL.PercentLeft);
                        return MLVL;
                    case Enums.Skill.Fist:
                        Fist.Name = "Fist";
                        Fist.CurrentSkill = Memory.ReadUInt(Addresses.Player.Fist);
                        Fist.PercentLeft = 100 - Memory.ReadInt(Addresses.Player.FistPercent);
                        Fist.PercentGained = GetGainedPercent("Fist");
                        Fist.PercentPerHour = GetPercentPerHour("Fist");
                        Fist.TimeLeft = GetTimeLeft("Fist", Fist.PercentGained, Fist.PercentLeft);
                        return Fist;
                    case Enums.Skill.Club:
                        Club.Name = "Club";
                        Club.CurrentSkill = Memory.ReadUInt(Addresses.Player.Club);
                        Club.PercentLeft = 100 - Memory.ReadInt(Addresses.Player.ClubPercent);
                        Club.PercentGained = GetGainedPercent("Club");
                        Club.PercentPerHour = GetPercentPerHour("Club");
                        Club.TimeLeft = GetTimeLeft("Club", Club.PercentGained, Club.PercentLeft);
                        return Club;
                    case Enums.Skill.Sword:
                        Sword.Name = "Sword";
                        Sword.CurrentSkill = Memory.ReadUInt(Addresses.Player.Sword);
                        Sword.PercentLeft = 100 - Memory.ReadInt(Addresses.Player.SwordPercent);
                        Sword.PercentGained = GetGainedPercent("Sword");
                        Sword.PercentPerHour = GetPercentPerHour("Sword");
                        Sword.TimeLeft = GetTimeLeft("Sword", Sword.PercentGained, Sword.PercentLeft);
                        return Sword;
                    case Enums.Skill.Axe:
                        Axe.Name = "Axe";
                        Axe.CurrentSkill = Memory.ReadUInt(Addresses.Player.Axe);
                        Axe.PercentLeft = 100 - Memory.ReadInt(Addresses.Player.AxePercent);
                        Axe.PercentGained = GetGainedPercent("Axe");
                        Axe.PercentPerHour = GetPercentPerHour("Axe");
                        Axe.TimeLeft = GetTimeLeft("Axe", Axe.PercentGained, Axe.PercentLeft);
                        return Axe;
                    case Enums.Skill.Distance:
                        Distance.Name = "Distance";
                        Distance.CurrentSkill = Memory.ReadUInt(Addresses.Player.Distance);
                        Distance.PercentLeft = 100 - Memory.ReadInt(Addresses.Player.DistancePercent);
                        Distance.PercentGained = GetGainedPercent("Distance");
                        Distance.PercentPerHour = GetPercentPerHour("Distance");
                        Distance.TimeLeft = GetTimeLeft("Distance", Distance.PercentGained, Distance.PercentLeft);
                        return Distance;
                    case Enums.Skill.Shielding:
                        Shielding.Name = "Shielding";
                        Shielding.CurrentSkill = Memory.ReadUInt(Addresses.Player.Shielding);
                        Shielding.PercentLeft = 100 - Memory.ReadInt(Addresses.Player.ShieldingPercent);
                        Shielding.PercentGained = GetGainedPercent("Shielding");
                        Shielding.PercentGained = GetGainedPercent("Shielding");
                        Shielding.PercentPerHour = GetPercentPerHour("Shielding");
                        Shielding.TimeLeft = GetTimeLeft("Shielding", Shielding.PercentGained, Shielding.PercentLeft);
                        return Shielding;
                    case Enums.Skill.Fishing:
                        Fishing.Name = "Fishing";
                        Fishing.CurrentSkill = Memory.ReadUInt(Addresses.Player.Fishing);
                        Fishing.PercentLeft = 100 - Memory.ReadInt(Addresses.Player.FishingPercent);
                        Fishing.PercentGained = GetGainedPercent("Fishing");
                        Fishing.PercentPerHour = GetPercentPerHour("Fishing");
                        Fishing.TimeLeft = GetTimeLeft("Fishing", Fishing.PercentGained, Fishing.PercentLeft);
                        return Fishing;
                    default:
                        Structs.Skill tempSkill = new Structs.Skill();
                        tempSkill.CurrentSkill = 0;
                        tempSkill.Name = "None";
                        tempSkill.PercentGained = 0;
                        tempSkill.PercentLeft = 100;
                        tempSkill.PercentPerHour = 0;
                        tempSkill.TimeLeft = "infinity";
                        return tempSkill;
                }
            }

            private static double GetPercentPerHour(string Skill)
            {
                uint _gainedPercent = GetGainedPercent(Skill);
                if (_gainedPercent > 0)
                {
                    double PercentPerHour = _gainedPercent / GetTotalRunningTime().TotalSeconds * 3600;
                    PercentPerHour = Math.Ceiling(PercentPerHour);
                    return PercentPerHour;
                }
                return 0;
            }

            private static string GetTimeLeft(string Skill, uint gainedPercent, int percentLeft)
            {
                if (gainedPercent > 0)
                {
                    TimeSpan timespanTimeLeft = new TimeSpan();
                    double TimeLeft = (percentLeft * 3600) / GetPercentPerHour(Skill);
                    timespanTimeLeft = TimeSpan.FromSeconds(TimeLeft);
                    return string.Format("{0:D2}:{1:D2}:{2:D2}", timespanTimeLeft.Hours, timespanTimeLeft.Minutes, timespanTimeLeft.Seconds);
                }
                return "infinity";
            }

            private static uint GetGainedPercent(string Skill)
            {
                int _gainedSkillsInPercent = 0, _gainedSkillPercent = 0;

                switch (Skill)
                {
                    case "MLVL":
                        gainedMLVL.SkillNew = Memory.ReadInt(Addresses.Player.MagicLevel);
                        gainedMLVL.PercentNew = Memory.ReadInt(Addresses.Player.MagicLevelPercent);
                        if (gainedMLVL.SkillNew - gainedMLVL.SkillOld > 1)
                        {
                            _gainedSkillsInPercent = (gainedMLVL.SkillNew - gainedMLVL.SkillOld - 1) * 100;
                            _gainedSkillPercent = 100 - gainedMLVL.PercentOld + gainedMLVL.PercentNew;
                        }
                        else if (gainedMLVL.SkillNew - gainedMLVL.SkillOld == 1)
                        {
                            _gainedSkillPercent = 100 - gainedMLVL.PercentOld + gainedMLVL.PercentNew;
                        }
                        else if (gainedMLVL.SkillNew - gainedMLVL.SkillOld == 0)
                        {
                            _gainedSkillPercent = gainedMLVL.PercentNew - gainedMLVL.PercentOld;
                        }
                        return (uint)(_gainedSkillsInPercent + _gainedSkillPercent);
                    case "Fist":
                        gainedFist.SkillNew = Memory.ReadInt(Addresses.Player.Fist);
                        gainedFist.PercentNew = Memory.ReadInt(Addresses.Player.FistPercent);
                        if (gainedFist.SkillNew - gainedFist.SkillOld > 1)
                        {
                            _gainedSkillsInPercent = (gainedFist.SkillNew - gainedFist.SkillOld - 1) * 100;
                            _gainedSkillPercent = 100 - gainedFist.PercentOld + gainedFist.PercentNew;
                        }
                        else if (gainedFist.SkillNew - gainedFist.SkillOld == 1)
                        {
                            _gainedSkillPercent = 100 - gainedFist.PercentOld + gainedFist.PercentNew;
                        }
                        else if (gainedFist.SkillNew - gainedFist.SkillOld == 0)
                        {
                            _gainedSkillPercent = gainedFist.PercentNew - gainedFist.PercentOld;
                        }
                        return (uint)(_gainedSkillsInPercent + _gainedSkillPercent);
                    case "Club":
                        gainedClub.SkillNew = Memory.ReadInt(Addresses.Player.Club);
                        gainedClub.PercentNew = Memory.ReadInt(Addresses.Player.ClubPercent);
                        if (gainedClub.SkillNew - gainedClub.SkillOld > 1)
                        {
                            _gainedSkillsInPercent = (gainedClub.SkillNew - gainedClub.SkillOld - 1) * 100;
                            _gainedSkillPercent = 100 - gainedClub.PercentOld + gainedClub.PercentNew;
                        }
                        else if (gainedClub.SkillNew - gainedClub.SkillOld == 1)
                        {
                            _gainedSkillPercent = 100 - gainedClub.PercentOld + gainedClub.PercentNew;
                        }
                        else if (gainedClub.SkillNew - gainedClub.SkillOld == 0)
                        {
                            _gainedSkillPercent = gainedClub.PercentNew - gainedClub.PercentOld;
                        }
                        return (uint)(_gainedSkillsInPercent + _gainedSkillPercent);
                    case "Sword":
                        gainedSword.SkillNew = Memory.ReadInt(Addresses.Player.Sword);
                        gainedSword.PercentNew = Memory.ReadInt(Addresses.Player.SwordPercent);
                        if (gainedSword.SkillNew - gainedSword.SkillOld > 1)
                        {
                            _gainedSkillsInPercent = (gainedSword.SkillNew - gainedSword.SkillOld - 1) * 100;
                            _gainedSkillPercent = 100 - gainedSword.PercentOld + gainedSword.PercentNew;
                        }
                        else if (gainedSword.SkillNew - gainedSword.SkillOld == 1)
                        {
                            _gainedSkillPercent = 100 - gainedSword.PercentOld + gainedSword.PercentNew;
                        }
                        else if (gainedSword.SkillNew - gainedSword.SkillOld == 0)
                        {
                            _gainedSkillPercent = gainedSword.PercentNew - gainedSword.PercentOld;
                        }
                        return (uint)(_gainedSkillsInPercent + _gainedSkillPercent);
                    case "Axe":
                        gainedAxe.SkillNew = Memory.ReadInt(Addresses.Player.Axe);
                        gainedAxe.PercentNew = Memory.ReadInt(Addresses.Player.AxePercent);
                        if (gainedAxe.SkillNew - gainedAxe.SkillOld > 1)
                        {
                            _gainedSkillsInPercent = (gainedAxe.SkillNew - gainedAxe.SkillOld - 1) * 100;
                            _gainedSkillPercent = 100 - gainedAxe.PercentOld + gainedAxe.PercentNew;
                        }
                        else if (gainedAxe.SkillNew - gainedAxe.SkillOld == 1)
                        {
                            _gainedSkillPercent = 100 - gainedAxe.PercentOld + gainedAxe.PercentNew;
                        }
                        else if (gainedAxe.SkillNew - gainedAxe.SkillOld == 0)
                        {
                            _gainedSkillPercent = gainedAxe.PercentNew - gainedAxe.PercentOld;
                        }
                        return (uint)(_gainedSkillsInPercent + _gainedSkillPercent);
                    case "Distance":
                        gainedDistance.SkillNew = Memory.ReadInt(Addresses.Player.Distance);
                        gainedDistance.PercentNew = Memory.ReadInt(Addresses.Player.DistancePercent);
                        if (gainedDistance.SkillNew - gainedDistance.SkillOld > 1)
                        {
                            _gainedSkillsInPercent = (gainedDistance.SkillNew - gainedDistance.SkillOld - 1) * 100;
                            _gainedSkillPercent = 100 - gainedDistance.PercentOld + gainedDistance.PercentNew;
                        }
                        else if (gainedDistance.SkillNew - gainedDistance.SkillOld == 1)
                        {
                            _gainedSkillPercent = 100 - gainedDistance.PercentOld + gainedDistance.PercentNew;
                        }
                        else if (gainedDistance.SkillNew - gainedDistance.SkillOld == 0)
                        {
                            _gainedSkillPercent = gainedDistance.PercentNew - gainedDistance.PercentOld;
                        }
                        return (uint)(_gainedSkillsInPercent + _gainedSkillPercent);
                    case "Shielding":
                        gainedShielding.SkillNew = Memory.ReadInt(Addresses.Player.Shielding);
                        gainedShielding.PercentNew = Memory.ReadInt(Addresses.Player.ShieldingPercent);
                        if (gainedShielding.SkillNew - gainedShielding.SkillOld > 1)
                        {
                            _gainedSkillsInPercent = (gainedShielding.SkillNew - gainedShielding.SkillOld - 1) * 100;
                            _gainedSkillPercent = 100 - gainedShielding.PercentOld + gainedShielding.PercentNew;
                        }
                        else if (gainedShielding.SkillNew - gainedShielding.SkillOld == 1)
                        {
                            _gainedSkillPercent = 100 - gainedShielding.PercentOld + gainedShielding.PercentNew;
                        }
                        else if (gainedShielding.SkillNew - gainedShielding.SkillOld == 0)
                        {
                            _gainedSkillPercent = gainedShielding.PercentNew - gainedShielding.PercentOld;
                        }
                        return (uint)(_gainedSkillsInPercent + _gainedSkillPercent);
                    case "Fishing":
                        gainedFishing.SkillNew = Memory.ReadInt(Addresses.Player.Fishing);
                        gainedFishing.PercentNew = Memory.ReadInt(Addresses.Player.FishingPercent);
                        if (gainedFishing.SkillNew - gainedFishing.SkillOld > 1)
                        {
                            _gainedSkillsInPercent = (gainedFishing.SkillNew - gainedFishing.SkillOld - 1) * 100;
                            _gainedSkillPercent = 100 - gainedFishing.PercentOld + gainedFishing.PercentNew;
                        }
                        else if (gainedFishing.SkillNew - gainedFishing.SkillOld == 1)
                        {
                            _gainedSkillPercent = 100 - gainedFishing.PercentOld + gainedFishing.PercentNew;
                        }
                        else if (gainedFishing.SkillNew - gainedFishing.SkillOld == 0)
                        {
                            _gainedSkillPercent = gainedFishing.PercentNew - gainedFishing.PercentOld;
                        }
                        return (uint)(_gainedSkillsInPercent + _gainedSkillPercent);
                    default:
                        return 0;
                }
            }
            #endregion
        }

        internal class XTea
        {
            internal unsafe static bool Encrypt(ref byte[] buffer, ref int length, int index, uint[] key)
            {
                if (key == null)
                    return false;

                int msgSize = length - index;

                int pad = msgSize % 8;
                if (pad > 0)
                {
                    msgSize += (8 - pad);
                    length = index + msgSize;
                }

                fixed (byte* bufferPtr = buffer)
                {
                    uint* words = (uint*)(bufferPtr + index);

                    for (int pos = 0; pos < msgSize / 4; pos += 2)
                    {
                        uint x_sum = 0, x_delta = 0x9e3779b9, x_count = 32;

                        while (x_count-- > 0)
                        {
                            words[pos] += (words[pos + 1] << 4 ^ words[pos + 1] >> 5) + words[pos + 1] ^ x_sum
                                + key[x_sum & 3];
                            x_sum += x_delta;
                            words[pos + 1] += (words[pos] << 4 ^ words[pos] >> 5) + words[pos] ^ x_sum
                                + key[x_sum >> 11 & 3];
                        }
                    }
                }

                return true;
            }

            internal unsafe static bool Decrypt(ref byte[] buffer, ref int length, int index, uint[] key)
            {
                if (length <= index || (length - index) % 8 > 0 || key == null)
                    return false;

                fixed (byte* bufferPtr = buffer)
                {
                    uint* words = (uint*)(bufferPtr + index);
                    int msgSize = length - index;

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

                length = (int)(BitConverter.ToUInt16(buffer, index) + 2 + index);

                /*byte[] temp = buffer;
                buffer = new byte[length];
                Array.Copy(BitConverter.GetBytes((ushort)(length - 2)), buffer, 2); // length of packet
                Array.Copy(temp, 4, buffer, 2, length - 2);*/
                return true;
            }

            internal static void PrepareForEncryption(ref byte[] Packet)
            {
                List<byte> listPacket = Packet.ToList<byte>();
                Random random = new Random();
                while ((listPacket.Count) % 8 > 0)
                {
                    listPacket.Add((byte)random.Next(0x00, 0xFF));
                }
                Packet = listPacket.ToArray();
            }

            internal static void AddSizeOfEncryptedData(ref byte[] Packet, ushort PacketLength)
            {
                List<byte> listPacket = new List<byte>();
                byte[] encryptedLength = BitConverter.GetBytes((ushort)Packet.Length);
                listPacket.Add(encryptedLength[0]); listPacket.Add(encryptedLength[1]);
                foreach (byte data in Packet)
                {
                    listPacket.Add(data);
                }
                Packet = listPacket.ToArray();
            }
        }

        internal static bool FilterPackage(ref byte[] Package, List<Addresses.Enums.IncomingPacketTypes> packetFilter)
        {
            try
            {
                if (Package.Length > BitConverter.ToUInt16(Package, 0) + 2)
                {
                    int pos = 0;
                    List<byte> newPacket = new List<byte>();
                    newPacket.AddRange(Package);
                    while (Package.Length > pos)
                    {
                        ushort packetLength = BitConverter.ToUInt16(Package, pos);
                        if (packetFilter.Contains((Addresses.Enums.IncomingPacketTypes)Package[pos + 2]))
                        {
                            newPacket.RemoveRange(pos, packetLength + 2);
                            //MessageBox.Show(newPacket.Count.ToString());
                        }
                        pos += packetLength + 2;
                    }
                    Package = newPacket.ToArray();
                }
                else
                {
                    if (packetFilter.Contains((Addresses.Enums.IncomingPacketTypes)Package[2]))
                    {
                        Package = new byte[0];
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionHandler(ex);
            }
            return false;
        }

        /// <summary>
        /// Reads the outgoing loginserver packet to get the XTEA key (Tibia 7.7+)
        /// </summary>
        /// <param name="LoginServerRequestPacket"></param>
        /// <param name="XTEAKey"></param>
        /// <returns></returns>
        internal static bool GetXTEAKey(byte[] LoginServerRequestPacket, ref uint[] XTEAKey)
        {
            try
            {
                XTEAKey[0] = BitConverter.ToUInt32(LoginServerRequestPacket, 19);
                XTEAKey[1] = BitConverter.ToUInt32(LoginServerRequestPacket, 23);
                XTEAKey[2] = BitConverter.ToUInt32(LoginServerRequestPacket, 27);
                XTEAKey[3] = BitConverter.ToUInt32(LoginServerRequestPacket, 31);
                return true;
            }
            catch (Exception ex)
            {
                Utils.ExceptionHandler(ex);
            }
            return false;
        }

        /// <summary>
        /// Reads memory to get the XTEA key (Tibia 7.7+)
        /// </summary>
        /// <returns></returns>
        internal static uint[] GetXTEAKey()
        {
            uint[] xteaKey = new uint[4];
            xteaKey[0] = Memory.ReadUInt(Addresses.Client.XTEAKey);
            xteaKey[1] = Memory.ReadUInt(Addresses.Client.XTEAKey + 4);
            xteaKey[2] = Memory.ReadUInt(Addresses.Client.XTEAKey + 8);
            xteaKey[3] = Memory.ReadUInt(Addresses.Client.XTEAKey + 12);
            return xteaKey;
        }

        internal static void ExceptionHandler(Exception ex)
        {
            FileWrite("Error.txt", "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] v" +
                Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion + ": " +
                ex.Message + "\t" + ex.StackTrace, true);
        }

        private static int MakeLParam(int LoWord, int HiWord)
        {
            return ((HiWord << 16) | (LoWord & 0xffff));
        }

        internal static Process[] GetProcessesFromClassName(string className)
        {
            StringBuilder strBuilder = new StringBuilder(100);
            int classLength = 0;
            List<Process> tibiaList = new List<Process>();
            Process[] processlist = Process.GetProcesses();
            foreach (Process proc in processlist)
            {
                try
                {
                    classLength = WinApi.GetClassName(proc.MainWindowHandle, strBuilder, 100);
                    if (strBuilder.ToString() == className)
                    {
                        strBuilder.Remove(0, strBuilder.Length);
                        tibiaList.Add(proc);
                    }
                }
                catch { continue; }
            }
            return tibiaList.ToArray();
        }

        internal static Process StartTibia(string tibiaPath)
        {
            if (File.Exists(tibiaPath))
            {
                ProcessStartInfo TibiaStartInfo = new ProcessStartInfo();
                TibiaStartInfo.FileName = tibiaPath;
                Directory.SetCurrentDirectory(tibiaPath.Substring(0, tibiaPath.LastIndexOf('\\') + 1));
                Process Tibia = Process.Start(TibiaStartInfo);
                while (Tibia.MainWindowHandle == IntPtr.Zero && !Tibia.HasExited)
                {
                    Tibia.Refresh();
                    System.Threading.Thread.Sleep(200);
                }
                Directory.SetCurrentDirectory(Application.StartupPath + "\\");
                return Tibia;
            }
            return null;
        }

        internal class Pinger
        {
            internal static long LastRoundtripTime = 0;
            private static bool WaitingForReply = false;

            internal static void Ping(string IP)
            {
                if (!WaitingForReply)
                {
                    Ping pinger = new Ping();
                    pinger.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
                    byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                    pinger.SendAsync(IPAddress.Parse(IP), 10000, buffer, null);
                }
                WaitingForReply = true;
            }

            private static void PingCompletedCallback(object sender, PingCompletedEventArgs e)
            {
                WaitingForReply = false;
                if (e.Reply != null && e.Reply.Status == IPStatus.Success)
                {
                    LastRoundtripTime = e.Reply.RoundtripTime;
                }
                else { LastRoundtripTime = 0; }
            }

        }
    }
}
