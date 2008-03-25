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

        Bitmap darkTile;
        Bitmap lightTile;
        Bitmap blackRook;

        public GuiChessBoard()
        {
            InitializeComponent();

            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resNames = asm.GetManifestResourceNames();
            foreach (string curRes in resNames)
            {
                switch (curRes)
                {
                    case "UvsChess.Images.Chess_DarkBackground.png":
                        darkTile = Bitmap.FromStream(asm.GetManifestResourceStream(curRes)) as Bitmap;
                        break;

                    case "UvsChess.Images.Chess_LightBackground.png":
                        lightTile = Bitmap.FromStream(asm.GetManifestResourceStream(curRes)) as Bitmap;
                        break;

                    case "UvsChess.Images.ChessPiece_BlackRook.png":
                        blackRook = Bitmap.FromStream(asm.GetManifestResourceStream(curRes)) as Bitmap;
                        break;                       
                }
            }

            int res = 95;

            darkTile.SetResolution(res, res);
            lightTile.SetResolution(res, res);
            blackRook.SetResolution(res, res);

            _tileWidth = darkTile.Width;
            _tileHeight = darkTile.Height;
            _boardHeight = 8 * _tileHeight;
            _boardWidth = 8 * _tileWidth;
           
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

            //bool lightSquare = true;
            //for (int x = 0; x < 8; x++)
            //{
            //    int curX = x * defaultWidth;

            //    for (int y = 0; y < 8; y++)
            //    {
            //        int curY = y * defaultHeight;

            //        if (lightSquare)
            //        {
            //            //_chessBoard.DrawImage(chessImages.Images[0], curX, curY);
            //        }
            //        else
            //        {
            //            //_chessBoard.DrawImage(chessImages.Images[0], curX, curY);
            //        }

            //        lightSquare = !lightSquare;
            //    }
            //}

            //_chessBoard.
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
            for (int y = 0; y < 8; y++)
            {
                int curY = y * _tileHeight;

                for (int x = 0; x < 8; x++)
                {
                    int curX = x * _tileWidth;

                    if (lightSquare)
                    {
                        boardGraphics.DrawImage(lightTile, curX, curY);
                        //_chessBoard.DrawImage(chessImages.Images[0], curX, curY);
                    }
                    else
                    {
                        boardGraphics.DrawImage(darkTile, curX, curY);
                        //_chessBoard.DrawImage(chessImages.Images[0], curX, curY);
                    }

                    lightSquare = !lightSquare;
                }

                lightSquare = !lightSquare;
            }

            boardGraphics.DrawImage(blackRook, 0, 0);            

            return boardBitmap;
        }
    }
}
