using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceCollider : MonoBehaviour
{
	[SerializeField] float noPressHeight = 20;
	[SerializeField] float pressHeight = 30;
	[SerializeField] Transform bouncePointHeight;
	[SerializeField] Animator animator;
	[SerializeField] string animationName = "Bounce";

	int lastBounceStamp = 0;
	bool BouncedRecently { get { return Time.frameCount - lastBounceStamp <= 5; } }

	void OnTriggerStay(Collider obj)
	{
		if (obj.gameObject.tag == "Player")
		{
			if (bouncePointHeight != null && obj.transform.position.y < bouncePointHeight.position.y)
				return;

			PlayerHandler playerHandler = obj.GetComponent<PlayerHandler>();
			if (playerHandler.IsFalling() && !BouncedRecently)
				Bounce(playerHandler);
		}
	}

	void Bounce(PlayerHandler playerHandler)
	{
		lastBounceStamp = Time.frameCount;
		playerHandler.DoHighJump(pressHeight, noPressHeight);

		if (animator != null)
			animator.SetTrigger(animationName);
	}
}
