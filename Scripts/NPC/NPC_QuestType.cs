using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

// NOTE:
// Remember to make a function for nullifying all data that doesn't relate to current quest.
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum QUESTTYPE {

	COLLECT,		// Find and bring back a specific held object (coconut, etc)
	DEFEAT,			// Defeat number of specific enemies (total)
	BOSS,			// Go to area and defeat specific enemy
	SURVIVE,		// Go to area and survive waves of enemies
	GATHER,			// Get certain amount of specific objects (list / single)
	PLATFORM,		// Get to specific spot shown through cutscene
	EXPLORE,		// Find secret location
	BALL,			// Spawn ball, get ball into mug
	NULL

}
 
[Serializable]
public struct SurviveWave {

	public enum EnemyWaveType
	{
		dustbunny, flying
	}

	public int numberOfEnemies;

	public EnemyWaveType enemyWaveType;

	public float timeLimit;

	public bool empty;

}

[ExecuteInEditMode]
public class NPC_QuestType : MonoBehaviour {


	QUEST_STATUS questStatus = QUEST_STATUS.NEUTRAL;
	QUEST_STATUS oldStatus = QUEST_STATUS.NEUTRAL;
	public QUEST_STATUS QuestStatus { get { return questStatus; } set { questStatus = value; } } 


	// Events
	public delegate void QuestType_Complete();
	public event QuestType_Complete OnQuestComplete;	// - fire when completed

	public delegate void QuestType_Failed();
	public event QuestType_Failed OnQuestFailed;	// - fire when completed


	public Color npcColor;
	public string npcName;

	QuestObject questObject;
	public QUESTTYPE questType;

	Sprite questStarted;
	Sprite questComplete;
	Transform questHUDTextContainer;
	Text questHUDText;

	GameObject questAlertObj;

	public bool tutorials;

	// Are we using a timer for this quest?
	public bool useTimer;
	public bool useCutscene;
	GameObject timerObject;
	public GameObject cutsceneScriptManager;

	#region SURVIVE
	// SURVIVE

	public string storedWaveListFileName = "blank";

	public int numberOfWaves = 5;

	public List<SurviveWave> surviveWaveList;

	public SurviveWave SW1;
	public SurviveWave SW2;
	public SurviveWave SW3;
	public SurviveWave SW4;
	public SurviveWave SW5;

	public bool WaveToggleGroup;

	public int maxNumWaves = 5;

	public bool displayWaveInfo;

	public int selectedWaveNumber = 1;

	public int numEnemies;

	public SurviveWave.EnemyWaveType waveType;

	public float timeLimit;

	public bool showCurrentWaves;
	#endregion

	#region COLLECT
	// COLLECT

	public List<GameObject> collectObjects;

	public List<int> amountCollectObjects;

	public int numberOfDifferentObjects = 5;
	#endregion

	#region GATHER
	// GATHER
	Transform candyUIObj;

	int currentCandyAmount;
	int totalCandyRequired;

	public int numberOfCandyRequired;

	public GameObject candyContainer;	// We turn this on and allow the player to gather the candy within it
	#endregion

	#region PLATFORM
	// PLATFORM

	public GameObject endPlatformGoal;
	#endregion

	#region BOSS
	// BOSS

	public GameObject bossObject;
	#endregion

	#region EXPLORE
	// EXPLORE
	public GameObject exploreHintHUDObject;

	public string exploreHintText;
	#endregion

	#region DEFEAT
	// DEFEAT

	public int defeatNumberOfEnemies;

	public List<GameObject> specificEnemiesList;

	#endregion

	#region BALL
	// BALL

	public GameObject ballObj;

	public GameObject spawnLocationBall;

	Vector3 startLocationBall;
	#endregion

	void OnEnable(){

		Candy_Collect.OnCandyCollect += CandyCollect;

	}
	void OnDisable(){

		Candy_Collect.OnCandyCollect -= CandyCollect;

	}

	void Start () {

		LoadData ();

		SetEvents (true);

		Init ();

		if (questType == QUESTTYPE.GATHER) {

			candyContainer = transform.GetChild (0).gameObject;

			candyContainer.SetActive (true);
			candyContainer.SetActive (false);

			List<string> sceneList = new List<string> { "Pickup Zone", "Mountain Zone" };

			StartCoroutine (WaitForActiveScene (sceneList));
		}

		if (questType == QUESTTYPE.BALL) 
		{
			SpawnBall ();
		}

	}

	IEnumerator WaitForActiveScene(List<string> sceneList){

		bool check = true;

		while(check)
		{
			for (int i = 0; i < sceneList.Count; i++)
			{
				if (SceneManager.GetActiveScene ().name == sceneList [i])
				{
					check = false;

					if(SavingLoading.instance)
						currentCandyAmount = SavingLoading.instance.GetLoadCandy();

					totalCandyRequired = numberOfCandyRequired;
					numberOfCandyRequired -= currentCandyAmount;

					if (currentCandyAmount > 0)
					{
						candyUIObj.GetComponent<PopupText>().SetText(currentCandyAmount + " / " + totalCandyRequired, true);
						candyUIObj.GetComponent<PopupText>().DoPopUp();
					}
				}
			}

			yield return new WaitForEndOfFrame ();
		}
	}


	// true for +, false for -
	void SetEvents(bool status){

		if (status)
		{
			if (GetComponent<SavingLoading_StorageKeyCheck> ())
				GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck += KeyCheck;

			if (questType == QUESTTYPE.DEFEAT)
			{
				for (int i = 0; i < specificEnemiesList.Count; i++)
				{
					if (specificEnemiesList [i].transform.GetChild(1).GetComponent<QuestObject> () != null)
						specificEnemiesList [i].transform.GetChild(1).GetComponent<QuestObject> ().OnQuestEnemyDefeat += EnemyDefeat;
				}
			}

			if (questType == QUESTTYPE.BOSS)
			{
				questObject = bossObject.GetComponent<QuestObject> ();
				questObject.OnQuestBossDefeat += BossDefeat;
			}

			if (questType == QUESTTYPE.PLATFORM)
				endPlatformGoal.GetComponent<WinZone> ().OnWinZoneActivate += CompleteQuest;

			if (questType == QUESTTYPE.EXPLORE)
				endPlatformGoal.GetComponent<WinZone> ().OnWinZoneActivate += CompleteQuest;
	
			if (questType == QUESTTYPE.BALL)
				endPlatformGoal.GetComponent<WinZone> ().OnWinZoneActivate += CompleteQuest;
		}
		else
		{
			if (GetComponent<SavingLoading_StorageKeyCheck> ())
				GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck -= KeyCheck;

			if (questType == QUESTTYPE.DEFEAT)
			{
				for (int i = 0; i < specificEnemiesList.Count; i++)
				{
					if (specificEnemiesList [i] != null)
					{
						if (questObject) {
							questObject = specificEnemiesList [i].GetComponent<QuestObject> ();
							questObject.OnQuestEnemyDefeat -= EnemyDefeat;
						}
					}
				}
			}

			if (questType == QUESTTYPE.BOSS)
			{
				questObject = bossObject.GetComponent<QuestObject> ();
				questObject.gameObject.GetComponent<QuestObject>().OnQuestBossDefeat -= BossDefeat;
			}

			if (questType == QUESTTYPE.PLATFORM)
				endPlatformGoal.GetComponent<WinZone> ().OnWinZoneActivate -= CompleteQuest;

			if (questType == QUESTTYPE.EXPLORE)
				endPlatformGoal.GetComponent<WinZone> ().OnWinZoneActivate -= CompleteQuest;

			if (questType == QUESTTYPE.BALL)
				endPlatformGoal.GetComponent<WinZone> ().OnWinZoneActivate -= CompleteQuest;
		}

	}

	void Init(){
		
		// init
		candyUIObj = GameObject.Find ("MainHUDCanvas").transform.GetChild (1);
		questAlertObj = GameObject.Find ("QuestAlert");
		questStarted = Resources.Load<Sprite> ("questStart");
		questComplete = Resources.Load<Sprite> ("questComplete");
		questHUDTextContainer = questAlertObj.transform.GetChild (0);
		questHUDText = questHUDTextContainer.GetChild(1).GetComponent<Text> ();
	}

	public void SpawnBall(){

		startLocationBall = ballObj.transform.position;
		ballObj.GetComponent<Rigidbody> ().velocity = Vector3.zero;

	}

	void OnDestroy(){
		if(candyContainer)
			candyContainer.SetActive (true);
	}

	bool showQuestStart = true;
	public bool ShowQuestStart { set {showQuestStart = value;}}

	// When quest state changes to Quest Start
	void OnQuestStart()
	{
		if (oldStatus != questStatus && questStatus == QUEST_STATUS.STARTED) 
		{
			// Using a timer?  Give event to the timer
			if (useTimer)
			if (timerObject != null)
			if (timerObject.GetComponent<ActivatedTimer> ()) {
				timerObject.GetComponent<ActivatedTimer> ().OnTimerStart += TimerStart;
				timerObject.GetComponent<ActivatedTimer> ().OnTimerRunOut += TimerEnd;
			}

			if (showQuestStart)
			{
				// Play Quest Complete animation
				questAlertObj.GetComponent<Image> ().sprite = questStarted;
				questAlertObj.GetComponent<Animator> ().SetTrigger ("START");
				questHUDTextContainer.gameObject.SetActive (false);
			}

			if(questType == QUESTTYPE.BALL){

				// Move ball
				ballObj.transform.position = spawnLocationBall.transform.position;
				ballObj.GetComponent<Rigidbody> ().velocity = Vector3.zero;

				endPlatformGoal.GetComponent<WinZone> ().PuzzleON ();
			}

			if(questType == QUESTTYPE.EXPLORE){

				endPlatformGoal.GetComponent<WinZone> ().PuzzleON ();
			}

			if(questType == QUESTTYPE.PLATFORM){

				endPlatformGoal.GetComponent<WinZone> ().PuzzleON ();
			}
		}
	}

	// Quest Start doesn't count
	void OnQuestStateChange(){
		if (oldStatus != questStatus && questStatus != QUEST_STATUS.STARTED) {

			// Clean up timer references
			if (useTimer)
			if (timerObject != null)
			if (timerObject.GetComponent<ActivatedTimer> ()) {
				timerObject.GetComponent<ActivatedTimer> ().OnTimerStart -= TimerStart;
				timerObject.GetComponent<ActivatedTimer> ().OnTimerRunOut -= TimerEnd;
			}

		}
	}

	void Update () {

		OnQuestStateChange ();

		OnQuestStart ();

		// Update quest status tracking
		oldStatus = questStatus;

		// Update the boss quest status
		if (questType == QUESTTYPE.BOSS && questStatus != QUEST_STATUS.COMPLETE) {

			if (bossObject)
				bossObject.GetComponent<QuestObject> ().QuestStatus = questStatus;
		}

		if (questType == QUESTTYPE.COLLECT && questStatus != QUEST_STATUS.COMPLETE) {

			// Counter for quest
			int countdownToCompleteQuest = collectObjects.Count;

			// Add to the counter for each completed object
			for (int i = 0; i < collectObjects.Count; i++) {

				if (amountCollectObjects [i] <= 0) {
					countdownToCompleteQuest -= 1;
				}

			}

			// If you turned in all objects, complete quest
			if (countdownToCompleteQuest <= 0) {
				CompleteQuest ();
			}

		}

		if (questType == QUESTTYPE.GATHER && questStatus != QUEST_STATUS.COMPLETE) {

			// Show the current candy collected
			candyUIObj.GetComponent<PopupText> ().SetText (currentCandyAmount + " / " + totalCandyRequired, true);

			if(questStatus == QUEST_STATUS.STARTED)
				candyContainer.SetActive (true);

			if (numberOfCandyRequired <= 0) {
				CompleteQuest ();
			}
		}

		if (questType == QUESTTYPE.DEFEAT && questStatus != QUEST_STATUS.COMPLETE) {

			if (defeatNumberOfEnemies <= 0) {

				bool allSpecificEnemiesDead = true;

				for (int i = 0; i < specificEnemiesList.Count; i++) {

					if (specificEnemiesList [i] != null) {
						allSpecificEnemiesDead = false;

						GameObject enemyObj = specificEnemiesList [i].GetComponentInChildren<NewSmallEnemyScript> ().gameObject;

						if (enemyObj.GetComponent<QuestObject> ())
							enemyObj.GetComponent<QuestObject> ().QuestStatus = questStatus;
						else {
							Debug.LogError (specificEnemiesList [i].name + " does not have a <b>QuestObject</b> component attached and will not work correctly.", specificEnemiesList [i]);
						}
					}

				}

				// If specific enemies are all dead, and we defeated enough total...
				if (allSpecificEnemiesDead)
					CompleteQuest ();

			}
		}

	}

	public void SetCandyContainerActive(bool status){
		candyContainer.SetActive (status);
	}

	// If storageKey marks this as completed, perform these actions
	void KeyCheck(){

		questStatus = QUEST_STATUS.COMPLETE;

		// Specific checks
		if (questType == QUESTTYPE.DEFEAT) {
			
			defeatNumberOfEnemies = 0;

			foreach (GameObject enemy in specificEnemiesList) {
				Destroy (enemy);
			}

			specificEnemiesList.Clear ();
		}

		if (questType == QUESTTYPE.EXPLORE) {

			endPlatformGoal.SetActive (false);
		}

		if (questType == QUESTTYPE.PLATFORM) {

			endPlatformGoal.SetActive (false);
		}

		if (questType == QUESTTYPE.BALL) {

			endPlatformGoal.SetActive (false);
		}

		if (questType == QUESTTYPE.GATHER) {
			
			currentCandyAmount = totalCandyRequired;

			numberOfCandyRequired = 0;

			Destroy (candyContainer);

		}

		if (questType == QUESTTYPE.BOSS) {

			Destroy (bossObject);
		}

		if (questType == QUESTTYPE.COLLECT) {

			collectObjects.Clear ();

			amountCollectObjects.Clear ();

			numberOfDifferentObjects = 0;
		}

		if (OnQuestComplete != null)
			OnQuestComplete ();

		GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
	}

	// called from the quest container, don't call from within this class
	public void ResetQuest(){

		timerOn = false;

		SetEvents (false);	// remove all event calls

		if (questType == QUESTTYPE.BOSS) {

			if (bossObject != null)
			if (bossObject.GetComponent<SmallEnemyControl> ())
				bossObject.GetComponent<SmallEnemyControl> ().HealEnemy (99999);
		}

		if (questType == QUESTTYPE.BALL) {

			// replace ball
			ballObj.transform.position = startLocationBall;
			ballObj.GetComponent<Rigidbody> ().velocity = Vector3.zero;

		}

		if (questType == QUESTTYPE.EXPLORE) {

			endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();
		}

		if (questType == QUESTTYPE.BALL) {

			endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();
		}

		if (questType == QUESTTYPE.PLATFORM) {

			endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();
		}

		if (questType == QUESTTYPE.GATHER) {

			currentCandyAmount = 0;
			numberOfCandyRequired = 100;
			totalCandyRequired = 100;
		}

	}

	// Timer grouping
	bool timerOn = false;
	void TimerStart(){ timerOn = true; }

	void TimerEnd(){ 

		if (questStatus == QUEST_STATUS.STARTED) {
			
			timerOn = false; 

			// timer runs out, you lose
			if (OnQuestFailed != null)
				OnQuestFailed ();
			
		}
	}

	void BossDefeat(){

		// if we're not using a timer, just set it to true
		if (!useTimer) {
			timerOn = true;
		}

		if (timerOn) {
			
			CompleteQuest ();

			// Always clean up...!
			timerOn = false;
		}

	}

	void EnemyDefeat(){

		if(questStatus != QUEST_STATUS.COMPLETE)
		if (defeatNumberOfEnemies > 0) {
			
			defeatNumberOfEnemies -= 1;

		} 

	}

	void CandyCollect(){

		if (questType != QUESTTYPE.GATHER) return;

		if (numberOfCandyRequired > 0)
		{
			numberOfCandyRequired -= 1;

			// Calculates how much candy has been collected
			currentCandyAmount = totalCandyRequired - numberOfCandyRequired;
			SavingLoading.instance.SaveCandy(currentCandyAmount);
		}

	}
		
	public void SaveData(){

		string path = Application.persistentDataPath + "/" + storedWaveListFileName + ".jamroot";

		BinaryFormatter binFormat = new BinaryFormatter ();
		FileStream fileStr = File.Create (path);

		binFormat.Serialize (fileStr, surviveWaveList);
		fileStr.Close ();

	}

	public bool LoadData(){

		string path = Application.persistentDataPath + "/" + storedWaveListFileName + ".jamroot";

		if (File.Exists (path)) {

			BinaryFormatter binFormat = new BinaryFormatter ();
			FileStream fileStr = File.Open (path, FileMode.Open);

			surviveWaveList = (List<SurviveWave>)binFormat.Deserialize (fileStr);
			fileStr.Close ();

			return true;

		} 
		else{
			//Debug.Log ("File does not exist at " + path);
			return false;
		}

	}

	public void CompleteQuest(){

		if (questStatus != QUEST_STATUS.COMPLETE) {	// you cannot complete a quest multiple times
			
			if (OnQuestComplete != null)
				OnQuestComplete ();

			questStatus = QUEST_STATUS.COMPLETE;	// needed

			Init ();

			// Play Quest Complete animation
			questAlertObj.GetComponent<Image> ().sprite = questComplete;
			questAlertObj.GetComponent<Animator> ().SetTrigger ("START");
			questHUDTextContainer.gameObject.SetActive (true);
			questHUDText.text = npcName;
			questHUDText.color = npcColor;

			// End timer
			if (useTimer)
			if (timerObject != null)
			if (timerObject.GetComponent<ActivatedTimer> ()) {
				timerObject.GetComponent<ActivatedTimer> ().Complete ();
			}

			if (questType == QUESTTYPE.PLATFORM) {
				endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();
				endPlatformGoal.SetActive (false);
			}

			if (questType == QUESTTYPE.BALL) {
				endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();
				endPlatformGoal.SetActive (false);
			}

			if (questType == QUESTTYPE.EXPLORE) {
				endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();
				endPlatformGoal.SetActive (false);
			}

			questType = QUESTTYPE.NULL;

			Debug.Log ("Quest Complete");
		}

	}

}

