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
using System.Text;
using UvsChess;

namespace ExampleAI
{
    public class ExampleAI : IChessAI
    {
        #region IChessAI Members

        /// <summary>
        /// This is set to true when the framework starts running your AI. When the AI's time has run out,
        /// the framework will set this variable to false, which the AI should detect and immediatly exit 
        /// and return your move to the framework.
        /// 
        /// You should NEVER EVER set this property, you should only read from it.
        /// IsRunning should be defined as an Automatic Property.
        /// IsRunning SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        /// 
        /// Log should be defined as an Automatic Property.
        /// Log SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AILoggerCallback Log { get; set; }
        #endregion

        /// <summary>
        /// The name of your AI
        /// </summary>
        public string Name
        {
            get { return "ExampleAI"; }
        }

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            ChessMove myNextMove = null;

            while (IsRunning)            
            {
                if (myNextMove == null)
                {
                    myNextMove = MoveAPawn(board, myColor);
                    this.Log(myColor.ToString() + " (" + this.Name + ") just moved.");
                    this.Log(string.Empty);

                    // Since I have a move, break out of loop
                    break;
                }               
            }

            return myNextMove;
        }

        /// <summary>
        /// Validates a move. The framework will use this to validate your opponents move.
        /// </summary>
        /// <param name="currentState">ChessState, including previous state, previous move. </param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessState currentState)
        {
            return true;
        }

        #region My AI Logic
        /// <summary>
        /// This method generates a ChessMove to move a pawn.
        /// </summary>
        /// <param name="currentBoard">This is the current board to generate the move for.</param>
        /// <param name="myColor">This is the color of the player that should generate the move.</param>
        /// <returns>A chess move.</returns>
        ChessMove MoveAPawn(ChessBoard currentBoard, ChessColor myColor)
        {
            // This logic only moves pawns one space forward. It does not move any other pieces.        
            ChessMove newMove = null;

            for (int Y = 1; Y < ChessBoard.NumberOfRows - 1; Y++)
            {
                for (int X = 0; X < ChessBoard.NumberOfColumns; X++)
                {
                    if (myColor == ChessColor.White)
                    {
                        if ((currentBoard[X, Y-1] == ChessPiece.Empty) &&
                            (currentBoard[X, Y] == ChessPiece.WhitePawn))
                        {
                            newMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y - 1));

                            return newMove;
                        }
                    }
                    else // myColor is black
                    {
                        if ((currentBoard[X, Y+1] == ChessPiece.Empty) &&
                            (currentBoard[X, Y] == ChessPiece.BlackPawn))
                        {
                            newMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y + 1));

                            return newMove;
                        }
                    }
                }
            }

            // If I couldn't find a valid move easily, 
            // I'll just create an empty move and flag a stalemate.
            newMove = new ChessMove(null, null);
            //newMove.Flag = ChessFlag.Stalemate;
            newMove.Flag = ChessFlag.Stalemate;

            return newMove;
        }
        #endregion
    }
}
