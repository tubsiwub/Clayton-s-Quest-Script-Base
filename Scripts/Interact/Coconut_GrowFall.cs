using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coconut_GrowFall : MonoBehaviour {

	[Tooltip ("Life until deletion, in seconds")]
	public float lifeTime = 40.0f;

	public float growSpeed = 1;

	public float maxOverallScale = 0.01f;

	float startOverallScale = 0.001f;

	Vector3 startPosition;
	Vector3 startScale;
	Vector3 newScale;

	Transform parentObj;

	bool fullyGrown = false;

	PlayerHolder playerHolder;

	[Tooltip("When the coconut finishes growing, it gets a little bump")]
	public bool popWhenGrown = false;

	public float popForce = 400.0f;

	void Start () {

		parentObj = transform.parent;

		playerHolder = GameObject.FindWithTag ("Player").GetComponentInChildren<PlayerHolder> ();

		startOverallScale = maxOverallScale / 10;

		startPosition = transform.position;
		startScale = Vector3.one * startOverallScale;

		InitializeCoconut ();

	}

	void InitializeCoconut(){

		transform.SetParent (parentObj);

		transform.position = startPosition;
		transform.localScale = startScale;
		newScale = startScale;

		// disallow coconut from falling
		GetComponent<Rigidbody>().useGravity = false;
		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody>().velocity = Vector3.zero;

		// start script as off
		GetComponent<Pickupable>().enabled = false;

		// let script know the coconut needs to grow
		fullyGrown = false;
	}

	void Update () {

		if (!fullyGrown) {
			if (newScale.x < maxOverallScale) {

				transform.localScale = newScale;
				newScale += newScale * (growSpeed / 10) * Time.deltaTime;

			}
			else {

				fullyGrown = true;

				StartCoroutine (Lifetime (lifeTime));

				// fall from tree
				GetComponent<Rigidbody> ().useGravity = true;
				GetComponent<Rigidbody> ().isKinematic = false;

				// add force
				if (popWhenGrown) {
					GetComponent<Rigidbody> ().AddForce (Vector3.up * popForce);
				}

				// turn on pickupable script
				GetComponent<Pickupable> ().enabled = true;

			}
		}

		// disallow the player from picking up the coconut if not fully grown
		GetComponent<Pickupable> ().enabled = fullyGrown;

	}

	IEnumerator Lifetime(float lifeTotal){

		float counter = lifeTotal;

		while (counter > 0) {

			// if the player is holding it during its lifecycle, keep it alive
			if (playerHolder.IsHolding && playerHolder.HeldObject.gameObject == this.gameObject)
				counter = lifeTotal;

			counter -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		// Drop it!  DROP IT NOW!  ...only if last second pickup
		if(playerHolder.IsHolding && playerHolder.HeldObject.gameObject == this.gameObject)
			playerHolder.Drop ();

		StartCoroutine (Shrink ());

	}

	IEnumerator Shrink() {

		fullyGrown = false;

		transform.parent = parentObj;

		newScale = transform.localScale;

		while (newScale.x > startOverallScale) {

			// if the player is holding it during its lifecycle, keep it alive
			if (playerHolder.IsHolding && playerHolder.HeldObject.gameObject == this.gameObject)
				transform.localScale = newScale;
			
			transform.localScale = newScale;
			newScale -= Vector3.one * (growSpeed / 7) * Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		transform.localScale = startScale;

		InitializeCoconut ();

	}
}
