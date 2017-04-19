using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Media;
using System.Reflection;

namespace Action_Timer
{
    public partial class Form1 : Form
    {
        #region EXECUTION_STATE
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }
        #endregion
        //timer1 is the main loop when the application starts counting backwards, timer2 is for some visual effects
        int hours, minutes, seconds, timer1tick = 0, timer2tick = 0;
        long ticks;
        Stopwatch sw; // Stowatch class is very accurate in time messures
        string filename;
        bool passFormClosingMessage;

        //for fun made different button names
        private enum Names
        {
            Go, LetsGo, RunMe, Start
        };
        //-----------------------------------

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) // some initilizations have been done in the [Design], the rest are here
        {
            passFormClosingMessage = false;
            sw = new Stopwatch();

            //for fun made different button names
            string[] names = Enum.GetNames(typeof(Names));
            Random random = new Random();
            int randomEnum = random.Next(names.Length);
            var ret = Enum.Parse(typeof(Names), names[randomEnum]);
            button1.Text = ret.ToString();
            //-----------------------------------
            
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "H:mm:ss";
            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            dateTimePicker1.ShowUpDown = true;
            hours = 0;
            minutes = 0;
            seconds = 0;
            ticks = 0;

            checkBox1.Checked = true;

            notifyIcon1.Text = "Please start the timer";
            button2.Enabled = false;
            comboBox1.SelectedIndex = 1;
            axWindowsMediaPlayer1.Visible = true; //there is an imported media player
            axWindowsMediaPlayer1.settings.volume = 80;

            timer3.Interval = 25 * 1000;
            timer3.Start();

            string[] args = Environment.GetCommandLineArgs(); //new string[] { "", "0", "0", "5", "2", @"D:\Program Files (x86)\CodeAndWeb\TexturePacker\" };
            //MessageBox.Show(args.Length.ToString());
            //foreach (string s in args)
            //    MessageBox.Show(s);
            if (args.Length > 1 && args.Length <= 7) // args[0] is always the path of .exe
            {
                int h = int.TryParse(args[1], out h) ? h : 0;
                int m = args.Length > 2 ? ((int.TryParse(args[2], out m) ? m : 0)) : 0;
                int s = args.Length > 3 ? ((int.TryParse(args[3], out s) ? s : 0)) : 0;
                int c = args.Length > 4 ? ((int.TryParse(args[4], out c) ? c : 0)) : 0;
                string p = args.Length > 5 ? args[5] : "";
                p.Replace(@"\", "/");
                bool b = args.Length > 6 ? (bool.TryParse(args[6], out b) ? b : true) : true;
                    
                automaticValues(h, m, s, c, p, b);
                button1_Click(sender, e);
                this.Hide();
                this.WindowState = FormWindowState.Minimized;
            }
            else if (args.Length > 1 && args.Length != 7)
            {
                MessageBox.Show("Wrong number of arguments");
            }
            
        }

        private void automaticValues(int hour = 0, int minute = 0, int second = 0, int comboSelection = 0, string file = "", bool preventSM = true)
        {
            if ((hour < 24 && hour >= 0) && (minute < 60 && minute >= 0) && (second < 60 && second >= 0) && (comboSelection >= 0 && comboSelection <= 2))
            {
                dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);
                comboBox1.SelectedIndex = comboSelection;
                filename = file;
                if (filename != "")
                {
                    if (comboBox1.SelectedIndex == 1)
                        button1.Text = "RunAlarm";
                    else if (comboBox1.SelectedIndex == 2)
                        button1.Text = "RunFile";
                }
                checkBox1.Checked = preventSM;
            }
            else
                MessageBox.Show("Something went wrong with the values");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1) // thus the Alarm choise, for 0 is the Shutdown
            {
                if (!(button1.Text == "RunAlarm")) // if its not "RunAlarm", thus it will be the "ChooseFile"
                {
                    DialogResult result = this.openFileDialog1.ShowDialog(); // opens a file dialog to navigate to the .mp3 file for the alarm
                    if (result == DialogResult.OK)
                    {
                        if (openFileDialog1.FileName.EndsWith(".mp3")) // safety check if the file is .mp3
                        {
                            filename = this.openFileDialog1.FileName;
                            button1.Text = "RunAlarm"; // this sets the button name and passes the "if (!(button1.Text == "RunAlarm"))"
                            return; // return because we dont want to run the program at this state
                        }
                        else
                            MessageBox.Show("Only .mp3 files");
                    }
                    else
                        return; // return if the file dialog is canceled
                }
            }
            if (comboBox1.SelectedIndex == 2) // the file open timer
            {
                if (!(button1.Text == "RunFile")) // if its not "RunFile", thus it will be the "ChooseFile"
                {
                    DialogResult result = this.openFileDialog1.ShowDialog(); // opens a file dialog to navigate to the file
                    if (result == DialogResult.OK)
                    {
                        filename = this.openFileDialog1.FileName;
                        button1.Text = "RunFile";
                        return;
                    }
                    else
                        return; // return if the file dialog is canceled
                }
            }
            hours = dateTimePicker1.Value.Hour;
            minutes = dateTimePicker1.Value.Minute;
            seconds = dateTimePicker1.Value.Second;
            ticks = (hours * 3600 + minutes * 60 + seconds)*1000; // the total milliseconds of timer
            if (comboBox1.SelectedIndex == 0)
                if (ticks < 10*1000) // no point to set the timer less than 5 seconds
                    return;
            timer1.Interval = 100;
            timer1.Start();
            button1.Enabled = false;
            sw.Start();
            comboBox1.Enabled = false;
        }

        // the main loop of the program is a timer with 100ms interval that gets started when the user presses the button
        private void timer1_Tick(object sender, EventArgs e)
        {
            // there are various graphical element enables and disables throughout the code for safity and visual purposes
            button2.Enabled = true;
            checkBox1.Enabled = false;
            TimeSpan t = TimeSpan.FromSeconds(ticks/1000 - sw.ElapsedMilliseconds/1000); // the milliseconds left
            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, t.Hours, t.Minutes, t.Seconds); // visually showing the time left
            notifyIcon1.Text = t.ToString(); // visually showing the time left in notify icon too
            if (sw.ElapsedMilliseconds >= (hours * 3600 + minutes * 60 + seconds) * 1000) // this check is not so accurate because the timer1_Tick runs every 100ms, but it works for this project
            {
                timer1.Stop();
                sw.Stop();
                sw.Reset();
                if (comboBox1.SelectedIndex == 0) // shutdown
                {
                    passFormClosingMessage = true; // this bool stops the popup messageBox when the program is about to exit
                    button1.Enabled = true;
                    button2.Enabled = false;
                    comboBox1.Enabled = true;
                    checkBox1.Enabled = true;

                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "CMD.exe";
                    startInfo.Arguments = "/C shutdown.exe -s -t 5";
                    process.StartInfo = startInfo;
                    process.Start();

                    //if (!File.Exists("shutdown.bat"))
                    //{
                    //    StreamWriter w = new StreamWriter("shutdown.bat");
                    //    w.WriteLine("shutdown.exe -s -t 5");
                    //    w.Close();
                    //}
                    //Process.Start("shutdown.bat"); // a .bat file runs when the time runs out
                    Application.Exit();
                }
                else if (comboBox1.SelectedIndex == 1) // alarm
                {
                    // temporary file path - your temp file = video.avi
                    var strTempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Alarm.mp3");

                    // ResourceName = the resource you want to play
                    File.WriteAllBytes(strTempFile, Properties.Resources.Alarm);

                    axWindowsMediaPlayer1.URL = strTempFile; //filename; // the .mp3 name
                    axWindowsMediaPlayer1.Ctlcontrols.play();

                    button2.Text = "OkStop"; // later this will stop the alarm sound
                    button1.Enabled = false;
                    button2.Enabled = true;
                    comboBox1.Enabled = false;
                    checkBox1.Enabled = true;
                }
                else if (comboBox1.SelectedIndex == 2) // file open timer
                {
                    passFormClosingMessage = true;

                    try { Process.Start(filename); } // try to open the file
                    catch { } // continue peacefully if not
                    finally {
                        button1.Enabled = true;
                        button2.Enabled = false;
                        comboBox1.Enabled = true;
                        checkBox1.Enabled = true;
                    }//{Application.Exit();} // exit after all
                }
            }

            timer1tick++;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "OkStop") // the alarm stop
            {
                axWindowsMediaPlayer1.Ctlcontrols.stop();
                axWindowsMediaPlayer1.close();
                button2.Text = "Pause";
                button2.Enabled = false;
                button1.Enabled = true;
                comboBox1.Enabled = true;
            }
            else // the shutdown pause
            {
                if (timer1.Enabled)
                {
                    timer1.Stop(); // if pause is asked, stop the timer1 with timer1.Stop(), it doesnt reset the timer
                    sw.Stop();     // same for the stopwatch
                    button2.Text = "Continue";

                    // this is for visual purposes
                    timer2.Interval = 700;
                    timer2.Start();
                    this.BackColor = Color.LightBlue;
                    // ---------------------------
                    notifyIcon1.Text = "Paused!";
                }
                else if (!timer1.Enabled)
                {
                    timer1.Start(); // continue if button2 is clicked and timer is not running
                    sw.Start();     // same
                    button2.Text = "Pause";

                    // this is for visual purposes
                    timer2.Stop();
                    this.BackColor = Color.LightSkyBlue;
                    // ---------------------------
                }
            }
        }

        // minimize also accours in Form1_Resize event, aside with others
        private void Form1_Resize(object sender, EventArgs e) // the sets of the tray icon when minimized
        {
            if (timer1.Enabled)
            {
                if (comboBox1.SelectedIndex == 0)
                {
                    notifyIcon1.BalloonTipTitle = "Shutdown Timer";
                    notifyIcon1.BalloonTipText = "Shutdown Timer is running...";
                }
                else if (comboBox1.SelectedIndex == 1)
                {
                    notifyIcon1.BalloonTipTitle = "Alarm Timer";
                    notifyIcon1.BalloonTipText = "Alarm Timer is running...";
                }
                else
                {
                    notifyIcon1.BalloonTipTitle = "File Timer";
                    notifyIcon1.BalloonTipText = "File Timer is running...";
                }
            }
            else
            {
                if (comboBox1.SelectedIndex == 0)
                {
                    notifyIcon1.BalloonTipTitle = "Shutdown Timer is not running";
                    notifyIcon1.BalloonTipText = "Please set or unpause the timer!\n(Click here to setup)";
                }
                else if (comboBox1.SelectedIndex == 1)
                {
                    notifyIcon1.BalloonTipTitle = "Alarm Timer is not running";
                    notifyIcon1.BalloonTipText = "Please set or unpause the timer!\n(Click here to setup)";
                }
                else
                {
                    notifyIcon1.BalloonTipTitle = "File Timer is not running";
                    notifyIcon1.BalloonTipText = "Please set or unpause the timer!\n(Click here to setup)";
                }
            }

            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Normal)
                notifyIcon1.Visible = false;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (passFormClosingMessage)
                return; // this have been set to occur if the program exits itself
            DialogResult dr = MessageBox.Show("Exit will cause the timer to stop, are you sure?", "Warning!", MessageBoxButtons.YesNo);
            switch(dr)
            {
                case DialogResult.Yes:
                    e.Cancel = false;
                    break;
                case DialogResult.No:
                    e.Cancel = true;
                    break;
            }
        }

        private void timer2_Tick(object sender, EventArgs e) // for visual purposes
        {
            if (timer2tick % 2 == 0)
                this.BackColor = Color.LightBlue;
            else
                this.BackColor = Color.LightSkyBlue;
            timer2tick++;
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Stop();

            if (timer2.Enabled)
                this.BackColor = Color.LightSkyBlue;
            timer2.Stop();
            sw.Stop();
            sw.Reset();

            if (comboBox1.SelectedIndex == 0)
            {
                string[] names = Enum.GetNames(typeof(Names));
                Random random = new Random();
                int randomEnum = random.Next(names.Length);
                var ret = Enum.Parse(typeof(Names), names[randomEnum]);
                button1.Text = ret.ToString();
            }
            else if (comboBox1.SelectedIndex == 1) // && this.openFileDialog1.FileName.EndsWith(".mp3"))
                button1.Text = "RunAlarm";
            else
                button1.Text = "ChooseFile";

            comboBox1.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = false;
            button2.Text = "Pause";
            checkBox1.Enabled = true;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            axWindowsMediaPlayer1.close();
            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        private void button4_Click(object sender, EventArgs e) // 10 min
        {
            if (timer1.Enabled || timer2.Enabled)
                return;
            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 10, 0);
        }

        private void button5_Click(object sender, EventArgs e) // 30 min
        {
            if (timer1.Enabled || timer2.Enabled)
                return;
            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 30, 0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled || timer2.Enabled)
                return;
            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 0, 0);
        }

        private void cbxDesign_DrawItem(object sender, DrawItemEventArgs e)
        {
            // By using Sender, one method could handle multiple ComboBoxes
            ComboBox cbx = sender as ComboBox;
            if (cbx != null)
            {
                // Always draw the background
                e.DrawBackground();

                // Drawing one of the items?
                if (e.Index >= 0)
                {
                    // Set the string alignment.  Choices are Center, Near and Far
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;

                    // Set the Brush to ComboBox ForeColor to maintain any ComboBox color settings
                    // Assumes Brush is solid
                    Brush brush = new SolidBrush(cbx.ForeColor);

                    // If drawing highlighted selection, change brush
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                        brush = SystemBrushes.HighlightText;

                    // Draw the string
                    e.Graphics.DrawString(cbx.Items[e.Index].ToString(), cbx.Font, brush, e.Bounds, sf);
                }
            }
        }

        private void timer3_Tick(object sender, EventArgs e) // 25sec interval
        {
            if (checkBox1.Checked)
                PreventSleep(); // every 25sec, if the "Prevent Sleep Mode" is checked, run this function to prevent sleep mode
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 8)
            {
                comboBox1.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = false;
                button2.Text = "Pause";
                checkBox1.Enabled = true;
                axWindowsMediaPlayer1.Ctlcontrols.stop();
                axWindowsMediaPlayer1.close();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                string[] names = Enum.GetNames(typeof(Names));
                Random random = new Random();
                int randomEnum = random.Next(names.Length);
                var ret = Enum.Parse(typeof(Names), names[randomEnum]);
                button1.Text = ret.ToString();
            }
            else if (comboBox1.SelectedIndex == 1)
                button1.Text = "RunAlarm";
            else
                button1.Text = "ChooseFile";
        }

        private void PreventSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
        }

        private void checkBox1_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            toolTip1.InitialDelay = 0;
            toolTip1.Show("Prevents the computer of going to sleep mode whenever the checkbox is ticked.", checkBox1);
        }

        private void dateTimePicker1_KeyDown(object sender, KeyEventArgs e)
        {
            if (button1.Enabled && e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
