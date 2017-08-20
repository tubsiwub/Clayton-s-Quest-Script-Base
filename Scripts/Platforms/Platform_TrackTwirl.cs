using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform_TrackTwirl : TrackTwirlBase
{
	Quaternion lastRot;
	float rotation;

	public enum AXIS { RIGHT, UP, FORWARD };
	[SerializeField] AXIS axis = AXIS.UP;

	public float Rotation { get { return rotation; } }
	public Vector3 Axis { get { return GetAxis(axis); } }

	public override Platform_TrackTwirl Get()
	{
		return this;
	}

	Vector3 GetAxis(AXIS axis)
	{
		switch (axis)
		{
			case AXIS.RIGHT: return Vector3.right;
			case AXIS.UP: return Vector3.up;
			case AXIS.FORWARD: return Vector3.forward;
			
			default: return Vector3.zero;
		}
	}

	void Update()
	{
		//GetAxis();
		Vector3 vecAxis = GetAxis(axis);

		// get a "forward vector" for each rotation
		Vector3 forwardA = lastRot * ((vecAxis == Vector3.forward) ? Vector3.right : Vector3.forward);
		Vector3 forwardB = transform.rotation * ((vecAxis == Vector3.forward) ? Vector3.right : Vector3.forward);
		float angleA, angleB;

		if (vecAxis == Vector3.right)
		{
			angleA = Mathf.Atan2(forwardA.z, forwardA.y) * Mathf.Rad2Deg;
			angleB = Mathf.Atan2(forwardB.z, forwardB.y) * Mathf.Rad2Deg;
		}
		else if (vecAxis == Vector3.up)
		{
			// get a numeric angle for each vector, on the X-Z plane (relative to world forward)
			angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
			angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;
		}
		else // Vector3.forward
		{
			angleA = Mathf.Atan2(forwardA.y, forwardA.x) * Mathf.Rad2Deg;
			angleB = Mathf.Atan2(forwardB.y, forwardB.x) * Mathf.Rad2Deg;
		}

		// get the signed difference in these angles
		rotation = Mathf.DeltaAngle(angleA, angleB);

		lastRot = transform.rotation;
	}
}
