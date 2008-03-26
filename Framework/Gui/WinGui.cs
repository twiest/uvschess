using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace UvsChess.Gui
{
    public partial class WinGui : Form
    {
        #region Members

        ChessState mainChessState = null;
        ChessPlayer WhitePlayer = null;
        ChessPlayer BlackPlayer = null;
        bool IsRunning = false;
        ChessMove humanMove = null;
        List<AI> AvailableAIs = new List<AI>();


        public delegate void PlayCompletedHandler();
        public event PlayCompletedHandler PlayCompleted;
        protected delegate void PlayDelegate();

        #endregion

        #region Properties

        #endregion

        #region Constructors
        public WinGui()
        {
            InitializeComponent();


            mainChessState = new ChessState();

            WhitePlayer = new ChessPlayer(ChessColor.White);
            BlackPlayer = new ChessPlayer(ChessColor.Black);
        }

        #endregion


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

        

        #region File menu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "FEN files (*.fen)|*.fen|All files (*.*)| *.*";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                Log("Resetting chess board");
                StreamReader reader = new StreamReader(openFileDialog1.FileName);
                string line = reader.ReadLine();

                ChessState newstate = new ChessState(line);
                chessBoardControl.ResetBoard(newstate.CurrentBoard);
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

                Log("Saving board to " + saveFileDialog1.FileName);

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
            MessageBox.Show("Not implemented yet");
        }
        #endregion

        #region History menu
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstHistory.Items.Clear();
        }
        #endregion


        private void radWhite_CheckedChanged(object sender, EventArgs e)
        {

        }

        #region Game play methods and events
        public IAsyncResult StartGame()
        {
            //TODO: disable radio buttons
            PlayDelegate pd = new PlayDelegate(Play); //Start a new thread from this method
            return pd.BeginInvoke(new AsyncCallback(EndPlay), null);
        }
        private void EndPlay(IAsyncResult ar)
        {

            throw new NotImplementedException();

            try
            {
                //AsyncResult result = (AsyncResult)ar;
                //PlayDelegate pd = (PlayDelegate)result.AsyncDelegate;
                //pd.EndInvoke(ar);
                //OnPlayCompleted();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Chess->MainForm->EndPlay: " + ex.Message);
            }
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

            Log("Game Over");
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

            if (isValidMove)//update the board
            {
                chessBoardControl.ResetBoard(newstate.CurrentBoard);
            }
            else //or reset the board
            {
                chessBoardControl.ResetBoard(mainChessState.CurrentBoard);
            }


            AddToHistory(player.Color.ToString() + ": " + nextMove.ToString(), newstate.ToFenBoard());

            if (isValidMove)
            {

                //update mainChessState for valid 
                mainChessState = newstate;

                if (player.Color == ChessColor.Black)
                {
                    mainChessState.FullMoves++;//Increment fullmoves after black's turn
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

                Log(mainChessState.ToFenBoard());

            }
            else
            {
                IsRunning = false;
                Log(String.Format("Invalid move returned, {0} loses!", (player.Color == ChessColor.Black) ? "Black" : "White"));

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
                Log("Too Much Time: Move timeout occurred!");
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
            //pieceMovedEvent.Reset();
            //pieceMovedEvent.WaitOne();
            throw new NotImplementedException();
            
            return humanMove;
        }

        ChessMove GetNextAIMove(ChessPlayer player, ChessBoard board)
        {
            throw new NotImplementedException();
            return null;
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
        public void AddToHistory(string message)
        {
            AddToHistory(message, string.Empty);
        }

        //TODO
        public void AddToHistory(string message, string fenboard)
        {
            
            lstHistory.Items.Add(message);
        }

        public static void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        private void cmbWhite_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhitePlayer.AIName = cmbWhite.SelectedItem.ToString();

        }
        private void cmbBlack_SelectedIndexChanged(object sender, EventArgs e)
        {

            BlackPlayer.AIName = cmbBlack.SelectedItem.ToString();
        }
    }
}