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

        public ChessMove DecidedMove { get; set; }

        public DecisionTree(ChessBoard board)
        {
            UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_ctor_ChessBoard);
            Children = new List<DecisionTree>();
            Board = board;
            DecidedMove = null;
        }

        private DecisionTree(DecisionTree parent, ChessBoard board, ChessMove move)
        {
            UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_ctor_DecisionTree_ChessBoard_ChessMove);
            Children = new List<DecisionTree>();
            Parent = parent;
            Board = board;
            Move = move;
            DecidedMove = null;
        }

        public DecisionTree LastChild
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_get_LastChild);
                return this.Children[this.Children.Count - 1];
            }                
        }

        internal List<DecisionTree> Children
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
                UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_get_ActualMoveValue);
                return Move.ValueOfMove.ToString();
            }
        }

        public string EventualMoveValue
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_get_EventualMoveValue);
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

        internal bool IsRootNode
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_get_IsRootNode);
                return (this.Parent == null);
            }
        }

        internal ChessBoard Board
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_get_Board);
                return _board;
            }

            set
            {
                UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_set_Board);
                _board = value.Clone();
            }
        }

        internal ChessMove Move
        {
            get
            {
                UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_get_Move);
                return _move;
            }

            set
            {
                UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_set_Move);
                _move = value.Clone();
            }
        }

        internal DecisionTree Clone()
        {
            UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_Clone);
            return this.Clone(null);
        }

        public DecisionTree Clone(DecisionTree parent)
        {
            UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_Clone_DecisionTree);
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
            retVal.DecidedMove = this.DecidedMove;

            foreach (DecisionTree curChild in this.Children)
            {
                retVal.Children.Add(curChild.Clone(this));
            }

            return retVal;
        }

        public void AddChild(ChessBoard board, ChessMove move)
        {
            UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_AddChild_ChessBoard_ChessMove);
            this.Children.Add(new DecisionTree(this, board, move));
        }

        public override string ToString()
        {
            UvsChess.Framework.Profiler.AddToMainProfile((int)ProfilerMethodKey.DecisionTree_ToString);
            if (IsRootNode)
            {
                return "Starting Board";
            }
            else
            {
                return Move.ToString();
            }
        }
    }
}
