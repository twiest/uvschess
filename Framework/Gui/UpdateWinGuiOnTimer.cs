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
        private static List<string> AddToMainLog_Parameter1 = new List<string>();
        private static List<string> AddToWhitesLog_Parameter1 = new List<string>();
        private static List<string> AddToBlacksLog_Parameter1 = new List<string>();
        private static List<string> AddToHistory_Parameter1 = new List<string>();
        private static List<string> AddToHistory_Parameter2 = new List<string>();        
        private static Timer _pollGuiTimer = null;

        public static void AddToMainLog(string param1)
        {
            lock (_updateGuiDataLockObject)
            {
                AddToMainLog_Parameter1.Add(param1);
            }
        }

        public static void AddToWhitesLog(string param1)
        {
            lock (_updateGuiDataLockObject)
            {
                AddToWhitesLog_Parameter1.Add(param1);
            }
        }

        public static void AddToBlacksLog(string param1)
        {
            lock (_updateGuiDataLockObject)
            {
                AddToBlacksLog_Parameter1.Add(param1);
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
            List<string> tmpAddToMainLog_Parameter1 = null;
            List<string> tmpAddToWhitesLog_Parameter1 = null;
            List<string> tmpAddToBlacksLog_Parameter1 = null;
            List<string> tmpAddToHistory_Parameter1 = null;
            List<string> tmpAddToHistory_Parameter2 = null;

            // This should guarantee that we won't lose any data.
            lock (_updateGuiDataLockObject)
            {                
                if (AddToMainLog_Parameter1.Count > 0)
                {
                    tmpAddToMainLog_Parameter1 = new List<string>(AddToMainLog_Parameter1);
                    AddToMainLog_Parameter1.Clear();
                }

                if (AddToWhitesLog_Parameter1.Count > 0)
                {
                    tmpAddToWhitesLog_Parameter1 = new List<string>(AddToWhitesLog_Parameter1);
                    AddToWhitesLog_Parameter1.Clear();
                }

                if (AddToBlacksLog_Parameter1.Count > 0)
                {
                    tmpAddToBlacksLog_Parameter1 = new List<string>(AddToBlacksLog_Parameter1);
                    AddToBlacksLog_Parameter1.Clear();
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
                    if ( (tmpAddToMainLog_Parameter1 != null) && (tmpAddToMainLog_Parameter1.Count > 0) )
                    {
                        Gui.AddToMainLog(tmpAddToMainLog_Parameter1);                        
                    }

                    if ((tmpAddToWhitesLog_Parameter1 != null) && (tmpAddToWhitesLog_Parameter1.Count > 0))
                    {
                        Gui.AddToWhitesLog(tmpAddToWhitesLog_Parameter1);
                    }

                    if ((tmpAddToBlacksLog_Parameter1 != null) && (tmpAddToBlacksLog_Parameter1.Count > 0))
                    {
                        Gui.AddToBlacksLog(tmpAddToBlacksLog_Parameter1);
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
