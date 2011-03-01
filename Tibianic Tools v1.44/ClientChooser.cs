using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace TibianicTools
{
    public partial class ClientChooser : Form
    {
        private bool isMouseDown = false;
        private Point LastCursorPosition;

        public ClientChooser()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(WinApi.CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
            picboxClose.Image = Properties.Resources.close_button;
            picboxClose.Size = Properties.Resources.close_button.Size;
            picboxMinimize.Image = Properties.Resources.minimize;
            picboxMinimize.Size = Properties.Resources.minimize.Size;
            lblTitle.MouseDown += new MouseEventHandler(ClientChooser_MouseDown);
            lblTitle.MouseMove += new MouseEventHandler(ClientChooser_MouseMove);
            lblTitle.MouseUp += new MouseEventHandler(ClientChooser_MouseUp);
        }

        private void picboxClose_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Environment.Exit(0);
            }
        }

        private void picboxMinimize_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        private void comboboxClients_Click(object sender, EventArgs e)
        {
            comboboxClients.Items.Clear();
            List<Process> tibiaList = new List<Process>();
            foreach (Process tibia in Utils.GetProcessesFromClassName("TibiaClient"))
            {
                if (!tibia.HasExited)
                {
                    Client.Tibia = tibia;
                    Client.TibiaHandle = Client.Tibia.Handle;
                    if (Addresses.SetAddresses(Client.Tibia.MainModule.FileVersionInfo.FileVersion))
                    {
                        Player player = new Player();
                        if (player.Connected)
                        {
                            comboboxClients.Items.Add("[" + tibia.Id + "] " + player.Name + " (" + Client.Tibia.MainModule.FileVersionInfo.FileVersion + ")");
                        }
                        else
                        {
                            comboboxClients.Items.Add("[" + tibia.Id + "] Offline (" + Client.Tibia.MainModule.FileVersionInfo.FileVersion + ")");
                        }
                    }
                }
            }
        }

        private void btnChoose_Click(object sender, EventArgs e)
        {
            if (comboboxClients.Text.Length > 0)
            {
                int ProcessID = int.Parse(comboboxClients.Text.Substring(1, comboboxClients.Text.IndexOf(']') - 1));
                try
                {
                    Process TibiaProcess = Process.GetProcessById(ProcessID);
                    Client.Tibia = TibiaProcess;
                    Client.TibiaHandle = TibiaProcess.Handle;
                    this.Hide();
                    UI mainForm = new UI();
                    mainForm.Show();
                }
                catch { }
            }
        }

        private void ClientChooser_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            LastCursorPosition = new Point(e.X, e.Y);
        }

        private void ClientChooser_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Location = new Point(Left - (LastCursorPosition.X - e.X), Top - (LastCursorPosition.Y - e.Y));
                Invalidate();
            }
        }

        private void ClientChooser_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }
    }
}
