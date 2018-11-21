using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Text_Raffle.Properties;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Media;

namespace Text_Raffle
{
    public partial class frmMain : Form
    {
        private List<string> entries, won;
        private bool jrun = false;
        private Random randomizer = new Random();
        private object syncL = new object();
        SoundPlayer ding = new SoundPlayer(Application.StartupPath + "\\sounds\\ding.wav");
        SoundPlayer cheer = new SoundPlayer(Application.StartupPath + "\\sounds\\cheer.wav");

        public frmMain()
        {
            InitializeComponent();
        }

        // Method for prompting won entries.
        private void queryMark()
        {
            if (jrun)
            {
                if (MessageBox.Show(this, "Mark entry as won?", "Winner", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    won.Add(lblWinner.Text);
                }
                jrun = false;
            }
        }

        // Loads a file to a list.
        private void loadFile()
        {
            entries = new List<string>();

            if (txtSource.Text.Contains("csv") || txtSource.Text.Contains("txt"))
            {
                StreamReader reader = new StreamReader(txtSource.Text);
                string line = "";
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine().Trim();
                    if (line == "" || line.Contains("name") || line.Contains("Name"))
                    {
                        continue;
                    }

                    entries.Add(line);
                }
            }

            else if (txtSource.Text.Contains("xlsx"))
            {
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(txtSource.Text);
                Excel._Worksheet xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[1];
                Excel.Range xlRange = xlWorksheet.UsedRange;

                int lastUsedRow = xlWorksheet.Cells.Find("*", System.Reflection.Missing.Value,
                               System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                               Excel.XlSearchOrder.xlByRows, Excel.XlSearchDirection.xlPrevious,
                               false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;

                for (int i = 2; i <= lastUsedRow; i++)
                {
                    entries.Add((string)(xlWorksheet.Cells[i, 1] as Excel.Range).Value);
                }

                //cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //release com objects to fully kill excel process from running in the background
                Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheet);

                //close and release
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);

                //quit and release
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }

            lblNumEntries.Text = entries.Count.ToString();
            btnStart.Enabled = true;
            btnStart.Visible = true;
        }

        // Random number generator
        public int randomNumber(int min, int max)
        {
            lock (syncL)
            {
                return randomizer.Next(min, max);
            }
        }

        // Handles the Load event of the frmMain control.
        // Turns the window into full screen, loads the saved settings, and initializes the main lists.
        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Bounds = Screen.PrimaryScreen.Bounds;
            won = new List<string>();
            if (!(Settings.Default["Won"] == null))
            {
                won = new List<string>(((string)Settings.Default["Won"]).Split('|'));
            }
            entries = new List<string>();

            btnStart.Enabled = false;
            btnStart.Visible = false;
        }

        // Handles the Click event of the btnExit control. Exits the application.
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Handles the Click event of the btnStart control.
        private void btnStart_Click(object sender, EventArgs e)
        {
            queryMark();
            if (entries.Count < 2)
            {
                return;
            }

            tmMain.Interval = 10;
            tmMain.Enabled = true;
        }

        // Triggers the Open File Dialog, and executes the loadFile method.
        private void btnSource_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Excel files (*.xlsx)|*.xlsx|CSV files (*.csv)| *.csv|Text files (*.txt)| *.txt";
            ofd.ShowDialog();
            
            if (ofd.FileName == "")
            {
                return;
            }

            txtSource.Text = ofd.FileName;
            loadFile();
        }

        // Main timer that randomizes the names.
        private void tmMain_Tick(object sender, EventArgs e)
        {
            string entry = "";
            entry = entries[randomNumber(0, entries.Count)];
            while (won.Contains(entry, StringComparer.Ordinal) && won.Count > 1)
            {
                entry = entries[randomNumber(0, entries.Count)];
            }
            lblWinner.Text = entry;
            lblWinner.ForeColor = Color.Red;
            ding.Play();

            tmMain.Interval += 20;
            if (tmMain.Interval >= 550)
            {
                tmCtrl.Enabled = true;
            }
        }

        // Stopping timer, gradually slows (increases interval) randomizing.
        private void tmCtrl_Tick(object sender, EventArgs e)
        {
            cheer.Play();

            lblWinner.ForeColor = Color.Green;
            tmMain.Enabled = false;
            tmCtrl.Enabled = false;
            jrun = true;
        }

        // Handles the FormClosing event of the frmMain control. Saves the settings before closing.
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            queryMark();
            Settings.Default["Won"] = String.Join("|", won.ToArray());
            Settings.Default.Save();
        }

        private void btnStart_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, btnStart.ClientRectangle,
                SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset,
                SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset,
                SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset,
                SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset);
        }

        private void gbSource_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, gbSource.ClientRectangle,
                SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset,
                SystemColors.ControlLightLight, 0, ButtonBorderStyle.Outset,
                SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset,
                SystemColors.ControlLightLight, 5, ButtonBorderStyle.Outset);
        }

        // Opens up the won entry list.
        private void llView_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            queryMark();
            frmWon wonFrm = new frmWon();
            wonFrm.loadList(won);
            wonFrm.ShowDialog();
            won = new List<string>();
            won = wonFrm.updateList();
        }
    }
}
