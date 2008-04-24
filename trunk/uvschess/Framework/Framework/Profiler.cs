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
using System.Text;
using UvsChess.Gui;

namespace UvsChess.Framework
{
    public class Profiler
    {
        private static object profilerLock = new object();

        private static Dictionary<string, int> _items = new Dictionary<string, int>();
        private static Dictionary<string, int> _WhiteItems = new Dictionary<string, int>();
        private static Dictionary<string, int> _BlackItems = new Dictionary<string, int>();

        public static void Profile(string key)
        {
            lock (profilerLock)
            {
                if (_items.ContainsKey(key))
                {
                    ++_items[key];
                }
                else
                {
                    _items.Add(key, 1);
                }
            }

        }
        public static void Profile(string key,ChessColor color)
        {
            if (color == ChessColor.White)
            {
                AddToWhitesProfile(key);
            }
            else
            {
                AddToBlacksProfile(key);
            }           

        }
        
        public static void AddToWhitesProfile(string key)
        {
            lock (profilerLock)
            {
                if (_WhiteItems.ContainsKey(key))
                {
                    ++_WhiteItems[key];
                }
                else
                {
                    _WhiteItems.Add(key, 1);
                }
            }
        }

        public static void AddToBlacksProfile(string key)
        {
            lock (profilerLock)
            {
                if (_BlackItems.ContainsKey(key))
                {
                    ++_BlackItems[key];
                }
                else
                {
                    _BlackItems.Add(key, 1);
                }
            }
        }

        public static void ClearAll()
        {
            lock (profilerLock)
            {
                _items = new Dictionary<string, int>();
                _WhiteItems = new Dictionary<string, int>();
                _BlackItems = new Dictionary<string, int>();
            }
        }
        public static void Clear(ChessColor color)
        {
            lock (profilerLock)
            {
                if (color == ChessColor.White)
                {
                    _WhiteItems = new Dictionary<string, int>();
                }
                else
                {
                    _BlackItems = new Dictionary<string, int>();
                }
            }
        }

        public static void Write()
        {
            lock (profilerLock)
            {
                if (_items.Count == 0)
                {
                    return;
                }

                Logger.Log("*** Profiler stats ***");
                foreach (string s in _items.Keys)
                {
                    Logger.Log(string.Format("{0} : {1}",s,_items[s]));
                }
            }
        }

        public static void Write(ChessColor color)
        {
            if (color == ChessColor.White)
            {
                WriteWhitesProfiles();
            }
            else
            {
                WriteBlacksProfiles();
            }

        }
        private static void WriteWhitesProfiles()
        {
            lock (profilerLock)
            {
                if (_WhiteItems.Count == 0)
                {
                    return;
                }
                UpdateWinGuiOnTimer.AddToWhitesLog("*** Profiler stats ***");
                foreach (string s in _WhiteItems.Keys)
                {
                    UpdateWinGuiOnTimer.AddToWhitesLog(string.Format("{0} : {1}", s, _WhiteItems[s]));
                }
            }
        }
        private static void WriteBlacksProfiles()
        {
            lock (profilerLock)
            {
                if (_BlackItems.Count == 0)
                {
                    return;
                }

                UpdateWinGuiOnTimer.AddToBlacksLog("*** Profiler stats ***");
                foreach (string s in _BlackItems.Keys)
                {
                    UpdateWinGuiOnTimer.AddToBlacksLog(string.Format("{0} : {1}", s, _BlackItems[s]));
                }
            }        
        }
            
    }
}
