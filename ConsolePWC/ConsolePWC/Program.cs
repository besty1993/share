using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace ConsolePWC
{
    class Program
    {
        private const string _DEFFILE_BATCHED = "_temp_source.txt";
        private const string _DEFFILE_COUPLED1 = "_image_batch\\image1.png";
        private const string _DEFFILE_COUPLED2 = "_image_batch\\image2.png";
        private const string _DEFOUTPUT_COUPLED = "_image_batch\\output_flow.png";
        private const string _DEFENV = "pwcnet";
        private const string _DEFSCRIPT_COUPLED = "C:\\Users\\ebin\\Projects\\python-projects\\tfoptflow\\tfoptflow\\coupledpredict.py";
        private const string _DEFSCRIPT_BATCHED = "C:\\Project\\COG\\tfoptflow\\tfoptflow\\batchedpredict.py";
        private const string _DEFCKPT = "C:\\Project\\COG\\tfoptflow\\tfoptflow\\models\\pwcnet-lg-6-2-multisteps-chairsthingsmix\\pwcnet.ckpt-595000";
        private const int _DEFDEVICE = -1; //..-1 for CPU
        private const int _DEFSLEEP = 10;

        private static bool _isrunning = false;

        static void Main(string[] args)
        {
            bool isbatchpredict = args != null && args.Length >= 2 && Path.GetFileName(args[1]).Contains("batched");
            isbatchpredict = true;

            if (isbatchpredict)
                runBatchedPredict(args);
            else
                runCoupledPredict(args);
        }

        #region batched_predict
        private static void runBatchedPredict(string[] args)
        {
            _isrunning = true;

            string envname = _DEFENV;
            string scriptpath = _DEFSCRIPT_BATCHED;
            string ckptpath = _DEFCKPT;
            int deviceid = _DEFDEVICE;
            int sleepinterval = _DEFSLEEP;
            string sourcefile = Path.Combine(Directory.GetCurrentDirectory(), _DEFFILE_BATCHED);

            //..check source directory
            if (args == null || args.Length != 6) //..create source directory if there is no argument
            {
                Console.WriteLine("arguments doesn't match. the arguments should be [env_name] [script_path] [ckpt_path] [device_id] [sleep_interval] [source_file]");
                Console.WriteLine("will load the default settings instead . . .");
            }
            else
            {
                envname = args[0];
                scriptpath = Path.GetFullPath(args[1]);
                ckptpath = Path.GetFullPath(args[2]);
                try
                {
                    deviceid = Convert.ToInt32(args[3]);
                }
                catch
                {
                    Console.WriteLine("argument [device_id] is not an integer. will run using CPU (-1) instead.");
                    deviceid = _DEFDEVICE;
                }
                try
                {
                    sleepinterval = Convert.ToInt32(args[4]);
                }
                catch
                {
                    Console.WriteLine("argument [sleep_interval] is not an integer.");
                    sleepinterval = _DEFSLEEP;
                }
                sourcefile = Path.GetFullPath(args[5]);
            }
            //..check source directory

            _isrunning = initializeBacthed(sourcefile, scriptpath, ckptpath);

            if (!_isrunning)
                return;

            ImgBatchScanner.Start(sourcefile, envname, scriptpath, ckptpath, deviceid, sleepinterval);
            while (_isrunning)
            {
                Thread.Sleep(sleepinterval);
                _isrunning = ImgCoupleScanner.IsRunning;
            }
            ImgCoupleScanner.Stop();
            Console.WriteLine("press any key to exit . . .");
            Console.ReadKey();
        }

        private static bool initializeBacthed(string sourceFile, string script, string ckpt)
        {
            //..check and create directories
            string tdir = Path.GetDirectoryName(sourceFile);
            if (!Directory.Exists(tdir))
                Directory.CreateDirectory(tdir);
            //..check and create directories

            return !string.IsNullOrEmpty(script) && !string.IsNullOrEmpty(ckpt) && File.Exists(script) && Directory.Exists(Path.GetDirectoryName(ckpt));
        }
        #endregion

        #region coupled_predict
        private static void runCoupledPredict(string[] args)
        {
            _isrunning = true;

            string envname = _DEFENV;
            string scriptpath = _DEFSCRIPT_COUPLED;
            string ckptpath = _DEFCKPT;
            int deviceid = _DEFDEVICE;
            int sleepinterval = _DEFSLEEP;
            string image1 = Path.Combine(Directory.GetCurrentDirectory(), _DEFFILE_COUPLED1);
            string image2 = Path.Combine(Directory.GetCurrentDirectory(), _DEFFILE_COUPLED2);
            string output = Path.Combine(Directory.GetCurrentDirectory(), _DEFOUTPUT_COUPLED);

            //..check source directory
            if (args == null || args.Length != 8) //..create source directory if there is no argument
            {
                Console.WriteLine("arguments doesn't match. the arguments should be [env_name] [script_path] [ckpt_path] [device_id] [sleep_interval] [image1_path] [image2_path] [output_path]");
                Console.WriteLine("will load the default settings instead . . .");
            }
            else
            {
                envname = args[0];
                scriptpath = Path.GetFullPath(args[1]);
                ckptpath = Path.GetFullPath(args[2]);
                try
                {
                    deviceid = Convert.ToInt32(args[3]);
                }
                catch
                {
                    Console.WriteLine("argument [device_id] is not an integer. will run using CPU (-1) instead.");
                    deviceid = _DEFDEVICE;
                }
                try
                {
                    sleepinterval = Convert.ToInt32(args[4]);
                }
                catch
                {
                    Console.WriteLine("argument [sleep_interval] is not an integer.");
                    sleepinterval = _DEFSLEEP;
                }
                image1 = Path.GetFullPath(args[5]);
                image2 = Path.GetFullPath(args[6]);
                output = Path.GetFullPath(args[7]);
            }
            //..check source directory

            _isrunning = initializeCoupled(image1, image2, output, scriptpath, ckptpath);

            if (!_isrunning)
                return;

            ImgCoupleScanner.Start(image1, image2, output, envname, scriptpath, ckptpath, deviceid, sleepinterval);
            while (_isrunning)
            {
                Thread.Sleep(sleepinterval);
                _isrunning = ImgCoupleScanner.IsRunning;
            }
            ImgCoupleScanner.Stop();
            Console.WriteLine("press any key to exit . . .");
            Console.ReadKey();
        }

        private static bool initializeCoupled(string image1, string image2, string output, string script, string ckpt)
        {
            //..check and create directories
            string[] dirs = new string[] { Path.GetDirectoryName(image1), Path.GetDirectoryName(image2) , Path.GetDirectoryName(output) };
            for (int i = 0; i < dirs.Length; i++)
            {
                if (!Directory.Exists(dirs[i]))
                    Directory.CreateDirectory(dirs[i]);
            }
            //..check and create directories

            return !string.IsNullOrEmpty(script) && !string.IsNullOrEmpty(ckpt) && File.Exists(script) && Directory.Exists(Path.GetDirectoryName(ckpt));
        }
        #endregion
    }
}