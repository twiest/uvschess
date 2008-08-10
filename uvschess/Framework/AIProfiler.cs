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
        public int Depth { get; set; }
        public int MinisProfileIndexNumber { get; set; }
        public int MaxsProfileIndexNumber { get; set; }
        private int[] Profile { get; set; }
        private int[] FxProfile { get; set; }
        internal List<int[]> Turns { get; set; }
        private List<int> MoveDepths { get; set; }
        private List<TimeSpan> MoveTimes { get; set; }
        private List<int[]> FxTurns { get; set; }
        internal bool IsEnabled { get; set; }
        public string[] KeyNames { get; set; }
        internal string[] FxKeyNames { get; set; }

        public AIProfiler(ChessColor myColor)
        {
            NodesPerSecond = -1;
            MinisProfileIndexNumber = -1;
            MaxsProfileIndexNumber = -1;
            AIName = string.Empty;
            AIColor = myColor;
            Depth = 0;

            _numFrameworkMethods = Enum.GetValues(typeof(ProfilerMethodKey)).Length;
            IsEnabled = false;
            MoveDepths = new List<int>();
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

        internal void EndTurn(Logger.LogCallback log, TimeSpan moveTime)
        {
            if (IsEnabled)
            {
                if ((KeyNames == null) || KeyNames.Length == 0)
                {
                    log(this.AIColor.ToString() + ": To use the profiler, you must set Profiler.MethodNames to a list of enum names that you used with the integer keys");
                }

                if (Depth != 0)
                {
                    MoveDepths.Add(this.Depth);
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

                    output.Add("\"" + AIColor.ToString() + "'s Moves:\",\"" + this.Turns.Count + "\"");                    

                    StringBuilder sb = new StringBuilder("\"Color\",\"Fx or AI\",\"Method Name\",\"Total\",\"Average\",");
                    for (int ix=1; ix <= Turns.Count; ix++)
                    {
                        sb.Append("\"Move " + ix + "\",");
                    }
                    // Trim off the trailing ", "
                    sb.Remove(sb.Length - 1, 1);

                    output.Add(sb.ToString());

                    if (MoveDepths.Count > 0)
                    {
                        sb = new StringBuilder();
                        int totalDepths = 0;
                        for (int ix = 0; ix < MoveDepths.Count; ix++)
                        {
                            totalDepths += MoveDepths[ix];
                            sb.Append("\"" + MoveDepths[ix].ToString() + "\",");
                        }
                        int averageDepth = totalDepths / MoveTimes.Count;
                        sb.Insert(0, "\"" + AIColor.ToString() + "\",\"AI\",\"Move Depths\",\"\",\"" + averageDepth + "\",");

                        // Trim off the trailing ","
                        sb.Remove(sb.Length - 1, 1);

                        output.Add(sb.ToString());
                    }
                    else
                    {
                        output.Add(AIColor.ToString() + "\",\"AI\",\"Move Depths\",\"Error: Set Profiler.Depth to get depth logging\"");
                    }


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
                        output.Add(WriteProfileLine("AI", Turns, curProfiledMethod, KeyNames, ref total));

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
                        output.Add(WriteProfileLine("Fx", FxTurns, curFxProfiledMethod, FxKeyNames, ref total));
                    }
                }
            }

            return output;
        }

        private string WriteProfileLine(string fxOrAi, List<int[]> info, int currentKey, string[] keyNames, ref int total)
        {
            StringBuilder sb = new StringBuilder();
            for (int curTurn = 0; curTurn < info.Count; curTurn++)
            {
                total += info[curTurn][currentKey];
                sb.Append("\"" + string.Format("{0:N0}", info[curTurn][currentKey]) + "\",");
            }

            sb.Insert(0, "\"" + AIColor.ToString() + "\",\"" + fxOrAi + "\",\"" + keyNames[currentKey].ToString() + "\",\"" +
                         string.Format("{0:N0}", total) + "\",\"" + string.Format("{0:N0}", ((decimal)total / (decimal)info.Count)) + "\",");

            // Trim off the trailing ","
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
