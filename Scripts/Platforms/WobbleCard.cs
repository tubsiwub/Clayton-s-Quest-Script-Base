using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobbleCard : MonoBehaviour {


	Transform wobbleCard;

	BoxCollider colliderComponent;

	Vector3 originPos;

	bool falling = false;

	public float timeToFall = 2.0f;


	void Start(){

		colliderComponent = GetComponent<BoxCollider> ();
		originPos = transform.position;

		wobbleCard = transform.GetChild (0);

	}

	void OnTriggerEnter(Collider col){

		if (col.transform.tag == "Player") {
			
			wobbleCard.GetComponent<Animator> ().SetBool ("Wobble", true);

			if(!falling)
				StartCoroutine (StartFall (timeToFall));

		}

	}

	float globalTimer = 2.0f;
	IEnumerator StartFall(float timer){

		// PLATFORM FALLS

		falling = true;

		yield return new WaitForSeconds (timer);

		wobbleCard.GetComponent<Animator> ().SetBool ("Wobble", false);

		colliderComponent.enabled = false;

		while (globalTimer > 0) {

			globalTimer -= Time.deltaTime;

			transform.position -= Vector3.up * 0.35f;

			yield return new WaitForEndOfFrame ();

		}
			
		globalTimer = 2.0f;

		StartCoroutine (ResetTimer (2.0f));

	}

	IEnumerator ResetTimer(float timer){

		// COOLDOWN

		yield return new WaitForSeconds (timer);

		falling = false;

		colliderComponent.enabled = true;

		transform.position = originPos;

	}
}
