using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CameraControl : MonoBehaviour {

	/*public bool DEBUG = false;

	public float cameraViewSpeed = 6;

	float
	distanceAway = 5,
	distanceUp = 2,
	distanceFrom = 3,
	scrollSpeed = 6,
	rightX,
	rightY,
	oldRayHeight = 0,
	oldRayChange = 0,
	rayChangeAmt = 0;

	const float xSpeed = 190;
	const float ySpeed = 6.25f;


	Transform
	follow,
	rotateMesh;


	GameObject
	playerObj;

	// Inverts control scheme
	[SerializeField] bool invertX = true;
	[SerializeField] bool invertY = true;

	bool
	playerGrounded = false,
	adjustCamera = false,
	playerSloped = false,
	playerRoped = false,
	playerHighFalling = false;

	// Used as a toggle for camera snapping
	bool cameraStateBehind = false;

	PlayerHandler.PlayerState playerLastState;
	PlayerHandler.PlayerState playerState;

	Vector2
	rightStickPrevFrame = Vector2.zero;

	Vector3
	targetPosition,
	curLookDir,
	savedPosition, 
	storedHitPoint = Vector3.zero,
	characterOffset = Vector3.zero,
	storedWallHitPoint = Vector3.zero;

	// Specific look direction of camera object
	Vector3 lookAtPosition;

	// Position stored where player was last on solid ground used for aiming camera view
	Vector3 playerGroundPosition;

	// andrew is using this in HealthManager, so the camera will spaz out less after respawning
	public bool DirectFollow { set; get; }


	#region ENUM
	public enum CameraStates{

		BEHIND,
		TARGET,
		FREE
	}

	public CameraStates camState = CameraStates.BEHIND;
	#endregion


	void Awake(){

		// controls framerate (turns off VSync automatically)
		//FramerateAdjust (60);

	}


	void Start () {

		Cursor.lockState = CursorLockMode.Locked;

		Initialize ();

	}


	void Update () {

		LockCursor ();

	}

	void Initialize(){

		// Set objects
		playerObj = GameObject.FindWithTag ("Player");
		follow = GameObject.Find ("Follow").transform;
		rotateMesh = GameObject.Find("RotateMesh").transform;

		playerState = playerObj.GetComponent<PlayerHandler> ().CurrentState;	

		// Set values
		curLookDir = follow.forward;
		lookAtPosition = playerObj.transform.position;
		playerGroundPosition = playerObj.transform.position;
		DirectFollow = false;

	}

	// no longer being called, go away pls
	void FramerateAdjust(int framerate){
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = framerate;
	}


	// Locks the cursor within the game window
	void LockCursor(){

		// Locks the mouse to the game screen / window
		if (Input.GetMouseButtonDown(0))
			Cursor.lockState = CursorLockMode.Locked;

		// Turns mouse visibility on / off depending.
		if (Cursor.lockState == CursorLockMode.Locked)
			Cursor.visible = false;
		else 
			Cursor.visible = true;

	}


	void OnGUI(){

		// Must be within OnGUI
		DistanceFrom ();

	}


	// Changes camera depth (distance between player and camera) with scroll wheel
	void DistanceFrom(){

		Event scrollEvent = Event.current;

		if (distanceFrom >= -2f && distanceFrom <= 4)
		if(scrollEvent.type == EventType.ScrollWheel){
			distanceFrom += (scrollEvent.delta.y / scrollSpeed) * 20 * Time.deltaTime;
		}
		if(distanceFrom < -2f) distanceFrom = -2f;
		if(distanceFrom > 4) distanceFrom = 4;

	}


	// Values meant to be reset every frame
	void ConstantInit(){

		targetPosition = Vector3.zero;
		playerGrounded = playerObj.GetComponent<PlayerHandler> ().IsGrounded ();	// Player is on ground?
		playerSloped = playerObj.GetComponent<PlayerHandler> ().IsSloped();			// Player on a slope?

		if (playerState == PlayerHandler.PlayerState.OnRope)
			playerRoped = true;
		else
			playerRoped = false;

		// Inputs
		rightX = Input.GetAxisRaw ("CamHorizontal") * Time.deltaTime;
		rightY = Input.GetAxisRaw ("CamVertical") * Time.deltaTime;

		// The only control over follow - setting rotation
		follow.rotation = Quaternion.Euler(0, rightStickPrevFrame.x, 0);

		characterOffset = playerObj.transform.position + new Vector3 (0, 2, 0);

	}

	void LateUpdate () {

		if (Cursor.lockState != CursorLockMode.Locked) return;  // only move camera when cursor is focused

		// Allow camera to follow accurately over gaps
		RaycastHit canyonHIT;
		if(Physics.Raycast(playerObj.transform.position - Vector3.up * 1.5f, -Vector3.up, out canyonHIT, 40)){

			if(Vector3.Distance(canyonHIT.point, playerObj.transform.position) > 6)
				StartCoroutine(AdjustCameraToPlayer(2.0f));

		}

		// Player is ball? or not?
		playerLastState = playerState;
		playerState = playerObj.GetComponent<PlayerHandler> ().CurrentState;	

		// If state changed
		if (playerState != playerLastState) {
			StartCoroutine (AdjustCameraToPlayer (2.0f));
		}

		ConstantInit ();

		// State check
		if (playerState == PlayerHandler.PlayerState.Ball) {

			// Player's physical position
			characterOffset += new Vector3 (0, 0.6f, 0);
		}

		RaycastHeightChecking();


		float groundAngle = RaycastSlopeChecking ();

		if (groundAngle > 45) {
			StartCoroutine (AdjustCameraToPlayer (1.0f));
		}


		if (!adjustCamera) {
			
			// Jump check 
			if ((playerGrounded && playerState == PlayerHandler.PlayerState.Human)) {

				storedHitPoint = Vector3.Lerp (storedHitPoint, characterOffset, cameraViewSpeed * Time.deltaTime);

			} 

			if (playerState == PlayerHandler.PlayerState.Ball) {

				storedHitPoint = Vector3.Lerp (storedHitPoint, characterOffset, cameraViewSpeed * Time.deltaTime);

			}

			if(characterOffset.y < storedHitPoint.y - 0.3f && !playerHighFalling){
				playerHighFalling = true;
			}
			if(playerGrounded){
				playerHighFalling = false;
			}

			// Moves camera to exactly follow player while falling or on slope
			if ((characterOffset.y < storedHitPoint.y && playerHighFalling) || playerSloped ||
				playerRoped || DirectFollow) {

				storedHitPoint = characterOffset;

			} 
		}


		savedPosition = storedHitPoint;			// camera position
		playerGroundPosition = storedHitPoint;	// camera view


		// FREE CAMERA MOVEMENT
		FreeCameraMovement();


		// Uses only one camera type at the moment
		if (Input.GetButtonDown("CameraBehind") && !cameraStateBehind)
		{
			camState = CameraStates.BEHIND;
			cameraStateBehind = true;
		}
		else
		{
			camState = CameraStates.TARGET;
			cameraStateBehind = false;
		}


		switch (camState) {

		case CameraStates.TARGET:

			// Camera physical position
			Vector3 newOffset = new Vector3 (characterOffset.x, savedPosition.y - 1.0f, characterOffset.z);

			// Where the camera is facing
			curLookDir = Vector3.Normalize (newOffset - this.transform.position - new Vector3 (0, 1, 0));
			curLookDir.y = 0;

			targetPosition = (newOffset + follow.up * distanceUp) - (Vector3.Normalize (curLookDir) * distanceAway) - (transform.forward * distanceFrom);

			break;

		case CameraStates.BEHIND:

			StartCoroutine (SetBehindPlayer (0.1f));

			break;

		}

		// SET CAMERA POSITION
		if(!behindLock)
			transform.position = targetPosition;


		// Keeps camera stationary
		lookAtPosition = new Vector3( playerObj.transform.position.x, playerGroundPosition.y, playerObj.transform.position.z);

		transform.position = WallStuff (lookAtPosition, transform.position);

		transform.LookAt (lookAtPosition);

	}
		
	bool behindLock = false;
	IEnumerator SetBehindPlayer(float camTime){

		float counter = camTime;

		behindLock = true;

		while (counter > 0) {
			// Where the camera is facing
			curLookDir = rotateMesh.transform.forward;
			curLookDir.y = 0;

			targetPosition = (characterOffset + follow.up * distanceUp) - (Vector3.Normalize (curLookDir) * distanceAway) - (transform.forward * distanceFrom) - Vector3.up * 1;

			transform.position = Vector3.Lerp (transform.position, targetPosition, 30 * Time.deltaTime);

			counter -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		curLookDir = rotateMesh.transform.forward - Vector3.up;
		curLookDir.y = 0;

		targetPosition = (characterOffset + follow.up * distanceUp) - (Vector3.Normalize (curLookDir) * distanceAway) - (transform.forward * distanceFrom);

		transform.position = targetPosition;

		behindLock = false;

	}


	// Allows for the camera to move horizontally and vertically freely
	void FreeCameraMovement(){

		// DISTANCE UP
		rightStickPrevFrame.y = (ySpeed * rightY * (invertY ? 1 : -1)) * 60 * Time.deltaTime;

		if (distanceUp >= -3 && distanceUp <= 6)
			distanceUp += rightStickPrevFrame.y * 20 * Time.deltaTime;
		if(distanceUp < -3) distanceUp = -3;
		if(distanceUp > 6) distanceUp = 6;


		// CAMERA ROTATION ADJUST
		rightStickPrevFrame.x += (xSpeed * rightX * (invertX ? 1 : -1)) * 20 * Time.deltaTime;


		// DISTANCE AWAY
		if (distanceAway >= 3.5f && distanceAway <= 6)
			distanceAway -= (rightStickPrevFrame.y / 2) * 20 * Time.deltaTime;
		if(distanceAway < 3.5f) distanceAway = 3.5f;
		if(distanceAway > 6) distanceAway = 6;

	}


	IEnumerator AdjustCameraToPlayer(float time){

		adjustCamera = true;

		float counter = time;

		while (counter > 0 && !playerGrounded && playerState != PlayerHandler.PlayerState.Ball) {

			storedHitPoint = Vector3.Lerp (storedHitPoint, characterOffset, cameraViewSpeed * Time.deltaTime);

			counter -= Time.deltaTime;

			yield return null;

		}

		adjustCamera = false;
	}


	Vector3 WallStuff(Vector3 charOffset, Vector3 targetPos){

		RaycastHit hit;

		if (distanceUp >= 0)
		{
			// If the camera is level or above the ground
			if (Physics.Linecast(charOffset, targetPos, out hit))
			{
				if (hit.transform.GetComponent<Camera_IgnoreObject> () || hit.transform.GetComponent<Collider>().isTrigger)
					return transform.position;

				if(hit.point.y < targetPos.y)
					storedWallHitPoint = new Vector3(hit.point.x, hit.point.y, hit.point.z);

				return new Vector3(hit.point.x, storedWallHitPoint.y, hit.point.z);
			}
		}
		else
		{ 
			// If the camera was moved below waist level
			if (Physics.Linecast(charOffset, targetPos - new Vector3(0, 0.12f, 0), out hit))
			{   
				// shift camera's position check down a tad
				if (hit.transform.GetComponent<Camera_IgnoreObject>() || hit.transform.GetComponent<Collider>().isTrigger)
					return transform.position;

				// position camera based off of every value of the ray instead of just x and z
				return new Vector3(hit.point.x, hit.point.y, hit.point.z) + ((follow.position - targetPos) * 0.03f);
			}
		}

		return transform.position;
	}


	// GET RAYCASTS FOR SLOPE CHANGES
	float RaycastSlopeChecking () {

		if (playerState != PlayerHandler.PlayerState.Ball) {
			
			float angle1, angle2;
			Ray ray0 = new Ray (playerObj.transform.position, Vector3.down * 444);
			Ray ray1 = new Ray (playerObj.transform.position + (playerObj.transform.GetChild (0).transform.forward * 0.4f) + Vector3.up * 5, Vector3.down * 444);
			Ray ray2 = new Ray (playerObj.transform.position - (playerObj.transform.GetChild (0).transform.forward * 0.4f) + Vector3.up * 5, Vector3.down * 444);
			Ray ray3 = new Ray (playerObj.transform.position + (playerObj.transform.GetChild (0).transform.right * 0.4f) + Vector3.up * 5, Vector3.down * 444);
			Ray ray4 = new Ray (playerObj.transform.position - (playerObj.transform.GetChild (0).transform.right * 0.4f) + Vector3.up * 5, Vector3.down * 444);

			RaycastHit hit0 = new RaycastHit ();
			RaycastHit hit1 = new RaycastHit ();
			RaycastHit hit2 = new RaycastHit ();

			Vector3 pos0, pos1, pos2;

			// ---------------------------------------------

			if (Physics.Raycast (ray0, out hit0)) {
				if(!hit0.collider.isTrigger)
					pos0 = hit0.point;
			}

			if (Physics.Raycast (ray1, out hit1)) {
				if(!hit1.collider.isTrigger)
					pos1 = hit1.point;
			}
			if (Physics.Raycast (ray2, out hit2)) {
				if(!hit2.collider.isTrigger)
					pos2 = hit2.point;
			}

			if (DEBUG) {
				Debug.DrawLine (hit1.point, hit1.point + Vector3.up, Color.red);
				Debug.DrawLine (hit2.point, hit2.point + Vector3.up, Color.red);
				Debug.DrawLine (hit1.point, hit2.point, Color.blue);
			}

			angle1 = Mathf.Rad2Deg * Mathf.Acos (
				(pos1.x * pos2.x + pos1.y * pos2.y + pos1.z + pos2.z) /
				((Mathf.Sqrt (pos1.x * pos1.x + pos1.y * pos1.y + pos1.z * pos1.z)) *
				(Mathf.Sqrt (pos2.x * pos2.x + pos2.y * pos2.y + pos2.z * pos2.z)))
			);

			angle1 = Vector3.Angle (pos2 - pos1, -playerObj.transform.GetChild (0).transform.forward);

			// ---------------------------------------------

			if (Physics.Raycast (ray3, out hit1)) {
				if(!hit1.collider.isTrigger)
				pos1 = hit1.point;
			}
			if (Physics.Raycast (ray4, out hit2)) {
				if(!hit2.collider.isTrigger)
				pos2 = hit2.point;
			}

			if (DEBUG) {
				Debug.DrawLine (hit1.point, hit1.point + Vector3.up, Color.red);
				Debug.DrawLine (hit2.point, hit2.point + Vector3.up, Color.red);
				Debug.DrawLine (hit1.point, hit2.point, Color.blue);
			}

			angle2 = Mathf.Rad2Deg * Mathf.Acos (
				(pos1.x * pos2.x + pos1.y * pos2.y + pos1.z + pos2.z) /
				((Mathf.Sqrt (pos1.x * pos1.x + pos1.y * pos1.y + pos1.z * pos1.z)) *
				(Mathf.Sqrt (pos2.x * pos2.x + pos2.y * pos2.y + pos2.z * pos2.z)))
			);

			angle2 = Vector3.Angle (pos2 - pos1, -playerObj.transform.GetChild (0).transform.right);

			// ---------------------------------------------

			if(DEBUG)
				GameObject.Find ("Angles").GetComponent<Text> ().text = "Angle1: " + angle1 + ",  2: " + angle2;

			if (angle2 > angle1)
				return angle2;
			else
				return angle1;
		} else {
			return 0;
		}

	}


	// GET RAYCASTS FOR HEIGHT CHANGES
	void RaycastHeightChecking(){

		// RAYCAST HEIGHT CHECKING
		float changeInRayHeights = 0;
		float currentDistance = 0;

		// Changes in height
		RaycastHit hit = new RaycastHit();

		if (Physics.BoxCast(playerObj.transform.position, new Vector3(1,1,1), Vector3.down * 8, out hit)) {

			if (hit.transform.tag != "Player" && !hit.collider.isTrigger) {

				Color rayColor = Color.blue;

				// Rotates the camera up to view the player if a new height is detected below
				playerGroundPosition = characterOffset;
				savedPosition = characterOffset;

				// compare a stored old ray distance to the current hit distance
				currentDistance = Mathf.Abs(playerObj.transform.position.y - hit.point.y);
				changeInRayHeights = Mathf.Abs(oldRayHeight - currentDistance);
				rayChangeAmt = changeInRayHeights - oldRayChange;

				if(Mathf.Abs(rayChangeAmt) > 0.4f && !adjustCamera && playerState != PlayerHandler.PlayerState.Ball){

					// Store the value
					StartCoroutine(AdjustCameraToPlayer (0.2f));

				}

				if (DEBUG) {
					Debug.DrawRay (playerObj.transform.position - Vector3.right * 1.5f, Vector3.down * 5, rayColor);
				}

				if(playerGrounded)
					oldRayHeight = currentDistance;

				oldRayChange = changeInRayHeights;
			}

		}

	}*/
}
