using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtCollider : DeathCollider
{
	[SerializeField] int lives = 1;
	[SerializeField] bool pushAway = true;
	[SerializeField] float pushForce = 10;

	protected override void PerformHit(HealthManager.AnimType animType)
	{
		if (!pushAway)
			HealthManager.instance.LoseLives(lives);
		else
		{
			Vector3 dir = GameObject.FindWithTag("Player").transform.position - transform.position; dir.y = 0;
			HealthManager.instance.LoseLivesAndPushAway(dir.normalized, pushForce, lives);
		}
	}
}
