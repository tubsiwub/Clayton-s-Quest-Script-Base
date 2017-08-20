using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StapleScript : MonoBehaviour 
{
	public GameObject player;
	public GameObject hManager;
    
	public float force;
    
	void Start ()
	{
		player = GameObject.FindWithTag("Player");
		hManager = GameObject.Find ("HealthManager(Clone)");
	}
	
	void OnTriggerEnter (Collider col) 
	{
        if (col.transform.tag == "Player")
        {
            Vector3 pushDir = player.transform.position - transform.position;
            pushDir.y = 0;
            pushDir.Normalize();

            hManager.GetComponent<HealthManager>().LoseALifeAndPushAway(pushDir, force);
        }
    }  
}