using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UvsChess
{
    public class UserPrefs
    {
        private static string TIME = "time";
        private static string GRACEPERIOD = "graceperiod";


        static string _filename;
        static Dictionary<string, string> items = null;
        private static string inifile = AppDomain.CurrentDomain.BaseDirectory + "UvsChess.ini";
        
        public static Dictionary<string,string> Settings
        {
            get { return items; }
            set { items = value; }
        }

        public static string FileName
        {
            get { return _filename; }
            set { _filename = value; }
        }
        
        public static int Time
        {
            get { return Convert.ToInt32(items[TIME]); }
            set { items[TIME] = value.ToString(); }
        }
        public static int GracePeriod
        {
            get { return Convert.ToInt32(items[GRACEPERIOD]); }
            set { items[GRACEPERIOD] = value.ToString(); }
        }

        public static void Load()
        {
            Load(inifile);
        }
        public static void Load(string filename)
        {
            FileName = filename;
            items = new Dictionary<string, string>();
            StreamReader infile = new StreamReader(FileName);

            string line = infile.ReadLine();
            while (line != null)
            {
                if (line == string.Empty)
                {
                    line = infile.ReadLine();
                    continue;
                }
                string[] sections = line.Split('=');
                items[sections[0]] = sections[1];
                line = infile.ReadLine();
            }
            infile.Close();
        } 
        public static void Save()
        {
            StreamWriter outfile = new StreamWriter(FileName);            
            foreach (string key in items.Keys)
            {
                outfile.WriteLine("{0}={1}", key, items[key]);
            }
            outfile.Close();
        }

        public static void Save(Dictionary<string, string> pairs)
        {
            items = pairs;
            Save();
        }
    }
}
