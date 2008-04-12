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

        private static List<string> _DeclareResults_Parameter1 = new List<string>();
        private static List<string> _AddToMainLog_Parameter1 = new List<string>();
        private static List<string> _AddToWhitesLog_Parameter1 = new List<string>();
        private static List<string> _AddToBlacksLog_Parameter1 = new List<string>();
        private static List<string> _AddToHistory_Parameter1 = new List<string>();
        private static List<string> _AddToHistory_Parameter2 = new List<string>();
        private static List<ChessState> _AddToHistory_ChessState = new List<ChessState>();
        private static bool? _isSwitchIntoGameMode_Parameter1 = null;
        private static bool? _guiChessBoard_IsLocked = null;

        private static Timer _pollGuiTimer = null;


        public delegate void ChessStateListParameterCallback(List<ChessState> states);
        public delegate void BoolParameterCallback(bool val);

        public delegate void StringListParameterCallback(List<string> text);
        

        public static void DeclareResults(string results)
        {
            lock (_updateGuiDataLockObject)
            {
                _DeclareResults_Parameter1.Add(results);
            }
        }

        public static void SetGuiChessBoard_IsLocked(bool isLocked)
        {
            lock (_updateGuiDataLockObject)
            {
                _guiChessBoard_IsLocked = isLocked;
            }
        }

        public static void SwitchWinGuiMode(bool isSwitchIntoGameMode)
        {
            lock (_updateGuiDataLockObject)
            {
                _isSwitchIntoGameMode_Parameter1 = isSwitchIntoGameMode;
            }
        }

        public static void AddToMainLog(string messages)
        {
            lock (_updateGuiDataLockObject)
            {
                _AddToMainLog_Parameter1.Add(messages);
            }
        }

        public static void AddToWhitesLog(string messages)
        {
            lock (_updateGuiDataLockObject)
            {
                _AddToWhitesLog_Parameter1.Add(messages);
            }
        }

        public static void AddToBlacksLog(string messages)
        {
            lock (_updateGuiDataLockObject)
            {
                _AddToBlacksLog_Parameter1.Add(messages);
            }
        }

        public static void AddToHistory(ChessState state)
        {
            lock (_updateGuiDataLockObject)
            {
                _AddToHistory_ChessState.Add(state);
            }
        }

        public static void PollGuiOnce()
        {
            // Run UpdateGui in <interval> ms, exactly one time. 
            // In UpdateGui, I'll tell it to run me again, exactly once.
            _pollGuiTimer = new Timer(UpdateGui, null, Interval, System.Threading.Timeout.Infinite);
        }

        public static void StopGuiPolling()
        {
            if (_pollGuiTimer != null)
            {
                _pollGuiTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _pollGuiTimer = null;
            }
        }

        private static void UpdateGui(object state)
        {

            List<string> tmpDeclareResults_Parameter1 = null;
            List<string> tmpAddToMainLog_Parameter1 = null;
            List<string> tmpAddToWhitesLog_Parameter1 = null;
            List<string> tmpAddToBlacksLog_Parameter1 = null;
            List<string> tmpAddToHistory_Parameter1 = null;
            List<string> tmpAddToHistory_Parameter2 = null;
            List<ChessState> tmpAddToHistory_ChessState = null;
            bool? tmpIsSwitchIntoGameMode = null;
            bool? tmpGuiChessBoard_IsLocked = null;

            // This should guarantee that we won't lose any data.
            lock (_updateGuiDataLockObject)
            {
                if (_DeclareResults_Parameter1.Count > 0)
                {
                    tmpDeclareResults_Parameter1 = new List<string>(_DeclareResults_Parameter1);
                    _DeclareResults_Parameter1.Clear();
                }

                if (_guiChessBoard_IsLocked != null)
                {
                    tmpGuiChessBoard_IsLocked = _guiChessBoard_IsLocked.Value;
                    _guiChessBoard_IsLocked = null;
                }

                if (_isSwitchIntoGameMode_Parameter1 != null)
                {
                    tmpIsSwitchIntoGameMode = _isSwitchIntoGameMode_Parameter1.Value;
                    _isSwitchIntoGameMode_Parameter1 = null;
                }

                if (_AddToMainLog_Parameter1.Count > 0)
                {
                    tmpAddToMainLog_Parameter1 = new List<string>(_AddToMainLog_Parameter1);
                    _AddToMainLog_Parameter1.Clear();
                }

                if (_AddToWhitesLog_Parameter1.Count > 0)
                {
                    tmpAddToWhitesLog_Parameter1 = new List<string>(_AddToWhitesLog_Parameter1);
                    _AddToWhitesLog_Parameter1.Clear();
                }

                if (_AddToBlacksLog_Parameter1.Count > 0)
                {
                    tmpAddToBlacksLog_Parameter1 = new List<string>(_AddToBlacksLog_Parameter1);
                    _AddToBlacksLog_Parameter1.Clear();
                }

                if (_AddToHistory_Parameter1.Count > 0)
                {
                    tmpAddToHistory_Parameter1 = new List<string>(_AddToHistory_Parameter1);
                    tmpAddToHistory_Parameter2 = new List<string>(_AddToHistory_Parameter2);
                    _AddToHistory_Parameter1.Clear();
                    _AddToHistory_Parameter2.Clear();
                }

                if (_AddToHistory_ChessState.Count > 0)
                {
                    tmpAddToHistory_ChessState = new List<ChessState>(_AddToHistory_ChessState);
                    _AddToHistory_ChessState.Clear();
                }
            }

            lock (_updateGuiLockObject)
            {                
                try
                {
                    // History MUST be on top, since it's the one that updates the GuiChessBoard control
                    if ((tmpAddToHistory_ChessState != null) && (tmpAddToHistory_ChessState.Count > 0))
                    {
                        Actually_AddToHistory(tmpAddToHistory_ChessState);
                    }

                    if (tmpGuiChessBoard_IsLocked != null)
                    {
                        Actually_SetGuiChessBoard_IsLocked(tmpGuiChessBoard_IsLocked.Value);
                    }

                    if (tmpIsSwitchIntoGameMode != null)
                    {
                        Gui.SwitchWinGuiMode(tmpIsSwitchIntoGameMode.Value);
                    }

                    if ((tmpDeclareResults_Parameter1 != null) && (tmpDeclareResults_Parameter1.Count > 0))
                    {
                        Actually_DeclareResults(tmpDeclareResults_Parameter1);                        
                    }                    

                    if ( (tmpAddToMainLog_Parameter1 != null) && (tmpAddToMainLog_Parameter1.Count > 0) )
                    {
                        Actually_AddToMainLog(tmpAddToMainLog_Parameter1);                        
                    }

                    if ((tmpAddToWhitesLog_Parameter1 != null) && (tmpAddToWhitesLog_Parameter1.Count > 0))
                    {
                        Actually_AddToWhitesLog(tmpAddToWhitesLog_Parameter1);
                    }

                    if ((tmpAddToBlacksLog_Parameter1 != null) && (tmpAddToBlacksLog_Parameter1.Count > 0))
                    {
                        Actually_AddToBlacksLog(tmpAddToBlacksLog_Parameter1);
                    }
                }
                catch
                {
                    // this is to catch any errant exceptions that might 
                    // be thrown when we shut down (if the form is closing 
                    // and we're trying to update the gui)
                }

                // Setup to Poll Again in <interval> ms
                PollGuiOnce();
            }
        }

        private static void Actually_AddToHistory(List<ChessState> states)
        {
            if (Gui.lstHistory.InvokeRequired)
            {
                Gui.lstHistory.Invoke(new ChessStateListParameterCallback(Actually_AddToHistory), new object[] { states });
            }
            else
            {
                Gui.lstHistory.BeginUpdate();
                //lstHistory.Items.AddRange(states.ToArray());
                foreach (ChessState state in states)
                {
                    Gui.lstHistory.Items.Add(state);
                }
                //lstHistory.Items.Add("hello");
                Gui.lstHistory.SelectedIndex = Gui.lstHistory.Items.Count - 1;
                Gui.lstHistory.EndUpdate();
            }
        }

        private static void Actually_SetGuiChessBoard_IsLocked(bool isLocked)
        {
            if (Gui.chessBoardControl.InvokeRequired)
            {
                Gui.chessBoardControl.Invoke(new BoolParameterCallback(SetGuiChessBoard_IsLocked), new object[] { isLocked });
            }
            else
            {
                Gui.chessBoardControl.IsLocked = isLocked;
            }
        }

        private static void Actually_DeclareResults(List<string> results)
        {
            if (Gui.InvokeRequired)
            {
                Gui.Invoke(new StringListParameterCallback(Actually_DeclareResults), new object[] { results });
            }
            else
            {
                foreach (string curResult in results)
                {
                    System.Windows.Forms.MessageBox.Show(Gui, curResult);
                }
            }
        }

        private static void Actually_AddToMainLog(List<string> messages)
        {
            if (Gui.lstMainLog.InvokeRequired)
            {
                Gui.lstMainLog.Invoke(new StringListParameterCallback(Actually_AddToMainLog), new object[] { messages });
            }
            else
            {
                Gui.lstMainLog.BeginUpdate();
                Gui.lstMainLog.Items.AddRange(messages.ToArray());
                Gui.lstMainLog.Items.Add("----" + Gui.lstMainLog.Items.Count.ToString() + "----" + messages.Count.ToString() + "----");

                if (Gui.chkBxAutoScrollMainLog.Checked)
                {
                    Gui.lstMainLog.SelectedIndex = Gui.lstMainLog.Items.Count - 1;
                    Gui.lstMainLog.ClearSelected();
                }

                Gui.lstMainLog.EndUpdate();
            }
        }

        private static void Actually_AddToWhitesLog(List<string> messages)
        {
            if (Gui.lstWhitesLog.InvokeRequired)
            {
                Gui.lstWhitesLog.Invoke(new StringListParameterCallback(Actually_AddToWhitesLog), new object[] { messages });
            }
            else
            {
                Gui.lstWhitesLog.BeginUpdate();
                Gui.lstWhitesLog.Items.AddRange(messages.ToArray());
                Gui.lstWhitesLog.Items.Add("----" + Gui.lstWhitesLog.Items.Count.ToString() + "----" + messages.Count.ToString() + "----");

                if (Gui.chkBxAutoScrollWhitesLog.Checked)
                {
                    Gui.lstWhitesLog.SelectedIndex = Gui.lstWhitesLog.Items.Count - 1;
                    Gui.lstWhitesLog.ClearSelected();
                }
                Gui.lstWhitesLog.EndUpdate();
            }
        }

        private static void Actually_AddToBlacksLog(List<string> messages)
        {
            if (Gui.lstBlacksLog.InvokeRequired)
            {
                Gui.lstBlacksLog.Invoke(new StringListParameterCallback(Actually_AddToBlacksLog), new object[] { messages });
            }
            else
            {
                Gui.lstBlacksLog.BeginUpdate();
                Gui.lstBlacksLog.Items.AddRange(messages.ToArray());
                Gui.lstBlacksLog.Items.Add("----" + Gui.lstBlacksLog.Items.Count.ToString() + "----" + messages.Count.ToString() + "----");

                if (Gui.chkBxAutoScrollBlacksLog.Checked)
                {
                    Gui.lstBlacksLog.SelectedIndex = Gui.lstBlacksLog.Items.Count - 1;
                    Gui.lstBlacksLog.ClearSelected();
                }

                Gui.lstBlacksLog.EndUpdate();
            }
        }
    }
}
