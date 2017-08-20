using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMeshLocation : MonoBehaviour
{
	[SerializeField] Transform footRayPoint;
	[SerializeField] Transform buttRayPoint;

	PlayerHandler playerHandler;
	HumanCollider humanCollider;

	Vector3 targetPos;
	const float moveSpeed = 20;

	void Start()
	{
		playerHandler = GetComponentInParent<PlayerHandler>();
		humanCollider = GetComponentInParent<HumanCollider>();
		targetPos = Vector3.zero;
	}

	void OnDisable()
	{
		targetPos = Vector3.zero;
		transform.localPosition = Vector3.zero;
	}

	void Update()
	{
		Vector3 rayPoint = Vector3.zero;
		float rayLength = 0;

		if (playerHandler.CurrentState == PlayerHandler.PlayerState.Human)
		{
			rayPoint = footRayPoint.position;
			rayLength = 0.7f;
		}
		else if (playerHandler.CurrentState == PlayerHandler.PlayerState.Slider)
		{
			rayPoint = buttRayPoint.position;
			rayLength = 1.4f;
		}

		RaycastHit hitInfo;
		Ray ray = new Ray(rayPoint + Vector3.up * 0.1f, Vector3.down);
		if (Physics.Raycast(ray, out hitInfo, rayLength, -1, QueryTriggerInteraction.Ignore))
		{
			float distDown = (hitInfo.point.y + rayLength) - playerHandler.transform.position.y;

			// if we hit the same collider the humancollider did, OR our new position will be higher than the last position
			if (hitInfo.collider == humanCollider.Collided || distDown > transform.localPosition.y)
			{
				Vector3 dist = hitInfo.point - rayPoint;
				dist.x = 0; dist.z = 0;
				targetPos = dist;
			} else targetPos = Vector3.zero;
		}
		else targetPos = Vector3.zero;

		Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.green);
		transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, moveSpeed * Time.deltaTime);
	}
}
