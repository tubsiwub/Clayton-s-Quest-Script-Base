using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFloorGetter : MonoBehaviour
{
	PlayerHandler playerHandler;
	HumanController humanController;
	BallController ballController;

	Vector3 playerFloor;
	public Vector3 PlayerFloor { get { return playerFloor; } }
	Vector3 target;

	Vector3 playerPos; // use this instead of transform.position to account for lower ball offset
	float lastFloorHeight;
	List<Vector3> offsets;
	float downVelLastFrame = 0;

	enum FollowState { Floor, Player };
	FollowState followState = FollowState.Player;

	bool GroundedOrLedged { get { return playerHandler.IsGrounded() || 
		playerHandler.CurrentSecondaryAction == PlayerHandler.SecondaryAction.OnLedge; } }

	const float lerpSpeed = 12;
	const int playerIgnoreLayer = -257;
	const float followOffset = 1;
	const float newHeightMin = 0.15f;   // must be higher than this value to be considered a higher platform
	const float ballOffset = 0.5f;

	int fallingFrames = 0;
	const int maxFallingFrames = 30;

	void Awake()		// need our startup to happen before Camera's
	{
		playerHandler = GetComponent<PlayerHandler>();
		humanController = GetComponent<HumanController>();
		ballController = GetComponent<BallController>();
		lastFloorHeight = playerHandler.transform.position.y;

		offsets = new List<Vector3>();
		offsets.Add(Vector3.forward);
		offsets.Add(Quaternion.Euler(0, 45, 0) * Vector3.forward);
		offsets.Add(Vector3.left);
		offsets.Add(Quaternion.Euler(0, 45, 0) * Vector3.left);
		offsets.Add(Vector3.back);
		offsets.Add(Quaternion.Euler(0, 45, 0) * Vector3.back);
		offsets.Add(Vector3.right);
		offsets.Add(Quaternion.Euler(0, 45, 0) * Vector3.right);
	}
	
	Vector3 FindTarget()
	{
		playerPos = transform.position;

		if (playerHandler.CurrentState == PlayerHandler.PlayerState.Ball)
			playerPos += (Vector3.up * ballOffset);

		return playerPos + (Vector3.down * followOffset);
	}

	/*void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		style.fontSize = 32;
		GUI.Label(new Rect(10, Screen.height - 40, 300, 300), followState + "", style);
	}*/

	void LateUpdate()
	{
		target = FindTarget();
		bool lockToPlayer = playerHandler.JustHighJumped || playerHandler.CurrentState == PlayerHandler.PlayerState.OnRope ||
			(humanController.GetMovingPlatform != null) || (ballController.GetMovingPlatform != null);

		if (FallingForALongWhile() || lockToPlayer)
			followState = FollowState.Player;
		else FindNearbyFloors();

		if (followState == FollowState.Floor)
			target.y = lastFloorHeight;

		if (!lockToPlayer)
		{
			CheckForHigherFloorHeight();
			CheckForLowerFloorHeight();
		}

		playerFloor.x = target.x; playerFloor.z = target.z;
		SetFloorHeight();
		
		if (playerHandler.GetVelocity().y < -1)	// only update down velocity when we're falling down
			downVelLastFrame = playerHandler.GetVelocity().y;
	}

	void SetFloorHeight()
	{
		Vector3 last = playerFloor;
		playerFloor = Vector3.Lerp(playerFloor, target, lerpSpeed * Time.deltaTime);
		playerFloor.x = last.x; playerFloor.z = last.z;
	}

	public void SetToPlayer()
	{
		playerFloor = FindTarget();
		lastFloorHeight = playerPos.y - followOffset;
	}

	void FindNearbyFloors()
	{
		if (Time.frameCount <= 10) return;	// make sure we have time to set some values before doing all this

		bool foundHigherPlatform = false;
		for (int i = 0; i < offsets.Count; i++)
		{
			float floorHeight = 0;
			if (FoundFloor(i, ref floorHeight) &&
				floorHeight > lastFloorHeight + newHeightMin)
			{
				foundHigherPlatform = true;
			}
		}

		if (foundHigherPlatform && followState != FollowState.Player)
			followState = FollowState.Player;

		if (!foundHigherPlatform && GroundedOrLedged && followState != FollowState.Floor)
		{
			if (downVelLastFrame > -20)
				followState = FollowState.Floor;
			else
				StartCoroutine(WaitThenSetToFloor());

			CheckForLowerFloorHeight();
		}
	}

	IEnumerator WaitThenSetToFloor()
	{
		// update down vel so we don't call this a bunch when we land
		downVelLastFrame = playerHandler.GetVelocity().y;

		yield return new WaitForSeconds(0.25f);

		followState = FollowState.Floor;
		CheckForLowerFloorHeight();
	}

	void CheckForLowerFloorHeight()
	{
		//check for lower floor height
		// readjust floor height manually if we're grounded, and this grounded position is lower than before
		if (GroundedOrLedged && playerPos.y - followOffset < lastFloorHeight - newHeightMin)
		{
			lastFloorHeight = playerPos.y - followOffset;
		}
	}

	void CheckForHigherFloorHeight()
	{
		// check for higher floor height
		RaycastHit hitInfo; Ray ray = new Ray(playerPos, Vector3.down);
		if (Physics.Raycast(ray, out hitInfo, 100, playerIgnoreLayer, QueryTriggerInteraction.Ignore))
		{
			if (hitInfo.point.y > lastFloorHeight)
			{
				if (followState == FollowState.Floor)
					target = hitInfo.point;    // only set floor directly when we're NOT following (avoids jank)

				lastFloorHeight = hitInfo.point.y;
			}
		}
	}

	bool FoundFloor(int offset, ref float floorHeight)
	{
		RaycastHit hitInfo;		// to-do: only set to follow when player is facing same direction as ray
		Ray ray = new Ray(playerPos + (offsets[offset] * 2.25f) + (Vector3.up * 3), Vector3.down);

		// Debug.DrawRay(ray.origin, ray.direction * 100, followState == FollowState.Floor ? Color.white : Color.green);
		if (Physics.Raycast(ray, out hitInfo, 100, playerIgnoreLayer, QueryTriggerInteraction.Ignore))
		{
			floorHeight = hitInfo.point.y;
			return true;
		}

		return false;
	}

	bool FallingForALongWhile()
	{
		if (!GroundedOrLedged && playerHandler.IsFalling())
			fallingFrames++;
		// divide instead of set to 0, to wait a few frames between transformation
		else fallingFrames = (int)(fallingFrames * 0.5f);

		return (fallingFrames > maxFallingFrames);
	}
}
