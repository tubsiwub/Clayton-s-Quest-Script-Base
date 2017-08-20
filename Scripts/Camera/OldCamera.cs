using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Adjust wall collision positions based on player to wall distance

// Adjust camera snap positioning where once you snap to the spot behind the player,
//		you'll stay there once the snap key is released


public class OldCamera : MonoBehaviour
{

	float
	distanceAway = 5,
	distanceUp = 2,
	distanceFrom = 0,
	scrollSpeed = 6;

	float
	rightX,
	rightY;

	const float xSpeed = 190;
	const float ySpeed = 6.25f;

	Transform
	follow,
	rotateMesh;

	public GameObject
	playerObj,
	parentRig;

	public bool
	lerpEnable = true;

	Vector2
	rightStickPrevFrame = Vector2.zero;

	Vector3
	targetPosition,
	curLookDir,
	savedPosition;

	[SerializeField]
	bool invertX;
	[SerializeField]
	bool invertY;

	public enum CameraStates
	{

		BEHIND,
		TARGET,
		FREE
	}

	public CameraStates camState = CameraStates.BEHIND;

	void Start()
	{
		string msg = "Hi, I'm Clayton! This is a long, mandatory message that you have to see. ";
		msg += "This isn't too bad, right?";

		Debug.LogWarning(msg);

		Cursor.lockState = CursorLockMode.Locked;

		follow = GameObject.Find("Follow").transform;
		rotateMesh = GameObject.Find("RotateMesh").transform;

		curLookDir = follow.forward;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
			Cursor.lockState = CursorLockMode.Locked;

		if (Cursor.lockState == CursorLockMode.Locked)
			Cursor.visible = false;
		else Cursor.visible = true;

	}



	void OnGUI()
	{

		#region DISTANCE FROM

		Event scrollEvent = Event.current;

		if (distanceFrom >= -2f && distanceFrom <= 4)
			if (scrollEvent.type == EventType.ScrollWheel)
			{
				distanceFrom += scrollEvent.delta.y / scrollSpeed;
			}
		if (distanceFrom < -2f) distanceFrom = -2f;
		if (distanceFrom > 4) distanceFrom = 4;
		#endregion

	}

	void LateUpdate()
	{
		if (Cursor.lockState != CursorLockMode.Locked) return;  // only move camera when cursor is focused

		Vector3 characterOffset = follow.position + new Vector3(0, 1, 0);

		//if (playerObj.GetComponent<HumanController> ().IsGrounded)
		//	savedPosition = characterOffset;

		//leftX = Input.GetAxis ("Horizontal");
		//leftY = Input.GetAxis ("Vertical");
		rightX = Input.GetAxisRaw("CamHorizontal");
		rightY = Input.GetAxisRaw("CamVertical");

		#region FREE CAMERA MOVEMENT

		#region DISTANCE UP
		rightStickPrevFrame.y = (ySpeed * Time.deltaTime) * rightY * (invertY ? 1 : -1);

		if (distanceUp >= -1 && distanceUp <= 6)
			distanceUp += rightStickPrevFrame.y;
		if (distanceUp < -1) distanceUp = -1;
		if (distanceUp > 6) distanceUp = 6;

		#endregion

		#region CAMERA ROTATION ADJUST

		rightStickPrevFrame.x += (xSpeed * Time.deltaTime) * rightX * (invertX ? 1 : -1);

		follow.rotation = Quaternion.Euler(Vector3.down * follow.parent.eulerAngles.y + new Vector3(0, follow.parent.eulerAngles.y, 0));


		#endregion

		#region DISTANCE AWAY
		if (distanceAway >= 3.5f && distanceAway <= 6)
			distanceAway -= rightStickPrevFrame.y / 2;
		if (distanceAway < 3.5f) distanceAway = 3.5f;
		if (distanceAway > 6) distanceAway = 6;
		#endregion

		#endregion

		if (rightX != 0)
		{

			camState = CameraStates.FREE;
			//savedRigToGoal = Vector3.zero;

		}

		targetPosition = Vector3.zero;

		if (Input.GetButton("CameraBehind"))
		{

			camState = CameraStates.BEHIND;

		}
		else
		{

			camState = CameraStates.TARGET;

		}


		follow.rotation = Quaternion.Euler(0, rightStickPrevFrame.x, 0);


		switch (camState)
		{

			case CameraStates.TARGET:

				curLookDir = Vector3.Normalize(characterOffset - this.transform.position);
				curLookDir.y = 0;

				targetPosition = (characterOffset + follow.up * distanceUp) - (Vector3.Normalize(curLookDir) * distanceAway) - (transform.forward * distanceFrom);

				break;

			case CameraStates.BEHIND:

				curLookDir = rotateMesh.transform.forward;
				curLookDir.y = 0;

				targetPosition = (characterOffset + follow.up * distanceUp) - (Vector3.Normalize(curLookDir) * distanceAway) - (transform.forward * distanceFrom);

				break;

			case CameraStates.FREE:


				break;
		}

		//CompensateForWalls (characterOffset, ref targetPosition);	// Old function


		transform.position = targetPosition;


		bool wallCollide = WallStuff(follow.position, transform.position); // new function

		if (wallCollide) {
			
			transform.LookAt (playerObj.transform.position);

		} else {
			
			transform.LookAt (playerObj.transform.position + Vector3.up * 2);

		}

		Debug.DrawLine(characterOffset, targetPosition, Color.red);

	}

	bool WallStuff(Vector3 charOffset, Vector3 targetPos)
	{

		RaycastHit hit;

		if (distanceUp >= 0)
		{ // If the camera is level or above the ground
			if (Physics.Linecast(charOffset, targetPos, out hit))
			{
				if (hit.transform.tag != "Player")
				{

					Vector3 newPos = new Vector3 (hit.point.x, hit.point.y, hit.point.z);
					transform.position = newPos;

					return true;

				}
			}
		}
//		else
//		{ // If the camera was moved below waist level
//			if (Physics.Linecast(charOffset, targetPos - new Vector3(0, 0.12f, 0), out hit))
//			{   // shift camera's position check down a tad
//				if (hit.transform.tag != "Player")
//				{
//
//					// position camera based off of every value of the ray instead of just x and z
//					Vector3 newPos = new Vector3(hit.point.x, hit.point.y + 0.08f, hit.point.z) + ((follow.position - targetPos) * 0.03f);
//					transform.position = newPos;
//
//					return true;
//
//				}
//			}
//		}

		return false;
	}

	void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
	{

		Debug.DrawLine(fromObject - new Vector3(0, 2, 0), toTarget, Color.red);

		RaycastHit wallHit = new RaycastHit();
		if (Physics.Linecast(fromObject, toTarget, out wallHit))
		{

			if (wallHit.collider.gameObject == playerObj)
				return;

			toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);

			Vector3 directionToPlayer = wallHit.point - follow.position;
			directionToPlayer.y = 0;
			directionToPlayer.Normalize();

			toTarget -= directionToPlayer * 0.4f;
		}
	}
}