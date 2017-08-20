using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxScript : MonoBehaviour
{
	GameObject player;

//	public float distance;
//	public float tooClose;
	public float force;

	// Use this for initialization
	void Start () 
	{
		player = GameObject.FindWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () 
	{
//		distance = Vector3.Distance (transform.position, player.transform.position);
//
//		if (distance <= tooClose)
//		{
//			Vector3 pushDir = player.transform.position - transform.position;
//			pushDir.y = 0;
//			pushDir.Normalize();
//
//			HealthManager.instance.LoseALifeAndPushAway(pushDir, force);
//		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			Vector3 pushDir = player.transform.position - transform.position;
			pushDir.y = 0;
			pushDir.Normalize();
			
			HealthManager.instance.LoseALifeAndPushAway(pushDir, force);
		}
	}
}
