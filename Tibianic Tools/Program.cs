using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace TibianicTools
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Application.StartupPath + "\\");
            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string TibiaPath = "";
            string[] arguments = Environment.GetCommandLineArgs();
            //arguments = new string[] { @"D:\Bots och programmering\Tibianic Tools\bin\Release\Tibianic-Suchy.kcam" };
            try
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i].ToLower() == "record" &&
                        i < arguments.Length)
                    {
                        Proxy.AutoRecordIP = arguments[i + 1];
                        Proxy.AutoRecordPort = int.Parse(arguments[i + 2]);
                        Proxy.doAutoRecord = true;
                        break;
                    }
                    else if (arguments[i].ToLower().EndsWith(".kcam"))
                    {
                        string camFile = arguments[i];
                        if (File.Exists(camFile))
                        {
                            Proxy.doAutoPlayback = true;
                            Proxy.AutoPlayBackName = camFile;
                            break;
                        }
                    }
                    else if (arguments[i].ToLower() == "starttibia" &&
                             i < arguments.Length - 1)
                    {
                        TibiaPath = arguments[i + 1];
                        if (!File.Exists(TibiaPath))
                        {
                            TibiaPath = "";
                        }
                        break;
                    }
                }
            }
            catch { Proxy.AutoPlayBackName = ""; Proxy.doAutoRecord = false; Proxy.doAutoPlayback = false; MessageBox.Show("Parameters were not filled in correctly."); }

            if (Proxy.doAutoPlayback || Proxy.doAutoRecord || TibiaPath.Length > 1)
            {
                string[] configs = Utils.FileRead(@"settings.ini");
                foreach (string line in configs)
                {
                    string[] split = line.Split('=');
                    string joinedstring = string.Join("=", split, 1, split.Length - 1);
                    if (split[0] == "ClientPath")
                    {
                        TibiaPath = joinedstring;
                        break;
                    }
                }
                if (!File.Exists(TibiaPath))
                {
                    OpenFileDialog openFile = new OpenFileDialog();
                    openFile.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
                    openFile.Title = "Please find your Tibia client";
                    if (openFile.ShowDialog() == DialogResult.OK)
                    {
                        TibiaPath = openFile.FileName;
                    }
                }
                Process Tibia = null;
                try
                {
                    Tibia = Utils.StartTibia(TibiaPath);
                }
                catch { }
                if (Tibia != null &&
                    Addresses.SetAddresses(Tibia.MainModule.FileVersionInfo.FileVersion))
                {
                    Client.Tibia = Tibia;
                    Client.TibiaHandle = Tibia.Handle;
                    Application.Run(new UI());
                }
                else
                {
                    Proxy.doAutoRecord = false;
                    Proxy.doAutoPlayback = false;
                    Application.Run(new ClientChooser());
                }
            }
            else
            {
                Process[] tibiaList = Utils.GetProcessesFromClassName("TibiaClient");
                if (tibiaList.Length == 1)
                {
                    if (Addresses.SetAddresses(tibiaList[0].MainModule.FileVersionInfo.FileVersion))
                    {
                        Client.Tibia = tibiaList[0];
                        Client.TibiaHandle = Client.Tibia.Handle;
                        Application.Run(new UI());
                    }
                    else
                    {
                        Application.Run(new ClientChooser());
                    }
                }
                else
                {
                    Application.Run(new ClientChooser());
                }
            }
        }
    }
}
