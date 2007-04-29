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
using Gtk;
using Gdk;
using UvsChess;


namespace UvsChess.Gui
{
    class GtkChessBoard : DrawingArea
    {
        const int numberOfChessPieces = 13;
        Pixbuf lightBackground;
        Pixbuf emptyMousePointer;
        Pixbuf darkBackground;
        Pixbuf[] piecePixbufs;
        ChessBoard _currentBoard;
        Gdk.Size tileSize = new Size(-1, -1);
        ChessLocation pieceStartLocation = new ChessLocation(-1, -1);
        Gdk.Point mouseLocation = new Point(-1, -1);
        bool isButtonDepressed = false;
        bool[,] isLightBackground = new bool[ChessBoard.NumberOfRows, ChessBoard.NumberOfColumns];
        Gdk.Image savedChessBoardImage;
        Gdk.Image chessBoardImage = null;
        private bool _isLocked = false;

        /// <summary>
        /// GtkChessBoard is locked when the game is in play, and the player is not human. Chess board is
        /// unlocked when the game is paused, or it's a human's turn to move.
        /// </summary>
        public bool IsLocked
        {
            get { return _isLocked; }
            set { _isLocked = value; }
        }


        #region Delegates
        public delegate void ChessPieceMoved(ChessMove move);
        public event ChessPieceMoved OnChessPieceMoved = null;
        #endregion


        public GtkChessBoard(ChessBoard initialBoard)
        {
            _currentBoard = initialBoard;                        
           
            //Load PixBufs
            piecePixbufs = new Pixbuf[numberOfChessPieces];
            for (ChessPiece i = 0; (int)i < numberOfChessPieces; ++i)
            {
                if (i != ChessPiece.Empty)
                {
                    string imageName = "UvsChess.Images.ChessPiece_" + i.ToString() + ".png";
                    Program.Log("Loading image: " + imageName);

                    //piecePixbufs[(int)i] = Pixbuf.LoadFromResource(imageName);
                    piecePixbufs[(int)i] = new Pixbuf(null,imageName);

                    if (tileSize.Width == -1)
                        tileSize.Width = piecePixbufs[(int)i].Width;

                    if (tileSize.Height == -1)
                        tileSize.Height = piecePixbufs[(int)i].Height;

                    if ( (tileSize.Width != piecePixbufs[(int)i].Width) ||
                         (tileSize.Height != piecePixbufs[(int)i].Height))
                    {
                        throw new Exception("Not all of the images have the same dimensions!");
                    }
                }
            }

            emptyMousePointer = new Pixbuf(null,"UvsChess.Images.Empty_1x1_Mouse_Cursor.png"); 
            lightBackground = new Pixbuf(null,"UvsChess.Images.Chess_LightBackground.png"); 
            darkBackground = new Pixbuf(null,"UvsChess.Images.Chess_DarkBackground.png");            

            this.ButtonPressEvent += new ButtonPressEventHandler(OnButtonPressEvent);
            this.ButtonReleaseEvent += new ButtonReleaseEventHandler(OnButtonReleaseEvent);
            this.MotionNotifyEvent += new MotionNotifyEventHandler(OnMotionNotifyEvent);
            this.ExposeEvent += new ExposeEventHandler(OnExposeEvent);
            
            this.Events |= Gdk.EventMask.ExposureMask | Gdk.EventMask.ButtonPressMask |
                           Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.PointerMotionMask;

            this.SetSizeRequest(tileSize.Width * ChessBoard.NumberOfColumns, tileSize.Height * ChessBoard.NumberOfRows);
        }

        private void OnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
        {
            if (isButtonDepressed && !IsLocked)
            {
                mouseLocation = new Point((int)args.Event.X, (int)args.Event.Y);

                this.QueueDraw();
            }
        }

        private void OnButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            // Left Click == 1
            if (IsLocked)
                return;

            if (args.Event.Button == 1)
            {
                if (((int)args.Event.X > tileSize.Width * ChessBoard.NumberOfColumns)||
                    ((int)args.Event.Y > tileSize.Height * ChessBoard.NumberOfRows))
                {
                    return;
                }
                pieceStartLocation = new ChessLocation((int)args.Event.X, (int)args.Event.Y, tileSize.Width, tileSize.Height);

                if (! _currentBoard.IsTileEmpty(pieceStartLocation))
                {
                    isButtonDepressed = true;

                    int x = pieceStartLocation.Column * tileSize.Width;
                    int y = pieceStartLocation.Row * tileSize.Height;
                    Pixbuf tile;

                    if (isLightBackground[pieceStartLocation.Row, pieceStartLocation.Column])
                    {
                        tile = lightBackground;
                    }
                    else
                    {
                        tile = darkBackground;
                    }
                    
                    
                    this.GdkWindow.DrawPixbuf(this.Style.BlackGC, tile,
                                            0, 0, x, y, tileSize.Width, tileSize.Height,
                                            Gdk.RgbDither.None, 0, 0);

                    //take snapshot of current board with out the active piece
                    savedChessBoardImage = this.GdkWindow.GetImage(0, 0, tileSize.Width * ChessBoard.NumberOfColumns, 
                                                                 tileSize.Height * ChessBoard.NumberOfRows);

                    // Make the mouse ptr invisible
                    args.Event.Window.Cursor = new Cursor(this.Display, emptyMousePointer, 0, 0);
                    

                    mouseLocation = new Point((int)args.Event.X, (int)args.Event.Y);                    

                    Program.Log(args.Event.X + " --- " + pieceStartLocation.Column);
                    Program.Log(args.Event.Y + " --- " + pieceStartLocation.Row);

                    this.QueueDraw();
                }
            }
        }

        private void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            if (IsLocked)
                return;

            if ( (args.Event.Button == 1) && (isButtonDepressed))
            {
                ChessMove newMove = new ChessMove();

                newMove.From = new ChessLocation(pieceStartLocation.Row, pieceStartLocation.Column);
                newMove.To = new ChessLocation((int)args.Event.X, (int)args.Event.Y, tileSize.Width, tileSize.Height);
                newMove.Flag = ChessFlag.NoFlag;

                if (newMove.From != newMove.To)
                {
                    if (OnChessPieceMoved != null)
                        OnChessPieceMoved(newMove);
                }
                else
                {
                    //They put it back down in the same place
                    Program.Log("No move");
                }
                                
                isButtonDepressed = false;

                // Set the cursor back to normal
                args.Event.Window.Cursor = null;

                //Log(args.Event.Device.HasCursor);
                pieceStartLocation = new ChessLocation(-1, -1);
                mouseLocation = new Point(-1, -1);

                
                this.QueueDraw();
            }
        }

        void OnExposeEvent(object o, ExposeEventArgs args)
        {
            this.GdkWindow.BeginPaintRegion(this.GdkWindow.VisibleRegion);
            if (isButtonDepressed)
            {
                this.GdkWindow.DrawImage(this.Style.WhiteGC, savedChessBoardImage, 0, 0, 0, 0, 
                                         savedChessBoardImage.Width, savedChessBoardImage.Height);
            }
            else
            {
                DrawChessBoard();

                for (int curRow = 0; curRow < ChessBoard.NumberOfRows; curRow++)
                {
                    for (int curCol = 0; curCol < ChessBoard.NumberOfColumns; curCol++)
                    {
                        if (!((curRow == pieceStartLocation.Row) && (curCol == pieceStartLocation.Column)))
                        {
                            if (_currentBoard[curRow, curCol] != ChessPiece.Empty)
                            {
                                int x = curCol * tileSize.Width;
                                int y = curRow * tileSize.Height;

                                this.GdkWindow.DrawPixbuf(this.Style.BlackGC, piecePixbufs[(int)_currentBoard[curRow, curCol]],
                                                        0, 0, x, y, tileSize.Width, tileSize.Height,
                                                        Gdk.RgbDither.None, 0, 0);
                            }
                        }
                    }
                }
            }

            if ((mouseLocation.X >= 0) && (mouseLocation.Y >= 0))
            {
                int letterXOffset = 37;
                int letterYOffset = 43;
                if (_currentBoard[pieceStartLocation.Row, pieceStartLocation.Column] != ChessPiece.Empty)
                {
                    this.GdkWindow.DrawPixbuf(this.Style.BlackGC,
                            piecePixbufs[(int)_currentBoard[pieceStartLocation.Row, pieceStartLocation.Column]],
                            0, 0, mouseLocation.X - letterXOffset, mouseLocation.Y - letterYOffset,
                            tileSize.Width, tileSize.Height,
                            Gdk.RgbDither.None, 100, 0);
                }
            }

            this.GdkWindow.EndPaint();
        }

        private void DrawChessBoard()
        {
            //Draw the chess tiles once and save as chessBoardImage
            if (chessBoardImage == null)
            {
                bool drawLightSquare = true;
                for (int curRow = 0; curRow < ChessBoard.NumberOfRows; curRow++)
                {
                    for (int curCol = 0; curCol < ChessBoard.NumberOfColumns; curCol++)
                    {
                        if (drawLightSquare)
                        {
                            isLightBackground[curRow, curCol] = true;

                            int x = curCol * tileSize.Width;
                            int y = curRow * tileSize.Height;
                            this.GdkWindow.DrawPixbuf(this.Style.WhiteGC, lightBackground, 0, 0, x, y, tileSize.Width,
                                                    tileSize.Height, Gdk.RgbDither.None, 0, 0);
                        }
                        else
                        {
                            isLightBackground[curRow, curCol] = false;

                            int x = curCol * (int)tileSize.Width;
                            int y = curRow * (int)tileSize.Height;
                            this.GdkWindow.DrawPixbuf(this.Style.BlackGC, darkBackground, 0, 0, x, y, tileSize.Width,
                                                    tileSize.Height, Gdk.RgbDither.None, 0, 0);
                        }

                        drawLightSquare = !drawLightSquare;
                    }

                    drawLightSquare = !drawLightSquare;
                }

                //take snapshot of chess board after it's drawn
                chessBoardImage = this.GdkWindow.GetImage(0, 0, tileSize.Width * ChessBoard.NumberOfColumns, 
                                                             tileSize.Height * ChessBoard.NumberOfRows);
            }
            else
            {
                this.GdkWindow.DrawImage(this.Style.WhiteGC, chessBoardImage, 0, 0, 0, 0, chessBoardImage.Width, chessBoardImage.Height);
            }
        }

        /// <summary>
        /// Sets the board regardless whether the board is locked
        /// </summary>
        /// <param name="board"></param>
        public void ResetBoard(ChessBoard board)
        {
           _currentBoard = board;
           Gtk.Application.Invoke(delegate
           {
               OnExposeEvent(null, null);
           });
        }

        /// <summary>
        /// Moves the piece regardless whether the board is locked
        /// </summary>
        /// <param name="chessMove"></param>
        public void MakeMove(ChessMove chessMove)
        {
            _currentBoard.MakeMove(chessMove);
           Gtk.Application.Invoke(delegate
           {
               OnExposeEvent(null, null);
           });

        }


    }
}
