using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace UvsChess.Gui
{
    public partial class GuiChessBoard : UserControl
    {
        int _boardWidth;
        int _boardHeight;
        int _tileWidth;
        int _tileHeight;
        bool _boardChanged = true;
        Bitmap _boardBitmap;

        Bitmap[] pieceBitmaps;
        Bitmap darkTile;
        Bitmap lightTile;
        Bitmap blackRook;

        public GuiChessBoard()
        {
            InitializeComponent();

            int numPieces = Enum.GetValues(typeof(ChessPiece)).Length;
            pieceBitmaps = new Bitmap[numPieces];

            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resNames = asm.GetManifestResourceNames();

            int res = 95;
            foreach (string curRes in resNames)
            {
                switch (curRes)
                {
                    case "UvsChess.Images.Chess_DarkBackground.png":
                        darkTile = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        darkTile.SetResolution(res, res);                        
                        break;

                    case "UvsChess.Images.Chess_LightBackground.png":
                        lightTile = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        lightTile.SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_WhitePawn.png":
                        pieceBitmaps[(int)ChessPiece.WhitePawn] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.WhitePawn].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_WhiteRook.png":
                        pieceBitmaps[(int)ChessPiece.WhiteRook] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.WhiteRook].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_WhiteKnight.png":
                        pieceBitmaps[(int)ChessPiece.WhiteKnight] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.WhiteKnight].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_WhiteBishop.png":
                        pieceBitmaps[(int)ChessPiece.WhiteBishop] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.WhiteBishop].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_WhiteQueen.png":
                        pieceBitmaps[(int)ChessPiece.WhiteQueen] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.WhiteQueen].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_WhiteKing.png":
                        pieceBitmaps[(int)ChessPiece.WhiteKing] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.WhiteKing].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_BlackPawn.png":
                        pieceBitmaps[(int)ChessPiece.BlackPawn] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.BlackPawn].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_BlackRook.png":
                        pieceBitmaps[(int)ChessPiece.BlackRook] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.BlackRook].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_BlackKnight.png":
                        pieceBitmaps[(int)ChessPiece.BlackKnight] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.BlackKnight].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_BlackBishop.png":
                        pieceBitmaps[(int)ChessPiece.BlackBishop] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.BlackBishop].SetResolution(res, res);
                        break;
                    
                    case "UvsChess.Images.ChessPiece_BlackQueen.png":
                        pieceBitmaps[(int)ChessPiece.BlackQueen] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.BlackQueen].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.ChessPiece_BlackKing.png":
                        pieceBitmaps[(int)ChessPiece.BlackKing] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        pieceBitmaps[(int)ChessPiece.BlackKing].SetResolution(res, res);
                        break;
                }
            }

            _tileWidth = darkTile.Width;
            _tileHeight = darkTile.Height;
            _boardHeight = ChessBoard.NumberOfRows * _tileHeight;
            _boardWidth = ChessBoard.NumberOfColumns * _tileWidth;
           
            this.Paint += OnPaint;


            // Validate the image sizes
            //int defaultWidth = chessImages.Images[0].Width;
            //int defaultHeight = chessImages.Images[0].Height;

            //foreach (Image curImage in chessImages.Images)
            //{
            //    if (curImage.Height != defaultHeight)
            //    {
            //        throw (new Exception("Image height is outside the default height!"));
            //    }

            //    if (curImage.Width != defaultWidth)
            //    {
            //        throw (new Exception("Image width is outside the default width!"));
            //    }
            //}
        }

        public void OnPaint(object sender, PaintEventArgs e)
        {
            if (_boardChanged)
            {
                _boardBitmap = GetBoardImage(new ChessBoard());
                _boardChanged = false;
            }
            
            e.Graphics.DrawImage(_boardBitmap, 0, 0);
        }

        public Bitmap GetBoardImage(ChessBoard board)
        {
            Bitmap boardBitmap = new Bitmap(_boardWidth, _boardHeight);

            Graphics boardGraphics = Graphics.FromImage(boardBitmap);

            bool lightSquare = true;
            for (int y = 0; y < ChessBoard.NumberOfRows; y++)
            {
                int curY = y * _tileHeight;

                for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
                {
                    int curX = x * _tileWidth;

                    if (lightSquare)
                    {
                        boardGraphics.DrawImage(lightTile, curX, curY);
                    }
                    else
                    {
                        boardGraphics.DrawImage(darkTile, curX, curY);
                    }

                    if (board[y, x] != ChessPiece.Empty)
                    {
                        boardGraphics.DrawImage(pieceBitmaps[(int)board[y, x]], curX, curY);
                    }

                    lightSquare = !lightSquare;
                }

                lightSquare = !lightSquare;
            }

            return boardBitmap;
        }
    }
}
