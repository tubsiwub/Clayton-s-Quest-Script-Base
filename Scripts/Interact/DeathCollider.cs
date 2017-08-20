using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCollider : MonoBehaviour
{
	[SerializeField] bool playDeathAnimation = false;

	bool DoWaterDeath { get { return gameObject.name == "Water"; } }

	void OnTriggerEnter(Collider col)
	{
		if (DoWaterDeath)
		{
			if (col.gameObject.tag == "Marble")
			{
				col.gameObject.GetComponent<Marble>().BeginCollectOffEdge();
			}

			Pickupable pickupable = col.gameObject.GetComponent<Pickupable>();
			if (pickupable != null)
				pickupable.Respawn();
		}
	}

	void OnTriggerStay(Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			HealthManager.AnimType animType;
			if (!playDeathAnimation)
				animType = HealthManager.AnimType.None;
			else
				animType = DoWaterDeath ? HealthManager.AnimType.Drown : HealthManager.AnimType.Default;

			PerformHit(animType);
		}
	}

	protected virtual void PerformHit(HealthManager.AnimType animType)
	{
		HealthManager.instance.LoseAllLives(animType);
	}
}
