using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Placement:  _name_ enemy type, parent body

// Purpose:  Controls enemy movement and behavior as governed by current states
using UnityEngine.AI;

public class Enemy_Movement_Old : MonoBehaviour {
	 
	Enemy_States stateScript;

	NavMeshAgent navAgent;
	NavMeshAgentScript navAgentScript;

	Rigidbody rb;

	GameObject
	ledgeCheck;

	public float 
	f_maxWanderRadius = 15,
	f_currentWanderPosition = 0,
	f_moveSpeed = 2;

	// Timers
	float
	f_movingTimer = 1,
	f_rotatingTimer = 3,
	massResolution = 100,
	f_rotateSpeed = 40;

	int 
	rotDirection = 1;

	Vector3 
	v3_startPosition,
	lookPos;

	bool
	b_moving = false,
	b_rotating = false,
	b_radiusCorrection = false,
	b_ledgeCollision = false;

	Quaternion
	lookRot;

	void Start () {

		// Script-Specific Components
		stateScript = GetComponent<Enemy_States> ();

		// Components
		rb = GetComponent<Rigidbody> ();
		navAgent = GetComponent<NavMeshAgent> ();
		navAgentScript = GetComponent<NavMeshAgentScript> ();

		// GameObjects
		foreach (Transform child in this.transform)
			if (child.name == "LedgeCheck")
				ledgeCheck = child.gameObject;

		massResolution = rb.mass;

		v3_startPosition = transform.position;
	}

	void Update () {

		// Overall movement decision check
		switch (stateScript.STATE) {

		case "wander":

			#region EDGE DETECTION
			RaycastHit hit;

			float rayLength = 50;

			if (Physics.Raycast (ledgeCheck.transform.position, Vector3.down, out hit, rayLength)) {

				if(!b_ledgeCollision){
					if (hit.distance > 2) {

						#region OLD
						lookPos = v3_startPosition - transform.position;

						lookPos.y = 0;

						lookRot = Quaternion.LookRotation (lookPos);

						transform.rotation = Quaternion.RotateTowards (transform.rotation, lookRot, 360 * Time.deltaTime);

						rb.velocity = transform.forward * f_moveSpeed * massResolution * Time.deltaTime;
						#endregion

					} 
					else if(hit.distance > 1 && hit.distance < 2) {

						v3_startPosition = hit.transform.position;
						v3_startPosition.y += GetComponent<MeshRenderer>().bounds.center.y;

					}
				}
				else {

					rb.velocity = transform.forward * -f_moveSpeed * massResolution * Time.deltaTime;

				}
			}
			#endregion

			#region MOVEMENT
			f_currentWanderPosition = Vector3.Distance (transform.position, v3_startPosition);

			// If the enemy leave its perimeter, have it try to return
			if (Vector3.Distance (transform.position, v3_startPosition) > f_maxWanderRadius) {

				navAgent.enabled = true;
				navAgentScript.enabled = true;
							
			} else {

				navAgent.enabled = false;
				navAgentScript.enabled = false;

			}

			#region OLD
//			if (Vector3.Distance (transform.position, v3_startPosition) > f_maxWanderRadius) {
//
//				b_radiusCorrection = true;
//							
//			} else {
//
//				b_radiusCorrection = false;
//
//			}
			#endregion

			// As long as you're within the correct wander radius
			if (!b_radiusCorrection) {

				f_rotateSpeed = 40;

				if (b_moving) {

					if (b_rotating)
						transform.Rotate (0, f_rotateSpeed * Time.deltaTime * rotDirection, 0);

					rb.velocity = transform.forward * f_moveSpeed * massResolution * Time.deltaTime;
				}

			} else {

				f_rotateSpeed = 65;

				if (b_moving) {
					
					lookPos = v3_startPosition - transform.position;

					lookPos.y = 0;

					lookRot = Quaternion.LookRotation (lookPos);

					transform.rotation = Quaternion.RotateTowards (transform.rotation, lookRot, f_rotateSpeed * Time.deltaTime);

					rb.velocity = transform.forward * f_moveSpeed * massResolution * Time.deltaTime;
				}

			}
			#endregion

			#region SLOW TO STOP - MOVEMENT
			// Not moving?  Stop moving.
			if (!b_moving) {
				
				rb.velocity = Vector3.Lerp (rb.velocity, Vector3.zero, 2 * Time.deltaTime);
			}
			#endregion

			#region TIMERS
			// Timer toggle for enemy movement
			f_movingTimer -= Time.deltaTime;
			if (f_movingTimer <= 0) {

				f_movingTimer = Random.Range (3, 6);

				b_moving = !b_moving;
			}

			// Timer toggle for enemy rotation
			f_rotatingTimer -= Time.deltaTime;
			if (f_rotatingTimer <= 0) {

				f_rotatingTimer = Random.Range (1, 3);

				int direction = Random.Range(0,2);

				if(direction == 0)
					rotDirection = -1;
				else 
					rotDirection = 1;
				
				b_rotating = !b_rotating;
			}
			#endregion

			break;

		case "attack":

			break;

		case "chase":

			break;

		}
	}

	public void LedgeCheck_Collision(bool value){

		b_ledgeCollision = value;

	}

	void LateUpdate()
	{

	}
}
