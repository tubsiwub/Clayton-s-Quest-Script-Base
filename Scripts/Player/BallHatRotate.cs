using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHatRotate : MonoBehaviour
{
	[SerializeField] Transform referencePoint;
	[SerializeField] Vector3 referenceOffset;
	[SerializeField] BallController ballController;

	Quaternion lastValidRot = Quaternion.identity;
	PlayerHandler playerHandler;

	void Awake()
	{
		playerHandler = ballController.gameObject.GetComponent<PlayerHandler>();
		PlayerHandler.OnHumanToBallFinish += SetRotation;
	}

	private void RemoveEvents()
	{
		PlayerHandler.OnHumanToBallFinish -= SetRotation;
	}

	void OnDestroy()
	{
		RemoveEvents();
	}

	void OnDisable()
	{
		RemoveEvents();
	}

	void SetRotation()
	{
		transform.rotation = Quaternion.LookRotation(playerHandler.RotateMesh.forward);
		lastValidRot = transform.rotation;
	}

	void LateUpdate()
	{
		if (referencePoint != null)
			transform.position = referencePoint.position + referenceOffset;

		Vector3 dist = ballController.Distance.normalized; dist.y = 0;

		if (dist != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(dist);
			lastValidRot = transform.rotation;
		}
		else transform.rotation = lastValidRot;
	}
}
