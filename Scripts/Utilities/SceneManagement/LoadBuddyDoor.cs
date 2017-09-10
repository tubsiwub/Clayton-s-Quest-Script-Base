using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LoadBuddyDoorData
{
	public LoadBuddyDoorData()
	{
		lastActiveScene = "";
	}

	public bool HasData { get { return lastActiveScene != ""; } }

	public float respawnX;
	public float respawnY;
	public float respawnZ;

	public string[] reloadScenes;
	public string lastActiveScene;
	public bool activeOnLoad = false;

	public int reloaded = 0;
}

public class LoadBuddyDoor : MonoBehaviour
{
	Vector3 respawnPos;
	string[] reloadScenes;
	string lastActiveScene;
	bool activeOnLoad = false;

	public bool ActiveOnLoad { get { return activeOnLoad; } }
	public string GetLastActiveScene { get { return lastActiveScene; } }

	int reloaded = 0;
	int doneTransitionStamp = 0;
	bool loadHubOnCompletion = false;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
		LevelManager.instance.DoneLoading += DoneLoading;
		ScreenTransition.OnDoneForward += DoneTransition;
	}

	public void Init(Vector3 respawnPos)
	{
		this.respawnPos = respawnPos;

		int sceneNum = SceneManager.sceneCount;
		reloadScenes = new string[sceneNum];

		for (int i = 0; i < sceneNum; i++)
			reloadScenes[i] = SceneManager.GetSceneAt(i).name;

		lastActiveScene = SceneManager.GetActiveScene().name;
	}

	public LoadBuddyDoorData GetData()
	{
		LoadBuddyDoorData data = new LoadBuddyDoorData();
		data.respawnX = respawnPos.x;
		data.respawnY = respawnPos.y;
		data.respawnZ = respawnPos.z;

		data.reloadScenes = reloadScenes;
		data.lastActiveScene = lastActiveScene;
		data.activeOnLoad = activeOnLoad;
		data.reloaded = reloaded;

		return data;
	}

	public void SetData(LoadBuddyDoorData data)
	{
		respawnPos.x = data.respawnX;
		respawnPos.y = data.respawnY;
		respawnPos.z = data.respawnZ;

		reloadScenes = data.reloadScenes;
		lastActiveScene = data.lastActiveScene;
		activeOnLoad = data.activeOnLoad;
		reloaded = data.reloaded;
	}

	public void Load(bool loadHubOnCompletion = false)
	{
		GameObject.FindWithTag("Player").GetComponent<PlayerHandler>().SetFrozen(true, true);

		this.loadHubOnCompletion = loadHubOnCompletion;
		Camera.main.GetComponent<ScreenTransition>().Forward(2, "circle_pattern");
		StartCoroutine("ReallyLoadAfterTransition");
	}

	void DoneTransition()
	{
		doneTransitionStamp = Time.frameCount;
	}

	IEnumerator ReallyLoadAfterTransition()
	{
		while (doneTransitionStamp != Time.frameCount)
		{
			yield return null;
		}

		activeOnLoad = true;

		SceneManager.LoadSceneAsync(reloadScenes[0], LoadSceneMode.Single);     // load SINGLE to unload last scene (ball door)

		for (int i = 1; i < reloadScenes.Length; i++)
			SceneManager.LoadSceneAsync(reloadScenes[i], LoadSceneMode.Additive);
	}

	void DoneLoading()
	{
		if (activeOnLoad)
		{
			GameObject player = GameObject.FindWithTag("Player");
			if (player != null) player.transform.position = respawnPos;

			int sceneNum = SceneManager.sceneCount;
			for (int i = 0; i < sceneNum; i++)
			{
				if (SceneManager.GetSceneAt(i).name == lastActiveScene)      // set active scene anyway, even though LevelManager will overwrite it
					SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));
			}

			reloaded++;
			if (reloaded >= reloadScenes.Length)
			{
				SavingLoading.instance.LoadCheckpoint();

				if (loadHubOnCompletion)
					LevelManager.instance.LoadHub();

				activeOnLoad = false;
				loadHubOnCompletion = false;
				
				Destroy(gameObject);
			}
		}
	}
}
