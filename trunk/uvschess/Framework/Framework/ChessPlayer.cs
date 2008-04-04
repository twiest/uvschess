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

        private bool _aiWentOverTime = false;
        private bool _forceAIToEndTurnEarly = false;
        private DateTime _startTime;
        private DateTime _endTime;
        private Thread _runAIThread;
        private bool _isMyTurn = false;
        private ChessBoard _currentBoard = null;
        private ChessMove _moveToReturn;
        private ManualResetEvent _waitForMoveEvent = new ManualResetEvent(true);
        private int Interval = 100;
        private Timer _pollAITimer;

        public string ColorAndName
        {
            get
            {
                return this.Color.ToString() + " (" + this.AIName + ")";
            }
        }

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
                _waitForMoveEvent.Reset();
                _waitForMoveEvent.WaitOne();
            }
            else
            {
                _runAIThread = new Thread(GetNextAIMoveInThread);

                // NO LOGGING ALLOWED between here
                _startTime = DateTime.Now;
                _endTime = _startTime.AddMilliseconds(UvsChess.Gui.Preferences.Time);
                _runAIThread.Start();
                // AND HERE because it would count against the AI's time.

                this.PollAIOnce();

                _waitForMoveEvent.Reset();
                _waitForMoveEvent.WaitOne();

                _runAIThread = null;
            }

            // Clean up the heap for the next player.
            // This doesn't cost the AI time 
            //(it's done after the AI's turn time has been calculated).
            GC.Collect();
            GC.WaitForPendingFinalizers();

            _isMyTurn = false;

            return _moveToReturn;
        }

        public void EndTurnEarly()
        {
            if (this.IsHuman)
            {
                _waitForMoveEvent.Set();
            }
            else
            {
                _forceAIToEndTurnEarly = true;
            }
        }

        private void PollAIOnce()
        {
            _pollAITimer = new Timer(this.PollAI, null, Interval, System.Threading.Timeout.Infinite);
        }

        private void PollAI(object state)
        {
            // NO LOGGING ALLOWED between here
            if ( (_forceAIToEndTurnEarly) || (!this.AI.IsRunning) || (DateTime.Now > _endTime) )
            {
                if (this.AI.IsRunning)
                {
                    this.AI.IsRunning = false;

                    int gracePeriod = Gui.Preferences.GracePeriod;
                    _pollAITimer = new Timer(this.GracePeriodTimer, null, gracePeriod, System.Threading.Timeout.Infinite);

                    _runAIThread.Join();
                    
                }

                TimeOfLastMove = DateTime.Now.Subtract(_startTime);
                // AND HERE because it would count against the AI's time.

                _waitForMoveEvent.Set();
            }
            else
            {
                // poll again
                PollAIOnce();
            }
        }

        private void GracePeriodTimer(object state)
        {
            if (this.AI.IsRunning)
            {
                // The AI is still running, even after the grace period.
                // They've now lost!
                _moveToReturn = new ChessMove(null, null);
                _moveToReturn.Flag = ChessFlag.AIWentOverTime;
                _runAIThread.Abort();
            }
        }

        public void HumanMovedPieceEvent(ChessMove move)
        {
            if ( (_isMyTurn) && (this.IsHuman) )
            {
                _moveToReturn = move;
                _waitForMoveEvent.Set();
            }
        }

        private void GetNextAIMoveInThread()
        {
            // This is the only place that IsRunning should be set to true.
            this.AI.IsRunning = true;
            _moveToReturn = this.AI.GetNextMove(_currentBoard, this.Color);
            this.AI.IsRunning = false;
        }
    }

}
