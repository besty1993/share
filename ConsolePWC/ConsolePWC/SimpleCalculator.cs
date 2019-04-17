using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace ConsolePWC
{
    public static class SimpleCalculator
    {
        public static bool IsRunning { get; private set; }

        private static Thread _thread = null;
        private static string _sourcedir = "";
        private static string _resultpath = "";
        private static int _sleepinterval = 1;

        public static void Start(string sourceDirectory, int sleepInterval)
        {
            IsRunning = true;
            _sourcedir = sourceDirectory;
            _resultpath = Path.Combine(_sourcedir, "result.txt");
            _sleepinterval = Math.Max(1, sleepInterval);
            _thread = new Thread(new ThreadStart(scanFile));
            _thread.Start();
        }

        public static void Stop()
        {
            if (_thread != null)
                _thread.Abort();
        }

        private static void scanFile()
        {
            while (IsRunning)
            {
                Console.WriteLine(String.Format("scanning {0} . . .", _sourcedir));
                Thread.Sleep(_sleepinterval);

                string filepath = getFirstFile();
                if (string.IsNullOrEmpty(filepath))
                {
                    Console.WriteLine("NO FILE");
                    continue;
                }

                Console.WriteLine("FILE FOUND");

                string[] sinputs = File.ReadAllLines(filepath);
                if (sinputs != null)
                {
                    if (isQuit(sinputs))
                    {
                        IsRunning = false;
                        break;
                    }

                    if (sinputs.Length > 1)
                    {
                        try
                        {
                            int a = Convert.ToInt32(sinputs[0]);
                            int b = Convert.ToInt32(sinputs[1]);
                            int c = a + b;

                            File.WriteAllText(filepath, "");
                            File.WriteAllText(_resultpath, "" + c);
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(err.ToString());
                        }
                    }
                }
            }
        }

        private static string getFirstFile()
        {
            return getPath(_sourcedir, "[1]");
        }

        private static string getSecondFile()
        {
            return getPath(_sourcedir, "[2]");
        }

        private static string getPath(string path, string namingCondition)
        {
            string[] val = Directory.GetFiles(path);
            if (val != null && val.Length > 0)
            {
                for (int i = 0; i < val.Length; i++)
                {
                    if (val[i].Contains(namingCondition))
                        return val[i];
                }
            }

            return "";
        }

        private static bool isQuit(string[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == "quit")
                    return true;
            }
            return false;
        }
    }
}