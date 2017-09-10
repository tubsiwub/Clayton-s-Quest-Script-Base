using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Purpose:  Plays once on first quest complete, then destroys itself

public class AreaCutscene : MonoBehaviour {

	public List<NPC_QuestContainer> questTypeList;

	public ScriptStateManager scriptStateManager;

	string storageKey = "";

	void Start () 
	{
		scriptStateManager.OnCutsceneCancel += SetInactive;

		if (GetComponent<SavingLoading_StorageKeyCheck> ())
		{
			GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck += KeyCheck;
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;
		}

		for (int i = 0; i < questTypeList.Count; i++)
		{
			questTypeList [i].OnQuestCompleteDialogue += ShowFirstCutscene;
		}
	}

	void KeyCheck()
	{
		SetInactive ();

		GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
	}

	void ShowFirstCutscene()
	{
		for (int i = 0; i < questTypeList.Count; i++)
		{
			questTypeList [i].OnQuestCompleteDialogue -= ShowFirstCutscene;
		}

		StartCoroutine (WaitAndShowCutscene (1.0f));
	}

	void SetInactive()
	{
		scriptStateManager.OnCutsceneCancel -= SetInactive;

		SavingLoading.instance.SaveStorageKey (storageKey, true);

		this.gameObject.SetActive (false);
	}

	IEnumerator WaitAndShowCutscene(float waitTime)
	{
		yield return new WaitForSeconds (waitTime);

		scriptStateManager.TriggerCutscene ();
	}
}
