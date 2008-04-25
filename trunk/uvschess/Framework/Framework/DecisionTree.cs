using System;
using System.Collections.Generic;
using System.Text;

namespace UvsChess.Framework
{
    public class DecisionTree
    {
        private ChessMove _move = null;
        private ChessBoard _board = null;

        public DecisionTree()
        {
            Level = 0;
            Children = new List<DecisionTree>();
        }

        private DecisionTree(DecisionTree parent, ChessBoard board, ChessMove move, int level)
        {
            Children = new List<DecisionTree>();
            Parent = parent;
            Board = board;
            Move = move;
            Level = level;
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

        private int Level
        {
            get;
            set;
        }

        private List<DecisionTree> Children
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

        public void AddChild(ChessBoard board, ChessMove move)
        {
            this.Children.Add(new DecisionTree(this, board, move, this.Level + 1));
        }

        public void AddFinalDecision(ChessBoard board, ChessMove move)
        {
            FinalDecision = new DecisionTree(this, board, move, 0);
        }

        //public void AddChild(ChessBoard board, ChessMove move)
        //{
        //    currentNode.Children.Add(new DecisionTree(currentNode, board, move, currentNode.Level + 1));
        //}

        //public void GoDownOneLevel()
        //{
        //    currentNode = currentNode.Children[currentNode.Children.Count - 1];
        //}

        //public void GoUpOneLevel()
        //{
        //    currentNode = currentNode.Parent;
        //}
    }
}
