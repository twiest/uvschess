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
        ChessGame _mainGame = null;

        string WhitePlayerName = string.Empty;
        string BlackPlayerName = string.Empty;
        
        List<AI> AvailableAIs = new List<AI>();
        

        public delegate void PlayCompletedHandler();
        protected delegate void PlayDelegate();

        public delegate void StringListParameterCallback(List<string> text);
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
                AddToHistory("Start of Game", line);

                chessBoardControl.ResetBoard(line);

                ChessState tmpState = new ChessState(line);
                if (tmpState.CurrentPlayerColor == ChessColor.White)
                {
                    radWhite.Checked = true;
                }
                else
                {
                    radBlack.Checked = true;
                }

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


                HistoryItem item = (HistoryItem)lstHistory.SelectedItem;
                string fenboard = item.fenboard;

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
            // TODO: CHANGE COLOR OF GUI SO USER KNOWS IT'S RUNNING


            RemoveHistoryAfterSelected();

            HistoryItem item = (HistoryItem)lstHistory.SelectedItem;

            _mainGame = new ChessGame(item.fenboard, WhitePlayerName, BlackPlayerName);

            DisableMenuItemsDuringPlay();
            DisableRadioBtnsAndComboBoxes();

            // Remove WinGui from the GuiChessBoard updates
            chessBoardControl.PieceMovedByHuman -= GuiChessBoardChangedByHuman;

            // Add the ChessGame to the GuiChessBoard updates
            chessBoardControl.PieceMovedByHuman += _mainGame.WhitePlayer_HumanMovedPieceEvent;
            chessBoardControl.PieceMovedByHuman += _mainGame.BlackPlayer_HumanMovedPieceEvent;

            // Add WinGui to the ChessGame updates
            _mainGame.GameUpdated += GameUpdated;

            _mainGame.StartGame();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_mainGame != null)
            {
                // Remove the ChessGame from the GuiChessBoard updates
                chessBoardControl.PieceMovedByHuman -= _mainGame.WhitePlayer_HumanMovedPieceEvent;
                chessBoardControl.PieceMovedByHuman -= _mainGame.BlackPlayer_HumanMovedPieceEvent;

                // Remove WinGui from the ChessGame updates
                _mainGame.GameUpdated -= GameUpdated;

                _mainGame.StopGameEarly();
                _mainGame = null;

                // Add WinGui to the GuiChessBoard updates
                chessBoardControl.PieceMovedByHuman += GuiChessBoardChangedByHuman;

                chessBoardControl.IsLocked = false;

                EnableMenuItemsAfterPlay();
                EnableRadioBtnsAndComboBoxes();
            }
        }

        private void clearHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            radWhite.Checked = true;
            radBlack.Checked = false;

            lstHistory.Items.Clear();

            AddToHistory("Start of Game", ChessState.FenStartState);
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
            HistoryItem item = (HistoryItem)lstHistory.SelectedItem;
            ChessState tmpState = new ChessState(item.fenboard);

            if (radWhite.Checked)
            {
                tmpState.CurrentPlayerColor = ChessColor.White;
            }
            else
            {
                tmpState.CurrentPlayerColor = ChessColor.Black;
            }

            item.fenboard = tmpState.ToFenBoard();

            lstHistory.Items[lstHistory.SelectedIndex] = item;
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

        private void DisableRadioBtnsAndComboBoxes()
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
            }
        }

        private void EnableRadioBtnsAndComboBoxes()
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
            }
        }
        private void DisableMenuItemsDuringPlay()
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
        private void EnableMenuItemsAfterPlay()
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
        public void GameUpdated(string playerColor, string nextMove, string fen)
        {
            UpdateWinGuiOnTimer.AddToHistory(playerColor + ": " + nextMove, fen);
        }

        public void GuiChessBoardChangedByHuman(ChessMove move)
        {
            HistoryItem item = (HistoryItem)lstHistory.SelectedItem;
            ChessState tmpState = new ChessState(item.fenboard);
            tmpState.MakeMove(move);

            if (radWhite.Checked)
            {
                tmpState.CurrentPlayerColor = ChessColor.White;
            }
            else
            {
                tmpState.CurrentPlayerColor = ChessColor.Black;
            }

            item.fenboard = tmpState.ToFenBoard();

            lstHistory.Items[lstHistory.SelectedIndex] = item;
        }

        private void RemoveHistoryAfterSelected()
        {
            int sel = lstHistory.SelectedIndex;

            if (sel < 0)
            {
                return;
            }
            while(lstHistory.Items.Count > sel + 1)
            {
                lstHistory.Items.RemoveAt(lstHistory.Items.Count-1);
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
            if (this.lblHalfMoves.InvokeRequired)
            {
                IntParameterCallback cb = new IntParameterCallback(SetHalfMoves);
                this.Invoke(cb, new object[] { halfmoves });
            }
            else
            {
                lblHalfMoves.Text = halfmoves.ToString();
            }
        }

        public void SetFullMoves(int fullmoves)
        {
            if (this.lblFullMoves.InvokeRequired)
            {
                IntParameterCallback cb = new IntParameterCallback(SetFullMoves);
                this.Invoke(cb, new object[] { fullmoves });
            }
            else
            {
                lblFullMoves.Text = fullmoves.ToString();
            }
        }

        public void AddToHistory(string message, string fenboards)
        {
            AddToHistory(new List<string>() { message }, new List<string>() { fenboards });
        }
    
        public void AddToHistory(List<string> messages, List<string> fenboards)
        {
            if (this.lstHistory.InvokeRequired)
            {
                this.Invoke(new TwoStringListParameterCallback(AddToHistory), new object[] { messages, fenboards });
            }
            else
            {
                List<HistoryItem> items = new List<HistoryItem>();

                for (int ix = 0; ix < messages.Count; ix++)
                {
                    items.Add(new HistoryItem(lstHistory.Items.Count.ToString() + ". " + messages[ix], fenboards[ix]));
                }

                lstHistory.BeginUpdate();
                lstHistory.Items.AddRange(items.ToArray());
                lstHistory.SelectedIndex = lstHistory.Items.Count - 1;
                lstHistory.EndUpdate();
            }
        }

        public void AddToMainLog(List<string> messages)
        {
            if (this.lstMainLog.InvokeRequired)
            {
                this.Invoke(new StringListParameterCallback(AddToMainLog), new object[] { messages });
            }
            else
            {
                lstMainLog.BeginUpdate();
                lstMainLog.Items.AddRange(messages.ToArray());
                lstMainLog.Items.Add("----" + lstMainLog.Items.Count.ToString() + "----" + messages.Count.ToString() + "----");

                if (chkBxAutoScrollMainLog.Checked)
                {
                    lstMainLog.SelectedIndex = lstMainLog.Items.Count - 1;
                    lstMainLog.ClearSelected();
                }

                lstMainLog.EndUpdate();
            }
        }

        public void AddToWhitesLog(List<string> messages)
        {
            if (this.lstWhitesLog.InvokeRequired)
            {
                this.Invoke(new StringListParameterCallback(AddToWhitesLog), new object[] { messages });
            }
            else
            {
                lstWhitesLog.BeginUpdate();
                lstWhitesLog.Items.AddRange(messages.ToArray());
                lstWhitesLog.Items.Add("----" + lstWhitesLog.Items.Count.ToString() + "----" + messages.Count.ToString() + "----");

                if (chkBxAutoScrollWhitesLog.Checked)
                {
                    lstWhitesLog.SelectedIndex = lstWhitesLog.Items.Count - 1;
                    lstWhitesLog.ClearSelected();
                }
                lstWhitesLog.EndUpdate();
            }
        }

        public void AddToBlacksLog(List<string> messages)
        {
            if (this.lstBlacksLog.InvokeRequired)
            {
                this.Invoke(new StringListParameterCallback(AddToBlacksLog), new object[] { messages });
            }
            else
            {
                lstBlacksLog.BeginUpdate();
                lstBlacksLog.Items.AddRange(messages.ToArray());
                lstBlacksLog.Items.Add("----" + lstBlacksLog.Items.Count.ToString() + "----" + messages.Count.ToString() + "----");

                if (chkBxAutoScrollBlacksLog.Checked)
                {
                    lstBlacksLog.SelectedIndex = lstBlacksLog.Items.Count - 1;
                    lstBlacksLog.ClearSelected();
                }

                lstBlacksLog.EndUpdate();
            }
        }
        #endregion
        
        private void lstHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            HistoryItem item = (HistoryItem)lstHistory.SelectedItem;
            ChessState tmpState = new ChessState(item.fenboard);

            if (tmpState.CurrentPlayerColor == ChessColor.White)
            {
                radWhite_SetChecked();
            }
            else
            {
                radBlack_SetChecked();
            }

            chessBoardControl.ResetBoard(new ChessBoard(item.fenboard));
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

        private void WinGui_FormClosing(object sender, FormClosingEventArgs e)
        {
            UpdateWinGuiOnTimer.StopGuiPolling();

            stopToolStripMenuItem_Click(null, null);
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
    }
}
