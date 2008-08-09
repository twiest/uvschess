/******************************************************************************
* The MIT License
* Copyright (c) 2008 Rusty Howell, Thomas Wiest
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest  twiest@users.sourceforge.net
//		Rusty Howell  rhowell@users.sourceforge.net

using System;
using UvsChess.Framework;

namespace UvsChess
{
    public class ChessMove:IComparable<ChessMove>
    {        
        public ChessMove(ChessLocation from, ChessLocation to):this(from, to, ChessFlag.NoFlag)
        {
            Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_ctor_ChessLocation_ChessLocation);
        }

        public ChessMove(ChessLocation from, ChessLocation to, ChessFlag flag)
        {
            Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_ctor_ChessLocation_ChessLocation_ChessFlag);
            From = from;
            To = to;
            Flag = flag;
            ToStringPrefix = string.Empty;
        }

        internal string ToStringPrefix { get; set; }

        public ChessFlag Flag
        {
            get;
            set;
        }

        public ChessLocation From
        {
            get;
            set;
        }

        public ChessLocation To
        {
            get;
            set;
        }

        /// <summary>
        /// This is the value of the board after this move has been made.
        /// </summary>
        public int ValueOfMove
        {
            get;
            set;
        }

        /// <summary>
        /// This property is used by the framework to see if the move is on the board,
        /// has a non-null To and From, etc. This property should NOT be used by students
        /// as it doesn't actually check the move to see if it is actually a valid chess
        /// move.
        /// </summary>
        public bool IsBasicallyValid
        {
            get
            {
                Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_get_IsBasicallyValid);
                if ( (this.Flag == ChessFlag.Stalemate) || (this.Flag == ChessFlag.AIWentOverTime) )
                {
                    return true;
                }

                if ((this.To == null) || (this.From == null))
                {
                    return false;
                }

                if ((!this.To.IsValid) || (!this.From.IsValid))
                {
                    return false;
                }

                if (this.From == this.To)
                {
                    return false;
                }

                return true;
            }
        }


        public ChessMove Clone()
        {
            Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_Clone);
            ChessMove newMove = new ChessMove(null, null);

            if (this.To != null)
                newMove.To = this.To.Clone();

            if (this.From != null)
                newMove.From = this.From.Clone();

            newMove.ValueOfMove = this.ValueOfMove;
            newMove.Flag = this.Flag;

            return newMove;
        }

        // TODO: We need to add == and != stuff so they can compare two moves.

        public override string ToString()
        {
            Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_ToString);
            string moveText = string.Empty;

            if (this.From == null)
            {
                moveText += "The Move's From field was null. ";
            }
            else
            {
                if (!this.From.IsValid)
                {
                    moveText += "The Move's From field was outside the bounds of the board coordinates. ";
                }
            }

            if (this.To == null)
            {
                moveText += "The Move's To field was null. ";
            }
            else
            {
                if (!this.To.IsValid)
                {
                    moveText += "The Move's To field was outside the bounds of the board coordinates. ";
                }
            }

            if (this.From == this.To)
            {
                moveText += "The Move's From and To fields are equal. ";
            }

            if ( (moveText != string.Empty) && (this.Flag != ChessFlag.NoFlag) )
            {
                moveText += "Flag: " + this.Flag.ToString();
            }
            else
            {
                moveText = ToStringPrefix + "From: [" + this.From.X + ", " + this.From.Y + "]   To: [" + this.To.X + ", " + this.To.Y + "] ";

                if (this.Flag != ChessFlag.NoFlag)
                {
                    moveText += "Flag: " + this.Flag.ToString(); ;
                }
            }

            moveText.TrimEnd();

            return moveText;
        }
        
        public override bool Equals(object obj)
        {
            Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_Equals);
            if (obj == null)
            {
                return false;
            }
            if (!(obj is ChessMove))
            {
                return false;
            }
            ChessMove move2 = (ChessMove)obj;
            if (this.Flag != move2.Flag)
            {
                return false;
            }
            if ((this.From != move2.From) || (this.To != move2.To))
            {
                return false;
            }

            return true;
        }
        public static bool operator ==(ChessMove move1, ChessMove move2)
        {
            Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_EQ);
            if ((((object)move1) == null) && (((object)move2) == null))
            {
                return true;
            }
            if (((object)move1) == null)
            {
                return false;
            }
            return move1.Equals(move2);
        }

        public static bool operator !=(ChessMove move1, ChessMove move2)
        {
            Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_NE);
            return !(move1 == move2);
        }


        #region IComparable<ChessMove> Members

        public int CompareTo(ChessMove other)
        {
            Profiler.AddToMainProfile((int)ProfilerMethodKey.ChessMove_CompareTo_ChessMove);
            // Sorts it from lowest value move to highest value move
            return (this.ValueOfMove - other.ValueOfMove);
        }

        #endregion
    }
}
