using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LargeEnemyController : MonoBehaviour {

	public List<Transform> pathPositions;

	int currentPathPosition = 0;

	float navSpeed = 3;

	public enum States{

		CHASE,
		WANDER,
		ATTACK

	}

	public States currentState = States.WANDER;

	PlayerHandler.PlayerState playerState;


	// NavMesh

	NavMeshAgent agent;


	// Move colliding objects

	List<GameObject> collidingObjects;

	float maxChaseRadius = 20;

	Vector3 lastPosition;
	Vector3 currentPosition;
	Vector3 lastSafePatrolPosition;


	// Sight

	bool detectionEnabled = true;


	void Start () {
		
		agent = transform.parent.GetComponent<NavMeshAgent> ();
			
		collidingObjects = new List<GameObject> ();

		currentPosition = transform.position;
	}

	//bool FoundPlayer = false;

	void Update () {

		lastPosition = currentPosition;
		currentPosition = transform.position;

		foreach (GameObject obj in collidingObjects) {
			if (Vector3.Distance (obj.transform.position, transform.position) > 3) {
				StartCoroutine(RemoveObjectFromList(obj));
			}
		}





		GameObject playerObj = GameObject.FindWithTag ("Player");
		float distanceToPlayer = Mathf.Abs (Vector3.Distance (playerObj.transform.position, transform.position));






		switch (currentState) {


		case States.WANDER:


			agent.speed = navSpeed;

			agent.SetDestination (pathPositions [currentPathPosition].position);


			float distanceToPathPoint = Vector3.Distance (pathPositions [currentPathPosition].position, transform.position);
			
			if (distanceToPathPoint < 2.2f) {
				if (currentPathPosition + 1 >= pathPositions.Count)
					currentPathPosition = 0;
				else 
					currentPathPosition += 1;
			}



			// Detect player
			RaycastHit hit;

			if (Physics.BoxCast(transform.position, new Vector3(0.8f,0.8f,0.8f) + transform.forward, transform.forward, out hit) && detectionEnabled) {
				if (hit.transform.tag == "Player") {

					currentState = States.CHASE;

					lastSafePatrolPosition = transform.position;

				}
			}

			break;


		case States.CHASE:

			agent.speed = navSpeed * 1.5f;

			agent.SetDestination (playerObj.transform.position);

			float distanceFromPatrolPosition = Vector3.Distance (lastSafePatrolPosition, transform.position);


			if (distanceFromPatrolPosition > maxChaseRadius) {

				currentState = States.WANDER;

				StartCoroutine (TurnOffDetection (2.0f));

			}


			if (distanceToPlayer > 8) {

				currentState = States.WANDER;

			}


			if (distanceToPlayer < 4) {

				currentState = States.ATTACK;

			}


			break;


		case States.ATTACK:


			if (distanceToPlayer > 5) {

				currentState = States.CHASE;

			}


			break;


		}


		Vector3 changeInDistance = lastPosition - transform.position;

		foreach (GameObject obj in collidingObjects) {
			obj.transform.position -= changeInDistance;
		}

	}

	void OnTriggerEnter(Collider col){

		if (col.transform.tag == "Player" && !collidingObjects.Contains(col.gameObject))
			collidingObjects.Add (col.gameObject);
		
	}

	void OnTriggerExit(Collider col){

		// Delay removing an object incase it happens during a foreach count or something.  You know the dril...
		if (col.transform.tag == "Player")
			StartCoroutine (RemoveObjectFromList (col.gameObject));
		
	}

	IEnumerator TurnOffDetection (float time){

		detectionEnabled = false;

		yield return new WaitForSeconds (time);

		detectionEnabled = true;

	}

	IEnumerator RemoveObjectFromList(GameObject obj){

		yield return new WaitForEndOfFrame ();

		if(collidingObjects.Contains(obj))
			collidingObjects.Remove (obj);
	}

}
