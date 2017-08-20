// Tristan

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class InteractPopup_DistanceCheck : MonoBehaviour {


	// Events
	public delegate void DialogueFinish_Action();
	public event DialogueFinish_Action OnDialogueFinish;	// - fire when completed

	NPC_QuestContainer questNPCScript;

	Transform parentObj;

	XML_Reader xmlReader;

	Dialogue_Popup dialoguePopup;

	GameObject playerObj;

	PlayerHolder playerHolder;

	bool showingButtonHintText = false;
	ButtonHintText buttonHintText;

	// Misleading:  This is the name read in from the XML doc
	public string NPCName = "";

	List<string> storedDialogue;

	int dialogueCounter = 0;
	public int DialogueCounter{get{ return dialogueCounter; }set{ dialogueCounter = value;}}

	Transform parentWithComponent;	// questNPC of this popup
	Texture questNone, questDone, questStart;

	bool 
	animate,
	NPC_FREEZE = false;

	// This prevents other NPCs from turning off the dialogue boxes.
	bool targetedByPlayer = false;

	void Awake () {

		playerHolder = GameObject.FindWithTag ("Player").GetComponentInChildren<PlayerHolder> ();

		dialoguePopup = GameObject.Find ("DialogueCanvas/DialoguePanel/DialogueBG").GetComponent<Dialogue_Popup> ();

		// NPC Object
		parentObj = this.transform.parent.parent;

		playerObj = GameObject.FindWithTag ("Player");

		animate = false;
		GetComponent<MeshRenderer> ().enabled = false;

		xmlReader = XML_Reader.getInstance ();

		parentWithComponent = this.transform;
		parentWithComponent = MathFunctions.FindParentWithComponent (this.transform, "NPC_QuestContainer");

		// Quest pop-up status
		questNone = Resources.Load<Texture> ("npcSpeech");
		questDone = Resources.Load<Texture> ("npcSpeechQuestDone");
		questStart = Resources.Load<Texture> ("npcSpeechQuestStart");

		ResetStoredDialogue ();
	}

	void Start() {

		buttonHintText = GameObject.Find("ButtonHintText").GetComponent<ButtonHintText>();

	}


	// When the dialogue changes, reset it here
	public void ResetStoredDialogue(){

		storedDialogue = new List<string> ();

		// store specific dialogue in THIS NPC.  Make sure NPCName is set correctly in inspector.
		for (int i = 0; i < xmlReader.GetDialogueText (NPCName).Length; i++) {
			if (xmlReader.GetDialogueText (NPCName) [i] != null) {
				storedDialogue.Add (xmlReader.GetDialogueText (NPCName) [i]);
			}
		}

	}

	// Reset all dialogue stuff - constantly updated
	void ResetDialogue(){

		dialogueCounter = 0;
		NPC_FREEZE = false;
		targetedByPlayer = false;

	}

	void FREEZE(bool enable){

		if(parentObj.GetComponent<NavMeshAgent> ())
			parentObj.GetComponent<NavMeshAgent> ().enabled = !enable;

		if (parentObj.GetComponent<NPC_Behavior> ()) {
			if (parentObj.GetComponent<NPC_Behavior> ().npcType == NPCTYPE.QUEST) {

				if(enable)
					parentObj.GetComponent<NPC_Behavior> ().currentBehavior = BEHAVIORS.TALK;
				else 
					parentObj.GetComponent<NPC_Behavior> ().currentBehavior = BEHAVIORS.IDLE;
			
			} else {

				//parentObj.GetComponent<NPC_Behavior> ().enabled = !enable;

			}
		}

		if (enable) {

			parentObj.GetComponent<NPC_Behavior> ().ChangeDialogueStatus (true);
			parentObj.GetComponent<NPC_Behavior> ().currentBehavior = BEHAVIORS.TALK;


		} else {

			parentObj.GetComponent<NPC_Behavior> ().ChangeDialogueStatus (false);

		}

	}

	void Update () {

		// Stop the NPC from performing movement or actions
		FREEZE (NPC_FREEZE);

		// Change popUp based on quest status
		if (parentWithComponent != this.transform && parentWithComponent.GetComponent<NPC_QuestContainer> ()) {
			if (parentWithComponent.GetComponent<NPC_QuestContainer> ().QuestStatus == QUEST_STATUS.NEUTRAL) {
				GetComponent<Renderer> ().material.mainTexture = questDone;
			} else if (parentWithComponent.GetComponent<NPC_QuestContainer> ().QuestStatus == QUEST_STATUS.STARTED) {
				GetComponent<Renderer> ().material.mainTexture = questDone;
			} else if (parentWithComponent.GetComponent<NPC_QuestContainer> ().QuestStatus == QUEST_STATUS.FINISHED) {
				GetComponent<Renderer> ().material.mainTexture = questStart;
			} else {
				GetComponent<Renderer> ().material.mainTexture = questNone;
			}
		}
		else 
		{
			GetComponent<Renderer> ().material.mainTexture = questNone;
		}

		if(parentWithComponent.GetComponent<NPC_QuestContainer> ())
		if (!animate && parentWithComponent.GetComponent<NPC_QuestContainer> ().QuestStatus == QUEST_STATUS.FINISHED) {
			GetComponent<Animator> ().Play ("InteractPopup_appear");

			GetComponent<MeshRenderer> ().enabled = true;
			animate = true;
		}

		if (Vector3.Distance (transform.position, playerObj.transform.position) < 3 && playerObj.transform.position.y < this.transform.position.y
			&& !playerHolder.IsHolding && !playerHolder.HasObjectsNearby) {

			if (!animate && !dialoguePopup.GetLockStatus ()) {
				if (parentWithComponent.GetComponent<NPC_QuestContainer> ()) {
					if (parentWithComponent.GetComponent<NPC_QuestContainer> ().QuestStatus != QUEST_STATUS.FINISHED) {
						GetComponent<Animator> ().Play ("InteractPopup_appear");

						GetComponent<MeshRenderer> ().enabled = true;
						animate = true;
					}
				} else {
					GetComponent<Animator> ().Play ("InteractPopup_appear");

					GetComponent<MeshRenderer> ().enabled = true;
					animate = true;
				}
			}
			else if(!animate && dialoguePopup.GetLockTarget () == this.gameObject || dialoguePopup.GetLockTarget () == null){

				GetComponent<MeshRenderer> ().enabled = true;
				animate = true;
			}

			dialoguePopup.AddNPC (this.gameObject);	// adds to list if not currently in list

		} else {

			dialoguePopup.RemoveNPC (this.gameObject);

			if (dialogueCounter >= storedDialogue.Count - 1) {
				
				if(dialoguePopup.GetLockStatus() && dialoguePopup.GetLockTarget() == this.gameObject)
					FinishDialogue ();

			} else {
				
				ResetDialogue ();

			}

			if (parentWithComponent.GetComponent<NPC_QuestContainer> ()) {
				if (parentWithComponent.GetComponent<NPC_QuestContainer> ().QuestStatus != QUEST_STATUS.FINISHED) {
					GetComponent<MeshRenderer> ().enabled = false;
					animate = false;
				}
			} else {
				GetComponent<MeshRenderer> ().enabled = false;
				animate = true;
			}

		}

		if (dialoguePopup.GetLockStatus () && dialoguePopup.GetLockTarget () == this.gameObject) {
			
			if (Vector3.Distance (transform.position, playerObj.transform.position) >= 3) {

				dialoguePopup.LockDialogue (false);
				ResetDialogue ();

			}

		}

		// While you're close to the NPC...
		if (Vector3.Distance (transform.position, playerObj.transform.position) < 3) {

			if ((!dialoguePopup.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsTag ("painting"))// this is a mess, but it prevents the dialogue from 
			    && !dialoguePopup.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsTag ("done")) {
				if (!showingButtonHintText) {

					buttonHintText.ShowNPCText ();
					showingButtonHintText = true;

				}
			} else {
				if (showingButtonHintText) {

					buttonHintText.CancelNPCText();
					showingButtonHintText = false;

				}
			}


			// is this npc the one closest?  is the list of NPCs filled?
			if (dialoguePopup.GetLockStatus ()) {

				if (dialoguePopup.GetLockTarget () == this.gameObject)
					DialogueActions ();

			} else {
				if (dialoguePopup.GetNearestNPC () == this.gameObject) {	

					DialogueActions ();

				} else {

					if (dialoguePopup.GetLockTarget () != this.gameObject) {
						GetComponent<MeshRenderer> ().enabled = false;

					}

					if (!dialoguePopup.GetLockStatus ()) {
						
						ResetDialogue ();
					}
				}
			}
		} else {

			if (showingButtonHintText) {

				buttonHintText.CancelNPCText();
				showingButtonHintText = false;

			}

		}

		if (Input.GetButtonDown ("Cancel")) {

			FinishDialogue ();
		}

	}

	// Clears dialogue for this NPC and the main Dialogue Canvas
	void FinishDialogue(){

		dialoguePopup.LockDialogue (false);

		dialoguePopup.TurnOffTextbox ();

		ResetDialogue ();
		dialoguePopup.ResetDialogue ();

	}

	void DialogueActions(){
		
		// when we switch to a new NPC, reset the dialogue
		if (!targetedByPlayer) {

			if(!dialoguePopup.GetLockStatus())	// if we aren't locked, reset stuff
				FinishDialogue ();

			targetedByPlayer = true;
		}

		// INTERACT KEY - Debug: E
		if (Input.GetButtonDown ("Interact") && !playerHolder.IsHolding && !playerHolder.HasObjectsNearby) 
		{
			if((!dialoguePopup.GetComponent<Animator> ().GetCurrentAnimatorStateInfo(0).IsTag("painting") && NPC_FREEZE) // this is a mess, but it prevents the dialogue from 
				|| !dialoguePopup.GetComponent<Animator> ().GetCurrentAnimatorStateInfo(0).IsTag("done")) 				 //		printing during the background animation
			if (dialoguePopup.GetLockTarget () == this.gameObject || dialoguePopup.GetLockTarget () == null) {	// make sure another NPC isn't the lock target

				NPC_FREEZE = true;
				parentObj.GetComponent<NPC_Behavior> ().SetTarget (playerObj.transform);

				// ...this causes the popup to jitter slightly.  Not an issue...
				Camera.main.GetComponent<CameraControlDeluxe> ().SetLookTarget (this.transform.position, 444 * Time.deltaTime);

				if (!dialoguePopup.GetLockStatus ()) {
					dialoguePopup.LockDialogue (true);
					dialoguePopup.SetLockTarget (this.gameObject);
				}

				// If the dialogue has finished being typed out - else, hitting 'E' does nothing
				if (dialoguePopup.DialogueFinished) {

					// If we still have dialogue to get through
					if (dialogueCounter < storedDialogue.Count) {	

						string npcName = parentObj.GetComponent<NPC_Behavior> ().NPCName;

						dialoguePopup.DialogueBoxAnimation (240, storedDialogue [dialogueCounter], dialogueCounter + 1, storedDialogue.Count, npcName);
						dialogueCounter++;

					} else {

						// FIRE EVENT
						if (OnDialogueFinish != null)
							OnDialogueFinish ();

						FinishDialogue ();

					}
				}
			}
		}
	}
}
