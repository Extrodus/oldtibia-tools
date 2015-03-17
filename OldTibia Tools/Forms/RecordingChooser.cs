using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace TibianicTools.Forms
{
    public partial class RecordingChooser : Form
    {
        List<Objects.Recording> recordings = new List<Objects.Recording>();
        bool refreshed = false;

        public RecordingChooser()
        {
            InitializeComponent();
            picboxClose.Image = Utils.Bitmaps.GetPolygon(Color.Red, 17, Settings.Polygons.Exit, Color.Black);
            picboxClose.Size = picboxClose.Image.Size;
            picboxMinimize.Image = Utils.Bitmaps.GetRectangle(Color.White, 20, 6);
            picboxMinimize.Size = picboxMinimize.Image.Size;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (datagridRecordings.SelectedRows.Count > 0)
            {
                DataGridViewRow row = datagridRecordings.SelectedRows[0];
                string filePath = row.Cells[0].Value.ToString();
                if (Settings.Tibiacam.Playback.OnlyPlayCompatibleFiles)
                {
                    string strVersion = row.Cells[2].Value.ToString();
                    ushort version = ushort.Parse(strVersion.Replace(".", ""));
                    if (version + 100 != Client.TibiaVersion)
                    {
                        if (MessageBox.Show("File version and Tibia version does not match.\nDo you want to play the file anyway?", "Warning", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                        {
                            return;
                        }
                    }
                }

            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            recordings.Clear();
            datagridRecordings.Rows.Clear();
            Thread t = new Thread(new ThreadStart(ReadRecordings));
            t.Start();
        }

        /// <summary>
        /// Should be run on its own thread.
        /// </summary>
        private void ReadRecordings()
        {
            try
            {
                refreshed = false;
                List<string> files = new List<string>();
                foreach (string kcam in System.IO.Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\", "*.kcam"))
                {
                    files.Add(kcam);
                }
                if (Settings.Tibiacam.Playback.SupportTibiaMovies)
                {
                    foreach (string tmv in System.IO.Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\", "*.tmv"))
                    {
                        files.Add(tmv);
                    }
                }
                foreach (string file in files) recordings.Add(new Objects.Recording(file, true, false, false));
            }
            catch { }
            refreshed = true;
        }

        private void timerUpdateRows_Tick(object sender, EventArgs e)
        {
            if (refreshed)
            {
                foreach (Objects.Recording rec in recordings)
                {
                    TimeSpan ts = TimeSpan.FromMilliseconds(rec.Duration);
                    datagridRecordings.Rows.Add(rec.FileNameShort, ts.Hours + ":" + ts.Minutes + ":" + ts.Seconds, rec.TibiaVersion);
                }
                refreshed = false;
                timerUpdateRows.Stop();
            }
        }
    }
}
