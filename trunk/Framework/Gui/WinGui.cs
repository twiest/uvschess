/******************************************************************************
* The MIT License
* Copyright (c) 2008 Rusty Howell, Thomas Wiest
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
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

        ChessState _mainChessState = null;
        ChessPlayer WhitePlayer = null;
        ChessPlayer BlackPlayer = null;
        string WhitePlayerName = string.Empty;
        string BlackPlayerName = string.Empty;
        bool IsRunning = false;
        List<AI> AvailableAIs = new List<AI>();
        Thread timerThread = null;

        public delegate void PlayCompletedHandler();
        protected delegate void PlayDelegate();

        public delegate void StringParameterCallback(string text);
        public delegate void TwoStringParameterCallback(string text1,string text2);
        public delegate string CmbBoxParamaterCallback(ComboBox cmb);
        private delegate void RadioBtnParameterCallback(RadioButton rad);
        delegate void NoParameterCallback();
        delegate void IntParameterCallback(int i);


        #endregion

        #region Properties
        private ChessState mainChessState
        {
            get { return _mainChessState; }
            set { _mainChessState = value; }
        }
        #endregion


        #region Constructors
        public WinGui()
        {
            InitializeComponent();

            mainChessState = new ChessState();

            Logger.GuiWriteLine = AddToMainOutput;
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

                mainChessState = new ChessState(line);
                chessBoardControl.ResetBoard(mainChessState.CurrentBoard);
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
                //MessageBox.Show("Not implemented yet");

                Logger.Log("Saving board to " + saveFileDialog1.FileName);

                string fenboard = mainChessState.ToFenBoard();
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
            IsRunning = false;
            chessBoardControl.ResetBoard();
        }
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsRunning = true;

            // CHANGE COLOR OF GUI SO USER KNOWS IT'S RUNNING

            StartGame();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsRunning = false;
            chessBoardControl.IsLocked = false;

            WhitePlayer.EndTurnEarly();
            BlackPlayer.EndTurnEarly();

            timerThread.Join();

            EnableRadioBtnsAndComboBoxes();

            WhitePlayer = null;
            BlackPlayer = null;
            //TODO: change color of gui so user knows it's not running
        }

        private void clearHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {

            lstHistory.Items.Clear();
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
        private void radWhite_CheckedChanged(object sender, EventArgs e)
        {
            mainChessState.CurrentPlayerColor = ChessColor.White;
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
        #endregion

        #region Game play methods and events
        public void StartGame()
        {
            DisableRadioBtnsAndComboBoxes();

            timerThread = new Thread(Play);
            timerThread.Start();
        }

        private void EndPlay(IAsyncResult ar)
        {
            

            //throw new NotImplementedException();

            //try
            //{
            //    //AsyncResult result = (AsyncResult)ar;
            //    //PlayDelegate pd = (PlayDelegate)result.AsyncDelegate;
            //    //pd.EndInvoke(ar);
            //    //OnPlayCompleted();
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine("Chess->MainForm->EndPlay: " + ex.Message);
            //}
        }
        private void RemoveHistory()
        {
            int sel = lstHistory.SelectedIndex;
            
            while(lstHistory.Items.Count > sel)
            {
                lstHistory.Items.RemoveAt(lstHistory.Items.Count-1);
            }
        }

        void Play()
        {
            //This method run in its own thread.

            // Setup the current state so that it's the same as the gui chess board
            mainChessState.CurrentBoard = chessBoardControl.Board;
                        
            // Setup the players based on the combo boxes
            WhitePlayer = new ChessPlayer(ChessColor.White);
            BlackPlayer = new ChessPlayer(ChessColor.Black);

            WhitePlayer.AIName = WhitePlayerName;
            BlackPlayer.AIName = BlackPlayerName;

            //Load the AI if it isn't loaded already
            LoadAI(WhitePlayer);
            LoadAI(BlackPlayer);

            if (WhitePlayer.IsHuman)
            {
                // Hook up the GUI chess event for White
                chessBoardControl.PieceMovedByHuman += WhitePlayer.HumanMovedPieceEvent;
            }

            if (BlackPlayer.IsHuman)
            {
                // Hook up the GUI chess event for Black
                chessBoardControl.PieceMovedByHuman += BlackPlayer.HumanMovedPieceEvent;
            }


            IsRunning = true;
            //ChessMove currentMove = null;
            chessBoardControl.IsLocked = true;
            AddToHistory("Start Of Game", mainChessState.ToFenBoard());

            while (IsRunning)
            {
                if (mainChessState.CurrentPlayerColor == ChessColor.White)
                {
                    //change radion button selection
                    SelectRadio(radWhite);
                    DoNextMove(WhitePlayer, BlackPlayer);
                }
                else
                {
                    SelectRadio(radBlack);
                    DoNextMove(BlackPlayer, WhitePlayer);
                }
            }

            if (WhitePlayer.IsHuman)
            {
                // Take away the event for White since the game is over
                chessBoardControl.PieceMovedByHuman -= WhitePlayer.HumanMovedPieceEvent;
            }

            if (BlackPlayer.IsHuman)
            {
                // Take away the event for Black since the game is over
                chessBoardControl.PieceMovedByHuman -= BlackPlayer.HumanMovedPieceEvent;
            }

            //Logger.Log("Game Over");
            IsRunning = false; //This is redundant, but it makes the code clear
            chessBoardControl.IsLocked = false;
        }

        void DoNextMove(ChessPlayer player, ChessPlayer opponent)
        {
            ChessMove nextMove = null;
            //DateTime start = DateTime.Now;
            bool isValidMove = false;
            ChessState newstate = null;


            if (player.IsComputer)
            {
                nextMove = player.GetNextMove(mainChessState.CurrentBoard);

                if (!IsRunning)
                {
                    // if we're not running, leave the method
                    return;
                }

                if (nextMove.Flag != ChessFlag.Stalemate)
                {
                    newstate = mainChessState.Clone();
                    newstate.MakeMove(nextMove);
                    if (opponent.IsComputer)
                    {
                        isValidMove = opponent.AI.IsValidMove(newstate);
                    }
                    else
                    {
                        isValidMove = true;
                    }
                }

                //TODO

                //isValidMove = isValidMove && !isOverTime(player, thread_time, TurnWaitTime);
                //isValidMove = isValidMove && !isOverTime(player, thread_time, PreferencesGUI.TurnLength);
                //isValidMove = isValidMove && !isOverTime(player, thread_time, UserPrefs.Time);

            }
            else //player is human
            {
                while (!isValidMove)
                {
                    chessBoardControl.IsLocked = false;

                    nextMove = player.GetNextMove(mainChessState.CurrentBoard.Clone());

                    if (! IsRunning)
                    {
                        // if we're not running, leave the method
                        return;
                    }

                    chessBoardControl.IsLocked = true;

                    newstate = mainChessState.Clone();
                    newstate.MakeMove(nextMove);

                    // TODO: Fix NRE here when a human is playing another human
                    isValidMove = opponent.AI.IsValidMove(newstate);
                }
            }

            if ( (isValidMove) && 
                 (nextMove.Flag == ChessFlag.Checkmate) )
            {
                    // Checkmate on a valid move has been signaled.
                    IsRunning = false;
                    Logger.Log(String.Format("{0} has signaled that the game is a stalemate.",
                                        (player.Color == ChessColor.Black) ? "Black" : "White"));
            }
            else if (isValidMove)
            {
                //update the board
                chessBoardControl.ResetBoard(newstate.CurrentBoard);
                AddToHistory(player.Color.ToString() + ": " + nextMove.ToString(), newstate.ToFenBoard());

                //update mainChessState for valid 
                mainChessState = newstate;

                if (player.Color == ChessColor.Black)
                {
                    mainChessState.FullMoves++;//Increment fullmoves after black's turn
                    SetFullMoves(mainChessState.FullMoves);
                }

                //Determine if a pawn was moved or a kill was made.
                if (ResetHalfMove())
                {
                    mainChessState.HalfMoves = 0;
                }
                else
                {
                    mainChessState.HalfMoves++;
                }
                SetHalfMoves(mainChessState.HalfMoves);
                Logger.Log(mainChessState.ToFenBoard());

            }
            else
            {
                // It is either a stalemate or an invalid move. Either way, we're done running.
                IsRunning = false;

                if (nextMove.Flag == ChessFlag.Stalemate)
                {
                    // A stalemate has occurred.
                    Logger.Log(String.Format("{0} has signaled that the game is a stalemate.",
                                        (player.Color == ChessColor.Black) ? "Black" : "White"));
                }
                else
                {
                    Logger.Log(String.Format("{0} has signaled that {1} returned an invalid move returned, therefore {1} loses!",
                                   (player.Color == ChessColor.Black) ? "White" : "Black",
                                   (player.Color == ChessColor.Black) ? "Black" : "White"));
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


        //This method checks if a pawn was moved or a kill was made.
        private bool ResetHalfMove()
        {
            ChessMove move = mainChessState.PreviousMove;
            //Check for a pawn move
            ChessPiece piece = mainChessState.PreviousBoard[move.From.X, move.From.Y];
            if ((piece == ChessPiece.WhitePawn) || (piece == ChessPiece.BlackPawn))
            {
                return true;
            }

            //Check for a kill
            piece = mainChessState.PreviousBoard[move.To.X, move.To.Y];
            if (piece != ChessPiece.Empty)
            {
                return true;
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Loads the AI for the chess player. If the AI is already loaded, it will just return.
        /// </summary>
        /// <param name="player"></param>
        void LoadAI(ChessPlayer player)
        {
            if (player.IsHuman)
            {
                // Player is a human, so we don't need to load an AI.
                return;
            }
            else if (player.AI != null)
            {
                // AI has already been loaded, so return
                return;
            }

            AI tmpAI = null;

            foreach (AI t in DllLoader.AvailableAIs)
            {
                if (t.ShortName == player.AIName)
                {
                    tmpAI = t;
                    break;
                }
            }
            System.Reflection.Assembly assem = System.Reflection.Assembly.LoadFile(tmpAI.FileName);
            IChessAI ai = (IChessAI)assem.CreateInstance(tmpAI.FullName);
            player.AI = ai;

        }

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
    
        public void AddToHistory(string message,string fenboard)
        {
            if (this.lstHistory.InvokeRequired)
            {
                this.Invoke(new TwoStringParameterCallback(AddToHistory), new object[] { message, fenboard });
            }
            else
            {
                HistoryItem item = new HistoryItem(message,fenboard);
                lstHistory.Items.Add(item);
                lstHistory.SelectedIndex = lstHistory.Items.Count - 1;
            }
        }

        public void AddToMainOutput(string message)
        {
            if (this.lstMainOutput.InvokeRequired)
            {
                this.Invoke(new StringParameterCallback(AddToMainOutput), new object[] { message });
            }
            else
            {
                lstMainOutput.Items.Add(message);
                lstMainOutput.SelectedIndex = lstMainOutput.Items.Count - 1;
                lstMainOutput.ClearSelected();
            }
        }
        #endregion
        
        private void lstHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            HistoryItem item = (HistoryItem)lstHistory.SelectedItem;
            Logger.Log(string.Format("clicked on: {0} - {1}",item,item.fenboard));

            mainChessState = new ChessState(item.fenboard);
            chessBoardControl.ResetBoard(mainChessState.CurrentBoard);
            
           
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

        private void radBlack_CheckedChanged(object sender, EventArgs e)
        {
            mainChessState.CurrentPlayerColor = ChessColor.Black;
        }

        private void WinGui_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopToolStripMenuItem_Click(null, null);
        }
    }
}
