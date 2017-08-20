using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class SliderController : PlayerController
{
	public override bool IsGrounded() { return true; }
	public override bool IsFalling() { return true; }
	public override Vector3 GetVelocity() { return rb.velocity; }
	public override void PushAway(Vector3 direction, float force) { }

	const float acceleration = 30;
	const float maxHorizVelocity = 14;
	const float maxDownVelocity = 20;

	const float extraGravity = 30;
	const float initialDownForce = 8;
	const float initialForwardForce = 5;
	const float tooSteepAngle = 85;     // above this? consider it a wall, and bounce off
	const float bumpAgainstWallForce = 4;
	const float slowdownSpeed = 0.915f;

	const float newRadius = 0.6f;
	float startRadius;

	Quaternion floorRot;
	Vector3 input;
	Vector3 lastForward = Vector3.forward;

	bool checkMaxAngle = true;

	int enabledFrameStamp = 0;
	bool RecentlyEnabled { get { return Mathf.Abs(Time.frameCount - enabledFrameStamp) < 5; } }

	float DownVel { get { return Vector3.Dot(rb.velocity, Vector3.down); } }
	float RightVel { get { return Vector3.Dot(rb.velocity, Vector3.right); } }

	new void Awake()
	{
		base.Awake();

		capsuleHeight = 0.8f;
		startRadius = capsuleCollider.radius;
		handsAvailable = true;
		isPhysicsControlled = true;
	}

	void OnDisable()
	{
		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, 0, 0);
	}

	void Update()
	{
		if (isFrozen) return;

		if (!GetFloor() && !RecentlyEnabled)
			playerHandler.SwitchState(PlayerHandler.PlayerState.Human);

		input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		rotateMesh.forward = lastForward;
		lastForward = rotateMesh.forward;
	}

	bool GetFloor()
	{
		RaycastHit hitInfo;
		Vector3 lastPos = rb.position;
		rb.MovePosition(rb.position + Vector3.up * 0.2f);

		if (rb.SweepTest(Vector3.down, out hitInfo, 10, QueryTriggerInteraction.Ignore))
		{
			rb.MovePosition(lastPos);

			float angleCollided;
			SetAngleCollided(hitInfo.normal, out angleCollided, out floorRot);

			if (RecentlyEnabled || Time.frameCount % 15 == 0)
			{
				bool hasForceSlide = CheckForForceSlide(hitInfo);

				if (!checkMaxAngle && !hasForceSlide)
					return false;
			}

			if (checkMaxAngle && angleCollided <= maxClimbAngle)
				return false;

			Collide(hitInfo);
		}
		else { rb.MovePosition(lastPos); return false; }

		if (rb.SweepTest(rotateMesh.right, out hitInfo, 0.5f, QueryTriggerInteraction.Ignore))
			CheckBumpWalls(hitInfo);

		if (rb.SweepTest(-rotateMesh.right, out hitInfo, 0.5f, QueryTriggerInteraction.Ignore))
			CheckBumpWalls(hitInfo);

		return true;
	}

	void Collide(RaycastHit hitInfo)
	{
		Vector3 normal = hitInfo.normal; normal.y = 0;
		if (normal.magnitude >= 0.2f)
		{
			rotateMesh.forward = normal.normalized;
			lastForward = rotateMesh.forward;
		}
	}

	bool CheckForForceSlide(RaycastHit hitInfo)
	{
		ForceSlide forceSlide = hitInfo.collider.gameObject.GetComponent<ForceSlide>();

		if (forceSlide != null)
		{
			checkMaxAngle = false;

			if (forceSlide.PushOff)
				PushForward(rotateMesh.forward);
		}

		return forceSlide != null;
	}

	void PushForward(Vector3 direction)
	{
		rb.AddForce(direction * bumpAgainstWallForce, ForceMode.VelocityChange);
	}

	bool CheckBumpWalls(RaycastHit hitInfo)
	{
		float angleCollided;
		SetAngleCollided(hitInfo.normal, out angleCollided, out floorRot);

		if (angleCollided > tooSteepAngle)
		{
			BumpAgainstWall(hitInfo.normal);
			return true;
		}

		return false;
	}

	void BumpAgainstWall(Vector3 direction)
	{
		rb.AddForce(direction * bumpAgainstWallForce, ForceMode.VelocityChange);
	}

	void FixedUpdate()
	{
		if (isFrozen) return;

		Vector3 movement = GetInput();

		rb.AddForce(movement * acceleration);
		AddGravity();
		MovementLimits();
	}

	/*void OnGUI()
	{
		GUIStyle newStyle = new GUIStyle();
		newStyle.fontSize = 20;
		GUI.Label(new Rect(10, Screen.height-30, 100, 20), rb.velocity.y+"", newStyle);
	}*/

	Vector3 GetInput()
	{
		Vector3 movement = GetMovement(input, floorRot);
		return movement * IgnoreUpInputAmount(movement);
	}

	float IgnoreUpInputAmount(Vector3 movement)
	{
		float dot = Vector3.Dot(movement, Vector3.up);	// see how close our input is to the up input
		dot = Mathf.Abs(dot - 1);						// inverse this amount: the more UP we are, the lower this number
		dot = Mathf.Clamp(dot, 0, 1);					// may go above 1, so... make sure we don't

		return dot;
	}

	void AddGravity()
	{
		rb.AddForce(Vector3.down * extraGravity);
	}

	void MovementLimits()
	{
		if (DownVel >= maxDownVelocity)
			rb.velocity *= slowdownSpeed;

		if (Mathf.Abs(RightVel) >= maxHorizVelocity)
			rb.velocity *= slowdownSpeed;
	}

	public override void DisableByHandler(PlayerHandler.PlayerState nextState)
	{
		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, 0, 0);

		KillVelocity();
		humanAnimator.SetBool("Sliding", false);
						
		capsuleCollider.radius = startRadius;
		rotateMesh.rotation = Quaternion.Euler(0, rotateMesh.eulerAngles.y, 0);

		KillVelocity();
		rb.isKinematic = true;
		rb.useGravity = false;
		enabled = false;
	}

	public override void EnableByHandler(Vector3 velocityChange, bool doHop)
	{
		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, 0.13f, 0.13f);

		humanAnimator.SetBool("Sliding", true);
		rb.isKinematic = false;
		rb.useGravity = true;
		rb.mass = 1;

		capsuleCollider.height = capsuleHeight;
		capsuleCollider.radius = newRadius;

		enabledFrameStamp = Time.frameCount;
		checkMaxAngle = true;

		rb.AddForce(Vector3.down * initialDownForce, ForceMode.VelocityChange);
		PushForward(rotateMesh.forward);
		enabled = true;
	}

	public override void SetFrozen(bool frozen, bool freezeAnimator)
	{
		isFrozen = frozen;

		if (isFrozen)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}

		rb.useGravity = !isFrozen;

		if (isFrozen && freezeAnimator)
			humanAnimator.enabled = false;

		if (!IsFrozen && !humanAnimator.enabled)
			humanAnimator.enabled = true;
	}
}
