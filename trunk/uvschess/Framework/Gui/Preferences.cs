/******************************************************************************
* The MIT License
* Copyright (c) 2008 Rusty Howell, Thomas Wiest
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest  twiest@users.sourceforge.net
//		Rusty Howell  rhowell@users.sourceforge.net

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace UvsChess.Gui
{
    public partial class Preferences : Form
    {
        #region Members

        private const string TIME = "time";
        private const string GRACEPERIOD = "graceperiod";

        private static int time_default = 5000;
        private static int grace_default = 1000;

        private static int _time = 5000;
        private static int _grace = 1000;

        private static string inifile = AppDomain.CurrentDomain.BaseDirectory + "UvsChess.ini";

        #endregion

        #region Properties

        public static int Time
        {
            get { return 
                _time; }
            set { _time = value; }
        }
        public static int GracePeriod
        {
            get { return _grace; }
            set { _grace = value; }
        }
        #endregion

        #region Contructors

        public Preferences()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void Preferences_Load(object sender, EventArgs e)
        {
            LoadFromFile();

            txtTime.Text = Time.ToString();
            txtGrace.Text = GracePeriod.ToString();
        }
        public static void LoadFromFile()
        {
            if (!File.Exists(inifile))
            {
                Time = time_default;
                GracePeriod = grace_default;
                return;
            }
            try
            {
                StreamReader infile = new StreamReader(inifile);

                string line = infile.ReadLine();
                while (line != null)
                {
                    if (line == string.Empty)
                    {
                        line = infile.ReadLine();
                        continue;
                    }
                    string[] sections = line.Split('=');
                    switch (sections[0])
                    {
                        case TIME:
                            Time = Convert.ToInt32(sections[1]);
                            break;
                        case GRACEPERIOD:
                            GracePeriod = Convert.ToInt32(sections[1]);
                            break;
                    }
                    line = infile.ReadLine();
                }
                infile.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                Time = time_default;
                GracePeriod = grace_default;
            }
        }
        public void SavePreferences()
        {

            StreamWriter outfile = new StreamWriter(inifile);
            Time = Convert.ToInt32(txtTime.Text);
            GracePeriod = Convert.ToInt32(txtGrace.Text);
            outfile.WriteLine("{0}={1}", TIME, Time);
            outfile.WriteLine("{0}={1}", GRACEPERIOD, GracePeriod);
            outfile.Close();
        }

        private void Preferences_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePreferences();
        }

        #endregion

        private void btnSavePrefs_Click(object sender, EventArgs e)
        {
            this.Close();//this should call FormClosing event, which should save prefs
        }
    }
}
