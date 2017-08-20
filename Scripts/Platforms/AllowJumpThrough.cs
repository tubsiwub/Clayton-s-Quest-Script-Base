using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllowJumpThrough : MonoBehaviour {

	GameObject playerObj;

	void Start () {
		playerObj = GameObject.FindWithTag ("Player");
	}


	void Update () {

		Vector3 playerPosition;

		// Check where the player is currently based on current state
		if(playerObj.GetComponent<PlayerHandler>().CurrentState == PlayerHandler.PlayerState.Human)
			playerPosition = playerObj.transform.position - Vector3.up * 0.5f;
		else 
			playerPosition = playerObj.transform.position;

		// If the player is a specific position above or below the platform, enable/disable collision
		if (playerPosition.y < transform.position.y) {
			foreach (Collider col in this.transform.GetComponents<Collider>()) {
				if(!col.isTrigger)
					GetComponent<Collider> ().enabled = false;
			}
		} else {
			foreach (Collider col in this.transform.GetComponents<Collider>()) {
				if(!col.isTrigger)
					GetComponent<Collider> ().enabled = true;
			}
		}
	}
}
