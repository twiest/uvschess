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
        int _horizontalBorderHeight;
        int _verticalBorderWidth;
        bool _boardChanged = true;
        Bitmap _boardBitmap;        

        Bitmap[] pieceBitmaps;
        Bitmap[] _verticalBorderBitmaps = new Bitmap[ChessBoard.NumberOfRows];
        Bitmap[] _horizontalBorderBitmaps = new Bitmap[ChessBoard.NumberOfColumns];
        Bitmap _cornerBorder;
        Bitmap _darkTile;
        Bitmap _lightTile;

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
                        _darkTile = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _darkTile.SetResolution(res, res);                        
                        break;

                    case "UvsChess.Images.Chess_LightBackground.png":
                        _lightTile = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _lightTile.SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Corner_Border.png":                    
                        _cornerBorder = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _cornerBorder.SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Horizontal_Border_0.png":
                        _horizontalBorderBitmaps[0] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _horizontalBorderBitmaps[0].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Horizontal_Border_1.png":
                        _horizontalBorderBitmaps[1] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _horizontalBorderBitmaps[1].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Horizontal_Border_2.png":
                        _horizontalBorderBitmaps[2] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _horizontalBorderBitmaps[2].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Horizontal_Border_3.png":
                        _horizontalBorderBitmaps[3] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _horizontalBorderBitmaps[3].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Horizontal_Border_4.png":
                        _horizontalBorderBitmaps[4] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _horizontalBorderBitmaps[4].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Horizontal_Border_5.png":
                        _horizontalBorderBitmaps[5] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _horizontalBorderBitmaps[5].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Horizontal_Border_6.png":
                        _horizontalBorderBitmaps[6] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _horizontalBorderBitmaps[6].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Horizontal_Border_7.png":
                        _horizontalBorderBitmaps[7] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _horizontalBorderBitmaps[7].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Vertical_Border_0.png":
                        _verticalBorderBitmaps[0] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _verticalBorderBitmaps[0].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Vertical_Border_1.png":
                        _verticalBorderBitmaps[1] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _verticalBorderBitmaps[1].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Vertical_Border_2.png":
                        _verticalBorderBitmaps[2] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _verticalBorderBitmaps[2].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Vertical_Border_3.png":
                        _verticalBorderBitmaps[3] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _verticalBorderBitmaps[3].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Vertical_Border_4.png":
                        _verticalBorderBitmaps[4] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _verticalBorderBitmaps[4].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Vertical_Border_5.png":
                        _verticalBorderBitmaps[5] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _verticalBorderBitmaps[5].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Vertical_Border_6.png":
                        _verticalBorderBitmaps[6] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _verticalBorderBitmaps[6].SetResolution(res, res);
                        break;

                    case "UvsChess.Images.Vertical_Border_7.png":
                        _verticalBorderBitmaps[7] = (Bitmap)Bitmap.FromStream(asm.GetManifestResourceStream(curRes));
                        _verticalBorderBitmaps[7].SetResolution(res, res);
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

            _horizontalBorderHeight = _horizontalBorderBitmaps[0].Height;
            _verticalBorderWidth = _verticalBorderBitmaps[0].Width;

            _tileWidth = _darkTile.Width;
            _tileHeight = _darkTile.Height;
            _boardHeight = (ChessBoard.NumberOfRows * _tileHeight) + _horizontalBorderHeight;
            _boardWidth = (ChessBoard.NumberOfColumns * _tileWidth) + _verticalBorderWidth;
           
            this.Paint += OnPaint;


            // Validate the image sizes
            foreach (Bitmap curBitmap in pieceBitmaps)
            {
                if (curBitmap != null)
                {
                    if (curBitmap.Height != _tileHeight)
                    {
                        throw (new Exception("Image height is outside the default height!"));
                    }

                    if (curBitmap.Width != _tileWidth)
                    {
                        throw (new Exception("Image width is outside the default width!"));
                    }
                }
            }
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

            boardGraphics.DrawImage(_cornerBorder, 0, 0);

            for (int ix = 0; ix < ChessBoard.NumberOfColumns; ix++)
            {
                int curPos = (ix * _tileWidth) + _verticalBorderWidth;

                boardGraphics.DrawImage(_horizontalBorderBitmaps[ix], curPos, 0);
                boardGraphics.DrawImage(_verticalBorderBitmaps[ix], 0, curPos);
            }

            bool lightSquare = true;
            for (int y = 0; y < ChessBoard.NumberOfRows; y++)
            {
                int curY = (y * _tileHeight) + _horizontalBorderHeight;

                for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
                {
                    int curX = (x * _tileWidth) + _verticalBorderWidth;

                    if (lightSquare)
                    {
                        boardGraphics.DrawImage(_lightTile, curX, curY);
                    }
                    else
                    {
                        boardGraphics.DrawImage(_darkTile, curX, curY);
                    }

                    if (board[x, y] != ChessPiece.Empty)
                    {
                        boardGraphics.DrawImage(pieceBitmaps[(int)board[x, y]], curX, curY);
                    }

                    lightSquare = !lightSquare;
                }

                lightSquare = !lightSquare;
            }

            return boardBitmap;
        }
    }
}
