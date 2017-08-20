using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public class HumanController : PlayerController
{
	private HumanCollider raycastCol;

	protected float maxSpeed = 6.75f;       // should be a constant, but meh
	protected const float gravity = 50;
	protected const float jumpSpeed = 18;
	protected const float acceleration = 30;
	protected const float deceleration = 45;
	protected const float ceilingHitVel = -1.25f;

	protected int edgeJumpFrames = 0;
	protected const int maxEdgeJumpFrames = 8;

	protected int enabledFrameStamp = 0;
	protected int FramesSinceEnable { get { return Time.frameCount - enabledFrameStamp; } }
	protected int shelvedFrameStamp = 0;
	protected int FramesSinceShelved { get { return Time.frameCount - shelvedFrameStamp; } }

	protected Vector3 lastPos;
	protected Vector3 lastVel;
	protected Vector3 impulse;
	protected float lastSpeed = 0;

	protected bool setFallingFace = false;

	protected float jumpyVel;
	protected float speedJumpedAt;
	protected float currJumpSpeed;
	protected bool enableReJump = true;
	protected bool allowJumpInput = true;

	protected const float rotSmooth = 20;          // smoothing on the lerp to rotate towards stick direction
	protected const float rotSmoothSlow = 5;
	protected const float maxFallSpeed = 30;
	protected const float jumpDetraction = 0.25f;
	protected const float fallDownFast = 0.90f;
	protected const float reverseDirModifier = -3.5f;

	protected bool isGrounded = false;
	protected bool shareIsGrounded = false; // record at end of frame (after raycast) to get acurate info
	public override bool IsGrounded() { return shareIsGrounded; }  // only use in external classes
	public override bool IsFalling() { return lastVel.y <= 0; }

	enum JumpState { Complex, Simple }
	JumpState jumpState = JumpState.Complex;

	public override Vector3 GetVelocity() { return lastVel; }

	public bool CheckForSteepSlope() { return raycastCol.CheckForSteepSlope(); }
	public Platform_TrackDistance GetMovingPlatform { get { return raycastCol.GetMovingPlatform; ; } }
	public void SetMovingPlatform(Platform_TrackDistance plat) { raycastCol.SetMovingPlatform(plat); }

	new protected void Awake()
	{
		base.Awake();

		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

		currJumpSpeed = jumpSpeed;
		capsuleHeight = capsuleCollider.height;
		transitionHopAmount = 0.6f;

		raycastCol = GetComponent<HumanCollider>();
		handsAvailable = true;
	}

	bool showText = false;
	void OnGUI()
	{
		if (showText)
		{
			GUIStyle style = new GUIStyle();
			style.fontSize = 32;
			Vector3 vel = GetVelocity(); vel.y = 0;
			GUI.Label(new Rect(10, Screen.height - 40, 300, 300), jumpState + "", style);
		}
	}

	Vector3 ColliderIgnoreDirection(Vector3 movement)
	{
		Vector3 origMovement = movement;
		movement.y = 0;

		float dot = Vector3.Dot(movement.normalized, raycastCol.NoInputDir);

		if (dot > 0.85f)
			origMovement = new Vector3(0, origMovement.y, 0);

		return origMovement;
	}

	protected virtual void Update()
	{
		if (!PlayerHandler.CanUpdate)
			return;  // skip slow frames: prevents us from skipping collision

		float distSpeed = GetDistSpeed();
		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		Vector3 moveDir = GetMovement(input, Quaternion.identity);
		float moveSpeed = moveDir.magnitude;

		if (!isFrozen) RotateMesh(moveDir.normalized);

		if (moveSpeed > 0.19f)      // greater than deadzone
			SpeedUp(ref moveSpeed);
		else
			SlowDown(ref moveSpeed);

		humanAnimator.SetFloat("Speed", moveSpeed / maxSpeed);

		Vector3 vel = CalcVelocity(moveSpeed);

		if (jumpState == JumpState.Complex)
			HandleJumpInputComplex(Input.GetButton(PlayerHandler.JumpString) && allowJumpInput, distSpeed, true, ref vel);

		if (jumpState == JumpState.Simple)
			HandleJumpInputSimple(0, false);

		bool hitCeiling = raycastCol.HitCeilingThisFrame();
		if (!isGrounded && vel.y > ceilingHitVel && hitCeiling)
			vel.y = ceilingHitVel;

		vel = ColliderIgnoreDirection(vel);

		if (!isFrozen) Move(ref vel);

		HandleFalling(vel);

		isGrounded = false;
		lastVel = vel;

		if (!isFrozen)
		{
			if (humanAnimator.GetCurrentAnimatorStateInfo(0).IsName("hero_jump"))
				humanAnimator.SetBool("IsFalling", IsFalling());
			else
				humanAnimator.SetBool("IsFalling", IsFalling() && !shareIsGrounded);	// && !raycastCol.HitSteepSlope);
		}

		HandleGrounded();

		if (Input.GetKeyDown(KeyCode.F1))
			showText = !showText;
	}

	protected float GetDistSpeed()
	{
		Vector3 horizPos = transform.position;
		horizPos.y = 0;
		float distSpeed = Vector3.Distance(horizPos, lastPos) / Time.deltaTime;
		lastPos = horizPos;
		return distSpeed;
	}

	protected void RotateMesh(Vector3 moveDir)
	{
		float angle = Vector3.Angle(moveDir, rotateMesh.forward);

		if (angle > 135)        // if we're a very big angle change, we'll want to snap right to it, instead of lerping
		{
			rotateMesh.forward = moveDir;
			// changing angle? set speed to negative to slow down
			lastSpeed = (lastSpeed / maxSpeed) * reverseDirModifier;
		}
		else
		{
			Vector3 targetRotation = Vector3.Lerp(rotateMesh.forward, moveDir,
				Time.deltaTime * (isGrounded ? rotSmooth : rotSmoothSlow));
			if (targetRotation != Vector3.zero)
				rotateMesh.rotation = Quaternion.LookRotation(targetRotation);
		}
	}

	protected float GetAirClamp()
	{
		const float speedFloor = 0.9f;
		const float maxClamp = 1.4f;
		float airClamp = 1;

		if (!isGrounded)
		{
			airClamp = (speedJumpedAt / maxSpeed) + speedFloor;      // percentage
			if (airClamp > maxClamp) airClamp = maxClamp;
		}

		return airClamp;
	}

	protected void SpeedUp(ref float speed)
	{
		float currentMaxSpeed = GetAirClamp() * maxSpeed * speed;
		float accelAmount = acceleration * Time.deltaTime;

		speed = (lastSpeed + accelAmount);
		if (speed > currentMaxSpeed)
		{
			if (speed < speed + accelAmount)
				SlowDown(ref speed, currentMaxSpeed);
			else
				SlowDown(ref speed);
		}

		lastSpeed = speed;
	}

	protected void SlowDown(ref float speed, float floor = 0)
	{
		speed = lastSpeed - (deceleration * Time.deltaTime);
		if (speed < floor) speed = floor;

		lastSpeed = speed;
	}

	protected void Stop(ref float speed)
	{
		speed = 0;
		lastSpeed = speed;
	}

	protected virtual Vector3 CalcVelocity(float moveSpeed)
	{
		Vector3 vel = rotateMesh.forward * moveSpeed;
		vel.y = lastVel.y + jumpyVel;

		return vel;
	}

	public void ForceJump(bool doAnimation)
	{
		lastVel.y = 0;
		jumpyVel = 0;

		currJumpSpeed = jumpSpeed;
		JUMP(maxSpeed, doAnimation, ref lastVel);
	}

	public override void DoHighJump(float pressHeight, float noPressHeight)
	{
		if (playerHandler.JustSlammed)
		{
			pressHeight = pressHeight * 1.2f;
			noPressHeight = pressHeight;
		}

		lastVel.y = 0;
		jumpyVel = 0;

		currJumpSpeed = jumpSpeed;

		float jSpeed = Input.GetButton(PlayerHandler.JumpString) ? pressHeight : noPressHeight;
		SIMPLE_JUMP(jSpeed);
	}

	public void SIMPLE_JUMP(float height)
	{
		jumpState = JumpState.Simple;
		HandleJumpInputSimple(height, true);
	}

	private void HandleJumpInputSimple(float height, bool goUp)
	{
		if (goUp)
		{
			if (height > 5)     // must be hight enough to play animation and whatnot
				HandleJumpEffects(true);

			jumpyVel = height;
		}
		else  // fall
		{
			jumpyVel = 0;
			SetEnableReJump(Input.GetButton(PlayerHandler.JumpString));
		}
	}

	private void HandleJumpInputComplex(bool jumpHeld, float speed, bool doAnimation, ref Vector3 vel)
	{
		if (jumpHeld && (isGrounded || edgeJumpFrames > 0) && enableReJump)
		{
			JUMP(speed, doAnimation, ref vel);
			return;
		}
		
		if ((!jumpHeld && !isGrounded) || IsFalling())
		{
			if (!IsFalling())      // set vel to fall down faster
				vel.y *= fallDownFast;

			currJumpSpeed = jumpSpeed;
			jumpyVel = 0;
		}

		SetEnableReJump(jumpHeld);
	}

	void SetEnableReJump(bool jumpHeld)
	{
		if (IsFalling() && !jumpHeld)
			enableReJump = true;
	}

	private void JUMP(float speed, bool doAnimation, ref Vector3 vel)
	{
		vel.y = 0;

		HandleJumpEffects(doAnimation);
		speedJumpedAt = speed;
		enableReJump = false;
		edgeJumpFrames = 0;

		SetJumpyVel();
	}

	private void HandleJumpEffects(bool doAnimation)
	{
		int r = UnityEngine.Random.Range(1, 5);
		SoundManager.instance.PlayClip("JumpGrunt0" + r);

		if (doAnimation) humanAnimator.SetTrigger("Jump");
	}

	private void SetJumpyVel()
	{
		const float floor = 0.4f;
		const float min = 1;
		const float max = 1.125f;

		float valueBasedOnRunningJumpSpeed = Mathf.Clamp((speedJumpedAt / maxSpeed) + floor, min, max);
		jumpyVel = jumpyVel + (currJumpSpeed * valueBasedOnRunningJumpSpeed);
		currJumpSpeed *= jumpDetraction;
	}

	protected virtual void Move(ref Vector3 vel)
	{
		vel.y -= gravity * Time.deltaTime;
		vel.y = Mathf.Clamp(vel.y, -maxFallSpeed, float.MaxValue);

		Vector3 movePlatDist = Vector3.zero;
		if (raycastCol.OnMovingPlatform)
			movePlatDist = raycastCol.MovePlatDist;

		DoTwirl();
		charController.Move(((vel + impulse) * Time.deltaTime) + movePlatDist);
	}

	void DoTwirl()
	{
		if (!raycastCol.OnTwirlingPlatform) return;

		Platform_TrackTwirl plat = raycastCol.GetTwirlingPlatform.Get();

		Vector3 playerLastPos = transform.position;
		Quaternion playerLastRot = transform.rotation;

		transform.RotateAround(plat.transform.position, plat.Axis, plat.Rotation);
		transform.position = new Vector3(transform.position.x, playerLastPos.y, transform.position.z);
		transform.rotation = playerLastRot;
	}

	protected void HandleFalling(Vector3 vel)
	{
		if (playerHandler.CurrentSecondaryAction != PlayerHandler.SecondaryAction.None)
			return;

		if (vel.y <= -maxFallSpeed && !setFallingFace)
			SetFalling();
		if (vel.y > -maxFallSpeed && setFallingFace)
			SetLanded();
	}

	public void PrepareForLanding() { lastVel.y = -maxFallSpeed; setFallingFace = true; }
	void SetFalling()
	{
		playerHandler.SetFaceAnimation("Shocked");
		playerHandler.AtMaxFallSpeed();
		setFallingFace = true;
	}

	void SetLanded()
	{
		playerHandler.SetFaceAnimation("Smile", true);
		playerHandler.Landed();
		setFallingFace = false;
	}

	bool settingFloored = false;
	public void SetFloored() { StartCoroutine("DoSetFloored"); }
	public void CancelSetFloored() {StopCoroutine("DoSetFloored"); settingFloored = false; }

	IEnumerator DoSetFloored()
	{
		settingFloored = true;

		while (!IsGrounded())
		{
			KillHorizontalVelocity();
			yield return null;
		}

		settingFloored = false;
		SetFrozen(true, false);
	}

	public void OnFloor(bool doRumble)
	{
		lastVel.y = 0;
		isGrounded = true;
		jumpState = JumpState.Complex;

		if (doRumble && !shareIsGrounded) // be here if last frame wasn't on ground
		{
			raycastCol.CancelNoInput();

			if (PlayerHandler.AllowVibration)
				StartCoroutine(Rumble(0.075f, 0.4f, 0.4f));
		}
	}

	void HandleGrounded()
	{
		if (IsFalling() && !isFrozen)
			raycastCol.CustomUpdate(lastVel.y);
		else
		{
			if (raycastCol.OnMovingPlatform)
				raycastCol.DisableMovingPlatform();

			if (raycastCol.OnTwirlingPlatform)
				raycastCol.DisableTwirlingPlatform();
		}

		// if character controller is grounded, but OUR collider isn't grounded... we're most likely stuck on a wall
		if (charController.isGrounded && !isGrounded)
		{
			if (playerHandler.CanBeSlider())
				playerHandler.SwitchState(PlayerHandler.PlayerState.Slider);
			else
				shelvedFrameStamp = Time.frameCount;

			OnFloor(false);
		}
		else allowJumpInput = true;

		if (edgeJumpFrames > 0)
			edgeJumpFrames--;

		// ACTIVATE EDGE JUMP
		if (shareIsGrounded && !isGrounded && IsFalling() && allowJumpInput &&
			FramesSinceEnable > 30 && FramesSinceShelved > 30) // wait a while after re-enabling (from another state) before allowing edge jump
		{
			edgeJumpFrames = maxEdgeJumpFrames;
		}

		shareIsGrounded = isGrounded;   // record at end of frame (after raycast) to get acurate info
		humanAnimator.SetBool("IsGrounded", IsGrounded());
	}

	IEnumerator Rumble(float time, float leftMotor, float rightMotor)
	{
		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, leftMotor, rightMotor);

		yield return new WaitForSeconds(time);

		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, 0, 0);
	}

	// convert from ball speed to human speed
	public void SetSpeedChange(float newSpeed)
	{
		lastSpeed = newSpeed * (maxSpeed * 2.5f);

		if (newSpeed > 0.1f)
			StartCoroutine(WaitForAnimatorThenSetSpeed(newSpeed));
	}

	// probably temporary... call after setting speed from ball
	// because the human was turned off, turning back on takes a bit for animator
	// to "initialize," so wait for that, then set speed
	protected IEnumerator WaitForAnimatorThenSetSpeed(float speed)
	{
		while (!humanAnimator.isInitialized)
		{
			yield return null;
		}

		humanAnimator.SetFloat("Speed", speed);
	}

	public override void EnableByHandler(Vector3 velocityChange, bool doHop)
	{
		transform.rotation = Quaternion.identity;
		if (doHop) transform.position += Vector3.up * transitionHopAmount;
		capsuleCollider.height = capsuleHeight;

		if (velocityChange.magnitude > 0.01f)  // only change direction (and speed) if the ball was fast enough
		{
			SetDirection(velocityChange.normalized);
			SetSpeedChange(velocityChange.magnitude);
		}

		charController.enabled = true;
		isGrounded = false;
		enabledFrameStamp = Time.frameCount;
		enabled = true;

		Platform_TrackDistance storedMovePlat = null;
		if (raycastCol.GetMovingPlatform != null)
			storedMovePlat = raycastCol.GetMovingPlatform;

		raycastCol.CustomUpdate(lastVel.y);
		shareIsGrounded = isGrounded;

		if (storedMovePlat != null)
			raycastCol.SetMovingPlatform(storedMovePlat);
	}

	public override void DisableByHandler(PlayerHandler.PlayerState nextState)
	{
		KillVelocity();
		jumpState = JumpState.Complex;

		StopAllCoroutines();

		if (settingFloored)
			SetFloored();

		humanAnimator.SetFloat("Speed", 0);
		isFrozen = false;

		if (nextState != PlayerHandler.PlayerState.Slider)
			GetComponentInChildren<PlayerHolder>().Drop();

		impulse = Vector3.zero;
		raycastCol.DisableMovingPlatform();
		raycastCol.DisableTwirlingPlatform();
		raycastCol.CancelNoInput();
		charController.enabled = false;
		enabled = false;
	}

	public void ResetJumpState()
	{
		jumpState = JumpState.Complex;
	}

	public override void SetFrozen(bool frozen, bool freezeAnimator)
	{
		isFrozen = frozen;

		if (isFrozen && freezeAnimator)
			humanAnimator.enabled = false;

		if (!IsFrozen && !humanAnimator.enabled)
			humanAnimator.enabled = true;
	}

	public override void KillVelocity()
	{
		lastSpeed = 0;
		lastVel = new Vector3(0, 0, 0);

		CancelJump();
	}

	void KillHorizontalVelocity()
	{
		lastSpeed = 0;
		lastVel = new Vector3(0, lastVel.y, 0);

		CancelJump(false);
	}

	void CancelJump(bool killGravity = true)
	{
		if (killGravity)
			lastVel = new Vector3(lastVel.x, 0, lastVel.z);

		jumpyVel = 0;
		enableReJump = false;
		allowJumpInput = false;
		edgeJumpFrames = 0;
	}

	public void CancelEnableReJump()
	{
		enableReJump = false;
	}

	public void SetDirection(Vector3 direction)
	{
		if (direction.magnitude != 0)
			rotateMesh.forward = direction;
	}

	void PushAwayInternal(Vector3 direction, float force, float time)
	{
		humanAnimator.SetTrigger("Recoil");
		rotateMesh.forward = direction;

		StopCoroutine("Push");
		impulse = (direction * force) * 2;  // multiply so that we're closer to physics AddForce

		StartCoroutine(PushAwayRoutine(time));
	}

	// use this to push the player away in a certain direction
	// direction: normalized vector for which direction to push
	// force: how much force to apply in this direction
	public override void PushAway(Vector3 direction, float force)
	{
		PushAwayInternal(direction, force, 0.8f);
	}

	public void PushAway(Vector3 direction, float force, float time)
	{
		PushAwayInternal(direction, force, time);
	}

	protected IEnumerator PushAwayRoutine(float time)
	{
		while (impulse.magnitude >= 0.1f)
		{
			impulse *= time;
			yield return null;
		}
		impulse = Vector3.zero;
	}
}
