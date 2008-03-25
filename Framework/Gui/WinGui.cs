using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UvsChess.Gui
{
    public partial class WinGui : Form
    {
        public WinGui()
        {
            InitializeComponent();
        }

        private void cmbBlack_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void WinGui_Load(object sender, EventArgs e)
        {
            DllLoader.SearhForAIs();

            foreach (AI ai in DllLoader.AvailableAIs)
            {
                cmbBlack.Items.Add(ai.ShortName);
                cmbWhite.Items.Add(ai.ShortName);
            }
            cmbBlack.SelectedIndex = 0;
            cmbWhite.SelectedIndex = 0;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstHistory.Items.Clear();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
            }

        }
    }
}