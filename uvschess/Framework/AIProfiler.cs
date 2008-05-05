using System;
using System.Collections.Generic;
using System.Text;
using UvsChess.Framework;

namespace UvsChess
{
    public class AIProfiler
    {
        internal const int MaxMethodsToProfile = 1000;

        private int[] Profile { get; set; }
        private int[] FxProfile { get; set; }
        private List<int[]> Turns { get; set; }
        private List<int[]> FxTurns { get; set; }
        internal bool IsEnabled { get; set; }
        public string[] MethodNames { get; set; }
        internal string[] FxMethodNames { get; set; }
        

        public AIProfiler()
        {
            int numFrameworkMethods = Enum.GetValues(typeof(ProfilerMethodKey)).Length;
            FxProfile = new int[numFrameworkMethods];
            Profile = new int[MaxMethodsToProfile];
            IsEnabled = false;
            Turns = new List<int[]>();
            FxTurns = new List<int[]>();
        }

        public void AddToProfile(int key)
        {
            if (! IsEnabled)
            {
                IsEnabled = true;
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

        internal void WriteAndEndTurn(Logger.LogCallback log)
        {
            if (IsEnabled)
            {
                if (MethodNames.Length == 0)
                {
                    log("To use the profiler, you must set Names to a list of enum names that you used with the integer keys");
                }
                else
                {
                    log("Move Stats");
                    for (int ix = 0; ix < MethodNames.Length; ix++)
                    {
                        log(string.Format("{0} : {1:N0}", MethodNames[ix], Profile[ix]));
                    }

                    for (int ix = 0; ix < FxMethodNames.Length; ix++)
                    {
                        log(string.Format("{0} : {1:N0}", FxMethodNames[ix], FxProfile[ix]));
                    }
                }

                Turns.Add(Profile);
                FxTurns.Add(FxProfile);
            }
        }
    }
}
