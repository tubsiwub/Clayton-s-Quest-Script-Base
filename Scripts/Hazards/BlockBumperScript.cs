using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBumperScript : MonoBehaviour 
{
	public GameObject player;


	void Start () 
	{
		player = GameObject.FindWithTag("Player");

		Physics.IgnoreCollision (player.GetComponent<Collider> (), GetComponent<Collider> ());
	}
	
	void Update ()
	{
		
	}
}
