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


// TODO: Go through and make sure that only the namespaces used are the ones referenced.
using System;
using System.Collections.Generic;
using System.Threading;

namespace UvsChess.Framework
{
    // Students don't need to see this class
    class ChessPlayer
    {
        public ChessColor Color;
        public string AIName;
        public IChessAI AI;
        public TimeSpan TimeOfLastMove = TimeSpan.MinValue;

        private ChessBoard _currentBoard = null;
        private ChessMove _moveToReturn;
        private ManualResetEvent _pieceMovedByHumanEvent = new ManualResetEvent(true);


        public ChessPlayer(ChessColor color)
        {
            Color = color;
        }

        public bool IsHuman
        {
            get { return ((this.AIName == null) || (this.AIName == "Human")); }
        }

        public bool IsComputer
        {
            get { return !IsHuman; }
        }

        public ChessMove GetNextMove(ChessBoard currentBoard, ref UvsChess.Gui.GuiChessBoard.PieceMovedByHumanDelegate PieceMovedByHumanDelegate)
        {
            _currentBoard = currentBoard.Clone();

            if (this.IsHuman)
            {
                // Hook up the GUI chess event
                PieceMovedByHumanDelegate += this.HumanMovedPieceEvent;

                _pieceMovedByHumanEvent.Reset();
                _pieceMovedByHumanEvent.WaitOne();

                // Take away the event since my turn is over
                PieceMovedByHumanDelegate -= this.HumanMovedPieceEvent;
            }
            else
            {
                Thread thread = new Thread(threadedNextMove);

                DateTime startTime = DateTime.Now;
                DateTime endTime = startTime.AddMilliseconds(UserPrefs.Time);
                thread.Start();

                //while (player.AI.IsRunning && (startTime.AddMilliseconds(UserPrefs.Time)) < DateTime.Now)
                while (this.AI.IsRunning && (DateTime.Now < endTime))
                {
                    Thread.Sleep(100);
                }

                this.AI.EndTurn();
                thread.Join();

                TimeOfLastMove = DateTime.Now.Subtract(startTime);
            }

            return _moveToReturn;
        }

        public void HumanMovedPieceEvent(ChessMove move)
        {
            Logger.Log("Human Playing " + Color.ToString() + " moved:");
            _moveToReturn = move;
            _pieceMovedByHumanEvent.Set();
        }

        private void threadedNextMove()
        {
            _moveToReturn = this.AI.GetNextMove(_currentBoard, this.Color);
        }
    }

    // TODO: Separate all classes into their own files.
    // Students don't need to see this class
    class AI
    {
        public string FullName;
        public string ShortName;
        public string FileName;

        public AI(string shortName)
        {
            ShortName = shortName;
        }
    }

    class HistoryItem
    {
        public string message;
        public string fenboard;
        public HistoryItem(string message, string fen)
        {
            this.message = message;
            this.fenboard = fen;
        }
        public override string ToString()
        {
            return message;
        }
    }

}
