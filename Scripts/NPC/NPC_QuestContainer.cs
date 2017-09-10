
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif


// Tips:  Instead of questComplete, questInProgress, etc... use state machine
//		Neutral, 		before a quest has been started
//		QuestStarted,	when quest is started
//		QuestFinished, 	when quest is finished, before any reward is given
//		QuestComplete	once player receives reward


public enum QUEST_STATUS {

	NEUTRAL, FAILED, STARTED, FINISHED, COMPLETE

}


public class NPC_QuestContainer : MonoBehaviour {


	QUEST_STATUS questStatus = QUEST_STATUS.NEUTRAL;
	QUEST_STATUS oldStatus = QUEST_STATUS.NEUTRAL;
	public QUEST_STATUS QuestStatus { get { return questStatus; } set { questStatus = value; } } 


	// Events
	public delegate void QuestNPC_Started();
	public event QuestNPC_Started OnQuestStarted;	// - fire when completed

	public delegate void QuestNPC_Failed();
	public event QuestNPC_Failed OnQuestFailed;	// - fire when failed

	public delegate void QuestNPC_CompleteDialogue();
	public event QuestNPC_CompleteDialogue OnQuestCompleteDialogue;	

	NPC_QuestType questTypeScript;
	InteractPopup_DistanceCheck interactPopup;

	public bool simpleShift;	// Instead of instantiating an object, shift a current one over

	public bool useTimer;
	ActivatedTimer timerScript;

	public Color npcColor;
	public string npcName;

	PlayerHolder playerHolder; 

	GameObject playerObj; 

	Transform candyUIObj;

	public GameObject dialogueType;	// the specific dialogue of this NPC

	public float timerTime;

	// Quest Specific

	public GameObject specificQuest;

	public GameObject rewardObject;	// just a marble, basically.  Prefab.

	public int rewardAmount;

	public Checkpoint checkpoint;

	public bool questInProgress = false;
	public bool questStartedCheck = false;
	public bool questFailedOnce = false;

	// Dialogue
	public string startingDialogue;
	public string questStartedDialogue;
	public string questFailedDialogue;
	public string questCompleteDialogue;
	public string endingDialogue;

	MenuManager menuManager;

	string storageKey = "";

	void Start () {

		// Event Triggers
		if (GetComponent<SavingLoading_StorageKeyCheck> ()) {
			GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck += KeyCheck;
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;
		}

		SceneLoader.OnSceneLoaderLoad += ResetQuest;
		SceneManager.sceneLoaded += SceneLoadedEvent;
			
		menuManager = GameObject.Find ("In-Game Menus(Clone)").GetComponent<MenuManager> ();
		questTypeScript = specificQuest.GetComponent<NPC_QuestType> ();

		menuManager.OnQuit += AboutToQuit;

		// Find ref.
		timerScript = GameObject.Find ("TimerUI").GetComponent<ActivatedTimer> ();	// find timer
		candyUIObj = GameObject.Find ("MainHUDCanvas").transform.GetChild (1);
		playerObj = GameObject.FindWithTag ("Player");
		playerHolder = GameObject.FindWithTag ("Player").GetComponentInChildren<PlayerHolder> ();

		interactPopup = GetComponentInChildren<InteractPopup_DistanceCheck> ();
		if (interactPopup == null) Debug.LogError ("No interact popup", this.gameObject);	// just make sure

		questStatus = SavingLoading.instance.LoadQuestStatus_Container(storageKey);
		questTypeScript.QuestStatus = SavingLoading.instance.LoadQuestStatus_Type(storageKey);

		// Prevent QuestStart appearing if we load into a started quest
		if (questStatus == QUEST_STATUS.STARTED)
		{
			questTypeScript.ShowQuestStart = false;
		}

		SetQuestLoadSpecifics (false);
		LoadKeyDialogue ();

		dialogueType.GetComponent<InteractPopup_DistanceCheck> ().OnDialogueFinish += DialogueFinish;

		// set color and name...
		npcColor = transform.GetChild (0).GetComponent<Renderer> ().material.color;
		npcName = GetComponent<NPC_Behavior> ().NPCName;

		// ...then give to the quest
		questTypeScript.npcColor = npcColor;
		questTypeScript.npcName = npcName;

	}	// Start

	void LoadKeyDialogue(){

		if (SavingLoading.instance.LoadQuestStatus_Container (storageKey) == QUEST_STATUS.NEUTRAL)
			ChangeDialogue (startingDialogue);

		if (SavingLoading.instance.LoadQuestStatus_Container (storageKey) == QUEST_STATUS.STARTED)
			ChangeDialogue (questStartedDialogue);

		if (SavingLoading.instance.LoadQuestStatus_Container (storageKey) == QUEST_STATUS.FAILED)
			ChangeDialogue (questFailedDialogue);

		if (SavingLoading.instance.LoadQuestStatus_Container (storageKey) == QUEST_STATUS.FINISHED)
			ChangeDialogue (endingDialogue);

		if (SavingLoading.instance.LoadQuestStatus_Container (storageKey) == QUEST_STATUS.COMPLETE)
			ChangeDialogue (questCompleteDialogue);
		
	}

	void ChangeDialogue(string newDialogue){

		// "npc name" determines dialogue in this case, not the name in the dialogue box
		dialogueType.GetComponent<InteractPopup_DistanceCheck>().NPCName = newDialogue;
		dialogueType.GetComponent<InteractPopup_DistanceCheck> ().ResetStoredDialogue ();	// after changing the name, you must reset it

	}

	void SetQuestLoadSpecifics(bool cutsceneActive)
	{		
		if(!GetComponent<NPC_QuestContainer>()){
			interactPopup = GetComponentInChildren<InteractPopup_DistanceCheck> ();
			if (interactPopup == null) Debug.LogError ("No interact popup", this.gameObject);	// just make sure
		}

		if (questStatus == QUEST_STATUS.NEUTRAL) {
			interactPopup.SetDialoguePopupTexture ("done");
			questStartedCheck = false; questInProgress = false;

			if(questTypeScript.endPlatformGoal)
				questTypeScript.endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();

			if (questTypeScript.questType == QUESTTYPE.GATHER) {
				StartCoroutine(CandyPopupDown ());
				questTypeScript.candyContainer.SetActive (false);
			}
		}

		if (questStatus == QUEST_STATUS.STARTED) {
			interactPopup.SetDialoguePopupTexture ("done");
			questStartedCheck = true; questInProgress = true;

			// Quest Initialization
			if (timerScript) 
			{
				// Event
				timerScript.OnTimerRunOut += QuestFailed;
			}

			if (specificQuest) 
			{
				// Event
				questTypeScript.OnQuestComplete += QuestComplete;
				questTypeScript.OnQuestFailed += QuestFailed;
			}

			// Set Timer
			NPC_QuestType questType;
			ScriptStateManager stateMan;
			WinZone winZone;

			if (useTimer) {
				questType = questTypeScript;
				stateMan = questType.cutsceneScriptManager.GetComponent<ScriptStateManager> ();
				winZone = questType.endPlatformGoal.GetComponent<WinZone> ();

				timerScript.ClaimTimer (checkpoint, TIMERTYPE.QUEST, this.gameObject, timerTime, stateMan, winZone);
				winZone.PuzzleON ();
			}
			// ---

			if(questTypeScript.endPlatformGoal)
				questTypeScript.endPlatformGoal.GetComponent<WinZone> ().PuzzleON ();

			if (questTypeScript.questType == QUESTTYPE.PLATFORM) 
			{
				if (questTypeScript.endPlatformGoal)
					questTypeScript.endPlatformGoal.SetActive (true);
			}

			if (questTypeScript.questType == QUESTTYPE.BALL) 
			{
				questTypeScript.SpawnBall ();

				if (questTypeScript.endPlatformGoal)
					questTypeScript.endPlatformGoal.SetActive (true);
			}

			if (questTypeScript.questType == QUESTTYPE.EXPLORE) {

				if (questTypeScript.endPlatformGoal)
					questTypeScript.endPlatformGoal.SetActive (true);

				if (questTypeScript.exploreHintHUDObject)
					questTypeScript.exploreHintHUDObject.GetComponentInChildren<Text> ().text = questTypeScript.exploreHintText;

			}

			if (questTypeScript.questType == QUESTTYPE.GATHER) 
			{
				candyUIObj.GetComponent<PopupText> ().DoPopUp ();
				questTypeScript.SetCandyContainerActive (true);

				// Go through and load candy checks
				for (int i = 0; i < specificQuest.transform.GetChild (0).childCount; i++) {
					specificQuest.transform.GetChild (0).GetChild (0).GetComponent<ObjInfo> ().LOAD ();
				}
			}

			// Fire event
			if (OnQuestStarted != null)
				OnQuestStarted ();
			
			// Play cutscene if one is available
			if (questTypeScript.cutsceneScriptManager && cutsceneActive)
				questTypeScript.cutsceneScriptManager.GetComponent<ScriptStateManager> ().TriggerCutscene ();
		}

		if (questStatus == QUEST_STATUS.FAILED) {
			interactPopup.SetDialoguePopupTexture ("none");
			questStartedCheck = false; questInProgress = false;

			if(questTypeScript.endPlatformGoal)
				questTypeScript.endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();

			if (questTypeScript.questType == QUESTTYPE.GATHER) {
				StartCoroutine(CandyPopupDown ());
				questTypeScript.candyContainer.SetActive (false);
			}
		}

		if (questStatus == QUEST_STATUS.FINISHED) {
			interactPopup.SetDialoguePopupTexture ("start");
			questStartedCheck = true; questInProgress = true;

			if(questTypeScript.endPlatformGoal)
				questTypeScript.endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();

			if (questTypeScript.questType == QUESTTYPE.GATHER) {
				StartCoroutine(CandyPopupDown ());
				questTypeScript.candyContainer.SetActive (false);
			}
		}

		if (questStatus == QUEST_STATUS.COMPLETE) {
			interactPopup.SetDialoguePopupTexture ("none");
			questStartedCheck = true; questInProgress = true;

			if(questTypeScript.endPlatformGoal)
				questTypeScript.endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();

			if (questTypeScript.questType == QUESTTYPE.GATHER) {
				StartCoroutine(CandyPopupDown ());
				questTypeScript.candyContainer.SetActive (false);
			}
		}
	}

	void SetQuestStatus(QUEST_STATUS status){

		if (status == QUEST_STATUS.COMPLETE)
		{
			if (OnQuestCompleteDialogue != null)
				OnQuestCompleteDialogue ();
		}

		questStatus = status;
		questTypeScript.QuestStatus = status;
		SavingLoading.instance.SaveQuestStatus (status, status, storageKey);
		SetQuestLoadSpecifics (true);

	}
		
	void SceneLoadedEvent(Scene scene, LoadSceneMode mode){

		// Reset events when the scene changes
		SceneLoader.OnSceneLoaderLoad -= ResetQuest;
	}

	// If storageKey marks this as completed, perform these actions
	void KeyCheck(){
		
		GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck -= KeyCheck;

		rewardAmount = 0;

		SetQuestStatus (QUEST_STATUS.COMPLETE);

		ChangeDialogue (endingDialogue);

		if (OnQuestStarted != null)
			OnQuestStarted ();

		GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
	}
		
	void QuestComplete(){

		print ("QUEST COMPLETE - NPC CONTAINER");

		if (questTypeScript.questType == QUESTTYPE.GATHER)
			StartCoroutine(CandyPopupDown ());
		
		SetQuestStatus (QUEST_STATUS.FINISHED);

		ChangeDialogue (questCompleteDialogue);

		questInProgress = false;
	}

	void QuestFailed(){

		if (questStatus == QUEST_STATUS.STARTED) {
			
			questFailedOnce = true;

			if (OnQuestFailed != null)
				OnQuestFailed ();

			SetQuestStatus (QUEST_STATUS.FAILED);

			ChangeDialogue (questFailedDialogue);

			ResetQuest ();

			questStartedCheck = false;
			questInProgress = false;
		}

	}

	void AboutToQuit()
	{
		print ("About to quit");

		// Remove event, allowing partial quest data to save when quitting the game
		SceneLoader.OnSceneLoaderLoad -= ResetQuest;
		menuManager.OnQuit -= AboutToQuit;
	}

	// This gets overridden by a storage key check
	void ResetQuest(){

		print ("RESET");

		SceneLoader.OnSceneLoaderLoad -= ResetQuest;

		if (specificQuest) {

			// Event
			questTypeScript.OnQuestComplete -= QuestComplete;
			questTypeScript.OnQuestFailed -= QuestFailed;

		}

		// Set status to neutral
		SetQuestStatus (QUEST_STATUS.NEUTRAL);

		// Perform Neutral status initialization
		interactPopup.SetDialoguePopupTexture ("done");
		questStartedCheck = false; questInProgress = false;

		if(questTypeScript.endPlatformGoal)
			questTypeScript.endPlatformGoal.GetComponent<WinZone> ().PuzzleOFF ();

		if (questTypeScript.questType == QUESTTYPE.GATHER) {
			StartCoroutine(CandyPopupDown ());
			questTypeScript.candyContainer.SetActive (false);
			SavingLoading.instance.ResetStoredCandy (SceneManager.GetActiveScene ().buildIndex);
		}

		// change dialogue to neutral state
		ChangeDialogue (startingDialogue);

		questTypeScript.ResetQuest ();

	}

	// Called every time the dialogue is closed
	void DialogueFinish(){

		if (questStatus != QUEST_STATUS.FAILED) {
			
			if (questStatus == QUEST_STATUS.FINISHED) 
			{
				// Remove event so it doesn't trigger...
				GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck -= KeyCheck;
				questTypeScript.GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck -= KeyCheck;
				// Then turn it off after...
				GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
				questTypeScript.GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;

				// Save the tags - only when quest is COMPLETE
				SavingLoading.instance.SaveStorageKey (GetComponent<SavingLoading_StorageKeyCheck> ().storageKey, true);
				SavingLoading.instance.SaveData ();

				print ("TEST");

				for (int i = 0; i < rewardAmount; i++) 
				{
					StartCoroutine (SpawnReward (i));
				}

				rewardAmount = 0;

				SetQuestStatus (QUEST_STATUS.COMPLETE);

				ChangeDialogue (endingDialogue);
			}

			// Initial check~ we actually start the quest just below.
			if (questStatus != QUEST_STATUS.FINISHED && 
				questStatus != QUEST_STATUS.COMPLETE && 
				!questInProgress) 
			{
				ChangeDialogue (questStartedDialogue);

				questInProgress = true;
			}

			// If quest just started, perform start events
			if (questStartedCheck != questInProgress && 
				questStatus != QUEST_STATUS.FINISHED) {

				StartQuest ();

			}

			// stores the difference
			questStartedCheck = questInProgress;

		} else {
			
			if (questStatus != QUEST_STATUS.FINISHED && 
				questStatus != QUEST_STATUS.COMPLETE && 
				!questInProgress) 
			{
				SetQuestStatus (QUEST_STATUS.NEUTRAL);

				ChangeDialogue (startingDialogue);
			}
		}
	}	// DialogueFinish

	// Begin necessary "Start Quest" events
	void StartQuest()
	{
		if (questStatus == QUEST_STATUS.NEUTRAL) 
		{
			SetQuestStatus (QUEST_STATUS.STARTED);
		}
	}

	IEnumerator SpawnReward(float i){

		yield return new WaitForSeconds (i / 100);

		// spawn a marble
		if (!simpleShift) {
			GameObject reward; 
			reward = (GameObject)Instantiate (rewardObject, playerObj.transform.position + Random.insideUnitSphere * 12, Quaternion.identity);
			StartCoroutine (FlyTowardPlayer (reward));
		} else {
			rewardObject.transform.position = playerObj.transform.position + (Vector3.up * (5 + i));

			if (rewardObject.GetComponent<Pickupable> ())
				rewardObject.GetComponent<Pickupable> ().ResetStartValues ();
			else
				Debug.LogError ("Reward Object isn't a <Pickupable> object.", rewardObject);
		}
		
	}

	IEnumerator FlyTowardPlayer(GameObject reward){

		while (reward) {
			reward.transform.position = Vector3.MoveTowards (reward.transform.position, playerObj.transform.position, 20 * Time.deltaTime);
			yield return new WaitForEndOfFrame ();
		}

	}

	void Update () {

		if (oldStatus != questStatus && questStatus == QUEST_STATUS.STARTED) {


		}

		oldStatus = questStatus;

		questTypeScript.QuestStatus = questStatus;	// use the QuestContainer to keep QuestType (for owned quest) updated

	}	// Update
		
	void OnTriggerEnter(Collider col){

		if (specificQuest) {

			if (questTypeScript.questType == QUESTTYPE.COLLECT) {

				if (col.GetComponent<QuestObject> ())
				if (col.GetComponent<QuestObject> ().questObjectType == QUESTOBJECTTYPE.collectible) {

					// for each different type of object
					for (int i = 0; i < questTypeScript.collectObjects.Count; i++) {

						// If Coconut
						if (questTypeScript.collectObjects [i].GetComponent<QuestObject> ().collectibleType == COLLECTIBLETYPE.coconut) {

							// Turn in the object
							if (questTypeScript.amountCollectObjects [i] > 0) {
								questTypeScript.amountCollectObjects [i] -= 1;
							 
								// if holding, drop then remove
								if (playerHolder.IsHolding) {
									if (playerHolder.HeldObject.gameObject == col.gameObject) {
										playerHolder.Drop ();
									}
								}

								// remove object
								col.gameObject.transform.position -= Vector3.one * 999999;

								// fire event
								col.GetComponent<QuestObject> ().Collected ();
							}

						}
					}
				}
			}
		}
	}	// OnTriggerEnter

	IEnumerator CandyPopupDown(){

		yield return new WaitForSeconds (1.0f);

		candyUIObj.GetComponent<PopupText> ().DoPopDown ();

	}

}


