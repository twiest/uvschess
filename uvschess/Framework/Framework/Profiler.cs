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
        private static bool _isEnabled = true;
        private static object profilerLock = new object();

        private static Dictionary<string, int> _MainItems = new Dictionary<string, int>();
        private static Dictionary<string, int> _WhiteItems = new Dictionary<string, int>();
        private static Dictionary<string, int> _BlackItems = new Dictionary<string, int>();

        private static Dictionary<string, int> _WholeGame_WhitesMainItems = new Dictionary<string, int>();
        private static Dictionary<string, int> _WholeGame_BlacksMainItems = new Dictionary<string, int>();
        private static Dictionary<string, int> _WholeGame_WhiteItems = new Dictionary<string, int>();
        private static Dictionary<string, int> _WholeGame_BlackItems = new Dictionary<string, int>();

        private static void AddToAProfile(string key, Dictionary<string, int> items)
        {
            if (_isEnabled)
            {
                lock (profilerLock)
                {
                    if (items.ContainsKey(key))
                    {
                        ++items[key];
                    }
                    else
                    {
                        items.Add(key, 1);
                    }
                }
            }
        }

        public static void AddToMainProfile(string key)
        {
            if (_isEnabled)
            {
                AddToAProfile(key, _MainItems);
                AddToAProfile("WholeGameWhite_" + key, _WholeGame_WhitesMainItems);
                AddToAProfile("WholeGameBlack_" + key, _WholeGame_BlacksMainItems);
            }

            //lock (profilerLock)
            //{
            //    if (_MainItems.ContainsKey(key))
            //    {
            //        ++_MainItems[key];
            //        ++_WholeGame_MainItems["WholeGame_" + key];
            //    }
            //    else
            //    {
            //        _MainItems.Add(key, 1);
            //        _WholeGame_MainItems.Add("WholeGame_" + key, 1);
            //    }
            //}
        }

        public static void AddToWhitesProfile(string key)
        {
            if (_isEnabled)
            {
                AddToAProfile(key, _WhiteItems);
                AddToAProfile("WholeGame_" + key, _WholeGame_WhiteItems);
            }
            //lock (profilerLock)
            //{
            //    if (_WhiteItems.ContainsKey(key))
            //    {
            //        ++_WhiteItems[key];
            //        ++_WholeGame_WhiteItems["WholeGame_" + key];
            //    }
            //    else
            //    {
            //        _WhiteItems.Add(key, 1);
            //        _WholeGame_WhiteItems.Add("WholeGame_" + key, 1);
            //    }
            //}
        }

        public static void AddToBlacksProfile(string key)
        {
            if (_isEnabled)
            {
                AddToAProfile(key, _BlackItems);
                AddToAProfile("WholeGame_" + key, _WholeGame_BlackItems);
            }
            //lock (profilerLock)
            //{
            //    if (_BlackItems.ContainsKey(key))
            //    {
            //        ++_BlackItems[key];
            //        ++_WholeGame_BlackItems["WholeGame_" + key];
            //    }
            //    else
            //    {
            //        _BlackItems.Add(key, 1);
            //        _WholeGame_BlackItems.Add("WholeGame_" + key, 1);
            //    }
            //}
        }

        //public static void Profile(string key,ChessColor color)
        //{
        //    if (color == ChessColor.White)
        //    {
        //        AddToWhitesProfile(key);
        //    }
        //    else
        //    {
        //        AddToBlacksProfile(key);
        //    }   
        //}

        public static void ClearAllProfiles()
        {
            if (_isEnabled)
            {
                lock (profilerLock)
                {
                    _MainItems = new Dictionary<string, int>();
                    _WhiteItems = new Dictionary<string, int>();
                    _BlackItems = new Dictionary<string, int>();

                    _WholeGame_WhitesMainItems = new Dictionary<string, int>();
                    _WholeGame_BlacksMainItems = new Dictionary<string, int>();
                    _WholeGame_WhiteItems = new Dictionary<string, int>();
                    _WholeGame_BlackItems = new Dictionary<string, int>();
                }
            }
        }

        public static void ClearPlayersPerMoveProfiles(ChessColor color)
        {
            if (_isEnabled)
            {
                lock (profilerLock)
                {
                    _MainItems = new Dictionary<string, int>();
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
        }

        private static void WriteProfile(string description, Dictionary<string, int> profile, Logger.LogCallback log)
        {
            if (_isEnabled)
            {
                lock (profilerLock)
                {
                    if (profile.Count == 0)
                    {
                        return;
                    }

                    log("Begin: " + description);
                    foreach (string s in profile.Keys)
                    {
                        log(string.Format("{0} : {1:N0}", s, profile[s]));
                    }
                    log("End: " + description);
                }
            }
        }

        public static void WriteMainMoveProfile(ChessColor color)
        {
            if (_isEnabled)
            {
                WriteProfile("*** Framework Profile Move Stats during " + color.ToString() + "'s turn ***", _MainItems, Logger.Log);
            }
            //lock (profilerLock)
            //{
            //    if (_MainItems.Count == 0)
            //    {
            //        return;
            //    }

            //    Logger.Log("*** Profiler stats ***");
            //    foreach (string s in _MainItems.Keys)
            //    {
            //        Logger.Log(string.Format("{0} : {1}", s, _MainItems[s]));
            //    }
            //}
        }

        public static void WritePlayerMoveProfile(ChessColor color)
        {
            if (_isEnabled)
            {
                if (color == ChessColor.White)
                {
                    WriteProfile("*** White's AI Profile Move Stats ***", _WhiteItems, Logger.AddToWhitesLog);
                    //WriteWhitesProfiles();
                }
                else
                {
                    WriteProfile("*** Blacks's AI Profile Move Stats ***", _BlackItems, Logger.AddToBlacksLog);
                    //WriteBlacksProfiles();
                }
            }
        }

        public static void WriteMainWholeGameProfile(ChessColor color)
        {
            if (_isEnabled)
            {
                if (color == ChessColor.White)
                {
                    WriteProfile("*** Framework Profile Game Stats during White's turns ***", _WholeGame_WhitesMainItems, Logger.Log);
                }
                else
                {
                    WriteProfile("*** Framework Profile Game Stats during Black's turns ***", _WholeGame_BlacksMainItems, Logger.Log);
                }
            }
        }

        public static void WritePlayerWholeGameProfile(ChessColor color)
        {
            if (_isEnabled)
            {
                if (color == ChessColor.White)
                {
                    WriteProfile("*** White's AI Profile Game Stats ***", _WholeGame_WhiteItems, Logger.AddToWhitesLog);
                }
                else
                {
                    WriteProfile("*** Blacks's AI Profile Game Stats ***", _WholeGame_BlackItems, Logger.AddToBlacksLog);
                }
            }
        }

        //private static void WriteWhitesProfiles()
        //{
        //    lock (profilerLock)
        //    {
        //        if (_WhiteItems.Count == 0)
        //        {
        //            return;
        //        }
        //        UpdateWinGuiOnTimer.AddToWhitesLog("*** Profiler stats ***");
        //        foreach (string s in _WhiteItems.Keys)
        //        {
        //            UpdateWinGuiOnTimer.AddToWhitesLog(string.Format("{0} : {1}", s, _WhiteItems[s]));
        //        }
        //    }
        //}
        //private static void WriteBlacksProfiles()
        //{
        //    lock (profilerLock)
        //    {
        //        if (_BlackItems.Count == 0)
        //        {
        //            return;
        //        }

        //        UpdateWinGuiOnTimer.AddToBlacksLog("*** Profiler stats ***");
        //        foreach (string s in _BlackItems.Keys)
        //        {
        //            UpdateWinGuiOnTimer.AddToBlacksLog(string.Format("{0} : {1}", s, _BlackItems[s]));
        //        }
        //    }        
        //}
            
    }
}
