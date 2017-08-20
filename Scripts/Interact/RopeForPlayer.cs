// lmao this script sucks

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeForPlayer : MonoBehaviour
{
	[SerializeField] bool jankSwitch = false;
	public bool IsJanked { get { return jankSwitch; } }

	PlayerHandler playerHandler;

	void Awake()
	{
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player")
			playerHandler.SetRope(transform);
	}

	void OnTriggerStay(Collider col)
	{
		if (col.gameObject.tag == "Player")
			playerHandler.SetRope(transform);
	}

	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.tag == "Player")
			playerHandler.SetRope(null);
	}
}
