using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroAnimationEvents : MonoBehaviour
{
	private Animator humanAnimator;
	private PlayerHandler playerHandler;
	private PlayerHolder playerHolder;

	protected void OnEnable()
	{
		humanAnimator = GetComponent<Animator>();
		playerHandler = GetComponentInParent<PlayerHandler>();
		playerHolder = GetComponentInParent<PlayerHolder>();
	}

	public virtual void ToBall()
	{
		playerHandler.ToBallAnimComplete();
	}

	public virtual void ToHuman()
	{
		playerHandler.ToHumanAnimComplete();
	}

	public void PickupReachedFloor()
	{
		playerHolder.PickupReachedFloor();
	}

	public void PickupAnimationDone()
	{
		playerHolder.PickupAnimationDone();
	}

	public void DoneWithDeathAnimation()
	{
		HealthManager.instance.DoRespawnStuff();
	}

	// right, so, this is pretty wacky.......
	// there are two rope join animations. One that's normal, one that has
	// an animation event at the BEGINNING which calls this. Why the beginning? 
	// Because it plays in reverse. It's the "leave" animation. Anyway, why do we need
	// to directly set the animation (crossfade) instead of use mecanim? well, mecanim
	// had this dumb pause at the end of the leave animation, because it was trying to reajust
	// for the position offset. using crossfade, by directly calling it at the 
	// end of the leave animation sovles this
	public void GetOffRope()
	{
		humanAnimator.CrossFade("hero_idle", 0.1f);
		StartCoroutine("DisableRopeLayer");
	}

	private IEnumerator DisableRopeLayer()
	{
		while (humanAnimator.GetLayerWeight(1) > 0)
		{
			if (playerHandler.CurrentState == PlayerHandler.PlayerState.OnRope)
			{	// in case we activate rope state while running this coroutine
				humanAnimator.SetLayerWeight(1, 1);
				StopCoroutine("DisableRopeLayer");
			}

			float weight = humanAnimator.GetLayerWeight(1);
			humanAnimator.SetLayerWeight(1, weight - (6 * Time.deltaTime));
			yield return null;
		}

		humanAnimator.SetLayerWeight(1, 0);
	}

	public void StandUpFromSit()
	{
		playerHandler.SetFrozen(false, false);
	}

	public void PickupFromHuggingNow()
	{
		playerHandler.PickupOrigami();
	}

	public void DisappearOrigami()
	{
		playerHandler.DisappearOrigami();
	}

	public void DoneHugging()
	{
		playerHandler.FinishOrigamiAnimation();
	}
}
