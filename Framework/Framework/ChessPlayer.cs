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

        private DateTime _startTime;
        private DateTime _endTime;
        private Thread _runAIThread;
        private bool _isMyTurn = false;
        private ChessBoard _currentBoard = null;
        private ChessMove _moveToReturn;
        private ManualResetEvent _pieceMovedByHumanEvent = new ManualResetEvent(true);
        private int Interval = 100;
        private Timer _pollAITimer;

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

        public ChessMove GetNextMove(ChessBoard currentBoard)
        {
            _isMyTurn = true;
            _currentBoard = currentBoard.Clone();

            if (this.IsHuman)
            {
                _pieceMovedByHumanEvent.Reset();
                _pieceMovedByHumanEvent.WaitOne();
            }
            else
            {
                _runAIThread = new Thread(GetNextAIMove);

                _startTime = DateTime.Now;
                _endTime = _startTime.AddMilliseconds(UvsChess.Gui.Preferences.Time);
                _runAIThread.Start();

                this.StartPollingAI();

                _pieceMovedByHumanEvent.Reset();
                _pieceMovedByHumanEvent.WaitOne();

                _runAIThread = null;
            }

            _isMyTurn = false;

            // Clean up the heap for the next player.
            // This doesn't cost the AI time 
            //(it's done after the AI's turn time has been calculated).
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return _moveToReturn;
        }

        public void EndTurnEarly()
        {
            if (this.IsHuman)
            {
                _pieceMovedByHumanEvent.Set();
            }
            else if (_runAIThread != null)
            {
                StopPollingAI();
                this.AI.EndTurn();
                _runAIThread.Join();

                _pieceMovedByHumanEvent.Set();
            }
        }

        private void StartPollingAI()
        {
            _pollAITimer = new Timer(this.PollAI, null, Interval, Interval);
        }

        private void StopPollingAI()
        {
            _pollAITimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        private void PollAI(object state)
        {
            this.StopPollingAI();

            if ((!this.AI.IsRunning) || 
                (DateTime.Now > _endTime))
            {
                this.AI.EndTurn();
                _runAIThread.Join();

                TimeOfLastMove = DateTime.Now.Subtract(_startTime);

                _pieceMovedByHumanEvent.Set();
            }
            else
            {
                // poll again
                StartPollingAI();
            }
        }

        public void HumanMovedPieceEvent(ChessMove move)
        {
            if ( (_isMyTurn) && (this.IsHuman) )
            {
                Logger.Log("Human Playing " + Color.ToString() + " moved:");
                _moveToReturn = move;
                _pieceMovedByHumanEvent.Set();
            }
        }

        private void GetNextAIMove()
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
