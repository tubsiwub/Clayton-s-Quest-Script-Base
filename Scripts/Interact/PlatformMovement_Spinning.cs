using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement_Spinning : MonoBehaviour {

	public Vector3 spinDirection;

	void Update () {

		transform.Rotate (spinDirection * Time.deltaTime);

	}
}
