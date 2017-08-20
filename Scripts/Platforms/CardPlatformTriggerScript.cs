using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlatformTriggerScript : MonoBehaviour
{
	public bool platTriggered = false;

	public GameObject player;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindWithTag("Player");
	}

	// Update is called once per frame
	void Update () 
	{
		float elapsed = Time.deltaTime;			
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			platTriggered = true;
		}
	}
}