using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public class HumanRopeController : HumanController
{
	Transform rope;

	bool inSpool = false;
	Vector3 inDir;
	const float positionOffset = 1.8f;

	int lastJumpedFrame = 0;
	public int LastJumpedFrame { get { return lastJumpedFrame; } }

	new protected void Awake()
	{
		base.Awake();
		maxSpeed *= 1.35f;
		handsAvailable = false;
	}

	protected override void Update()
	{
		if (!PlayerHandler.CanUpdate) return;

		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		Vector3 moveDir = GetMovement(input, Quaternion.identity);
		moveDir = rope.rotation * moveDir;

		float moveSpeed = moveDir.magnitude;
		
		if (Vector3.Angle(moveDir, rope.rotation * rotateMesh.forward) > 135)	// to-do: 135 should be const in humancontroller
			rotateMesh.forward = -rotateMesh.forward;

		if (moveSpeed > 0.19f)      // greater than deadzone
			SpeedUp(ref moveSpeed);
		else
			SlowDown(ref moveSpeed);

		float normalizedSpeed = moveSpeed / maxSpeed;
		humanAnimator.SetFloat("Speed", normalizedSpeed);

		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, Mathf.Clamp(normalizedSpeed, 0, 0.13f), Mathf.Clamp(normalizedSpeed, 0, 0.13f));

		Vector3 vel = CalcVelocity(moveSpeed);
		if (!isFrozen) Move(ref vel);

		isGrounded = false;
		lastVel = vel;

		if (Input.GetButtonDown(PlayerHandler.JumpString))
		{
			playerHandler.SwitchState(PlayerHandler.PlayerState.Human);

			// we have to get this reference the long way, not fully sure why /shrug
			playerHandler.gameObject.GetComponent<HumanController>().ForceJump(false, true);
			lastJumpedFrame = Time.frameCount;
		}
	}

	protected override Vector3 CalcVelocity(float moveSpeed)
	{
		Vector3 vel = rotateMesh.forward * moveSpeed;
		return vel;
	}

	protected override void Move(ref Vector3 vel)
	{
		if (inSpool && rotateMesh.forward == inDir)
			return;

		transform.position += vel * Time.deltaTime;
	}

	public void SetRope(Transform rope)
	{
		this.rope = rope;
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.name.ToLower() == "spool")
		{
			inDir = rotateMesh.forward;
			inSpool = true;
		}
	}

	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.name.ToLower() == "spool")
			inSpool = false;
	}

	public override void EnableByHandler(Vector3 velocityChange, bool doHop)
	{
		base.EnableByHandler(velocityChange, doHop);

		humanAnimator.SetLayerWeight(1, 1);
		humanAnimator.SetBool("OnRope", true);
		humanAnimator.CrossFade("hero_to_rope", 0.1f);

		PositionPlayerOnRope();
	}

	public override void DisableByHandler(PlayerHandler.PlayerState nextState)
	{
		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, 0, 0);

		base.DisableByHandler(nextState);
		humanAnimator.SetBool("OnRope", false);

		Vector3 forward = rotateMesh.forward;
		forward.y = 0;
		rotateMesh.forward = forward;	// make sure we're not tilted up
	}

	void PositionPlayerOnRope()
	{
		// calculate distance from rope, and which way this difference is going (sign)
		// to be used later
		Vector3 ropePos = rope.position; Vector3 playerPos = transform.position;
		ropePos.y = 0; playerPos.y = 0;
		float dist = Vector3.Distance(ropePos, playerPos);
		Vector3 dir = (ropePos - playerPos).normalized;
		float sign = -Mathf.Sign(Vector3.Dot(dir, Vector3.forward));

		if (rope.GetComponent<RopeForPlayer>().IsJanked) sign = -sign;

		transform.position = rope.position + (Vector3.down * positionOffset);

		// set player to rope's forward so that we can move him along in the direction of the rope
		Vector3 playerForward = rotateMesh.forward;	// store this for later
		rotateMesh.forward = rope.forward;
		transform.position += rotateMesh.forward * (dist * sign);	// move based on distance, and direction of dist

		// set the player's forward direciton for real, based on their previous direction
		Vector3 ropeForward = rope.forward;
		ropeForward.y = 0;
		float a1 = Vector3.Angle(ropeForward, playerForward);
		float a2 = Vector3.Angle(ropeForward, -playerForward);

		if (a1 >= a2)
			rotateMesh.forward = -rope.forward;
	}
}
