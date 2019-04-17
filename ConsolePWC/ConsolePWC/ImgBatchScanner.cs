using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace ConsolePWC
{
    public static class ImgBatchScanner
    {
        public static bool IsRunning { get; private set; }

        private static Thread _thread = null;
        private static int _sleepinterval = 1;
        private static string _sourcefile;

        public static void Start(string sourcefile, string environmentName, string scriptPath, string ckptPath, int deviceID, int sleepInterval)
        {
            IsRunning = true;
            PWCNetUtils.Start(environmentName, scriptPath, deviceID, ckptPath, null, sleepInterval);
            _sourcefile = sourcefile;
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
            DateTime lastmod = new DateTime();
            while (IsRunning)
            {
                while (PWCNetUtils.State != PWCNetUtils.CmdState.Inputting)
                    Thread.Sleep(_sleepinterval);
                
                Thread.Sleep(_sleepinterval);

                if (!File.Exists(_sourcefile))
                    continue;
                
                DateTime tmod = File.GetLastWriteTime(_sourcefile);
                if (PWCNetUtils.State == PWCNetUtils.CmdState.Inputting && tmod != lastmod)
                {
                    lastmod = tmod;
                    string tdir = File.ReadAllText(_sourcefile);

                    if (!Directory.Exists(tdir))
                        continue;

                    Console.WriteLine("======= WILL PROCESS DIRECTORY:" + tdir);
                    
                    PWCNetUtils.InputPath = String.Format(tdir);
                    Thread.Sleep(_sleepinterval);
                }
            }
        }
    }
}
