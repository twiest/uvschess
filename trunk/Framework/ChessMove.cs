/******************************************************************************
* The MIT License
* Copyright (c) 2006 Rusty Howell, Thomas Wiest
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
    public class ChessMove
    {        
        private ChessLocation _from;
        private ChessLocation _to;        
        private ChessFlag _flag = ChessFlag.NoFlag;

        public ChessFlag Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }
        
        public ChessLocation From
        {
            get { return _from; }
            set { _from = value; }
        }       

        public ChessLocation To
        {
            get { return _to; }
            set { _to = value; }
        }


        public ChessMove Clone()
        {
            ChessMove newMove = new ChessMove();

            if (this.To == null)
                newMove.To = null;
            else
                newMove.To = this.To.Clone();

            if (this.From == null)
                newMove.From = null;
            else
                newMove.From = this.From.Clone();

            newMove.Flag = this.Flag;

            return newMove;
        }

        public override string ToString()
        {
            return "From: [" + this.From.Row + ", " + this.From.Column + "]   To: [" + this.To.Row + ", " + this.To.Column + "]";
        }
    }
}
