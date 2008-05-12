using System;
using System.Collections.Generic;
using System.Text;
using UvsChess.Framework;

namespace UvsChess
{
    public class AIProfiler
    {
        internal const int MaxMethodsToProfile = 1000;
        private int _numFrameworkMethods;

        public ChessColor MyColor { get; set; }
        public string MyName { get; set; }
        public int DefaultDepth { get; set; }
        public int MinisProfileIndexNumber { get; set; }
        public int MaxsProfileIndexNumber { get; set; }
        private int[] Profile { get; set; }
        private int[] FxProfile { get; set; }
        internal List<int[]> Turns { get; set; }
        private List<TimeSpan> MoveTimes { get; set; }
        private List<int[]> FxTurns { get; set; }
        internal bool IsEnabled { get; set; }
        public string[] MethodNames { get; set; }
        internal string[] FxMethodNames { get; set; }


        public AIProfiler(ChessColor myColor)
        {
            MinisProfileIndexNumber = -1;
            MaxsProfileIndexNumber = -1;
            MyName = string.Empty;
            MyColor = myColor;
            DefaultDepth = 0;

            _numFrameworkMethods = Enum.GetValues(typeof(ProfilerMethodKey)).Length;
            IsEnabled = false;
            MoveTimes = new List<TimeSpan>();
            Turns = new List<int[]>();
            FxTurns = new List<int[]>();
        }

        public void AddToProfile(int key)
        {
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
            if (IsEnabled)
            {
                ++FxProfile[key];
            }
        }

        internal void WriteAndEndTurn(Logger.LogCallback log, TimeSpan moveTime)
        {
            if (IsEnabled)
            {
                if (MethodNames.Length == 0)
                {
                    log("To use the profiler, you must set Profiler.MethodNames to a list of enum names that you used with the integer keys");
                }
                else
                {
                    log("*** Move Stats ***");
                    log("Move time: " + moveTime.ToString());
                    for (int ix = 0; ix < MethodNames.Length; ix++)
                    {
                        log(string.Format("{0} : {1:N0}", MethodNames[ix], Profile[ix]));
                    }

                    for (int ix = 0; ix < FxMethodNames.Length; ix++)
                    {
                        log(string.Format("{0} : {1:N0}", FxMethodNames[ix], FxProfile[ix]));
                    }
                }

                MoveTimes.Add(moveTime);
                Turns.Add(Profile);
                FxTurns.Add(FxProfile);

                FxProfile = new int[_numFrameworkMethods];
                Profile = new int[MaxMethodsToProfile];
            }
        }

        internal void WriteAndEndGame(Logger.LogCallback log)
        {
            if (IsEnabled)
            {
                if ((MethodNames == null) || (MethodNames.Length == 0))
                {
                    log("To use the profiler, you must set Profiler.MethodNames to a list of enum names that you used with the integer keys");
                }
                else
                {
                    List<string> output = new List<string>();

                    if (MyName != string.Empty)
                    {
                        output.Add("\"" + MyColor.ToString() + "'s AI Name:\",\"" + this.MyName + "\"");
                    }

                    if (DefaultDepth != 0)
                    {
                        output.Add("\"" + MyColor.ToString() + "'s Default Depth:\",\"" + this.DefaultDepth + "\"");
                    }

                    output.Add("\"" + MyColor.ToString() + "'s Moves:\",\"" + this.Turns.Count + "\"");                    

                    StringBuilder sb = new StringBuilder("\"Color\",\"Fx or AI\",\"Method Name\",\"Total\",");
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
                    sb.Insert(0, "\"" + MyColor.ToString() + "\",\"\",\"Move Times\",\"" + totalTime + "\",");

                    // Trim off the trailing ", "
                    sb.Remove(sb.Length - 1, 1);

                    output.Add(sb.ToString());

                    int totalNodes = 0;
                    for (int curProfiledMethod = 0; curProfiledMethod < MethodNames.Length; curProfiledMethod++)
                    {
                        sb = new StringBuilder();
                        int total = 0;
                        for (int curTurn = 0; curTurn < Turns.Count; curTurn++)
                        {
                            total += Turns[curTurn][curProfiledMethod];
                            sb.Append("\"" + string.Format("{0:N0}", Turns[curTurn][curProfiledMethod])+ "\",");
                        }
                        
                        if ((MinisProfileIndexNumber != -1) &&
                            (MaxsProfileIndexNumber != -1) &&
                            ((MinisProfileIndexNumber == curProfiledMethod) ||
                             (MaxsProfileIndexNumber == curProfiledMethod)))
                        {
                            totalNodes += total;
                        }

                        sb.Insert(0, "\"" + MyColor.ToString() + "\",\"AI\",\"" + MethodNames[curProfiledMethod].ToString() + "\",\"" + string.Format("{0:N0}", total) + "\",");

                        // Trim off the trailing ", "
                        sb.Remove(sb.Length - 1, 1);

                        output.Add(sb.ToString());
                    }

                    if (totalNodes > 0)
                    {
                        output.Insert(1, "\"" + MyColor.ToString() + "'s Nodes/Sec:\",\"" + string.Format("{0:N2}", ((double)totalNodes / totalTime.TotalSeconds)) + "\"");
                    }

                    for (int curFxProfiledMethod = 0; curFxProfiledMethod < FxMethodNames.Length; curFxProfiledMethod++)
                    {
                        sb = new StringBuilder();
                        int FxTotal = 0;
                        for (int curFxTurn = 0; curFxTurn < Turns.Count; curFxTurn++)
                        {
                            FxTotal += FxTurns[curFxTurn][curFxProfiledMethod];
                            sb.Append("\"" + string.Format("{0:N0}", FxTurns[curFxTurn][curFxProfiledMethod]) + "\",");
                        }

                        sb.Insert(0, "\"" + MyColor.ToString() + "\",\"Fx\",\"" + FxMethodNames[curFxProfiledMethod].ToString() + "\",\"" + string.Format("{0:N0}", FxTotal) + "\",");

                        // Trim off the trailing ", "
                        sb.Remove(sb.Length - 1, 1);

                        output.Add(sb.ToString());
                    }

                    foreach (string curLine in output)
                    {
                        log(curLine);
                    }
                }
            }
        }
    }
}
