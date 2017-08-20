using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallStickToFloor : MonoBehaviour
{
	PlayerHandler playerHandler;
	BallController ballController;

	void Start()
	{
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
		ballController = GameObject.FindWithTag("Player").GetComponent<BallController>();
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player" && playerHandler.CurrentState == PlayerHandler.PlayerState.Ball)
		{
			ballController.StickToFloor();
		}
	}

	void OnTriggerStay(Collider col)
	{
		if (col.gameObject.tag == "Player" && playerHandler.CurrentState == PlayerHandler.PlayerState.Ball)
		{
			ballController.StickToFloor();
		}
	}
}
