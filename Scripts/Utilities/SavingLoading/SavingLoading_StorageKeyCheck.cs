using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Give objects that utilize this event a function KeyCheck() and have this function activate when OnKeyCheck() fires.
// KeyCheck() should contain code to auto-complete or remove specific puzzle/gameObjects and/or events sans reward so the player doesn't have to redo them.

public class SavingLoading_StorageKeyCheck : MonoBehaviour {

	// Events
	public delegate void KeyCheck_Success();
	public event KeyCheck_Success OnKeyCheck;

	public string storageKey;

	void Start(){

		if (storageKey == "") {
			Debug.LogError (gameObject.name + " is missing Storage Key!  Please input a value;");
		}

	}

	// Perform check until turned off
	void Update () {

		// If the storage key is active, this event should not function as it has already been completed and saved.
		if (storageKey != "")
		if(SavingLoading.instance.CheckStorageKeyExist(storageKey))
		if (SavingLoading.instance.CheckStorageKeyStatus (storageKey))
			TurnOff ();

	}

	void TurnOff(){
		
		// Fire Event
		if (OnKeyCheck != null)
			OnKeyCheck ();
	}
}
