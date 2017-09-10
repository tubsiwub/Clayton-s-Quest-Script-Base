using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;


// Place ObjInfo onto every saveable object in the scene

//Data path:  C:\Users\(name)\AppData\LocalLow\(CompanyName)\(ProductName) <-- Company and Product are under Player Settings in Unity

[Serializable]
public class SavingLoading : MonoBehaviour
{
	[Tooltip("Check this if you want to wipe all game save data every time the game starts")]
	[SerializeField] bool newGame = true; // permanent new game mode for now.

	string SAVE_PATH { get { return Application.persistentDataPath + "/jam.root"; } }

	// we're going to use this for the main menu, to change the UI options accordingly
	public bool HasSaveFile { get { return Deserialize<SaveContainer>(SAVE_PATH).exists; } }

	// we may have already created the save file, but it could be essentially "blank." make sure it's not.
	bool SaveFileValid { get { return sc.marblesCollected >= 1 || sc.savedCheckpoint != ""; } }

	[Serializable]
	class SaveContainer
	{
		public SaveContainer()
		{
			npcSaved = new List<string>();

			storageKeyDictionary = new Dictionary<string, StorageKey>();
			storedObjectDictionary = new Dictionary<string, StoredObject>();
			questStatusDataDictionary = new Dictionary<string, QuestStatusData>();
			storedCandyDictionary = new Dictionary<string, StoredCandy>();
			origami = OrigamiManager.SetupOrigamiDictionary();
		}

		public bool exists = false;		// imma use this to see if we have a save file or not

		public Dictionary<string, StoredObject> storedObjectDictionary;

		// Game Persistant Storage Variable Key Checks - turns on and off events in game
		public Dictionary<string, StorageKey> storageKeyDictionary;

		public Dictionary<string, QuestStatusData> questStatusDataDictionary;

		public Dictionary<string, StoredCandy> storedCandyDictionary;

		public List<string> npcSaved;

		public List<string> savedScenes;
		public LoadBuddyDoorData buddyDoorData;
		public string savedCheckpoint = "";
		public int marblesCollected = 0;

		public int candyCollectedPickup = 0;
		public int candyCollectedMountain = 0;

		public int lives = HealthManager.totalLives;

		public Dictionary<OrigamiManager.OrigamiType, bool> origami;
		public string playerTexture = "";
		public string playerHat = "";
		public string ballModel = "";
	}
	SaveContainer sc;

	[Serializable]
	struct WeightScaleInfo 
	{
		public string scaleName;
		public List<StoredObject> objList;
	}

	[Serializable]
	struct StorageKey
	{
		public string keyName;
		public bool keyCheck;
	}

	[Serializable]
	public struct StoredObject
	{
		public string objectRefID;      // object name

		public string objectRef;        // file path to Resources folder location of a PREFAB GameObject
		public int presentSceneIndex;

		public float worldPositionX;
		public float worldPositionY;
		public float worldPositionZ;

		public float worldRotationX;
		public float worldRotationY;
		public float worldRotationZ;

		public float worldScaleX;
		public float worldScaleY;
		public float worldScaleZ;

		public bool exists;
	}

	[Serializable]
	public struct QuestStatusData
	{
		public QUEST_STATUS containerStatus;
		public QUEST_STATUS typeStatus;
	}

	[Serializable]
	public struct StoredCandy
	{
		public string name;
		public float positionX;
		public float positionY;
		public float positionZ;
		public bool collected;
		public int sceneIndex;
	}

	public StoredObject nullObject;

	public static SavingLoading instance = null;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		sc = new SaveContainer();

		nullObject = new StoredObject();
		nullObject.objectRefID = "NULL";    // Don't name your scene objects this...
		nullObject.objectRef = "NULL";

		// only ever reset/load game data if we're not on the start scene
		// if we're on the start scene, we'll even ignore the newGame toggle because yolo
		if (SceneManager.GetActiveScene().buildIndex != 0)
		{
			GameObject loader = GameObject.Find("Loader");
			if (loader != null)
				newGame = loader.GetComponent<Loader>().NewGame;

			// if new game, reset all data
			if (newGame)
				ResetAllData();

			// Bring in data relating to game progress
			LoadAllData();
		}

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += (scene, loadingMode) => { SceneLoaded(); };

	}

	void SceneLoaded()
	{
		// On Scene Load...

		// For each stored object...  
		foreach (var obj in sc.storedObjectDictionary)
		{


			// If the object exists within THIS scene... 
			if (sc.storedObjectDictionary[obj.Key].presentSceneIndex == SceneManager.GetActiveScene().buildIndex)
			{


				// If the object exists AT ALL... 
				if (sc.storedObjectDictionary[obj.Key].exists)
				{


					// If the object is not found using the RefID, spawn one
					if (!GameObject.Find(sc.storedObjectDictionary[obj.Key].objectRefID))
					{

						if (sc.storedObjectDictionary [obj.Key].objectRef != null) {
							GameObject newObj = Instantiate<GameObject> (Resources.Load<GameObject> (sc.storedObjectDictionary [obj.Key].objectRef));

							newObj.name = sc.storedObjectDictionary [obj.Key].objectRefID;
							newObj.transform.position = new Vector3 (
								sc.storedObjectDictionary [obj.Key].worldPositionX,
								sc.storedObjectDictionary [obj.Key].worldPositionY,
								sc.storedObjectDictionary [obj.Key].worldPositionZ);
							newObj.transform.rotation = Quaternion.Euler (new Vector3 (
								sc.storedObjectDictionary [obj.Key].worldRotationX,
								sc.storedObjectDictionary [obj.Key].worldRotationY,
								sc.storedObjectDictionary [obj.Key].worldRotationZ));
							newObj.transform.localScale = new Vector3 (
								sc.storedObjectDictionary [obj.Key].worldScaleX,
								sc.storedObjectDictionary [obj.Key].worldScaleY,
								sc.storedObjectDictionary [obj.Key].worldScaleZ);
						}

					}
				}
				else
				{

					// If you find it when it shouldn't exist... delete it.
					if (GameObject.Find(sc.storedObjectDictionary[obj.Key].objectRefID))
					{
						Destroy(GameObject.Find(sc.storedObjectDictionary[obj.Key].objectRefID));
					}

				}
			}
		}
	}

	public void SaveObject
	(
		string objectRefID,         // Object reference as a string (object name) used in dictionary
		int sceneID,                // Which scene is this object meant for?
		string objectRef,           // Object reference as a Gameobject from a Resources location to a prefab
		Vector3 positionRef,        // Object's position (World)
		Quaternion rotationRef,     // Object's rotation value (Quaternion)
		Vector3 scaleRef,           // localScale of object
		bool overwriteFileIfFound,  // Replace data with new data?
		bool exists                 // Is object destroyed or not?
	)
	{

		StoredObject newStruct = new StoredObject();
		newStruct.objectRefID = objectRefID;
		newStruct.presentSceneIndex = sceneID;
		newStruct.objectRef = objectRef;
		newStruct.worldPositionX = positionRef.x;
		newStruct.worldPositionY = positionRef.y;
		newStruct.worldPositionZ = positionRef.z;
		newStruct.worldRotationX = rotationRef.x;
		newStruct.worldRotationY = rotationRef.y;
		newStruct.worldRotationZ = rotationRef.z;
		newStruct.worldScaleX = scaleRef.x;
		newStruct.worldScaleY = scaleRef.y;
		newStruct.worldScaleZ = scaleRef.z;
		newStruct.exists = exists;

		if (!sc.storedObjectDictionary.ContainsKey(objectRefID))
		{

			// Store the data if no data currently exists for this specific object
			sc.storedObjectDictionary.Add(objectRefID, newStruct);

		}
		else
		{

			// If the dictionary already has a reference and data for this object, you can change out the saved data...
			if (overwriteFileIfFound)
			{
				sc.storedObjectDictionary.Remove(objectRefID);
				sc.storedObjectDictionary.Add(objectRefID, newStruct);
			}
			// ...or not.  If you make a mistake, nothing will be overwritten if you prevent that functionality.

		}

	}

	public StoredObject LoadObject(string objectRefID)
	{
		// If the object has stored data, set that data.
		if (sc.storedObjectDictionary.ContainsKey(objectRefID))
		{

			return sc.storedObjectDictionary[objectRefID];

		}
		else
		{
			return nullObject;
		}
	}

	public void RemoveObject(string objectRefID)
	{

		if (sc.storedObjectDictionary.ContainsKey(objectRefID))
		{
			sc.storedObjectDictionary.Remove(objectRefID);
		}

	}

	// Stores or Updates dictionary values for StorageKeys (only lowercase values used)
	public void SaveStorageKey(
		string keyName,
		bool status
	)
	{

		if (sc.storageKeyDictionary.ContainsKey(keyName.ToLower()))
		{   // already have this key?  Update it.

			StorageKey updateKey = new StorageKey();
			updateKey.keyName = sc.storageKeyDictionary[keyName.ToLower()].keyName;
			updateKey.keyCheck = status;
			sc.storageKeyDictionary[keyName.ToLower()] = updateKey;

		}
		else
		{   // Don't have the key?  Add a new entry with given values

			StorageKey newKey = new StorageKey();
			newKey.keyName = keyName.ToLower();
			newKey.keyCheck = status;
			sc.storageKeyDictionary.Add(keyName.ToLower(), newKey);

		}

	}

	// Returns the status of the given storage key
	public bool CheckStorageKeyStatus(string keyName)
	{
		if (sc.storageKeyDictionary.ContainsKey(keyName.ToLower()))
			return sc.storageKeyDictionary[keyName.ToLower()].keyCheck;

		return false;
	}

	// Returns true if the key exists in the dictionary, regardless of status
	public bool CheckStorageKeyExist(string keyName)
	{

		if (sc.storageKeyDictionary.ContainsKey(keyName.ToLower()))
			return true;

		return false;
	}

	float elapsedTime = 0;
	void Update()
	{
		elapsedTime += Time.deltaTime;

		CHEATS();
	}

	int spawnChestCheatcode = 0;
	void CHEATS()
	{

		#region SPAWN CHEST CHEAT

		if (Input.anyKeyDown)
		{
			if (!Input.GetKeyDown(KeyCode.C) || !Input.GetKeyDown(KeyCode.H) || !Input.GetKeyDown(KeyCode.E) || !Input.GetKeyDown(KeyCode.S) || !Input.GetKeyDown(KeyCode.T))
			{
				spawnChestCheatcode = 0;
			}
		}

		if (Input.GetKeyDown(KeyCode.C))
		{
			spawnChestCheatcode = 1;
		}
		if (Input.GetKeyDown(KeyCode.H) && spawnChestCheatcode == 1)
		{
			spawnChestCheatcode = 2;
		}
		if (Input.GetKeyDown(KeyCode.E) && spawnChestCheatcode == 2)
		{
			spawnChestCheatcode = 3;
		}
		if (Input.GetKeyDown(KeyCode.S) && spawnChestCheatcode == 3)
		{
			spawnChestCheatcode = 4;
		}
		if (Input.GetKeyDown(KeyCode.T) && spawnChestCheatcode == 4)
		{

			GameObject newObj;
			newObj = (GameObject)Instantiate(Resources.Load("TreasureChest", typeof(GameObject)), GameObject.FindWithTag("Player").transform.position + new Vector3(0, 4, 0), Quaternion.identity);

			newObj.name = "CheatChest" + elapsedTime.ToString();

			spawnChestCheatcode = 0;
		}
		#endregion

	}

	void Serialize(string path, object obj)
	{
		BinaryFormatter binFormat = new BinaryFormatter();
		FileStream fileStr = File.Create(path);

		binFormat.Serialize(fileStr, obj);
		fileStr.Close();
	}

	T Deserialize<T>(string path)
	{
		T obj = default(T);

		if (File.Exists(path))
		{
			BinaryFormatter binFormat = new BinaryFormatter();
			FileStream fileStr = File.Open(path, FileMode.Open);

			obj = (T)binFormat.Deserialize(fileStr);
			fileStr.Close();

		}
		else
			Debug.Log("File does not exist at " + path);

		return obj;
	}

	public void SaveData()
	{
		// never save anything while we're on the start scene
		if (SceneManager.GetActiveScene().buildIndex == 0)
			return;

		if (SaveFileValid)
			sc.exists = true;

		Serialize(SAVE_PATH, sc);

		//print("Data Saved");
	}

	// to-do: this needs to call all/most of the save functions, so we can save ALL of this on application quit
	public void SaveAllData()
	{
		SaveSceneInfo();
		SaveCheckpoint();
		SaveData();
	}

	public void LoadAllData()
	{
		sc = Deserialize<SaveContainer>(SAVE_PATH);

		LoadStoredObjectData();
		LoadStorageKeyData();
		LoadSceneInfo();
		LoadCheckpoint();

	}

	public void LoadStoredObjectData()
	{
		Dictionary<string, StoredObject> newDict = Deserialize<SaveContainer>(SAVE_PATH).storedObjectDictionary;

		if (newDict != null)
			sc.storedObjectDictionary = newDict;
	}

	public void LoadStorageKeyData()
	{
		// Bring in data and store into array
		Dictionary<string, StorageKey> newDict = Deserialize<SaveContainer>(SAVE_PATH).storageKeyDictionary;

		if (newDict != null)
			sc.storageKeyDictionary = newDict;
	}

	public void SaveSceneInfo()
	{
		int sceneNum = SceneManager.sceneCount;
		sc.savedScenes = new List<string>();

		bool hasSceneBase = false;
		for (int i = 0; i < sceneNum; i++)
		{
			sc.savedScenes.Add(SceneManager.GetSceneAt(i).name);
			if (sc.savedScenes[i] == "SceneBase")
				hasSceneBase = true;
		}

		if (!hasSceneBase)
		{
			GameObject door = GameObject.Find("LoadBuddyDoor");
			LoadBuddyDoorData obj = null;

			if (door != null)
				obj = door.GetComponent<LoadBuddyDoor>().GetData();

			if (obj != null)
			{
				sc.buddyDoorData = obj;
				LevelManager.instance.NewPlayerData();
			}
		}
	}

	public string GetLastLoadedScene()
	{
		// Bring in data and store into array
		List<string> scenes = Deserialize<SaveContainer>(SAVE_PATH).savedScenes;

		if (scenes != null && scenes.Count >= 1)
		{
			sc.savedScenes = scenes;
			return sc.savedScenes[sc.savedScenes.Count-1];
		}
		else return "";
	}

	public void LoadSceneInfo()
	{
		// Bring in data and store into array
		List<string> scenes = Deserialize<SaveContainer>(SAVE_PATH).savedScenes;

		if (scenes != null && scenes.Count >= 1)
		{
			sc.savedScenes = scenes;

			bool hasSceneBase = false;
			for (int i = 0; i < sc.savedScenes.Count; i++)
			{
				if (sc.savedScenes[i] == "SceneBase")
					hasSceneBase = true;
			}

			if (hasSceneBase)
			{
				LoadBuddyDoorData data = Deserialize<SaveContainer>(SAVE_PATH).buddyDoorData;
				if (data != null && data.HasData)   // clear out BuddyDoorData (if it exists)
					sc.buddyDoorData = new LoadBuddyDoorData();

				// create persistent LoadBuddy object, then load (SINGLE) SceneBase, to load all from there
				GameObject obj = new GameObject();
				obj.AddComponent<LoadBuddy>();
				obj.name = "LoadBuddy";
				obj.GetComponent<LoadBuddy>().Init(sc.savedScenes);

				SceneManager.LoadSceneAsync("SceneBase", LoadSceneMode.Single);
			}
			else
			{
				if (!GameObject.Find("LoadBuddyDoor"))
				{
					LoadBuddyDoorData data = Deserialize<SaveContainer>(SAVE_PATH).buddyDoorData;

					if (data != null)
					{
						sc.buddyDoorData = data;
						GameObject obj = new GameObject();
						obj.AddComponent<LoadBuddyDoor>();
						obj.name = "LoadBuddyDoor";
						obj.GetComponent<LoadBuddyDoor>().SetData(sc.buddyDoorData);
					}
				}

				SceneManager.LoadSceneAsync(sc.savedScenes[0], LoadSceneMode.Single);
			}
		}
	}

	public void SaveNPC(string name)
	{
		sc.npcSaved.Add (name);
	}

	public int LoadNPC_String(string name)
	{
		if (sc.npcSaved.Contains (name)) 
		{
			return sc.npcSaved.IndexOf (name);
		}
		return -1;
	}

	public string LoadNPC_Index(int index)
	{
		if (sc.npcSaved[index] != null) 
		{
			return sc.npcSaved [index];
		}
		return "";
	}

	public int GetNPCCount()
	{
		return sc.npcSaved.Count;
	}

	public void SaveCheckpoint()
	{
		Checkpoint checkpoint = GameObject.Find("Player").GetComponent<PlayerHandler>().LastCheckpoint;

		if (checkpoint != null)
			sc.savedCheckpoint = checkpoint.gameObject.name;
	}

	public void LoadCheckpoint()
	{
		string data = Deserialize<SaveContainer>(SAVE_PATH).savedCheckpoint;

		if (data != null && data != "")
		{
			sc.savedCheckpoint = data;
			GameObject.FindWithTag("Player").GetComponent<PlayerHandler>().FindAndSetCheckpoint(sc.savedCheckpoint);
		}
	}

	public void SaveMarbles(int collected)
	{
		sc.marblesCollected = collected;
	}

	public void LoadMarbles()
	{
		if (MarbleManager.instance)
		{
			int amount = Deserialize<SaveContainer>(SAVE_PATH).marblesCollected;
			MarbleManager.instance.SetMarbles(amount);
		}
	}

	public void SaveOrigami(Dictionary<OrigamiManager.OrigamiType, bool> origami)
	{
		sc.origami = origami;
	}

	public void LoadOrigami()
	{
		if (OrigamiManager.instance)
		{
			Dictionary<OrigamiManager.OrigamiType, bool> origami = Deserialize<SaveContainer>(SAVE_PATH).origami;
			OrigamiManager.instance.SetOrigamiDirect(origami);
		}
	}

	public void SavePlayerTexture(string textureName)
	{
		sc.playerTexture = textureName;
	}

	public void LoadPlayerTexture()
	{
		GameObject player = GameObject.FindWithTag("Player");
		if (player != null)
		{
			string texture = Deserialize<SaveContainer>(SAVE_PATH).playerTexture;

			if (texture != "")
				player.GetComponent<PlayerCustomizer>().ChangeTexture(texture);
		}
	}

	public void SavePlayerHat(string hatName)
	{
		sc.playerHat = hatName;
	}

	public void LoadPlayerHat()
	{
		GameObject player = GameObject.FindWithTag("Player");
		if (player != null)
		{
			string hatName = Deserialize<SaveContainer>(SAVE_PATH).playerHat;
		
			if (hatName != "")
				player.GetComponent<PlayerCustomizer>().ChangeHat(hatName);
		}
	}

	public void SaveBallModel(string ballModel)
	{
		sc.ballModel = ballModel;
	}

	public void LoadBallModel()
	{
		GameObject player = GameObject.FindWithTag("Player");
		if (player != null)
		{
			string ballModel = Deserialize<SaveContainer>(SAVE_PATH).ballModel;

			if (ballModel != "")
				player.GetComponent<PlayerCustomizer>().ChangeBallModel(ballModel);
		}
	}

	// Total / Amount / Number - not individual
	public void SaveCandy(int collected)
	{
		if (SceneManager.GetActiveScene().name == "Pickup Zone")
			sc.candyCollectedPickup = collected;
		else if (SceneManager.GetActiveScene().name == "Mountain Zone")
			sc.candyCollectedMountain = collected;
	}

	public void SaveStoredCandy(GameObject candyObject, bool collectValue)
	{
		if (!sc.storedCandyDictionary.ContainsKey (candyObject.name)) 
		{
			StoredCandy newCandy = new StoredCandy ();

			newCandy.collected = collectValue;
			newCandy.name = candyObject.name;
			newCandy.positionX = candyObject.transform.position.x;
			newCandy.positionY = candyObject.transform.position.y;
			newCandy.positionZ = candyObject.transform.position.z;
			newCandy.sceneIndex = SceneManager.GetActiveScene ().buildIndex;

			sc.storedCandyDictionary.Add (candyObject.name, newCandy);

			//print ("Saving candy new.");
		} 
		else 
		{
			StoredCandy newCandy = new StoredCandy ();

			newCandy = sc.storedCandyDictionary [candyObject.name];
			newCandy.collected = collectValue;
			sc.storedCandyDictionary [candyObject.name] = newCandy;

			//print ("Saving candy override.");
		}

	}

	public bool LoadStoredCandy(string candyName){

		if (sc.storedCandyDictionary.ContainsKey (candyName)) 
		{
			if (!sc.storedCandyDictionary [candyName].collected) 
			{
				if (sc.storedCandyDictionary [candyName].sceneIndex == SceneManager.GetActiveScene ().buildIndex) {

//					GameObject newCandy = Instantiate(Resources.Load<GameObject> ("Candy"));
//					newCandy.GetComponent<ObjInfo> ().assignRandomName = false;
//					newCandy.name = sc.storedCandyDictionary [candyName].name;
//					newCandy.transform.position = new Vector3 (
//						sc.storedCandyDictionary [candyName].positionX, 
//						sc.storedCandyDictionary [candyName].positionY, 
//						sc.storedCandyDictionary [candyName].positionZ);
//
//					newCandy.transform.SetParent (GameObject.Find ("CandyContainer").transform);

					//print ("Loaded a saved candy");

					return true;
				} 
				else 
				{
					//print ("Wrong scene ID.");
				}
			} 
			else 
			{
				//print ("Loaded candy has been collected.");

				// We do not load a candy when returning
				return false;
			}
		} 
		else 
		{
			//print ("Couldn't find candy name.");
		}

		return true;
	}

	public void ResetStoredCandy(int sceneIndex)
	{
		//print ("Reset Candy was called.");

		List<string> dicKeys = new List<string> (sc.storedCandyDictionary.Keys);

		foreach(string storedCandyKey in dicKeys)
		{
			if (sc.storedCandyDictionary[storedCandyKey].sceneIndex == sceneIndex) 
			{
				StoredCandy newCandy = sc.storedCandyDictionary[storedCandyKey];
				newCandy.collected = false;
				sc.storedCandyDictionary[storedCandyKey] = newCandy;
			}
		}
	}

	public int GetLoadCandy()
	{
		int amount = 0;

		if (SceneManager.GetActiveScene().name == "Pickup Zone")
			amount = Deserialize<SaveContainer>(SAVE_PATH).candyCollectedPickup;
		else if (SceneManager.GetActiveScene().name == "Mountain Zone")
			amount = Deserialize<SaveContainer>(SAVE_PATH).candyCollectedMountain;

		return amount;
	}

	public void SaveLives(int lives)
	{
		sc.lives = lives;
	}

	public void LoadLives()
	{
		if (HealthManager.instance)
		{
			int lives = Deserialize<SaveContainer>(SAVE_PATH).lives;
			HealthManager.instance.SetLives(lives);
		}
	}

	public void SaveQuestStatus(QUEST_STATUS container, QUEST_STATUS type, string storageKey)
	{
		QuestStatusData qsd = new QuestStatusData ();
		qsd.containerStatus = container;
		qsd.typeStatus = type;

		if (!sc.questStatusDataDictionary.ContainsKey (storageKey)) // add if new
		{
			sc.questStatusDataDictionary.Add (storageKey, qsd);
		} 
		else 														// update if exists
		{
			sc.questStatusDataDictionary.Remove (storageKey);
			sc.questStatusDataDictionary.Add (storageKey, qsd);
		}

		SaveAllData ();
	}

	public QUEST_STATUS LoadQuestStatus_Container(string storageKey)
	{
		if (sc.questStatusDataDictionary.ContainsKey (storageKey)) 
		{
			return sc.questStatusDataDictionary [storageKey].containerStatus;
		}
		else 
		{
			return QUEST_STATUS.NEUTRAL;
		}
	}

	public QUEST_STATUS LoadQuestStatus_Type(string storageKey)
	{
		if (sc.questStatusDataDictionary.ContainsKey (storageKey)) 
		{
			return sc.questStatusDataDictionary [storageKey].typeStatus;
		}
		else 
		{
			return QUEST_STATUS.NEUTRAL;
		}
	}

	public void ResetAllData()
	{
		sc = new SaveContainer();
		Serialize(SAVE_PATH, sc);

		//print("Data Wiped");
	}
}
