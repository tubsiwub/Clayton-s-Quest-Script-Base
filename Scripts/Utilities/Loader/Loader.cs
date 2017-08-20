using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
	[SerializeField] GameObject levelSwitch;	
	[SerializeField] GameObject saveLoad;
	[SerializeField] GameObject marbleManager;
	[SerializeField] GameObject healthManager;
	[SerializeField] GameObject soundManager;
	[SerializeField] GameObject platformManager;
	[SerializeField] GameObject levelManager;
	[SerializeField] GameObject butterflyManager;
	[SerializeField] GameObject npcManager;
	[SerializeField] GameObject menu;
	[SerializeField] GameObject fpsDisplay;
	[SerializeField] GameObject origamiManager;
	[SerializeField] GameObject distanceCuller;
	[SerializeField] GameObject analyticsManager;

	bool spawnButterflies = true;       // [SerializeField] 
	bool enablePauseMenu = true;        // [SerializeField] 
	bool startFromSceneBase = false;        // [SerializeField] 
	[SerializeField] bool newGame = true;
	public bool NewGame { get { return newGame; } }
	[SerializeField] float cullerDistFromPlayer = 60;

	[SerializeField] string[] scenesToLoadFirst;

	void Awake()
	{
		CreateManagers();

		// always set the new dist for each new Loader loaded in
		DistanceCuller.instance.SetDistFromPlayer(cullerDistFromPlayer);
	}

	void Start()
	{
		HandleStartup();
	}

	void HandleStartup()
	{
		if (GameObject.Find("LoadBuddyDoor") && !GameObject.Find("LoadBuddy"))
			return;

		if (startFromSceneBase)
		{
			LevelManager.instance.CreateBuddy();
		}
		else if (GameObject.Find("LoadBuddy"))
		{
			//print("load from buddy: " + FindGameObjects("LoadBuddy").Length);
			LevelManager.instance.LoadFromBuddy();
		}
		else if (SceneManager.sceneCount <= 1 && SceneManager.GetActiveScene().name == "SceneBase")
		{
			//print("load first scenes (loader)");
			LevelManager.instance.Load(scenesToLoadFirst);
		}
	}

	void CreateManagers()
	{
		//if (LevelSwitch.instance == null)
		//	Instantiate(levelSwitch);

		if (SavingLoading.instance == null)
			Instantiate(saveLoad);

		if (MarbleManager.instance == null)
			Instantiate(marbleManager);

		if (HealthManager.instance == null)
			Instantiate(healthManager);

		if (SoundManager.instance == null)
			Instantiate(soundManager);

		if (PlatformDetectionManager.instance == null)
			Instantiate(platformManager);

		if (LevelManager.instance == null)
			Instantiate(levelManager);

		if (ButterflyManager.instance == null && spawnButterflies)
			Instantiate(butterflyManager);

		if (NPC_Manager.instance == null)
			Instantiate(npcManager);

        if (!GameObject.Find("In-Game Menus(Clone)") && enablePauseMenu)
            Instantiate(menu);

		if (!GameObject.Find("FPS Display(Clone)"))
			Instantiate(fpsDisplay);

		if (OrigamiManager.instance == null)
			Instantiate(origamiManager);

		if (DistanceCuller.instance == null)
			Instantiate(distanceCuller);

		if (AnalyticsManager.instance == null)
			Instantiate(analyticsManager);
	}
}
