using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHolder : MonoBehaviour
{
	[SerializeField] Transform rightHand;
	[SerializeField] Transform leftHand;
	public Transform GetRightHand { get { return rightHand; } }
	public Transform GetLeftHand { get { return leftHand; } }

	List<Pickupable> nearbyObjects;
	Pickupable heldObject;
	Pickupable lastClosest;
	HumanController humanController;
	PlayerHandler playerHandler;
	
	public bool IsHolding { get { return heldObject != null; } }
	public Pickupable HeldObject { get { return heldObject; } }
	public bool HasObjectsNearby { get { return nearbyObjects.Count > 0; } }

	bool doingWindUp = false;
	public bool DoingWindUp { get { return doingWindUp; } }

	const float windUpSpeed = 10;
	const float minThrowForce = 3;
	const float maxThrowForce = 8;
	float throwForce = minThrowForce;

	void Awake()
	{
		heldObject = null;
		nearbyObjects = new List<Pickupable>();
		humanController = GetComponentInParent<HumanController>();
		playerHandler = GetComponentInParent<PlayerHandler>();
	}

	void OnDisable()
	{
		if (lastClosest != null)
			lastClosest.StopGlow();
	}

	public void AddToList(Pickupable item)
	{
		nearbyObjects.Add(item);
	}

	public void RemoveFromList(Pickupable item)
	{
		nearbyObjects.Remove(item);
	}

	public void SetHolding(Pickupable newObj)
	{
		int r = Random.Range(1, 4);
		SoundManager.instance.PlayClip("PickupGrunt0" + r);

		Vector3 direction = (newObj.transform.position - transform.position);
		direction.y = 0;
		humanController.SetDirection(direction.normalized);

		humanController.HumanAnimator.SetTrigger("Pickup");
		humanController.SetFrozen(true, false);
		heldObject = newObj;
		lastClosest = null;
		playerHandler.SetSecondaryAction(PlayerHandler.SecondaryAction.Holding);
		playerHandler.SetFaceAnimation("Confused");
	}

	public void PickupReachedFloor()
	{
		if (!IsHolding) return;

		humanController.HumanAnimator.SetBool("Holding", true);
		heldObject.SetHandPos(rightHand);
	}

	public void PickupAnimationDone()
	{
		if (!IsHolding) return;

		humanController.SetFrozen(false, false);
		heldObject.PickupAnimationDone();
	}

	void LateUpdate()
	{
		if (IsHolding)
		{
			HandleHolding();
			HandleThowing();
		}
		else
			HandleNearby();
	}

	void HandleHolding()
	{
		heldObject.CustomUpdate();
	}

	void HandleThowing()
	{
		bool buttonDown = Input.GetButtonDown(PlayerHandler.AttackString);
		bool buttonHeld = Input.GetButton(PlayerHandler.AttackString);
		bool buttonUp = Input.GetButtonUp(PlayerHandler.AttackString);
		bool inTossAnim = humanController.HumanAnimator.GetCurrentAnimatorStateInfo(4).IsName("hero_upperbody_toss");

		if (!doingWindUp && buttonDown && !inTossAnim)
		{
			humanController.HumanAnimator.SetTrigger("Windup");
			doingWindUp = true;
		}

		if (doingWindUp)
		{
			if (buttonHeld && !inTossAnim)
			{
				throwForce += windUpSpeed * Time.deltaTime;
				if (throwForce > maxThrowForce) throwForce = maxThrowForce;
			}

			if (buttonUp && !inTossAnim)
			{
				humanController.HumanAnimator.SetTrigger("Toss");
			}

			if (inTossAnim)
			{
				doingWindUp = false;
				Throw();

				throwForce = minThrowForce;
			}
		}
	}

	void HandleNearby()
	{
		// can't pick stuff up while jumping, you jerk
		if (!humanController.IsGrounded())
		{
			if (lastClosest != null) lastClosest.StopGlow();
			return;
		}
		
		Pickupable closest = FindClosestObject();

		if (closest != null)
		{
			if (closest != lastClosest)		// switched to closer objects
			{
				closest.Glow();
				if (lastClosest != null) lastClosest.StopGlow();	// stop glowing object if we switched to a closer one
			}

			lastClosest = closest;

			closest.CustomUpdate();
		}
		if (closest == null && lastClosest != null)		// stop glowing object if we're too far away
		{
			lastClosest.StopGlow();
			lastClosest = null;
		}
	}

	Pickupable FindClosestObject()
	{
		float minDist = float.MaxValue;
		int foundNdx = -1;

		for (int i = 0; i < nearbyObjects.Count; i++)
		{
			float dist = Vector3.Distance(nearbyObjects[i].transform.position, transform.position);
			if (dist < minDist)
			{
				foundNdx = i;
				minDist = dist;
			}
		}

		if (foundNdx != -1)
			return nearbyObjects[foundNdx];
		else return null;
	}

	void StopHolding()
	{
		humanController.HumanAnimator.SetBool("Holding", false);
		playerHandler.SetSecondaryAction(PlayerHandler.SecondaryAction.None);
		playerHandler.SetFaceAnimation("Smile");

		throwForce = minThrowForce;
		doingWindUp = false;
	}

	public void Drop()
	{
		if (IsHolding)
		{
			int r = Random.Range(1, 4);
			SoundManager.instance.PlayClip("PickupGrunt0" + r);

			StopHolding();
			heldObject.PutDown(true);
			heldObject = null;
		}
	}

	void Throw()
	{
		int r = Random.Range(1, 4);
		SoundManager.instance.PlayClip("PickupGrunt0" + r);

		float targetThrowForce = throwForce;
		StopHolding();	// we clear throwForce here, so save that for the throw
		heldObject.Thow(transform.forward, targetThrowForce);
		heldObject = null;
	}
}
