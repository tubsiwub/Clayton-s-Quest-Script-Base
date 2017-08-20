
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

	NPC_QuestType questTypeScript;

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
	public bool questFailed = false;
	public bool questFailedOnce = false;


	// Dialogue

	public string startingDialogue;

	public string questStartedDialogue;

	public string questFailedDialogue;

	public string questCompleteDialogue;

	public string endingDialogue;


	string storageKey = "";


	void Start () {

		// Event Triggers
		if (GetComponent<SavingLoading_StorageKeyCheck> ()) {
			GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck += KeyCheck;
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;
		}
			
		questTypeScript = specificQuest.GetComponent<NPC_QuestType> ();

		questStatus = SavingLoading.instance.LoadQuestStatus_Container(storageKey);
		questTypeScript.QuestStatus = SavingLoading.instance.LoadQuestStatus_Type(storageKey);

		timerScript = GameObject.Find ("TimerUI").GetComponent<ActivatedTimer> ();	// find timer


		candyUIObj = GameObject.Find ("MainHUDCanvas").transform.GetChild (1);

		ChangeDialogue (startingDialogue);

		playerObj = GameObject.FindWithTag ("Player");

		playerHolder = GameObject.FindWithTag ("Player").GetComponentInChildren<PlayerHolder> ();

		dialogueType.GetComponent<InteractPopup_DistanceCheck> ().OnDialogueFinish += DialogueFinish;

		// set color and name...
		npcColor = transform.GetChild (0).GetComponent<Renderer> ().material.color;
		npcName = GetComponent<NPC_Behavior> ().NPCName;

		// ...then give to the quest
		questTypeScript.npcColor = npcColor;
		questTypeScript.npcName = npcName;

		LoadKeyDialogue ();

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


	// If storageKey marks this as completed, perform these actions
	void KeyCheck(){

		rewardAmount = 0;

		rewardGiven = true;

		questStatus = QUEST_STATUS.COMPLETE;
		questTypeScript.QuestStatus = QUEST_STATUS.COMPLETE;
		SavingLoading.instance.SaveQuestStatus (questStatus, questTypeScript.QuestStatus, storageKey);

		ChangeDialogue (endingDialogue);

		GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
	}


	bool questComplete = false;
	bool rewardGiven = false;

	void QuestComplete(){

		if (questTypeScript.questType == QUESTTYPE.GATHER)
			StartCoroutine(CandyPopupDown ());
		
		questStatus = QUEST_STATUS.FINISHED;
		questTypeScript.QuestStatus = QUEST_STATUS.FINISHED;
		SavingLoading.instance.SaveQuestStatus (questStatus, questTypeScript.QuestStatus, storageKey);

		ChangeDialogue (questCompleteDialogue);

		questComplete = true;

		questInProgress = false;


	}

	void QuestFailed(){

		if (questStatus == QUEST_STATUS.STARTED) {
			
			questFailed = true;
			questFailedOnce = true;

			if (OnQuestFailed != null)
				OnQuestFailed ();

			questStatus = QUEST_STATUS.FAILED;
			questTypeScript.QuestStatus = QUEST_STATUS.FAILED;
			SavingLoading.instance.SaveQuestStatus (questStatus, questTypeScript.QuestStatus, storageKey);

			ChangeDialogue (questFailedDialogue);

			ResetQuest ();

			questStartedCheck = false;
			questInProgress = false;
			questComplete = false;
		}

	}

	void ResetQuest(){

		questTypeScript.ResetQuest ();

	}


	// Called every time the dialogue is closed
	void DialogueFinish(){

		if (!questFailed) {
			if (questComplete && !rewardGiven) {

				for (int i = 0; i < rewardAmount; i++) {
					StartCoroutine (SpawnReward (i));
				}

				rewardAmount = 0;

				rewardGiven = true;

				questStatus = QUEST_STATUS.COMPLETE;
				questTypeScript.QuestStatus = QUEST_STATUS.COMPLETE;
				SavingLoading.instance.SaveQuestStatus (questStatus, questTypeScript.QuestStatus, storageKey);

				ChangeDialogue (endingDialogue);

				// Save the tags - only when quest is COMPLETE
				SavingLoading.instance.SaveStorageKey (GetComponent<SavingLoading_StorageKeyCheck> ().storageKey, true);
				SavingLoading.instance.SaveData ();

			}

			// Initial check~ we actually start the quest just below.
			if (!questComplete && !rewardGiven && !questInProgress) {

				ChangeDialogue (questStartedDialogue);

				questInProgress = true;

			}

			// If quest just started, perform start events
			if (questStartedCheck != questInProgress && !questComplete) {

				StartQuest ();

			}

			// stores the difference
			questStartedCheck = questInProgress;

		} else {
			
			if (!questComplete && !rewardGiven && !questInProgress) {

				questStatus = QUEST_STATUS.NEUTRAL;
				questTypeScript.QuestStatus = QUEST_STATUS.NEUTRAL;
				SavingLoading.instance.SaveQuestStatus (questStatus, questTypeScript.QuestStatus, storageKey);

				ChangeDialogue (startingDialogue);

				questFailed = false;

			}

		}


	}	// DialogueFinish



	// Begin necessary "Start Quest" events
	void StartQuest(){

		if (questStatus == QUEST_STATUS.NEUTRAL) {


			// Quest Initialization

			if (timerScript) {

				// Event
				timerScript.OnTimerRunOut += QuestFailed;

			}

			if (specificQuest) {

				// Event
				questTypeScript.OnQuestComplete += QuestComplete;
				questTypeScript.OnQuestFailed += QuestFailed;

			}


			questStatus = QUEST_STATUS.STARTED;
			questTypeScript.QuestStatus = QUEST_STATUS.STARTED;
			SavingLoading.instance.SaveQuestStatus (questStatus, questTypeScript.QuestStatus, storageKey);
	
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

			// Fire event
			if (OnQuestStarted != null)
				OnQuestStarted ();
		
			// Start the cutscene
			if (questTypeScript.questType == QUESTTYPE.PLATFORM) {

				if (questTypeScript.endPlatformGoal)
					questTypeScript.endPlatformGoal.SetActive (true);
			
			}

			// Start the cutscene
			if (questTypeScript.questType == QUESTTYPE.BALL) {

				if (questTypeScript.endPlatformGoal)
					questTypeScript.endPlatformGoal.SetActive (true);

			}

			if (questTypeScript.questType == QUESTTYPE.EXPLORE) {

				if (questTypeScript.endPlatformGoal)
					questTypeScript.endPlatformGoal.SetActive (true);

				if (questTypeScript.exploreHintHUDObject)
					questTypeScript.exploreHintHUDObject.GetComponentInChildren<Text> ().text = questTypeScript.exploreHintText;
			
			}

			if (questTypeScript.questType == QUESTTYPE.GATHER) {

				candyUIObj.GetComponent<PopupText> ().DoPopUp ();

			}

			// Play cutscene if one is available
			if (questTypeScript.cutsceneScriptManager)
				questTypeScript.cutsceneScriptManager.GetComponent<ScriptStateManager> ().TriggerCutscene ();
		
		}
	}

	IEnumerator SpawnReward(float i){

		yield return new WaitForSeconds (i / 100);

		// spawn a marble
		if (!simpleShift){
			GameObject reward; 
			reward = (GameObject)Instantiate (rewardObject, playerObj.transform.position + Random.insideUnitSphere * 12, Quaternion.identity);
			StartCoroutine (FlyTowardPlayer (reward));
		}
		else
			rewardObject.transform.position = playerObj.transform.position + (Vector3.up * (5 + i));
		
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


