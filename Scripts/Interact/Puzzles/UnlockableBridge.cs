using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockableBridge : MonoBehaviour {

	public GameObject puzzleButton;
	public GameObject questNPC;

	Animator anim;

	public bool questObject;

	string storageKey = "";

	void Start () {

		if(puzzleButton)
			puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated += Unlock;
		
		if (questNPC)
			questNPC.GetComponent<NPC_QuestContainer> ().OnQuestStarted += Unlock;

		anim = GetComponent<Animator> ();


		if (GetComponent<SavingLoading_StorageKeyCheck> ()) {
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;

			// If this object relies on a quest, use that instead
			if (questObject) {

				if (SavingLoading.instance.LoadQuestStatus_Container (storageKey) == QUEST_STATUS.STARTED) {
					anim.SetTrigger ("Unlock");
				}

			} else {
				
				if (SavingLoading.instance.CheckStorageKeyStatus (storageKey)) {
					anim.SetTrigger ("Unlock");
				}
			}
		}

	}


	void Unlock() {

		anim.SetTrigger ("Unlock");

	}

}
