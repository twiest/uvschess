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
using System.Threading;

namespace UvsChess.Framework
{
    class ChessGame
    {
        public delegate void GameUpdatedDelegate(string playerColor, string nextMove, string currentFen);
        public GameUpdatedDelegate GameUpdated = null;

        ChessState _mainChessState = null;
        public bool IsRunning = false;

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
            IsRunning = false;            

            WhitePlayer.EndTurnEarly();
            BlackPlayer.EndTurnEarly();

            _chessGameThread.Join();

            WhitePlayer = null;
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

            IsRunning = true;
            //ChessMove currentMove = null;

            while (IsRunning)
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

            //Logger.Log("Game Over");
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
                    nextMove = player.GetNextMove(mainChessState.CurrentBoard.Clone());

                    if (!IsRunning)
                    {
                        // if we're not running, leave the method
                        return;
                    }

                    newstate = mainChessState.Clone();
                    newstate.MakeMove(nextMove);

                    // TODO: Fix NRE here when a human is playing another human
                    isValidMove = opponent.AI.IsValidMove(newstate);
                }
            }

            if ((isValidMove) &&
                 (nextMove.Flag == ChessFlag.Checkmate))
            {
                // Checkmate on a valid move has been signaled.
                IsRunning = false;
                Logger.Log(String.Format("{0} has signaled that the game is a stalemate.",
                                    (player.Color == ChessColor.Black) ? "Black" : "White"));
            }
            else if (isValidMove)
            {
                //update the board
                //chessBoardControl.ResetBoard(newstate.CurrentBoard);
                //AddToHistory(player.Color.ToString() + ": " + nextMove.ToString(), newstate.ToFenBoard());                

                //update mainChessState for valid 
                mainChessState = newstate;

                if (player.Color == ChessColor.Black)
                {
                    mainChessState.FullMoves++;//Increment fullmoves after black's turn
                    //SetFullMoves(mainChessState.FullMoves);
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

                //SetHalfMoves(mainChessState.HalfMoves);

                if (GameUpdated != null)
                {
                    GameUpdated(player.Color.ToString(), nextMove.ToString(), newstate.ToFenBoard());
                }

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
