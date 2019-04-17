using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataGettorX : MonoBehaviour {
	private Transform  _blocksTransform;
	private Transform  _birdsTransform;
	private Transform  _plaftformsTransform;
	private Transform  _slingshotBaseTransform;
	private int DataID=0;
	public static List<string> GameDataList=new List<string>();
	private float lastTime;
	private float curTime;
	public static float Timeing;

	public float FixTimeing;

	private string FullGameData="";

	ABLevel currentLevel=null;

	List<string> birdsType=null;

	private void init()
	{
		birdsType = new List<string> ();
		currentLevel = LevelList.Instance.GetCurrentLevel ();
		foreach(BirdData gameObj in currentLevel.birds)
		{
			birdsType.Add(gameObj.type);
		}
		_blocksTransform = GameObject.Find ("Blocks").transform;
		_birdsTransform  = GameObject.Find ("Birds").transform;
		_plaftformsTransform = GameObject.Find ("Platforms").transform;
		this.DataID=0;
		this.FullGameData = "";
		if (GameDataList != null) {
			GameDataList.Clear ();
		}
		//GameDataList = new List<string> ();
	}

	// Use this for initialization
	void Start () {
		Timeing = FixTimeing;
		lastTime = Time.time;
		init ();
	}

	public static void StopGetGameData () {
		DataGettorX.Timeing = float.PositiveInfinity;
	}

//	public static void StartGetGameData () {
//		DataGettorX.Timeing = this.FixTimeing;
//	}

	// Update is called once per frame
	void Update () {
		curTime = Time.time;

		if (curTime - lastTime >= Timeing && false)
		{
			PrintGameData ();
			CollectGameData();
			lastTime = curTime;
		}
	}

	public void PrintGameData()
	{
        StreamWriter fileOut = new StreamWriter("test.txt");

        fileOut.WriteLine("LevelNumber" + (LevelList.Instance.CurrentIndex + 1) + "." + (ABGameWorld._levelTimesTried + 1));
        fileOut.Close();

        Debug.Log ("LevelNumber"+(LevelList.Instance.CurrentIndex+1)+"."+(ABGameWorld._levelTimesTried+1));

		if(ABGameWorld.BirdID<birdsType.Count)
		Debug.Log("BirdID,Bird_Type:"+(ABGameWorld.BirdID+1)+":"+birdsType[ABGameWorld.BirdID]);

		Debug.Log("Bird_X:"+ABGameWorld._birds[0].transform.position.x);
		Debug.Log("Bird_Y:"+ABGameWorld._birds[0].transform.position.y);
		Debug.Log("Bird_Z:"+ABGameWorld._birds[0].transform.rotation.z);

		foreach (Transform block in _blocksTransform)
		{	
			string name = block.transform.name.Replace ("(Clone)", "");
			Debug.Log("Name:"+name);
			Debug.Log("X:"+block.transform.position.x);
			Debug.Log("Y:"+block.transform.position.y);
			Debug.Log("Rotation:"+block.transform.rotation.z);
			Debug.Log("Life:"+block.GetComponent<ABGameObject>()._life);

			if (name.IndexOf ("Basic") < 0　&& name.IndexOf ("TNT") < 0　) {
				Debug.Log("Material:"+block.GetComponent<ABBlock>()._material);
			}
		}

		foreach (Transform plaftform in _plaftformsTransform)
		{	
			string name = plaftform.transform.name.Replace ("(Clone)", "");
			Debug.Log("Name:"+name);
			Debug.Log("X:"+plaftform.transform.position.x);
			Debug.Log("Y:"+plaftform.transform.position.y);
			Debug.Log("Rotation:"+plaftform.transform.rotation.z);
		}
	}

	public void CollectGameData()
	{
        //Debug.Log ("GameDataList:"+GameDataList.Count);
        //"LevelNumber,ID,GameObjectName,Material,PosX,PosY,RotationZ,GameObjectLife,GameState"

        //Debug.Log ("LevelNumber"+(LevelList.Instance.CurrentIndex+1));
        this.FullGameData = "";

		if (ABGameWorld.BirdID < birdsType.Count && ABGameWorld._birds.Count > 0) {
			this.FullGameData += (LevelList.Instance.CurrentIndex + 1) + "." + (ABGameWorld._levelTimesTried + 1) + ",";
			this.FullGameData += DataID.ToString ()+ ",";
			this.FullGameData += (ABGameWorld.BirdID + 1) + ":" + birdsType [ABGameWorld.BirdID]+ ",";
			this.FullGameData += ""+",";
			this.FullGameData += ABGameWorld._birds[0].transform.position.x+",";
			this.FullGameData += ABGameWorld._birds[0].transform.position.y+",";
			this.FullGameData += ABGameWorld._birds[0].transform.rotation.z+",";
			this.FullGameData += ""+",";
			this.FullGameData += ABGameWorld.GameState;

			GameDataList.Add (this.FullGameData);
			this.FullGameData = "";
		}

		this.FullGameData = "";

		foreach (Transform block in _blocksTransform)
		{	
			string Material = "";
			if (name.IndexOf ("Basic") >-1 || name.IndexOf ("TNT") >-1　) {
				Material=block.GetComponent<ABBlock> ()._material.ToString();
			}
			else
			{
				Material = "";
			}
			this.FullGameData += (LevelList.Instance.CurrentIndex + 1) + "." + (ABGameWorld._levelTimesTried + 1) + ",";
			this.FullGameData += DataID.ToString ()+ ",";
			this.FullGameData += block.transform.name.Replace ("(Clone)", "") + ",";
			this.FullGameData += Material+",";
			this.FullGameData += block.transform.position.x+",";
			this.FullGameData += block.transform.position.y+",";
			this.FullGameData += block.transform.rotation.z+",";
			this.FullGameData += block.GetComponent<ABGameObject>()._life+",";
			this.FullGameData += ABGameWorld.GameState;

			GameDataList.Add (this.FullGameData);
			this.FullGameData = "";
		}

		this.FullGameData = "";

		foreach (Transform plaftform in _plaftformsTransform)
		{	

			this.FullGameData += (LevelList.Instance.CurrentIndex + 1) + "." + (ABGameWorld._levelTimesTried + 1) + ",";
			this.FullGameData += DataID.ToString ()+ ",";
			this.FullGameData += plaftform.transform.name.Replace ("(Clone)", "") + ",";
			this.FullGameData += ""+",";
			this.FullGameData += plaftform.transform.position.x+",";
			this.FullGameData += plaftform.transform.position.y+",";
			this.FullGameData += plaftform.transform.rotation.z+",";
			this.FullGameData += ""+",";
			this.FullGameData += ABGameWorld.GameState;

			GameDataList.Add (this.FullGameData);
			this.FullGameData = "";
		}
			
		this.FullGameData = "";
		DataID++;
	}

	public void ClearFullGameData()
	{
		this.FullGameData = "";
		this.DataID = 0;
//		if (GameDataList != null) {
//			GameDataList.Clear ();
//		}
//		GameDataList = new List<string> ();
	}

    public string GetFullGameData()
    {
        foreach (string str in GameDataList)
        {
            this.FullGameData += str;
        }

		return this.FullGameData;
	}
}