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
        
        private static int Interval = 10;
        private static object _updateGuiDataLockObject = new object();
        private static object _updateGuiLockObject = new object();
        private static List<string> AddToMainOutput_Parameter1 = new List<string>();
        private static List<string> AddToWhiteAILog_Parameter1 = new List<string>();
        private static List<string> AddToBlackAILog_Parameter1 = new List<string>();
        private static List<string> AddToHistory_Parameter1 = new List<string>();
        private static List<string> AddToHistory_Parameter2 = new List<string>();        
        private static Timer _pollGuiTimer = null;

        public static void AddToMainOutput(string param1)
        {
            lock (_updateGuiDataLockObject)
            {
                AddToMainOutput_Parameter1.Add(param1);
            }
        }

        public static void AddToWhiteAILog(string param1)
        {
            lock (_updateGuiDataLockObject)
            {
                AddToWhiteAILog_Parameter1.Add(param1);
            }
        }

        public static void AddToBlackAILog(string param1)
        {
            lock (_updateGuiDataLockObject)
            {
                AddToBlackAILog_Parameter1.Add(param1);
            }
        }

        public static void AddToHistory(string param1, string param2)
        {
            lock (_updateGuiDataLockObject)
            {
                AddToHistory_Parameter1.Add(param1);
                AddToHistory_Parameter2.Add(param2);
            }
        }

        public static void PollGuiOnce()
        {
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
            //StopGuiPolling();
            List<string> tmpAddToMainOutput_Parameter1 = null;
            List<string> tmpAddToWhiteAILog_Parameter1 = null;
            List<string> tmpAddToBlackAILog_Parameter1 = null;
            List<string> tmpAddToHistory_Parameter1 = null;
            List<string> tmpAddToHistory_Parameter2 = null;

            // This should guarantee that we won't lose any data.
            lock (_updateGuiDataLockObject)
            {                
                if (AddToMainOutput_Parameter1.Count > 0)
                {
                    tmpAddToMainOutput_Parameter1 = new List<string>(AddToMainOutput_Parameter1);
                    AddToMainOutput_Parameter1.Clear();
                }

                if (AddToWhiteAILog_Parameter1.Count > 0)
                {
                    tmpAddToWhiteAILog_Parameter1 = new List<string>(AddToWhiteAILog_Parameter1);
                    AddToWhiteAILog_Parameter1.Clear();
                }

                if (AddToBlackAILog_Parameter1.Count > 0)
                {
                    tmpAddToBlackAILog_Parameter1 = new List<string>(AddToBlackAILog_Parameter1);
                    AddToBlackAILog_Parameter1.Clear();
                }

                if (AddToHistory_Parameter1.Count > 0)
                {
                    tmpAddToHistory_Parameter1 = new List<string>(AddToHistory_Parameter1);
                    tmpAddToHistory_Parameter2 = new List<string>(AddToHistory_Parameter2);
                    AddToHistory_Parameter1.Clear();
                    AddToHistory_Parameter2.Clear();
                }
            }

            lock (_updateGuiLockObject)
            {                
                try
                {
                    if ( (tmpAddToMainOutput_Parameter1 != null) && (tmpAddToMainOutput_Parameter1.Count > 0) )
                    {
                        Gui.AddToMainOutput(tmpAddToMainOutput_Parameter1);                        
                    }

                    if ((tmpAddToWhiteAILog_Parameter1 != null) && (tmpAddToWhiteAILog_Parameter1.Count > 0))
                    {
                        Gui.AddToWhiteAILog(tmpAddToWhiteAILog_Parameter1);
                    }

                    if ((tmpAddToBlackAILog_Parameter1 != null) && (tmpAddToBlackAILog_Parameter1.Count > 0))
                    {
                        Gui.AddToBlackAILog(tmpAddToBlackAILog_Parameter1);
                    }

                    if ((tmpAddToHistory_Parameter1 != null) && (tmpAddToHistory_Parameter1.Count > 0))
                    {
                        Gui.AddToHistory(tmpAddToHistory_Parameter1, tmpAddToHistory_Parameter2);
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
    }
}
