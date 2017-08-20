using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollider : MonoBehaviour
{
	BallController ballController;
	
	bool colIsGrounded = false;
	float angleCollided;
	Quaternion floorRot;
	int frameStamp = -1;

	bool OnWall(float angle) { return angle > ballController.GetWallAngle; }

	Platform_TrackDistance currentMovePlat = null;
	public Platform_TrackDistance GetMovingPlatform { get { return currentMovePlat; } }
	public void SetMovingPlatform(Platform_TrackDistance plat) { currentMovePlat = plat; }

	public bool OnMovingPlatform { get { return currentMovePlat != null; } }
	public Vector3 MovePlatDist { get { return currentMovePlat == null ? Vector3.zero : currentMovePlat.Distance; } }
	public void DisableMovingPlatform() { if (!ballController.RecentlyEnabled) StartCoroutine(WaitThenDisablePlat()); }

	TrackTwirlBase twirlPlat = null;
	public TrackTwirlBase GetTwirlingPlatform { get { return twirlPlat; } }
	public bool OnTwirlingPlatform { get { return twirlPlat != null; } }
	public void DisableTwirlingPlatform() { twirlPlat = null; }

	void Awake()
	{
		ballController = GetComponent<BallController>();
	}

	public void CustomUpdate(out BallController.AirState airState, out Quaternion floorRot)
	{
		airState = colIsGrounded ? BallController.AirState.Grounded : BallController.AirState.Air;  // set directly, first

		if (angleCollided >= ballController.GetMaxClimbAngle && !OnWall(angleCollided))
			airState = BallController.AirState.SteepSlope;     // overwrite colIsGrounded if angle is too steep
		
		floorRot = this.floorRot;
	}

	void GetCollisionInfo(Collision collision)
	{
		bool hitMovingPlatform = false;
		bool hitTwirlingPlatform = false;

		float newAngleCollided = angleCollided;
		Quaternion newFloorRot = Quaternion.identity;
		ballController.SetAngleCollided(collision.contacts[0].normal, out newAngleCollided, out newFloorRot);

		if (collision.gameObject.GetComponent<ForceSlide>())
			newAngleCollided = ballController.GetMaxClimbAngle;

		bool lesserAngleSameFrame = frameStamp == Time.frameCount && newAngleCollided < angleCollided;
		bool newFrame = frameStamp != Time.frameCount;

		if (lesserAngleSameFrame || newFrame)
		{
			angleCollided = newAngleCollided;
			if (angleCollided < ballController.GetMaxClimbAngle)
			{
				colIsGrounded = true;
				floorRot = newFloorRot;

				if (collision.gameObject.tag == "MovingPlatform")
				{
					hitMovingPlatform = true;
					GameObject platObj = collision.collider.gameObject;
					if (currentMovePlat == null || currentMovePlat.gameObject != platObj)
						currentMovePlat = platObj.GetComponent<Platform_TrackDistance>();
				}

				if (collision.gameObject.tag == "TwirlingPlatform")
				{
					hitTwirlingPlatform = true;
					GameObject platObj = collision.collider.gameObject;
					if (twirlPlat == null || twirlPlat.gameObject != platObj)
						twirlPlat = platObj.GetComponent<TrackTwirlBase>();
				}
			}
		}

		if (!hitMovingPlatform)
			DisableMovingPlatform();

		if (!hitTwirlingPlatform)
			DisableTwirlingPlatform();

		frameStamp = Time.frameCount;
	}

	void OnCollisionEnter(Collision collision)
	{
		GetCollisionInfo(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		GetCollisionInfo(collision);
	}

	void OnCollisionExit(Collision collision)
	{
		colIsGrounded = false;
		angleCollided = 0;
		floorRot = Quaternion.identity;

		DisableMovingPlatform();
		DisableTwirlingPlatform();
	}

	IEnumerator WaitThenDisablePlat()
	{
		yield return new WaitForSeconds(0.22f);

		if (!ballController.RecentlyEnabled)
			currentMovePlat = null;
	}
}
