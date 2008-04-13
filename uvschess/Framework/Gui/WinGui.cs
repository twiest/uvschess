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
using System.Windows.Forms;
using UvsChess.Framework;

namespace UvsChess.Gui
{
    public partial class WinGui : Form
    {
        List<AI> AvailableAIs = new List<AI>();

        #region Constructors
        public WinGui()
        {
            InitializeComponent();

            UpdateWinGuiOnTimer.Gui = this;

            UpdateWinGuiOnTimer.ResetHistory(new ChessState());

            UpdateWinGuiOnTimer.PollGuiOnce();

            chessBoardControl.PieceMovedByHuman += PieceMovedByHuman_Changed;
        }

        private void WinGui_Load(object sender, EventArgs e)
        {
            DllLoader.SearchForAIs();

            foreach (AI ai in DllLoader.AvailableAIs)
            {
                cmbBlack.Items.Add(ai.ShortName);
                cmbWhite.Items.Add(ai.ShortName);
            }
            cmbBlack.SelectedIndex = 0;
            cmbWhite.SelectedIndex = 0;
        }
        #endregion    
        
        public void PieceMovedByHuman_Changed(ChessMove move)
        {
            UpdateWinGuiOnTimer.UpdateBoardBasedOnMove((ChessState)this.lstHistory.SelectedItem, move, this.radWhite.Checked);
        }

        #region File menu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.OpenStateFromDisk();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.SaveSelectedStateToDisk();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region Game menu
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.ResetHistory(new ChessState());
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.StartGame();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.StopGame();
        }

        private void clearHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.ResetHistory(new ChessState());
        }
        #endregion

        #region Help menu
        private void aboutUvsChessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutUvsChess about = new AboutUvsChess();
            about.ShowDialog();
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Preferences prefs = new Preferences();
            prefs.ShowDialog();
        }
        #endregion

        #region AISelector controls
        private void cmbWhite_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.UpdateWhitesName(this.cmbWhite.SelectedItem.ToString());
        }

        private void cmbBlack_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.UpdateBlacksName(this.cmbBlack.SelectedItem.ToString());
        }
        #endregion

        #region Update ChessState Info
        private void lstHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.UpdateBoardBasedOnLstHistory((ChessState)this.lstHistory.SelectedItem);
        }
        #endregion

        #region Log Tabs
        private void btnSaveMainLog_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.SaveMainLogToDisk();
        }

        private void btnSaveWhitesLog_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.SaveWhitesLogToDisk();
        }

        private void btnSaveBlacksLog_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.SaveBlacksLogToDisk();
        }

        private void btnClearMainLog_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.ClearMainLog();
        }

        private void btnClearWhitesLog_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.ClearWhitesLog();
        }

        private void btnClearBlacksLog_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.ClearBlacksLog();
        }
        #endregion

        private void WinGui_FormClosing(object sender, FormClosingEventArgs e)
        {            
            UpdateWinGuiOnTimer.ShutdownGuiEventLoop();
        }
    }
}
