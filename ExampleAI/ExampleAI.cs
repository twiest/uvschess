/******************************************************************************
* The MIT License
* Copyright (c) 2006 Rusty Howell, Thomas Wiest
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
using System.Text;
using UvsChess;

namespace ExampleAI
{
    public class ExampleAI : IChessAI
    {
        bool running = false;

        #region IChessAI Members
        public bool IsRunning
        {
            get { return running; }
        }

        public string Name
        {
            get { return "ExampleAI"; }
        }


        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
        
            running = true;
            ChessMove myNextMove = null;

            //if (currentState.PreviousMove.Flag == ChessFlag.Checkmate)
            //{
            //    myNextMove = new ChessMove();

            //    // Normally I would run my move validator on their move 
            //    // to make sure they made a valid move. If they did,
            //    // and I agree that I'm checkmated, then I surrender.
            //    // Otherwise I set "Invalid Move" on them
            //    return myNextMove;
            //}

            while (running)
            {
                //if (currentState.PreviousBoard == null)
                //{
                //    // 1st move of the game
                //    myNextMove = FindAMove(currentState);
                //}
                //else
                //{
                    //myNextMove = new ChessMove();
                    //myNextMove.From = currentState.PreviousMove.From;
                    //myNextMove.From.Row = myNextMove.From.Row - 7;

                    //myNextMove.To = currentState.PreviousMove.To;
                    //myNextMove.To.Row = myNextMove.To.Row - 7;
                //}

                myNextMove = FindAMove(board, myColor);

                if (myNextMove != null)
                {
                    running = false;
                }

                
            }

            return myNextMove;
        }

        public bool IsValidMove(ChessState currentState)
        {
            return true;
        }

        // This Method gets called when time has run out on your turn.
        public void EndTurn()
        {
            running = false;
            Log("AI was told to end turn at: " + System.DateTime.Now.ToString());
        }

        public void Log(string message)
        {
        }
        #endregion


        #region My AI Logic
        ChessMove FindAMove(ChessBoard currentBoard, ChessColor currentColor)
        {
            ChessMove newMove = null;

            for (int curRow = 1; curRow < ChessBoard.NumberOfRows - 1; curRow++)
            {
                for (int curCol = 0; curCol < ChessBoard.NumberOfColumns; curCol++)
                {
                    if (currentColor == ChessColor.White)
                    {
                        if ((currentBoard[curRow - 1, curCol] == ChessPiece.Empty) &&
                            ((currentBoard[curRow, curCol] == ChessPiece.WhitePawn) ||
                              (currentBoard[curRow, curCol] == ChessPiece.WhiteKing) ||
                              (currentBoard[curRow, curCol] == ChessPiece.WhiteQueen) ||
                              (currentBoard[curRow, curCol] == ChessPiece.WhiteRook)))
                        {
                            newMove = new ChessMove();
                            newMove.From = new ChessLocation(curRow, curCol);
                            newMove.To = new ChessLocation(curRow - 1, curCol);

                            return newMove;
                        }
                    }
                    else
                    {
                        if ((currentBoard[curRow + 1, curCol] == ChessPiece.Empty) &&
                            ((currentBoard[curRow, curCol] == ChessPiece.BlackPawn) ||
                              (currentBoard[curRow, curCol] == ChessPiece.BlackKing) ||
                              (currentBoard[curRow, curCol] == ChessPiece.BlackQueen) ||
                              (currentBoard[curRow, curCol] == ChessPiece.BlackRook)))
                        {
                            newMove = new ChessMove();
                            newMove.From = new ChessLocation(curRow, curCol);
                            newMove.To = new ChessLocation(curRow + 1, curCol);

                            return newMove;
                        }
                    }
                }
            }

            // If I couldn't find a valid move easily, 
            // I'll just create an empty move.
            newMove = new ChessMove();

            return newMove;
        }
        #endregion
    }
}
