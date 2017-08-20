using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadLookAt : MonoBehaviour
{
	Transform lookAtPoint;
	public bool IsLooking { get { return lookAtPoint != null; } }

	Vector3 rotateOffset = new Vector3(0, 0, -90);
	Quaternion lastRotation;

	const float lerpSpeed = 15;
	const int castLayer = -257;
	const int updateFrames = 15;

	void Start()
	{
		
	}

	void Update()
	{
		if (Time.frameCount % updateFrames == 0)
			lookAtPoint = FindLookAt();
	}

	Transform FindLookAt()
	{
		const float maxSendDist = 0.01f;    // don't shoot the "ray" very far, we want sphere on us
		const float sphereRadius = 5;
		RaycastHit[] hits = Physics.SphereCastAll(new Ray(transform.position, Vector3.forward * maxSendDist),
			sphereRadius, maxSendDist, castLayer);

		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].collider.GetComponent<LookAtThis>())
				return hits[i].collider.gameObject.transform;
		}

		return null;
	}

	void LateUpdate()
	{
		Quaternion rot = transform.rotation;
		float dot = 0;

		if (lookAtPoint)
			dot = Vector3.Dot(transform.forward, (lookAtPoint.position - transform.position).normalized);

		if (dot >= 0.25f)
		{
			rot = Quaternion.LookRotation(lookAtPoint.position - transform.position);
			rot *= Quaternion.Euler(rotateOffset);
		}

		if (Quaternion.Angle(lastRotation, rot) < 160)
			transform.rotation = Quaternion.Slerp(lastRotation, rot, lerpSpeed * Time.deltaTime);
		
		lastRotation = transform.rotation;
	}
}
