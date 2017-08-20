using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwirlingPlatform : MonoBehaviour
{
	float rotation;
	Vector3 direction;

	void Start()
	{
		rotation = 25;
		direction = Vector3.up;
	}

	void Update()
	{
		if (direction == Vector3.right)
			transform.Rotate(rotation * Time.deltaTime, 0, 0);

		if (direction == Vector3.up)
			transform.Rotate(0, rotation * Time.deltaTime, 0);

		if (direction == Vector3.forward)
			transform.Rotate(0, 0, rotation * Time.deltaTime);
	}

	public float GetRotation()
	{
		return rotation * Time.deltaTime;
	}

	public Vector3 GetDirection()
	{
		return direction;
	}
}
