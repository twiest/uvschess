using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UvsChess.Gui
{
    public partial class GuiDecisionTree : Form
    {
        UvsChess.Framework.DecisionTree _dt = null;

        public GuiDecisionTree(UvsChess.Framework.DecisionTree dt)
        {
            InitializeComponent();
            _dt = dt;
            guiChessBoard1.ResetBoard(_dt.Board);
        }
    }
}
