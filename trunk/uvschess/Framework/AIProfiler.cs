﻿using System;
using System.Collections.Generic;
using System.Text;
using UvsChess.Framework;

namespace UvsChess
{
    public class AIProfiler
    {
        internal const int MaxMethodsToProfile = 1000;
        private int _numFrameworkMethods;
        private bool _onlyProfileMiniMax = false;

        internal double NodesPerSecond { get; set; }
        public ChessColor AIColor { get; set; }
        public string AIName { get; set; }
        public int DefaultDepth { get; set; }
        public int MinisProfileIndexNumber { get; set; }
        public int MaxsProfileIndexNumber { get; set; }
        private int[] Profile { get; set; }
        private int[] FxProfile { get; set; }
        internal List<int[]> Turns { get; set; }
        private List<TimeSpan> MoveTimes { get; set; }
        private List<int[]> FxTurns { get; set; }
        internal bool IsEnabled { get; set; }
        public string[] KeyNames { get; set; }
        internal string[] FxKeyNames { get; set; }


        public bool OnlyProfileMiniMax
        {
            set
            {
                if (MinisProfileIndexNumber < 0)
                {
                    throw (new Exception("AIProfiler.MinisProfileIndexNumber needs to be set before you can set this property."));
                }

                if (MaxsProfileIndexNumber < 0)
                {
                    throw (new Exception("AIProfiler.MaxsProfileIndexNumber needs to be set before you can set this property."));
                }

                _onlyProfileMiniMax = value;
            }

            internal get
            {
                return _onlyProfileMiniMax;
            }
        }


        public AIProfiler(ChessColor myColor)
        {
            NodesPerSecond = -1;
            MinisProfileIndexNumber = -1;
            MaxsProfileIndexNumber = -1;
            AIName = string.Empty;
            AIColor = myColor;
            DefaultDepth = 0;

            _numFrameworkMethods = Enum.GetValues(typeof(ProfilerMethodKey)).Length;
            IsEnabled = false;
            MoveTimes = new List<TimeSpan>();
            Turns = new List<int[]>();
            FxTurns = new List<int[]>();
        }

        public void AddToProfile(int key)
        {
            if (OnlyProfileMiniMax && (key != MinisProfileIndexNumber) && (key != MaxsProfileIndexNumber))
            {
                return;
            }

            if (! IsEnabled)
            {
                IsEnabled = true;                
            }

            if (Profile == null)
            {
                FxProfile = new int[_numFrameworkMethods];
                Profile = new int[MaxMethodsToProfile];
            }

            ++Profile[key];
        }

        internal void AddToFxProfile(int key)
        {
            if ((IsEnabled) && (! OnlyProfileMiniMax))
            {
                ++FxProfile[key];
            }
        }

        internal void WriteAndEndTurn(Logger.LogCallback log, TimeSpan moveTime)
        {
            if (IsEnabled)
            {
                if ((KeyNames == null) || KeyNames.Length == 0)
                {
                    log(this.AIColor.ToString() + ": To use the profiler, you must set Profiler.MethodNames to a list of enum names that you used with the integer keys");
                }
                else
                {
                    log("*** Move Stats ***");
                    log("Move time: " + moveTime.ToString());
                    for (int ix = 0; ix < KeyNames.Length; ix++)
                    {
                        log(string.Format("{0} : {1:N0}", KeyNames[ix], Profile[ix]));
                    }

                    for (int ix = 0; ix < FxKeyNames.Length; ix++)
                    {
                        log(string.Format("{0} : {1:N0}", FxKeyNames[ix], FxProfile[ix]));
                    }
                }

                MoveTimes.Add(moveTime);
                Turns.Add(Profile);
                FxTurns.Add(FxProfile);

                FxProfile = new int[_numFrameworkMethods];
                Profile = new int[MaxMethodsToProfile];
            }
        }

        internal List<string> WriteAndEndGame()
        {
            List<string> output = new List<string>();
            if (IsEnabled)
            {
                if ((KeyNames == null) || (KeyNames.Length == 0))
                {
                    output.Add(this.AIColor.ToString() + ": To use the profiler, you must set Profiler.MethodNames to a list of enum names that you used with the integer keys");
                }
                else
                {
                    if (AIName != string.Empty)
                    {
                        output.Add("\"" + AIColor.ToString() + "'s AI Name:\",\"" + this.AIName + "\"");
                    }

                    if (DefaultDepth != 0)
                    {
                        output.Add("\"" + AIColor.ToString() + "'s Default Depth:\",\"" + this.DefaultDepth + "\"");
                    }

                    output.Add("\"" + AIColor.ToString() + "'s Moves:\",\"" + this.Turns.Count + "\"");                    

                    StringBuilder sb = new StringBuilder("\"Color\",\"Fx or AI\",\"Method Name\",\"Total\",\"Average\",");
                    for (int ix=1; ix <= Turns.Count; ix++)
                    {
                        sb.Append("\"Move " + ix + "\",");
                    }
                    // Trim off the trailing ", "
                    sb.Remove(sb.Length - 1, 1);

                    output.Add(sb.ToString());

                    sb = new StringBuilder();
                    TimeSpan totalTime = new TimeSpan();
                    for (int ix = 0; ix < MoveTimes.Count; ix++)
                    {
                        totalTime += MoveTimes[ix];
                        sb.Append("\"" + MoveTimes[ix].ToString() + "\",");
                    }
                    TimeSpan averageTime = TimeSpan.FromMilliseconds((totalTime.TotalMilliseconds / MoveTimes.Count));
                    sb.Insert(0, "\"" + AIColor.ToString() + "\",\"AI\",\"Move Times\",\"" + totalTime + "\",\"" + averageTime + "\",");

                    // Trim off the trailing ","
                    sb.Remove(sb.Length - 1, 1);

                    output.Add(sb.ToString());

                    int totalNodes = 0;
                    for (int curProfiledMethod = 0; curProfiledMethod < KeyNames.Length; curProfiledMethod++)
                    {
                        int total = 0;
                        output.Add(WriteProfileLine(Turns, curProfiledMethod, KeyNames, ref total));

                        if ((MinisProfileIndexNumber != -1) &&
                            (MaxsProfileIndexNumber != -1) &&
                            ((MinisProfileIndexNumber == curProfiledMethod) ||
                             (MaxsProfileIndexNumber == curProfiledMethod)))
                        {
                            totalNodes += total;
                        }                       
                    }

                    if (totalNodes > 0)
                    {
                        this.NodesPerSecond = ((double)totalNodes / totalTime.TotalSeconds);
                        output.Insert(1, "\"" + AIColor.ToString() + "'s Nodes/Sec:\",\"" + string.Format("{0:N2}", this.NodesPerSecond) + "\"");
                    }

                    for (int curFxProfiledMethod = 0; curFxProfiledMethod < FxKeyNames.Length; curFxProfiledMethod++)
                    {
                        int total = 0;
                        output.Add(WriteProfileLine(FxTurns, curFxProfiledMethod, FxKeyNames, ref total));
                    }
                }
            }

            return output;
        }

        private string WriteProfileLine(List<int[]> info, int currentKey, string[] keyNames, ref int total)
        {
            StringBuilder sb = new StringBuilder();
            for (int curTurn = 0; curTurn < info.Count; curTurn++)
            {
                total += info[curTurn][currentKey];
                sb.Append("\"" + string.Format("{0:N0}", info[curTurn][currentKey]) + "\",");
            }

            sb.Insert(0, "\"" + AIColor.ToString() + "\",\"AI\",\"" + keyNames[currentKey].ToString() + "\",\"" +
                         string.Format("{0:N0}", total) + "\",\"" + string.Format("{0:N0}", ((decimal)total / (decimal)info.Count)) + "\",");

            // Trim off the trailing ","
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
