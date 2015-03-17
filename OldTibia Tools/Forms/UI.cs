using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace TibianicTools.Forms
{
    public partial class UI : Form
    {
        private static Form formHUD = new Form();
        private static Thread HUDThread;
        private static int FormXDefault = 0, FormYDefault = 0, FormXExpCounter = 285, FormXTibiaCam = 537, 
                                                               FormXHotkeys = 410, FormXSkillCounter = 462,
                                                               FormXSettings = 335, FormXHUD = 368;
        private static string UILastClickedBtn = "";
        internal static int highestSkill = 0;
        private bool isMouseDown = false, isExpanded = false, doAnimate = false, doInvalidate = false, HotkeysStatus = true,
                     hasHUDShown = false, doUpdate = false;
        private Point LastCursorPosition;
        internal static globalKeyboardHook gkh = new globalKeyboardHook();
        uint levelOld = 0, levelNew = 0, axeOld = 0, shieldingOld = 0, distanceOld = 0, mlvlOld = 0, 
             clubOld = 0, fishingOld = 0, swordOld = 0, fistOld = 0;
        internal Objects.TibiaCam tibiaCam;

        public UI()
        {
            InitializeComponent();

            Form.CheckForIllegalCrossThreadCalls = false;
            checkboxSettingsCheckForUpdates.Checked = true;

            grpboxExpCounter.Hide();
            grpboxHotkeys.Hide();
            grpboxHUD.Hide();
            grpboxSettings.Hide();
            grpboxSkillCounter.Hide();
            grpboxTibiaCam.Hide();

            Client.Player = new Objects.Player();
            Client.TibiaPath = Client.Tibia.MainModule.FileName;
            HUDThread = new Thread(ThreadHUD);
            formHUD.Shown += new EventHandler(HUD_Shown);
            formHUD.Enter += new EventHandler(HUD_Focused);

            Utils.ExperienceCounter.Start();
            Utils.SkillCounter.Start();

            lblFileVer.MouseDown += new MouseEventHandler(UI_MouseDown);
            lblFileVer.MouseMove += new MouseEventHandler(UI_MouseMove);
            lblFileVer.MouseUp += new MouseEventHandler(UI_MouseUp);

            Point[] exitPolys = new Point[] { new Point(0, 4), new Point(4, 0), new Point(8, 4), new Point(12, 0), new Point(16, 4),
                                              new Point(12, 8), new Point(16, 12), new Point(12, 16), new Point(8, 12), new Point(4, 16),
                                              new Point(0, 12), new Point(4, 8), new Point(0, 4) };
            picboxClose.Image = Utils.Bitmaps.GetPolygon(Color.Red, 17, exitPolys, Color.Black);
            picboxClose.Size = picboxClose.Image.Size;
            picboxMinimize.Image = Utils.Bitmaps.GetRectangle(Color.White, 20, 6);
            picboxMinimize.Size = picboxMinimize.Image.Size;
            Point[] hidetotrayPolys = new Point[] { new Point(0, 0), new Point(12, 0), new Point(6, 6), new Point(0, 0) };
            picboxHideToTray.Image = Utils.Bitmaps.GetPolygon(Color.Gray, 12, hidetotrayPolys, Color.Black);
            picboxHideToTray.Size = picboxHideToTray.Image.Size;
            Region = System.Drawing.Region.FromHrgn(WinApi.CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;


            gkh.KeyDown += new KeyEventHandler(gkh_KeyDown);
            gkh.KeyUp += new KeyEventHandler(gkh_KeyUp);

            if (Settings.General.CheckForUpdates)
            {
                Thread t = new Thread(new ThreadStart(this.ThreadCheckForUpdates));
                t.Start();
            }

            // try-catch in case of error
            try
            {
                Utils.WindowsRegistry.SetAssociation(".kcam", "TibianicTools.Recording", Process.GetCurrentProcess().MainModule.FileName, "Tibianic Tools TibiaCam recording");
            }
            catch { }
        }

        #region User Interface Methods & Control events
        private void UI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.tibiaCam != null) this.tibiaCam.Stop();
            if (!Client.Tibia.HasExited && Client.Tibia.MainWindowTitle != "Tibia") WinApi.SetWindowText(Client.Tibia.MainWindowHandle, "Tibia");
            Settings.SaveSettings("settings.ini");
            trayIcon.Visible = false;
            if (this.doUpdate) Process.Start("patcher.bat");
            Environment.Exit(0);
        }

        private void picboxClose_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) this.Close();
        }

        private void picboxMinimize_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) this.WindowState = FormWindowState.Minimized;
        }

        private void picboxHideToTray_MouseClick(object sender, MouseEventArgs e)
        {
            Hide();
            trayIcon.Visible = true;
        }

        private void UI_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            LastCursorPosition = new Point(e.X, e.Y);
        }

        private void UI_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Location = new Point(Left - (LastCursorPosition.X - e.X), Top - (LastCursorPosition.Y - e.Y));
                if (doInvalidate) this.Invalidate();
            }
        }

        private void UI_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }

        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            trayIcon.Visible = false;
        }

        private void SetFormWidth(int width)
        {
            if (doAnimate)
            {
                while (this.Width != width)
                {
                    if (this.Width > width)
                    {
                        this.Width--;
                    }
                    else if (this.Width < width)
                    {
                        this.Width++;
                    }
                    //Region = System.Drawing.Region.FromHrgn(WinApi.CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
                }
            }
            else
            {
                this.Width = width;
                //Region = System.Drawing.Region.FromHrgn(WinApi.CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
            }
        }

        private void ShowControls(string ButtonText)
        {
            if (isExpanded)
            {
                SetFormWidth(FormXDefault);
                Thread.Sleep(100);
            }
            switch (ButtonText)
            {
                case "Experience Counter":
                    grpboxHUD.Visible = false;
                    grpboxExpCounter.Visible = false;
                    grpboxSkillCounter.Visible = false;
                    grpboxHotkeys.Visible = false;
                    grpboxTibiaCam.Visible = false;
                    grpboxSettings.Visible = false;
                    SetFormWidth(FormXExpCounter);
                    grpboxExpCounter.Visible = true;
                    UILastClickedBtn = "Experience Counter";
                    break;
                case "Skill Counter":
                    grpboxHUD.Visible = false;
                    grpboxExpCounter.Visible = false;
                    grpboxSkillCounter.Visible = false;
                    grpboxHotkeys.Visible = false;
                    grpboxTibiaCam.Visible = false;
                    grpboxSettings.Visible = false;
                    SetFormWidth(FormXSkillCounter);
                    grpboxSkillCounter.Visible = true;
                    UILastClickedBtn = "Skill Counter";
                    break;
                case "Hotkeys":
                    grpboxExpCounter.Visible = false;
                    grpboxSkillCounter.Visible = false;
                    grpboxHotkeys.Visible = false;
                    grpboxTibiaCam.Visible = false;
                    grpboxSettings.Visible = false;
                    grpboxHUD.Visible = false;
                    SetFormWidth(FormXHotkeys);
                    grpboxHotkeys.Visible = true;
                    UILastClickedBtn = "Hotkeys";
                    break;
                case "HUD":
                    grpboxExpCounter.Visible = false;
                    grpboxSkillCounter.Visible = false;
                    grpboxHotkeys.Visible = false;
                    grpboxTibiaCam.Visible = false;
                    grpboxSettings.Visible = false;
                    grpboxHotkeys.Visible = false;
                    SetFormWidth(FormXHUD);
                    grpboxHUD.Visible = true;
                    UILastClickedBtn = "HUD";
                    break;
                case "TibiaCam":
                    grpboxHUD.Visible = false;
                    grpboxExpCounter.Visible = false;
                    grpboxSkillCounter.Visible = false;
                    grpboxHotkeys.Visible = false;
                    grpboxTibiaCam.Visible = false;
                    grpboxSettings.Visible = false;
                    SetFormWidth(FormXTibiaCam);
                    grpboxTibiaCam.Visible = true;
                    UILastClickedBtn = "TibiaCam";
                    break;
                case "Settings":
                    grpboxHUD.Visible = false;
                    grpboxExpCounter.Visible = false;
                    grpboxSkillCounter.Visible = false;
                    grpboxHotkeys.Visible = false;
                    grpboxTibiaCam.Visible = false;
                    grpboxSettings.Visible = false;
                    SetFormWidth(FormXSettings);
                    grpboxSettings.Visible = true;
                    UILastClickedBtn = "Settings";
                    break;
                default:
                    grpboxHUD.Visible = false;
                    grpboxExpCounter.Visible = false;
                    grpboxSkillCounter.Visible = false;
                    grpboxHotkeys.Visible = false;
                    grpboxTibiaCam.Visible = false;
                    grpboxHotkeys.Visible = false;
                    SetFormWidth(FormXDefault);
                    isExpanded = false;
                    break;
            }
        }

        private void btnUIExpCounter_Click(object sender, EventArgs e)
        {
            if (UILastClickedBtn != "Experience Counter")
            {
                ShowControls("Experience Counter");
                isExpanded = true;
            }
            else if (UILastClickedBtn == "Experience Counter" && !isExpanded)
            {
                ShowControls("Experience Counter");
                isExpanded = true;
            }
            else
            {
                ShowControls("null");
            }
        }

        private void btnUISkillCounter_Click(object sender, EventArgs e)
        {
            if (UILastClickedBtn != "Skill Counter")
            {
                ShowControls("Skill Counter");
                isExpanded = true;
            }
            else if (UILastClickedBtn == "Skill Counter" && !isExpanded)
            {
                ShowControls("Skill Counter");
                isExpanded = true;
            }
            else
            {
                ShowControls("null");
            }
        }

        private void btnUIHotkeys_Click(object sender, EventArgs e)
        {
            if (UILastClickedBtn != "Hotkeys")
            {
                ShowControls("Hotkeys");
                isExpanded = true;
            }
            else if (UILastClickedBtn == "Hotkeys" && !isExpanded)
            {
                ShowControls("Hotkeys");
                isExpanded = true;
            }
            else
            {
                ShowControls("null");
            }
        }

        private void btnUIHUD_Click(object sender, EventArgs e)
        {
            if (UILastClickedBtn != "HUD")
            {
                ShowControls("HUD");
                isExpanded = true;
            }
            else if (UILastClickedBtn == "HUD" && !isExpanded)
            {
                ShowControls("HUD");
                isExpanded = true;
            }
            else
            {
                ShowControls("null");
            }
        }

        private void btnUITibiaCam_Click(object sender, EventArgs e)
        {
            if (UILastClickedBtn != "TibiaCam")
            {
                ShowControls("TibiaCam");
                isExpanded = true;
            }
            else if (UILastClickedBtn == "TibiaCam" && !isExpanded)
            {
                ShowControls("TibiaCam");
                isExpanded = true;
            }
            else
            {
                ShowControls("null");
            }
        }

        private void btnUISettings_Click(object sender, EventArgs e)
        {
            if (UILastClickedBtn != "Settings")
            {
                ShowControls("Settings");
                isExpanded = true;
            }
            else if (UILastClickedBtn == "Settings" && !isExpanded)
            {
                ShowControls("Settings");
                isExpanded = true;
            }
            else
            {
                ShowControls("null");
            }
        }

        private void timerMultiUse_Tick(object sender, EventArgs e)
        {
            if (Client.Tibia.HasExited)
            {
                this.Close();
                return;
            }
            if (Client.Player != null) { Client.Player.SetBattlelistAddress(); }
            if (Utils.ExperienceCounter.GetGainedExperience() <= 0) { Utils.ExperienceCounter.Reset(); }

            if (checkboxSettingsScrnShooterAutosave.Checked)
            {
                #region screenshot stuff
                if (levelOld == 0)
                {
                    axeOld = Memory.ReadUInt(Addresses.Player.Axe);
                    clubOld = Memory.ReadUInt(Addresses.Player.Club);
                    fistOld = Memory.ReadUInt(Addresses.Player.Fist);
                    swordOld = Memory.ReadUInt(Addresses.Player.Sword);
                    distanceOld = Memory.ReadUInt(Addresses.Player.Distance);
                    shieldingOld = Memory.ReadUInt(Addresses.Player.Shielding);
                    fishingOld = Memory.ReadUInt(Addresses.Player.Fishing);
                    mlvlOld = Memory.ReadUInt(Addresses.Player.MagicLevel);
                    levelOld = Memory.ReadUInt(Addresses.Player.Level);
                    levelNew = levelOld;
                }
                levelNew = Memory.ReadUInt(Addresses.Player.Level);
                if (levelNew < levelOld)
                {
                    levelOld = 0;
                    return;
                }
                if (checkboxSettingsScrnShooterAutosave.Checked)
                {
                    if (checkboxSettingsScrnShooterAutoSaveLevel.Checked)
                    {
                        if (levelNew > levelOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            levelOld = levelNew;
                        }
                    }
                    if (checkboxSettingsScrnShooterAutoSaveSkill.Checked)
                    {
                        uint axeNew = Memory.ReadUInt(Addresses.Player.Axe);
                        uint clubNew = Memory.ReadUInt(Addresses.Player.Club);
                        uint fistNew = Memory.ReadUInt(Addresses.Player.Fist);
                        uint swordNew = Memory.ReadUInt(Addresses.Player.Sword);
                        uint distanceNew = Memory.ReadUInt(Addresses.Player.Distance);
                        uint shieldingNew = Memory.ReadUInt(Addresses.Player.Shielding);
                        uint fishingNew = Memory.ReadUInt(Addresses.Player.Fishing);
                        uint mlvlNew = Memory.ReadUInt(Addresses.Player.MagicLevel);
                        if (axeNew > axeOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            axeOld = axeNew;
                            clubOld = clubNew;
                            fistOld = fistNew;
                            swordOld = swordNew;
                            distanceOld = distanceNew;
                            shieldingOld = shieldingNew;
                            fishingOld = fishingNew;
                            mlvlOld = mlvlNew;
                        }
                        if (clubNew > clubOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            axeOld = axeNew;
                            clubOld = clubNew;
                            fistOld = fistNew;
                            swordOld = swordNew;
                            distanceOld = distanceNew;
                            shieldingOld = shieldingNew;
                            fishingOld = fishingNew;
                            mlvlOld = mlvlNew;
                        }
                        if (fistNew > fistOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            axeOld = axeNew;
                            clubOld = clubNew;
                            fistOld = fistNew;
                            swordOld = swordNew;
                            distanceOld = distanceNew;
                            shieldingOld = shieldingNew;
                            fishingOld = fishingNew;
                            mlvlOld = mlvlNew;
                        }
                        if (swordNew > swordOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            axeOld = axeNew;
                            clubOld = clubNew;
                            fistOld = fistNew;
                            swordOld = swordNew;
                            distanceOld = distanceNew;
                            shieldingOld = shieldingNew;
                            fishingOld = fishingNew;
                            mlvlOld = mlvlNew;
                        }
                        if (distanceNew > distanceOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            axeOld = axeNew;
                            clubOld = clubNew;
                            fistOld = fistNew;
                            swordOld = swordNew;
                            distanceOld = distanceNew;
                            shieldingOld = shieldingNew;
                            fishingOld = fishingNew;
                            mlvlOld = mlvlNew;
                        }
                        if (shieldingNew > shieldingOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            axeOld = axeNew;
                            clubOld = clubNew;
                            fistOld = fistNew;
                            swordOld = swordNew;
                            distanceOld = distanceNew;
                            shieldingOld = shieldingNew;
                            fishingOld = fishingNew;
                            mlvlOld = mlvlNew;
                        }
                        if (fishingNew > fishingOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            axeOld = axeNew;
                            clubOld = clubNew;
                            fistOld = fistNew;
                            swordOld = swordNew;
                            distanceOld = distanceNew;
                            shieldingOld = shieldingNew;
                            fishingOld = fishingNew;
                            mlvlOld = mlvlNew;
                        }
                        if (mlvlNew > mlvlOld)
                        {
                            Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                             Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                             checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                            axeOld = axeNew;
                            clubOld = clubNew;
                            fistOld = fistNew;
                            swordOld = swordNew;
                            distanceOld = distanceNew;
                            shieldingOld = shieldingNew;
                            fishingOld = fishingNew;
                            mlvlOld = mlvlNew;
                        }
                    }
                }
                #endregion
            }

            if (this.tibiaCam != null && this.tibiaCam.IsRunning)
            {
                if (this.tibiaCam.Mode == Objects.TibiaCam.TibiaCamMode.Record && this.tibiaCam.CurrentRecording != null && !string.IsNullOrEmpty(this.tibiaCam.PreferredFileName))
                {
                    string prefName = this.tibiaCam.PreferredFileName;
                    if (prefName.Contains("\\")) prefName = prefName.Remove(0, prefName.LastIndexOf('\\') + 1);
                    lblTibiaCamFileName.Text = "File name: " + prefName;
                }
                else lblTibiaCamFileName.Text = "File name: --";
                if (this.tibiaCam.Mode == Objects.TibiaCam.TibiaCamMode.Record && this.tibiaCam.CurrentRecording.TimePassed > 0 && Client.Player.Connected)
                {
                    TimeSpan ts = TimeSpan.FromMilliseconds(this.tibiaCam.CurrentRecording.TimePassed);
                    lblTibiaCamTime.Text = "Time: " + string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours,
                                                                                            ts.Minutes,
                                                                                            ts.Seconds);
                }
                else if (this.tibiaCam.Mode == Objects.TibiaCam.TibiaCamMode.Playback && Client.Player.Connected)
                {
                    if (!btnTibiaCamFastForward.Enabled) btnTibiaCamFastForward.Enabled = true;
                    if (this.tibiaCam.CurrentRecording != null)
                    {
                        TimeSpan TimePlayed = TimeSpan.FromMilliseconds(this.tibiaCam.CurrentRecording.TimePassed);
                        TimeSpan TimePlayedTotal = TimeSpan.FromMilliseconds(this.tibiaCam.CurrentRecording.Duration);
                        lblTibiaCamTime.Text = "Time: " + string.Format("{0:D2}:{1:D2}:{2:D2}", TimePlayed.Hours, TimePlayed.Minutes, TimePlayed.Seconds) + " / " +
                                                  string.Format("{0:D2}:{1:D2}:{2:D2}", TimePlayedTotal.Hours, TimePlayedTotal.Minutes, TimePlayedTotal.Seconds);
                        WinApi.SetWindowText(Client.Tibia.MainWindowHandle, this.tibiaCam.CurrentRecording.FileNameShort + " - " + string.Format("{0:D2}:{1:D2}:{2:D2}", TimePlayed.Hours, TimePlayed.Minutes, TimePlayed.Seconds) + " / " +
                                                  string.Format("{0:D2}:{1:D2}:{2:D2}", TimePlayedTotal.Hours, TimePlayedTotal.Minutes, TimePlayedTotal.Seconds) + " - " + this.tibiaCam.PlaybackSpeed + "x");
                    }
                }
                else
                {
                    lblTibiaCamTime.Text = "Time: --";
                    btnTibiaCamFastForward.Enabled = false;
                }
            }
            else
            {
                lblTibiaCamFileName.Text = "File name: --";
                lblTibiaCamTime.Text = "Time: --";
                btnTibiaCamFastForward.Enabled = false;
            }
        }
        #endregion

        #region Experience Counter Controls
        private void btnExpCounterStart_Click(object sender, EventArgs e)
        {
            if (btnExpCounterStart.Text == "Start")
            {
                Utils.ExperienceCounter.Start();
                if (!timerExperienceSkillCounter.Enabled) timerExperienceSkillCounter.Start();
                btnExpCounterStart.Text = "Stop";
            }
            else
            {
                //Utils.ExperienceCounter.Stop();
                WinApi.SetWindowText(Client.Tibia.MainWindowHandle, "Tibia");
                btnExpCounterStart.Text = "Start";
            }
        }

        private void btnExpCounterReset_Click(object sender, EventArgs e)
        {
            Utils.ExperienceCounter.Reset();
        }

        private void btnExpCounterPauseResume_Click(object sender, EventArgs e)
        {
            if (btnExpCounterPauseResume.Text == "Pause")
            {
                Utils.ExperienceCounter.Pause();
                btnExpCounterPauseResume.Text = "Resume";
            }
            else
            {
                Utils.ExperienceCounter.Resume();
                btnExpCounterPauseResume.Text = "Pause";
            }
        }

        private void btnExpCounterCopyInfo_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(DateTime.Now.ToString("HH:mm:ss ") + "Exp to lvl " + (Memory.ReadInt(Addresses.Player.Level) + 1) + ": " + Utils.ExperienceCounter.GetExperienceTNL() +
                                      " [" + Utils.ExperienceCounter.GetLevelPercentTNL() + "%]" + " | Exp/h: " + Utils.ExperienceCounter.GetExperiencePerHour() +
                                      " [" + Utils.ExperienceCounter.GetLevelPercentPerHour() + "%/h]" + " | Exp gained: " + Utils.ExperienceCounter.GetGainedExperience() +
                                      " [" + Utils.ExperienceCounter.GetGainedLevelPercent() + "%]" + " | Active since: " + 
                                      string.Format("{0:D2}:{1:D2}:{2:D2}", Utils.ExperienceCounter.GetTotalRunningTime().Hours, Utils.ExperienceCounter.GetTotalRunningTime().Minutes, Utils.ExperienceCounter.GetTotalRunningTime().Seconds) +
                                      " | Time left: " + Utils.ExperienceCounter.GetTimeLeftTNL());
        }

        private void comboboxExpCounterOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboboxExpCounterExpTNLSetting.SelectedIndex)
            {
                case 0: // formula
                    Settings.Counters.Experience.ExpTNLSource = Settings.Counters.Experience.ExpTNLSources.Formula;
                    break;
                case 1: // level%
                    Settings.Counters.Experience.ExpTNLSource = Settings.Counters.Experience.ExpTNLSources.LevelPercent;
                    break;
                case 2: // exp table
                    Settings.Counters.Experience.ExpTNLSource = Settings.Counters.Experience.ExpTNLSources.ExpTable;
                    break;
            }
        }
        #endregion

        #region Skill Counter Controls
        private void btnSkillCounterStart_Click(object sender, EventArgs e)
        {
            if (btnSkillCounterStart.Text == "Start")
            {
                if (!timerExperienceSkillCounter.Enabled)
                {
                    timerExperienceSkillCounter.Start();
                }
                btnSkillCounterStart.Text = "Stop";
                btnSkillCounterPause.Text = "Pause";
                Utils.SkillCounter.Reset();
                Utils.SkillCounter.Start();
            }
            else
            {
                btnSkillCounterStart.Text = "Start";
                btnSkillCounterPause.Text = "Pause";
                highestSkill = 0;
                lblSkillCounterActive.Text = "Active since: 00:00:00";
                lblSkillCounterDistance.Text = "";
                lblSkillCounterFishing.Text = "";
                lblSkillCounterMelee.Text = "";
                lblSkillCounterMeleeSkill.Text = "Melee:";
                lblSkillCounterMLVL.Text = "";
                lblSkillCounterShielding.Text = "";
                progbarSkillCounterDistance.Value = 0;
                progbarSkillCounterFishing.Value = 0;
                progbarSkillCounterMelee.Value = 0;
                progbarSkillCounterMLVL.Value = 0;
                progbarSkillCounterShielding.Value = 0;
                //Utils.SkillCounter.Stop();
            }
        }

        private void btnSkillCounterReset_Click(object sender, EventArgs e)
        {
            highestSkill = 0;
            Utils.SkillCounter.Reset();
        }

        private void btnSkillCounterPause_Click(object sender, EventArgs e)
        {
            if (btnSkillCounterPause.Text == "Pause")
            {
                Utils.SkillCounter.Pause();
                btnSkillCounterPause.Text = "Resume";
            }
            else
            {
                Utils.SkillCounter.Resume();
                btnSkillCounterPause.Text = "Pause";
            }
        }
        #endregion

        #region ExperienceSkillCounter Timer
        private void timerExperienceSkillCounter_Tick(object sender, EventArgs e)
        {
            if (Memory.ReadByte(Addresses.Client.Connection) == 8)
            {
                if (btnExpCounterStart.Text == "Stop")
                {
                    if (comboboxExpCounterOutput.Text == "Titlebar" || comboboxExpCounterOutput.Text == "")
                    {
                        WinApi.SetWindowText(Client.Tibia.MainWindowHandle, "Exp to lvl " + (Memory.ReadInt(Addresses.Player.Level) + 1) + ": " + Utils.ExperienceCounter.GetExperienceTNL() +
                                                                            " [" + Utils.ExperienceCounter.GetLevelPercentTNL() + "%] | Exp/h: " + Utils.ExperienceCounter.GetExperiencePerHour() +
                                                                            " [" + Utils.ExperienceCounter.GetLevelPercentPerHour() + "%/h] | Exp gained: " + Utils.ExperienceCounter.GetGainedExperience() +
                                                                            " [" + Utils.ExperienceCounter.GetGainedLevelPercent() + "%] | Active since: " +
                                                                            string.Format("{0:D2}:{1:D2}:{2:D2}", Utils.ExperienceCounter.GetTotalRunningTime().Hours,
                                                                                                                  Utils.ExperienceCounter.GetTotalRunningTime().Minutes,
                                                                                                                  Utils.ExperienceCounter.GetTotalRunningTime().Seconds) +
                                                                            " | Time left: " + Utils.ExperienceCounter.GetTimeLeftTNL());
                    }
                    else if (comboboxExpCounterOutput.Text == "Statusbar")
                    {
                        Client.Misc.WriteStatusBar("Exp to lvl " + (Memory.ReadInt(Addresses.Player.Level) + 1) + ": " + Utils.ExperienceCounter.GetExperienceTNL() +
                                                   " [" + Utils.ExperienceCounter.GetLevelPercentTNL() + "%] | Exp/h: " + Utils.ExperienceCounter.GetExperiencePerHour() +
                                                   " [" + Utils.ExperienceCounter.GetLevelPercentPerHour() + "%/h] | Exp gained: " + Utils.ExperienceCounter.GetGainedExperience() +
                                                   " [" + Utils.ExperienceCounter.GetGainedLevelPercent() + "%] | Active since: " +
                                                   string.Format("{0:D2}:{1:D2}:{2:D2}", Utils.ExperienceCounter.GetTotalRunningTime().Hours,
                                                                                         Utils.ExperienceCounter.GetTotalRunningTime().Minutes,
                                                                                         Utils.ExperienceCounter.GetTotalRunningTime().Seconds) +
                                                   " | Time left: " + Utils.ExperienceCounter.GetTimeLeftTNL(), 2);
                    }
                }
                if (btnSkillCounterStart.Text == "Stop")
                {
                    Enums.Skill highestSkillType = new Enums.Skill();
                    highestSkill = 0;
                    int Axe = Memory.ReadInt(Addresses.Player.Axe), Club = Memory.ReadInt(Addresses.Player.Club),
                        Sword = Memory.ReadInt(Addresses.Player.Sword), Fist = Memory.ReadInt(Addresses.Player.Fist);
                    if (Axe > highestSkill)
                    {
                        highestSkill = Axe;
                        highestSkillType = Enums.Skill.Axe;
                    }
                    if (Club > highestSkill)
                    {
                        highestSkill = Club;
                        highestSkillType = Enums.Skill.Club;
                    }
                    if (Sword > highestSkill)
                    {
                        highestSkill = Sword;
                        highestSkillType = Enums.Skill.Sword;
                    }
                    if (Fist > highestSkill)
                    {
                        highestSkill = Fist;
                        highestSkillType = Enums.Skill.Fist;
                    }
                    Structs.Skill Melee = Utils.SkillCounter.GetSkillInfo(highestSkillType);
                    progbarSkillCounterMelee.Value = 100 - Melee.PercentLeft;
                    lblSkillCounterMeleeSkill.Text = Melee.Name + ":";

                    Structs.Skill MLVL = Utils.SkillCounter.GetSkillInfo(Enums.Skill.MagicLevel);
                    Structs.Skill Distance = Utils.SkillCounter.GetSkillInfo(Enums.Skill.Distance);
                    Structs.Skill Fishing = Utils.SkillCounter.GetSkillInfo(Enums.Skill.Fishing);
                    Structs.Skill Shielding = Utils.SkillCounter.GetSkillInfo(Enums.Skill.Shielding);

                    progbarSkillCounterDistance.Value = 100 - Distance.PercentLeft;
                    progbarSkillCounterFishing.Value = 100 - Fishing.PercentLeft;
                    progbarSkillCounterMLVL.Value = 100 - MLVL.PercentLeft;
                    progbarSkillCounterShielding.Value = 100 - Shielding.PercentLeft;

                    lblSkillCounterMelee.Text = Melee.CurrentSkill + " [" + Melee.PercentLeft + "%, " + Melee.PercentGained + "% gained, " +
                                                Melee.PercentPerHour + "%/h, " + Melee.TimeLeft + " left]";
                    lblSkillCounterMLVL.Text = MLVL.CurrentSkill + " [" + MLVL.PercentLeft + "%, " + MLVL.PercentGained + "% gained, " +
                                                MLVL.PercentPerHour + "%/h, " + MLVL.TimeLeft + " left]";
                    lblSkillCounterDistance.Text = Distance.CurrentSkill + " [" + Distance.PercentLeft + "%, " + Distance.PercentGained + "% gained, " +
                                                   Distance.PercentPerHour + "%/h, " + Distance.TimeLeft + " left]";
                    lblSkillCounterShielding.Text = Shielding.CurrentSkill + " [" + Shielding.PercentLeft + "%, " + Shielding.PercentGained + "% gained, " +
                                                   Shielding.PercentPerHour + "%/h, " + Shielding.TimeLeft + " left]";
                    lblSkillCounterFishing.Text = Fishing.CurrentSkill + " [" + Fishing.PercentLeft + "%, " + Fishing.PercentGained + "% gained, " +
                                                   Fishing.PercentPerHour + "%/h, " + Fishing.TimeLeft + " left]";

                    lblSkillCounterActive.Text = "Active since: " + string.Format("{0:D2}:{1:D2}:{2:D2}",
                                                                                  Utils.SkillCounter.GetTotalRunningTime().Hours,
                                                                                  Utils.SkillCounter.GetTotalRunningTime().Minutes,
                                                                                  Utils.SkillCounter.GetTotalRunningTime().Seconds);
                }
                else if (btnSkillCounterStart.Text == "Start" && btnExpCounterStart.Text == "Start")
                {
                    timerExperienceSkillCounter.Stop();
                }
            }
            else
            {
                if (Client.Tibia.MainWindowTitle != "Tibia") WinApi.SetWindowText(Client.Tibia.MainWindowHandle, "Tibia");
                if (lblSkillCounterDistance.Text.Length > 0)
                {
                    lblSkillCounterDistance.Text = "";
                    lblSkillCounterFishing.Text = "";
                    lblSkillCounterMelee.Text = "";
                    lblSkillCounterMeleeSkill.Text = "Melee:";
                    lblSkillCounterMLVL.Text = "";
                    lblSkillCounterShielding.Text = "";
                    progbarSkillCounterDistance.Value = 0;
                    progbarSkillCounterFishing.Value = 0;
                    progbarSkillCounterMelee.Value = 0;
                    progbarSkillCounterMLVL.Value = 0;
                    progbarSkillCounterShielding.Value = 0;
                }
            }
        }
        #endregion

        #region TibiaCam Controls
        private void btnTibiaCamFastForward_Click(object sender, EventArgs e)
        {
            if (this.tibiaCam == null) return;
            TimeSpan ts;
            if (TimeSpan.TryParse(txtboxTibiaCamFastForward.Text, out ts))
            {
                this.tibiaCam.FastForward(ts);
            }
        }

        private void btnTibiaCamPlay_Click(object sender, EventArgs e)
        {
            if (btnTibiaCamPlay.Text == "Play" && btnTibiaCamRecord.Text == "Record")
            {
                if (this.tibiaCam == null) this.tibiaCam = new Objects.TibiaCam();
                else { this.tibiaCam.Stop(); }
                if (this.tibiaCam.Mode != Objects.TibiaCam.TibiaCamMode.Playback) this.tibiaCam = new Objects.TibiaCam();

                if (Memory.ReadByte(Addresses.Client.Connection) == 0)
                {
                    btnTibiaCamPlay.Text = "Stop";
                    this.tibiaCam.Start();
                    btnTibiaCamRecord.Enabled = false;
                    btnTibiaCamBrowse.Enabled = true;

                    if (!gkh.HookedKeys.Contains(Keys.Up)) gkh.HookedKeys.Add(Keys.Up);
                    if (!gkh.HookedKeys.Contains(Keys.Down)) gkh.HookedKeys.Add(Keys.Down);
                    if (!gkh.HookedKeys.Contains(Keys.Left)) gkh.HookedKeys.Add(Keys.Left);
                    if (!gkh.HookedKeys.Contains(Keys.Right)) gkh.HookedKeys.Add(Keys.Right);
                    if (!gkh.HookedKeys.Contains(Keys.Back)) gkh.HookedKeys.Add(Keys.Back);
                }
                else
                {
                    btnTibiaCamPlay.Text = "Play";
                    this.tibiaCam.Stop();
                    btnTibiaCamRecord.Enabled = true;
                    btnTibiaCamBrowse.Enabled = false;
                }
            }
            else if (btnTibiaCamPlay.Text == "Stop")
            {
                this.tibiaCam.Stop();
                btnTibiaCamPlay.Text = "Play";
                btnTibiaCamRecord.Enabled = true;
                btnTibiaCamBrowse.Enabled = false;
            }
        }

        private void btnTibiaCamRecord_Click(object sender, EventArgs e)
        {
            if (btnTibiaCamRecord.Text == "Record" && btnTibiaCamPlay.Text == "Play" &&
                txtboxTibiaCamIP.Text != "Write IP or DNS here")
            {
                if (this.tibiaCam == null) this.tibiaCam = new Objects.TibiaCam(new Objects.LoginServer(txtboxTibiaCamIP.Text, (ushort)numericTibiaCamPort.Value));
                else { this.tibiaCam.Stop(); this.tibiaCam = new Objects.TibiaCam(new Objects.LoginServer(txtboxTibiaCamIP.Text, (ushort)numericTibiaCamPort.Value)); }

                if (Memory.ReadByte(Addresses.Client.Connection) == 0)
                {
                    btnTibiaCamRecord.Text = "Stop";
                    this.tibiaCam.Start();
                    btnTibiaCamPlay.Enabled = false;
                    btnTibiaCamBrowse.Enabled = true;
                    checkboxTibiaCamRecordMouse.Enabled = false;
                }
                else
                {
                    btnTibiaCamRecord.Text = "Record";
                    txtboxTibiaCamIP.Enabled = true;
                    numericTibiaCamPort.Enabled = true;
                    this.tibiaCam.Stop();
                    btnTibiaCamPlay.Enabled = true;
                    btnTibiaCamBrowse.Enabled = false;
                    checkboxTibiaCamRecordMouse.Enabled = true;
                }
            }
            else if (btnTibiaCamRecord.Text == "Stop")
            {
                btnTibiaCamRecord.Text = "Record";
                txtboxTibiaCamIP.Enabled = true;
                numericTibiaCamPort.Enabled = true;
                if (this.tibiaCam != null) this.tibiaCam.Stop();
                btnTibiaCamPlay.Enabled = true;
                btnTibiaCamBrowse.Enabled = false;
                checkboxTibiaCamRecordMouse.Enabled = true;
            }
        }

        private void btnTibiaCamBrowse_Click(object sender, EventArgs e)
        {
            if (btnTibiaCamRecord.Text == "Stop")
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.Filter = "Tibianic Tools recordings (*.kcam)|*.kcam";
                saveFile.Title = "Choose where to save your recording";
                saveFile.InitialDirectory = Application.StartupPath;
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    this.tibiaCam.PreferredFileName = saveFile.FileName;
                    lblTibiaCamFileName.Text = "File name: " + Path.GetFileName(saveFile.FileName);
                }
            }
            else if (btnTibiaCamPlay.Text == "Stop" && Memory.ReadByte(Addresses.Client.Connection) == 0)
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Filter = "Tibianic Tools recordings (*.kcam)|*.kcam|IryontCam recordings (*.cam)|*.cam";
                openFile.Title = "Choose a recording to play";
                openFile.InitialDirectory = Application.StartupPath;
                if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (this.tibiaCam != null) tibiaCam.Stop();
                    this.tibiaCam = new Objects.TibiaCam();
                    tibiaCam.AutoPlayback = true;
                    tibiaCam.CurrentRecording = new Objects.Recording(openFile.FileName, Settings.Tibiacam.Playback.ReadMetaData, true, Settings.Tibiacam.Playback.doPlayMouse);
                    this.tibiaCam.Start();
                }
            }
        }

        private void trackbarTibiaCamSpeed_Scroll(object sender, EventArgs e)
        {
            if (this.tibiaCam == null) return;
            this.tibiaCam.PlaybackSpeed = trackbarTibiaCamSpeed.Value;
            if (trackbarTibiaCamSpeed.Value == trackbarTibiaCamSpeed.Maximum)
            {
                this.tibiaCam.PlaybackSpeed = 200;
            }
            lblTibiaCamSpeed.Text = this.tibiaCam.PlaybackSpeed.ToString();
        }

        private void checkboxTibiaCamEnableTibiaMovieSupport_CheckedChanged(object sender, EventArgs e)
        {
            if (checkboxTibiaCamEnableTibiaMovieSupport.Checked)
            {
                if (!File.Exists("zlib1.dll"))
                {
                    MessageBox.Show("File zlib1.dll was not found.\nThis file is needed to decompress TibiaMovie recordings.\nPlease copy zlib1.dll to the Tibianic Tools folder.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    checkboxTibiaCamEnableTibiaMovieSupport.Checked = false;
                    Settings.Tibiacam.Playback.SupportTibiaMovies = false;
                }
                else { Settings.Tibiacam.Playback.SupportTibiaMovies = true; }
            }
            else { Settings.Tibiacam.Playback.SupportTibiaMovies = false; }
        }

        private void checkboxTibiaCamReadMetaData_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Tibiacam.Playback.ReadMetaData = checkboxTibiaCamReadMetaData.Checked;
            if (checkboxTibiaCamReadMetaData.Checked)
            {
                checkboxTibiaCamOnlyPlayCompatible.Enabled = true;
            }
            else
            {
                checkboxTibiaCamOnlyPlayCompatible.Enabled = false;
                checkboxTibiaCamOnlyPlayCompatible.Checked = false;
            }
        }

        private void txtboxTibiaCamIP_TextChanged(object sender, EventArgs e)
        {
            Settings.Tibiacam.Recorder.ServerIP = txtboxTibiaCamIP.Text;
        }

        private void numericTibiaCamPort_ValueChanged(object sender, EventArgs e)
        {
            Settings.Tibiacam.Recorder.ServerPort = (ushort)numericTibiaCamPort.Value;
        }
        #endregion

        #region Hotkeys Controls
        private void btnHotkeysHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hotkey is where you press a key to hotkey\nAction is an action that is executed when you press the hotkey\n\n" +
                            "Available actions:\ntoggle hotkeys --toggles hotkeys on and off\nscreenshot / screenshoot / ss --captures a screenshot\nup / right / down / left / upright / upleft / downright / downleft --walks in a direction\n" + 
                            "F1 to F12 / shift+F1 to shift+F12 / ctrl+F1 to ctrl+F12 --presses an F-key" +
                            "\n\nAll hotkeys are not case sensitive\nWant more actions? Request them!", "Hotkeys Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHotkeysSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Hotkey config files (*.hcfg)|*.hcfg";
            saveFile.Title = "Save Hotkeys";
            if (Client.Player.Name.Length > 0)
            {
                saveFile.FileName = "Hotkeys-" + Client.Player.Name;
            }
            else
            {
                saveFile.FileName = "Hotkeys-";
            }
            saveFile.InitialDirectory = Application.StartupPath;
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(saveFile.FileName)) { File.Delete(saveFile.FileName); }
                Utils.FileWrite(saveFile.FileName, "[Hotkeys]", false);
                for (int i = 0; i < datagridHotkeys.Rows.Count; i++)
                {
                    Utils.FileWrite(saveFile.FileName,
                                    "true," + datagridHotkeys.Rows[i].Cells[0].Value.ToString() + "," +
                                    datagridHotkeys.Rows[i].Cells[1].Value.ToString(), true);
                }
            }
        }

        private void btnHotkeysLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.CheckFileExists = true;
            openFile.CheckPathExists = true;
            openFile.Filter = "Hotkey config files (*.hcfg)|*.hcfg";
            openFile.InitialDirectory = Application.StartupPath;
            openFile.Multiselect = false;
            openFile.Title = "Load Hotkeys";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    datagridHotkeys.Rows.Clear();
                    int j = 0;
                    bool found = false;
                    foreach (string line in Utils.FileRead(openFile.FileName))
                    {
                        if (!found && line == "[Hotkeys]") { found = true; continue; }
                        else if (!found && line != "[Hotkeys]") { continue; }
                        if (line.Length < 2)
                        {
                            break;
                        }
                        string[] lineSplit = line.Split(',');
                        // 0 obsolete bool, 1 hotkey, 2 action
                        datagridHotkeys.Rows.Add(lineSplit[1], lineSplit[2]);
                        if (!gkh.HookedKeys.Contains((Keys)int.Parse(lineSplit[1].Substring(1,
                                                                     lineSplit[1].IndexOf("]") - 1))))
                        {
                            gkh.HookedKeys.Add((Keys)int.Parse(lineSplit[1].Substring(1,
                                                               lineSplit[1].IndexOf("]") - 1)));
                        }

                        j++;
                    }
                }
                catch (Exception ex)
                {
                    datagridHotkeys.Rows.Clear();
                    MessageBox.Show("Could not load " + openFile.FileName + "\n\n" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void btnHotkeysClear_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in datagridHotkeys.Rows)
            {
                string cell = row.Cells[0].Value.ToString();
                int key = int.Parse(cell.Substring(1, cell.IndexOf(']') - 1));
                gkh.HookedKeys.Remove((Keys)key);
            }
            datagridHotkeys.Rows.Clear();
        }

        void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
            if (WinApi.GetForegroundWindow() == Client.Tibia.MainWindowHandle &&
                Memory.ReadByte(Addresses.Client.Connection) == 8)
            {
                for (int i = 0; i < datagridHotkeys.Rows.Count; i++)
                {
                    int _Hotkey = int.Parse(datagridHotkeys.Rows[i].Cells[0].Value.ToString().Substring(1,
                                            datagridHotkeys.Rows[i].Cells[0].Value.ToString().IndexOf(']') - 1));
                    string _Action = datagridHotkeys.Rows[i].Cells[1].Value.ToString();
                    Objects.Packet p;
                    if (e.KeyValue == _Hotkey)
                    {
                        switch (_Action.ToLower())
                        {
                            case "toggle hotkeys":
                                if (HotkeysStatus)
                                {
                                    for (int integer = 0; integer < datagridHotkeys.Rows.Count; integer++)
                                    {
                                        if (datagridHotkeys.Rows[integer].Cells[1].Value.ToString().ToLower() != "toggle hotkeys")
                                        {
                                            int hotkey = int.Parse(datagridHotkeys.Rows[integer].Cells[0].Value.ToString().Substring(1,
                                                                   datagridHotkeys.Rows[integer].Cells[0].Value.ToString().IndexOf(']') - 1));
                                            gkh.HookedKeys.Remove((Keys)hotkey);
                                        }
                                    }
                                    Client.Misc.WriteStatusBar("Hotkeys off!", 2);
                                    HotkeysStatus = false;
                                }
                                else if (!HotkeysStatus)
                                {
                                    for (int i2 = 0; i2 < datagridHotkeys.Rows.Count; i2++)
                                    {
                                        int hotkey = int.Parse(datagridHotkeys.Rows[i2].Cells[0].Value.ToString().Substring(1,
                                                                   datagridHotkeys.Rows[i2].Cells[0].Value.ToString().IndexOf(']') - 1));
                                        if (datagridHotkeys.Rows[i2].Cells[1].Value.ToString().ToLower() != "toggle hotkeys" && !gkh.HookedKeys.Contains((Keys)hotkey))
                                        {
                                            gkh.HookedKeys.Add((Keys)hotkey);
                                        }
                                    }
                                    Client.Misc.WriteStatusBar("Hotkeys on!", 2);
                                    HotkeysStatus = true;
                                }
                                e.Handled = true;
                                break;
                            case "count gold":
                                List<Objects.Container> Containers = Objects.Container.GetContainers();
                                int goldCount = 0;
                                foreach (Objects.Container container in Containers)
                                {
                                    foreach (Objects.Item item in container.Items)
                                    {
                                        // gold id = 2148
                                        if (item.ID == 2148) goldCount += item.Count;
                                    }
                                }
                                Client.Misc.WriteStatusBar("You have " + goldCount + " gold.", 3);
                                e.Handled = true;
                                break;
                            case "up":
                            case "right":
                            case "down":
                            case "left":
                            case "downleft":
                            case "downright":
                            case "upleft":
                            case "upright":
                            case "f1":
                            case "f2":
                            case "f3":
                            case "f4":
                            case "f5":
                            case "f6":
                            case "f7":
                            case "f8":
                            case "f9":
                            case "f10":
                            case "f11":
                            case "f12":
                            case "shift+f1":
                            case "shift+f2":
                            case "shift+f3":
                            case "shift+f4":
                            case "shift+f5":
                            case "shift+f6":
                            case "shift+f7":
                            case "shift+f8":
                            case "shift+f9":
                            case "shift+f10":
                            case "shift+f11":
                            case "shift+f12":
                            case "ctrl+f1":
                            case "ctrl+f2":
                            case "ctrl+f3":
                            case "ctrl+f4":
                            case "ctrl+f5":
                            case "ctrl+f6":
                            case "ctrl+f7":
                            case "ctrl+f8":
                            case "ctrl+f9":
                            case "ctrl+f10":
                            case "ctrl+f11":
                            case "ctrl+f12":
                                Utils.SendTibiaKeys(_Action);
                                e.Handled = true;
                                break;
                            case "screenshot":
                            case "screenshoot":
                            case "ss":
                                Utils.Screenshot(txtboxSettingsScrnShooterFilePath.Text,
                                                 Utils.ConvertStringToImageFormat(comboboxSettingsScrnShooterFileFormat.Text),
                                                 checkboxSettingsScrnShooterCaptureActiveWindow.Checked);
                                e.Handled = true;
                                break;
                            default:
                                if (Utils.isStringNumeric(_Action))
                                {
                                    Utils.SendTibiaKeys(_Action);
                                }
                                e.Handled = true;
                                break;
                        }
                    }
                }
                if (this.tibiaCam != null && this.tibiaCam.IsRunning && this.tibiaCam.Mode == Objects.TibiaCam.TibiaCamMode.Playback &&
                    Memory.ReadByte(Addresses.Client.DialogOpen) == 0)
                {
                    if (this.tibiaCam.PlaybackSpeed < 0 || this.tibiaCam.PlaybackSpeed > 200) this.tibiaCam.PlaybackSpeed = 0;
                    switch (e.KeyCode)
                    {
                        case Keys.Up:
                            this.tibiaCam.PlaybackSpeed = 200;
                            e.Handled = true;
                            break;
                        case Keys.Right:
                            if (this.tibiaCam.PlaybackSpeed < 200 &&
                                this.tibiaCam.PlaybackSpeed >= 1)
                            {
                                this.tibiaCam.PlaybackSpeed++;
                            }
                            else if (this.tibiaCam.PlaybackSpeed < 1 &&
                                     this.tibiaCam.PlaybackSpeed >= 0)
                            {
                                this.tibiaCam.PlaybackSpeed += 0.1;
                            }
                            e.Handled = true;
                            break;
                        case Keys.Left:
                            if (this.tibiaCam.PlaybackSpeed > 1) this.tibiaCam.PlaybackSpeed--;
                            else if (this.tibiaCam.PlaybackSpeed <= 1 &&
                                     this.tibiaCam.PlaybackSpeed > 0)
                            {
                                this.tibiaCam.PlaybackSpeed -= 0.1;
                            }
                            e.Handled = true;
                            break;
                        case Keys.Down:
                            this.tibiaCam.PlaybackSpeed = 1.0;
                            e.Handled = true;
                            break;
                        case Keys.Back:
                            if (this.tibiaCam.CurrentRecording.TimePassed> 60 * 1000) // 1 minute
                            {
                                this.tibiaCam.FastForward(TimeSpan.FromMilliseconds(this.tibiaCam.CurrentRecording.TimePassed - 60 * 1000));
                            }
                            else this.tibiaCam.FastForward(TimeSpan.FromMilliseconds(0));
                            e.Handled = true;
                            break;
                    }
                    this.tibiaCam.PlaybackSpeed = Math.Round(this.tibiaCam.PlaybackSpeed, 1);
                    TimeSpan TimePlayed = TimeSpan.FromMilliseconds(this.tibiaCam.CurrentRecording.TimePassed);
                    TimeSpan TimePlayedTotal = TimeSpan.FromMilliseconds(this.tibiaCam.CurrentRecording.Duration);
                    WinApi.SetWindowText(Client.Tibia.MainWindowHandle, this.tibiaCam.CurrentRecording.FileNameShort + " - " + string.Format("{0:D2}:{1:D2}:{2:D2}", TimePlayed.Hours, TimePlayed.Minutes, TimePlayed.Seconds) + " / " +
                                         string.Format("{0:D2}:{1:D2}:{2:D2}", TimePlayedTotal.Hours, TimePlayedTotal.Minutes, TimePlayedTotal.Seconds) + " - " + this.tibiaCam.PlaybackSpeed + "x");

                    if (this.tibiaCam.PlaybackSpeed < 0 || this.tibiaCam.PlaybackSpeed > 200) this.tibiaCam.PlaybackSpeed = 0;
                }
                if (e.Handled == false && Memory.ReadByte(Addresses.Client.DialogOpen) == 0) gkh.HookedKeys.Remove(e.KeyCode);
            }
            else
            {
                e.Handled = false;
            }
        }
        void gkh_KeyUp(object sender, KeyEventArgs e)
        {
            if (WinApi.GetForegroundWindow() == Client.Tibia.MainWindowHandle)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    e.Handled = false;
                    return;
                }
                else
                {
                    e.Handled = true;
                    return;
                }
            }
            else { e.Handled = false; }
        }

        private void txtboxHotkeysHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            txtboxHotkeysHotkey.Text = "[" + e.KeyValue + "] " + e.KeyCode.ToString();
            e.SuppressKeyPress = true;
        }

        private void btnHotkeysAdd_Click(object sender, EventArgs e)
        {
            if (comboboxHotkeysAction.Text.Length > 0 &&
                txtboxHotkeysHotkey.Text.Length > 0)
            {
                datagridHotkeys.Rows.Add(txtboxHotkeysHotkey.Text, comboboxHotkeysAction.Text);
                if (!gkh.HookedKeys.Contains((Keys)int.Parse(txtboxHotkeysHotkey.Text.Substring(1, txtboxHotkeysHotkey.Text.IndexOf("]") - 1))))
                {
                    gkh.HookedKeys.Add((Keys)int.Parse(txtboxHotkeysHotkey.Text.Substring(1, txtboxHotkeysHotkey.Text.IndexOf("]") - 1)));
                }
                comboboxHotkeysAction.Text = "";
                txtboxHotkeysHotkey.Text = "";
            }
        }

        private void txtboxHotkeysAction_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnHotkeysAdd_Click(sender, e);
            }
        }

        private void datagridHotkeys_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (datagridHotkeys.SelectedRows.Count > 0)
                {
                    string cell = datagridHotkeys.SelectedRows[0].Cells[0].Value.ToString();
                    int vkey = int.Parse(cell.Substring(1, cell.IndexOf(']') - 1));
                    if (gkh.HookedKeys.Contains((Keys)vkey))
                    {
                        gkh.HookedKeys.Remove((Keys)vkey);
                    }
                    datagridHotkeys.Rows.Remove(datagridHotkeys.SelectedRows[0]);
                }
            }
        }
        #endregion

        #region Settings Controls
        private void numericupdownSettingsOpacity_ValueChanged(object sender, EventArgs e)
        {
            this.Opacity = (double)numericupdownSettingsOpacity.Value / 100;
        }

        private void checkboxSettingsAnimateForm_CheckedChanged(object sender, EventArgs e)
        {
            doAnimate = checkboxSettingsAnimateForm.Checked;
        }

        private void checkboxSettingsRedrawForm_CheckedChanged(object sender, EventArgs e)
        {
            doInvalidate = checkboxSettingsRedrawForm.Checked;
        }

        private void btnSettingsScrnShooterBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Title = "Choose where to save screenshots";
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                txtboxSettingsScrnShooterFilePath.Text = saveFile.FileName;
                Settings.Screenshooter.FilePath = saveFile.FileName;
            }
        }

        private void checkboxSettingsAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkboxSettingsAlwaysOnTop.Checked;
        }

        private void comboboxSettingsScrnShooterFileFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Screenshooter.ImageFormat = comboboxSettingsScrnShooterFileFormat.SelectedIndex == 0 ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Png;
        }

        private void checkboxSettingsCheckForUpdates_CheckedChanged(object sender, EventArgs e)
        {
            Settings.General.CheckForUpdates = checkboxSettingsCheckForUpdates.Checked;
        }

        private void checkboxSettingsScrnShooterCaptureActiveWindow_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Screenshooter.ActiveWindowOnly = checkboxSettingsScrnShooterCaptureActiveWindow.Checked;
        }

        private void checkboxSettingsScrnShooterAutosave_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Screenshooter.doAutoScreenshot = checkboxSettingsScrnShooterAutosave.Checked;
        }

        private void checkboxSettingsScrnShooterAutoSaveLevel_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Screenshooter.doOnLevelAdvance = checkboxSettingsScrnShooterAutoSaveLevel.Checked;
        }

        private void checkboxSettingsScrnShooterAutoSaveSkill_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Screenshooter.doOnSkillAdvance = checkboxSettingsScrnShooterAutoSaveSkill.Checked;
        }
        #endregion

        #region HUD
        private void HUD_Focused(object sender, EventArgs e)
        {
            WinApi.SetForegroundWindow(Client.Tibia.MainWindowHandle);
        }

        private void HUD_Shown(object sender, EventArgs e)
        {
            const int GWL_EXSTYLE = -20;
            const int WS_EX_TRANSPARENT = 0x20;

            int exstyle = WinApi.GetWindowLong(formHUD.Handle, GWL_EXSTYLE);
            exstyle |= WS_EX_TRANSPARENT;
            WinApi.SetWindowLong(formHUD.Handle, GWL_EXSTYLE, exstyle);
            
            /*IntPtr hwndf = formHUD.Handle;
            IntPtr hwndParent = IntPtr.Zero;
            WinApi.SetParent(hwndf, hwndParent);*/

            //this.TopMost = true;
            hasHUDShown = true;

            formHUD.ShowInTaskbar = false;
            formHUD.ShowIcon = false;
            formHUD.TopMost = true;
            formHUD.Show();
            formHUD.FormBorderStyle = FormBorderStyle.None;
            formHUD.TransparencyKey = formHUD.BackColor;
            WinApi.RECT rect = new WinApi.RECT();
            WinApi.GetClientRect(Client.Tibia.MainWindowHandle, out rect);
            formHUD.Size = new Size(rect.right, rect.bottom);
            WinApi.GetWindowRect(Client.Tibia.MainWindowHandle, out rect);
            formHUD.Location = new Point(rect.left, rect.top);
            timerHUD.Start();
        }

        private void timerHUD_Tick(object sender, EventArgs e)
        {
            if (!HUDThread.IsAlive)
            {
                HUDThread = new Thread(ThreadHUD);
                HUDThread.Start();
            }
        }

        private void ThreadHUD()
        {
            try
            {
                while (timerHUD.Enabled)
                {
                    if (!Client.Tibia.HasExited && WinApi.GetForegroundWindow() == Client.Tibia.MainWindowHandle)
                    {
                        if (!formHUD.Visible)
                        {
                            formHUD.Show();
                        }
                        if (Memory.ReadByte(Addresses.Client.Connection) == 8)
                        {
                            try
                            {
                                string firstLoginServer = Client.Misc.GetLoginServers()[0].IP;
                                if (firstLoginServer != null)
                                {
                                    string IP = Objects.CharacterList.GetPlayers()[Memory.ReadByte(Addresses.Charlist.SelectedIndex)].IP;
                                    Utils.Pinger.Ping(IP);
                                }
                            }
                            catch (Exception ex) { }
                        }
                        WinApi.RECT rect = new WinApi.RECT();
                        WinApi.GetWindowRect(Client.Tibia.MainWindowHandle, out rect);
                        if (!formHUD.Size.Equals(new Size(rect.right - rect.left, rect.bottom - rect.top)) ||
                            !formHUD.Location.Equals(new Point(rect.left, rect.top)))
                        {
                            formHUD.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                            formHUD.Location = new Point(rect.left, rect.top);
                        }
                        if (listboxHUDLabels.Items.Count > 0)
                        {
                            foreach (Label formLabel in formHUD.Controls)
                            {
                                string labelName = formLabel.Name;
                                for (int i = 0; i < listboxHUDLabels.Items.Count; i++)
                                {
                                    string text = listboxHUDLabels.Items[i].ToString();
                                    string ID = text.Substring(0, text.IndexOf(':'));
                                    if (ID == formLabel.Name)
                                    {
                                        string Text = text.Substring(text.IndexOf(':') + 1);
                                        // to be replaced with a stringbuilder
                                        formLabel.Text = Text.Replace("$experience", Memory.ReadUInt(Addresses.Player.Exp).ToString())
                                                                   .Replace("$exptnl", Utils.ExperienceCounter.GetExperienceTNL().ToString())
                                                                   .Replace("$expgained", Utils.ExperienceCounter.GetGainedExperience().ToString())
                                                                   .Replace("$exph", Utils.ExperienceCounter.GetExperiencePerHour().ToString())
                                                                   .Replace("$lvlperctnl", Utils.ExperienceCounter.GetLevelPercentTNL().ToString())
                                                                   .Replace("$lvlperch", Utils.ExperienceCounter.GetLevelPercentPerHour().ToString())
                                                                   .Replace("$lvlpercgained", Utils.ExperienceCounter.GetGainedLevelPercent().ToString())
                                                                   .Replace("$lvltimetnl", Utils.ExperienceCounter.GetTimeLeftTNL())
                                                                   .Replace("$axeskill", Memory.ReadUShort(Addresses.Player.Axe).ToString())
                                                                   .Replace("$axegained", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Axe).PercentGained.ToString())
                                                                   .Replace("$axepercent", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Axe).PercentLeft.ToString())
                                                                   .Replace("$axeperch", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Axe).PercentPerHour.ToString())
                                                                   .Replace("$axetimetnl", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Axe).TimeLeft)
                                                                   .Replace("$clubskill", Memory.ReadUShort(Addresses.Player.Club).ToString())
                                                                   .Replace("$clubgained", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Axe).PercentGained.ToString())
                                                                   .Replace("$clubpercent", (100 - Memory.ReadUShort(Addresses.Player.ClubPercent)).ToString())
                                                                   .Replace("$clubperch", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Club).PercentPerHour.ToString())
                                                                   .Replace("$clubtimetnl", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Club).TimeLeft)
                                                                   .Replace("$swordskill", Memory.ReadUShort(Addresses.Player.Sword).ToString())
                                                                   .Replace("$swordgained", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Sword).PercentGained.ToString())
                                                                   .Replace("$swordpercent", (100 - Memory.ReadUShort(Addresses.Player.SwordPercent)).ToString())
                                                                   .Replace("$swordperch", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Sword).PercentPerHour.ToString())
                                                                   .Replace("$swordtimetnl", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Sword).TimeLeft)
                                                                   .Replace("$distskill", Memory.ReadUShort(Addresses.Player.Distance).ToString())
                                                                   .Replace("$distgained", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Distance).PercentGained.ToString())
                                                                   .Replace("$distpercent", (100 - Memory.ReadUShort(Addresses.Player.DistancePercent)).ToString())
                                                                   .Replace("$distperch", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Distance).PercentPerHour.ToString())
                                                                   .Replace("$disttimetnl", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Distance).TimeLeft)
                                                                   .Replace("$fistskill", Memory.ReadUShort(Addresses.Player.Fist).ToString())
                                                                   .Replace("$fistgained", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Fist).PercentGained.ToString())
                                                                   .Replace("$fistpercent", (100 - Memory.ReadUShort(Addresses.Player.FistPercent)).ToString())
                                                                   .Replace("$fistperch", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Fist).PercentPerHour.ToString())
                                                                   .Replace("$fisttimetnl", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Fist).TimeLeft)
                                                                   .Replace("$mlvlskill", Memory.ReadUShort(Addresses.Player.MagicLevel).ToString())
                                                                   .Replace("$mlvlgained", Utils.SkillCounter.GetSkillInfo(Enums.Skill.MagicLevel).PercentGained.ToString())
                                                                   .Replace("$mlvlpercent", (100 - Memory.ReadUShort(Addresses.Player.MagicLevelPercent)).ToString())
                                                                   .Replace("$mlvlperch", Utils.SkillCounter.GetSkillInfo(Enums.Skill.MagicLevel).PercentPerHour.ToString())
                                                                   .Replace("$mlvltimetnl", Utils.SkillCounter.GetSkillInfo(Enums.Skill.MagicLevel).TimeLeft)
                                                                   .Replace("$shieldingskill", Memory.ReadUShort(Addresses.Player.Shielding).ToString())
                                                                   .Replace("$shieldinggained", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Shielding).PercentGained.ToString())
                                                                   .Replace("$shieldingpercent", (100 - Memory.ReadUShort(Addresses.Player.ShieldingPercent)).ToString())
                                                                   .Replace("$shieldingperch", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Shielding).PercentPerHour.ToString())
                                                                   .Replace("$shieldingtimetnl", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Shielding).TimeLeft)
                                                                   .Replace("$fishingskill", Memory.ReadUShort(Addresses.Player.Fishing).ToString())
                                                                   .Replace("$fishinggained", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Fishing).PercentGained.ToString())
                                                                   .Replace("$fishingpercent", (100 - Memory.ReadUShort(Addresses.Player.FishingPercent)).ToString())
                                                                   .Replace("$fishingperch", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Fishing).PercentPerHour.ToString())
                                                                   .Replace("$fishingtimetnl", Utils.SkillCounter.GetSkillInfo(Enums.Skill.Fishing).TimeLeft)
                                                                   .Replace("$timeactiveexp", Utils.ExperienceCounter.GetTotalRunningTimeString())
                                                                   .Replace("$timeactiveskill", Utils.SkillCounter.GetTotalRunningTimeString())
                                                                   .Replace("$tibiastarttime", Client.Tibia.StartTime.ToString())
                                                                   .Replace("$fps", Client.Misc.FPS.ToString())
                                                                   .Replace("$ping", Utils.Pinger.LastRoundtripTime.ToString() + "ms");

                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (formHUD.Visible)
                        {
                            formHUD.Hide();
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionHandler(ex);
            }
        }

        private void btnHUDAdd_Click(object sender, EventArgs e)
        {
            if (txtboxHUDText.Text.Length > 0)
            {
                Label label = new Label();
                label.Text = txtboxHUDText.Text;
                label.Font = fontDialog.Font;
                label.ForeColor = fontDialog.Color;
                label.BackColor = Color.Transparent;
                label.AutoSize = true;
                label.Location = new Point((int)numericupdownHUDPosX.Value, (int)numericupdownHUDPosY.Value);
                int labelID = Utils.RandomizeInt(1000, 9999);
                bool canUseID = true;
                while (canUseID)
                {
                    for (int i = 0; i < listboxHUDLabels.Items.Count; i++)
                    {
                        if (listboxHUDLabels.Items[i].ToString().Split(':')[0] == labelID.ToString())
                        {
                            canUseID = false;
                            break;
                        }
                    }
                    if (!canUseID)
                    {
                        labelID = Utils.RandomizeInt(1000, 9999);
                        canUseID = true;
                    }
                    else { break; }
                }
                label.Name = labelID.ToString();
                label.Click += new EventHandler(HUD_Focused);
                listboxHUDLabels.Items.Add(label.Name + ":" + label.Text);
                formHUD.Controls.Add(label);
                if (!hasHUDShown)
                {
                    formHUD.Show();
                }
                txtboxHUDText.Text = "";
            }
        }

        private void btnHUDClear_Click(object sender, EventArgs e)
        {
            listboxHUDLabels.Items.Clear();
            formHUD.Controls.Clear();
        }

        private void btnHUDChooseFont_Click(object sender, EventArgs e)
        {
            fontDialog.ShowDialog();
        }

        private void btnHUDSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "HUD config files (*.hudcfg)|*.hudcfg";
            saveFile.Title = "Save HUD config";
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(saveFile.FileName))
                {
                    File.Delete(saveFile.FileName);
                }
                for (int i = 0; i < formHUD.Controls.Count; i++)
                {
                    string text = listboxHUDLabels.Items[i].ToString().Substring(listboxHUDLabels.Items[i].ToString().IndexOf(':') + 1);
                    Label label = (Label)formHUD.Controls[i];
                    Utils.FileWrite(saveFile.FileName, label.Name + "," + label.Font.OriginalFontName + "," +
                                    Math.Round(label.Font.Size) + "," + label.ForeColor.ToKnownColor().ToString() + "," + 
                                    label.Location.X + "," + label.Location.Y + "," +
                                    text, true);
                }
            }
        }

        private void btnHUDLoad_Click(object sender, EventArgs e)
        {
            string fileName = "a HUD config file";
            try
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.CheckFileExists = true;
                openFile.CheckPathExists = true;
                openFile.Filter = "HUD config files (*.hudcfg)|*.hudcfg";
                openFile.InitialDirectory = Application.StartupPath;
                openFile.Multiselect = false;
                openFile.Title = "Load HUD config";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    fileName = openFile.FileName;
                    listboxHUDLabels.Items.Clear();
                    formHUD.Controls.Clear();
                    string[] config = Utils.FileRead(openFile.FileName);
                    for (int i = 0; i < config.Length; i++)
                    {
                        if (config[i].Length > 3)
                        {
                            //0=Name,1=FontName,2=FontSize,3=ForeColor,4=LocX,5=LocY,6+=Text
                            string[] split = config[i].Split(',');
                            Label label = new Label();
                            label.Name = split[0];
                            label.Font = new Font(split[1], float.Parse(split[2]));
                            label.ForeColor = Color.FromName(split[3]);
                            label.Location = new Point(int.Parse(split[4]), int.Parse(split[5]));
                            label.Text = string.Join(",", split, 6, split.Length - 6).TrimEnd('\r');
                            label.BackColor = Color.Transparent;
                            label.AutoSize = true;
                            listboxHUDLabels.Items.Add(label.Name + ":" + label.Text);
                            formHUD.Controls.Add(label);
                        }
                    }
                    if (!hasHUDShown)
                    {
                        HUD_Shown(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionHandler(ex);
                MessageBox.Show("An error occurred while trying to load " + fileName + ".\nSee Error.txt for more info.",
                                "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void listboxHUDLabels_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && listboxHUDLabels.SelectedIndex > -1)
            {
                string ID = listboxHUDLabels.Items[listboxHUDLabels.SelectedIndex].ToString().Split(':')[0];
                foreach (Label label in formHUD.Controls)
                {
                    if (label.Name == ID)
                    {
                        formHUD.Controls.Remove(label);
                        break;
                    }
                }
                listboxHUDLabels.Items.RemoveAt(listboxHUDLabels.SelectedIndex);
            }
        }

        private void btnHUDHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Available variables:\n\n$experience - current experience\n$exptnl - experience left to next level" +
                            "\n$exph - experience per hour\n$expgained - gained experience" +
                            "\n$lvlperctnl - level percent to next level\n$lvlperch - level percent per hour\n$lvlpercgained - gained level percent\n$lvltimetnl - time to next level advance" +
                            "\n$axeskill - current axe skill\n$axegained - gained axe percent\n$axepercent - axe percent to next advance\n$axeperch - axe percent per hour\n$axetimetnl - time to next axe advance" +
                            "\n$clubskill - current club skill\n$clubgained - gained club percent\n$clubpercent - club percent to next advance\n$clubperch - club percent per hour\n$clubtimetnl - time to next club advance" +
                            "\n$fistskill - current fist skill\n$fistgained - gained fist percent\n$fistpercent - fist percent to next advance\n$fistperch - fist percent per hour\n$fisttimetnl - time to next fist advance" +
                            "\n$swordskill - current sword skill\n$swordgained - gained sword percent\n$swordpercent - sword percent to next advance\n$swordperch - sword percent per hour\n$swordtimetnl - time to next sword advance" +
                            "\n$distskill - current distance skill\n$distgained - gained distance percent\n$distpercent - distance percent to next advance\n$distperch - distance percent per hour\n$disttimetnl - time to next distance advance" +
                            "\n$mlvlskill - current magic level\n$mlvlgained - gained magic level percent\n$mlvlpercent - magic level percent to next advance\n$mlvlperch - magic level percent per hour\n$mlvltimetnl - time to next magic level advance" +
                            "\n$fishingskill - current fishing skill\n$fishinggained - gained fishing percent\n$fishingpercent - fishing percent to next advance\n$fishingperch - fishing percent per hour\n$fishingtimetnl - time to next fishing advance" +
                            "\n$timeactiveexp - time since counting experience started (hh:mm:ss)\n$timeactiveskill - time since counting skills started (hh:mm:ss)\n$tibiastarttime - time when tibia was started (hh:mm:ss)" +
                            "\n$fps - shows your current fps\n$ping - shows your ping (excl. client delay) to the server you're connected to",
                            "HUD Help",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        private void UI_Shown(object sender, EventArgs e)
        {
            if (!WinApi.IsIconic(Client.Tibia.MainWindowHandle)) //IsIconic = minimized
            {
                WinApi.RECT rectTibiaPos = new WinApi.RECT();
                WinApi.GetWindowRect(Client.Tibia.MainWindowHandle, out rectTibiaPos);
                WinApi.RECT rectClient = new WinApi.RECT();
                WinApi.GetClientRect(Client.Tibia.MainWindowHandle, out rectClient);
                this.Location = new Point(rectTibiaPos.right - rectClient.right, rectTibiaPos.bottom - rectClient.bottom);
            }
            if (this.tibiaCam != null && (this.tibiaCam.AutoPlayback || this.tibiaCam.AutoRecord))
            {
                btnTibiaCamPlay.Text = "Stop";
                this.tibiaCam.Start();
                btnTibiaCamRecord.Enabled = false;
                btnTibiaCamBrowse.Enabled = true;

                if (!gkh.HookedKeys.Contains(Keys.Up)) gkh.HookedKeys.Add(Keys.Up);
                if (!gkh.HookedKeys.Contains(Keys.Down)) gkh.HookedKeys.Add(Keys.Down);
                if (!gkh.HookedKeys.Contains(Keys.Left)) gkh.HookedKeys.Add(Keys.Left);
                if (!gkh.HookedKeys.Contains(Keys.Right)) gkh.HookedKeys.Add(Keys.Right);
                if (!gkh.HookedKeys.Contains(Keys.Back)) gkh.HookedKeys.Add(Keys.Back);
            }
            this.Width = 105; // for some reason the width fucks up in the designer
            FormXDefault = this.Width;
            FormYDefault = this.Height;

            Settings.LoadDefaults();
            Settings.LoadSettings("settings.ini");
            checkboxSettingsAlwaysOnTop.Checked = Settings.General.AlwaysOnTop;
            checkboxSettingsAnimateForm.Checked = false;
            checkboxSettingsCheckForUpdates.Checked = Settings.General.CheckForUpdates;
            checkboxSettingsRedrawForm.Checked = false;
            checkboxSettingsScrnShooterAutosave.Checked = Settings.Screenshooter.doAutoScreenshot;
            checkboxSettingsScrnShooterAutoSaveLevel.Checked = Settings.Screenshooter.doOnLevelAdvance;
            checkboxSettingsScrnShooterAutoSaveSkill.Checked = Settings.Screenshooter.doOnSkillAdvance;
            checkboxSettingsScrnShooterCaptureActiveWindow.Checked = Settings.Screenshooter.ActiveWindowOnly;
            checkboxTibiaCamFiltersOutPM.Checked = Settings.Tibiacam.Recorder.Filters.OutgoingPrivateMessages;
            checkboxTibiaCamOnlyPlayCompatible.Checked = Settings.Tibiacam.Playback.OnlyPlayCompatibleFiles;
            checkboxTibiaCamPlayMouse.Checked = Settings.Tibiacam.Playback.doPlayMouse;
            checkboxTibiaCamReadMetaData.Checked = Settings.Tibiacam.Playback.ReadMetaData;
            checkboxTibiaCamRecordMouse.Checked = Settings.Tibiacam.Recorder.doRecordMouse;
            numericTibiaCamPort.Value = Settings.Tibiacam.Recorder.ServerPort;
            numericupdownTibiaCamMouseInterval.Value = Settings.Tibiacam.Recorder.MouseInterval;
            comboboxExpCounterOutput.SelectedIndex = (byte)Settings.Counters.Experience.Output;
            comboboxExpCounterExpTNLSetting.SelectedIndex = (byte)Settings.Counters.Experience.ExpTNLSource;
            comboboxSettingsScrnShooterFileFormat.SelectedIndex = Settings.Screenshooter.ImageFormat == System.Drawing.Imaging.ImageFormat.Jpeg ? 0 : 1;
            txtboxSettingsScrnShooterFilePath.Text = Settings.Screenshooter.FilePath;
            txtboxTibiaCamIP.Text = Settings.Tibiacam.Recorder.ServerIP;
        }

        private void UI_Resize(object sender, EventArgs e)
        {
            this.Region = System.Drawing.Region.FromHrgn(WinApi.CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
        }

        private void ThreadCheckForUpdates()
        {
            try
            {
                System.Net.WebClient webclient = new System.Net.WebClient();
                string data = webclient.DownloadString(@"http://tibianic-tools.googlecode.com/files/version.txt");
                string[] split = data.Split('\n');
                if (int.Parse(split[0]) > Settings.CurrentVersion)
                {
                    if (MessageBox.Show("There is a newer version available.\nWould you like to download it?", "Update available", MessageBoxButtons.YesNo)
                        != DialogResult.Yes)
                    {
                        return;
                    }
                    string url = split[1];
                    webclient.DownloadFile(url, "Tibianic Tools.bak");
                    string fileName = Process.GetCurrentProcess().MainModule.FileName;
                    List<string> batch = new List<string>();
                    batch.Add("@ECHO OFF");
                    batch.Add("echo Waiting for Tibianic Tools to close ...");
                    batch.Add("timeout /T 5");
                    batch.Add("del \"" + fileName + "\"");
                    batch.Add("ren \"Tibianic Tools.bak\" \"Tibianic Tools.exe\"");
                    batch.Add("echo Patching complete. You may run Tibianic Tools now.");
                    batch.Add("pause");
                    if (File.Exists("patcher.bat")) File.Delete("patcher.bat");
                    File.WriteAllLines("patcher.bat", batch.ToArray(), Encoding.ASCII);
                    this.doUpdate = true;
                    this.Close();
                }
            }
            catch { }
        }

        private void checkboxTibiaCamRecordMouse_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Tibiacam.Recorder.doRecordMouse = checkboxTibiaCamRecordMouse.Checked;
            numericupdownTibiaCamMouseInterval.Enabled = checkboxTibiaCamRecordMouse.Checked && btnTibiaCamRecord.Text == "Record";
        }

        private void numericupdownTibiaCamMouseInterval_ValueChanged(object sender, EventArgs e)
        {
            Settings.Tibiacam.Recorder.MouseInterval = (ushort)numericupdownTibiaCamMouseInterval.Value;
        }

        private void checkboxTibiaCamPlayMouse_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Tibiacam.Playback.doPlayMouse = checkboxTibiaCamPlayMouse.Checked;
        }

        private void checkboxTibiaCamFiltersOutPM_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Tibiacam.Recorder.Filters.OutgoingPrivateMessages = checkboxTibiaCamFiltersOutPM.Checked;
        }

        #region Toolstrip events
        private void contextmenuTray_Opening(object sender, CancelEventArgs e)
        {
            toolstripExpCounterStartStop.Text = btnExpCounterStart.Text;
            toolstripExpCounterReset.Text = btnExpCounterReset.Text;
            toolstripExpCounterPauseResume.Text = btnExpCounterPauseResume.Text;
            toolstripSkillCounterStartStop.Text = btnSkillCounterStart.Text;
            toolstripSkillCounterReset.Text = btnSkillCounterReset.Text;
            toolstripSkillCounterPause.Text = btnSkillCounterPause.Text;
        }

        private void toolstripExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolstripHide_Click(object sender, EventArgs e)
        {
            this.Show();
            trayIcon.Visible = false;
        }

        private void toolstripExpCounterStartStop_Click(object sender, EventArgs e)
        {
            btnExpCounterStart_Click(sender, e);
        }

        private void toolstripExpCounterReset_Click(object sender, EventArgs e)
        {
            btnExpCounterReset_Click(sender, e);
        }

        private void toolstripExpCounterPauseResume_Click(object sender, EventArgs e)
        {
            btnExpCounterPauseResume_Click(sender, e);
        }

        private void toolstripSkillCounterStartStop_Click(object sender, EventArgs e)
        {
            btnSkillCounterStart_Click(sender, e);
        }

        private void toolstripSkillCounterReset_Click(object sender, EventArgs e)
        {
            btnSkillCounterReset_Click(sender, e);
        }

        private void toolstripSkillCounterPause_Click(object sender, EventArgs e)
        {
            btnSkillCounterPause_Click(sender, e);
        }

        private void toolstripExpCounterCopyInfo_Click(object sender, EventArgs e)
        {
            btnExpCounterCopyInfo_Click(sender, e);
        }
        #endregion

        private void comboboxExpCounterOutput_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            switch (comboboxExpCounterOutput.SelectedIndex)
            {
                case 0: // titlebar
                    Settings.Counters.Experience.Output = Settings.Counters.Experience.OutputInfo.Titlebar;
                    break;
                case 1: // statusbar
                    Settings.Counters.Experience.Output = Settings.Counters.Experience.OutputInfo.Statusbar;
                    break;
            }
        }
    }
}
