using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	public static LevelManager instance = null;

	public static string SCENE_BASE = "SceneBase";
	public static string START_SCENE = "StartScene";

	string firstLoadedScene = "";
	bool[] hasDuplicateObject;

	public bool SceneBaseLoaded { get { return SceneManager.GetSceneByName(SCENE_BASE).isLoaded; } }
	public bool SceneBaseActive { get { return SceneManager.GetActiveScene().name == SCENE_BASE; } }

	Transform playerStartPoint;
	List<Transform> playerJumpPoints;
	int currentPoint = 0;
	bool setPlayerStartMainZone = false;

	List<GameObject> storedLights;

	List<SceneLoader> sceneLoaders;

	GameObject openAnimaticRef;

	string newPlayerDataScene;
	public void NewPlayerData() { newPlayerDataScene = SceneManager.GetActiveScene().name; }
	public void CancelPlayerData() { newPlayerDataScene = "CANCEL"; }

	public delegate void SceneAction();
	public event SceneAction DoneLoading;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);

		playerJumpPoints = new List<Transform>();
		hasDuplicateObject = new bool[EssentialObjects.Objects.Length];
		newPlayerDataScene = SavingLoading.instance.GetLastLoadedScene();
		storedLights = new List<GameObject>();
		sceneLoaders = new List<SceneLoader>();

		SceneManager.sceneLoaded += SceneLoaded;
		SceneManager.sceneUnloaded += SceneUnloaded;
	}

	public void CreateBuddy()
	{
		if (SceneBaseLoaded) return;

		GameObject obj = new GameObject();
		obj.AddComponent<LoadBuddy>();
		obj.name = "LoadBuddy";
		obj.GetComponent<LoadBuddy>().Init(SceneManager.GetActiveScene().name);

		SceneManager.LoadSceneAsync("SceneBase", LoadSceneMode.Single);
	}

	public void LoadFromBuddy()
	{
		GameObject obj = GameObject.Find("LoadBuddy");
		string[] scenes = obj.GetComponent<LoadBuddy>().ScenesToLoad;
		Destroy(obj);

		Load(scenes);
	}

	void SceneLoaded(Scene scene, LoadSceneMode loadingMode)
	{
		SetFirstLoadedScene(scene);

		if (loadingMode == LoadSceneMode.Additive)
		{
			GameObject[] allObjects = FindObjectsOfType<GameObject>();

			if (scene.name != SCENE_BASE && SceneManager.GetActiveScene() != scene)
				StartCoroutine(SetActiveScene(scene, allObjects));

			MarkDuplicateObjects(allObjects);
			DisableDuplicates(allObjects);
		} // not loading from SceneBase, not loading additively (simply starting scene)
		else if (!SceneBaseActive)
			StartCoroutine(UnfreezeBud(scene));

		SetPlayerJumpPoints(true);

		// new data check here
		if (scene.name == newPlayerDataScene)
		{
			SavingLoading.instance.LoadCheckpoint();
			StartCoroutine(WaitThenRespawnToCP());
		}
		else if (newPlayerDataScene == "CANCEL") // if we just marked CancelPlayerData,
			NewPlayerData();					// make sure we set it back again, so we can load position

		if (DoneLoading != null)
			DoneLoading();
	}

	void SceneUnloaded(Scene scene)
	{
		SetPlayerJumpPoints(false);
		ResetSceneLoaderList();

		if (SceneManager.GetActiveScene().name == SCENE_BASE)
			StartCoroutine(WaitThenRecoverActive());
	}

	IEnumerator WaitThenRecoverActive()
	{
		yield return null;

		int sceneToEnable;
		for (sceneToEnable = SceneManager.sceneCount-1; sceneToEnable >= 0; sceneToEnable--)
		{
			Scene scene = SceneManager.GetSceneAt(sceneToEnable);
			if (scene.name != SCENE_BASE)
				break;
		}

		SceneManager.SetActiveScene(SceneManager.GetSceneAt(sceneToEnable));
		RecoverExtraObjects(storedLights, SceneManager.GetSceneAt(sceneToEnable).name);
	}

	void SetFirstLoadedScene(Scene scene)
	{
		// set first loaded name to SceneBase, even if not loaded first
		if (firstLoadedScene == "" || scene.name == SCENE_BASE)
			firstLoadedScene = scene.name;
	}

	void SetPlayerJumpPoints(bool setCheckpointIfNone)
	{
		SetPlayerStart(setCheckpointIfNone);

		playerJumpPoints.Clear();

		if (playerStartPoint != null)
			playerJumpPoints.Add(playerStartPoint); // ensure PlayerStart is first in Jump list

		GameObject[] points = GameObject.FindGameObjectsWithTag("PlayerJump");
		for (int i = 0; i < points.Length; i++)
			playerJumpPoints.Add(points[i].transform);
	}

	void SetPlayerStart(bool setCheckpointIfNone)
	{
		GameObject[] starts = GameObject.FindGameObjectsWithTag("PlayerStart");

		GameObject player = GameObject.FindWithTag("Player");
		if (starts.Length == 1 && playerStartPoint == null)
		{
			PlayerHandler playerHandler = player.GetComponent<PlayerHandler>();
			if (setPlayerStartMainZone && !SceneBaseLoaded &&   // if we've already set the player start in main scene
			playerHandler.LastCheckpoint == null && !playerHandler.HasRespawnPos) // but player has no checkpoint...
				setPlayerStartMainZone = false;		// give us another chance to set it!

			if ((SceneBaseLoaded && !setPlayerStartMainZone) || !SceneBaseLoaded)
			{
				if (SceneBaseLoaded && !setPlayerStartMainZone)
					setPlayerStartMainZone = true;

				playerStartPoint = starts[0].transform;
				currentPoint = 0;

				if (player != null)
				{
					player.transform.position = playerStartPoint.position;
					Vector3 rot = playerStartPoint.forward; rot.y = 0;
					playerHandler.RotateMesh.rotation = Quaternion.LookRotation(rot);
					Camera.main.GetComponent<CameraControlDeluxe>().DoCamBehindPlayer();
				}
			}
		}

		if (setCheckpointIfNone)
			StartCoroutine(SetCheckpointIfNone());
	}

	// this needs to happen after all data is loaded, so loaded data doens't get overwritten with brand-new save file
	IEnumerator SetCheckpointIfNone()
	{
		yield return null;

		GameObject player = GameObject.FindWithTag("Player");

		if (player != null)
		{
			// only set starting position as checkpoint if we can't find another one...
			PlayerHandler playerHandler = player.GetComponent<PlayerHandler>();

			if (playerHandler.LastCheckpoint == null && !playerHandler.HasRespawnPos && !SceneBaseActive)
			{
				Transform start = (playerStartPoint != null) ? playerStartPoint : player.transform;
				playerHandler.SetCheckpoint(start.position, start.rotation);
			}
		}
	}

	IEnumerator SetActiveScene(Scene scene, GameObject[] allObjects)
	{
		yield return null;  // we have to wait 1 frame because unity is a dumb boy

		if (scene.IsValid())    // after a frame, we may have unloaded this scene... check first
		{
			SceneManager.SetActiveScene(scene);
			DisableExtraObjects("directional light", storedLights, allObjects);

			ScreenTransitionBackward();

			if (scene.name != START_SCENE)
				GameObject.FindWithTag("Player").GetComponent<PlayerHandler>().SetFrozen(false, false);

			if (scene.name == "Tutorial Zone" && !SavingLoading.instance.HasSaveFile)
			{
				openAnimaticRef = Instantiate(Resources.Load<GameObject>("OpeningAnimatic"));

				yield return null;	// wait for code that may RESPAWN (then unfreeze) the player. ugh.

				GameObject.FindWithTag("Player").GetComponent<PlayerHandler>().SetFrozen(true, true);
				Camera.main.GetComponent<CameraControlDeluxe>().SetFreeze(true, false);
			}
		}
	}

	IEnumerator UnfreezeBud(Scene scene)
	{
		yield return null;

		// after a frame, we may have unloaded this scene... check first
		if (scene.IsValid() && scene.name != START_SCENE)
			GameObject.FindWithTag("Player").GetComponent<PlayerHandler>().SetFrozen(false, false);

		ScreenTransitionBackward();
	}

	void ScreenTransitionBackward()
	{
		ScreenTransition sc = Camera.main.GetComponent<ScreenTransition>();
		if (!SceneBaseActive && sc.IsBlack)
			sc.Backward(1, "circle_pattern");
	}

	IEnumerator WaitThenRespawnToCP()
	{
		yield return null;

		PlayerHandler playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();

		if (playerHandler.HasRespawnPos && openAnimaticRef == null)
			playerHandler.Respawn();
	}

	void MarkDuplicateObjects(GameObject[] allObjects)
	{
		for (int i = 0; i < allObjects.Length; i++)
		{
			if (allObjects[i].scene.name != firstLoadedScene)
				continue;

			for (int j = 0; j < EssentialObjects.Objects.Length; j++)
			{
				if (allObjects[i].name == EssentialObjects.Objects[j])
					hasDuplicateObject[j] = true;
			}
		}
	}

	void DisableDuplicates(GameObject[] allObjects)
	{
		for (int i = 0; i < allObjects.Length; i++)
		{
			if (allObjects[i].scene.name == firstLoadedScene)
				continue;

			for (int j = 0; j < EssentialObjects.Objects.Length; j++)
			{
				if (EssentialObjects.Objects[j] == "Loader" || !hasDuplicateObject[j])
					continue;

				if (allObjects[i].name == EssentialObjects.Objects[j])
				{
					//print("Disabled " + allObjects[i].name + " from " + allObjects[i].scene.name + " scene.");
					allObjects[i].SetActive(false);
				}
			}
		}
	}

	void DisableExtraObjects(string objName, List<GameObject> storedList, GameObject[] allObjects)
	{
		if (allObjects == null || allObjects.Length < 1) return;

		for (int i = 0; i < allObjects.Length; i++)
		{
			if (allObjects[i] == null)
				continue;

			if (allObjects[i].scene == SceneManager.GetActiveScene())
				continue;

			if (allObjects[i].name.ToLower().Contains(objName))
			{
				storedList.Add(allObjects[i]);
				allObjects[i].SetActive(false);
			}
		}
	}

	void RecoverExtraObjects(List<GameObject> storedList, string sceneName)
	{
		for (int i = storedList.Count-1; i >= 0; i--)
		{
			if (storedList[i] == null)
			{
				storedList.Remove(storedList[i]);
				continue;
			}

			if (storedList[i].scene.name == sceneName)
			{
				storedList[i].SetActive(true);
				storedList.Remove(storedList[i]);
				continue;
			}
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F9) && playerJumpPoints.Count > 0)
		{
			GameObject.FindWithTag("Player").transform.position = playerJumpPoints[currentPoint].position;

			currentPoint++;
			if (currentPoint > playerJumpPoints.Count - 1)
				currentPoint = 0;
		}
	}

	// tells you if scene is loaded, is is *currently* loading (unlike scene.isLoaded)
	bool SceneInList(string sceneName)
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			if (SceneManager.GetSceneAt(i).name == sceneName)
				return true;
		}

		return false;
	}

	public bool Load(string scene)
	{
		bool loadedAnything = false;

		if (scene != "" && !SceneInList(scene))		// use instead of scene.isLoaded, so we can check against currently loading scenes
		{
			SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
			loadedAnything = true;
		}

		return loadedAnything;
	}

	public bool Load(string[] scenes)
	{
		bool loadedAnything = false;

		for (int i = 0; i < scenes.Length; i++)
		{
			bool result = Load(scenes[i]);
			if (result) loadedAnything = true;
		}

		return loadedAnything;
	}

	public void Unload(string scene)
	{
		if (scene != "" && SceneManager.GetSceneByName(scene).isLoaded)
			SceneManager.UnloadSceneAsync(scene);
	}

	public void Unload(string[] scenes)
	{
		for (int i = 0; i < scenes.Length; i++)
			Unload(scenes[i]);
	}

	public void UnloadOthers(string[] keeps)
	{
		int sceneNum = SceneManager.sceneCount;

		for (int i = 0; i < sceneNum; i++)
		{
			string scene = SceneManager.GetSceneAt(i).name;

			if (scene == SCENE_BASE)
				continue;

			bool keep = false;
			for (int j = 0; j < keeps.Length; j++)
			{
				if (scene == keeps[j])
					keep = true;
			}

			if (!keep) Unload(scene);
		}
	}

	public void AddSceneLoader(SceneLoader sceneLoader)
	{
		if (!sceneLoaders.Contains(sceneLoader))
			sceneLoaders.Add(sceneLoader);
	}

	public void ResetSceneLoaderList()
	{
		for (int i = sceneLoaders.Count-1; i >= 0; i--)
		{
			if (sceneLoaders[i] == null)
				sceneLoaders.RemoveAt(i);
		}
	}

	public bool CanLoadHub()
	{
		string checkScene = SceneManager.GetActiveScene().name;

		GameObject loadBuddyDoor = GameObject.Find("LoadBuddyDoor");
		if (loadBuddyDoor != null)
			checkScene = loadBuddyDoor.GetComponent<LoadBuddyDoor>().GetLastActiveScene;

		switch (checkScene)
		{
			case "Pickup Zone": return true;
			case "Mountain Zone": return true;
			
			case "Tutorial Zone": return false;
			case "Level Shell": return false;

			default: return false;
		}
	}

	public void LoadHub()
	{
		GameObject loadBuddyDoor = GameObject.Find("LoadBuddyDoor");
		if (loadBuddyDoor != null && !SceneBaseLoaded)
		{
			loadBuddyDoor.GetComponent<LoadBuddyDoor>().Load(true);
			return;
		}

		SceneLoader.LoaderID loaderID = SceneLoader.LoaderID.None;
		switch (SceneManager.GetActiveScene().name)
		{
			case "Tutorial Zone":
				loaderID = SceneLoader.LoaderID.Tutorial;
			break;

			case "Pickup Zone":
				loaderID = SceneLoader.LoaderID.Pickup;
			break;

			case "Mountain Zone":
				loaderID = SceneLoader.LoaderID.Mountain;
			break;
		}

		LoadHubFromSceneLoader(loaderID);
	}

	public void LoadHubFromSceneLoader(SceneLoader.LoaderID loaderID)
	{
		ResetSceneLoaderList();

		for (int i = 0; i < sceneLoaders.Count; i++)
		{
			if (sceneLoaders[i].GetID == loaderID)
			{
				sceneLoaders[i].BeginSceneLoad(true);
				return;
			}
		}

		Debug.LogError("couldn't find anything sorry");
	}
}



// This may be a really useful function for something else, but it's real costly, so... yeah
/*GameObject[] FindGameObjects(string name)
{
	GameObject[] allObjects = FindObjectsOfType<GameObject>();
	List<int> intList = new List<int>();

	for (int i = 0; i < allObjects.Length; i++)
	{
		if (string.Compare(allObjects[i].name, name) == 0)
			intList.Add(i);
	}

	GameObject[] objectsWithName = new GameObject[intList.Count];
	for (int i = 0; i < intList.Count; i++)
		objectsWithName[i] = allObjects[intList[i]];

	return objectsWithName;
}*/
