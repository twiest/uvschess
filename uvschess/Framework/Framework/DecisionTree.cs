using System;
using System.Collections.Generic;
using System.Text;

namespace UvsChess.Framework
{
    public class DecisionTree
    {
        private ChessMove _move = null;
        private ChessBoard _board = null;

        public DecisionTree(ChessBoard board)
        {
            Children = new List<DecisionTree>();
            Board = board;
        }

        private DecisionTree(DecisionTree parent, ChessBoard board, ChessMove move)
        {
            Children = new List<DecisionTree>();
            Parent = parent;
            Board = board;
            Move = move;
        }

        public DecisionTree LastChild
        {
            get
            {
                return this.Children[this.Children.Count - 1];
            }                
        }

        public DecisionTree FinalDecision
        {
            get;
            private set;
        }

        public List<DecisionTree> Children
        {
            get;
            set;
        }

        public DecisionTree Parent
        {
            get;
            private set;
        }

        public bool IsRootNode
        {
            get
            {
                return (this.Parent == null);
            }
        }

        public ChessBoard Board
        {
            get
            {
                return _board;
            }

            set
            {
                _board = value.Clone();
            }
        }

        public ChessMove Move
        {
            get
            {
                return _move;
            }

            set
            {
                _move = value.Clone();
            }
        }

        public DecisionTree Clone()
        {
            return this.Clone(null);
        }

        public DecisionTree Clone(DecisionTree parent)
        {
            DecisionTree retVal = null;

            if (parent == null)
            {
                retVal = new DecisionTree(this.Board);
            }
            else
            {
                retVal = new DecisionTree(parent, this.Board, this.Move);
            }

            foreach (DecisionTree curChild in this.Children)
            {
                retVal.Children.Add(curChild.Clone(this));
            }

            return retVal;
        }

        public void AddChild(ChessBoard board, ChessMove move)
        {
            this.Children.Add(new DecisionTree(this, board, move));
        }

        public void AddFinalDecision(ChessBoard board, ChessMove move)
        {
            DecisionTree rootNode = this;
            while (! rootNode.IsRootNode)
            {
                rootNode = rootNode.Parent;
            }

            rootNode.FinalDecision = new DecisionTree(rootNode, board, move);
        }

        public override string ToString()
        {
            if (IsRootNode)
                return "Starting Board";
            else
                return Move.ToString();
        }
    }
}
