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
        private static List<string> AddToMainOutput_Parameter1 = new List<string>();
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
            List<string> tmpAddToHistory_Parameter1 = null;
            List<string> tmpAddToHistory_Parameter2 = null;

            lock (_updateGuiDataLockObject)
            {
                // This should guarantee that we won't lose any data.
                if (AddToMainOutput_Parameter1.Count > 0)
                {
                    tmpAddToMainOutput_Parameter1 = new List<string>(AddToMainOutput_Parameter1);
                    AddToMainOutput_Parameter1.Clear();
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
