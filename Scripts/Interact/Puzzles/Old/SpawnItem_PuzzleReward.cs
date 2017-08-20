using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PUZZLETYPE{

	BOWLING

}

public class SpawnItem_PuzzleReward : MonoBehaviour {

	public PUZZLETYPE puzzleType = PUZZLETYPE.BOWLING;

	public GameObject puzzleObject;

	void Start () {

		if (puzzleType == PUZZLETYPE.BOWLING) {

			puzzleObject.GetComponent<BowlingEvent> ().OnBowlingSuccess += SpawnItem;

		}

		foreach (Transform obj in this.transform)
			obj.transform.gameObject.SetActive (false);

	}

	void SpawnItem(){

		foreach (Transform obj in this.transform)
			obj.transform.gameObject.SetActive (true);

	}
}
