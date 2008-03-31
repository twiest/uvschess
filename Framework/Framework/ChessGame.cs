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
using System.Threading;

namespace UvsChess.Framework
{
    class ChessGame
    {
        public delegate void UpdatedDelegate(string playerColor, string nextMove, string currentFen);
        public UpdatedDelegate Updated = null;

        public delegate void DeclareResultsDelegate(string results);
        public DeclareResultsDelegate DeclareResults = null;

        ChessState _mainChessState = null;
        public bool IsGameRunning = false;
        string results = string.Empty;

        ChessPlayer WhitePlayer = null;
        ChessPlayer BlackPlayer = null;
        Thread _chessGameThread = null;
        
        public ChessGame(string fen, string whitePlayerName, string blackPlayerName)
        {            
            mainChessState = new ChessState(fen);

            WhitePlayer = new ChessPlayer(ChessColor.White);
            BlackPlayer = new ChessPlayer(ChessColor.Black);

            WhitePlayer.AIName = whitePlayerName;
            BlackPlayer.AIName = blackPlayerName;

            //Load the AI if it isn't loaded already
            LoadAI(WhitePlayer);
            LoadAI(BlackPlayer);

            // Hook up the AI Log methods to the GUI
            if (WhitePlayer.IsComputer)
            {
                WhitePlayer.AI.Log += Logger.AddToWhitesLog;
            }

            if (BlackPlayer.IsComputer)
            {
                BlackPlayer.AI.Log += Logger.AddToBlacksLog;
            }
        }

        private ChessState mainChessState
        {
            get { return _mainChessState; }
            set { _mainChessState = value; }
        }

        public void StartGame()
        {
            _chessGameThread = new Thread(PlayInThread);
            _chessGameThread.Start();
        }

        public void StopGameEarly()
        {
            IsGameRunning = false;            

            WhitePlayer.EndTurnEarly();
            BlackPlayer.EndTurnEarly();

            _chessGameThread.Join();


            // Explicitly get rid of the AI and Player objects
            WhitePlayer.AI = null;
            WhitePlayer = null;
            BlackPlayer.AI = null;
            BlackPlayer = null;           
        }

        public void WhitePlayer_HumanMovedPieceEvent(ChessMove move)
        {
            WhitePlayer.HumanMovedPieceEvent(move);
        }

        public void BlackPlayer_HumanMovedPieceEvent(ChessMove move)
        {
            BlackPlayer.HumanMovedPieceEvent(move);
        }

        void PlayInThread()
        {
            //This method run in its own thread.          

            IsGameRunning = true;

            while (IsGameRunning)
            {
                if (mainChessState.CurrentPlayerColor == ChessColor.White)
                {
                    //change radion button selection
                    //SelectRadio(radWhite);
                    DoNextMove(WhitePlayer, BlackPlayer);
                }
                else
                {
                    //SelectRadio(radBlack);
                    DoNextMove(BlackPlayer, WhitePlayer);
                }
            }

            // Remove the AI Log methods from the GUI
            if (WhitePlayer.IsComputer)
            {
                WhitePlayer.AI.Log -= Logger.AddToWhitesLog;
            }

            if (BlackPlayer.IsComputer)
            {
                BlackPlayer.AI.Log -= Logger.AddToBlacksLog;
            }

            if (DeclareResults != null)
            {
                DeclareResults(results);
            }
            
            Logger.Log("Game Over");
            //StopGame();
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

                if (!this.IsGameRunning)
                {
                    // if we're not running, leave the method
                    return;
                }

                if ( (nextMove == null) || (!nextMove.IsBasicallyValid) )
                {
                    IsGameRunning = false;
                    results = "The framework caught " + player.ColorAndName + " returning a completely invalid move, therefore " +
                              player.ColorAndName + " loses!";

                    if (nextMove == null)
                    {
                        Logger.Log(player.ColorAndName + " returned a null move object.");
                    }
                    else
                    {
                        Logger.Log(player.ColorAndName + "'s invalid move was: " + nextMove.ToString());
                    }

                    return;
                }

                if (nextMove.Flag == ChessFlag.AIWentOverTime)
                {
                    // the AI went over it's time limit.
                    IsGameRunning = false;
                    results = player.ColorAndName + " went over the time limit and grace period. Total move time was: " + player.TimeOfLastMove.ToString();
                    return;
                }

                if (nextMove.Flag != ChessFlag.Stalemate)
                {
                    // The move is not a stale mate, and it's basically valid,
                    // so, let's see if the move is actually valid.
                    newstate = mainChessState.Clone();
                    newstate.MakeMove(nextMove);

                    if (opponent.IsComputer)
                    {
                        isValidMove = opponent.AI.IsValidMove(newstate);
                    }
                    else
                    {
                        // All moves are valid against a human, as there's no
                        // way for the human to object to a move.
                        isValidMove = true;
                    }
                }
            }
            else //player is human
            {
                while (!isValidMove)
                {
                    nextMove = player.GetNextMove(mainChessState.CurrentBoard);

                    if (!IsGameRunning)
                    {
                        // if we're not running, leave the method
                        return;
                    }

                    newstate = mainChessState.Clone();
                    newstate.MakeMove(nextMove);

                    if (opponent.IsHuman)
                    {
                        isValidMove = true;
                    }
                    else
                    {
                        isValidMove = opponent.AI.IsValidMove(newstate);
                    }
                }
            }

            if (isValidMove)
            {
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

                if (Updated != null)
                {
                    Updated(player.Color.ToString(), nextMove.ToString(), newstate.ToFenBoard());
                }

                if (nextMove.Flag == ChessFlag.Check)
                {
                    Logger.Log(player.ColorAndName + " has put " + opponent.ColorAndName + " in Check!");
                }

                if (nextMove.Flag == ChessFlag.Checkmate)
                {
                    // Checkmate on a valid move has been signaled.
                    IsGameRunning = false;

                    results = player.ColorAndName + " has signaled that the game is a checkmate _and_ " +
                              opponent.ColorAndName + " said the last move was valid.";
                }
            }
            else
            {
                // It is either a stalemate or an invalid move. Either way, we're done running.
                IsGameRunning = false;

                if (nextMove.Flag == ChessFlag.Stalemate)
                {
                    // A stalemate has occurred. Since stalemates can occur because the AI can't
                    // make a move, we don't have the other AI check their move (because it would
                    // probably just be an empty move).
                    results = player.ColorAndName + " has signaled that the game is a stalemate.";
                }
                else
                {
                    results = opponent.ColorAndName + " has signaled that " + player.ColorAndName + 
                              " returned an invalid move returned, therefore " + player.ColorAndName + " loses!";

                    Logger.Log(player.ColorAndName + "'s invalid move was: " + nextMove.ToString());
                }
            }
        }

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
    }
}
