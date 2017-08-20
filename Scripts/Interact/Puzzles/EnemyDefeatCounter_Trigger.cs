using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDefeatCounter_Trigger : MonoBehaviour {


	// Events
	public delegate void EnemyDefeat_Triggered();
	public event EnemyDefeat_Triggered OnTriggered;


	public List<GameObject> enemyList;

	public bool triggerFinish = false;

	string storageKey = "";

	void Start () {

		// Save Data Check
		if(GetComponent<SavingLoading_StorageKeyCheck>()){
			
			storageKey = GetComponent<SavingLoading_StorageKeyCheck>().storageKey;

			if(SavingLoading.instance.CheckStorageKeyStatus(storageKey))
				triggerFinish = true;
		}

	}

	void Update () {

		for (int i = 0; i < enemyList.Count; i++) {
			if (enemyList [i] == null)
				enemyList.RemoveAt (i);
		}

		if (enemyList.Count <= 0 && !triggerFinish) {
			
			triggerFinish = true;

			if (OnTriggered != null)
				OnTriggered ();

			SavingLoading.instance.SaveStorageKey (storageKey, true);
		}
	}
}
