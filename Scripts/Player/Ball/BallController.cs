using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using XInputDotNetPure;

public class BallController : PlayerController
{
	[SerializeField] Collider hitTrigger;
	[SerializeField] ParticleSystem ballRollDust;
	[SerializeField] GameObject slamParticle;
	[SerializeField] GameObject chargeParticle;

	BallCollider ballCollider;
	Quaternion floorRot;

	const int acceleration = 30;
	const int MAX_VELOCITY = 15;
	const float maxFallSpeed = 22;
	const float jumpHeight = 8.5f;
	const float slamJumpHeight = 16;
	const float changeInVelDot = 0.25f;
	float maxVelocity = MAX_VELOCITY;

	float hitTriggerStartSize;
	float hitTriggerSlamSize;

	float extraGravity;
	const float regExtraGravity = 0.25f;
	const float slamExtraGravity = 2.5f;
	const float playerSlowdownSpeed = 0.915f;

	const float responsiveDrag = 0;		// normal moving drag
	const float resistiveDrag = 2.5f;    // slow-down drag
	const float stickFloorIntensity = 20;
	const float neededHeightForBallSlam = 1.75f;
	const float lowNeededHeightForBallSlam = 0.88f;
	const int maxSlideTime = 15;

	Vector3 lastPos;
	Vector3 distanceInFrame;
	public Vector3 Distance { get { return distanceInFrame; } }

	bool pressingJump = false;
	bool pressingJumpDown = false;
	bool enableReJump = false;
	bool storedJumpInput = false;
	bool atMaxVel = false;

	int afterSlamStamp = 0;
	const int maxAfterSlamFrames = 5;
	bool allowStickToFloorBox = true;
	bool useMaxVelocity = true;

	int edgeJumpFrames = 0;
	const int maxEdgeJumpFrames = 8;

	float capsuleStartRadius;
	const float capsuleBallRadius = 0.4f;

	int steepSlopeStamp = 0;
	int enabledStamp = 0;
	public bool RecentlyEnabled { get { return Mathf.Abs(Time.frameCount - enabledStamp) <= 10; } }

	public enum AirState { Grounded, SteepSlope, Air, Nothing }; // "Nothing" happens only after human -> ball, to clear it out
	AirState airState;

	float airControlMod;
	float CalcAirControlMod { get { return Mathf.Clamp(GetSpeed, 0.3f, float.MaxValue); } }

	public override Vector3 GetVelocity() { return rb.velocity; }
	public Vector3 GetDirection { get { return rb.velocity.normalized; } }
	float CalcSpeed { get { Vector3 vel = rb.velocity; vel.y = 0; return vel.magnitude / MAX_VELOCITY; } }
	float speedThisFrame;
	public float GetSpeed { get { return speedThisFrame; } }

	public override bool IsGrounded() { return airState != AirState.Air; }
	public override bool IsFalling() { return rb.velocity.y <= 0.01f; }
	public bool IsSlamming { get { return extraGravity == slamExtraGravity; } }
	public bool JustSlammed { get { return Mathf.Abs(Time.frameCount-afterSlamStamp) < maxAfterSlamFrames; } }
	public bool SearchingForSlide { get { return Time.frameCount - steepSlopeStamp <= maxSlideTime; } }

	public Platform_TrackDistance GetMovingPlatform { get { return ballCollider.GetMovingPlatform; ; } }
	public void SetMovingPlatform(Platform_TrackDistance plat) { ballCollider.SetMovingPlatform(plat); }

	new void Awake()
	{
		base.Awake();

		ballCollider = GetComponent<BallCollider>();

		airState = AirState.Air;
		extraGravity = regExtraGravity;

		capsuleHeight = 0.8f;
		transitionHopAmount = 0.6f;
		capsuleStartRadius = capsuleCollider.radius;
		handsAvailable = false;
		isPhysicsControlled = true;

		SphereCollider sphere = (SphereCollider)hitTrigger;
		hitTriggerStartSize = sphere.radius;
		hitTriggerSlamSize = hitTriggerStartSize * 3;
	}
	
	/*void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		style.fontSize = 32;
		GUI.Label(new Rect(10, Screen.height - 40, 300, 300), (CalcSpeed * MAX_VELOCITY) + "", style);
	}*/

	void Update()
	{
		if (isFrozen) return;
		if (!PlayerHandler.CanUpdate) return;

		pressingJump = Input.GetButton(PlayerHandler.JumpString);
		pressingJumpDown = Input.GetButtonDown(PlayerHandler.JumpString);

		if (extraGravity != slamExtraGravity && airState == AirState.Air)
			HandleMidAirSlam();

		if (ballCollider.OnMovingPlatform)
			transform.position += ballCollider.MovePlatDist;

		DoTwirl();
		HandleSliderCheck();

		if (ballCollider.OnMovingPlatform || ballCollider.OnTwirlingPlatform)
			SetInterpolation(RigidbodyInterpolation.None);
		else
			SetInterpolation(RigidbodyInterpolation.Interpolate);
	}

	void SetInterpolation(RigidbodyInterpolation mode)
	{
		if (rb.interpolation != mode)
			rb.interpolation = mode;
	}

	void DoTwirl()
	{
		if (!ballCollider.OnTwirlingPlatform)
			return;

		Platform_TrackTwirl plat = ballCollider.GetTwirlingPlatform.Get();

		Vector3 playerLastPos = transform.position;
		Quaternion playerLastRot = transform.rotation;

		transform.RotateAround(plat.transform.position, plat.Axis, plat.Rotation);
		transform.position = new Vector3(transform.position.x, playerLastPos.y, transform.position.z);
		transform.rotation = playerLastRot;
	}

	void HandleSliderCheck()
	{
		int slideTime = Time.frameCount - steepSlopeStamp;

		if (slideTime < maxSlideTime && airState != AirState.SteepSlope)
		{
			steepSlopeStamp = 0;
			slideTime = Time.frameCount;
		}

		if (slideTime == maxSlideTime)
			playerHandler.SwitchStateInstant(PlayerHandler.PlayerState.Slider);
	}

	void FixedUpdate()
	{
		if (!PlayerHandler.CanUpdate) return;

		AirState lastState = airState;
		HandleCollision();

		if (isFrozen) return;

		if (lastState != AirState.SteepSlope && airState == AirState.SteepSlope)
			steepSlopeStamp = Time.frameCount;

		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		if (airState == AirState.Grounded)
			maxVelocity = MAX_VELOCITY;

		if (airState == AirState.Air)
		{
			// if new state is diff than last, or (to fix bug) JustSlammed (sometimes, doing ball slam doesn't change state)
			if (lastState != AirState.Air || JustSlammed)
			{
				airControlMod = CalcAirControlMod;
				maxVelocity = MAX_VELOCITY * (CalcSpeed * 1.15f);            // set air max velocity to value slightly higher than current

				// make sure our absolute MINIMUM max air velocity is 3, max can be higher
				maxVelocity = Mathf.Clamp(maxVelocity, JustSlammed ? 5 : 3, MAX_VELOCITY + 1);
			}

			if (CalcSpeed <= 0.1f && maxVelocity > MAX_VELOCITY * 0.75f)
				maxVelocity *= 0.25f;

			input *= airControlMod;
		}
		else if (airState == AirState.SteepSlope) input = Vector3.zero;     // on a steep slope? forget about input!

		Vector3 movement = GetMovement(input, floorRot) * acceleration;

		if (!IsSlamming && !IsJumpFrozen)
			HandleJump();

		Vector3 flatVel = rb.velocity; flatVel.y = 0;
		float flatSpeed = flatVel.magnitude;

		// if we change directions dramatically, override maxVelocity
		float velToInput = Vector3.Dot(flatVel.normalized, movement / acceleration);
		if (flatSpeed <= maxVelocity || velToInput <= changeInVelDot || !useMaxVelocity)
			rb.AddForce(movement);

		// kill flat horizontal velocity if we excede slighly higher than max
		if (flatSpeed > maxVelocity+1)// && velToInput > changeInVelDot)
		{
			flatVel *= playerSlowdownSpeed;
			flatVel.y = rb.velocity.y;
			rb.velocity = flatVel;
		}

		if (horizontalFrozen)
			rb.velocity = new Vector3(0, rb.velocity.y, 0);

		MovementLimits(input);
		AddGravity();
		HandleFalling();
		
		if (!IsSlamming)
			SetHitTriggerFromSpeed();

		if (!isFrozen)
			SetMotionParticleFromSpeed();

		speedThisFrame = CalcSpeed;

		Vector3 potentialDistance = transform.position - lastPos;
		if (potentialDistance.magnitude > 0.01f)
			distanceInFrame = potentialDistance;

		lastPos = transform.position;
	}

	void HandleCollision()
	{
		AirState lastAirState = airState;
		ballCollider.CustomUpdate(out airState, out floorRot);

		if (lastAirState == AirState.Air && airState != AirState.Air)
		{
			if (!IsSlamming && PlayerHandler.AllowVibration)
				StartCoroutine(Rumble(0.075f, 0.4f, 0.4f));
		}

		if (edgeJumpFrames > 0)
			edgeJumpFrames--;

		if (lastAirState == AirState.Grounded && airState == AirState.Air && IsFalling())
			edgeJumpFrames = maxEdgeJumpFrames;
	}

	void HandleMidAirSlam()
	{
		// button? no problem, this is easy
		if (Input.GetButtonDown("BallToggle") || Input.GetButtonDown(PlayerHandler.AttackString))
			StartBallSlam(false, lowNeededHeightForBallSlam);
	}

	void HandleJump()
	{
		if (airState == AirState.Grounded && !allowStickToFloorBox)
			allowStickToFloorBox = true;

		if ((pressingJump || storedJumpInput) && (airState == AirState.Grounded || edgeJumpFrames > 0) && enableReJump)
		{
			float dot = Vector3.Dot(Vector3.up, rb.velocity);
			dot = Mathf.Clamp(dot, 1, 1.5f);
			if (GetSpeed <= 0.3f) dot = 1;

			rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);     // before jump: reset Y velocity to offset any gravity
			rb.AddForce(Vector3.up * (jumpHeight * dot), ForceMode.VelocityChange);
			enableReJump = false;
			storedJumpInput = false;
			allowStickToFloorBox = false;

			return;
		}

		if (!pressingJump)
			enableReJump = true;

		if (rb.velocity.y <= -0.01f && pressingJumpDown)
			storedJumpInput = true;
	}

	// When no keys are pressed, slow down the player faster allowing
	// for faster direction shifts
	void MovementLimits(Vector3 movement)
	{
		if (movement.magnitude <= 0.19f && airState == AirState.Grounded && !JustSlammed && !IsSlamming)
		{
			rb.velocity *= playerSlowdownSpeed;
			rb.angularVelocity *= playerSlowdownSpeed;
			rb.drag = resistiveDrag;
		}
		else rb.drag = responsiveDrag;
	}

	public void StickToFloor()
	{
		if (!allowStickToFloorBox) return;

		RaycastHit hitInfo;

		rb.position += Vector3.up * 0.1f;	// check slightly above the rb
		if (rb.SweepTest(Vector3.down, out hitInfo, 1.0f, QueryTriggerInteraction.Ignore))
		{
			Vector3 pos = transform.position;
			pos.y = hitInfo.point.y + (capsuleCollider.height / 2);
			transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * stickFloorIntensity);
		}
		rb.position -= Vector3.up * 0.1f;	// alright, put us back down now
	}

	void AddGravity()
	{
		if (IsSlamming && airState != AirState.Air)       // (doneSlamming || airState != AirState.Air)
			SLAM();
		else
		{
			//if (airState == AirState.Air)
				rb.AddForce(Vector3.down * extraGravity, ForceMode.VelocityChange); // add extra gravity
			//else
			//	rb.AddForce(Vector3.down * (extraGravity * 0.5f), ForceMode.VelocityChange); // still add extra gravity, just... less

			//if (pressingJumpDown)
			//{
			//	// gotta add extra gravity TWICE, to make up for the fact it's no longer being added while we're grounded
			//	rb.AddForce(Vector3.down * extraGravity, ForceMode.VelocityChange);
			//	rb.AddForce(Vector3.down * extraGravity, ForceMode.VelocityChange);
			//}

			if (rb.velocity.y < -maxFallSpeed)
				rb.velocity = new Vector3(rb.velocity.x, -maxFallSpeed, rb.velocity.z);
		}
	}

	void HandleFalling()
	{
		if (rb.velocity.y <= -maxFallSpeed && !atMaxVel)// && !IsSlamming)
		{
			playerHandler.AtMaxFallSpeed();
			atMaxVel = true;
		}

		if (rb.velocity.y > -maxFallSpeed && atMaxVel)
		{
			playerHandler.Landed();
			atMaxVel = false;
		}
	}

	void SLAM()
	{
		if (CanDoHighJump())
		{
			SoundManager.instance.PlayClip("BallSlam");

			rb.velocity = new Vector3(rb.velocity.x, slamJumpHeight, rb.velocity.z);
			GameObject particle = Instantiate(slamParticle, transform.position, Quaternion.identity);
			Destroy(particle, 2);

			CancelChargeParticle();
		}

		extraGravity = regExtraGravity;
		afterSlamStamp = Time.frameCount;

		SetHitTriggerSize(hitTriggerStartSize);
		
		hitTrigger.enabled = false;
		HealthManager.instance.SetExternalIsInvincible(false);

		if (PlayerHandler.AllowVibration)
			StartCoroutine(Rumble(0.2f, 1, 1));
	}

	void SetHitTriggerSize(float size)
	{
		SphereCollider sphere = (SphereCollider)hitTrigger;
		sphere.radius = size;
	}

	IEnumerator Rumble(float time, float leftMotor, float rightMotor)
	{
		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, leftMotor, rightMotor);

		yield return new WaitForSeconds(time);

		if (PlayerHandler.AllowVibration)
			GamePad.SetVibration(0, 0, 0);
	}

	void SetHitTriggerFromSpeed()
	{
		bool enableHitTrigger = speedThisFrame > 0.75f;
		if (enableHitTrigger != hitTrigger.enabled)
		{
			hitTrigger.enabled = enableHitTrigger;
			HealthManager.instance.SetExternalIsInvincible(enableHitTrigger);
		}
	}

	void SetMotionParticleFromSpeed(bool turnOffDirect = false)
	{
		bool turnOn = speedThisFrame > 0.01f;
		if (!IsGrounded()) turnOn = false;
		if (IsSlamming || JustSlammed) turnOn = false;
		if (turnOffDirect) turnOn = false;

		bool turnOnSound = false;
		if (turnOn) turnOnSound = speedThisFrame > 0.1f;

		var main = ballRollDust.main;
		Color col = main.startColor.color;
		main.startColor = new Color(col.r, col.g, col.b, turnOn ? 1 : 0);

		if (turnOnSound && !SoundManager.instance.HasLoopingClip)
			SoundManager.instance.PlayClipLooping("RollingBall");
		
		if (!turnOnSound && SoundManager.instance.HasLoopingClip)
			SoundManager.instance.FadeOutLoopingClip();
	}

	bool CanDoHighJump()	// not about to land on steep slope
	{
		RaycastHit hitInfo;
		Vector3 lastRbPos = rb.transform.position;
		rb.transform.position += Vector3.up * 0.5f; // move up a bit before sweep test
		bool canDoIt = false;

		if (rb.SweepTest(Vector3.down, out hitInfo, 100, QueryTriggerInteraction.Ignore))
		{
			if (hitInfo.collider.gameObject.GetComponent<ForceSlide>())
				return false;

			Vector3 normal = hitInfo.normal;
			float angle = Vector3.Dot(normal, Vector3.down);
			angle = 180 - (Mathf.Acos(angle) * Mathf.Rad2Deg);

			if (float.IsNaN(angle)) angle = 0;

			if (angle <= maxClimbAngle)
				canDoIt = true;
		}

		rb.transform.position = lastRbPos;
		return canDoIt;
	}

	// called when switching human -> ball
	public void SetVelocityChange(Vector3 velocityChange)
	{
		velocityChange = velocityChange * (acceleration * 0.2f);
		rb.AddForce(velocityChange, ForceMode.VelocityChange);
	}

	public override void EnableByHandler(Vector3 velocityChange, bool doHop)
	{
		SetInterpolation(RigidbodyInterpolation.Interpolate);

		// calc this AS SOON AS we start, otherwise it has to wait until end of fixed update
		speedThisFrame = CalcSpeed;
		hitTrigger.enabled = false;
		SetHitTriggerSize(hitTriggerStartSize);
		HealthManager.instance.SetExternalIsInvincible(false);

		rb.isKinematic = false;
		rb.useGravity = true;
		capsuleCollider.height = capsuleHeight;
		capsuleCollider.radius = capsuleBallRadius;
		ballCollider.DisableTwirlingPlatform();

		if (doHop) transform.position += Vector3.down * transitionHopAmount;
		if (velocityChange != Vector3.zero) SetVelocityChange(velocityChange);

		enabled = true;
		enabledStamp = Time.frameCount;

		HandleCollision();
	}

	public override void DisableByHandler(PlayerHandler.PlayerState nextState)
	{
		SetInterpolation(RigidbodyInterpolation.Interpolate);

		SoundManager.instance.EndClipLooping();
		KillVelocity();

		hitTrigger.enabled = false;
		SetHitTriggerSize(hitTriggerStartSize);
		HealthManager.instance.SetExternalIsInvincible(false);

		ballCollider.DisableMovingPlatform();
		ballCollider.DisableTwirlingPlatform();

		extraGravity = regExtraGravity;
		isFrozen = false;
		atMaxVel = false;

		rb.isKinematic = true;
		rb.useGravity = false;
		rb.drag = responsiveDrag;   // must be 0
		capsuleCollider.radius = capsuleStartRadius;

		enableReJump = false;
		storedJumpInput = false;
		enabled = false;

		playerHandler.Landed();
		atMaxVel = false;

		SetMotionParticleFromSpeed(true);
		CancelChargeParticle();

		airState = AirState.Nothing;
	}

	public override void PushAway(Vector3 direction, float force)
	{
		rb.velocity = Vector3.zero;
		rb.AddForce(direction * force, ForceMode.VelocityChange);
	}

	public void PushAway(Vector3 direction, float force, float exceedMaxVelTime = 0)
	{
		rb.velocity = Vector3.zero;
		rb.AddForce(direction * force, ForceMode.VelocityChange);

		if (exceedMaxVelTime > 0)
			StartCoroutine(DisableVelocityCap(exceedMaxVelTime));
	}

	public void PushForward(float force, float exceedMaxVelTime = 0)
	{
		Vector3 forward = GetDirection;

		rb.velocity = Vector3.zero;
		rb.AddForce(forward * force, ForceMode.VelocityChange);

		if (exceedMaxVelTime > 0)
			StartCoroutine(DisableVelocityCap(exceedMaxVelTime));
	}

	IEnumerator DisableVelocityCap(float time)
	{
		useMaxVelocity = false;
		yield return new WaitForSeconds(time);
		useMaxVelocity = true;
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
	}

	public override void SetFreezeJump(bool frozen)
	{
		base.SetFreezeJump(frozen);
	
		if (IsJumpFrozen)
			CancelBallSlam(false);
	}

	public override void SetHorizontalFrozen(bool frozen)
	{
		horizontalFrozen = frozen;

		if (horizontalFrozen)
			rb.velocity = new Vector3(0, rb.velocity.y, 0);
	}

	public void CancelBallSlam(bool killVelocity)
	{
		CancelChargeParticle();
		extraGravity = regExtraGravity;
		isFrozen = false;

		if (killVelocity)
			KillVelocity();
	}

	void CancelChargeParticle()
	{
		StopCoroutine("ShowChargeParticle");
		chargeParticle.transform.localScale = Vector3.zero;
	}

	public bool StartBallSlam(bool doExtraLongPause, float neededDist = neededHeightForBallSlam)
	{
		float floorPoint = SweepDown();
		float distDown = transform.position.y - floorPoint;

		if (atMaxVel) return false;
		if (IsJumpFrozen) return false;

		if (distDown > neededDist)
		{
			const float ParticleExtraSpeed = 1.5f;
			StartCoroutine("ShowChargeParticle", doExtraLongPause ? 1 : ParticleExtraSpeed);

			DoBallSlam(doExtraLongPause, floorPoint);
			return true;
		}

		return false;
	}

	void DoBallSlam(bool doExtraLongPause, float floorPoint)
	{
		allowStickToFloorBox = false;

		//distToSlam = floorPoint + capsuleCollider.radius;
		StartCoroutine("FreezeThenSlam", doExtraLongPause);

		hitTrigger.enabled = true;
		SetHitTriggerSize(hitTriggerSlamSize);

		HealthManager.instance.SetExternalIsInvincible(true);
	}

	IEnumerator FreezeThenSlam(bool doExtraLongPause)
	{
		isFrozen = true;
		float t = 0;

		float freezeSeconds = 0.08f;
		if (doExtraLongPause) freezeSeconds = 0.25f;

		while (t < freezeSeconds)
		{
			t += Time.deltaTime;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.useGravity = false;
			yield return null;
		}

		rb.useGravity = true;
		isFrozen = false;
		enableReJump = false;
		storedJumpInput = false;
		extraGravity = slamExtraGravity;
	}

	IEnumerator ShowChargeParticle(float speedMod)
	{
		float GrowSpeed = 6 * speedMod;
		float ShrinkSpeed = 8 * speedMod;
		const float MaxSize = 1.6f;

		chargeParticle.transform.localScale = Vector3.zero;

		while (chargeParticle.transform.localScale.x < MaxSize)
		{
			chargeParticle.transform.localScale += (Vector3.one * GrowSpeed * Time.deltaTime);
			yield return null;
		}

		while (chargeParticle.transform.localScale.x > 0.1f)
		{
			chargeParticle.transform.localScale -= (Vector3.one * ShrinkSpeed * Time.deltaTime);
			yield return null;
		}

		chargeParticle.transform.localScale = Vector3.zero;
	}

	float SweepDown()
	{
		const float MaxDist = 300;
		RaycastHit hitInfo;
		rb.transform.position += Vector3.up * 0.2f;

		bool rbHit = rb.SweepTest(Vector3.down, out hitInfo, MaxDist, QueryTriggerInteraction.Ignore);
		rb.transform.position -= Vector3.up * 0.2f;

		if (rbHit)
		{
			return hitInfo.point.y;
		}
		else return float.MinValue;
	}

	public override void DoHighJump(float pressHeight, float noPressHeight)
	{
		if (IsSlamming)
		{
			CancelBallSlam(true);
			pressHeight = pressHeight * 1.2f;
			noPressHeight = pressHeight;
		}

		const float consistantWithHuman = 0.6f;
		float jSpeed = Input.GetButton(PlayerHandler.JumpString) ? pressHeight : noPressHeight;

		enableReJump = false;       // make sure our movement isn't limited by jump function (sets y = 0)
		storedJumpInput = false;
		afterSlamStamp = Time.frameCount;   // fake slam, such that we don't limit movement
		rb.angularVelocity = Vector3.zero;
		rb.drag = responsiveDrag;	// just wanna be sure, you know? /shrug
		rb.MovePosition(rb.position + Vector3.up * 0.2f);	// move up slightly so we don't collide

		rb.velocity = new Vector3(rb.velocity.x, jSpeed * consistantWithHuman, rb.velocity.z);
	}
}
