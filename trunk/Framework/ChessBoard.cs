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
using System.Text;

namespace UvsChess
{
    public class ChessBoard
    {
        #region Members
        public const int NumberOfRows = 8;
        public const int NumberOfColumns = 8;

        ChessPiece[,] _board;
        #endregion

        #region Constructors
        public ChessBoard():this(ChessState.StartState)
        {            
        }

        public ChessBoard(string fenBoard)
        {
            _board = new ChessPiece[NumberOfRows, NumberOfColumns];

            FromFenBoard(fenBoard);
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the piece on the board in a specified location; 0,0 is the upper left hand corner of the board.
        /// </summary>
        /// <param name="row">Row of the desired piece location</param>
        /// <param name="col">Column of the desired piece location</param>
        /// <returns></returns>
        public ChessPiece this[int row, int col]
        {
            get { return _board[row, col]; }
            set { _board[row, col] = value; }
        }
        #endregion

        #region Methods and Operators
        public ChessBoard Clone()
        {
            ChessBoard newChessBoard = new ChessBoard();

            for (int curRow = 0; curRow < NumberOfRows; curRow++)
            {
                for (int curCol = 0; curCol < NumberOfColumns; curCol++)
                {
                    newChessBoard._board[curRow, curCol] = this._board[curRow, curCol];
                }
            }

            return newChessBoard;
        }

        public void MakeMove(ChessMove move)
        {
            if ( (move.To.IsValid) && (move.From.IsValid) )
            {
                if (move.From != move.To)
                {
                    this._board[move.To.Row, move.To.Column] = this._board[move.From.Row, move.From.Column];                            
                    this._board[move.From.Row, move.From.Column] = ChessPiece.Empty;
                }

                //Program.Log("Piece Moved: " + move.ToString());
            }
        }

        public bool IsTileEmpty(ChessLocation location)
        {
            return (_board[location.Row, location.Column] == ChessPiece.Empty);
        }

        /// <summary>
        /// This function accepts a full fen board and sets the ChessBoard object to that state.
        /// </summary>
        /// <param name="fenBoard"></param>
        public void FromFenBoard(string fenBoard)
        {
            string[] lines = fenBoard.Split(' ')[0].Split('/');
            int spaces = 0;

            for (int row = 0; row < ChessBoard.NumberOfRows; ++row)
            {
                for (int col = 0; col < ChessBoard.NumberOfColumns; ++col)
                {
                    this[row, col] = ChessPiece.Empty;
                }
            }
            ChessPiece piece = ChessPiece.Empty;
            for (int row = 0; row < ChessBoard.NumberOfRows; ++row)
            {
                for (int col = 0, boardCol = 0; col < lines[row].Length; ++col, ++boardCol)
                {
                    if (Char.IsDigit(lines[row][col]))
                    {
                        spaces = Convert.ToInt32(lines[row][col]) - 48;
                        boardCol += spaces - 1;
                    }
                    else
                    {
                        switch (lines[row][col])
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
                        this[row, boardCol] = piece;
                    }
                }
            }
        }

        public string ToFenBoard()
        {
            StringBuilder strBuild = new StringBuilder();
            for (int row = 0; row < ChessBoard.NumberOfRows; ++row)
            {
                int spaces = 0;
                for (int col = 0; col < ChessBoard.NumberOfColumns; ++col)
                {
                    char c = 'x';
                    switch (this[row, col])
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
                if (row < 7)
                {
                    strBuild.Append('/');
                }
            }
            return strBuild.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToFenBoard().GetHashCode();
        }
        #endregion
    }
}
