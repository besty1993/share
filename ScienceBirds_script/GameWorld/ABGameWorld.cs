// SCIENCE BIRDS: A clone version of the Angry Birds game used for 
// research purposes
// 
// Copyright (C) 2016 - Lucas N. Ferreira - lucasnfe@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
//

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using Assets.Scripts.MctsAi;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;
using System.Linq;

public class ABGameWorld : ABSingleton<ABGameWorld>
{

    public static int _levelTimesTried;

    private bool _levelCleared;

    public static List<ABPig> _pigs;
    public static List<ABBird> _birds;
    public static List<ABBlock> _blocks;
    private List<ABParticle> _birdTrajectory;

    private ABBird _lastThrownBird;
    private Transform _blocksTransform;
    private Transform _birdsTransform;
    private Transform _plaftformsTransform;
    private Transform _slingshotBaseTransform;

    private DataGettorX _dataGettor;

    private GameObject _slingshot;
    public GameObject Slingshot() { return _slingshot; }

    private GameObject _levelFailedBanner;
    public bool LevelFailed() { return _levelFailedBanner.activeSelf; }

    private GameObject _levelClearedBanner;
    public bool LevelCleared() { return _levelClearedBanner.activeSelf; }

    private int _pigsAtStart;
    public int PigsAtStart { get { return _pigsAtStart; } }

    private int _birdsAtStart;
    public int BirdsAtStart { get { return _birdsAtStart; } }

    private int birdscount;

    private int _blocksAtStart;
    public int BlocksAtStart { get { return _blocksAtStart; } }

    public ABGameplayCamera GameplayCam { get; set; }
    public float LevelWidth { get; set; }
    public float LevelHeight { get; set; }

    // Game world properties
    public bool _isSimulation;
    public int _timesToGiveUp;
    public float _timeToResetLevel = 1f;
    public int _birdsAmounInARow = 5;

    public AudioClip[] _clips;

    //Code For Get Game Data Tool(DataGettorX)
    public static int BirdID = 0;
    public static string GameState;
    //Code For Get Game Data Tool(DataGettorX)

    // YANG
    


    void Awake()
    {

        _blocksTransform = GameObject.Find("Blocks").transform;
        _birdsTransform = GameObject.Find("Birds").transform;
        _plaftformsTransform = GameObject.Find("Platforms").transform;

        _levelFailedBanner = GameObject.Find("LevelFailedBanner").gameObject;
        _levelFailedBanner.gameObject.SetActive(false);

        _levelClearedBanner = GameObject.Find("LevelClearedBanner").gameObject;
        _levelClearedBanner.gameObject.SetActive(false);

        GameplayCam = GameObject.Find("Camera").GetComponent<ABGameplayCamera>();

        _dataGettor = gameObject.AddComponent<DataGettorX>();

        Application.targetFrameRate = 6000;

        
    }

    // Use this for initialization
    void Start()
    {
        //Code For Get Game Data Tool(DataGettorX)
        BirdID = 0;
        GameState = GameStateX.PLAYING;
        //Code For Get Game Data Tool(DataGettorX)

        _pigs = new List<ABPig>();
        _birds = new List<ABBird>();
        _blocks = new List<ABBlock>();
        _birdTrajectory = new List<ABParticle>();

        _levelCleared = false;

        if (!_isSimulation)
        {

            GetComponent<AudioSource>().PlayOneShot(_clips[0]);
            GetComponent<AudioSource>().PlayOneShot(_clips[1]);
        }

        // If there are objects in the scene, use them to play
        if (_blocksTransform.childCount > 0 || _birdsTransform.childCount > 0)
        {

            foreach (Transform bird in _birdsTransform)
                AddBird(bird.GetComponent<ABBird>());

            foreach (Transform block in _blocksTransform)
            {

                ABPig pig = block.GetComponent<ABPig>();
                if (pig != null)
                    _pigs.Add(pig);
            }

        }
        else
        {

            ABLevel currentLevel = LevelList.Instance.GetCurrentLevel();
            if (ABStream.ver2&&ABStream.simulate) {
                currentLevel = LevelLoader.LoadXmlLevel(File.ReadAllText(ABStream.tempStateFileName));
            }

            if (currentLevel != null)
            {
                // if (!ABStream.streaming)
                DecodeLevel(currentLevel);
                
                AdaptCameraWidthToLevel();

                _levelTimesTried = 0;

                _slingshotBaseTransform = GameObject.Find("slingshot_base").transform;
            }
        }
        // YANG
        // Only for streaming, reload levels from xml file
        if (ABStream.streaming)
            BuildLevelFromXmlFile(ABStream.loadedLevels[ABStream.currentLevel-4]);
        // Finished
    }

    public void DecodeLevel(ABLevel currentLevel)
    {

        ClearWorld();

        LevelHeight = ABConstants.LEVEL_ORIGINAL_SIZE.y;
        LevelWidth = (float)currentLevel.width * ABConstants.LEVEL_ORIGINAL_SIZE.x;

        Vector3 cameraPos = GameplayCam.transform.position;
        cameraPos.x = currentLevel.camera.x;
        cameraPos.y = currentLevel.camera.y;
        GameplayCam.transform.position = cameraPos;

        GameplayCam._minWidth = currentLevel.camera.minWidth;
        GameplayCam._maxWidth = currentLevel.camera.maxWidth;

        Vector3 landscapePos = ABWorldAssets.LANDSCAPE.transform.position;
        Vector3 backgroundPos = ABWorldAssets.BACKGROUND.transform.position;

        if (currentLevel.width > 1)
        {

            landscapePos.x -= LevelWidth / 4f;
            backgroundPos.x -= LevelWidth / 4f;
        }

        for (int i = 0; i < currentLevel.width; i++)
        {

            GameObject landscape = (GameObject)Instantiate(ABWorldAssets.LANDSCAPE, landscapePos, Quaternion.identity);
            landscape.transform.parent = transform;

            foreach (Transform child in landscape.transform)
            {
                if (child.name == "Ground")
                {
                    child.gameObject.GetComponent<BoxCollider2D>().sharedMaterial = ABWorldAssets.GROUND_MATERIAL;
                    //PhysicsMaterial2D materialGround = Resources.Load("Materials/Ground") as PhysicsMaterial2D;
                }
            }

            float screenRate = currentLevel.camera.maxWidth / LevelHeight;
            if (screenRate > 2f)
            {

                for (int j = 0; j < (int)screenRate; j++)
                {

                    Vector3 deltaPos = Vector3.down * (LevelHeight / 1.5f + (j * 2f));
                    Instantiate(ABWorldAssets.GROUND_EXTENSION, landscapePos + deltaPos, Quaternion.identity);
                }
            }

            landscapePos.x += ABConstants.LEVEL_ORIGINAL_SIZE.x - 0.01f;

            GameObject background = (GameObject)Instantiate(ABWorldAssets.BACKGROUND, backgroundPos, Quaternion.identity);
            background.transform.parent = GameplayCam.transform;
            backgroundPos.x += ABConstants.LEVEL_ORIGINAL_SIZE.x - 0.01f;
        }

        Vector2 slingshotPos = new Vector2(currentLevel.slingshot.x, currentLevel.slingshot.y);
        _slingshot = (GameObject)Instantiate(ABWorldAssets.SLINGSHOT, slingshotPos, Quaternion.identity);
        _slingshot.name = "Slingshot";
        _slingshot.transform.parent = transform;

        foreach (BirdData gameObj in currentLevel.birds)
        {

            AddBird(ABWorldAssets.BIRDS[gameObj.type], ABWorldAssets.BIRDS[gameObj.type].transform.rotation);
        }

        foreach (OBjData gameObj in currentLevel.pigs)
        {

            Vector2 pos = new Vector2(gameObj.x, gameObj.y);
            Quaternion rotation = Quaternion.Euler(0, 0, gameObj.rotation);
            AddPig(ABWorldAssets.PIGS[gameObj.type], pos, rotation);
        }

        foreach (BlockData gameObj in currentLevel.blocks)
        {

            Vector2 pos = new Vector2(gameObj.x, gameObj.y);
            Quaternion rotation = Quaternion.Euler(0, 0, gameObj.rotation);

            GameObject block = AddBlock(ABWorldAssets.BLOCKS[gameObj.type], pos, rotation);

            MATERIALS material = (MATERIALS)System.Enum.Parse(typeof(MATERIALS), gameObj.material);
            block.GetComponent<ABBlock>().SetMaterial(material);
        }

        foreach (PlatData gameObj in currentLevel.platforms)
        {

            Vector2 pos = new Vector2(gameObj.x, gameObj.y);
            Quaternion rotation = Quaternion.Euler(0, 0, gameObj.rotation);

            AddPlatform(ABWorldAssets.PLATFORM, pos, rotation, gameObj.scaleX, gameObj.scaleY);
        }

        foreach (OBjData gameObj in currentLevel.tnts)
        {

            Vector2 pos = new Vector2(gameObj.x, gameObj.y);
            Quaternion rotation = Quaternion.Euler(0, 0, gameObj.rotation);

            AddBlock(ABWorldAssets.TNT, pos, rotation);
        }

        StartWorld();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if birds was trown, if it died and swap them when needed
        ManageBirds();
        // AIControl();

        // YANG
        if (ABStream.ver2&&!ABStream.simulate&&!ABStream.streaming) {
            Ver2RealPlay();
            return;
        }
        if (ABStream.streaming)
        {
            StreamRealPlay();
        } else {
            AIControl();
        }
    }

    private void ReadActions()
    {
        //for (int i = 0; i < 69; i++)
        //{
        //    double p = 1.00;
        //    AIData.ps.Add(p);
        //}

        AIData.NeedToReadAction = false;
        AIData.NeedToMakeRoot = true;
        return;
        // string action_filename = "D:/AbDatas/Worker4/ActionToGet/ActionToGet.txt";
        string action_filename = Application.dataPath + "/../.." + "/AbDatas/Worker4/ActionToGet/ActionToGet.txt";

        if (File.Exists(action_filename))
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(action_filename))
            {
                string action_str = file.ReadToEnd();
                file.Close();

                System.IO.File.Delete(@action_filename);

                var action_strs = action_str.Split(' ');
                AIData.ps.Clear();

                for (int i = 0; i < action_strs.Length; i++)
                {
                    double p = 0.0;
                    try
                    {
                        p = Convert.ToDouble(action_strs[i]);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        AIData.ps.Add(p);
                    }
                }

            }

            AIData.have_p = true;

            AIData.NeedToReadAction = false;
            AIData.NeedToMakeRoot = true;
        }
    }

    void AIControl()
    {
        if (AIData.play_end)
        {
            // Original Code
            // if (!AIData.write_end) WriteToResult();
            Application.Quit();
        }

        if (!IsLevelStable()) { // AI shall not do anything when stage is not stable
            // Fake Optical Flow
            // if (!ABStream.optical_flow) return;
            // else {
            //     if (AIData.InSimulation) {
            //         float of=GetOF();
            //         ABStream.total_displacement += of;
            //     }
            //     return;
            // }
            
            // YANG
            //When the level is unstable, take screenshot for optical flow
            if (!ABStream.optical_flow) return;
            else {
                if (AIData.InSimulation) {
                    ABStream.frameNum++;
                    Application.CaptureScreenshot(ABStream.frameNum.ToString()+".png");
                    // SaveTextureToFile(ABStream.frameNum.ToString()+".png");
                }
                return;
            }
            // Finished
        }
        //if (GetBirdsAvailableAmount() == 0) return;

        Vector3 slingshotPos = ABGameWorld.Instance.Slingshot().transform.position - ABConstants.SLING_SELECT_POS;

        //Stop all simulation
        if (AIData.StopSimulation)
        {
            // print("2");
            Time.timeScale = 1;
            ResetLevel();
            if (GetPigsAvailableAmount() == 0) ShowLevelClearedBanner();
            else if (GetBirdsAvailableAmount() == 0) ShowLevelFailedBanner();
            else AIData.StopSimulation = false;
            return;
        }

        //Using too many reset makes game very lag so reload game
        if (AIData.ReloadBecauseOfVeryLag)
        {
            // print("3");
            AIData.ReloadBecauseOfVeryLag = false;
            BuildLevelFromXmlFile(AIData.ReloadXmlFile);
            return;
        }

        //Simulation time over, return to play
        if (AIData.root == null)
        {
            // print("4");
        }
        else if (AIData.root.nVisits >= AIData.MctsSimulationLimit && AIData.InSimulation == true)
        {
            // print("5");
            AIData.InSimulation = false;
            AIData.WaitToPlay = true;
            //AIData.rebuild = true;
            Time.timeScale = 1;
            BuildLevelFromXmlFile(AIData.root.XmlFileName);
            return;
        }

        //AI shall not control the game when it is do real shoot
        if (AIData.WaitToPlayEnd && !IsLevelStable())
        {
            // print("6");
            return;
        }
        if (AIData.WaitToPlayEnd && AIData.BirdsCountGameWorld != _birds.Count)
        {
            // print("7");
            return;
        }

        if (AIData.WaitToPlayEnd)
        {
            //AIData.BirdsCountGameWorld--;

            int NowPigsCount = _pigs.Count;
            double NowScore = HUD.Instance.GetScore();
            int NowBlocksCount = GetBlocksAvailableAmount();

            double ScoreToUpdate = 0.9 * (double)(AIData.AllPigsCount - NowPigsCount) / AIData.AllPigsCount
                                 + 0.1 * (double)(AIData.AllBlocksCount - NowBlocksCount) / AIData.AllBlocksCount;
            
            // YANG
            // Update ScoreToUpdate with new score
            // if (ABStream.optical_flow)
            //     ScoreToUpdate = CombineTGIFWithScore(ScoreToUpdate,ABStream.tgif2,0.5);
            // Finished

            AIData.play_percentage.Add(ScoreToUpdate);

            if (_birds.Count == 0 || _pigs.Count == 0)
            {
                AIData.play_end = true;
                return;
            }

            AIData.WaitToPlayEnd = false;
            AIData.NeedToMakeRoot = true;
            AIData.NeedToWriteGamestate = true;
        }

        //Do real shoot first if need
        if (AIData.WaitToPlay == true)
        {
            RealPlay();
            return;
        }
        else
        {
            
            if (!AIData.pureMcts)
            {
                if (AIData.NeedToReadAction)
                {
                    ReadActions();
                    return;
                }

                if (AIData.NeedToWriteGamestate)
                {
                    if (AIData.WaitCount > 50 && !_birds[0].JumpToSlingshot)
                    {
                        SaveGameWorldToArrayForPlay();
                        AIData.WaitCount = 0;
                    }
                    else
                    {
                        AIData.WaitCount++;
                    }
                    return;
                }
            }

            if (AIData.NeedToMakeRoot) {
                MakeRoot();
            }

            //Stop all simulation
            if (AIData.StopSimulation)
            {
                Time.timeScale = 1;
                //ResetLevel();
                return;
            }


            if (!AIData.InSimulation)
            {
                Time.timeScale = 100;
                // YANG
                // Optical flow needs to be slow to get movements
                if (ABStream.optical_flow)
                {
                    Time.timeScale = 20;
                }
                // Finished

                AIData.InSimulation = true;
            }

            DoSimulation();
        }

    }

    // YANG
    // For streaming. This code reads actions from file and act just like that.
    // Base code is RealPlay()
    private void StreamRealPlay()
    {
        if (!ABStream.streaming) return;
        if (!IsLevelStable()) return;
        if (_levelClearedBanner.active||_levelFailedBanner.active)  {
            Invoke("NextLevel", 1f);
            return;
        }
        Time.timeScale = 1;

        int randomAction = 0;

        if (AIData.WaitCount > 50 && !_birds[0].JumpToSlingshot)
        {
            Vector3 slingshotPos = ABGameWorld.Instance.Slingshot().transform.position - ABConstants.SLING_SELECT_POS;
            Vector2 dragVec = new Vector2();
            
            string pr = "";
            foreach (var item in ABStream.loadedActions[ABStream.currentLevel - 4])
            {
                    pr = pr +" " + item.ToString();
            }
            print("level : " + ABStream.currentLevel.ToString() + " " + pr);

            try
            {
                dragVec = ABStream.loadedActions[ABStream.currentLevel - 4][ABStream.actionNum];
                print(ABStream.actionNum.ToString()+ " " + dragVec.ToString());
            }
            catch
            {
                // Just in case there is no more action.
                // dragVec = ABStream.loadedActions[ABStream.currentLevel - 4][ABStream.actionNum-1];
                if (ABStream.r ==-1) {
                    ABStream.r = ABStream.loadedActions[ABStream.currentLevel - 4].Count();
                    ABStream.r = UnityEngine.Random.Range(0,ABStream.r);
                }
                dragVec = ABStream.loadedActions[ABStream.currentLevel - 4][ABStream.r];
                print("No actions left "+ABStream.currentLevel.ToString()+" "+ABStream.actionNum.ToString());
            }

            _birds[0].DragBird(dragVec);

            ABStream.delay += Time.deltaTime;
            if (ABStream.delay >= 1f)
            {
                _birds[0].LaunchBird();
                ABStream.delay = 0;
                ABStream.actionNum++;
                AIData.WaitCount = 0;
                ABStream.r = -1;
                ABStream.reloadLevel = true;
            }

        }
        // This part may make some error, but it's okay
        else if (!_birds[0].IsFlying && !_birds[0].IsDying)
        {
            AIData.WaitCount++;


            // YANG
            // To reload level
            // if (AIData.WaitCount <3&&ABStream.reloadLevel)
            // {
            //     //     string fileName = Application.dataPath + "/../.." + "/AbDatas/Worker4/xmls/MCTS/level-" +
            //     // ABStream.currentLevel.ToString() +
            //     // "-sim" + AIData.MctsSimulationLimit.ToString() +
            //     // "-" + (ABStream.reloadLevelCount).ToString() +
            //     // ".xml";
            //     //     if (ABStream.optical_flow)
            //     //         fileName = Application.dataPath + "/../.." + "/AbDatas/Worker4/xmls/OF/level-" +
            //     //             ABStream.currentLevel.ToString() +
            //     //             "-sim" + AIData.MctsSimulationLimit.ToString() +
            //     //             "-" + (ABStream.reloadLevelCount).ToString() +
            //     //             ".xml";
            //     // string[] xmls = System.IO.Directory.GetFiles("C:\\Project\\COG\\AbDatas\\Worker4\\xmls\\OF\\"
            //     //     , "*.xml").OrderBy(p => p.CreationTime);
            //     // string fileName = xmls[ABStream.reloadLevelCount];
            //     DirectoryInfo info = new DirectoryInfo("C:\\Project\\COG\\AbDatas\\Worker4\\xmls\\OF\\");
            //     FileInfo[] files = info.GetFiles("C:\\Project\\COG\\AbDatas\\Worker4\\xmls\\OF\\*.xml").OrderBy(p => p.CreationTime).ToArray();
            //     string fileName = files[ABStream.reloadLevelCount].FullName;

            //     BuildLevelFromXmlFile(fileName);
            //     print("Loaded level : "+fileName);
            //     ABStream.reloadLevel = false;
            //     ABStream.reloadLevelCount++;
            // }
        }
        else
        {
        }

    }

    private void RealPlay()
    {
        Vector3 slingshotPos = ABGameWorld.Instance.Slingshot().transform.position - ABConstants.SLING_SELECT_POS;
        Time.timeScale = 1;

        AIData.IsShootingInPlay = true;

        Node selected = AIData.root.GetMostVisted();

        Vector2 dragPos = new Vector2(slingshotPos.x + (float)(selected.x), slingshotPos.y + (float)(selected.y));

        if (ABStream.simulate&&ABStream.ver2)
        {
            StreamWriter file = new StreamWriter(ABStream.tempActionFileName, false);
            file.WriteLine(dragPos.x.ToString("F4") + " " + dragPos.y.ToString("F4"));
            file.Close();
            AIData.Reset();
            if (File.Exists(ABStream.tempStateFileName)) File.Delete(ABStream.tempStateFileName);
            ABSceneManager.Instance.LoadScene("MainMenu");
        }


        _birds[0].DragBird(dragPos);


        AIData.ShootingTimerInPlay += Time.deltaTime;
        //Drag and wait for some time when real shoot, it can let others see the shoot
        if (AIData.ShootingTimerInPlay >= 2.0f)
        {
            // YANG
            // Save Actions
            ABStream.actions = ABStream.actions + dragPos.x.ToString("F4") + " " + dragPos.y.ToString("F4") + ", ";
            print("realPlay :" + dragPos.x.ToString("F4") + " " + dragPos.y.ToString("F4"));
            // 

            _birds[0].LaunchBird();
            //string action = "" + (float)(selected.x) + "_" + (float)(selected.y);
            string action = "";
            for (int i = 0; i < AIData.root.children.Count; i++)
            {
                action += AIData.root.children[i].nVisits;
                if (i != AIData.root.children.Count - 1)
                {
                    action += "_";
                }
            }

            AIData.play_actions.Add(action);
            //SaveGameWorldToArrayForRecord();
            
            AIData.IsShootingInPlay = false;
            AIData.ShootingTimerInPlay = 0;
            AIData.BirdsCountGameWorld--;
            AIData.WaitToPlay = false;
            AIData.WaitToPlayEnd = true;
            AIData.SimulationCount = 0;
        }
    }

    private void MakeRoot()
    {
        if (AIData.root == null)
        {
            AIData.root = new Node(null);

            string FileName = SaveGameWorldToLevel();
            AIData.root.XmlFileName = FileName;

            AIData.SimulationStartTime = System.DateTime.Now;
            AIData.SimulationEndTime = AIData.SimulationStartTime.AddSeconds(AIData.SimulationTimeLimit);

            AIData.root.maxDepth = GetBirdsAvailableAmount();

            if (AIData.have_p)
            {
                AIData.root.have_p = true;
                AIData.root.SetP(AIData.ps);
                AIData.have_p = false;
            }
            AIData.root.Expand();

            AIData.BirdsCountGameWorld = GetBirdsAvailableAmount();
            AIData.BirdsCountSimulation = GetBirdsAvailableAmount();

            AIData.AllBirdsCount = BirdsAtStart;
            AIData.AllPigsCount = PigsAtStart;
            AIData.AllBlocksCount = BlocksAtStart;

            AIData.NodesToBeSimulated.Clear();

            //AIData.root.nVisits = 0;
        }
        else
        {
            AIData.root = new Node(null);
            //AIData.root = AIData.root.GetMostVisted();

            string FileName = SaveGameWorldToLevel();
            AIData.root.XmlFileName = FileName;

            AIData.SimulationStartTime = System.DateTime.Now;
            AIData.SimulationEndTime = AIData.SimulationStartTime.AddSeconds(AIData.SimulationTimeLimit);

            AIData.root.maxDepth = GetBirdsAvailableAmount();

            if (AIData.have_p)
            {
                AIData.root.have_p = true;
                AIData.root.SetP(AIData.ps);
                AIData.have_p = false;
            }
            AIData.root.Expand();
            AIData.root._parent = null;

            AIData.BirdsCountGameWorld = GetBirdsAvailableAmount();
            AIData.BirdsCountSimulation = GetBirdsAvailableAmount();

            AIData.NodesToBeSimulated.Clear();
            AIData.IsWaitingScoreInSimulation = false;
            AIData.IsShootingInSimulation = false;

            AIData.AllBirdsCount = BirdsAtStart - 1;
            AIData.AllPigsCount = GetPigsAvailableAmount();
            AIData.AllBlocksCount = GetBlocksAvailableAmount();

            AIData.NodesToBeSimulated.Clear();

            //AIData.root.ResetVisit();
        }
        AIData.NeedToMakeRoot = false;

        if (GetBirdsAvailableAmount() == 0 || GetPigsAvailableAmount() == 0)
        {
            AIData.StopSimulation = true;
        }
    }

    private void DoSimulation()
    {
        if (!AIData.InSimulation) return; //May avoid some trouble
        if (AIData.root == null) return;
        //If is waiting for score, wait to game stable
        if (AIData.IsWaitingScoreInSimulation)
        {
            WaitForScoreInSimulation();
            return;
        }

        //If shoot is seted, just do it
        if (AIData.IsShootingInSimulation)
        {
            ShootInSimulation();
            // StopCoroutine("shootInEnumeration");
            // StartCoroutine("shootInEnumeration");
            return;
        }

        //If there is no node to simulate, set next simulation
        if (AIData.NodesToBeSimulated.Count == 0)
        {
            SetSimulation();
            return;
        }

        //If there are some nodes to simulate, set the first to shoot
        {
            SetShootInSimulation();
            return;
        }
    }

    private void WaitForScoreInSimulation()
    {
        if (!IsLevelStable()) return;
        if (AIData.BirdsCountSimulation != GetBirdsAvailableAmount()) return;

        int NowPigsCount = _pigs.Count;
        double NowScore = HUD.Instance.GetScore();
        int NowBlocksCount = GetBlocksAvailableAmount();
        int NowBirdsCount = _birds.Count;

        double ScoreToUpdate = 0.9 * (double)(AIData.AllPigsCount - NowPigsCount) / AIData.AllPigsCount
                             + 0.1 * (double)(AIData.AllBlocksCount - NowBlocksCount) / AIData.AllBlocksCount
                             + (double)NowBirdsCount;

        //Fake Optical Flow
        // if (ABStream.optical_flow) {
        //     ScoreToUpdate = 0.5*ScoreToUpdate;
        //     double tdScore = ABStream.total_displacement/(double)2457;
        //     tdScore = 0.5*tdScore;
        //     ScoreToUpdate = ScoreToUpdate + tdScore;
        // }

        if (AIData.NodesToBeSimulated.Count == 1)
        {
            // YANG//////


            //Fake Optical Flow
            // if (ABStream.optical_flow)
            // {
            //     using (StreamWriter file = new StreamWriter("TotalDisplacement.txt",true))
            //     {
            //         file.Write(ABStream.total_displacement.ToString() + "\n");
            //         file.Close();
            //     }
            //     ABStream.total_displacement = 0;
            // }

            //Real Optical Flow (PWC)
            if (ABStream.optical_flow&&ABStream.frameNum>=2)
            {
                string[] flos = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/../../", "*.flo");
                string[] pngs = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/../../", "*.png");

                if (ABStream.startTGIF)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "python";
                    p.StartInfo.Arguments = "portableTGIF.py";
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;

                    p.StartInfo.WorkingDirectory = Application.streamingAssetsPath + "/../../";
                    p.StartInfo.UseShellExecute = false;

                    p.Start();
                    
                    ABStream.tgif = double.Parse(p.StandardOutput.ReadToEnd());
                    p.WaitForExit();
                    p.Close();

                    StreamWriter file = new StreamWriter(ABStream.tgifFileName,true);
                    file.WriteLine(ABStream.tgif);
                    file.Close();

                    if (ABStream.tgif != 0)
                    {
                        // print("8-1");
                        ABStream.startTGIF = false;
                        File.Delete("tgifDone.txt");
                        return;
                    }
                    else
                    {
                        // print("8-2");
                        // Normally it will not come to here, but just in case
                        return;
                    }
                }

                if (File.Exists("pwcProcessing.txt")) {
                    // print("2");
                    // if (ABStream.delay4 <= 1000f)
                    // {
                    //     ABStream.delay4 += Time.deltaTime;
                    //     return;
                    // } else if (ABStream.delay4 > 1000f) {
                    //     // If pwcnet got stuck, rerun it.
                    //     ABStream.TurnOnPWC();
                    // }
                    // ABStream.delay4 = 0;
                    return;
                }

                // Save _temp_source.txt, so that pwcnet can start computing.
                if (ABStream.startPWC)
                {
                    // print("3");
                    // if (ABStream.delay2 < 100f)
                    // {
                    //     ABStream.delay2 += Time.deltaTime;
                    //     return;
                    // }
                    // ABStream.delay2 = 0;
                    //Strat PWCNet
                    File.WriteAllText("C:\\Project\\COG\\ConsolePWC\\ConsolePWC\\bin\\Debug\\_temp_source.txt",
                        "C:\\Project\\COG\\ABMCTS-OF");
                    ABStream.startPWC = false;
                    return;
                }
                else if (File.Exists("pwcDone.txt") && flos.Length > 0)
                {
                    // print("6");
                    ABStream.startTGIF = true;
                    File.Delete("pwcDone.txt");
                    return;
                }
                // else
                // {
                //     print("7");
                //     ABStream.delay3+=Time.deltaTime;
                //     if (ABStream.delay3 < 500f)
                //         return;
                //     else {
                //         ABStream.startPWC = true;
                //         ABStream.delay3 = 0;
                //         return;
                //     }

                // }

                // print("PWC Finished");
                // //PWCNet Finished.

                // if (ABStream.frameNum-1!=flos.Length) {
                //     print("????");
                //     return;
                //     // Wait for OF and TGIF computation...
                // } else if (ABStream.frameNum!=pngs.Length) return;
                // else
                // {
                //     print("PWC Computation Finished");
                //     // Computation Done?
                // }

                // Start TGIF calculator after optical flow is gained.
                
                // print("9");
                if (ABStream.tgif != 0)
                    ABStream.DeleteScreenshots(false);
                else  {
                    // print("100");
                    return;
                }
                ScoreToUpdate = CombineTGIFWithScore(ScoreToUpdate,ABStream.tgif, 0.5);
                ABStream.frameNum = 0;
                ABStream.startPWC = true;
                ABStream.tgif=0;
            } else if (ABStream.optical_flow&&ABStream.frameNum<2) {
                ScoreToUpdate = ScoreToUpdate/2;
                ABStream.frameNum = 0;
                ABStream.startPWC = true;
                ABStream.tgif=0;
            }
            ////Finished

            // print(ScoreToUpdate);
            AIData.NodesToBeSimulated[0].UpdateStats(ScoreToUpdate);
            if (!ABStream.optical_flow)
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(ABStream.scoreFileName, true))
                {
                    file.WriteLine(ScoreToUpdate.ToString());
                    file.Close();
                }
            }
        }
        //AIData.NodesToBeSimulated.Clear();
        AIData.NodesToBeSimulated.RemoveAt(0);
        AIData.IsWaitingScoreInSimulation = false;
        AIData.SimulationCount++;
    }

    //YANG
    private double CombineTGIFWithScore(double orig, double tgif, double ratio)
    {
        //original score + normalized TGIF
        //CDF calculator setting
        double normtgif = 0;

        Process p = new Process();
        p.StartInfo.FileName = "python";
        p.StartInfo.Arguments = "CDFcalculator.py -x "+ tgif.ToString();
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.WorkingDirectory = Application.streamingAssetsPath + "/../../";
        p.StartInfo.UseShellExecute = false;
        p.Start();
        normtgif = double.Parse(p.StandardOutput.ReadToEnd());
        p.WaitForExit();
        p.Close();

        // With Birds left
        double score = ratio * orig + ratio * tgif;

        // Without Birds Left
        // if (orig >1 ) orig = 1;
        // double score = ratio * orig + ratio * tgif;

        // Geometric Mean
        // double score = Math.Sqrt(orig*tgif);

        //Saving the score difference
        using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(ABStream.scoreFileName,true))
        {
            file.WriteLine(orig.ToString()+","+score.ToString());
            file.Close();
        }
        return score;
    }
    // Finished
    
    private void ShootInSimulation()
    {
        if (_birds.Count == 0 || _pigs.Count == 0)
        {
            //AIData.NodesToBeSimulated.Clear();
            AIData.IsShootingInSimulation = false;

            // Original Code
            // AIData.NodesToBeSimulated[AIData.NodesToBeSimulated.Count - 1].UpdateStats(0.9);
            // AIData.NodesToBeSimulated.Clear();

            // YANG
            // It seems that it comes to here when there are left birds with level clear.
            // Original Code doesn't go to WaitforScoreInSimulation function.
            if (ABStream.optical_flow)
                //Go to WaitforScoreInSimulation function to get optical flow
                AIData.IsWaitingScoreInSimulation = true;
            else {
                AIData.NodesToBeSimulated[AIData.NodesToBeSimulated.Count - 1].UpdateStats(0.9);
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(ABStream.scoreFileName, true))
                {
                    file.WriteLine((0.91111).ToString());
                    file.Close();
                }
                AIData.NodesToBeSimulated.Clear();
            }
            // Finished


            return;
        }

        Vector3 slingshotPos = ABGameWorld.Instance.Slingshot().transform.position - ABConstants.SLING_SELECT_POS;
        float sling_x = slingshotPos.x;
        float sling_y = slingshotPos.y;

        //print(toBeSimulated[0].x);
        //print(toBeSimulated[0].y);

        Vector2 dragPos = new Vector2(slingshotPos.x + (float)(AIData.NodesToBeSimulated[0]).x, slingshotPos.y + (float)(AIData.NodesToBeSimulated[0].y));

        _birds[0].DragBird(dragPos);

        AIData.ShootingTimerInSimulation += Time.deltaTime;
        if (AIData.ShootingTimerInSimulation >= 0.20f)
        {
            AIData.IsShootingInSimulation = false;
            _birds[0].LaunchBird();
            AIData.IsWaitingScoreInSimulation = true;
            AIData.ShootingTimerInSimulation = 0;
            AIData.BirdsCountSimulation--;
        }
    }

    private void SetSimulation()
    {
        List<Node> visited = AIData.root.SelectAction();

        Node last = visited[visited.Count - 1];

        while (last.nVisits >= 5)
        {
            last.UpdateStats(last.q); // just re-use q when n>5
            AIData.SimulationCount++;
            if (AIData.root.nVisits >= AIData.MctsSimulationLimit) return;
            visited = AIData.root.SelectAction();
            last = visited[visited.Count - 1];
        }

        AIData.NodesToBeSimulated.Clear();
        foreach (Node node in visited)
        {
            AIData.NodesToBeSimulated.Add(node);
        }

        while (AIData.NodesToBeSimulated.Count < AIData.AllBirdsCount)
        {
            AIData.NodesToBeSimulated.Add(new Node(AIData.NodesToBeSimulated[AIData.NodesToBeSimulated.Count - 1]));
        }

        //BuildLevelFromXmlFile(AIData.root.XmlFileName);
        AIData.ReloadBecauseOfVeryLag = true;
        AIData.ReloadXmlFile = AIData.root.XmlFileName;

        ABSceneManager.Instance.LoadScene("GameWorld");
    }

    private void SetShootInSimulation()
    {
        AIData.IsShootingInSimulation = true;
    }

    private int XPositionToCoordinate(float x)
    {
        int i = 0;
        float width = GameplayCam._maxWidth;
        i = (int)((x + width / 2) / width * world_width);

        if (i < world_width) return i;
        else return 104;
    }

    private int world_width = 210;
    private int world_height = 120;

    private int YPositionToCoordinate(float y)
    {
        int j = 0;
        float camera_y = GameplayCam.transform.position.y;
        float width = GameplayCam._maxWidth;
        float h = width / world_width * world_height;
        j = (int)((camera_y - y + h / 2) / h * world_height);

        if (j < world_height) return j;
        else return 59;
    }

    private void DrawObjectOnWorldArray(Transform transform, float x_max, float y_max, int[,] world, int value)
    {
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;

        for (float a = x - x_max; a < x + x_max; a += (float)0.01)
        {
            for (float b = y - y_max; b < y + y_max; b += (float)0.01)
            {
                Vector3 p = new Vector3(a, b, z);
                bool f = transform.GetComponent<Renderer>().bounds.Contains(p);
                if (f)
                {
                    int xi = XPositionToCoordinate(a);
                    int yi = YPositionToCoordinate(b);
                    world[yi, xi] = value;
                }
            }
        }
    }

    private int[,] world;

    private string SaveGameWorldToArrayForPlay(bool intofile = true)
    {
        //ScreenCapture.CaptureScreenshot(Application.dataPath + "/savedata.PNG");
        //string folderFullName = "D:/AbDatas/Worker4/StateToDo/";
        //DirectoryInfo states_folder = new DirectoryInfo(folderFullName);
        //int new_state_index = states_folder.GetFiles().Length + 1;
        // string fileName = "D:/AbDatas/Worker4/StateToDo/StateToDo.txt";
        string fileName = Application.dataPath + "/../.." + "/AbDatas/Worker4/StateToDo/StateToDo.txt";



        AIData.NeedToWriteGamestate = false;
        AIData.NeedToReadAction = true;
        AIData.WaitCount = 0;
        return fileName;

        // string folderFullName = "D:/AbDatas/Worker4/states/";
        string folderFullName = Application.dataPath + "/../.." + "/AbDatas/Worker4/states/";
        DirectoryInfo states_folder = new DirectoryInfo(folderFullName);
        int new_state_index = states_folder.GetFiles().Length + 1;
        string recordFileName = folderFullName + new_state_index + ".txt";

        // string screenshotFolderFullName = "D:/AbDatas/Worker4/screenshots/";
        string screenshotFolderFullName = Application.dataPath + "/../.." + "/AbDatas/Worker4/screenshots/";
        //DirectoryInfo screenshotFolder = new DirectoryInfo(screenshotFolderFullName);
        //int new_screenshot_index = screenshotFolder.GetFiles().Length + 1;
        string screenFileName = screenshotFolderFullName + new_state_index + ".png";

        //ScreenCapture.CaptureScreenshot(screenFileName);

        world = new int[world_height, world_width];

        for (int i = 0; i < world_height; ++i)
        {
            for (int j = 0; j < world_width; ++j)
            {
                world[i, j] = 0;
            }
        }

        float width = GameplayCam._maxWidth;
        float h = width / world_width * world_height;
        float sling_x = _birds[0].transform.position.x;
        float sling_y = _birds[0].transform.position.y;
        float camera_y = GameplayCam.transform.position.y;

        int sling_x_int = (int)((sling_x + width / 2) / width * world_width);
        int sling_y_int = (int)((camera_y - sling_y + h / 2) / h * world_height);

        if (_birds[0].GetType().ToString().Equals("ABBird"))
        {
            world[sling_y_int, sling_x_int] = 11;
        }
        else if (_birds[0].GetType().ToString().Equals("ABBirdBlack")) world[sling_y_int, sling_x_int] = 12;
        else if (_birds[0].GetType().ToString().Equals("ABBirdWhite")) world[sling_y_int, sling_x_int] = 13;
        else if (_birds[0].GetType().ToString().Equals("ABBirdYellow")) world[sling_y_int, sling_x_int] = 14;
        else if (_birds[0].GetType().ToString().Equals("ABBBirdBlue")) world[sling_y_int, sling_x_int] = 15;

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {
            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("Basic") < 0) continue;

            int x = XPositionToCoordinate(_blocksTransform.GetChild(i).transform.position.x);
            int y = YPositionToCoordinate(_blocksTransform.GetChild(i).transform.position.y);

            world[y, x] = 21;

            DrawObjectOnWorldArray(_blocksTransform.GetChild(i), (float)2.07, (float)2.07, world, 21);
        }

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {

            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("Basic") >= 0 || name.IndexOf("TNT") >= 0) continue;

            string material = _blocksTransform.GetChild(i).GetComponent<ABBlock>()._material.ToString();
            int type = 0;

            if (material.Equals("wood")) type = 31;
            else if (material.Equals("stone")) type = 32;
            else if (material.Equals("ice")) type = 33;

            DrawObjectOnWorldArray(_blocksTransform.GetChild(i), (float)2.07, (float)2.07, world, type);
        }

        for (int i = 0; i < _plaftformsTransform.childCount; i++)
        {
            int x = XPositionToCoordinate(_plaftformsTransform.GetChild(i).transform.position.x);
            int y = YPositionToCoordinate(_plaftformsTransform.GetChild(i).transform.position.y);

            world[y, x] = 41;

            DrawObjectOnWorldArray(_plaftformsTransform.GetChild(i), (float)2.07, (float)2.07, world, 41);
        }

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {
            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("TNT") < 0) continue;

            int x = XPositionToCoordinate(_blocksTransform.GetChild(i).transform.position.x);
            int y = YPositionToCoordinate(_blocksTransform.GetChild(i).transform.position.y);

            world[y, x] = 51;

            DrawObjectOnWorldArray(_blocksTransform.GetChild(i), (float)2.07, (float)2.07, world, 51);
        }

        if (intofile)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter("StateToDo.txt"))
            {
                for (int i = 0; i < world_height; ++i)
                {
                    for (int j = 0; j < world_width; ++j)
                    {
                        file.Write(world[i, j]);
                        file.Write(" ");
                    }
                    file.Write("\n");
                }
                file.Close();
            }

            print("Write game state finished in file " + fileName + " and " + recordFileName);
            System.IO.File.Move("StateToDo.txt", recordFileName);
            //System.IO.File.Move("pic.png", screenFileName);
            AIData.play_states.Add(recordFileName);
            //System.IO.File.Move("StateToDo.txt", fileName);
        }
        AIData.NeedToWriteGamestate = false;
        AIData.NeedToReadAction = true;
        AIData.WaitCount = 0;
        return fileName;
    }

    private void Ver2RealPlay() {
        if (ABStream.simulate) return;
        if (_levelClearedBanner.active||_levelFailedBanner.active)  {
            Invoke("NextLevel", 1f);
            return;
        }
        if (_birds[0].IsFlying||_birds[0].IsDying) return;
        if (!File.Exists(ABStream.tempStateFileName)) {
            SaveGameWorldToLevel(ABStream.tempStateFileName);
            print("saved gameworld");
            return;
        }
        else if (File.Exists(ABStream.tempActionFileName)) {
            print("load action");
            System.IO.StreamReader file = new System.IO.StreamReader(ABStream.tempActionFileName);
            ABStream.loadFile = file.ReadToEnd();
            file.Close();
            File.Delete(ABStream.tempActionFileName);
        }
        else if (IsLevelStable()&& ABStream.loadFile != "")
        {
            print("Acting action");
            Vector2 dragVec = new Vector2();
            var act = ABStream.loadFile.Split(' ');
            dragVec.x = (float)Convert.ToDouble(act[0]);
            dragVec.y = (float)Convert.ToDouble(act[1]);

            _birds[0].DragBird(dragVec);

            ABStream.delay += Time.deltaTime;
            if (ABStream.delay >= 1f)
            {
                _birds[0].LaunchBird();
                ABStream.delay = 0;
                ABStream.loadFile = "";
                File.Delete(ABStream.tempStateFileName);
            }
        } else {
            print("Waiting for action..");
        }

    }

    private string SaveGameWorldToLevel(string fileName) {


        System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fileName);
        writer.WriteStartElement("Level");

        writer.WriteStartAttribute("width");
        writer.WriteValue(LevelWidth / ABConstants.LEVEL_ORIGINAL_SIZE.x);
        writer.WriteEndAttribute();
        writer.WriteString("\n");

        writer.WriteStartElement("Camera");
        writer.WriteStartAttribute("x");
        writer.WriteValue(GameplayCam.transform.position.x);
        writer.WriteStartAttribute("y");
        writer.WriteValue(GameplayCam.transform.position.y);
        writer.WriteStartAttribute("minWidth");
        writer.WriteValue(GameplayCam._minWidth);
        writer.WriteStartAttribute("maxWidth");
        writer.WriteValue(GameplayCam._maxWidth);
        writer.WriteEndElement();
        writer.WriteString("\n");

        writer.WriteStartElement("Birds");
        writer.WriteString("\n");

        for (int i = 0; i < _birds.Count; i++)
        {
            writer.WriteStartElement("Bird");
            writer.WriteStartAttribute("type");
            string type = "";
            if (_birds[i].GetType().ToString().Equals("ABBird")) type = "BirdRed";
            else if (_birds[i].GetType().ToString().Equals("ABBirdBlack")) type = "BirdBlack";
            else if (_birds[i].GetType().ToString().Equals("ABBirdWhite")) type = "BirdWhite";
            else if (_birds[i].GetType().ToString().Equals("ABBirdYellow")) type = "BirdYellow";
            else if (_birds[i].GetType().ToString().Equals("ABBBirdBlue")) type = "BirdBlue";
            writer.WriteValue(type);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }
        writer.WriteEndElement();
        writer.WriteString("\n");

        writer.WriteStartElement("Slingshot");

        writer.WriteStartAttribute("x");
        writer.WriteValue(_slingshot.transform.position.x);

        writer.WriteStartAttribute("y");
        writer.WriteValue(_slingshot.transform.position.y);
        writer.WriteEndElement();
        writer.WriteString("\n");

        writer.WriteStartElement("GameObjects");
        writer.WriteString("\n");

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {
            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("Basic") >= 0 || name.IndexOf("TNT") >= 0) continue;
            writer.WriteStartElement("Block");
            writer.WriteStartAttribute("type");
            writer.WriteValue(name);
            writer.WriteStartAttribute("material");
            writer.WriteValue(_blocksTransform.GetChild(i).GetComponent<ABBlock>()._material.ToString());
            writer.WriteStartAttribute("x");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.x);
            writer.WriteStartAttribute("y");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.y);
            writer.WriteStartAttribute("rotation");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.rotation.eulerAngles.z);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {
            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("Basic") < 0) continue;
            writer.WriteStartElement("Pig");
            writer.WriteStartAttribute("type");
            writer.WriteValue(name);
            writer.WriteStartAttribute("material");

            writer.WriteStartAttribute("x");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.x);
            writer.WriteStartAttribute("y");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.y);
            writer.WriteStartAttribute("rotation");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.rotation.eulerAngles.z);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }
        for (int i = 0; i < _plaftformsTransform.childCount; i++)
        {
            writer.WriteStartElement("Platform");
            writer.WriteStartAttribute("type");
            writer.WriteValue("Platform");
            writer.WriteStartAttribute("material");

            writer.WriteStartAttribute("x");
            writer.WriteValue(_plaftformsTransform.GetChild(i).transform.position.x);
            writer.WriteStartAttribute("y");
            writer.WriteValue(_plaftformsTransform.GetChild(i).transform.position.y);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {
            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("TNT") < 0) continue;
            writer.WriteStartElement("TNT");
            writer.WriteStartAttribute("type");

            writer.WriteStartAttribute("material");

            writer.WriteStartAttribute("x");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.x);
            writer.WriteStartAttribute("y");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.y);
            writer.WriteStartAttribute("rotation");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.rotation.eulerAngles.z);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }

        writer.WriteEndElement();
        writer.WriteString("\n");
        writer.WriteEndElement();
        writer.WriteString("\n");
        writer.Close();

        return fileName;
    }

    private string SaveGameWorldToLevel()
    {
        int FileIndex = 1 + AIData.XmlIndex;
        // Original
        // string fileName = "D:/AbDatas/Worker4/xmls/level-" + FileIndex.ToString() + ".xml";

        // YANG
        // Save the node states
        // string fileName = Application.dataPath + "/../.." + "/AbDatas/Worker4/xmls/level-" + FileIndex.ToString() + ".xml";
        string fileName = Application.dataPath + "/../.." + "/AbDatas/Worker4/xmls/MCTS/level-" + 
            ABStream.currentLevel.ToString()+
            "-sim"+AIData.MctsSimulationLimit.ToString()+
            "-"+ABStream.additionalFileName+
            "-"+FileIndex.ToString()+
            ".xml";
        if (ABStream.optical_flow) 
            fileName = Application.dataPath + "/../.." + "/AbDatas/Worker4/xmls/OF/level-" + 
                ABStream.currentLevel.ToString() +
                "-sim"+AIData.MctsSimulationLimit.ToString()+
                "-"+ABStream.additionalFileName+
                "-"+FileIndex.ToString()+
                ".xml";
        // Finished


        System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fileName);
        writer.WriteStartElement("Level");

        writer.WriteStartAttribute("width");
        writer.WriteValue(LevelWidth / ABConstants.LEVEL_ORIGINAL_SIZE.x);
        writer.WriteEndAttribute();
        writer.WriteString("\n");

        writer.WriteStartElement("Camera");
        writer.WriteStartAttribute("x");
        writer.WriteValue(GameplayCam.transform.position.x);
        writer.WriteStartAttribute("y");
        writer.WriteValue(GameplayCam.transform.position.y);
        writer.WriteStartAttribute("minWidth");
        writer.WriteValue(GameplayCam._minWidth);
        writer.WriteStartAttribute("maxWidth");
        writer.WriteValue(GameplayCam._maxWidth);
        writer.WriteEndElement();
        writer.WriteString("\n");

        writer.WriteStartElement("Birds");
        writer.WriteString("\n");

        for (int i = 0; i < _birds.Count; i++)
        {
            writer.WriteStartElement("Bird");
            writer.WriteStartAttribute("type");
            string type = "";
            if (_birds[i].GetType().ToString().Equals("ABBird")) type = "BirdRed";
            else if (_birds[i].GetType().ToString().Equals("ABBirdBlack")) type = "BirdBlack";
            else if (_birds[i].GetType().ToString().Equals("ABBirdWhite")) type = "BirdWhite";
            else if (_birds[i].GetType().ToString().Equals("ABBirdYellow")) type = "BirdYellow";
            else if (_birds[i].GetType().ToString().Equals("ABBBirdBlue")) type = "BirdBlue";
            writer.WriteValue(type);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }
        writer.WriteEndElement();
        writer.WriteString("\n");

        writer.WriteStartElement("Slingshot");

        writer.WriteStartAttribute("x");
        writer.WriteValue(_slingshot.transform.position.x);

        writer.WriteStartAttribute("y");
        writer.WriteValue(_slingshot.transform.position.y);
        writer.WriteEndElement();
        writer.WriteString("\n");

        writer.WriteStartElement("GameObjects");
        writer.WriteString("\n");

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {
            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("Basic") >= 0 || name.IndexOf("TNT") >= 0) continue;
            writer.WriteStartElement("Block");
            writer.WriteStartAttribute("type");
            writer.WriteValue(name);
            writer.WriteStartAttribute("material");
            writer.WriteValue(_blocksTransform.GetChild(i).GetComponent<ABBlock>()._material.ToString());
            writer.WriteStartAttribute("x");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.x);
            writer.WriteStartAttribute("y");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.y);
            writer.WriteStartAttribute("rotation");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.rotation.eulerAngles.z);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {
            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("Basic") < 0) continue;
            writer.WriteStartElement("Pig");
            writer.WriteStartAttribute("type");
            writer.WriteValue(name);
            writer.WriteStartAttribute("material");

            writer.WriteStartAttribute("x");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.x);
            writer.WriteStartAttribute("y");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.y);
            writer.WriteStartAttribute("rotation");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.rotation.eulerAngles.z);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }
        for (int i = 0; i < _plaftformsTransform.childCount; i++)
        {
            writer.WriteStartElement("Platform");
            writer.WriteStartAttribute("type");
            writer.WriteValue("Platform");
            writer.WriteStartAttribute("material");

            writer.WriteStartAttribute("x");
            writer.WriteValue(_plaftformsTransform.GetChild(i).transform.position.x);
            writer.WriteStartAttribute("y");
            writer.WriteValue(_plaftformsTransform.GetChild(i).transform.position.y);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }

        for (int i = 0; i < _blocksTransform.childCount; i++)
        {
            string name = _blocksTransform.GetChild(i).name.Replace("(Clone)", "");
            if (name.IndexOf("TNT") < 0) continue;
            writer.WriteStartElement("TNT");
            writer.WriteStartAttribute("type");

            writer.WriteStartAttribute("material");

            writer.WriteStartAttribute("x");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.x);
            writer.WriteStartAttribute("y");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.position.y);
            writer.WriteStartAttribute("rotation");
            writer.WriteValue(_blocksTransform.GetChild(i).transform.rotation.eulerAngles.z);
            writer.WriteEndElement();
            writer.WriteString("\n");
        }

        writer.WriteEndElement();
        writer.WriteString("\n");
        writer.WriteEndElement();
        writer.WriteString("\n");
        writer.Close();

        AIData.XmlIndex++;

        // YANG
        // Save the first level state filename for streaming
        if (!ABStream.streaming && ABStream.saveLevelPath)
        {
            ABStream.levels = fileName;
            ABStream.saveLevelPath = false;
        }
        // Finished

        return fileName;
    }

    private ABLevel BuildLevelFromXmlFile(string xmlFileName)
    {
        string fileName = xmlFileName;

        string xmlText;
        StreamReader stream = new StreamReader(fileName);
        xmlText = stream.ReadToEnd();
        stream.Close();

        ABLevel level = LevelLoader.LoadXmlLevel(xmlText);
        DecodeLevel(level);

        AIData.BirdsCountSimulation = GetBirdsAvailableAmount();

        return level;
    }

    private void WriteToResult()
    {
        //if(_pigs.Count == 0)
        //{
        //    for (int i = 0; i < AIData.play_actions.Count; i++) AIData.play_percentage.Add(1);
        //}
        //else
        //{
        //    for (int i = 0; i < AIData.play_actions.Count; i++) AIData.play_percentage.Add(-1);
        //}

        int min_count = 999;
        if (AIData.play_actions.Count < min_count) min_count = AIData.play_actions.Count;
        if (AIData.play_percentage.Count < min_count) min_count = AIData.play_actions.Count;
        if (AIData.play_states.Count < min_count) min_count = AIData.play_actions.Count;
        print(min_count);

        //if (AIData.play_actions.Count == AIData.play_percentage.Count && AIData.play_actions.Count == AIData.play_states.Count)
        //{
        //System.Random r = new System.Random();
        //int i = r.Next(AIData.play_actions.Count);

        // string folderFullName = "D:/AbDatas/Worker4/results/";

        string folderFullName = Application.dataPath + "/../.." + "/AbDatas/Worker4/results/";
        DirectoryInfo results_folder = new DirectoryInfo(folderFullName);

        int new_result_index = results_folder.GetFiles().Length + 1;

        // string fileName = "D:/AbDatas/Worker4/results/" + new_result_index + ".txt";
        string fileName = Application.dataPath + "/../.." + "/AbDatas/Worker4/results/" + new_result_index + ".txt";

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@fileName))
        {

            for (int i = 0; i < min_count; i++)
            {
                if (i == 0) file.WriteLine("level");
                else file.WriteLine("shoot");
                // file.WriteLine(AIData.play_states[i]);
                file.WriteLine(AIData.play_actions[i]);
                file.WriteLine(AIData.play_percentage[i]);
            }
            file.Close();
            print("Save result finished in file " + fileName);
        }
        //}
        AIData.write_end = true;

    }


    public bool IsObjectOutOfWorld(Transform abGameObject, Collider2D abCollider)
    {

        Vector2 halfSize = abCollider.bounds.size / 2f;

        if (abGameObject.position.x - halfSize.x > LevelWidth / 2f ||
           abGameObject.position.x + halfSize.x < -LevelWidth / 2f)

            return true;

        return false;
    }

    void ManageBirds()
    {

        if (_birds.Count == 0)
            return;

        // Move next bird to the slingshot
        if (_birds[0].JumpToSlingshot)
            _birds[0].SetBirdOnSlingshot();

        //		int birdsLayer = LayerMask.NameToLayer("Birds");
        //		int blocksLayer = LayerMask.NameToLayer("Blocks");
        //		if(_birds[0].IsFlying || _birds[0].IsDying)
        //			
        //			Physics2D.IgnoreLayerCollision(birdsLayer, blocksLayer, false);
        //		else 
        //			Physics2D.IgnoreLayerCollision(birdsLayer, blocksLayer, true);
    }

    public ABBird GetCurrentBird()
    {

        if (_birds.Count > 0)
            return _birds[0];

        return null;
    }

    public void NextLevel()
    {
        AIData.Reset();
        AIData.play_end = false;

        // YANG
        // Save actions and level name at the end of the level.
        
        if (!AIData.InSimulation) {
        if (!ABStream.streaming)
        {
            ABStream.finish = true;
            ABStream.actions = ABStream.currentLevel.ToString() +") "+ABStream.actions;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(ABStream.actionFileName, true))
            {
                file.Write(ABStream.actions);
                file.Close();
            }
            print(ABStream.actions);
            ABStream.actions = "";
            

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(ABStream.levelFileName,true))
            {
                file.Write(ABStream.levels + "\n");
                file.Close();
            }
            print("Saved level " + ABStream.levels);
            ABStream.levels = "";
            ABStream.saveLevelPath = true;
        }
        ABStream.currentLevel++;
        ABStream.actionNum = 0;
        // Finished
        }

        if (LevelList.Instance.NextLevel() == null) {
            // Original Code
            // ABSceneManager.Instance.LoadScene("MainMenu");

            // YANG
            // When there is no more level, finish the game in case of simulation but keep playing in case of streaming
            if (ABStream.streaming) {
                ABStream.currentLevel = 4;
                ABSceneManager.Instance.LoadScene("MainMenu");
            } else {
                ABStream.finish = true;
                Application.Quit();
                ABSceneManager.Instance.LoadScene("MainMenu");
            }
            // Finished

        }
        else {
            ABSceneManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ResetLevel()
    {
        AIData.Reset();
        AIData.play_end = false;

        if (_levelFailedBanner.activeSelf)
            _levelTimesTried++;

        ABSceneManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddTrajectoryParticle(ABParticle trajectoryParticle)
    {

        _birdTrajectory.Add(trajectoryParticle);
    }

    public void RemoveLastTrajectoryParticle()
    {

        foreach (ABParticle part in _birdTrajectory)
            part.Kill();
    }

    public void AddBird(ABBird readyBird)
    {

        if (_birds.Count == 0)
            readyBird.GetComponent<Rigidbody2D>().gravityScale = 0f;

        if (readyBird != null)
            _birds.Add(readyBird);
    }

    public GameObject AddBird(GameObject original, Quaternion rotation)
    {

        Vector3 birdsPos = _slingshot.transform.position - ABConstants.SLING_SELECT_POS;

        if (_birds.Count >= 1)
        {

            birdsPos.y = _slingshot.transform.position.y;

            for (int i = 0; i < _birds.Count; i++)
            {

                if ((i + 1) % _birdsAmounInARow == 0)
                {

                    float coin = (UnityEngine.Random.value < 0.5f ? 1f : -1);
                    birdsPos.x = _slingshot.transform.position.x + (UnityEngine.Random.value * 0.5f * coin);
                }

                birdsPos.x -= ABWorldAssets.BIRDS[original.name].GetComponent<SpriteRenderer>().bounds.size.x * 1.75f;
            }
        }

        GameObject newGameObject = (GameObject)Instantiate(original, birdsPos, rotation);
        Vector3 scale = newGameObject.transform.localScale;
        scale.x = original.transform.localScale.x;
        scale.y = original.transform.localScale.y;
        newGameObject.transform.localScale = scale;

        newGameObject.transform.parent = _birdsTransform;
        newGameObject.name = "bird_" + _birds.Count;

        ABBird bird = newGameObject.GetComponent<ABBird>();
        bird.SendMessage("InitSpecialPower", SendMessageOptions.DontRequireReceiver);

        if (_birds.Count == 0)
            bird.GetComponent<Rigidbody2D>().gravityScale = 0f;

        if (bird != null)
            _birds.Add(bird);

        return newGameObject;
    }

    public GameObject AddPig(GameObject original, Vector3 position, Quaternion rotation, float scale = 1f)
    {

        GameObject newGameObject = AddBlock(original, position, rotation, scale);

        ABPig pig = newGameObject.GetComponent<ABPig>();
        if (pig != null)
            _pigs.Add(pig);

        return newGameObject;
    }

    public GameObject AddPlatform(GameObject original, Vector3 position, Quaternion rotation, float scaleX = 1f, float scaleY = 1f)
    {

        GameObject platform = AddBlock(original, position, rotation, scaleX, scaleY);
        platform.transform.parent = _plaftformsTransform;

        return platform;
    }

    public GameObject AddBlock(GameObject original, Vector3 position, Quaternion rotation, float scaleX = 1f, float scaleY = 1f)
    {

        GameObject newGameObject = (GameObject)Instantiate(original, position, rotation);
        newGameObject.transform.parent = _blocksTransform;

        Vector3 newScale = newGameObject.transform.localScale;
        newScale.x = scaleX;
        newScale.y = scaleY;
        newGameObject.transform.localScale = newScale;

        return newGameObject;
    }

    private void ShowLevelFailedBanner()
    {
        if (AIData.InSimulation)
            return;

        if (_levelCleared)
            return;

        if (!IsLevelStable())
        {
            Invoke("ShowLevelFailedBanner", 1f);
        }
        else
        {
            //Update GameState
            GameState = GameStateX.FAIL;
            // Player lost the game
            HUD.Instance.gameObject.SetActive(false);

            if (_levelTimesTried < _timesToGiveUp - 1)
            {
                _levelFailedBanner.SetActive(true);
            }
            else
            {
                _levelClearedBanner.SetActive(true);
                _levelClearedBanner.GetComponentInChildren<Text>().text = "Level Failed!";
            }

            // Original Code
            // NextLevel();

            // YANG
            if (ABStream.actions == "") return;
            if (ABStream.streaming)
            {
                // Wait for a while before going to next level in streaming
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(ABStream.streamingResult, true))
                {
                    file.WriteLine(ABStream.currentLevel.ToString() + ", " +
                    HUD.Instance.GetScore().ToString() +", "+
                    "Failed");
                    file.Close();
                }
                Invoke("NextLevel", 1f);
            }
            else
            {
                // Level Failed
                ABStream.actions = ABStream.actions+"F;"+HUD.Instance.GetScore().ToString()+"\n";
                NextLevel();
            }
            // Finished

        }
    }

    private void ShowLevelClearedBanner()
    {
        if (AIData.InSimulation)
            return;

        if (!IsLevelStable())
        {
            Invoke("ShowLevelClearedBanner", 1f);
        }
        else
        {
            //Update GameState
            GameState = GameStateX.CLEAR;

            // Player won the game
            HUD.Instance.gameObject.SetActive(false);

            _levelClearedBanner.SetActive(true);
            _levelClearedBanner.GetComponentInChildren<Text>().text = "Level Cleared!";

            // YANG
            if (ABStream.actions == "") return;
            if (ABStream.streaming)
            {
                // Wait for a while before going to next level in streaming
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(ABStream.streamingResult, true))
                {
                    file.WriteLine(ABStream.currentLevel.ToString() + ", " +
                    HUD.Instance.GetScore().ToString() +", "+
                    "Failed");
                    file.Close();
                }
                Invoke("NextLevel", 1f);
            }
            else
            {
                // Level Cleared
                ABStream.actions = ABStream.actions+"C;"+HUD.Instance.GetScore().ToString()+"\n";
                NextLevel();
            }
            // Finished

        }
    }

    public void KillPig(ABPig pig)
    {

        _pigs.Remove(pig);

        if (_pigs.Count == 0)
        {

            // Check if player won the game
            if (!_isSimulation & !AIData.InSimulation)
            {

                _levelCleared = true;
                Invoke("ShowLevelClearedBanner", _timeToResetLevel);
            }

            return;
        }
    }

    public void KillBird(ABBird bird)
    {

        if (!_birds.Contains(bird))
            return;

        _birds.Remove(bird);

        if (_birds.Count == 0)
        {

            // Check if player lost the game
            if (!_isSimulation & !AIData.InSimulation)
            {
                Invoke("ShowLevelFailedBanner", _timeToResetLevel);
            }

            return;
        }

        //Code For Get Game Data Tool(DataGettorX)
        BirdID++;
        //Code For Get Game Data Tool(DataGettorX)

        _birds[0].GetComponent<Rigidbody2D>().gravityScale = 0f;
        _birds[0].JumpToSlingshot = true;
    }

    public int GetPigsAvailableAmount()
    {

        return _pigs.Count;
    }

    public int GetBirdsAvailableAmount()
    {

        return _birds.Count;
    }

    public int GetBlocksAvailableAmount()
    {

        int blocksAmount = 0;

        foreach (Transform b in _blocksTransform)
        {

            if (b.GetComponent<ABPig>() == null)

                for (int i = 0; i < b.GetComponentsInChildren<Rigidbody2D>().Length; i++)
                    blocksAmount++;
        }

        return blocksAmount;
    }

    public bool IsLevelStable()
    {

        return GetLevelStability() == 0f;
    }

    public float GetLevelStability()
    {

        float totalVelocity = 0f;

        foreach (Transform b in _blocksTransform)
        {

            Rigidbody2D[] bodies = b.GetComponentsInChildren<Rigidbody2D>();

            foreach (Rigidbody2D body in bodies)
            {

                if (!IsObjectOutOfWorld(body.transform, body.GetComponent<Collider2D>()))
                    totalVelocity += body.velocity.magnitude;
            }
        }

        return totalVelocity;
    }

    public List<GameObject> BlocksInScene()
    {

        List<GameObject> objsInScene = new List<GameObject>();

        foreach (Transform b in _blocksTransform)
            objsInScene.Add(b.gameObject);

        return objsInScene;
    }

    public Vector3 DragDistance()
    {

        Vector3 selectPos = (_slingshot.transform.position - ABConstants.SLING_SELECT_POS);
        return _slingshotBaseTransform.transform.position - selectPos;
    }

    public void SetSlingshotBaseActive(bool isActive)
    {

        _slingshotBaseTransform.gameObject.SetActive(isActive);
    }

    public void ChangeSlingshotBasePosition(Vector3 position)
    {

        _slingshotBaseTransform.transform.position = position;
    }

    public void ChangeSlingshotBaseRotation(Quaternion rotation)
    {

        _slingshotBaseTransform.transform.rotation = rotation;
    }

    public bool IsSlingshotBaseActive()
    {

        return _slingshotBaseTransform.gameObject.activeSelf;
    }

    public Vector3 GetSlingshotBasePosition()
    {

        return _slingshotBaseTransform.transform.position;
    }

    public void StartWorld()
    {

        _pigsAtStart = GetPigsAvailableAmount();
        _birdsAtStart = GetBirdsAvailableAmount();
        birdscount = GetBirdsAvailableAmount();
        _blocksAtStart = GetBlocksAvailableAmount();

        if (!AIData.pureMcts) AIData.BirdsCountGameWorld = GetBirdsAvailableAmount();
    }

    public void ClearWorld()
    {

        foreach (Transform b in _blocksTransform)
            Destroy(b.gameObject);

        _pigs.Clear();

        foreach (Transform b in _birdsTransform)
            Destroy(b.gameObject);

        _birds.Clear();
    }

    private void AdaptCameraWidthToLevel()
    {

        Collider2D[] bodies = _blocksTransform.GetComponentsInChildren<Collider2D>();

        if (bodies.Length == 0)
            return;

        // Adapt the camera to show all the blocks		
        float levelLeftBound = -LevelWidth / 2f;
        float groundSurfacePos = LevelHeight / 2f;

        float minPosX = Mathf.Infinity;
        float maxPosX = -Mathf.Infinity;
        float maxPosY = -Mathf.Infinity;

        // Get position of first non-empty stack
        for (int i = 0; i < bodies.Length; i++)
        {
            float minPosXCandidate = bodies[i].transform.position.x - bodies[i].bounds.size.x / 2f;
            if (minPosXCandidate < minPosX)
                minPosX = minPosXCandidate;

            float maxPosXCandidate = bodies[i].transform.position.x + bodies[i].bounds.size.x / 2f;
            if (maxPosXCandidate > maxPosX)
                maxPosX = maxPosXCandidate;

            float maxPosYCandidate = bodies[i].transform.position.y + bodies[i].bounds.size.y / 2f;
            if (maxPosYCandidate > maxPosY)
                maxPosY = maxPosYCandidate;
        }

        float cameraWidth = Mathf.Abs(minPosX - levelLeftBound) +
            Mathf.Max(Mathf.Abs(maxPosX - minPosX), Mathf.Abs(maxPosY - groundSurfacePos));
            // Mathf.Max(Mathf.Abs(maxPosX - minPosX), Mathf.Abs(maxPosY - groundSurfacePos)) + 0.5f;
            //YANG
            // Camera Zoom out

        GameplayCam.SetCameraWidth(cameraWidth);
    }
}
