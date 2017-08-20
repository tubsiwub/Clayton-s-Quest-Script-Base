using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHandler : MonoBehaviour
{
	public enum PlayerState { Human, Ball, Slider, OnRope };
	public enum SecondaryAction { None, Holding, OnLedge };

	[SerializeField] bool doSlopes = true;
	[SerializeField] bool doLedges = false;

	[SerializeField] Transform rotateMesh;
	public Transform RotateMesh { get { return rotateMesh; } }

	[SerializeField] GameObject ballMesh;

	[SerializeField] Animator humanAnimator;
	public Animator HumanAnimator { get { return humanAnimator; } }

	[SerializeField] Animator faceAnimator;

	Renderer[] humanRends;

	HumanController humanController;
	BallController ballController;
	SliderController sliderController;
	HumanRopeController humanRopeController;

	PlayerAttack playerAttack;
	PlayerHolder playerHolder;
	LedgeCheckController ledgeCheckController;
	RotateMeshLocation rotateMeshLocation;

	PlayerController currentController;

	Vector3 respawnPos = Vector3.zero;
	public bool HasRespawnPos { get { return respawnPos != Vector3.zero; } }
	const float deathY = -50;

	const float maxFallDist = 30;
	float heightAtMaxFall = 0;
	bool setFallDeath = false;

	HealthManager.AnimType lastDeathAnim = HealthManager.AnimType.None;
	public HealthManager.AnimType LastDeathAnim { get { return lastDeathAnim; } }
	bool AnimatingDeath { get { return lastDeathAnim != HealthManager.AnimType.None && lastDeathAnim != HealthManager.AnimType.None; } }

	[SerializeField] LayerMask castLayers;

	Transform nearestRope = null;

	Checkpoint lastCheckpoint = null;
	public Checkpoint LastCheckpoint { get { return lastCheckpoint; } }

	Origami_Collect currentOrigami;
	public bool HasOrigami { get { return currentOrigami != null; } }

	float restartButtonHeld = 0;
	const float restartButtonHoldAmount = 0.5f;

	string[,] faceEyesMap = new string[,]
		{ { "Smile", "Eyes_Normal" } , { "Frown", "Eyes_Gloomy" }, { "Overjoyed", "Eyes_Joyous" }, { "Angry", "Eyes_Angry" },
		  { "Confused", "Eyes_Inquisitive" }, { "Distraught", "Eyes_Closed" }, { "Shocked", "Eyes_Shocked" } };

	public bool CanSit { get { return CurrentState == PlayerState.Human && 
		CurrentSecondaryAction == SecondaryAction.None && !playerHolder.HasObjectsNearby; } }

	public bool SeatInRange { set; get; }

	public PlayerState CurrentState {		// to do - this should be less bad
		get
		{
			if (currentController == humanController)
				return PlayerState.Human;
			else if (currentController == ballController)
				return PlayerState.Ball;
			else if (currentController == sliderController)
				return PlayerState.Slider;
			else
				return PlayerState.OnRope;
		}
	}

	SecondaryAction secondaryAction;
	public SecondaryAction CurrentSecondaryAction { get { return secondaryAction; } }

	public bool IsGrounded() { return currentController.IsGrounded(); }
	public bool IsFalling() { return currentController.IsFalling(); }
	public Vector3 GetVelocity() { return currentController.GetVelocity(); }
	public bool IsMoving() { return currentController.GetVelocity().magnitude > 0.9f; }
	public bool IsFrozen { get { return currentController.IsFrozen; } }
	public bool IsPhysicsControlled { get { return currentController.IsPhysicsControlled; } }
	public bool JustSlammed { get { return ballController.JustSlammed; } }

	public void KillVelocity() { currentController.KillVelocity(); }
	public void DisableCurrent() { currentController.DisableByHandler((PlayerState)(-1)); }
	public void SetDetectCollisions(bool set) { currentController.SetDetectCollisions(set); }

	bool justHighJumped = false;
	const int totalHighBounceFrames = 3;
	int highBounceFrame = 0;

	public bool JustHighJumped { get { return justHighJumped; } }
	public void DoHighJump(float pressHeight, float noPressHeight)
		{ justHighJumped = true; highBounceFrame = Time.frameCount;
		currentController.DoHighJump(pressHeight, noPressHeight); }

	public static bool AllowVibration { set; get; }
	public static bool UsingPrimaryAttack { set; get; }
	public static string AttackString
	{ get {
			if (CameraControlDeluxe.CursorLockFrame+1 == Time.frameCount) return "None";
			return UsingPrimaryAttack ? "AttackPrimary" : "AttackSecondary";
	} }

	public static bool UsingPrimaryJump { set; get; }
	public static string JumpString
	{ get {
			if (CameraControlDeluxe.CursorLockFrame+1 == Time.frameCount) return "None";
			return UsingPrimaryJump ? "JumpPrimary" : "JumpSecondary";
	} }

	public static void SetClickToJump(bool set) { UsingPrimaryAttack = !set; UsingPrimaryJump = !set; }
	private static bool searchedForAndrewFile = false;

	bool ready = false;
	public bool Ready { get { return ready; } }

	public static bool CanUpdate { get { return (Time.deltaTime <= 0.1f && Time.timeScale > 0.1f); } }


	public void SetFrozen(bool frozen, bool freezeAnimator)
	{
		currentController.SetFrozen(frozen, freezeAnimator);

		if (!frozen && !ready)
			ready = true;
	}

	// use this to push the player away in a certain direction
	// direction: normalized vector for which direction to push
	// force: how much force to apply in this direction
	public void PushAway(Vector3 direction, float force) { currentController.PushAway(direction, force); }

	void Awake()
	{
		AllowVibration = false;

		if (!searchedForAndrewFile)
			SetClickToJump(AndrewFileFound());

		if (transform.rotation != Quaternion.identity)
			ResetStartRotation();

		if (!GameObject.Find("Loader"))
			Debug.LogError("Your scene is missing a Loader prefab! You'll need this.");
	}

	bool AndrewFileFound()
	{
		searchedForAndrewFile = true;

		try
		{
			using (System.IO.StreamReader sr = new System.IO.StreamReader("Andrew.txt"))
				return true;
		}
		catch
		{
			return false;
		}
	}

	void ResetStartRotation()
	{
		string msg = "Hi, I'm Clayton! Remember: my default rotation should start at (0, 0, 0). ";
		msg += "I've gone ahead and reset myself for you! Contact Andrew if you have any questions or concerns.";

		if (!SceneManager.GetActiveScene().name.Contains("Tristan"))
			Debug.LogWarning(msg);

		transform.rotation = Quaternion.identity;
	}

	void Start()
	{
		humanController = GetComponent<HumanController>();
		ballController = GetComponent<BallController>();
		sliderController = GetComponent<SliderController>();
		humanRopeController = GetComponent<HumanRopeController>();

		playerAttack = GetComponent<PlayerAttack>();
		playerHolder = GetComponentInChildren<PlayerHolder>();
		rotateMeshLocation = GetComponentInChildren<RotateMeshLocation>();
		ledgeCheckController = GetComponent<LedgeCheckController>();
		ledgeCheckController.enabled = doLedges;

		ballController.DisableByHandler(PlayerState.Human);
		currentController = humanController;

		SetFaceAnimation("Smile", true);
		secondaryAction = SecondaryAction.None;

		humanRends = GetComponentsInChildren<SkinnedMeshRenderer>();

		SetFrozen(true, true);
	}

	public void ToBallAnimComplete()
	{
		if (CurrentState == PlayerState.Ball)
		{
			ballMesh.SetActive(true);
			rotateMesh.localPosition = Vector3.zero;

			for (int i = 0; i < humanRends.Length; i++)
				humanRends[i].enabled = false;
		}
	}

	public void ToHumanAnimComplete()
	{
		// stuff used to happen here, but now it doesn't. meh. 
	}

	public void SetRope(Transform nearest)
	{
		nearestRope = nearest;
	}

	public void SetCheckpoint(Checkpoint newCheckpoint)
	{
		if (lastCheckpoint != null &&
		newCheckpoint == lastCheckpoint) return;

		if (lastCheckpoint != null) lastCheckpoint.Deactivate();
		lastCheckpoint = newCheckpoint;
		lastCheckpoint.Activate();
		respawnPos = lastCheckpoint.RespawnPos.position;
		SetFaceAnimation("Overjoyed", 1.5f);
	}

	public void SetCheckpoint(Vector3 newCheckpoint)
	{
		if (lastCheckpoint != null && 
		newCheckpoint == lastCheckpoint.RespawnPos.position) return;

		respawnPos = newCheckpoint;
	}

	public void FindAndSetCheckpoint(string objName)
	{
		StartCoroutine(WaitThenSetCP(objName));
	}

	IEnumerator WaitThenSetCP(string objName)
	{
		yield return null;

		GameObject cp = GameObject.Find(objName);

		if (cp != null)
		{
			SetCheckpoint(cp.GetComponent<Checkpoint>());
		}
	}

	public void SetDeathAnimation(HealthManager.AnimType animType)
	{
		lastDeathAnim = animType;

		if (animType != HealthManager.AnimType.None)
		{
			if (CurrentState == PlayerState.Ball)
				SwitchStateInstant(PlayerState.Human);

			bool fallToFloor = !IsGrounded() && (animType == HealthManager.AnimType.Default || animType == HealthManager.AnimType.BallToHuman);
			if (fallToFloor)
				humanController.SetFloored();
			else
				SetFrozen(true, false);

			SetFaceAnimation("Shocked");
			humanAnimator.SetLayerWeight(2, 1);
			DisableCurrent();

			if (fallToFloor)		// re-enable so we can continue the SetFloored routine
				humanController.EnableByHandler(Vector3.zero, false);

			if (HasOrigami)
				DisappearOrigami();
		}

		switch (animType)
		{
			case HealthManager.AnimType.Default:
				SoundManager.instance.PlayClip("Faint01");
				int r = Random.Range(1, 4);
				humanAnimator.CrossFade("hero_death0" + r, 0.1f);
			break;

			case HealthManager.AnimType.Drown:
				SoundManager.instance.PlayClip("Gurgle01");
				humanAnimator.CrossFade("heroDrown", 0.1f);
			break;

			case HealthManager.AnimType.Splat:
				SoundManager.instance.PlayClip("Faint01");
				humanAnimator.SetTrigger("FallSplat");
			break;

			case HealthManager.AnimType.BallToHuman:
				SoundManager.instance.PlayClip("Faint01");
				rotateMeshLocation.enabled = false;
				rotateMesh.localPosition += Vector3.down * 0.5f;	// account for animation offset
				humanAnimator.CrossFade("ball-to-hero_death", 0.1f);
			break;

			case HealthManager.AnimType.None:
				humanAnimator.SetLayerWeight(2, 0);
				humanAnimator.CrossFade("hero_idle", 0.1f);
				SetFaceAnimation("Smile");
			break;
		}
	}

	public void SetOrigamiAnimation(Origami_Collect currentOrigami)
	{
		if (CurrentState == PlayerState.Ball)
			SwitchStateInstant(PlayerState.Human);

		Vector3 facingDir = (currentOrigami.transform.position - transform.position).normalized;
		facingDir.y = 0; rotateMesh.forward = facingDir;

		humanController.SetFloored();
		humanAnimator.CrossFade("hero_itemHug", 0.1f);
		this.currentOrigami = currentOrigami;
	}

	public void PickupOrigami()
	{
		if (currentOrigami == null) return;

		currentOrigami.Pickup(playerHolder.GetLeftHand, playerHolder.transform);
	}

	public void DisappearOrigami()
	{
		if (currentOrigami == null) return;

		currentOrigami.Disappear();
		currentOrigami = null;
	}

	public void FinishOrigamiAnimation()
	{
		SetFrozen(false, false);
		humanAnimator.CrossFade("hero_idle", 0.05f);
	}

	public void SetSit(bool set, Vector3 forward)
	{
		forward.y = 0;
		rotateMesh.forward = forward;
		transform.position += rotateMesh.forward * (0.45f * (set ? -1 : 1));
		transform.position += Vector3.down * (0.2f * (set ? 1 : -1));
		humanAnimator.SetTrigger(set ? "Sit" : "Stand");
	}

	string GetFaceStateName(string cond)
	{
		for (int i = 0; i < faceEyesMap.Length; i++)
		{
			if (cond == faceEyesMap[i, 0])
				return faceEyesMap[i, 1];
		}
		return "";
	}

	bool IsCurrentOrNextFace(string face)
	{
		return (faceAnimator.GetCurrentAnimatorStateInfo(1).IsName(GetFaceStateName(face)) ||
				faceAnimator.GetNextAnimatorStateInfo(1).IsName(GetFaceStateName(face)));
	}

	public void SetFaceAnimation(string face, bool forceSwitch = false)
	{
		if (faceAnimator.isInitialized && (!IsCurrentOrNextFace(face) || forceSwitch))
			faceAnimator.SetTrigger(face);
	}

	public void SetFaceAnimation(string face, float time)
		{ if (faceAnimator.isInitialized && !IsCurrentOrNextFace(face)) StartCoroutine(DoFaceAnimation(face, time)); }
	IEnumerator DoFaceAnimation(string face, float time)
	{
		SetFaceAnimation(face);
		yield return new WaitForSeconds(time);
		if (faceAnimator.isInitialized && IsCurrentOrNextFace(face))
			SetFaceAnimation("Smile");
	}

	void CancelMaxFallSpeed()
	{
		heightAtMaxFall = 0;
		setFallDeath = false;
	}

	public void Landed()
	{
		if (setFallDeath)
			HealthManager.instance.LoseAllLives(HealthManager.AnimType.Splat);
		else
			CancelMaxFallSpeed();
	}

	public void AtMaxFallSpeed()
	{
		heightAtMaxFall = transform.position.y;
	}

	void BeginFallDeath()
	{
		bool wasBall = CurrentState == PlayerState.Ball;
		if (CurrentState == PlayerState.Ball)
			SwitchStateInstant(PlayerState.Human);

		humanController.PrepareForLanding();
		humanAnimator.SetLayerWeight(2, 1);
		humanAnimator.SetTrigger(wasBall ? "FallDeathFromBall" : "FallDeath");
		setFallDeath = true;
	}

	bool showText = false;
	void OnGUI()
	{
		if (showText)
		{
			GUIStyle style = new GUIStyle();
			style.fontSize = 32;
			Vector3 vel = GetVelocity(); vel.y = 0;
			GUI.Label(new Rect(10, Screen.height - 40, 300, 300), vel.magnitude + "", style);
		}
	}

	void Update()
	{
		if (!CanUpdate) return;

		if (Input.GetKeyDown(KeyCode.F2))
			showText = !showText;

		if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftShift))
		{
			SavingLoading.instance.SaveSceneInfo();

			GetComponent<ObjInfo>().SAVE(true, true);
			LevelManager.instance.NewPlayerData();

			SavingLoading.instance.SaveCheckpoint();
			SavingLoading.instance.SaveData();
		}

		if (Input.GetKeyDown(KeyCode.O) && Input.GetKey(KeyCode.LeftShift))
		{
			SavingLoading.instance.LoadSceneInfo();
			//SavingLoading.instance.LoadCheckpoint();
			// dont load player location, that is handled from LevelManager (after scenes loaded)
		}

		if (CanDoBallToggling())
			HandleBallToggling();

		if (CanDoRope())
			SwitchState(PlayerState.OnRope);

		HandleRestartInput();

		if (transform.position.y < deathY)
			HealthManager.instance.LoseAllLives();

		if (heightAtMaxFall - transform.position.y > maxFallDist && !setFallDeath)
			BeginFallDeath();
	}

	bool CanDoBallToggling()
	{
		return CurrentState != PlayerState.OnRope && CurrentState != PlayerState.Slider;
	}

	bool CanDoRope()
	{
		return nearestRope != null && transform.position.y < nearestRope.position.y && 
			!IsGrounded() && humanRopeController.LastJumpedFrame < Time.frameCount - 30;
	}

	void HandleRestartInput()
	{
		if (Input.GetButtonUp("Restart") || !HealthManager.instance.CanGetHurt)
		{
			restartButtonHeld = 0;
			return;
		}

		if (Input.GetButton("Restart"))
			restartButtonHeld += Time.deltaTime;

		if (restartButtonHeld > restartButtonHoldAmount)
		{
			restartButtonHeld = 0;
			HealthManager.instance.LoseAllLives();
		}
	}

	void HandleBallToggling()
	{
		float axis = Input.GetButton("BallToggle") == true ? -1 : 0;
		if (axis == 0) axis = Input.GetAxisRaw("BallToggle");

		PlayerState newState = CurrentState;

		if (CloseEnough(axis, 0.0f))
		{
			if (CanBeHuman())
				newState = PlayerState.Human;
		}
		else if (CloseEnough(axis, -1.0f))
			newState = PlayerState.Ball;

		if (newState != CurrentState)
			SwitchState(newState);
	}

	bool CanBeHuman()
	{
		return !currentController.BallCollidingUpward() && !ballController.SearchingForSlide && !ballController.IsSlamming;
	}

	bool CloseEnough(float num, float target)
	{
		return (Mathf.Abs(num - target) < 0.2f);
	}

	public void Respawn()
	{
		SetFrozen(false, false);
		if (CurrentState != PlayerState.Human)
			SwitchState(PlayerState.Human);

		restartButtonHeld = 0;
		CancelMaxFallSpeed();
		currentController.EnableByHandler(Vector3.zero, false);
		humanController.CancelSetFloored();

		transform.position = respawnPos;

		if (lastCheckpoint != null)
			rotateMesh.forward = lastCheckpoint.RespawnPos.forward;
		else
			rotateMesh.forward = Vector3.forward;
		
		Camera.main.GetComponent<CameraControlDeluxe>().DoCamBehindPlayer();

		rotateMeshLocation.enabled = true;
		rotateMesh.localPosition = Vector3.zero;
		rotateMesh.localRotation = Quaternion.Euler(0, rotateMesh.localEulerAngles.y, 0);
	}

	void LateUpdate()
	{
		if (justHighJumped && IsGrounded())
			StartCoroutine(CheckForHighBounce());
	}

	IEnumerator CheckForHighBounce()
	{
		yield return null;

		if ((justHighJumped && IsGrounded()) &&
			Time.frameCount - highBounceFrame > totalHighBounceFrames)
		{
			justHighJumped = false;
		}
	}

	public void SetSecondaryAction(SecondaryAction action)
	{
		if (!currentController.HandsAvailable)
		{
			playerHolder.enabled = false;
			playerAttack.enabled = false;
			ledgeCheckController.enabled = false;
			secondaryAction = SecondaryAction.None;
			return;
		}

		// if last was on ledge, and new is off ledge, turn on rotateMeshLocation
		if (action == SecondaryAction.OnLedge)
			rotateMeshLocation.enabled = false;
		else if (secondaryAction == SecondaryAction.OnLedge)
			rotateMeshLocation.enabled = true;

		secondaryAction = action;

		if (secondaryAction == SecondaryAction.None)
		{
			playerHolder.enabled = true;
			playerAttack.enabled = true;
			ledgeCheckController.enabled = doLedges ? true : false;
		}

		if (secondaryAction == SecondaryAction.Holding)
		{
			playerAttack.enabled = false;
			ledgeCheckController.enabled = false;
		}

		if (secondaryAction == SecondaryAction.OnLedge)
		{
			playerAttack.enabled = false;
			playerHolder.enabled = false;
			humanController.ResetJumpState();
		}
	}

	public void SwitchState(PlayerState newState)
	{
		if (IsFrozen) return;   // can't switch state NO MATTER WHAT
		if (newState == PlayerState.Slider && !doSlopes) return;    // temp
		if (setFallDeath) return;
		if (AnimatingDeath) return;

		HealthManager.instance.SetExternalIsInvincible(false);

		switch (newState)
		{
			case PlayerState.Human:
				if (CurrentState == PlayerState.Ball)	BallToHuman();
				if (CurrentState == PlayerState.OnRope)	ClimbRopeToHuman();
				if (CurrentState == PlayerState.Slider)	SliderToHuman();
			break;

			case PlayerState.Ball: HumanToBall(); break;

			case PlayerState.OnRope:
				if (CurrentState == PlayerState.Human) HumanToClimbRope();
			break;

			case PlayerState.Slider:
				if (CurrentState == PlayerState.Human) HumanToSlider();
				break;
		}

		SetSecondaryAction(secondaryAction);
	}

	// will only work properly for human and ball
	public void SwitchStateInstant(PlayerState newState)
	{
		if (IsFrozen) return;   // can't switch state NO MATTER WHAT
		if (newState == PlayerState.Slider && !doSlopes) return;    // temp
		if (setFallDeath) return;
		// if (AnimatingDeath) return;		// do not worry about this check for INSTANT (since we'll be changing while animating)

		HealthManager.instance.SetExternalIsInvincible(false);

		switch (newState)
		{
			case PlayerState.Human:
				if (CurrentState == PlayerState.Ball) BallToHumanInstant(); break;

			case PlayerState.Ball:
				if (CurrentState == PlayerState.Human) HumanToBallInstant(); break;

			case PlayerState.Slider:
				if (CurrentState == PlayerState.Ball) BallToSliderInstant(); break;
		}

		SetSecondaryAction(secondaryAction);
	}

	void BallToHuman()
	{
		SoundManager.instance.PlayClip("BallToHuman0" + Random.Range(1, 4));

		Vector3 dir = ballController.GetDirection; dir.y = 0;
		dir *= ballController.GetSpeed;
		float jumpVel = ballController.GetVelocity().y;

		if (ballController.GetMovingPlatform != null)
		{
			humanController.SetMovingPlatform(ballController.GetMovingPlatform);
			transform.position += ballController.GetMovingPlatform.Distance;
		}

		ballController.DisableByHandler(PlayerState.Human);
		humanController.EnableByHandler(dir, true);
		humanController.SIMPLE_JUMP((jumpVel > 0) ? (jumpVel * 1.5f) : (jumpVel * 0.2f));

		ballMesh.SetActive(false);

		for (int i = 0; i < humanRends.Length; i++)
			humanRends[i].enabled = true;

		rotateMeshLocation.enabled = true;
		SetFaceAnimation("Smile");

		humanAnimator.SetTrigger("BallToHuman");

		currentController = humanController;
	}

	void BallToHumanInstant()
	{
		Vector3 dir = ballController.GetDirection; dir.y = 0;

		ballController.DisableByHandler(PlayerState.Human);
		humanController.EnableByHandler(Vector3.zero, true);

		ballMesh.SetActive(false);

		for (int i = 0; i < humanRends.Length; i++)
			humanRends[i].enabled = true;

		rotateMeshLocation.enabled = true;

		if (dir != Vector3.zero)
			rotateMesh.forward = dir;

		humanAnimator.CrossFade("hero_idle", 0.0f);
		SetFaceAnimation("Smile");

		currentController = humanController;
	}

	void HumanToBall()
	{
		SoundManager.instance.PlayClip("HumanToBall0" + Random.Range(1, 3));

		rotateMeshLocation.enabled = false;
		rotateMesh.localPosition = new Vector3(0, 0.65f, 0);	// set position relative to animation ending state (ball)
		rotateMesh.localPosition += -0.3f * rotateMesh.forward;
		// to-do: make this a bit more accurate while human is moving into ball form

		humanAnimator.CrossFade("hero-to-ball_transform", 0.1f);    // force the transition to this state

		Vector3 vel = humanController.GetVelocity(); vel.y = 0;
		bool humanGrounded = currentController.IsGrounded();

		if (humanController.GetMovingPlatform != null)
		{
			ballController.SetMovingPlatform(humanController.GetMovingPlatform);
			transform.position += humanController.GetMovingPlatform.Distance;
		}

		humanController.DisableByHandler(PlayerState.Ball);
		ballController.EnableByHandler(humanGrounded ? vel.normalized : Vector3.zero, true);

		if (!humanGrounded)
			ballController.StartBallSlam(true);

		currentController = ballController;
	}

	void HumanToBallInstant()
	{
		rotateMeshLocation.enabled = false;
		rotateMesh.localPosition = new Vector3(0, 0.65f, 0);    // set position relative to animation ending state (ball)
		rotateMesh.localPosition += -0.3f * rotateMesh.forward;
		// to-do: make this a bit more accurate while human is moving into ball form

		ToBallAnimComplete();

		humanController.DisableByHandler(PlayerState.Ball);
		ballController.EnableByHandler(Vector3.zero, true);

		currentController = ballController;
	}

	void SliderToHuman()
	{
		rotateMeshLocation.enabled = true;
		sliderController.DisableByHandler(PlayerState.Human);
		humanController.EnableByHandler(Vector3.zero, true);   // to-do: send vel change from slider
		
		currentController = humanController;
	}

	public bool CanBeSlider()
	{
		if (IsFrozen) return false;
		if (FoundNearbyNoSlide()) return false;
		if (!humanController.CheckForSteepSlope()) return false;

		return true;
	}

	bool FoundNearbyNoSlide()
	{
		const float maxSendDist = 0.01f;    // don't shoot the "ray" very far, we want sphere on us
		const float sphereRadius = 2;
		RaycastHit[] hits = Physics.SphereCastAll(new Ray(transform.position, Vector3.forward * maxSendDist),
			sphereRadius, maxSendDist, castLayers, QueryTriggerInteraction.Ignore);

		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].collider.gameObject.GetComponent<NoSlide>())
				return true;
		}

		return false;
	}

	void BallToSliderInstant()
	{
		BallToHumanInstant();

		if (CanBeSlider())
			HumanToSlider();
	}

	void HumanToSlider()
	{
		rotateMeshLocation.enabled = false;
		humanController.DisableByHandler(PlayerState.Slider);
		sliderController.EnableByHandler(Vector3.zero, false);
		currentController = sliderController;
	}

	void HumanToClimbRope()
	{
		humanController.DisableByHandler(PlayerState.OnRope);
		humanRopeController.SetRope(nearestRope);
		humanRopeController.EnableByHandler(Vector3.zero, false);

		currentController = humanRopeController;
	}

	void ClimbRopeToHuman()
	{
		humanRopeController.DisableByHandler(PlayerState.Human);
		humanController.EnableByHandler(Vector3.zero, false);

		currentController = humanController;
	}
}
