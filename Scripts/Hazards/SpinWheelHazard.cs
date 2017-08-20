using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DIRECTION {

	UP,DOWN,LEFT,RIGHT

}

public class SpinWheelHazard : MonoBehaviour {

	public float pushForce = 50.0f;

	public DIRECTION direction = DIRECTION.DOWN;

	[Tooltip("Measured in seconds")]
	public float startDelay = 0; //in seconds

	void Start(){

		StartCoroutine (DelayStart (startDelay));

	}

	IEnumerator DelayStart(float delay){

		float counter = delay;

		while (counter > 0.0f) {
			
			counter -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		yield return new WaitForSeconds (delay);

		transform.parent.GetComponent<Animator> ().SetTrigger ("Start");

	}

	void Update(){

		#if UNITY_EDITOR
		switch(direction){
		case DIRECTION.UP:
			Debug.DrawRay(transform.position, transform.up * 8, Color.green);
			break;
		case DIRECTION.DOWN:
			Debug.DrawRay(transform.position, -transform.up * 8, Color.green);
			break;
		case DIRECTION.LEFT:
			Debug.DrawRay(transform.position, -transform.right * 8, Color.green);
			break;
		case DIRECTION.RIGHT:
			Debug.DrawRay(transform.position, transform.right * 8, Color.green);
			break;
		}
		#endif

	}

	void OnTriggerEnter(Collider col){

		if (col.transform.tag == "Player") {

			switch(direction){
			case DIRECTION.UP:
				col.GetComponent<PlayerHandler> ().PushAway (transform.up, pushForce);
				break;
			case DIRECTION.DOWN:
				col.GetComponent<PlayerHandler> ().PushAway (-transform.up, pushForce);
				break;
			case DIRECTION.LEFT:
				col.GetComponent<PlayerHandler> ().PushAway (-transform.right, pushForce);
				break;
			case DIRECTION.RIGHT:
				col.GetComponent<PlayerHandler> ().PushAway (transform.right, pushForce);
				break;
			}

		}

	}

}
