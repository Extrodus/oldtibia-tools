using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace TibianicTools
{
    class Settings
    {
        internal static ushort CurrentVersion = 156;
        internal static void LoadSettings(string fileName)
        {
            if (!File.Exists(fileName)) return;
            string[] split = File.ReadAllLines(fileName);
            foreach (string line in split)
            {
                if (string.IsNullOrEmpty(line)) break;
                try
                {
                    string[] linesplit = line.Split('=');
                    string val = string.Join("=", linesplit, 1, linesplit.Length - 1);
                    switch (linesplit[0])
                    {
                        case "AnimateForm":
                            break;
                        case "RedrawForm":
                            break;
                        case "ExperienceCounterOutputIndex":
                            Settings.Counters.Experience.Output = (Counters.Experience.OutputInfo)byte.Parse(val);
                            break;
                        case "ExperienceCounterCalculateIndex":
                            Settings.Counters.Experience.ExpTNLSource = (Counters.Experience.ExpTNLSources)byte.Parse(val);
                            break;
                        case "ScreenshooterFileFormatIndex":
                            Settings.Screenshooter.ImageFormat = byte.Parse(val) == 1 ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case "ScreenshooterFilePath":
                            Settings.Screenshooter.FilePath = val;
                            break;
                        case "ScreenshooterCaptureActiveWindow":
                            Settings.Screenshooter.ActiveWindowOnly = bool.Parse(val);
                            break;
                        case "ScreenshooterAutoSave":
                            Settings.Screenshooter.doAutoScreenshot = bool.Parse(val);
                            break;
                        case "ScreenshooterAutoSaveLevel":
                            Settings.Screenshooter.doOnLevelAdvance = bool.Parse(val);
                            break;
                        case "ScreenshooterAutoSaveSkill":
                            Settings.Screenshooter.doOnSkillAdvance = bool.Parse(val);
                            break;
                        case "TibiaCamLastUsedIP":
                            Settings.Tibiacam.Recorder.ServerIP = val;
                            break;
                        case "TibiaCamLastUsedPort":
                            Settings.Tibiacam.Recorder.ServerPort = ushort.Parse(val);
                            break;
                        case "TibiaCamReadMetaData":
                            Settings.Tibiacam.Playback.ReadMetaData = bool.Parse(val);
                            break;
                        case "TibiaCamPlayMouse":
                            Settings.Tibiacam.Playback.doPlayMouse = bool.Parse(val);
                            break;
                        case "TibiaCamRecordMouse":
                            Settings.Tibiacam.Recorder.doRecordMouse = bool.Parse(val);
                            break;
                        case "TibiaCamMouseInterval":
                            Settings.Tibiacam.Recorder.MouseInterval = ushort.Parse(val);
                            break;
                        case "TibiaCamFiltersOutgoingPMs":
                            Settings.Tibiacam.Recorder.Filters.OutgoingPrivateMessages = bool.Parse(val);
                            break;
                        case "ClientPath":
                            Settings.General.TibiaPath = val;
                            break;
                        case "CheckForUpdates":
                            Settings.General.CheckForUpdates = bool.Parse(val);
                            break;
                    }
                }
                catch (Exception ex) { /*System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);*/ }
            }
        }
        internal static void SaveSettings(string fileName)
        {
            if (File.Exists(fileName)) File.Delete(fileName);
            List<string> configs = new List<string>();
            configs.Add("AnimateForm=" + false);
            configs.Add("RedrawForm=" + false);
            configs.Add("ExperienceCounterOutputIndex=" + (byte)Settings.Counters.Experience.Output);
            configs.Add("ExperienceCounterCalculateIndex=" + (byte)Settings.Counters.Experience.ExpTNLSource);
            configs.Add("ScreenshooterFileFormatIndex=" + (Settings.Screenshooter.ImageFormat == System.Drawing.Imaging.ImageFormat.Jpeg ? 0 : 1));
            configs.Add("ScreenshooterFilePath=" + Settings.Screenshooter.FilePath);
            configs.Add("ScreenshooterCaptureActiveWindow=" + Settings.Screenshooter.ActiveWindowOnly);
            configs.Add("ScreenshooterAutoSave=" + Settings.Screenshooter.doAutoScreenshot);
            configs.Add("ScreenshooterAutoSaveLevel=" + Settings.Screenshooter.doOnLevelAdvance);
            configs.Add("ScreenshooterAutoSaveSkill=" + Settings.Screenshooter.doOnSkillAdvance);
            configs.Add("TibiaCamLastUsedIP=" + Settings.Tibiacam.Recorder.ServerIP);
            configs.Add("TibiaCamLastUsedPort=" + Settings.Tibiacam.Recorder.ServerPort);
            configs.Add("TibiaCamReadMetaData=" + Settings.Tibiacam.Playback.ReadMetaData);
            configs.Add("TibiaCamPlayMouse=" + Settings.Tibiacam.Playback.doPlayMouse);
            configs.Add("TibiaCamRecordMouse=" + Settings.Tibiacam.Recorder.doRecordMouse);
            configs.Add("TibiaCamMouseInterval=" + Settings.Tibiacam.Recorder.MouseInterval);
            configs.Add("TibiaCamFiltersOutgoingPMs=" + Settings.Tibiacam.Recorder.Filters.OutgoingPrivateMessages);
            configs.Add("ClientPath=" + Settings.General.TibiaPath);
            configs.Add("CheckForUpdates=" + Settings.General.CheckForUpdates);
            File.WriteAllLines(fileName, configs.ToArray());
        }
        internal static void LoadDefaults()
        {
            Settings.Counters.Experience.ExpTNLSource = Counters.Experience.ExpTNLSources.Formula;
            Settings.Counters.Experience.Output = Counters.Experience.OutputInfo.Titlebar;
            //Settings.General.AlwaysOnTop = false;
            Settings.General.CheckForUpdates = true;
            Settings.General.TibiaPath = Client.Tibia != null ? Client.Tibia.MainModule.FileName : string.Empty;
            Settings.Screenshooter.ActiveWindowOnly = false;
            Settings.Screenshooter.doAutoScreenshot = false;
            Settings.Screenshooter.doOnLevelAdvance = false;
            Settings.Screenshooter.doOnSkillAdvance = false;
            Settings.Screenshooter.FilePath = System.Windows.Forms.Application.StartupPath + "\\Tibianic";
            Settings.Screenshooter.ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            Settings.Tibiacam.LocalListenerPort = 7171;
            Settings.Tibiacam.Playback.doPlayMouse = false;
            Settings.Tibiacam.Playback.OnlyPlayCompatibleFiles = false;
            Settings.Tibiacam.Playback.ReadMetaData = true;
            Settings.Tibiacam.Playback.SupportTibiaMovies = false;
            Settings.Tibiacam.Recorder.doRecordMouse = false;
            Settings.Tibiacam.Recorder.Filters.OutgoingPrivateMessages = false;
            Settings.Tibiacam.Recorder.Filters.IncomingPrivateMessages = false;
            Settings.Tibiacam.Recorder.Filters.DefaultChat = false;
            Settings.Tibiacam.Recorder.MouseInterval = 30;
            Settings.Tibiacam.Recorder.ServerIP = "oldtibia.net";
            Settings.Tibiacam.Recorder.ServerPort = 7272;
        }

        internal class Polygons
        {
            internal static Point[] Exit = new Point[] { new Point(0, 4), new Point(4, 0), new Point(8, 4), new Point(12, 0), new Point(16, 4),
                                           new Point(12, 8), new Point(16, 12), new Point(12, 16), new Point(8, 12), new Point(4, 16),
                                           new Point(0, 12), new Point(4, 8), new Point(0, 4) };
            internal static Point[] Hide = new Point[] { new Point(0, 0), new Point(12, 0), new Point(6, 6), new Point(0, 0) };
        }

        internal class General
        {
            internal static string TibiaPath { get; set; }
            internal static bool CheckForUpdates { get; set; }
            private static bool alwaysOnTop { get; set; }
            internal static bool AlwaysOnTop
            {
                get { return alwaysOnTop; }
                set
                {
                    Forms.UI.ActiveForm.TopMost = value;
                    alwaysOnTop = value;
                }
            }
        }

        internal class Tibiacam
        {
            internal static int LocalListenerPort { get; set; }

            internal class Recorder
            {
                internal static bool doRecordMouse { get; set; }
                internal static ushort MouseInterval = 30;
                internal static string ServerIP { get; set; }
                internal static ushort ServerPort { get; set; }

                internal class Filters
                {
                    internal static bool IncomingPrivateMessages { get; set; }
                    internal static bool OutgoingPrivateMessages { get; set; }
                    internal static bool DefaultChat { get; set; }
                }
            }

            internal class Playback
            {
                /// <summary>
                /// Makes loading playlist longer, but shows every recording's Tibia version.
                /// </summary>
                internal static bool ReadMetaData { get; set; }
                internal static bool OnlyPlayCompatibleFiles { get; set; }
                /// <summary>
                /// Requires zlib1.dll.
                /// </summary>
                internal static bool SupportTibiaMovies { get; set; }
                internal static bool doPlayMouse { get; set; }
            }
        }

        internal class Hotkeys
        {
            internal static Dictionary<System.Windows.Forms.Keys, string> HookedKeys = new Dictionary<System.Windows.Forms.Keys, string>();
        }

        internal class Counters
        {
            internal class Experience
            {
                internal enum OutputInfo : byte
                {
                    Titlebar = 0,
                    Statusbar = 1
                }
                internal enum ExpTNLSources : byte
                {
                    Formula = 0,
                    LevelPercent = 1,
                    ExpTable = 2
                }
                
                internal static bool doEstimateExperienceTNL { get; set; }
                internal static ExpTNLSources ExpTNLSource = ExpTNLSources.ExpTable;
                internal static OutputInfo Output = OutputInfo.Titlebar;
            }
        }

        internal class Screenshooter
        {
            internal static bool doAutoScreenshot { get; set; }
            internal static bool doOnLevelAdvance { get; set; }
            internal static bool doOnSkillAdvance { get; set; }
            internal static string FilePath { get; set; }
            internal static bool ActiveWindowOnly { get; set; }
            internal static System.Drawing.Imaging.ImageFormat ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
        }
    }
}
