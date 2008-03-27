using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace UvsChess.Gui
{
    public partial class WinGui : Form
    {
        #region Members

        ManualResetEvent pieceMovedEvent = new ManualResetEvent(true);

        ChessState _mainChessState = null;
        ChessPlayer WhitePlayer = null;
        ChessPlayer BlackPlayer = null;
        bool IsRunning = false;
        ChessMove humanMove = null;
        List<AI> AvailableAIs = new List<AI>();


        public delegate void PlayCompletedHandler();
        public event PlayCompletedHandler PlayCompleted;
        protected delegate void PlayDelegate();

        //These are for the threading involved with telling the AI to EndTurn
        //int TurnWaitTime = 5000;// time in milliseconds for each turn. 
        ChessPlayer thread_player = null;
        ChessMove thread_move = null;
        ChessBoard thread_board = null;
        TimeSpan thread_time = TimeSpan.MinValue;

        public delegate void StringParameterCallback(string text);
        public delegate void TwoStringParameterCallback(string text1,string text2);
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

            WhitePlayer = new ChessPlayer(ChessColor.White);
            BlackPlayer = new ChessPlayer(ChessColor.Black);

            chessBoardControl.PieceMovedByHuman += HumanMovedPieceEvent;

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
            //TODO: change color of gui so user knows it's not running
        }
        #endregion

        #region History menu
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstHistory.Items.Clear();
        }
        #endregion

        #region AISelector controls
        private void radWhite_CheckedChanged(object sender, EventArgs e)
        {

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
        private void cmbWhite_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhitePlayer.AIName = cmbWhite.SelectedItem.ToString();

        }
        private void cmbBlack_SelectedIndexChanged(object sender, EventArgs e)
        {

            BlackPlayer.AIName = cmbBlack.SelectedItem.ToString();
        }
        #endregion

        #region Game play methods and events
        public IAsyncResult StartGame()
        {
            DisableRadioBtnsAndComboBoxes();

            PlayDelegate pd = new PlayDelegate(Play); //Start a new thread from this method
            return pd.BeginInvoke(new AsyncCallback(EndPlay), null);
        }
        private void EndPlay(IAsyncResult ar)
        {
            EnableRadioBtnsAndComboBoxes();

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

        void Play()
        {
            //This method run in its own thread.

            //Load the AI if it isn't loaded already
            LoadAI(WhitePlayer);
            LoadAI(BlackPlayer);

            IsRunning = true;
            //ChessMove currentMove = null;
            chessBoardControl.IsLocked = true;

            while (IsRunning)
            {
                if (mainChessState.CurrentPlayerColor == ChessColor.White)
                {
                    DoNextMove(WhitePlayer, BlackPlayer);
                }
                else
                {
                    DoNextMove(BlackPlayer, WhitePlayer);
                }
            }

            Logger.Log("Game Over");
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
                nextMove = GetNextAIMove(player, mainChessState.CurrentBoard.Clone());

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

                    nextMove = GetNextHumanMove();

                    chessBoardControl.IsLocked = true;

                    newstate = mainChessState.Clone();
                    newstate.MakeMove(nextMove);
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
        ChessMove GetNextHumanMove()
        {
            //chessBoardControl.IsLocked = false;

            pieceMovedEvent.Reset();
            pieceMovedEvent.WaitOne();            

            return humanMove;
        }
        void HumanMovedPieceEvent(ChessMove move)
        {
            if (IsRunning)
            {
                Logger.Log("Human move:");
                humanMove = move;
                pieceMovedEvent.Set();
            }
            else
            {
                Logger.Log("Pregame setup:");
                mainChessState.CurrentBoard.MakeMove(move);
                Logger.Log(mainChessState.CurrentBoard.ToFenBoard());
            }

        } 

        ChessMove GetNextAIMove(ChessPlayer player, ChessBoard board)
        {
            
            thread_player = player;
            thread_board = board;

            //Add threading here. Wait n seconds then call ChessAI.EndTurn()
            ThreadStart job = new ThreadStart(threadedNextMove);
            Thread thread = new Thread(job);

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddMilliseconds(UserPrefs.Time);
            thread.Start();

            //while (player.AI.IsRunning && (startTime.AddMilliseconds(UserPrefs.Time)) < DateTime.Now)
            while (player.AI.IsRunning && (DateTime.Now < endTime))
            {
                Thread.Sleep(100);
            }

            player.AI.EndTurn();
            thread.Join();

            //thread_time = DateTime.Now - startTime;
            thread_time = DateTime.Now.Subtract(startTime);

            return thread_move;
        }

        private void threadedNextMove()
        {
            ChessPlayer player = thread_player;
            thread_move = player.AI.GetNextMove(thread_board, player.Color);
        }
        #endregion

        /// <summary>
        /// Loads the AI for the chess player. If the AI is already loaded, it will just return.
        /// </summary>
        /// <param name="player"></param>
        void LoadAI(ChessPlayer player)
        {
            if ((player.AIName == null) || (player.AIName == "Human"))
            {
                return;
            }
            else if (player.AI != null)
            {
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
                this.Invoke(new TwoStringParameterCallback(AddToHistory), new object[] { message,fenboard });
            }
            else
            {
                HistoryItem item = new HistoryItem(message,fenboard);
                lstHistory.Items.Add(item);
                
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
    }
}
