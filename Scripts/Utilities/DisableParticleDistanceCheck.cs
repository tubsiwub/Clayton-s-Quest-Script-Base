using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableParticleDistanceCheck : MonoBehaviour {

	Transform playerObj;

	public float distanceToDisable;

	void Start () 
	{
		playerObj = GameObject.FindWithTag ("Player").transform;
	}
	
	void Update () 
	{
		float distanceCheck = Vector3.Distance (this.transform.position, playerObj.position);

		var em = GetComponent<ParticleSystem> ().emission;

		if (distanceCheck < distanceToDisable)
		{
			em.enabled = false;
		}
		else
		{
			em.enabled = true;
		}
	}
}
