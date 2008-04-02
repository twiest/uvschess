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
            Logger.Log("In " + this.Color.ToString() + "'s GetNextMove.");
            _isMyTurn = true;
            _currentBoard = currentBoard.Clone();

            if (this.IsHuman)
            {
                Logger.Log("In " + this.Color.ToString() + "'s GetNextMove and their human.");
                _waitForMoveEvent.Reset();
                _waitForMoveEvent.WaitOne();
            }
            else
            {
                Logger.Log("In " + this.Color.ToString() + "'s GetNextMove and their an AI.");
                _runAIThread = new Thread(GetNextAIMoveInThread);

                // NO LOGGING ALLOWED between here
                _startTime = DateTime.Now;
                _endTime = _startTime.AddMilliseconds(UvsChess.Gui.Preferences.Time);
                _runAIThread.Start();
                // AND HERE because it would count against the AI's time.

                Logger.Log("In " + this.Color.ToString() + "'s GetNextMove and calling StartPollAI().");
                this.PollAIOnce();

                Logger.Log("In " + this.Color.ToString() + "'s GetNextMove waiting for the resetEvent to trigger.");
                _waitForMoveEvent.Reset();
                _waitForMoveEvent.WaitOne();
                Logger.Log("In " + this.Color.ToString() + "'s GetNextMove and just got the resetEvent.");

                _runAIThread = null;
            }

            Logger.Log("In " + this.Color.ToString() + "'s GetNextMove and running GC Cleanup.");
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
            Logger.Log("In " + this.Color.ToString() + "'s EndTurnEarly.");
            if (this.IsHuman)
            {
                Logger.Log("In " + this.Color.ToString() + "'s EndTurnEarly and their human and firing the _waitForMoveEvent.");
                _waitForMoveEvent.Set();
            }
            else
            {
                Logger.Log("In " + this.Color.ToString() + "'s EndTurnEarly and their an AI and setting the _forceAIToEndTurnEarly flag to true.");
                _forceAIToEndTurnEarly = true;
            }
        }

        private void PollAIOnce()
        {
            Logger.Log("In " + this.Color.ToString() + "'s StartPollAI and _pollAITimer == null. Starting the Timer.");
            _pollAITimer = new Timer(this.PollAI, null, Interval, System.Threading.Timeout.Infinite);
        }

        private void PollAI(object state)
        {
            // NO LOGGING ALLOWED between here
            if ( (_forceAIToEndTurnEarly) || (!this.AI.IsRunning) || (DateTime.Now > _endTime) )
            {
                //if (_forceAIToEndTurnEarly)
                //{
                //    Logger.Log("In " + this.Color.ToString() + "'s PollAI and telling the AI to end it's turn because it's been forced to.");
                //}
                //else if (!this.AI.IsGameRunning)
                //{
                //    Logger.Log("In " + this.Color.ToString() + "'s PollAI and telling the AI to end it's turn because it's done running, and it needs to cleanup.");
                //}
                //else
                //{
                //    Logger.Log("In " + this.Color.ToString() + "'s PollAI and telling the AI to end it's turn because it's time is over.");
                //}

                if (this.AI.IsRunning)
                {
                    int gracePeriod = Gui.Preferences.GracePeriod;
                    _pollAITimer = new Timer(this.GracePeriodTimer, null, gracePeriod, System.Threading.Timeout.Infinite);

                    //Logger.Log("In " + this.Color.ToString() + "'s PollAI and Joining _runAIThread");
                    _runAIThread.Join();
                    this.AI.IsRunning = false;
                }

                TimeOfLastMove = DateTime.Now.Subtract(_startTime);
                // AND HERE because it would count against the AI's time.

                //Logger.Log("In " + this.Color.ToString() + "'s PollAI and firing the _waitForMoveEvent.");
                _waitForMoveEvent.Set();
            }
            else
            {
                Logger.Log("In " + this.Color.ToString() + "'s PollAI and setting it to poll one more time.");
                // poll again
                PollAIOnce();
            }

            Logger.Log("In " + this.Color.ToString() + "'s PollAI [End].");
        }

        private void GracePeriodTimer(object state)
        {
            if (this.AI.IsRunning)
            {
                // The AI is still running, even after the grace period.
                // They've now lost!
                Logger.Log(this.ColorAndName + " has gone over the time limit and grace period. Having to abort his AI thread.");
                _moveToReturn = new ChessMove();
                _moveToReturn.Flag = ChessFlag.AIWentOverTime;
                _runAIThread.Abort();
            }
        }

        public void HumanMovedPieceEvent(ChessMove move)
        {
            if ( (_isMyTurn) && (this.IsHuman) )
            {
                Logger.Log("Human Playing " + Color.ToString() + " moved:");
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

    //class HistoryItem
    //{
    //    public string message;
    //    public string fenboard;
    //    public HistoryItem(string message, string fen)
    //    {
    //        this.message = message;
    //        this.fenboard = fen;
    //    }
    //    public override string ToString()
    //    {
    //        return message;
    //    }
    //}

}
