using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Parent the timer with relevant puzzle / quest objects


public enum TIMERTYPE {

	BOWLING, BUTTON, QUEST, TRIGGER

}


// 	TIMER RULE SET
// 	--------------
// 	
// 	Button - Uses WinZone, ButtonObj, Cutscene
// 	Quest - Uses WinZone, QuestNPC, Cutscene
// 	Trigger - Uses WinZone, TriggerObj, Cutscene
// 	
// 	WinZone:  If included; shuts off timer when reached
// 	ButtonObj, QuestNPC, or TriggerObj:  When included; starts timer by various means [MUST INCLUDE]
// 	Cutscene:  If included; timer waits for cutscene to finish before beginning


public class ActivatedTimer : MonoBehaviour {

	public TIMERTYPE timerType;


	// Inspector bools
	public bool tutorial;
	public bool usesCutscene;


	// Events
	public delegate void Timer_RunOut();
	public event Timer_RunOut OnTimerRunOut;

	public delegate void Timer_Start();
	public event Timer_Start OnTimerStart;

	PlayerHandler playerHandler;

	public GameObject puzzleButton;
	public GameObject questNPC;
	public GameObject triggerObj;
	public GameObject startZone;
	public ScriptStateManager scriptStateManager;

	public float timeDefault = 5;

	bool waitForCutscene = false;


	float timeRemaining = 5;

	public float TimeRemaining { get { return timeRemaining; } }

	bool timerStarted = false;


	public WinZone winZone;

	bool winState = false;
	public bool WinState { get { return winState; } }

	GameObject claimObj;
	public GameObject ClaimObj { get { return claimObj; } }

	Checkpoint storedCheckpoint;

	void Start () {
		
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();

		SetupEvents ();

	}

	public void SetupEvents(){
		
		// Set the event
		if(winZone)
			winZone.OnWinZoneActivate += Complete;

		if (scriptStateManager) 
			scriptStateManager.OnCutsceneCancel += CutsceneStartTimer;

		if (timerType == TIMERTYPE.QUEST) {
			
			// If we have a cutscene, don't start the timer based on the npc.
			if (!scriptStateManager) {
				if (questNPC)  questNPC.GetComponent<NPC_QuestContainer> ().OnQuestStarted += StartTimer;
			} else {
				if (questNPC)  questNPC.GetComponent<NPC_QuestContainer> ().OnQuestStarted += WaitForCutscene;
			}
		}

		if (timerType == TIMERTYPE.BUTTON) {

			// If we have a cutscene, don't start the timer based on the button.
			if (!scriptStateManager) {
				if (puzzleButton) puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated += StartTimer;
			} else {
				if (puzzleButton) puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated += WaitForCutscene;
			}

			if (puzzleButton) puzzleButton.GetComponent<PuzzleButton> ().OnButtonReset += TimerReset;

		}

		if (timerType == TIMERTYPE.TRIGGER) {
			// If we have a cutscene, don't start the timer based on the button.
			if (!scriptStateManager) {
				if (triggerObj) triggerObj.GetComponent<Cutscene_TriggerScript> ().OnTriggered += StartTimer;
			} else {
				if (triggerObj) triggerObj.GetComponent<Cutscene_TriggerScript> ().OnTriggered += WaitForCutscene;
			}
		}
			
	}

	public void ClaimTimer(Checkpoint checkpoint, TIMERTYPE timerType, GameObject startObj, float timerTime){

		if(checkpoint != null)
			storedCheckpoint = checkpoint;

		this.timerType = timerType;

		claimObj = startObj;

		if (startObj.GetComponent<PuzzleButton> ())
			this.puzzleButton = startObj;
		if (startObj.GetComponent<NPC_QuestContainer> ())
			this.questNPC = startObj;
		if (startObj.GetComponent<Cutscene_TriggerScript> ())
			this.triggerObj = startObj;
		
		this.timeDefault = timerTime;

		this.usesCutscene = false;

		SetupEvents ();
	}

	public void ClaimTimer(Checkpoint checkpoint, TIMERTYPE timerType, GameObject startObj, float timerTime, WinZone winZone){

		if(checkpoint != null)
			storedCheckpoint = checkpoint;

		this.timerType = timerType;

		claimObj = startObj;

		if (startObj.GetComponent<PuzzleButton> ())
			this.puzzleButton = startObj;
		if (startObj.GetComponent<NPC_QuestContainer> ())
			this.questNPC = startObj;
		if (startObj.GetComponent<Cutscene_TriggerScript> ())
			this.triggerObj = startObj;

		this.timeDefault = timerTime;

		this.usesCutscene = false;

		this.winZone = winZone;

		SetupEvents ();
	}

	public void ClaimTimer(Checkpoint checkpoint, TIMERTYPE timerType, GameObject startObj, float timerTime, ScriptStateManager cutscene){

		if(checkpoint != null)
			storedCheckpoint = checkpoint;

		this.timerType = timerType;

		claimObj = startObj;

		if (startObj.GetComponent<PuzzleButton> ())
			this.puzzleButton = startObj;
		if (startObj.GetComponent<NPC_QuestContainer> ())
			this.questNPC = startObj;
		if (startObj.GetComponent<Cutscene_TriggerScript> ())
			this.triggerObj = startObj;

		this.timeDefault = timerTime;

		this.usesCutscene = true;
		this.scriptStateManager = cutscene;

		SetupEvents ();
	}

	public void ClaimTimer(Checkpoint checkpoint, TIMERTYPE timerType, GameObject startObj, float timerTime, ScriptStateManager cutscene, WinZone winZone){

		if(checkpoint != null)
			storedCheckpoint = checkpoint;

		this.timerType = timerType;

		claimObj = startObj;

		if (startObj.GetComponent<PuzzleButton> ())
			this.puzzleButton = startObj;
		if (startObj.GetComponent<NPC_QuestContainer> ())
			this.questNPC = startObj;
		if (startObj.GetComponent<Cutscene_TriggerScript> ())
			this.triggerObj = startObj;

		this.timeDefault = timerTime;

		this.winZone = winZone;

		this.usesCutscene = true;
		this.scriptStateManager = cutscene;

		SetupEvents ();
	}



	public void Complete(){

		if(winZone)
			winZone.OnWinZoneActivate -= Complete;
		
		winState = true;

		if(winZone)
			winZone.gameObject.SetActive (false);
	}

	public void EndTimer(){

		timeRemaining = 0;

	}

	bool timerPaused = false;
	public void PauseTimer(){ timerPaused = true; }
	public void UnpauseTimer(){ timerPaused = false; }

	void Update () {

		if (timerStarted) {
			
			if (winState)
				timeRemaining = 0;

			if(!timerPaused)
				timeRemaining -= Time.deltaTime;

			if (timeRemaining <= 0) {

				timeRemaining = 0;

				TimerRunOut ();

			}
		}
	}

	void WaitForCutscene(){

		// Reset event calls
		if (timerType == TIMERTYPE.QUEST) 
			if (scriptStateManager) 
				if (questNPC)  questNPC.GetComponent<NPC_QuestContainer> ().OnQuestStarted -= WaitForCutscene;

		if (timerType == TIMERTYPE.BUTTON) 
			if (scriptStateManager) 
				if (puzzleButton) puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated -= WaitForCutscene;

		if (timerType == TIMERTYPE.TRIGGER) 
			if (scriptStateManager)
				if (triggerObj) triggerObj.GetComponent<Cutscene_TriggerScript> ().OnTriggered -= WaitForCutscene;
			

		waitForCutscene = true;

	}

	void CutsceneStartTimer(){

		if (scriptStateManager) 
			scriptStateManager.OnCutsceneCancel -= CutsceneStartTimer;
		
		waitForCutscene = false;

		StartTimer ();

	}

	bool setActive = false;	// allows the timer to show up

	// BEGIN timer
	public void StartTimer(){

		// Reset all event calls
		if (timerType == TIMERTYPE.QUEST) 
			if (!scriptStateManager) 
				if (questNPC)  questNPC.GetComponent<NPC_QuestContainer> ().OnQuestStarted -= StartTimer;

		if (timerType == TIMERTYPE.BUTTON) 
			if (!scriptStateManager)
				if (puzzleButton) puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated -= StartTimer;

		if (timerType == TIMERTYPE.TRIGGER) 
			if (!scriptStateManager) 
				if (triggerObj) triggerObj.GetComponent<Cutscene_TriggerScript> ().OnTriggered -= StartTimer;
			

		winState = false;

		if (!timerStarted) { // cannot start if already started

			transform.GetChild (0).gameObject.SetActive (true);	// TimerText
			transform.GetChild (1).gameObject.SetActive (true);	// Hourglass

			setActive = true;

			if (!waitForCutscene && setActive) {

				// Pointless for now, but leave in case this gets expanded
				TimerReset ();

				timeRemaining = timeDefault;

				timerStarted = true;

				// Make sure event isn't null, then cast.
				if (OnTimerStart != null)
					OnTimerStart ();
			}
		}
	}

	// END timer
	public void TimerReset(){	// doesn't fire a 'FAIL' event when ended

		// Reset event calls
		if (timerType == TIMERTYPE.BUTTON) 
			if (puzzleButton) puzzleButton.GetComponent<PuzzleButton> ().OnButtonReset -= TimerReset;

		winState = false;

		// Placeholder for other code should we need to reset specifics
		UnpauseTimer();

		timerStarted = false;

	}

	void TimerRunOut(){			// fires 'FAIL' event when ended

		timerStarted = false;

		if (!winState) {
			// Andrew was here (PuzzleFail)
			HealthManager.instance.LoseAllLives (HealthManager.AnimType.PuzzleFail);
		}

		transform.GetChild (0).gameObject.SetActive (false);	// TimerText
		transform.GetChild (1).gameObject.SetActive (false);	// Hourglass

		// change stored checkpoint on player to the appropriate location
		playerHandler.SetCheckpoint (storedCheckpoint);

		// Make sure event isn't null, then cast.
		if(OnTimerRunOut != null)
			OnTimerRunOut ();

	}
}


