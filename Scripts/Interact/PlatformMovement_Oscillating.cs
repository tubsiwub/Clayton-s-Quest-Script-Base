using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement_Oscillating : MonoBehaviour {

	public Vector3 pointA, pointB;

	public float moveSpeed;

	bool movingToA = false;

	void Update () {

		if (movingToA)
		if (Vector3.Distance(transform.localPosition, pointA) > 0.01f) {
			transform.localPosition = Vector3.MoveTowards (transform.localPosition, pointA, moveSpeed * Time.deltaTime);
		} else {
			transform.localPosition = pointA;
			movingToA = false;
		}

		if (!movingToA)
		if (Vector3.Distance(transform.localPosition, pointB) > 0.01f) {
			transform.localPosition = Vector3.MoveTowards (transform.localPosition, pointB, moveSpeed * Time.deltaTime);
		} else {
			transform.localPosition = pointB;
			movingToA = true;
		}

	}
}
