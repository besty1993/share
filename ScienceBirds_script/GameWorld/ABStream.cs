// YANG
using System.Diagnostics;
using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.MctsAi;

public class ABStream
{
// For General
    public static float delay = 0f;
    public static bool finish = false;
    public static string actions = "";
    public static string levels = "";
    public static int currentLevel = 7;
    public static bool delete_every_data = false;


// For Streaming
    public static bool streaming = false;
    public static int actionNum = 0;
    public static Vector2[][] loadedActions;
    public static string[] loadedLevels;
    public static bool reloadLevel = true;
    public static int r = -1;

// For Optical Flow Simulation

    public static bool optical_flow = false;
    public static int frameNum = 0;
    public static bool startPWC = true;
    public static bool startTGIF = false;
    public static double tgif = 0f;
    public static double tgif2 = 0f;
    public static bool saveLevelPath = true;

    // public static float total_displacement = 0f;

    public static string levelFileName = "";
    public static string actionFileName = "";
    public static string scoreFileName = "";
    public static string streamingResult = "";
    public static string additionalFileName = "";
    public static string tgifFileName = "";

    // Ver2.0
    public static bool ver2 = true;
    public static bool simulate = true;
    public static string tempActionFileName = "tempAction.txt";
    public static string tempStateFileName = "tempState.txt";
    public static string loadFile = "";

    // Turn on the PWCNet
    public static void TurnOnPWC()
    {
        Process _cmd = null;

        ProcessStartInfo info = new ProcessStartInfo("C:\\Project\\COG\\ConsolePWC\\ConsolePWC\\bin\\Debug\\ConsolePWC.exe");
        info.RedirectStandardInput = false;
        info.RedirectStandardOutput = false;
        info.CreateNoWindow = false;
        info.Arguments = String.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\"", "pwcnet", "C:\\Project\\COG\\tfoptflow\\tfoptflow\\batchedpredict.py", "C:\\Project\\COG\\tfoptflow\\tfoptflow\\models\\pwcnet-lg-6-2-multisteps-chairsthingsmix\\pwcnet.ckpt-595000", "0", "10", "C:\\Project\\COG\\ConsolePWC\\ConsolePWC\\bin\\Debug\\_temp_source.txt");
        _cmd = Process.Start(info);
    }

    // Delete Screenshots and flo files after one rollout is done.
    public static void DeleteScreenshots(bool txt)
    {
        string[] flos = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/../../", "*.flo");
        string[] pngs = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/../../", "*.png");
        string[] txts = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/../../", "*.txt");

        foreach (string filepath in flos)
        {
            System.IO.File.Delete(filepath);
        }
        foreach (string filepath in pngs)
        {
            System.IO.File.Delete(filepath);
        }
        if (txt)
            foreach (string filepath in txts)
            {
                System.IO.File.Delete(filepath);
            }
        frameNum = 0;
    }

    // Read saved actions for streaming
    public static Vector2[][] ReadActions(string filepath)
    {
        System.IO.StreamReader file = new System.IO.StreamReader(filepath);
        string rawfile = file.ReadToEnd();
        file.Close();

        var lvls = rawfile.Split('\n');
        Vector2[][] actions = new Vector2[lvls.Length-1][];
        UnityEngine.Debug.Log(rawfile);
        for (int i = 0; i < actions.Length; i++)
        {
            var a1 = lvls[i].Split(new string[] { ") " }, StringSplitOptions.None);
            var a2 = a1[1].Split(new string[] { ", " }, StringSplitOptions.None);
            actions[i] = new Vector2[a2.Length - 1];
            for (int j = 0; j < a2.Length - 1; j++)
            {
                // actions[i][j] = new Vector2(0f,0f);
                var a3 = a2[j].Split(' ');
                Vector2 a4 = new Vector2 (0f,0f);
                a4.x = (float)Convert.ToDouble(a3[0]);
                a4.y = (float)Convert.ToDouble(a3[1]);
                actions[i][j] = new Vector2(a4.x,a4.y);
                // UnityEngine.Debug.Log(a4.ToString());
            }
        }
        return actions;
    }

    // Read saved levels for streaming
    public static string[] ReadLevels(string filepath)
    {
        System.IO.StreamReader file = new System.IO.StreamReader(filepath);
        string rawfile = file.ReadToEnd();
        file.Close();

        return (rawfile.Split('\n'));
    }

    public static void SetFileName () {
        if (ABStream.optical_flow) additionalFileName = "HalfBirdsLeft";
        if (optical_flow) {
            actionFileName = "actions-OF"+"-sim"+AIData.MctsSimulationLimit;
            levelFileName = "levels-OF"+"-sim"+AIData.MctsSimulationLimit;
            scoreFileName = "scores-OF"+"-sim"+AIData.MctsSimulationLimit;
            tgifFileName = "tgifSave-OF"+"-sim"+AIData.MctsSimulationLimit;
            if (streaming) {
            streamingResult = "streaming-OF"+"-sim"+AIData.MctsSimulationLimit;
            }
        }
        else {
            actionFileName = "actions-MCTS"+"-sim"+AIData.MctsSimulationLimit;
            levelFileName = "levels-MCTS"+"-sim"+AIData.MctsSimulationLimit;
            scoreFileName = "scores"+"-sim"+AIData.MctsSimulationLimit;
            if (streaming) {
            streamingResult = "streaming-MCTS"+"-sim"+AIData.MctsSimulationLimit;
            }
        }
        // YANG
        // Set filename forTo save data: actions, levels, score differences
        
        actionFileName = actionFileName+additionalFileName+".txt";
        levelFileName = levelFileName + additionalFileName+".txt";
        scoreFileName = scoreFileName + additionalFileName+".txt";
        tgifFileName = tgifFileName + additionalFileName+".txt";
        // Finished
    }
}
