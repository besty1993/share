using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.MctsAi
{
    class AIData
    {
        public static bool pureMcts = false;

        public static Node root;

        public static bool rebuild = true;

        public static int XmlIndex = 0;
        public static List<string> XmlTexts = new List<string>();

        public static bool InSimulation = false;
        public static int BirdsCountGameWorld = -1;
        public static int BirdsCountSimulation = -1;

        public static int WaitCount = 0;
        public static bool NeedToWriteGamestate = true;
        public static bool NeedToReadAction = false;
        public static bool have_p = false;
        public static List<double> ps = new List<double>();

        public static bool WaitToPlay = false;
        public static bool WaitToPlayEnd = false;
        public static bool IsShootingInPlay = false;
        public static float ShootingTimerInPlay = 0f;

        public static int MctsSimulationLimit = 10;
        public static int SimulationTimeLimit = 600;

        public static bool AtStart = true;

        public static bool NeedToMakeRoot = true;

        public static DateTime SimulationStartTime = System.DateTime.Now;
        public static DateTime SimulationEndTime = SimulationStartTime.AddSeconds(1000);
        public static int SimulationCount = 0;

        public static bool IsShootingInSimulation = false;
        public static bool IsWaitingScoreInSimulation = false;
        public static List<Node> NodesToBeSimulated = new List<Node>();
        public static float ShootingTimerInSimulation = 0f;

        public static int AllBirdsCount = -1;
        public static int AllPigsCount = -1;
        public static int AllBlocksCount = -1;
        //public static double 

        public static bool ReloadBecauseOfVeryLag = false;
        public static string ReloadXmlFile = "";

        public static bool StopSimulation = false;

        //For Save Game Play Sample
        public static List<string> play_states = new List<string>();
        public static List<string> play_actions = new List<string>();
        public static List<double> play_percentage = new List<double>();
        public static bool play_end = false;
        public static bool write_end = false;

        public static void Reset()
        {
            root = null;
            rebuild = true;
            InSimulation = false;
            BirdsCountGameWorld = -1;
            BirdsCountSimulation = -1;
            WaitToPlay = false;
            WaitToPlayEnd = false;
            IsShootingInPlay = false;
            ShootingTimerInPlay = 0f;

            AtStart = true;

            NeedToMakeRoot = true;

            SimulationStartTime = System.DateTime.Now;
            SimulationEndTime = SimulationStartTime.AddSeconds(1000);

            IsShootingInSimulation = false;
            IsWaitingScoreInSimulation = false;
            NodesToBeSimulated.Clear();
            ShootingTimerInSimulation = 0f;

            AllPigsCount = -1;
            AllBlocksCount = -1;

            ReloadBecauseOfVeryLag = false;
            ReloadXmlFile = "";

            StopSimulation = false;
            Debug.Log("Reset");
        }
    }
}
