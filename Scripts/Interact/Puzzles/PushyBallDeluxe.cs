using System;
using System.Collections;
using UnityEngine;

public class PushyBallDeluxe : MonoBehaviour
{
	[SerializeField] Transform ballMesh;
	[SerializeField] Collider trigger;
	Rigidbody rb;

	PlayerHolder playerHolder;
	PlayerHandler playerHandler;
	HumanController humanController;
	Vector3 lastPos;

	const float RotationSpeedMod = 1000;
	bool AttachedToPlayer { get { return transform.parent == playerHolder.transform; } }

	void Start()
	{
		rb = GetComponent<Rigidbody>();

		playerHolder = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerHolder>();
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
		humanController = GameObject.FindWithTag("Player").GetComponent<HumanController>();
		lastPos = transform.position;
	}

	void FixedUpdate()
	{
		if (AttachedToPlayer)
		{
			rb.angularVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}

	void Update()
	{
		Vector3 pos = transform.position; pos.y = 0;
		float distance = Vector3.Distance(pos, lastPos);

		transform.Rotate(Vector3.right * ((distance * RotationSpeedMod) * Time.deltaTime));
		lastPos = pos;

		if (Input.GetKey(KeyCode.P))
			EndPushy();
	}

	void OnTriggerStay(Collider col)
	{
		if (col.gameObject.tag == "Player" && playerHandler.CurrentState == PlayerHandler.PlayerState.Human)
		{
			Vector3 playerToBall = (playerHandler.transform.position - transform.position).normalized;

			if (Vector3.Dot(playerToBall, playerHandler.RotateMesh.forward) < -0.5f)
				StartPushy();
		}
	}

	void StartPushy()
	{
		if (!AttachedToPlayer)
		{
			trigger.enabled = false;

			Quaternion storedRot = transform.rotation;
			transform.right = playerHandler.RotateMesh.right;
			ballMesh.rotation = storedRot;

			transform.parent = playerHolder.transform;
			playerHandler.HumanAnimator.SetBool("Pushing", true);
			gameObject.layer = LayerMask.NameToLayer("NoPlayerPhysics");

			humanController.GetPushyBall(this);
		}
	}

	public void EndPushy()
	{
		if (AttachedToPlayer)
		{
			StartCoroutine(WaitToTurnOnTrigger());

			Quaternion storedRot = ballMesh.rotation;
			transform.rotation = storedRot;
			ballMesh.localRotation = Quaternion.identity;

			transform.parent = null;
			playerHandler.HumanAnimator.SetBool("Pushing", false);
			gameObject.layer = LayerMask.NameToLayer("Default");
		}
	}

	IEnumerator WaitToTurnOnTrigger()
	{
		yield return new WaitForSeconds(0.5f);
		trigger.enabled = true;
	}
}
