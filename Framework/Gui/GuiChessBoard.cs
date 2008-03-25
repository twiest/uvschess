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
        Bitmap darkTile;
        Bitmap lightTile;
        Bitmap blackRook;

        public GuiChessBoard()
        {
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

            InitializeComponent();
            this.Paint += OnPaint;


            // Validate the image sizes
            int defaultWidth = chessImages.Images[0].Width;
            int defaultHeight = chessImages.Images[0].Height;

            foreach (Image curImage in chessImages.Images)
            {
                if (curImage.Height != defaultHeight)
                {
                    throw (new Exception("Image height is outside the default height!"));
                }

                if (curImage.Width != defaultWidth)
                {
                    throw (new Exception("Image width is outside the default width!"));
                }
            }

            bool lightSquare = true;
            for (int x = 0; x < 8; x++)
            {
                int curX = x * defaultWidth;

                for (int y = 0; y < 8; y++)
                {
                    int curY = y * defaultHeight;

                    if (lightSquare)
                    {
                        //_chessBoard.DrawImage(chessImages.Images[0], curX, curY);
                    }
                    else
                    {
                        //_chessBoard.DrawImage(chessImages.Images[0], curX, curY);
                    }

                    lightSquare = !lightSquare;
                }
            }

            //_chessBoard.
        }

        public void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics dc = e.Graphics;
            //dc.DrawImage(darkTile, 0, 0);
            dc.DrawImage(lightTile, 0, 0);
            dc.DrawImage(blackRook, 0, 0);
            
        }
    }
}
