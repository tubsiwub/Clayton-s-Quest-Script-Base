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
	[SerializeField] bool invisibleObject;

	void Awake()
	{
		if (invisibleObject && GetComponent<Renderer>())
			GetComponent<Renderer>().enabled = false;
	}

	void OnTriggerStay(Collider obj)
	{
		if (obj.gameObject.tag == "Player")
		{
			if (bouncePointHeight != null && obj.transform.position.y < bouncePointHeight.position.y)
				return;

			PlayerHandler playerHandler = obj.GetComponent<PlayerHandler>();
			if (playerHandler.IsFalling())
				Bounce(playerHandler);
		}
	}

	void Bounce(PlayerHandler playerHandler)
	{
		playerHandler.DoHighJump(pressHeight, noPressHeight);

		if (animator != null)
			animator.SetTrigger(animationName);
	}
}
