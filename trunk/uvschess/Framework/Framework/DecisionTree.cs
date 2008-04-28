using System;
using System.Collections.Generic;
using System.Text;

namespace UvsChess.Framework
{
    public class DecisionTree
    {
        private ChessMove _move = null;
        private ChessBoard _board = null;
        private string _eventualMoveValue = null;

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

        public string ActualMoveValue
        {
            get
            {
                return Move.ValueOfMove.ToString();
            }
        }

        public string EventualMoveValue
        {
            get
            {
                if (_eventualMoveValue == null)
                {
                    return "Not Set";
                }

                return _eventualMoveValue.ToString();
            }

            set
            {
                _eventualMoveValue = value;
            }
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

            retVal.EventualMoveValue = this.EventualMoveValue;
            retVal.FinalDecision = this.FinalDecision;

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

            foreach (DecisionTree curDt in rootNode.Children)
            {
                if (curDt.Move == move)
                {
                    rootNode.FinalDecision = new DecisionTree(rootNode, board, move);
                    rootNode.FinalDecision.EventualMoveValue = curDt.EventualMoveValue;
                    break;
                }
            }            
        }

        public override string ToString()
        {
            if (IsRootNode)
            {
                return "Starting Board";
            }
            else if ((this.Parent.IsRootNode) && (this == this.Parent.FinalDecision))
            {
                // This is the Final Decision!
                return "Final Decision";
            }
            else
            {
                return Move.ToString();
            }
        }
    }
}
