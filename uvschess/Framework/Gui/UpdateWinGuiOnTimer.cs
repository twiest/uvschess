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
using System.IO;

namespace UvsChess.Gui
{
    public static class UpdateWinGuiOnTimer
    {
        public static WinGui Gui = null;

        private static string WhitePlayerName = string.Empty;
        private static string BlackPlayerName = string.Empty;
        
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

        public static void ResetHistory(ChessState newState)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_ResetHistory, newState));
            }
        }

        public static void UpdateState_CurrentPlayerColor()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_UpdateState_CurrentPlayerColor, null));
            }
        }

        public static void UpdateWhitesName()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_UpdateWhitesName, null));
            }            
        }

        public static void UpdateBlacksName()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_UpdateBlacksName, null));
            }
        }

        public static void UpdateState_HalfMoves()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_UpdateState_HalfMoves, null));
            }            
        }

        public static void UpdateState_FullMoves()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_UpdateState_FullMoves, null));
            }
        }

        public static void UpdateBoardBasedOnLstHistory()
        {            
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_UpdateBoardBasedOnLstHistory, null));
            }
        }

        public static void UpdateBoardBasedOnMove(ChessMove move)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_UpdateBoardBasedOnMove, move));
            }            
        }

        public static void SaveMainLogToDisk()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_SaveMainLogToDisk, null));
            }
        }

        public static void SaveWhitesLogToDisk()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_SaveWhitesLogToDisk, null));
            }
        }

        public static void SaveBlacksLogToDisk()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_SaveBlacksLogToDisk, null));
            }
        }

        public static void ClearMainLog()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_ClearMainLog, null));
            }
        }

        public static void ClearWhitesLog()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_ClearWhitesLog, null));
            }
        }

        public static void ClearBlacksLog()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_ClearBlacksLog, null));
            }
        }

        public static void OpenStateFromDisk()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_OpenFenFromDisk, null));
            }
        }

        public static void SaveSelectedStateToDisk()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_SaveFenToDisk, null));
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

        public static void StartGame()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_StartGame, null));
            }
        }

        public static void StopGame()
        {
            lock (_updateGuiDataLockObject)
            {
                _guiEvents.Add(new GuiEvent(Actually_StopGame, null));
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

        public static void ShutdownGuiEventLoop()
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

                Actually_StopGame(null);
            }
        }

        private static void UpdateGui(object state)
        {
            if (! _isShuttingDown)
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
                        RunThroughAllGuiEvents();
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

        private static void RunThroughAllGuiEvents()
        {
            if (Gui.InvokeRequired)
            {
                Gui.Invoke(new NoParameterCallback(RunThroughAllGuiEvents));
            }
            else
            {
                LstBoxes_BeginUpdate();

                while ((!_isShuttingDown) && (_tmpGuiEvents != null) && (_tmpGuiEvents.Count > 0))
                {
                    GuiEvent curEvent = _tmpGuiEvents[0];

                    // Process the GUI Events one event at a time,
                    // _and_ in the same order that they were received.
                    curEvent.EventCallback(curEvent.EventArgs);

                    _tmpGuiEvents.RemoveAt(0);
                }

                if (!_isShuttingDown)
                {
                    LstBoxes_EndUpdate();
                }
            }
        }

        private static void LstBoxes_BeginUpdate()
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

        private static void LstBoxes_EndUpdate()
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

        private static void Actually_ResetHistory(object newState)
        {
            Gui.lstHistory.Items.Clear();
            Actually_AddToHistory((ChessState) newState);
        }

        private static void Actually_AddToHistory(object state)
        {
            Gui.lstHistory.Items.Add(state);
            Gui.lstHistory.SelectedIndex = Gui.lstHistory.Items.Count - 1;
        }

        private static void Actually_SetGuiChessBoard_IsLocked(object isLocked)
        {
            Gui.chessBoardControl.IsLocked = (bool)isLocked;
        }

        private static void Actually_DeclareResults(object result)
        {
            Actually_StopGame(null);
            System.Windows.Forms.MessageBox.Show(Gui, (string)result);            
        }

        private static void Actually_AddToMainLog(object message)
        {
            Gui.lstMainLog.Items.Add(message);

            if (Gui.chkBxAutoScrollMainLog.Checked)
            {
                Gui.lstMainLog.SelectedIndex = Gui.lstMainLog.Items.Count - 1;
                Gui.lstMainLog.ClearSelected();
            }
        }

        private static void Actually_AddToWhitesLog(object message)
        {
            Gui.lstWhitesLog.Items.Add(message);

            if (Gui.chkBxAutoScrollWhitesLog.Checked)
            {
                Gui.lstWhitesLog.SelectedIndex = Gui.lstWhitesLog.Items.Count - 1;
                Gui.lstWhitesLog.ClearSelected();
            }
        }

        private static void Actually_AddToBlacksLog(object message)
        {
            Gui.lstBlacksLog.Items.Add(message);

            if (Gui.chkBxAutoScrollBlacksLog.Checked)
            {
                Gui.lstBlacksLog.SelectedIndex = Gui.lstBlacksLog.Items.Count - 1;
                Gui.lstBlacksLog.ClearSelected();
            }
        }

        private static ChessState Actually_GetLstHistory_SelectedItem()
        {
            return (ChessState)Gui.lstHistory.SelectedItem;
        }

        private static void Actually_StartGame(object eventArgs)
        {
            if (! _isInGameMode)
            {
                _isInGameMode = true;

                ChessState item = Actually_GetLstHistory_SelectedItem();

                _mainGame = new UvsChess.Framework.ChessGame(item, UpdateWinGuiOnTimer.WhitePlayerName, UpdateWinGuiOnTimer.BlackPlayerName);

                // Remove WinGui from the GuiChessBoard updates
                Gui.chessBoardControl.PieceMovedByHuman -= UpdateWinGuiOnTimer.UpdateBoardBasedOnMove;

                // Add the ChessGame to the GuiChessBoard updates
                Gui.chessBoardControl.PieceMovedByHuman += _mainGame.WhitePlayer_HumanMovedPieceEvent;
                Gui.chessBoardControl.PieceMovedByHuman += _mainGame.BlackPlayer_HumanMovedPieceEvent;

                // Add WinGui to the ChessGame updates event
                _mainGame.UpdatedState += UpdateWinGuiOnTimer.AddToHistory;

                _mainGame.SetGuiChessBoard_IsLocked += UpdateWinGuiOnTimer.SetGuiChessBoard_IsLocked;

                // Add WinGui to the DeclareResults event
                _mainGame.DeclareResults += UpdateWinGuiOnTimer.DeclareResults;

                Actually_RemoveHistoryAfterSelected();

                Actually_DisableMenuItemsDuringPlay();
                Actually_DisableRadioBtnsAndComboBoxes();
                Actually_DisableHistoryWindowClicking();
                UpdateWinGuiOnTimer.SetGuiChessBoard_IsLocked(true);

                _mainGame.StartGame();
            }
        }

        private static void Actually_StopGame(object eventArgs)
        {
            if (_isInGameMode)
            {
                _isInGameMode = false;

                // Remove the ChessGame from the GuiChessBoard updates
                Gui.chessBoardControl.PieceMovedByHuman -= _mainGame.WhitePlayer_HumanMovedPieceEvent;
                Gui.chessBoardControl.PieceMovedByHuman -= _mainGame.BlackPlayer_HumanMovedPieceEvent;

                // Remove WinGui from the ChessGame updates event
                //_mainGame.Updated -= OnChessGameUpdated;
                _mainGame.UpdatedState -= UpdateWinGuiOnTimer.AddToHistory;

                _mainGame.SetGuiChessBoard_IsLocked -= UpdateWinGuiOnTimer.SetGuiChessBoard_IsLocked;

                // Remove WinGui from the DeclareResults event
                _mainGame.DeclareResults -= UpdateWinGuiOnTimer.DeclareResults;

                // Add WinGui to the GuiChessBoard updates
                Gui.chessBoardControl.PieceMovedByHuman += UpdateWinGuiOnTimer.UpdateBoardBasedOnMove;

                Actually_EnableMenuItemsAfterPlay();
                Actually_EnableRadioBtnsAndComboBoxes();
                Actually_EnableHistoryWindowClicking();
                UpdateWinGuiOnTimer.SetGuiChessBoard_IsLocked(false);

                _mainGame.StopGameEarly();

                _mainGame = null;                
            }
        }

        private static void Actually_RemoveHistoryAfterSelected()
        {
            int sel = Gui.lstHistory.SelectedIndex;

            while (Gui.lstHistory.Items.Count > sel + 1)
            {
                Gui.lstHistory.Items.RemoveAt(Gui.lstHistory.Items.Count - 1);
            }
        }

        private static void Actually_DisableMenuItemsDuringPlay()
        {
            Gui.startToolStripMenuItem.Enabled = false;
            Gui.clearHistoryToolStripMenuItem.Enabled = false;
            Gui.newToolStripMenuItem.Enabled = false;

            Gui.openToolStripMenuItem.Enabled = false;
            Gui.saveToolStripMenuItem.Enabled = false;

            Gui.stopToolStripMenuItem.Enabled = true;
        }

        private static void Actually_DisableRadioBtnsAndComboBoxes()
        {
            Gui.radBlack.Enabled = false;
            Gui.radWhite.Enabled = false;
            Gui.cmbBlack.Enabled = false;
            Gui.cmbWhite.Enabled = false;
            Gui.numFullMoves.Enabled = false;
            Gui.numHalfMoves.Enabled = false;
        }

        private static void Actually_DisableHistoryWindowClicking()
        {
            Gui.lstHistory.Enabled = false;
        }

        private static void Actually_EnableMenuItemsAfterPlay()
        {
            Gui.startToolStripMenuItem.Enabled = true;
            Gui.clearHistoryToolStripMenuItem.Enabled = true;
            Gui.newToolStripMenuItem.Enabled = true;

            Gui.openToolStripMenuItem.Enabled = true;
            Gui.saveToolStripMenuItem.Enabled = true;

            Gui.stopToolStripMenuItem.Enabled = false;
        }

        private static void Actually_EnableRadioBtnsAndComboBoxes()
        {
            Gui.radBlack.Enabled = true;
            Gui.radWhite.Enabled = true;
            Gui.cmbBlack.Enabled = true;
            Gui.cmbWhite.Enabled = true;
            Gui.numFullMoves.Enabled = true;
            Gui.numHalfMoves.Enabled = true;
        }

        private static void Actually_EnableHistoryWindowClicking()
        {
            Gui.lstHistory.Enabled = true;
        }

        private static void SaveLogToDisk(string message, System.Windows.Forms.ListBox lstBox)
        {
            Gui.saveFileDialog1.Filter = "Text Files (*.txt) | *.txt";
            System.Windows.Forms.DialogResult result = Gui.saveFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(Gui.saveFileDialog1.FileName);

                AddToMainLog(message + Gui.saveFileDialog1.FileName);

                foreach (string curLine in lstBox.Items)
                {
                    writer.WriteLine(curLine);
                }

                writer.Close();
            }
        }

        private static void Actually_SaveMainLogToDisk(object eventArgs)
        {
            SaveLogToDisk("Saving Main Log to: ", Gui.lstMainLog);
        }

        private static void Actually_SaveWhitesLogToDisk(object eventArgs)
        {
            SaveLogToDisk("Saving White AI's Log to: ", Gui.lstWhitesLog);
        }

        private static void Actually_SaveBlacksLogToDisk(object eventArgs)
        {
            SaveLogToDisk("Saving Black AI's Log to: ", Gui.lstBlacksLog);
        }

        private static void Actually_OpenFenFromDisk(object eventArgs)
        {
            Gui.openFileDialog1.Filter = "FEN files (*.fen)|*.fen|All files (*.*)| *.*";
            System.Windows.Forms.DialogResult result = Gui.openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                AddToMainLog("Resetting chess board");
                StreamReader reader = new StreamReader(Gui.openFileDialog1.FileName);
                string line = reader.ReadLine();

                ResetHistory(new ChessState(line));

                reader.Close();
            }
        }

        private static void Actually_SaveFenToDisk(object eventArgs)
        {
            Gui.saveFileDialog1.Filter = "FEN files (*.fen) | *.fen";
            System.Windows.Forms.DialogResult result = Gui.saveFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(Gui.saveFileDialog1.FileName);

                AddToMainLog("Saving board to: " + Gui.saveFileDialog1.FileName);


                ChessState item = (ChessState)Gui.lstHistory.SelectedItem;
                string fenboard = item.ToFenBoard();

                writer.WriteLine(fenboard);
                writer.Close();
            }
        }

        private static void Actually_ClearMainLog(object eventArgs)
        {
            Gui.lstMainLog.Items.Clear();
        }

        private static void Actually_ClearWhitesLog(object eventArgs)
        {
            Gui.lstWhitesLog.Items.Clear();
        }

        private static void Actually_ClearBlacksLog(object eventArgs)
        {
            Gui.lstBlacksLog.Items.Clear();
        }

        private static void Actually_UpdateState_CurrentPlayerColor(object eventArgs)
        {
            ChessState tmpState = (ChessState)Gui.lstHistory.SelectedItem;

            if (Gui.radWhite.Checked)
            {
                tmpState.CurrentPlayerColor = ChessColor.White;
            }
            else
            {
                tmpState.CurrentPlayerColor = ChessColor.Black;
            }

            Gui.lstHistory.Items[Gui.lstHistory.SelectedIndex] = tmpState;
        }

        private static void Actually_UpdateState_HalfMoves(object eventArgs)
        {
            ChessState state = (ChessState)Gui.lstHistory.SelectedItem;
            state.HalfMoves = Convert.ToInt32(Gui.numHalfMoves.Value);
        }

        private static void Actually_UpdateState_FullMoves(object eventArgs)
        {
            ChessState state = (ChessState)Gui.lstHistory.SelectedItem;
            state.FullMoves = Convert.ToInt32(Gui.numFullMoves.Value);
        }

        private static void Actually_UpdateBoardBasedOnLstHistory(object eventArgs)
        {
            ChessState tmpState = (ChessState)Gui.lstHistory.SelectedItem;
            //ChessState tmpState = new ChessState(item.fenboard);

            if (tmpState.CurrentPlayerColor == ChessColor.White)
            {
                Gui.radWhite.Checked = true;
            }
            else
            {
                Gui.radBlack.Checked = true;
            }

            Gui.numFullMoves.Value = tmpState.FullMoves;
            Gui.numHalfMoves.Value = tmpState.HalfMoves;

            Gui.chessBoardControl.ResetBoard(tmpState.CurrentBoard, tmpState.PreviousMove);
        }

        private static void Actually_UpdateBoardBasedOnMove(object move)
        {
            ChessState tmpState = (ChessState)Gui.lstHistory.SelectedItem;
            //ChessState tmpState = new ChessState(item.fenboard);
            tmpState.MakeMove((ChessMove)move);
            tmpState.PreviousMove = null;
            tmpState.PreviousBoard = null;

            if (Gui.radWhite.Checked)
            {
                tmpState.CurrentPlayerColor = ChessColor.White;
            }
            else
            {
                tmpState.CurrentPlayerColor = ChessColor.Black;
            }

            //item.fenboard = tmpState.ToFenBoard();

            // This causes the "selected index changed event"
            //lstHistory.Items[lstHistory.SelectedIndex] = tmpState;
            Gui.lstHistory.SelectedItem = tmpState;

            // Force update the lstHistory and the GuiChessBoard
            Actually_UpdateBoardBasedOnLstHistory(null);
        }

        private static void Actually_UpdateWhitesName(object eventArgs)
        {
            WhitePlayerName = Gui.cmbWhite.SelectedItem.ToString();
        }

        private static void Actually_UpdateBlacksName(object eventArgs)
        {
            BlackPlayerName = Gui.cmbBlack.SelectedItem.ToString();
        }
    }
}
