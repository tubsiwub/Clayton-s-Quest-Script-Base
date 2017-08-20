using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepGlobalRotation : MonoBehaviour
{
	[SerializeField] bool onPlayer = true;

	Quaternion startRot;
	PlayerHandler playerHandler;
	float playerOffset;
	const float ballOffset = 0.6f;

	void Start()
	{
		startRot = transform.localRotation;

		if (onPlayer)
		{
			playerHandler = transform.parent.GetComponent<PlayerHandler>();
			playerOffset = transform.position.y - playerHandler.transform.position.y;
		}
	}

	void Update()
	{
		transform.position = transform.parent.position + (Vector3.up * playerOffset);

		if (onPlayer)
			if (playerHandler.CurrentState == PlayerHandler.PlayerState.Ball)
				transform.position += (Vector3.up * ballOffset);

		transform.rotation = startRot;
	}

	void LateUpdate()
	{
		transform.position = transform.parent.position + (Vector3.up * playerOffset);

		if (onPlayer)
			if (playerHandler.CurrentState == PlayerHandler.PlayerState.Ball)
				transform.position += (Vector3.up * ballOffset);

		transform.rotation = startRot;
	}
}
