/******************************************************************************
* The MIT License
* Copyright (c) 2008 Rusty Howell, Thomas Wiest
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
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



namespace UvsChess
{
    /// <summary>
    /// Represents a location on the chess board.
    /// </summary>
    public class ChessLocation
    {
        #region Members
        private int _x;
        private int _y;
        #endregion

        #region Constructors
        public ChessLocation(int x, int y)
        {
            X = x;
            Y = y;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Column number. Ranges from 0 - 7
        /// </summary>
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>
        /// Row number. Ranges from 0 - 7
        /// </summary>
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        /// <summary>
        /// Returns true if the row and column numbers are each between 0 and 7
        /// </summary>
        public bool IsValid
        {
            get
            {
                return ((X >= 0) && (X < ChessBoard.NumberOfColumns) &&
                         (Y >= 0) && (Y < ChessBoard.NumberOfRows));
            }
        }
        #endregion

        #region Methods and Operators
        public ChessLocation Clone()
        {
            return new ChessLocation(this.X, this.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is ChessLocation))
                return false;

            ChessLocation tmpLoc = (ChessLocation)obj;

            if ((this.X == tmpLoc.X) &&
                 (this.Y == tmpLoc.Y))
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(ChessLocation lhs, ChessLocation rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator== (ChessLocation lhs, ChessLocation rhs)
        {
            if ( ((object) lhs == null) &&
                 ((object) rhs == null) )
            {
                return true;
            }

            if ( ((object) lhs == null) ||
                 ((object) rhs == null) )
            {
                return false;
            }

            return lhs.Equals(rhs);                 
        }
	
	    public override int GetHashCode()
	    {
            return _x * 10 + _y;
	    }
        #endregion
    }
}
