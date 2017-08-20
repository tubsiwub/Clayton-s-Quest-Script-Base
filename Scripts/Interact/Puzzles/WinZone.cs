// Tristan

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WINZONE_TYPE {

	PUZZLE, QUEST, BALLQUEST

}


public class WinZone : MonoBehaviour {

	public WINZONE_TYPE winZoneType;

	// Events
	public delegate void WinZone_Activate();
	public event WinZone_Activate OnWinZoneActivate;

	public bool useTimer = true;

	GameObject timerObj;

	bool puzzleActive = false;

	void Start () {

		// Event Triggers
		if(GetComponent<SavingLoading_StorageKeyCheck> ())
			GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck += KeyCheck;

		timerObj = GameObject.Find ("TimerUI");

		PuzzleOFF ();

	}

	void Update(){

	}

	public void PuzzleON(){
		
		if (useTimer) {
			timerObj.GetComponent<ActivatedTimer> ().OnTimerStart += PuzzleON;
			timerObj.GetComponent<ActivatedTimer> ().OnTimerRunOut += PuzzleOFF;
		}

		puzzleActive = true;

		GetComponent<Collider> ().enabled = true;

		if (transform.childCount > 0)
			transform.GetChild (0).gameObject.SetActive (true);
		
	}

	public void PuzzleOFF(){
		
		puzzleActive = false;

		GetComponent<Collider> ().enabled = false;

		if (transform.childCount > 0)
			transform.GetChild (0).gameObject.SetActive (false);
		
	}

	void OnTriggerEnter(Collider col){

		if (!useTimer || winZoneType != WINZONE_TYPE.PUZZLE)
			puzzleActive = true;

		if(winZoneType == WINZONE_TYPE.QUEST || winZoneType == WINZONE_TYPE.PUZZLE)
		if (col.transform.tag == "Player" && puzzleActive) {

			if (OnWinZoneActivate != null)
				OnWinZoneActivate ();

			SaveStorageKey ();

		}


		if(winZoneType == WINZONE_TYPE.BALLQUEST)
		if (col.transform.tag == "PushBall" && puzzleActive) {

			if (OnWinZoneActivate != null)
				OnWinZoneActivate ();

			SaveStorageKey ();

		}

	}

	// If setting is true, enable progress and save to file
	void SaveStorageKey(){

		// Make sure this isn't a quest zone
		if (winZoneType == WINZONE_TYPE.PUZZLE) {
			
			// Store key - Quests don't use this specific key
			if (GetComponent<SavingLoading_StorageKeyCheck> ())
				SavingLoading.instance.SaveStorageKey (GetComponent<SavingLoading_StorageKeyCheck> ().storageKey, true);

			// Save game progress
			SavingLoading.instance.SaveData ();
		}

	}

	// If storageKey marks this as completed, perform these actions
	void KeyCheck(){
		GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
		this.gameObject.SetActive (false);
	}
}
