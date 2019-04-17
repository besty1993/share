using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace ConsolePWC
{
    public static class ImgCoupleScanner
    {
        public static bool IsRunning { get; private set; }

        private static Thread _thread = null;
        private static int _sleepinterval = 1;
        private static string _image1, _image2;

        public static void Start(string image1, string image2, string output, string environmentName, string scriptPath, string ckptPath, int deviceID, int sleepInterval)
        {
            IsRunning = true;
            PWCNetUtils.Start(environmentName, scriptPath, deviceID, ckptPath, output, sleepInterval);
            _image1 = image1;
            _image2 = image2;
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
            DateTime lastmod1 = new DateTime();
            DateTime lastmod2 = new DateTime();
            while (IsRunning)
            {
                while (PWCNetUtils.State != PWCNetUtils.CmdState.Inputting)
                    Thread.Sleep(_sleepinterval);

                //Console.WriteLine(String.Format("scanning {0} and {1} . . .", _image1, _image2));
                Thread.Sleep(_sleepinterval);
                
                if (!File.Exists(_image1))
                {
                    //Console.WriteLine(String.Format("{0} is not existed", _image1));
                    continue;
                }

                if (!File.Exists(_image2))
                {
                    //Console.WriteLine(String.Format("{0} is not existed", _image2));
                    continue;
                }

                //Console.WriteLine("FILE FOUND");

                DateTime tmod1 = File.GetLastWriteTime(_image1);
                DateTime tmod2 = File.GetLastWriteTime(_image2);
                if (PWCNetUtils.State == PWCNetUtils.CmdState.Inputting && (tmod1 != lastmod1 || tmod2 != lastmod2))
                {
                    lastmod1 = tmod1;
                    lastmod2 = tmod2;
                    Console.WriteLine("======= WILL PROCESS:" + String.Format("{0}||{1}", _image1, _image2));
                    
                    PWCNetUtils.InputPath = String.Format("{0}||{1}", _image1, _image2);
                    Thread.Sleep(_sleepinterval);
                }
            }
        }
    }
}
