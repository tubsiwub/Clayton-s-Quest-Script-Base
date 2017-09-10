using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
	protected Transform cam;
	protected CharacterController charController;
	protected PlayerHandler playerHandler;
	protected CapsuleCollider capsuleCollider;
	protected Transform rotateMesh;
	protected Rigidbody rb;
	protected Animator humanAnimator;
	public Animator HumanAnimator { get { return humanAnimator; } }

	protected float capsuleHeight;
	protected float transitionHopAmount;
	protected const float maxClimbAngle = 45;
	protected const float wallAngle = 80;
	protected const float minSlopeAngle = 5; // minimum angle to be considered (climbable) slope
	public float GetMaxClimbAngle { get { return maxClimbAngle; } }
	public float GetMinSlopeAngle { get { return minSlopeAngle; } }
	public float GetWallAngle { get { return wallAngle; } }
	protected bool handsAvailable;
	public bool HandsAvailable { get { return handsAvailable; } }
	protected bool isPhysicsControlled = false;
	public bool IsPhysicsControlled { get { return isPhysicsControlled; } }

	public abstract bool IsGrounded();
	public abstract bool IsFalling();
	public abstract Vector3 GetVelocity();
	public abstract void PushAway(Vector3 direction, float force);
	public virtual void SetFrozen(bool frozen, bool freezeAnimator) { isFrozen = frozen; }
	public virtual void SetFreezeJump(bool frozen) { jumpFrozen = frozen; }
	public virtual void SetHorizontalFrozen(bool frozen) { horizontalFrozen = frozen; }
	protected bool isFrozen = false;
	private static bool jumpFrozen = false;
	protected bool horizontalFrozen = false;
	public bool IsFrozen { get { return isFrozen; } }
	public bool IsJumpFrozen { get { return jumpFrozen; } }
	public bool IsHorizontalFrozen { get { return horizontalFrozen; } }
	public virtual void KillVelocity() { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
	public void SetDetectCollisions(bool set) { rb.detectCollisions = set; }
	public virtual void DoHighJump(float noPressHeight, float pressHeight) { }

	public abstract void DisableByHandler(PlayerHandler.PlayerState nextState);
	public abstract void EnableByHandler(Vector3 velocityChange, bool doHop);

	protected void Awake()	// needs to be called from sub-classes
	{
		cam = Camera.main.transform;
		charController = GetComponent<CharacterController>();
		playerHandler = GetComponent<PlayerHandler>();
		capsuleCollider = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();

		rotateMesh = playerHandler.RotateMesh;
		humanAnimator = playerHandler.HumanAnimator;
	}

	public static Vector3 GetMovement(Transform cam, Vector3 stickDir, Quaternion floorRotation)
	{
		float speed = Mathf.Clamp(Vector3.Magnitude(stickDir), 0, 1);

		Vector3 cameraDir = cam.forward; cameraDir.y = 0.0f;
		Vector3 moveDir = Quaternion.FromToRotation(Vector3.forward, cameraDir) * stickDir;     // referential shift
		moveDir = floorRotation * moveDir;

		// fixes bug by flipping movement x around
		if (cam.eulerAngles.y >= 179.920f && cam.eulerAngles.y <= 180.08f)
			moveDir = new Vector3(-moveDir.x, moveDir.y, moveDir.z);

		//Debug.DrawRay(transform.position, moveDir.normalized * 3, Color.blue);

		return moveDir.normalized * speed;
	}

	public static Vector3 GetMovement(Transform cam, Vector3 stickDir)
	{
		return GetMovement(cam, stickDir, Quaternion.identity);
	}

	protected Vector3 GetMovement(Vector3 stickDir, Quaternion floorRotation)
	{
		return GetMovement(cam, stickDir, floorRotation);
	}

	public void SetAngleCollided(Vector3 normal, out float angleCollided, out Quaternion floorRot)
	{
		float angle = Vector3.Dot(normal, Vector3.down);
		angle = 180 - (Mathf.Acos(angle) * Mathf.Rad2Deg);

		if (float.IsNaN(angle)) angle = 0;

		Vector3 rotation = Vector3.Cross(normal, Vector3.down);
		rotation = rotation.normalized * angle;

		floorRot = Quaternion.Euler(rotation);
		angleCollided = Quaternion.Angle(floorRot, Quaternion.identity);
	}

	public bool BallCollidingUpward()
	{
		RaycastHit hitInfo;
		const float HeightOffset = 0.2f;
		float sweepHeight = (charController.height - capsuleCollider.height) + HeightOffset;

		Vector3 lastPos = rb.position;
		rb.MovePosition(rb.position + (Vector3.down * HeightOffset));

		bool whatever = (rb.SweepTest(Vector3.up, out hitInfo, sweepHeight, QueryTriggerInteraction.Ignore));

		rb.MovePosition(lastPos);

		return whatever;
	}
}
