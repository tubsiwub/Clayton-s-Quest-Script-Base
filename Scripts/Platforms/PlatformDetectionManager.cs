using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformDetectionManager : MonoBehaviour {

	GameObject[] movingPlatforms;

	GameObject playerObj;

	public static PlatformDetectionManager instance = null;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += (scene, loadingMode) => { SceneLoaded(); };
	}

	void SceneLoaded()
	{

	}

	void Start () {

		playerObj = GameObject.FindWithTag ("Player");

	}
		
	// Call this with a given platform to remove player from any other storages
	public void RunPlatformCheck(GameObject currentPlatform) {

		movingPlatforms = GameObject.FindGameObjectsWithTag ("MovingPlatform");

		for (int i = 0; i < movingPlatforms.Length; i++) {
			
			if (movingPlatforms [i] != currentPlatform) {

				if (movingPlatforms [i].GetComponent<Platform_Movement> ().CollidingObjects.Contains(playerObj))
					movingPlatforms [i].GetComponent<Platform_Movement> ().CollidingObjects.Remove (playerObj);

			}

		}

	}
}
