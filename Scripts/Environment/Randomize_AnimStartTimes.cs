using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomize_AnimStartTimes : MonoBehaviour {

	public string animationStateName;

	Animator anim;

	void Start() {

		anim = GetComponent<Animator> ();

		anim.Play (animationStateName, 0, GetRandomNumber ());

	}

	float GetRandomNumber(){

		float animLength = anim.GetCurrentAnimatorStateInfo (0).length * Application.targetFrameRate;

		float seg1 = transform.position.x + transform.position.y + transform.position.z;
		float seg2 = transform.rotation.x + transform.rotation.y + transform.rotation.z;
		float seg3 = transform.localScale.x + transform.localScale.y + transform.localScale.z;
		float final = (((Random.Range(0,500) * (seg1 - seg2)) * (seg3 + seg1)) - seg2/seg1) % animLength;

		final = Mathf.Abs (final) % 1.0f;

		return final;

	}

}
