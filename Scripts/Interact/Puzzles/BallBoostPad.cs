using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBoostPad : MonoBehaviour {

	bool disabled = false;

	public float pushForce;

	void OnTriggerEnter(Collider col){

		if (!disabled) {
			
			if (col.transform.tag == "Player") {

				StartCoroutine (Boost (2.0f, col.transform));

			}

		}

	}

	void Update(){



	}

	IEnumerator Boost(float cooldown, Transform playerObj){

		disabled = true;

		playerObj.GetComponent<BallController> ().PushForward (pushForce, 1.0f);

		yield return new WaitForSeconds (cooldown);

		disabled = false;

	}
}
