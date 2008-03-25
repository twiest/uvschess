namespace UvsChess.Gui
{
    partial class GuiChessBoard
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GuiChessBoard));
            this.chessImages = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // chessImages
            // 
            this.chessImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("chessImages.ImageStream")));
            this.chessImages.TransparentColor = System.Drawing.Color.Transparent;
            this.chessImages.Images.SetKeyName(0, "Chess_LightBackground.png");
            this.chessImages.Images.SetKeyName(1, "Chess_DarkBackground.png");
            this.chessImages.Images.SetKeyName(2, "ChessPiece_BlackRook.png");
            // 
            // GuiChessBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "GuiChessBoard";
            this.Size = new System.Drawing.Size(610, 521);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList chessImages;
    }
}
