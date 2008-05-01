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
            UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.ctor(ChessBoard)");
            Children = new List<DecisionTree>();
            Board = board;
        }

        private DecisionTree(DecisionTree parent, ChessBoard board, ChessMove move)
        {
            UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.ctor(DecisionTree, ChessBoard, ChessMove)");
            Children = new List<DecisionTree>();
            Parent = parent;
            Board = board;
            Move = move;
        }

        public DecisionTree LastChild
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.get_LastChild()");
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
                UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.get_ActualMoveValue()");
                return Move.ValueOfMove.ToString();
            }
        }

        public string EventualMoveValue
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.get_EventualMoveValue()");
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
                UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.get_IsRootNode()");
                return (this.Parent == null);
            }
        }

        public ChessBoard Board
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.get_Board()");
                return _board;
            }

            set
            {
                UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.set_Board()");
                _board = value.Clone();
            }
        }

        public ChessMove Move
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.get_Move()");
                return _move;
            }

            set
            {
                UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.set_Move()");
                _move = value.Clone();
            }
        }

        public DecisionTree Clone()
        {
            UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.Clone()");
            return this.Clone(null);
        }

        public DecisionTree Clone(DecisionTree parent)
        {
            UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.Clone(DecisionTree)");
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
            UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.AddChild(ChessBoard, ChessMove)");
            this.Children.Add(new DecisionTree(this, board, move));
        }

        public void AddFinalDecision(ChessBoard board, ChessMove move)
        {
            UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.AddFinalDecision(ChessBoard, ChessMove)");
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
            UvsChess.Framework.Profiler.AddToMainProfile("DecisionTree.ToString()");
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
