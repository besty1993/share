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

﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System;
using System.IO;

public class ABMenu : MonoBehaviour {


	public void LoadNextScene(string sceneName) {

		ABSceneManager.Instance.LoadScene(sceneName);
	}

	public void LoadNextScene(string sceneName, bool loadTransition, ABSceneManager.ActionBetweenScenes action) {

		ABSceneManager.Instance.LoadScene(sceneName, loadTransition, action);
	}

    public void Start()
    {
        Application.runInBackground = true;

		ABStream.tempStateFileName = Application.dataPath + "/../../AbDatas/simulationState" + ".xml";
        ABStream.tempActionFileName = Application.dataPath + "/../../AbDatas/simulationAction" + ".txt";

        Scene scene = SceneManager.GetActiveScene();

        if (ABStream.ver2&&ABStream.simulate) {
            return;
        }

        if(scene.name == "MainMenu"&&!ABStream.finish)
        {
            ABSceneManager.Instance.LoadScene("LevelSelectMenu");
        }
        if (scene.name == "MainMenu"&&ABStream.finish) {
            Application.Quit();
        }

        //ABSceneManager.Instance.LoadScene("LevelSelectMenu");
    }

    void Update() {
        Scene scene = SceneManager.GetActiveScene();
        string temp = File.ReadAllText (ABStream.tempStateFileName);
		if (scene.name == "MainMenu" &&
        ABStream.simulate && ABStream.ver2 &&
        File.Exists (ABStream.tempStateFileName) && 
        ABStream.loadFile != temp) {
//		if (File.Exists (simulationLevelFilePath) && !File.Exists (tntPositionFilePath)) {
				
			ABStream.loadFile = temp;
//			print ("New loadFile: " + loadFile);
			print ("To GameWorld???");
			if (ABStream.loadFile.Contains("Pig"))
//				LoadNextScene ("GameWorld");
                
				ABSceneManager.Instance.LoadScene("LevelSelectMenu");
		} 
//		else
//			print ("File Same");
	}

}
