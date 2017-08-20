using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TYPE {

	CHARACTER,
	MOVING_PLATFORM

}

public class CutsceneComponent : MonoBehaviour {

	Vector3 startPosition;
	Quaternion startRotation;
	Vector3 startScale;

	public TYPE componentType;

	[Tooltip("Time delay before platform starts moving.  Use to stagger events.")]
	public float delayTime = 2.0f;

	void Awake () {
		startPosition = transform.localPosition;
		startRotation = transform.localRotation;
		startScale = transform.localScale;
	}

	public void INITIALIZE(){
		transform.localPosition = startPosition;
		transform.localRotation = startRotation;
		transform.localScale = startScale;

		switch (componentType) {

		case TYPE.CHARACTER:

			GetComponent<Character_Actions> ().INITIALIZE ();

			break;

		case TYPE.MOVING_PLATFORM:

			GetComponent<MovingPlatform_Actions> ().INITIALIZE (delayTime);

			break;
	
		}
	}
}
