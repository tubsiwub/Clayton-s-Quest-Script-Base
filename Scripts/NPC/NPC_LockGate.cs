using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Place this on gates that open based on saved NPC count

public class NPC_LockGate : MonoBehaviour {

	GameObject playerObj;

	public int npcOpenCount;

	public Vector3 openPosition, closePosition;

	bool gateUnlock = false;
	bool gateFullyOpen = false;

	string storageKey = "";

	void Start(){

		playerObj = GameObject.FindWithTag ("Player");

		// Event Triggers
		if (GetComponent<SavingLoading_StorageKeyCheck> ()) {
			GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck += KeyCheck;
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;
		}
	}

	void Update () {

		if (!gateUnlock) 
		{
			if (Vector3.Distance (playerObj.transform.position, this.transform.position) < 3) 
			{
				if (SavingLoading.instance.GetNPCCount () >= npcOpenCount)
				{
					gateUnlock = true;
					GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
					SavingLoading.instance.SaveStorageKey (storageKey, true);
				}
			} 
			else 
			{
				transform.position = Vector3.MoveTowards (transform.position, closePosition, 2 * Time.deltaTime);
				if (Vector3.Distance (closePosition, this.transform.position) < 1)
				{
					transform.position = closePosition;
					gateFullyOpen = false;
				}
			}
		} 
		else 
		{
			if (!gateFullyOpen) 
			{
				transform.position = Vector3.MoveTowards (transform.position, openPosition, 2 * Time.deltaTime);
				if (Vector3.Distance (openPosition, this.transform.position) < 1)
				{
					transform.position = openPosition;
					gateFullyOpen = true;
				}
			}
		}
	}

	// If storageKey marks this as completed, perform these actions
	void KeyCheck()
	{
		Destroy (this.gameObject);	// simple, effective, deadly.
	}
}
