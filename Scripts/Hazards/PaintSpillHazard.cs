using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintSpillHazard : MonoBehaviour
{
	GameObject player;
	GameObject rotation;

	bool isSliding;

	public float force;

	PlayerHandler playerHandler;
	BallController ballController;

	void Start ()
	{
		player = GameObject.FindWithTag("Player");
		rotation = GameObject.Find ("Player/RotateMesh");

		playerHandler = player.GetComponent<PlayerHandler>();
		ballController = player.GetComponent<BallController>();
	}
	

	void Update () 
	{
		//Debug.Log(player.transform.forward);

		if (isSliding)
		{
			if (playerHandler.CurrentState == PlayerHandler.PlayerState.Human)
				player.transform.position += rotation.transform.forward * force * Time.deltaTime;

			if (playerHandler.CurrentState == PlayerHandler.PlayerState.Ball)
			{
				//Debug.Log("Got here?");
				player.transform.position += ballController.GetDirection * force * Time.deltaTime;

			}
		}
	} 

	void OnTriggerStay (Collider col) 
	{
		if (col.transform.tag == "Player")
		{
			if (playerHandler.CurrentState == PlayerHandler.PlayerState.Ball)
				playerHandler.SwitchState(PlayerHandler.PlayerState.Human);

			if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
			{
				player.GetComponent<PlayerHandler> ().SetFrozen (true, false);
				isSliding = true;
			}

            if (Input.GetButtonDown("Jump"))
            {
                isSliding = false;
                player.GetComponent<PlayerHandler>().SetFrozen(false, false);
            }
        }
	}  

	void OnTriggerExit (Collider col) 
	{
		if (col.transform.tag == "Player")
		{
			player.GetComponent<PlayerHandler>().SetFrozen(false, false);
            isSliding = false;

		}
	}  
}
