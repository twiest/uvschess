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
using System.Threading;

using UvsChess.Framework;

namespace UvsChess.Gui
{
    public partial class WinGui : Form
    {
        #region Members        
        Object _startStopGame_LockObj = new object();

        public string WhitePlayerName = string.Empty;
        public string BlackPlayerName = string.Empty;
        
        List<AI> AvailableAIs = new List<AI>();
        

        public delegate void PlayCompletedHandler();
        protected delegate void PlayDelegate();

        
        public delegate void StringParameterCallback(string text);
        public delegate void TwoStringListParameterCallback(List<string> text1, List<string> text2);
        
        public delegate string CmbBoxParamaterCallback(ComboBox cmb);
        private delegate void RadioBtnParameterCallback(RadioButton rad);
        delegate void NoParameterCallback();
        delegate void IntParameterCallback(int i);
        #endregion

        #region Constructors
        public WinGui()
        {
            InitializeComponent();

            UpdateWinGuiOnTimer.Gui = this;
            UpdateWinGuiOnTimer.PollGuiOnce();

            // Setup history lstBox
            clearHistoryToolStripMenuItem_Click(null, null);

            chessBoardControl.PieceMovedByHuman += GuiChessBoardChangedByHuman;
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

        #endregion             

        #region File menu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "FEN files (*.fen)|*.fen|All files (*.*)| *.*";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                Logger.Log("Resetting chess board");
                StreamReader reader = new StreamReader(openFileDialog1.FileName);
                string line = reader.ReadLine();

                lstHistory.Items.Clear();
                //UpdateWinGuiOnTimer.ClearHistory();

                ChessState tmpState = new ChessState(line);

                UpdateWinGuiOnTimer.AddToHistory(tmpState);

                reader.Close();                
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "FEN files (*.fen) | *.fen";
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);

                Logger.Log("Saving board to: " + saveFileDialog1.FileName);


                ChessState item = (ChessState)lstHistory.SelectedItem;
                string fenboard = item.ToFenBoard();

                writer.WriteLine(fenboard);
                writer.Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region Game menu
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearHistoryToolStripMenuItem_Click(null, null);
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.SwitchWinGuiMode(true);
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateWinGuiOnTimer.SwitchWinGuiMode(false);
        }

        private void clearHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstHistory.Items.Clear();
            //UpdateWinGuiOnTimer.ClearHistory();

            UpdateWinGuiOnTimer.AddToHistory(new ChessState());
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
        private void radWhiteBlack_CheckedChanged(object sender, EventArgs e)
        {
            ChessState tmpState = (ChessState)lstHistory.SelectedItem;
            //ChessState tmpState = new ChessState(item.fenboard);

            if (radWhite.Checked)
            {
                tmpState.CurrentPlayerColor = ChessColor.White;
            }
            else
            {
                tmpState.CurrentPlayerColor = ChessColor.Black;
            }

            //item.fenboard = tmpState.ToFenBoard();

            lstHistory.Items[lstHistory.SelectedIndex] = tmpState;
        }

        private void radWhite_SetChecked()
        {
            if (this.radWhite.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(radWhite_SetChecked), null);
            }
            else
            {
                this.radWhite.Checked = true;
            }
        }

        private void radBlack_SetChecked()
        {
            if (this.radBlack.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(radBlack_SetChecked), null);
            }
            else
            {
                this.radBlack.Checked = true;
            }
        }

        public void DisableRadioBtnsAndComboBoxes()
        {
            if (this.radBlack.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(DisableRadioBtnsAndComboBoxes), null);
            }
            else
            {
                this.radBlack.Enabled = false;
                this.radWhite.Enabled = false;
                this.cmbBlack.Enabled = false;
                this.cmbWhite.Enabled = false;
                this.numFullMoves.Enabled = false;
                this.numHalfMoves.Enabled = false;
            }
        }

        public void EnableRadioBtnsAndComboBoxes()
        {
            if (this.radBlack.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(EnableRadioBtnsAndComboBoxes), null);
            }
            else
            {
                this.radBlack.Enabled = true;
                this.radWhite.Enabled = true;
                this.cmbBlack.Enabled = true;
                this.cmbWhite.Enabled = true;
                this.numFullMoves.Enabled = true;
                this.numHalfMoves.Enabled = true;
            }
        }

        public void DisableMenuItemsDuringPlay()
        {
            if (this.radBlack.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(DisableMenuItemsDuringPlay), null);
            }
            else
            {
                startToolStripMenuItem.Enabled = false;
                clearHistoryToolStripMenuItem.Enabled = false;
                newToolStripMenuItem.Enabled = false;

                openToolStripMenuItem.Enabled = false;
                saveToolStripMenuItem.Enabled = false;

                stopToolStripMenuItem.Enabled = true;

            }
        }
        public void EnableMenuItemsAfterPlay()
        {
            if (this.radBlack.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(EnableMenuItemsAfterPlay), null);
            }
            else
            {
                startToolStripMenuItem.Enabled = true;
                clearHistoryToolStripMenuItem.Enabled = true;
                newToolStripMenuItem.Enabled = true;

                openToolStripMenuItem.Enabled = true;
                saveToolStripMenuItem.Enabled = true;

                stopToolStripMenuItem.Enabled = false;
            }
        }
        #endregion

        #region Game play methods and events
        public void OnChessGameDeclareResults(string results)
        {
            UpdateWinGuiOnTimer.DeclareResults(results);
            UpdateWinGuiOnTimer.SwitchWinGuiMode(false);
        }

        public void OnChessGameUpdated(ChessState state)
        {
            UpdateWinGuiOnTimer.AddToHistory(state);
        }

        public void GuiChessBoardChangedByHuman(ChessMove move)
        {
            ChessState tmpState = (ChessState)lstHistory.SelectedItem;
            //ChessState tmpState = new ChessState(item.fenboard);
            tmpState.MakeMove(move);
            tmpState.PreviousMove = null;
            tmpState.PreviousBoard = null;

            if (radWhite.Checked)
            {
                tmpState.CurrentPlayerColor = ChessColor.White;
            }
            else
            {
                tmpState.CurrentPlayerColor = ChessColor.Black;
            }

            //item.fenboard = tmpState.ToFenBoard();

            // This causes the "selected index changed event"
            //lstHistory.Items[lstHistory.SelectedIndex] = tmpState;
            lstHistory.SelectedItem = tmpState;

            // Force update the lstHistory and the GuiChessBoard
            lstHistory_SelectedIndexChanged(null, null);
        }

        public void DisableHistoryWindowClicking()
        {
            if (this.lstHistory.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(DisableHistoryWindowClicking), null);
            }
            else
            {
                lstHistory.Enabled = false;
            }
        }

        public void EnableHistoryWindowClicking()
        {
            if (this.lstHistory.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(EnableHistoryWindowClicking), null);
            }
            else
            {
                lstHistory.Enabled = true;
            }
        }

        public void RemoveHistoryAfterSelected()
        {
            if (this.lstHistory.InvokeRequired)
            {
                this.Invoke(new NoParameterCallback(RemoveHistoryAfterSelected), null);
            }
            else
            {
                int sel = lstHistory.SelectedIndex;

                if (sel < 0)
                {
                    return;
                }
                while (lstHistory.Items.Count > sel + 1)
                {
                    lstHistory.Items.RemoveAt(lstHistory.Items.Count - 1);
                }
            }
        }

        private bool isOverTime(ChessPlayer player, TimeSpan time, int limit)
        {
            bool isovertime = false;
            Console.WriteLine("{0} stopped after {1:0} ms", player.AIName, time.TotalMilliseconds);

            //do we need/want a buffer ?
            limit += 1000;  //time buffer

            if ((time.TotalMilliseconds > (double)limit) && (player.IsComputer))
            {
                isovertime = true;
                Logger.Log("Too Much Time: Move timeout occurred!");
            }
            return isovertime;
        }
        #endregion



        #region GUI update methods
        public void SetHalfMoves(int halfmoves)
        {
            if (this.numHalfMoves.InvokeRequired)
            {
                IntParameterCallback cb = new IntParameterCallback(SetHalfMoves);
                this.Invoke(cb, new object[] { halfmoves });
            }
            else
            {
                //lblHalfMoves.Text = halfmoves.ToString();
                numHalfMoves.Value = halfmoves;
            }
        }

        public void SetFullMoves(int fullmoves)
        {
            if (this.numFullMoves.InvokeRequired)
            {
                IntParameterCallback cb = new IntParameterCallback(SetFullMoves);
                this.Invoke(cb, new object[] { fullmoves });
            }
            else
            {
                //lblFullMoves.Text = fullmoves.ToString();
                numFullMoves.Value = fullmoves;
            }
        }


        //public void AddToHistory(string message, string fenboards)
        //{
        //    AddToHistory(new List<string>() { message }, new List<string>() { fenboards });
        //}
    
        //public void AddToHistory(List<string> messages, List<string> fenboards)
        //{
        //    if (this.lstHistory.InvokeRequired)
        //    {
        //        this.Invoke(new TwoStringListParameterCallback(AddToHistory), new object[] { messages, fenboards });
        //    }
        //    else
        //    {
        //        List<HistoryItem> items = new List<HistoryItem>();

        //        for (int ix = 0; ix < messages.Count; ix++)
        //        {
        //            items.Add(new HistoryItem(lstHistory.Items.Count.ToString() + ". " + messages[ix], fenboards[ix]));
        //        }

        //        lstHistory.BeginUpdate();
        //        lstHistory.Items.AddRange(items.ToArray());
        //        lstHistory.SelectedIndex = lstHistory.Items.Count - 1;
        //        lstHistory.EndUpdate();
        //    }
        //}




        #endregion
        
        private void lstHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChessState tmpState = (ChessState)lstHistory.SelectedItem;
            //ChessState tmpState = new ChessState(item.fenboard);

            if (tmpState.CurrentPlayerColor == ChessColor.White)
            {
                radWhite_SetChecked();
            }
            else
            {
                radBlack_SetChecked();
            }

            SetFullMoves(tmpState.FullMoves);
            SetHalfMoves(tmpState.HalfMoves);

            chessBoardControl.ResetBoard(tmpState.CurrentBoard, tmpState.PreviousMove);
        }

        private void SelectRadio(RadioButton rad)
        {
            if (rad.InvokeRequired)
            {
                this.Invoke(new RadioBtnParameterCallback(SelectRadio), new object[] { rad });
            }
            else
            {
                rad.Checked = true;
            }
        }

        private void cmbWhite_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhitePlayerName = cmbWhite.SelectedItem.ToString();
        }

        private void cmbBlack_SelectedIndexChanged(object sender, EventArgs e)
        {
            BlackPlayerName = cmbBlack.SelectedItem.ToString();
        }

        private void btnSaveMainLog_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files (*.txt) | *.txt";
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);

                Logger.Log("Saving Main Log to: " + saveFileDialog1.FileName);

                foreach (string curLine in lstMainLog.Items)
                {
                    writer.WriteLine(curLine);
                }

                writer.Close();
            }
        }

        private void btnSaveWhitesLog_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files (*.txt) | *.txt";
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);

                Logger.Log("Saving White AI's Log to: " + saveFileDialog1.FileName);

                foreach (string curLine in lstWhitesLog.Items)
                {
                    writer.WriteLine(curLine);
                }

                writer.Close();
            }
        }

        private void btnSaveBlacksLog_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files (*.txt) | *.txt";
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);

                Logger.Log("Saving Black AI's Log to: " + saveFileDialog1.FileName);

                foreach (string curLine in lstBlacksLog.Items)
                {
                    writer.WriteLine(curLine);
                }

                writer.Close();
            }
        }

        private void btnClearMainLog_Click(object sender, EventArgs e)
        {
            lstMainLog.Items.Clear();
        }

        private void btnClearWhitesLog_Click(object sender, EventArgs e)
        {
            lstWhitesLog.Items.Clear();
        }

        private void btnClearBlacksLog_Click(object sender, EventArgs e)
        {
            lstBlacksLog.Items.Clear();
        }

        private void WinGui_FormClosing(object sender, FormClosingEventArgs e)
        {
            UpdateWinGuiOnTimer.StopGuiPolling();

            stopToolStripMenuItem_Click(null, null);
        }

        private void numHalfMoves_ValueChanged(object sender, EventArgs e)
        {
            ChessState state = (ChessState)lstHistory.SelectedItem;
            state.HalfMoves = Convert.ToInt32(numHalfMoves.Value);
        }

        private void numFullMoves_ValueChanged(object sender, EventArgs e)
        {
            ChessState state = (ChessState)lstHistory.SelectedItem;
            state.FullMoves = Convert.ToInt32(numFullMoves.Value);
        }
    }
}
