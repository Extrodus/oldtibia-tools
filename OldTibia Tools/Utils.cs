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
using Microsoft.Win32;

namespace TibianicTools
{
    public class Utils
    {
        internal class WindowsRegistry
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="extension">The file extension (i.e. '.txt').</param>
            /// <param name="keyName"></param>
            /// <param name="execPath">The file path to the executable.</param>
            /// <param name="fileDescription">The description of the file extension.</param>
            internal static void SetAssociation(string extension, string keyName, string execPath, string fileDescription)
            {
                RegistryKey BaseKey;
                RegistryKey OpenMethod;
                RegistryKey Shell;
                RegistryKey CurrentUser;

                BaseKey = Registry.CurrentUser.CreateSubKey(extension);
                BaseKey.SetValue("", keyName);

                OpenMethod = Registry.CurrentUser.CreateSubKey(keyName);
                OpenMethod.SetValue("", fileDescription);
                OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + execPath + "\",0");
                Shell = OpenMethod.CreateSubKey("Shell");
                Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + execPath + "\"" + " \"%1\"");
                Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + execPath + "\"" + " \"%1\"");
                BaseKey.Close();
                OpenMethod.Close();
                Shell.Close();

                // Delete the key instead of trying to change it
                CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + extension, true);
                CurrentUser.DeleteSubKey("UserChoice", false);
                CurrentUser.Close();

                // Tell explorer the file association has been changed
                WinApi.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }

        }

        /// <summary>
        /// All bitmaps are outlined with a black 1px pen.
        /// </summary>
        internal class Bitmaps
        {
            internal static Bitmap GetBall(Color c, int size)
            {
                Bitmap b = new Bitmap(size, size);
                Graphics g = Graphics.FromImage(b);
                g.FillEllipse(new SolidBrush(c), new Rectangle(0, 0, size - 1, size - 1));
                g.DrawEllipse(new Pen(Color.Black), new Rectangle(0, 0, size - 1, size - 1));
                b.MakeTransparent(SystemColors.Control);
                return b;
            }

            internal static Bitmap GetRectangle(Color c, int x, int y)
            {
                Bitmap b = new Bitmap(x, y);
                Graphics g = Graphics.FromImage(b);
                g.FillRectangle(new SolidBrush(c), new Rectangle(0, 0, x - 1, y - 1));
                g.DrawRectangle(new Pen(Color.Black), new Rectangle(0, 0, x - 1, y - 1));
                b.MakeTransparent(SystemColors.Control);
                return b;
            }

            internal static Bitmap GetRectangles(Color c, Rectangle[] rectangles)
            {
                int x = 0, y = 0;
                foreach (Rectangle r in rectangles)
                {
                    if (r.Right > x) { x = r.Right; }
                    if (r.Bottom > y) { y = r.Bottom; }
                }
                x++; y++;
                Bitmap b = new Bitmap(x, y);
                Graphics g = Graphics.FromImage(b);
                g.FillRectangles(new SolidBrush(c), rectangles);
                g.DrawRectangles(new Pen(Color.Black), rectangles);
                b.MakeTransparent(SystemColors.Control);
                return b;
            }

            internal static Bitmap GetPolygon(Color c, int size, Point[] points, Color outline)
            {
                Bitmap b = new Bitmap(size, size);
                Graphics g = Graphics.FromImage(b);
                g.FillPolygon(new SolidBrush(c), points);
                g.DrawPolygon(new Pen(outline), points);
                b.MakeTransparent(SystemColors.Control);
                return b;
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

        internal static void Screenshot(string fileName, ImageFormat imgFormat, bool activeWindowOnly)
        {
            try
            {
                if (fileName.Length > 0 && imgFormat != null)
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
                    bitmap.Save(fileName + DateTime.Now.ToString("-yyyy-MM-dd~HH-mm-ss") + "." + imgFormat.ToString().ToLower(), imgFormat);
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
                catch { }
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
            catch { return false; }
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

        internal class TibiaCam
        {
            /// <summary>
            /// Wrappers for reading TibiaMovie files (*.tmv)
            /// Credits: http://cboard.cprogramming.com/csharp-programming/126794-my-zlib1-dll-wrapper-no-does-not-work-net-4-0-a.html
            /// </summary>
            internal class Zlib
            {
                /* METADATA:
                 * 2 bytes tibiamovie version
                 * 2 bytes tibia version
                 * 4 bytes movie duration (milliseconds)
                 * PACKET DATA:
                 * 1 byte EOF
                 * 4 bytes amount of time to sleep (milliseconds)
                 * 2 bytes packet length
                 * x bytes packet
                 */
                [DllImport("zlib1.dll", CallingConvention = CallingConvention.Cdecl)]
                private static extern int compress(byte[] destBuffer, ref uint destLen, byte[] sourceBuffer, uint sourceLen);
                [DllImport("zlib1.dll", CallingConvention = CallingConvention.Cdecl)]
                private static extern int uncompress(byte[] destBuffer, ref uint destLen, byte[] sourceBuffer, uint sourceLen);

                internal static byte[] Compress(byte[] data)
                {
                    uint _dLen = (uint)((data.Length * 1.1) + 12);
                    byte[] _d = new byte[_dLen];
                    compress(_d, ref _dLen, data, (uint)data.Length);
                    byte[] result = new byte[_dLen];
                    Array.Copy(_d, 0, result, 0, result.Length);
                    return result;
                }

                internal static byte[] Decompress(byte[] data)
                {
                    uint _dLen = 1024*1024*5;
                    byte[] _d = new byte[_dLen];

                    int res = uncompress(_d, ref _dLen, data, (uint)data.Length);
                    //if (res != 0) { return null; }
                    MessageBox.Show(_dLen.ToString() + "\n" + res.ToString());
                    byte[] result = new byte[_dLen];
                    Array.Copy(_d, 0, result, 0, result.Length);
                    return result;
                }

                internal static List<Objects.Packet> BytesToPacket(byte[] data)
                {
                    Objects.Packet file = new Objects.Packet(data);
                    int packetlen = file.ToBytes().Length;
                    List<Objects.Packet> packets = new List<Objects.Packet>();
                    //packets.Add(new Packet(BitConverter.GetBytes(file.GetUInt16()))); // tibiamovie version
                    //packets.Add(new Packet(BitConverter.GetBytes(file.GetUInt16()))); // tibia version
                    //packets.Add(new Packet(BitConverter.GetBytes(file.GetUInt32()))); // duration in ms
                    while (packetlen < file.GetPosition)
                    {
                        byte eof = file.GetByte();
                        if (eof == 0xFF) { break; }
                        List<byte> listbytes = new List<byte>();
                        listbytes.AddRange(file.GetBytes(4));
                        ushort len = file.GetUInt16();
                        byte[] packet = file.GetBytes(len);
                        listbytes.AddRange(BitConverter.GetBytes(len));
                        listbytes.AddRange(packet);
                        packets.Add(new Objects.Packet(listbytes.ToArray()));
                    }
                    return packets;
                }
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
            /// Compresses a *.kcam file. Version 1.42 and higher.
            /// </summary>
            /// <param name="Packets"></param>
            /// <param name="FileName"></param>
            internal static void CompressCam(List<Objects.Packet> packets, string fileName)
            {
                MemoryStream stream = new MemoryStream();
                byte[] buffer = new byte[4096];
                foreach (Objects.Packet p in packets)
                {
                    stream.Write(p.ToBytes(), 0, p.ToBytes().Length);
                }
                stream.Position = 0;
                if (!fileName.EndsWith(".kcam"))
                {
                    fileName += ".kcam";
                }
                // Create the compressed file.
                using (FileStream outFile =
                        File.Create(fileName))
                {
                    using (DeflateStream Compress =
                        new DeflateStream(outFile,
                        CompressionMode.Compress))
                    {
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
            /// Compresses any list of packets without filename checks.
            /// </summary>
            /// <param name="packets"></param>
            /// <param name="fileName"></param>
            internal static void Compress(List<Objects.Packet> packets, string fileName)
            {
                MemoryStream stream = new MemoryStream();
                byte[] buffer = new byte[4096];
                foreach (Objects.Packet p in packets)
                {
                    stream.Write(p.ToBytes(), 0, p.ToBytes().Length);
                }
                stream.Position = 0;
                // Create the compressed file.
                using (FileStream outFile =
                        File.Create(fileName))
                {
                    using (DeflateStream Compress =
                        new DeflateStream(outFile,
                        CompressionMode.Compress))
                    {
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
                        using (DeflateStream Decompress = new DeflateStream(inFile, CompressionMode.Decompress))
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
                        if (currentLine == null) { break; }
                        DecompressedList.Add(currentLine);
                    }
                    streamReader.Close();
                    streamReader.Dispose();
                    return DecompressedList;
                }
                catch { return new List<string>(); }
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
                    binReader.Close();
                    return DecompressedList;
                }
                catch (Exception ex) { }
                return new List<byte[]>();
            }

            /// <summary>
            /// Decompresses a *.kcam file. Version 1.42 and higher.
            /// </summary>
            /// <param name="FilePath"></param>
            /// <returns></returns>
            internal static List<Objects.Packet> DecompressCamToPackets(string filePath)
            {
                try
                {
                    MemoryStream uncompressedStream = new MemoryStream();
                    FileInfo fi = new FileInfo(filePath);
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
                    List<Objects.Packet> DecompressedList = new List<Objects.Packet>();
                    BinaryReader binReader = new BinaryReader(uncompressedStream);
                    byte[] tempBuffer = binReader.ReadBytes(8); // 2 bytes TibiaVersion, 2 bytes CamVersion, 4 bytes RunningLength(ms)
                    DecompressedList.Add(new Objects.Packet(new byte[] { tempBuffer[0], tempBuffer[1] }));
                    DecompressedList.Add(new Objects.Packet(new byte[] { tempBuffer[2], tempBuffer[3] }));
                    DecompressedList.Add(new Objects.Packet(new byte[] { tempBuffer[4], tempBuffer[5], tempBuffer[6], tempBuffer[7] }));
                    while (binReader.BaseStream.Length > binReader.BaseStream.Position)
                    {
                        Objects.Packet p = new Objects.Packet();
                        p.AddUInt32(binReader.ReadUInt32()); // delay
                        uint len = binReader.ReadUInt32();
                        p.AddUInt32(len);
                        p.AddBytes(binReader.ReadBytes((int)len)); // package
                        DecompressedList.Add(p);
                    }
                    binReader.Close();
                    return DecompressedList;
                }
                catch (Exception ex) { }
                return new List<Objects.Packet>();
            }

            internal static Stream DecompressCamToStream(string filePath)
            {
                MemoryStream uncompressedStream = new MemoryStream();
                FileInfo fi = new FileInfo(filePath);
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
                return uncompressedStream;
            }
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
            private static Stopwatch stopWatch = new Stopwatch();
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

            /// <summary>
            /// Sets the experience table to Tibianic's values.
            /// </summary>
            internal static void SetExpTable()
            {
                ExperienceTable[70] = 5246300;
                ExperienceTable[71] = 5481000;
                ExperienceTable[72] = 5727600;
                ExperienceTable[73] = 5991200;
                ExperienceTable[74] = 6271900;
                ExperienceTable[75] = 6569800;
                ExperienceTable[76] = 6885000;
                ExperienceTable[77] = 7217600;
                ExperienceTable[78] = 7567700;
                ExperienceTable[79] = 7935400;
                ExperienceTable[80] = 8320800;
                ExperienceTable[81] = 8724000;
                ExperienceTable[82] = 9145100;
                ExperienceTable[83] = 9584200;
                ExperienceTable[84] = 10041400;
                ExperienceTable[85] = 10516800;
                ExperienceTable[86] = 11010500;
                ExperienceTable[87] = 11522600;
                ExperienceTable[88] = 12053200;
                ExperienceTable[89] = 12602400;
                ExperienceTable[90] = 13170300;
                ExperienceTable[91] = 13757000;
                ExperienceTable[92] = 14362600;
                ExperienceTable[93] = 14987200;
                ExperienceTable[94] = 15630900;
                ExperienceTable[95] = 16293800;
                ExperienceTable[96] = 16976000;
                ExperienceTable[97] = 17677600;
                ExperienceTable[98] = 18398700;
                ExperienceTable[99] = 19139400;
                ExperienceTable[100] = 19899800;
                ExperienceTable[101] = 20680000;
                ExperienceTable[102] = 21480100;
                ExperienceTable[103] = 22300200;
                ExperienceTable[104] = 23140400;
                ExperienceTable[105] = 24000800;
                ExperienceTable[106] = 24881500;
                ExperienceTable[107] = 25782600;
                ExperienceTable[108] = 26704200;
                ExperienceTable[109] = 27646400;
                ExperienceTable[110] = 28609300;
            }

            internal static long GetExperienceTNL()
            {
                uint level = Client.Player.Level;
                uint exp = Client.Player.Experience;
                switch (Settings.Counters.Experience.ExpTNLSource)
                {
                    case Settings.Counters.Experience.ExpTNLSources.ExpTable:
                        if (level + 1 > ExperienceTable.Length) return 0;
                        return ExperienceTable[level + 1] - exp;
                    case Settings.Counters.Experience.ExpTNLSources.Formula:
                        return (50 * (level + 1) * (level + 1) * (level + 1) - 150 * (level + 1) * (level + 1) + 400 * (level + 1)) / 3 - (long)exp;
                    case Settings.Counters.Experience.ExpTNLSources.LevelPercent:
                        double expgained = GetGainedExperience();
                        double percentgained = GetGainedLevelPercent();
                        if (expgained <= 0 || percentgained <= 0) return 0;
                        double estimatedexpperpercent = expgained / percentgained;
                        return (long)(estimatedexpperpercent * GetLevelPercentTNL());
                }
                return 0;
            }

            internal static int GetLevelPercentTNL()
            {
                return 100 - Memory.ReadInt(Addresses.Player.LevelPercent);
            }

            internal static void Start()
            {
                SetExpTable();
                Reset();
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
                    switch (Settings.Counters.Experience.ExpTNLSource)
                    {
                        case Settings.Counters.Experience.ExpTNLSources.ExpTable:
                        case Settings.Counters.Experience.ExpTNLSources.Formula:
                            double TimeLeft = (GetExperienceTNL() * 3600) / ExpPerHour;
                            timespanTimeLeft = TimeSpan.FromSeconds(TimeLeft);
                            return string.Format("{0:D2}:{1:D2}:{2:D2}", timespanTimeLeft.Hours, timespanTimeLeft.Minutes, timespanTimeLeft.Seconds);
                        case Settings.Counters.Experience.ExpTNLSources.LevelPercent:
                            LevelPercentNew = 100 - Memory.ReadInt(Addresses.Player.LevelPercent);
                            double percentPerHour = GetLevelPercentPerHour();
                            if (percentPerHour == 0) break;
                            double TimeLeftBasedOnLevelPercent = (LevelPercentNew * 3600) / percentPerHour;
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
                ExperienceNew = Client.Player.Experience;
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
                LevelNew = (int)Client.Player.Level;
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
