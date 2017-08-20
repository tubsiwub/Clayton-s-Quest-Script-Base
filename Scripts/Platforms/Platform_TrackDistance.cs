// lmao look how simple this script is, look at it!
// I'm just using it for the player controller - to report how much it should move on the platform

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform_TrackDistance : MonoBehaviour
{
	Vector3 lastPos;
	public Vector3 Distance { get { return transform.position - lastPos; ; } }

	void Awake()
	{
		lastPos = transform.position;
	}

	void LateUpdate()
	{
		lastPos = transform.position;
	}
}
