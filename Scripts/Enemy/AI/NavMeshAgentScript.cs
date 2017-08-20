using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
 
public class NavMeshAgentScript : MonoBehaviour {

	Enemy_States stateScript;

	public Transform[] target;
	public Transform[] SetTarget{ 
		set 
		{ 
			target = value;
		} 
	}

	public Transform originMarker;

	int choice = 0;

	NavMeshAgent agent;
	NavMeshAgentScript agentScript;

	void Start () 
	{
		// Script-Specific Components
		stateScript = GetComponent<Enemy_States> ();

		// Components
		agent = GetComponent<NavMeshAgent> ();
		agentScript = GetComponent<NavMeshAgentScript> ();
	}


	void Update () 
	{
		switch (stateScript.STATE) {

		#region WANDER
		case "wander":

			agent.SetDestination (originMarker.position);
		
			break;
			#endregion

		#region ATTACK
		case "attack":

			break;
			#endregion

		#region CHASE
		case "chase":

			agent.SetDestination (target[choice].position);

			float distance = Vector3.Distance (this.transform.position, target [choice].position);

			if (Mathf.Abs (distance) < 1.0f) {

				choice += 1;

				if (choice >= target.Length) {
					choice = 0;
				}
			}

			break;
			#endregion

		}
	}

	public void JUMP(Vector3 direction, float powerX, float powerY, float time){

		agent.enabled = false;
		agentScript.enabled = false;

		Vector3 lookAtPosition = transform.position + direction;
		transform.LookAt (lookAtPosition);

		GetComponent<Rigidbody> ().velocity += (direction * powerX) + new Vector3 (0, powerY, 0) ;

		StartCoroutine (ResetAgent (time));

	}

	IEnumerator ResetAgent(float time){

		yield return new WaitForSeconds(time);

		agent.enabled = true;
		agentScript.enabled = true;

	}
}
