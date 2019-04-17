using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace ConsolePWC
{
    class PWCNetUtils
    {
        public enum CmdState
        {
            None,
            Initialized,
            Inputting,
            Processing,
            Quit
        }

        public static string InputPath { get; set; }
        public static string Log { get; private set; }
        public static CmdState State
        {
            get { return _state; }
        }

        private const string _CMDPATH = "C:\\Windows\\system32\\cmd.exe";

        private static Process _cmd = null;
        private static Thread _thread = null;
        private static CmdState _state = CmdState.None;
        private static string _envname = "";
        private static int _deviceid = -1;
        private static string _scriptpath = "";
        private static string _ckptpath = "";
        private static string _outpath = "";
        private static int _sleepinterval = 1;

        public static void Start(string envName, string scriptPath, int deviceID, string ckptPath, string outputPath, int sleepInterval)
        {
            try
            {
                if (_state != CmdState.None)
                    Stop();

                Log = "";
                _state = CmdState.None;
                _envname = envName;
                _scriptpath = scriptPath;
                _deviceid = deviceID;
                _ckptpath = ckptPath;
                _outpath = outputPath;
                _sleepinterval = sleepInterval;
                _thread = new Thread(new ThreadStart(ofThread));
                _thread.IsBackground = true;
                _thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Stop()
        {
            if (_thread != null)
                _thread.Abort();

            if (_cmd != null)
            {
                _cmd.Close();
                _cmd = null;
            }

            _state = CmdState.None;
        }

        private static void ofThread()
        {
            _state = CmdState.None;

            string initcommand = generateInitCommand(_scriptpath, _deviceid, _ckptpath, _outpath);
            ProcessStartInfo info = new ProcessStartInfo(_CMDPATH);
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = false;
            info.UseShellExecute = false;

            _cmd = Process.Start(info);
            _cmd.OutputDataReceived += outputHandler;
            _cmd.ErrorDataReceived += outputHandler;

            //..initialization
            Console.WriteLine("activating: " + _envname);
            _cmd.StandardInput.Write(" activate "+_envname+"\n");
            _cmd.StandardInput.Flush();
            _cmd.BeginOutputReadLine();
            Console.WriteLine(_envname + " activated");

            _cmd.StandardInput.WriteLine(initcommand);
            _cmd.StandardInput.Flush();
            while (!(_state == CmdState.Initialized || _state == CmdState.Inputting))
                Thread.Sleep(_sleepinterval);
            Console.WriteLine("initialize completed");
            //..initialization

            //while (_state != CmdState.Quit)
            //{
            //    while (_state != CmdState.Inputting)
            //    {
            //        Thread.Sleep(_sleepinterval);
            //        Console.WriteLine(_state);
            //    }

            //    Thread.Sleep(_sleepinterval);
            //    while (string.IsNullOrEmpty(InputPath))
            //        Thread.Sleep(_sleepinterval);

            //    _cmd.StandardInput.WriteLine(InputPath);
            //    _cmd.StandardInput.Flush();
            //    InputPath = "";
            //    Thread.Sleep(_sleepinterval);
            //    while (_state == CmdState.Processing)
            //        Thread.Sleep(_sleepinterval);
            //}

            _cmd.WaitForExit();
        }

        static void outputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (_cmd == null)
                return;

            Console.WriteLine(String.Format("output handler {0} : {1}", _state, outLine.Data));
            Log += "\n" + outLine.Data;

            switch (outLine.Data)
            {
                case "_INITIALIZED":
                    _state = CmdState.Initialized;
                    break;
                case "_INPUTTING":
                    _state = CmdState.Inputting;
                    break;
                case "_PROCESSING":
                    _state = CmdState.Processing;
                    break;
                case "_QUIT":
                    _state = CmdState.Quit;
                    break;
                default:
                    break;
            }
        }

        private static string generateInitCommand(string scriptPath, int deviceID, string ckptPath, string outputPath)
        {
            return string.IsNullOrEmpty(outputPath) ? String.Format("python \"{0}\" -c \"{1}\" -d {2}", _scriptpath, _ckptpath, _deviceid) :
                String.Format("python \"{0}\" -c \"{1}\" -o \"{2}\" -d {3}", _scriptpath, _ckptpath, _outpath, _deviceid);
            //return string.IsNullOrEmpty(outputPath) ? String.Format("python {0}", _scriptpath) :
            //    String.Format("python \"{0}\" -c \"{1}\" -o \"{2}\" -d {3}", _scriptpath, _ckptpath, _outpath, _deviceid);
        }
    }
}
