using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeCheckController : MonoBehaviour
{
	[SerializeField] LayerMask castLayers;

	PlayerHandler playerHandler;
	HumanController humanController;
	Transform rotateMesh;
	Transform cam;
	List<Vector3> directions;

	RaycastHit ledgeHitInfo;
	Ray hitRay;

	bool onLedge = false;
	public bool OnLedge { get { return onLedge; } }
	const float neededHeightOffGround = 2.5f;
	const float heightOffset = 1.6f;
	const float dirDiffAllowance = -0.5f;	// higher is more lenient

	int leaveStamp = 0;
	const int maxLeaveStamp = 15;

	int heldBackFrames = 0;
	const int maxHeldBackFrames = 5;

	float SliderOffset { get { return playerHandler.CurrentState == PlayerHandler.PlayerState.Slider ? 0.4f : 0; } }
	Vector3 SliderOffsetVec { get { return new Vector3(0, SliderOffset, 0); } }

	void Start()
	{
		playerHandler = GetComponent<PlayerHandler>();
		humanController = GetComponent<HumanController>();
		rotateMesh = playerHandler.RotateMesh;
		cam = Camera.main.transform;

		directions = new List<Vector3>();
		directions.Add(Vector3.forward);
		directions.Add(Quaternion.Euler(0, 45, 0) * Vector3.forward);
		directions.Add(Vector3.left);
		directions.Add(Quaternion.Euler(0, 45, 0) * Vector3.left);
		directions.Add(Vector3.back);
		directions.Add(Quaternion.Euler(0, 45, 0) * Vector3.back);
		directions.Add(Vector3.right);
		directions.Add(Quaternion.Euler(0, 45, 0) * Vector3.right);
	}

	void Update()
	{
		if (!PlayerHandler.CanUpdate) return;

		if (!onLedge)
			UpdateLookForLedge();
		else
			UpdateHandleLeaveLedge();
	}

	void UpdateLookForLedge()
	{
		if (CanLookForLedge())
		{
			if (LookForLedge())
				SetOnLedge(ledgeHitInfo);
		}
	}

	/*void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		style.fontSize = 32;
		GUI.Label(new Rect(10, Screen.height - 40, 300, 300), leaveStamp + "", style);
	}*/

	bool CanLookForLedge()
	{
		if ((Time.frameCount - leaveStamp <= maxLeaveStamp) || playerHandler.IsFrozen || !playerHandler.IsFalling())
			return false;

		float distDown;
		Vector3 downRayDir;

		if (playerHandler.CurrentState == PlayerHandler.PlayerState.Slider)
			downRayDir = playerHandler.GetVelocity().normalized;
		else downRayDir = Vector3.down;

		RaycastHit hitInfo; Ray downRay = new Ray(transform.position + SliderOffsetVec, downRayDir);
		if (Physics.Raycast(downRay, out hitInfo, 4, castLayers, QueryTriggerInteraction.Ignore))
		{
			distDown = downRay.origin.y - hitInfo.point.y;
		}
		else distDown = float.MaxValue;

		return (distDown > neededHeightOffGround);
	}
	
	bool LookForLedge()
	{
		float[] heights = new float[] { 0.6f, 0.4f, 0.2f, 0.0f, -0.2f };

		FeelTouchType lastHitResult = FeelTouchType.Ignore;
		bool hadAnyHits = false;
		for (int i = 0; i < heights.Length; i++)
		{
			RaycastHit tempHitInfo = new RaycastHit();
			Ray tempRay = new Ray();
			FeelTouchType newHitResult = CreateFeeler(heights[i], ref tempHitInfo, ref tempRay);

			if (newHitResult == FeelTouchType.Hit)
			{
				if (hadAnyHits == true)     // if this isn't our FIRST hit, and we haven't left yet
					return false;

				if (lastHitResult == FeelTouchType.NoHit) // if the last hit was false, and the current one is true
				{
					ledgeHitInfo = tempHitInfo;
					hitRay = tempRay;

					return true;
				}

				hadAnyHits = true;
			}

			lastHitResult = newHitResult;
		}

		return false;
	}

	enum FeelTouchType { Ignore, Hit, NoHit }
	FeelTouchType CreateFeeler(float height, ref RaycastHit tempHitInfo, ref Ray tempRay)
	{
		RaycastHit hitInfo;
		float length = 2 * ((height * 0.25f) + 0.8f);	// extend or shorten ray length based on how high
		Vector3 dir = PlayerController.GetMovement(cam, new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")));
		Ray ray = new Ray(transform.position + (Vector3.up * height) + SliderOffsetVec, dir.normalized);

		bool hit = Physics.Raycast(ray, out hitInfo, dir.magnitude * length, 
			castLayers, QueryTriggerInteraction.Ignore);

		Debug.DrawRay(ray.origin, ray.direction * (dir.magnitude * length), hit ? Color.red : Color.blue);

		if (hit)
		{
			if (hitInfo.collider.gameObject.GetComponent<LedgeGrabIgnore>() ||
				hitInfo.collider.gameObject.GetComponent<ForceSlide>())
				return FeelTouchType.Ignore;

			if (!NearTopOfPlatform(dir, hitInfo.collider) || !NearTopOfPlatform(Quaternion.Euler(0, 35, 0) * dir, hitInfo.collider) || 
			!NearTopOfPlatform(Quaternion.Euler(0, -35, 0) * dir, hitInfo.collider))
				return FeelTouchType.Ignore;

			tempHitInfo = hitInfo;
			tempRay = ray;
		}

		return hit ? FeelTouchType.Hit : FeelTouchType.NoHit;
	}

	bool NearTopOfPlatform(Vector3 dir, Collider col)
	{
		const float longRayDist = 3.5f;
		Ray tipTopRay = new Ray(transform.position + (Vector3.up * 1.5f) + SliderOffsetVec, dir);
		RaycastHit hitInfo;

		Debug.DrawRay(tipTopRay.origin, tipTopRay.direction * longRayDist, Color.green);
		bool hit = Physics.Raycast(tipTopRay, out hitInfo, longRayDist, castLayers, QueryTriggerInteraction.Ignore);
		if (!hit) return true;

		if (hitInfo.collider == col)
			return false;
		else return true;
	}

	float CalcAngle(Vector3 normal)
	{
		float angle = Vector3.Dot(normal, Vector3.down);
		angle = 180 - (Mathf.Acos(angle) * Mathf.Rad2Deg);

		if (float.IsNaN(angle)) angle = 0;

		return angle;
	}

	void SetOnLedge(RaycastHit hitInfo)
	{
		int r = Random.Range(1, 5);
		SoundManager.instance.PlayClip("LedgeGrab0" + r);

		playerHandler.SwitchState(PlayerHandler.PlayerState.Human);
		playerHandler.SetFrozen(true, false);
		playerHandler.SetSecondaryAction(PlayerHandler.SecondaryAction.OnLedge);    // do this to reset rotate mesh location

		hitInfo = ScanDown(hitInfo);

		Vector3 normal = hitInfo.normal; normal.y = 0;
		rotateMesh.forward = -normal.normalized;

		Vector3 handsPos = DownRayFromHands(hitInfo.point), newPos;
		newPos = handsPos - (rotateMesh.forward * 0.5f);   //move back
		newPos += Vector3.down * heightOffset;      // move down

		StartCoroutine(LerpToNewPos(newPos, handsPos));

		playerHandler.HumanAnimator.CrossFade("ledge_idle", 0.12f);
	}

	RaycastHit ScanDown(RaycastHit initialHit)
	{
		float[] heights = new float[] { 0.7f, 0.65f, 0.6f, 0.55f, 0.5f, 0.45f, 0.4f, 0.35f, 0.3f, 0.25f, 0.2f, 0.15f, 0.1f,
			0.05f, 0.0f, -0.05f, -0.1f, -0.15f, -0.2f, -0.25f, -0.3f, -0.35f, -0.4f, -0.45f };

		RaycastHit hitInfo;
		RaycastHit returnHitInfo = new RaycastHit();
		bool hitLast = false;
		bool hitAnythingAtAll = false;
		for (int i = 0; i < heights.Length; i++)
		{
			Ray ray = new Ray(transform.position + (Vector3.up * heights[i]) - (hitRay.direction.normalized * 0.3f)
				+ SliderOffsetVec, hitRay.direction);

			bool hit = Physics.Raycast(ray, out hitInfo, 3.5f,
				castLayers, QueryTriggerInteraction.Ignore);

			if (hit)
			{
				if (hitInfo.collider != initialHit.collider)
				{
					hitLast = false;
					continue;
				}

				float hitAngle = CalcAngle(hitInfo.normal);
				if (hitAngle >= 88)
				{
					hitLast = false;
					continue;
				}

				hitAnythingAtAll = true;
				returnHitInfo = hitInfo;
			}

			if (hitLast && !hit)
				return hitInfo;

			hitLast = hit;
		}

		if (hitAnythingAtAll) return returnHitInfo;
		else return initialHit;
	}

	IEnumerator LerpToNewPos(Vector3 newPos, Vector3 handsPos)
	{
		Vector3 lastPos = transform.position;
		transform.position = newPos;
		rotateMesh.RotateAround(handsPos, rotateMesh.right, BodyAlignAngle());
		transform.position = lastPos;

		while (Vector3.Distance(transform.position, newPos) > 0.1f)
		{
			transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 15);
			yield return null;
		}
		onLedge = true;		// don't set this until coroutine is over - this way, we can't leave ledge until this is done
	}

	Vector3 DownRayFromHands(Vector3 hitPoint)
	{
		RaycastHit downRayInfo;

		// position this ray above the hitpoint, and slightly forward from the player, to ensure we reach the platform.
		Ray downRay = new Ray(hitPoint + Vector3.up + (rotateMesh.forward * 0.2f), Vector3.down);
		Debug.DrawRay(downRay.origin, downRay.direction, Color.red);
		if (Physics.Raycast(downRay, out downRayInfo, 3, castLayers, QueryTriggerInteraction.Ignore))
		{
			return downRayInfo.point;
		}
		return hitPoint;	// should never happen, but ehhh
	}

	//public Transform head, mid, foot;
	float BodyAlignAngle()
	{
		Vector3 footRayPos = transform.position + Vector3.down * 0.75f; // set initial position, move down
		footRayPos -= rotateMesh.forward * 2;   // move back
		Ray footRay = new Ray(footRayPos, rotateMesh.forward);
		Ray midRay = new Ray(footRayPos + Vector3.up, rotateMesh.forward);
		Ray headRay = new Ray(footRayPos + Vector3.up*2, rotateMesh.forward);
		RaycastHit footHitInfo, headHitInfo, midHitInfo;

		float headToMidAngle = 0, headToFootAngle = 0;

		if (Physics.Raycast(midRay, out midHitInfo, 4, castLayers, QueryTriggerInteraction.Ignore) &&
			Physics.Raycast(headRay, out headHitInfo, 4, castLayers, QueryTriggerInteraction.Ignore))
		{
			if (midHitInfo.collider == headHitInfo.collider)
				headToMidAngle = CalcAngle((headHitInfo.point - midHitInfo.point).normalized);
		}

		if (Physics.Raycast(footRay, out footHitInfo, 4, castLayers, QueryTriggerInteraction.Ignore) &&
			Physics.Raycast(headRay, out headHitInfo, 4, castLayers, QueryTriggerInteraction.Ignore))
		{
			if (footHitInfo.collider == headHitInfo.collider)
				headToFootAngle = CalcAngle((headHitInfo.point - footHitInfo.point).normalized);
		}

		return Mathf.Max(headToMidAngle, headToFootAngle);
	}

	void UpdateHandleLeaveLedge()
	{
		Vector3 stickDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		float direction = Vector3.Dot(rotateMesh.forward, PlayerController.GetMovement(cam, stickDir));

		if (direction <= -0.5f) heldBackFrames++;
		else heldBackFrames = 0;

		if (Input.GetButton(PlayerHandler.JumpString) || heldBackFrames > maxHeldBackFrames)
			LeaveLedge();
	}

	void LeaveLedge()
	{
		if (!Input.GetButton(PlayerHandler.JumpString))
		{
			int r = Random.Range(1, 3);
			SoundManager.instance.PlayClip("LedgeJump0" + r);
		}

		heldBackFrames = 0;
		onLedge = false;
		playerHandler.SetFrozen(false, false);
		playerHandler.KillVelocity();
		playerHandler.SetSecondaryAction(PlayerHandler.SecondaryAction.None);

		rotateMesh.localPosition = Vector3.zero;
		rotateMesh.rotation = Quaternion.Euler(0, rotateMesh.eulerAngles.y, 0);
		transform.position += Vector3.down * -1.25f;

		if (Input.GetButton(PlayerHandler.JumpString))
		{
			humanController.ForceJump(false);
			playerHandler.HumanAnimator.CrossFade("hero_jump", 0.1f);
		}
		else playerHandler.HumanAnimator.CrossFade("hero_fall_transition", 0.1f);
		leaveStamp = Time.frameCount;

		StartCoroutine(CancelReJump());
	}

	IEnumerator CancelReJump()
	{
		yield return null;
		humanController.CancelEnableReJump();
	}
}
