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
using System.Collections.Generic;
using System.Threading;

namespace UvsChess.Gui
{
    public static class UpdateWinGuiOnTimer
    {
        public static WinGui Gui = null;
        
        private static int Interval = 50;
        private static object _updateGuiDataLockObject = new object();
        private static object _updateGuiLockObject = new object();
        private static object _pollGuiLockObject = new object();
        private static UvsChess.Framework.ChessGame _mainGame = null;

        private static bool _isInGameMode = false;

        private static List<GuiEvent> _guiEvents = new List<GuiEvent>();
        private static List<GuiEvent> _tmpGuiEvents = null;

        private static bool _isShuttingDown = false;

        private static bool _wasMainLogUpdated = false;
        private static bool _tmpWasMainLogUpdated = false;

        private static bool _wasWhitesLogUpdated = false;
        private static bool _tmpWasWhitesLogUpdated = false;

        private static bool _wasBlacksLogUpdated = false;
        private static bool _tmpWasBlacksLogUpdated = false; 

        private static bool _wasHistoryUpdated = false;
        private static bool _tmpWasHistoryUpdated = false;        

        private static Timer _pollGuiTimer = null;

        delegate void NoParameterCallback();
        delegate ChessState NoParameterChessStateReturnCallback();        
        delegate void ObjectParameterCallback(object obj);

        private class GuiEvent
        {
            public ObjectParameterCallback EventCallback { get; set; }
            public object EventArgs { get; set; }

            public GuiEvent(ObjectParameterCallback eventCallback, object eventArgs)
            {
                EventCallback = eventCallback;
                EventArgs = eventArgs;
            }
        }


        public static void ClearHistory()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_ClearHistory, null));
            }
        }

        public static void DeclareResults(string results)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_DeclareResults, results));
            }
        }

        public static void SetGuiChessBoard_IsLocked(bool isLocked)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_SetGuiChessBoard_IsLocked, isLocked));
            }
        }

        public static void SwitchWinGuiMode(bool isSwitchIntoGameMode)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_SwitchWinGuiMode, isSwitchIntoGameMode));
            }
        }

        public static void AddToMainLog(string message)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_AddToMainLog, message));
                _wasMainLogUpdated = true;
            }
        }

        public static void AddToWhitesLog(string message)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_AddToWhitesLog, message));
                _wasWhitesLogUpdated = true;
            }
        }

        public static void AddToBlacksLog(string message)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_AddToBlacksLog, message));
                _wasBlacksLogUpdated = true;
            }
        }

        public static void AddToHistory(ChessState state)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_AddToHistory, state));
                _wasHistoryUpdated = true;
            }
        }

        public static void PollGuiOnce()
        {
            lock (_pollGuiLockObject)
            {
                // only poll if we're not shutting down
                if (!_isShuttingDown)
                {
                    // Run UpdateGui in <interval> ms, exactly one time. 
                    // In UpdateGui, I'll tell it to run me again, exactly once.
                    _pollGuiTimer = new Timer(UpdateGui, null, Interval, System.Threading.Timeout.Infinite);
                }
            }
        }

        public static void StopGuiPolling()
        {
            lock (_pollGuiLockObject)
            {
                _isShuttingDown = true;

                if (_pollGuiTimer != null)
                {
                    // if there's a timer out there ready to go off, shut it down.
                    _pollGuiTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    _pollGuiTimer = null;
                }
            }
        }

        private static void UpdateGui(object state)
        {
            if (!_isShuttingDown)
            {
                // This should guarantee that we won't lose any data.
                lock (_updateGuiDataLockObject)
                {
                    if (_guiEvents.Count > 0)
                    {
                        _tmpGuiEvents =_guiEvents;
                        _guiEvents = new List<GuiEvent>();
                    }
                    else
                    {
                        _tmpGuiEvents = null;
                    }

                    if (_wasMainLogUpdated)
                    {
                        _tmpWasMainLogUpdated = true;
                        _wasMainLogUpdated = false;
                    }
                    else
                    {
                        _tmpWasMainLogUpdated = false;
                    }

                    if (_wasWhitesLogUpdated)
                    {
                        _tmpWasWhitesLogUpdated = true;
                        _wasWhitesLogUpdated = false;
                    }
                    else
                    {
                        _tmpWasWhitesLogUpdated = false;
                    }

                    if (_wasBlacksLogUpdated)
                    {
                        _tmpWasBlacksLogUpdated = true;
                        _wasBlacksLogUpdated = false;
                    }
                    else
                    {
                        _tmpWasBlacksLogUpdated = false;
                    }

                    if (_wasHistoryUpdated)
                    {
                        _tmpWasHistoryUpdated = true;
                        _wasHistoryUpdated = false;
                    }
                    else
                    {
                        _tmpWasHistoryUpdated = false;
                    }
                }

                lock (_updateGuiLockObject)
                {
                    try
                    {
                        LstBoxes_BeginUpdate();

                        if (_tmpGuiEvents != null)
                        {
                            foreach (GuiEvent curEvent in _tmpGuiEvents)
                            {
                                // Process the GUI Events one event at a time,
                                // _and_ in the same order that they were received.
                                curEvent.EventCallback(curEvent.EventArgs);
                            }
                        }

                        LstBoxes_EndUpdate();
                    }
                    catch (Exception e)
                    {
                        // this is to catch any errant exceptions that might 
                        // be thrown when we shut down (if the form is closing 
                        // and we're trying to update the gui)
                        int a = 1;
                    }

                    // Setup to Poll Again in <interval> ms
                    PollGuiOnce();
                }
            }
        }

        private static void LstBoxes_BeginUpdate()
        {
            if (Gui.InvokeRequired)
            {
                Gui.Invoke(new NoParameterCallback(LstBoxes_BeginUpdate));
            }
            else
            {
                if (_tmpWasHistoryUpdated)
                {
                    Gui.lstHistory.BeginUpdate();
                }

                if (_tmpWasMainLogUpdated)
                {
                    Gui.lstMainLog.BeginUpdate();
                }

                if (_tmpWasWhitesLogUpdated)
                {
                    Gui.lstWhitesLog.BeginUpdate();
                }

                if (_tmpWasBlacksLogUpdated)
                {
                    Gui.lstBlacksLog.BeginUpdate();
                }
            }
        }

        private static void LstBoxes_EndUpdate()
        {
            if (Gui.InvokeRequired)
            {
                Gui.Invoke(new NoParameterCallback(LstBoxes_EndUpdate));
            }
            else
            {
                if (_tmpWasHistoryUpdated)
                {
                    Gui.lstHistory.EndUpdate();
                }

                if (_tmpWasMainLogUpdated)
                {
                    Gui.lstMainLog.EndUpdate();
                }

                if (_tmpWasWhitesLogUpdated)
                {
                    Gui.lstWhitesLog.EndUpdate();
                }

                if (_tmpWasBlacksLogUpdated)
                {
                    Gui.lstBlacksLog.EndUpdate();
                }
            }
        }

        private static void Actually_ClearHistory(object eventArgs)
        {
            Gui.lstHistory.Items.Clear();
        }

        private static void Actually_AddToHistory(object state)
        {
            if (Gui.lstHistory.InvokeRequired)
            {
                Gui.lstHistory.Invoke(new ObjectParameterCallback(Actually_AddToHistory), new object[] { state });
            }
            else
            {                    
                Gui.lstHistory.Items.Add(state);
                Gui.lstHistory.SelectedIndex = Gui.lstHistory.Items.Count - 1;                    
            }
        }

        private static void Actually_SetGuiChessBoard_IsLocked(object isLocked)
        {
            if (Gui.chessBoardControl.InvokeRequired)
            {
                Gui.chessBoardControl.Invoke(new ObjectParameterCallback(Actually_SetGuiChessBoard_IsLocked), new object[] { isLocked });
            }
            else
            {
                Gui.chessBoardControl.IsLocked = (bool)isLocked;
            }
        }

        private static void Actually_DeclareResults(object result)
        {
            if (Gui.InvokeRequired)
            {
                Gui.Invoke(new ObjectParameterCallback(Actually_DeclareResults), new object[] { result});
            }
            else
            {
                 System.Windows.Forms.MessageBox.Show(Gui, (string)result);
            }
        }

        private static void Actually_AddToMainLog(object message)
        {
            if (Gui.lstMainLog.InvokeRequired)
            {
                Gui.lstMainLog.Invoke(new ObjectParameterCallback(Actually_AddToMainLog), new object[] { message });
            }
            else
            {
                Gui.lstMainLog.Items.Add(message);
 
                if (Gui.chkBxAutoScrollMainLog.Checked)
                {
                    Gui.lstMainLog.SelectedIndex = Gui.lstMainLog.Items.Count - 1;
                    Gui.lstMainLog.ClearSelected();
                }
            }
        }

        private static void Actually_AddToWhitesLog(object message)
        {
            if (Gui.lstWhitesLog.InvokeRequired)
            {
                Gui.lstWhitesLog.Invoke(new ObjectParameterCallback(Actually_AddToWhitesLog), new object[] { message });
            }
            else
            {
                Gui.lstWhitesLog.Items.Add(message);

                if (Gui.chkBxAutoScrollWhitesLog.Checked)
                {
                    Gui.lstWhitesLog.SelectedIndex = Gui.lstWhitesLog.Items.Count - 1;
                    Gui.lstWhitesLog.ClearSelected();
                }
            }
        }

        private static void Actually_AddToBlacksLog(object message)
        {
            if (Gui.lstBlacksLog.InvokeRequired)
            {
                Gui.lstBlacksLog.Invoke(new ObjectParameterCallback(Actually_AddToBlacksLog), new object[] { message });
            }
            else
            {
                Gui.lstBlacksLog.Items.Add(message);

                if (Gui.chkBxAutoScrollBlacksLog.Checked)
                {
                    Gui.lstBlacksLog.SelectedIndex = Gui.lstBlacksLog.Items.Count - 1;
                    Gui.lstBlacksLog.ClearSelected();
                }
            }
        }

        private static ChessState Actually_GetLstHistory_SelectedItem()
        {
            if (Gui.lstHistory.InvokeRequired)
            {
                return (ChessState)Gui.lstHistory.Invoke(new NoParameterChessStateReturnCallback(Actually_GetLstHistory_SelectedItem));
            }
            else
            {
                return (ChessState)Gui.lstHistory.SelectedItem;
            }
        }

        private static void Actually_SwitchWinGuiMode(object shouldSwitchIntoGameMode)
        {
            if ((! _isInGameMode) && ((bool)shouldSwitchIntoGameMode))
            {
                _isInGameMode = true;


                ChessState item = Actually_GetLstHistory_SelectedItem();

                _mainGame = new UvsChess.Framework.ChessGame(item, Gui.WhitePlayerName, Gui.BlackPlayerName);



                // Remove WinGui from the GuiChessBoard updates
                Gui.chessBoardControl.PieceMovedByHuman -= Gui.GuiChessBoardChangedByHuman;

                // Add the ChessGame to the GuiChessBoard updates
                Gui.chessBoardControl.PieceMovedByHuman += _mainGame.WhitePlayer_HumanMovedPieceEvent;
                Gui.chessBoardControl.PieceMovedByHuman += _mainGame.BlackPlayer_HumanMovedPieceEvent;

                // Add WinGui to the ChessGame updates event
                //_mainGame.Updated += OnChessGameUpdated;
                _mainGame.UpdatedState += Gui.OnChessGameUpdated;

                _mainGame.SetGuiChessBoard_IsLocked += UpdateWinGuiOnTimer.SetGuiChessBoard_IsLocked;

                // Add WinGui to the DeclareResults event
                _mainGame.DeclareResults += Gui.OnChessGameDeclareResults;

                Gui.RemoveHistoryAfterSelected();

                Gui.DisableMenuItemsDuringPlay();
                Gui.DisableRadioBtnsAndComboBoxes();
                Gui.DisableHistoryWindowClicking();
                UpdateWinGuiOnTimer.SetGuiChessBoard_IsLocked(true);

                _mainGame.StartGame();
            }

            if ( (_isInGameMode) && (! (bool)shouldSwitchIntoGameMode) )
            {
                _isInGameMode = false;

                // Remove the ChessGame from the GuiChessBoard updates
                Gui.chessBoardControl.PieceMovedByHuman -= _mainGame.WhitePlayer_HumanMovedPieceEvent;
                Gui.chessBoardControl.PieceMovedByHuman -= _mainGame.BlackPlayer_HumanMovedPieceEvent;

                // Remove WinGui from the ChessGame updates event
                //_mainGame.Updated -= OnChessGameUpdated;
                _mainGame.UpdatedState -= Gui.OnChessGameUpdated;

                _mainGame.SetGuiChessBoard_IsLocked -= UpdateWinGuiOnTimer.SetGuiChessBoard_IsLocked;

                // Remove WinGui from the DeclareResults event
                _mainGame.DeclareResults -= Gui.OnChessGameDeclareResults;

                // Add WinGui to the GuiChessBoard updates
                Gui.chessBoardControl.PieceMovedByHuman += Gui.GuiChessBoardChangedByHuman;

                Gui.EnableMenuItemsAfterPlay();
                Gui.EnableRadioBtnsAndComboBoxes();
                Gui.EnableHistoryWindowClicking();
                UpdateWinGuiOnTimer.SetGuiChessBoard_IsLocked(false);

                _mainGame.StopGameEarly();

                _mainGame = null;                
            }
        }
    }
}
