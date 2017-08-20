using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeInPlace : MonoBehaviour
{
	Vector3 startPos;
	Quaternion startRot;

	void Start()
	{
		startPos = transform.localPosition;
		startRot = transform.localRotation;
	}
	
	void Update()
	{
		transform.localPosition = startPos;
		transform.localRotation = startRot;
	}

	void LateUpdate()
	{
		transform.localPosition = startPos;
		transform.localRotation = startRot;
	}
}
