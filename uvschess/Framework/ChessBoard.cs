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
using System.Text;

namespace UvsChess
{
    public class ChessBoard
    {
        #region Members
        public const int NumberOfRows = 8;
        public const int NumberOfColumns = 8;
        #endregion

        #region Constructors
        public ChessBoard():this(ChessState.FenStartState)
        {            
        }

        public ChessBoard(string fenBoard)
        {
            Board = new ChessPiece[NumberOfRows, NumberOfColumns];

            FromFenBoard(fenBoard);
        }

        public ChessBoard(ChessPiece[,] board)
        {
            Board = CloneBoard(board);
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the piece on the board in a specified location; 0,0 is the upper left hand corner of the board.
        /// </summary>
        /// <param name="location">Location of the desired piece</param>
        /// <returns>ChessPiece</returns>
        public ChessPiece this[ChessLocation location]
        {
            get { return this[location.X, location.Y]; }
            set { this[location.X, location.Y] = value; }
        }

        /// <summary>
        /// Gets or sets the piece on the board in a specified location; 0,0 is the upper left hand corner of the board.
        /// </summary>
        /// <param name="x">Column of the desired piece location</param>
        /// <param name="y">Row of the desired piece location</param>
        /// <returns>ChessPiece</returns>
        public ChessPiece this[int x, int y]
        {
            get { return Board[x, y]; }
            set { Board[x, y] = value; }
        }

        private ChessPiece[,] Board
        {
            get;
            set;
        }

        public ChessPiece[,] RawBoard
        {
            get { return this.Clone().Board; }
        }
        #endregion

        #region Methods and Operators
        private ChessPiece[,] CloneBoard(ChessPiece[,] board)
        {
            ChessPiece[,] retBoard = new ChessPiece[NumberOfRows, NumberOfColumns];
            for (int Y = 0; Y < NumberOfRows; Y++)
            {
                for (int X = 0; X < NumberOfColumns; X++)
                {
                    retBoard[X, Y] = board[X, Y];
                }
            }

            return retBoard;
        }

        public ChessBoard Clone()
        {
            return new ChessBoard(this.Board);
        }

        public void MakeMove(ChessMove move)
        {
            if (move.IsBasicallyValid)
            {
                // Handle Queening
                if ((this[move.From] == ChessPiece.WhitePawn) && (move.From.Y == 1) && (move.To.Y == 0))
                {
                    this[move.To] = ChessPiece.WhiteQueen;
                }
                else if ((this[move.From] == ChessPiece.BlackPawn) && (move.From.Y == 6) && (move.To.Y == 7))
                {
                    this[move.To] = ChessPiece.BlackQueen;
                }
                else
                {
                    this[move.To] = this[move.From];                    
                }

                this[move.From] = ChessPiece.Empty;
            }
        }

        /// <summary>
        /// This function accepts a full fen board and sets the ChessBoard object to that state.
        /// </summary>
        /// <param name="fenBoard"></param>
        public void FromFenBoard(string fenBoard)
        {
            string[] lines = fenBoard.Split(' ')[0].Split('/');
            int spaces = 0;

            for (int y = 0; y < ChessBoard.NumberOfRows; ++y)
            {
                for (int x = 0; x < ChessBoard.NumberOfColumns; ++x)
                {
                    this[x, y] = ChessPiece.Empty;
                }
            }
            ChessPiece piece = ChessPiece.Empty;
            for (int y = 0; y < ChessBoard.NumberOfRows; ++y)
            {
                for (int x = 0, boardCol = 0; x < lines[y].Length; ++x, ++boardCol)
                {
                    if (Char.IsDigit(lines[y][x]))
                    {
                        spaces = Convert.ToInt32(lines[y][x]) - 48;
                        boardCol += spaces - 1;
                    }
                    else
                    {
                        switch (lines[y][x])
                        {
                            case 'r':
                                piece = ChessPiece.BlackRook;
                                break;
                            case 'n':
                                piece = ChessPiece.BlackKnight;
                                break;
                            case 'b':
                                piece = ChessPiece.BlackBishop;
                                break;
                            case 'q':
                                piece = ChessPiece.BlackQueen;
                                break;
                            case 'k':
                                piece = ChessPiece.BlackKing;
                                break;
                            case 'p':
                                piece = ChessPiece.BlackPawn;
                                break;
                            case 'K':
                                piece = ChessPiece.WhiteKing;
                                break;
                            case 'Q':
                                piece = ChessPiece.WhiteQueen;
                                break;
                            case 'R':
                                piece = ChessPiece.WhiteRook;
                                break;
                            case 'B':
                                piece = ChessPiece.WhiteBishop;
                                break;
                            case 'N':
                                piece = ChessPiece.WhiteKnight;
                                break;
                            case 'P':
                                piece = ChessPiece.WhitePawn;
                                break;
                            default:
                                throw new Exception("Invalid FEN board");

                        }
                        this[boardCol, y] = piece;
                    }
                }
            }
        }

        public string ToPartialFenBoard()
        {
            StringBuilder strBuild = new StringBuilder();
            for (int y = 0; y < ChessBoard.NumberOfRows; ++y)
            {
                int spaces = 0;
                for (int x = 0; x < ChessBoard.NumberOfColumns; ++x)
                {
                    char c = 'x';
                    switch (this[x, y])
                    {
                        case ChessPiece.WhitePawn:
                            c = 'P';
                            break;
                        case ChessPiece.WhiteRook:
                            c = 'R';
                            break;
                        case ChessPiece.WhiteKnight:
                            c = 'N';
                            break;
                        case ChessPiece.WhiteBishop:
                            c = 'B';
                            break;
                        case ChessPiece.WhiteQueen:
                            c = 'Q';
                            break;
                        case ChessPiece.WhiteKing:
                            c = 'K';
                            break;
                        case ChessPiece.BlackPawn:
                            c = 'p';
                            break;
                        case ChessPiece.BlackRook:
                            c = 'r';
                            break;
                        case ChessPiece.BlackKnight:
                            c = 'n';
                            break;
                        case ChessPiece.BlackBishop:
                            c = 'b';
                            break;
                        case ChessPiece.BlackQueen:
                            c = 'q';
                            break;
                        case ChessPiece.BlackKing:
                            c = 'k';
                            break;
                        case ChessPiece.Empty:
                            ++spaces;
                            continue;
                        //break;
                        default:
                            throw new Exception("Invalid chess piece");
                    }
                    if ((c != 'x') && (spaces > 0))
                    {
                        strBuild.Append(spaces.ToString());
                        spaces = 0;
                    }
                    strBuild.Append(c);
                }

                if (spaces > 0)
                {
                    strBuild.Append(spaces.ToString());
                    spaces = 0;
                }
                if (y < 7)
                {
                    strBuild.Append('/');
                }
            }
            return strBuild.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToPartialFenBoard().GetHashCode();
        }
        #endregion
    }
}
