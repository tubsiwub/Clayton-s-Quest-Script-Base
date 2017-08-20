using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SPAWNTYPE {

	TIMER, BUTTON, QUEST

}

public class SpawnItem_Enabler : MonoBehaviour {

	public SPAWNTYPE spawnType;

	GameObject timerActivator;
	public GameObject puzzleButton;
	public GameObject questNPC;
	public GameObject cutscene;

	// inspector bool
	public bool useWinZone;
	public bool useCutscene;

	public  List<GameObject> childObjs;
	List<Vector3> childPositions;

	public GameObject winZone;

	bool winState = false;

	public bool itemPermanence;

	void Awake () {

		timerActivator = GameObject.Find ("TimerUI");

		if(spawnType == SPAWNTYPE.TIMER)	{
			// Set the function as an event
			timerActivator.GetComponent<ActivatedTimer> ().OnTimerStart += ShowItem;
			timerActivator.GetComponent<ActivatedTimer> ().OnTimerRunOut += RemoveItem;
		}
		if(spawnType == SPAWNTYPE.BUTTON)	{
			if (!useCutscene) {
				puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated += ShowItem;
				puzzleButton.GetComponent<PuzzleButton> ().OnButtonReset += RemoveItem;
			} else {
				cutscene.GetComponent<ScriptStateManager> ().OnCutsceneCancel += ShowItem;
				puzzleButton.GetComponent<PuzzleButton> ().OnButtonReset += RemoveItem;
			}
		}
		if(spawnType == SPAWNTYPE.QUEST)	{
			questNPC.GetComponent<NPC_QuestContainer> ().OnQuestStarted += ShowItem;
			questNPC.GetComponent<NPC_QuestContainer> ().OnQuestFailed += RemoveItem;
		}
			

		// If you reach the winZone, it'll fire an event you'll receive here
		if (winZone != null) {
			winZone.GetComponent<WinZone> ().OnWinZoneActivate += Complete;
		}

		childObjs = new List<GameObject> ();
		childPositions = new List<Vector3> ();

		for (int i = 0; i < this.transform.childCount; i++) {
			
			childObjs.Add (this.transform.GetChild (i).gameObject);
			childPositions.Add (this.transform.GetChild (i).position);

			// Start the items out as invisible / disabled
			StartItemDisabled ();
		}

	}

	void Complete(){

		winState = true;

	}

	void ShowItem(){
		
		for(int i = 0; i < this.transform.childCount; i++){

			if (childObjs [i]) {
				childObjs [i].SetActive (true);
				childObjs [i].transform.position = childPositions [i];
			}

		}

	}

	// Same as RemoveItem, but works without any filters
	void StartItemDisabled(){

		// Start objects out as disabled
		foreach (Transform child in this.transform) {

			if(child)
				child.gameObject.SetActive (false);

		}

	}

	void RemoveItem(){

		// Prevent objects from being disabled if conditions are met
		if(!winState && !itemPermanence)
		foreach (Transform child in this.transform) {

			if(child)
				child.gameObject.SetActive (false);

		}

	}
}

