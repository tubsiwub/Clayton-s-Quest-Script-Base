using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCollider : MonoBehaviour
{
	[SerializeField] LayerMask castLayers;
	[SerializeField] GameObject rayPointsRoot;
	RayPoint[] rayPoints;

	HumanController humanController;
	float controllerHeight;

	Platform_TrackDistance currentMovePlat;
	public Platform_TrackDistance GetMovingPlatform { get { return currentMovePlat; } }
	public void SetMovingPlatform(Platform_TrackDistance plat) { currentMovePlat = plat; }

	public bool OnMovingPlatform { get { return currentMovePlat != null; } }
	public Vector3 MovePlatDist { get { return currentMovePlat == null ? Vector3.zero : currentMovePlat.Distance; } }
	public void DisableMovingPlatform() { currentMovePlat = null; }

	TrackTwirlBase twirlPlat = null;
	public TrackTwirlBase GetTwirlingPlatform { get { return twirlPlat; } }
	public bool OnTwirlingPlatform { get { return twirlPlat != null; } }
	public void DisableTwirlingPlatform() { twirlPlat = null; }

	enum HitType { NoHit, FloorHit, SteepSlopeHit };

	const float rayLength = 12.5f;
	const float rayInset = 12.5f;

	const float downRayVelMod = 1.25f;
	const float backRayMod = 18.75f;

	float maxClimbAngle;
	float minSlopeAngle;

	class PointNdx
	{
		public float pos;
		public int ndx;

		public PointNdx(float pos, int ndx) { this.pos = pos; this.ndx = ndx; }
	}

	void Start()
	{
		maxClimbAngle = GetComponent<PlayerController>().GetMaxClimbAngle;
		minSlopeAngle = GetComponent<PlayerController>().GetMinSlopeAngle;
		controllerHeight = GetComponent<CharacterController>().height;
		humanController = GetComponent<HumanController>();
		DisableMovingPlatform();

		rayPoints = rayPointsRoot.GetComponentsInChildren<RayPoint>();
	}

	public bool HitCeilingThisFrame()
	{
		const int NeededForHit = 6;
		const float HeightForCeilHit = 0.27f;
		int count = 0;

		List<Ray> freeRays = new List<Ray>();
		float[] heightToHitList = new float[rayPoints.Length];

		for (int i = 0; i < rayPoints.Length; i++)
		{
			Ray ray = new Ray();
			if (CastUp(i, out ray, out heightToHitList[i]))
				count++;
			else
				freeRays.Add(ray);
		}

		if (count == 0)
			cancelFrames++;

		if (count <= NeededForHit && count != 0)
		{
			cancelFrames = 0;
			NudgeOver(freeRays);
			return false;
		}

		if (count != 0)
		{
			float avg = 0;
			for (int i = 0; i < heightToHitList.Length; i++)
				avg += heightToHitList[i];

			avg = avg / heightToHitList.Length;

			return avg <= HeightForCeilHit;
		}

		return false;
	}

	Vector3 noInputDir = Vector3.zero;
	public Vector3 NoInputDir { get { return noInputDir; } }
	public void CancelNoInput() { noInputDir = Vector3.zero; }

	int cancelFrames = 0;
	const int maxCancelFrames = 20;

	void LateUpdate()
	{
		if (cancelFrames > maxCancelFrames)
		{
			CancelNoInput();
			cancelFrames = 0;
		}
	}

	void NudgeOver(List<Ray> freeRays)
	{
		Vector3 averagePoint = Vector3.zero;
		for (int i = 0; i < freeRays.Count; i++)
		{
			averagePoint += freeRays[i].origin;
		}
		averagePoint /= freeRays.Count;
		averagePoint.y = transform.position.y;

		Vector3 direction = averagePoint - transform.position;
		Vector3 otherDirection = transform.position - averagePoint;
		direction.Normalize();
		noInputDir = otherDirection.normalized;

		for (int i = 0; i < 3; i++)
		{
			humanController.CharController.Move(direction * 0.05f);

			bool hittingStuff = false;
			for (int j = 0; j < rayPoints.Length; j++)
			{
				Ray ray = new Ray();
				float height;
				if (CastUp(j, out ray, out height))
					hittingStuff = true;
			}

			if (!hittingStuff)
				break;
		}
	}

	bool CastUp(int i, out Ray ray, out float heightHit)
	{
		const float CeilingHeightCheck = 0.65f;
		const float HeightDownSlightly = 0.5f;
		const float HeadInset = 0.5f;

		Vector3 dirToCenter = (rayPoints[i].GetPosition - transform.position).normalized;
		Vector3 dirToCenterFlipped = dirToCenter; dirToCenterFlipped.y = -dirToCenter.y;
		ray = new Ray(rayPoints[i].GetPosition + (Vector3.up * (controllerHeight - HeightDownSlightly)) -
			(dirToCenter * HeadInset), dirToCenterFlipped);
		heightHit = float.MaxValue;

		RaycastHit hitInfo;

		if (Physics.Raycast(ray, out hitInfo, CeilingHeightCheck, castLayers, QueryTriggerInteraction.Ignore))
		{
			if (!hitInfo.collider.GetComponent<NoNudge>())
			{
				//Debug.DrawLine(ray.origin, ray.origin + (ray.direction * CeilingHeightCheck), Color.red);
				heightHit = hitInfo.distance;
				return true;
			}
		}

		//Debug.DrawLine(ray.origin, ray.origin + (ray.direction * CeilingHeightCheck), Color.white);
		return false;
	}

	public void CustomUpdate(float playerDownVel)
	{
		List<PointNdx> hitRays = new List<PointNdx>();
		bool allowBackRays = false;
		bool hitMovingPlatform = false;
		bool hitTwirlingPlatform = false;

		ShootAllRays(ref playerDownVel, ref allowBackRays, ref hitMovingPlatform, ref hitTwirlingPlatform, ref hitRays);

		if (!hitMovingPlatform)
			DisableMovingPlatform();

		if (!hitTwirlingPlatform)
			twirlPlat = null;

		ClearBackRays();

		if (hitRays.Count > 0)
			SetPosToHighestPoint(hitRays, allowBackRays);
	}

	void ShootAllRays(ref float playerDownVel, ref bool allowBackRays, ref bool hitMovingPlatform, 
		ref bool hitTwirlingPlatform, ref List<PointNdx> hitRays)
	{
		for (int i = 0; i < rayPoints.Length; i++)
		{
			float length = GetRayLength(i, playerDownVel);
			Vector3 hitPos;
			bool floorAngled = false;

			HitType hitType = ShootRay(rayPoints[i].GetPosition, length, out hitPos, out floorAngled, 
				ref hitMovingPlatform, ref hitTwirlingPlatform);

			if (hitType == HitType.FloorHit)
			{
				if (floorAngled) allowBackRays = true;

				float pos = hitPos.y - (rayPoints[i].GetDirection.y * (controllerHeight / 2));
				hitRays.Add(new PointNdx(pos, i));
			}
		}
	}

	public bool CheckForSteepSlope()
	{
		int hitSlopes = 0;
		float avgNonSlopeDist = 0;
		const float neededDistAboveFloor = 0.3f;

		for (int i = 0; i < rayPoints.Length; i++)
		{
			float length = float.MaxValue;
			Vector3 hitPos; bool floorAngled = false; bool hitMovingPlatform = false;
			bool hitTwirlingPlatform = false;

			HitType hitType = ShootRay(rayPoints[i].GetPosition, length, out hitPos, out floorAngled, 
				ref hitMovingPlatform, ref hitTwirlingPlatform);

			if (hitType == HitType.SteepSlopeHit)
				hitSlopes++;
			else if (hitType == HitType.FloorHit)
				avgNonSlopeDist += Vector3.Distance(rayPoints[i].GetPosition, hitPos);
			else if (hitType == HitType.NoHit)
				avgNonSlopeDist += float.MaxValue;
		}

		avgNonSlopeDist = avgNonSlopeDist / rayPoints.Length;
		return (hitSlopes > rayPoints.Length / 2) || (avgNonSlopeDist > neededDistAboveFloor);
	}

	float currentFloorAngle = 0;
	public float CurrentFloorAngle { get { return currentFloorAngle; } }
	Collider collided;
	public Collider Collided { get { return collided; } }

	HitType ShootRay(Vector3 origin, float length, out Vector3 hitPos, out bool floorAngled, 
		ref bool hitMovingPlatform, ref bool hitTwirlingPlatform)
	{
		floorAngled = false; hitPos = Vector3.zero;
		origin -= Vector3.down * rayInset*Time.deltaTime;     // when we're on the floor, make sure ray is high enough to still touch the floor

		Ray rayMan = new Ray(origin, Vector3.down);
		RaycastHit hitMan;
		Debug.DrawLine(rayMan.origin, rayMan.origin + (rayMan.direction * length), Color.white);

		if (Physics.Raycast(rayMan, out hitMan, length, castLayers, QueryTriggerInteraction.Ignore))
		{
			float angle = Vector3.Dot(rayMan.direction, hitMan.normal);
			angle = 180 - (Mathf.Acos(angle) * Mathf.Rad2Deg);

			if (float.IsNaN(angle)) angle = 0;

			floorAngled = (angle > minSlopeAngle);
			hitPos = hitMan.point;

			if (angle > maxClimbAngle || hitMan.collider.gameObject.GetComponent<ForceSlide>())
			{
				if (hitMan.collider.gameObject.GetComponent<NoSlide>() == null)
					return HitType.SteepSlopeHit;
				else return HitType.NoHit;
			}

			if (angle != currentFloorAngle)
				currentFloorAngle = angle;

			if (collided != hitMan.collider)
				collided = hitMan.collider;

			if (hitMan.collider.gameObject.tag == "MovingPlatform")
			{
				hitMovingPlatform = true;
				GameObject platObj = hitMan.collider.gameObject;
				if (currentMovePlat == null || currentMovePlat.gameObject != platObj)
					currentMovePlat = platObj.GetComponent<Platform_TrackDistance>();
			}

			if (hitMan.collider.gameObject.tag == "TwirlingPlatform")
			{
				hitTwirlingPlatform = true;
				GameObject platObj = hitMan.collider.gameObject;
				if (twirlPlat == null || twirlPlat.gameObject != platObj)
					twirlPlat = platObj.GetComponent<TrackTwirlBase>();
			}

			return HitType.FloorHit;
		}

		return HitType.NoHit;
	}

	// only set position once per frame
	// mark ready to set pos
	// at the end of frame, set to the highest point
	void SetPosToHighestPoint(List<PointNdx> hitRays, bool allowBackRays)
	{
		float highest = float.MinValue;
		int highestNdx = -1;
		for (int i = 0; i < hitRays.Count; i++)
		{
			if (hitRays[i].pos > highest)
			{
				highest = hitRays[i].pos;
				highestNdx = hitRays[i].ndx;
			}
		}

		SetPos(1, highest);     // hard-coded to down
		if (allowBackRays) rayPoints[highestNdx].SetBackRay(true);
	}

	void SetPos(int axis, float pos)
	{
		if (axis == 1) humanController.OnFloor(true);

		Vector3 newPos = transform.position;
		newPos[axis] = pos;
		transform.position = newPos;
	}

	float GetRayLength(int rayNdx, float playerDownVel)
	{
		float defaultLength = rayLength*Time.deltaTime;

		float backRay = 0;
		if (rayPoints[rayNdx].IsBackRay)
			backRay = backRayMod * Time.deltaTime;

		playerDownVel = -playerDownVel * downRayVelMod * Time.deltaTime;
		float gravityRay = Mathf.Clamp(playerDownVel, defaultLength, float.MaxValue);
		float returnLength = Mathf.Max(gravityRay, backRay);

		if (OnMovingPlatform) returnLength += 0.25f;

		return returnLength;
	}

	void ClearBackRays()
	{
		for (int i = 0; i < rayPoints.Length; i++)
		{
			rayPoints[i].SetBackRay(false);
		}
	}
}
