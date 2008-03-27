using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UvsChess.Framework
{
    class DllLoader
    {
        private static List<AI> _availableais = new List<AI>();
        public static List<AI> AvailableAIs
        {
            get { return _availableais; }
        }

        #region Loading DLLs
        public static void SearhForAIs()
        {
            _availableais = new List<AI>();
            _availableais.Add(new AI("Human"));
            //AvailableAIs.Add("Human");
            string appFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string[] dlls = Directory.GetFiles(appFolder, "*.dll");

            foreach (string dll in dlls)
            {
                LoadAIsFromFile(dll);
            }
        }

        static void LoadAIsFromFile(string filename)
        {
            try
            {
                System.Reflection.Assembly assem = System.Reflection.Assembly.LoadFile(filename);
                System.Type[] types = assem.GetTypes();

                foreach (System.Type type in types)
                {
                    System.Type[] interfaces = type.GetInterfaces();

                    foreach (System.Type inter in interfaces)
                    {
                        if (inter == typeof(UvsChess.IChessAI))
                        {
                            IChessAI ai = (IChessAI)assem.CreateInstance(type.FullName);
                            AI tmp = new AI(ai.Name);
                            tmp.FileName = filename;
                            tmp.FullName = type.FullName;
                            _availableais.Add(tmp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Chess->MainForm->LoadAI: " + ex.Message);
            }
        }

        #endregion

    }
}
