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

	void Start(){

		playerObj = GameObject.FindWithTag ("Player");

	}

	void Update () {

		if (!gateUnlock) {
			if (Vector3.Distance (playerObj.transform.position, this.transform.position) < 3) {

				if (NPC_Manager.instance.GetStoredNameCount >= npcOpenCount) {

					gateUnlock = true;

				} 

			} else {

				transform.position = Vector3.MoveTowards (transform.position, closePosition, 2 * Time.deltaTime);

			}
		} else {

			if (!gateFullyOpen) {
				transform.position = Vector3.MoveTowards (transform.position, openPosition, 2 * Time.deltaTime);
				if (Vector3.Distance (openPosition, this.transform.position) < 1){
					transform.position = openPosition;
					gateFullyOpen = true;
				}
			}

		}


	}

}
