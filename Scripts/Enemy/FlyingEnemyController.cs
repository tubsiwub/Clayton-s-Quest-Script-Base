using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyController : Enemy {

	public bool PUSHABLE = true;

	// Enemy will fly around this point while Wandering
	public GameObject navPoint;
	public GameObject awayPoint;

	Vector3 navPointOrigin;

	public float rotaAngle = 1;			// how fast the 'bird' rotates around it's target position
	public float moveToSpeed = 1;		// how fast the 'bird' moves toward it's position
	public float diveSpeed = 0.1f;		// how fast the 'bird' dives
	public float diveHeight = 5.0f;		// the height the navPoint should follow the player at
	public float diveTimer = 4.0f;		// how often the 'bird' dives

	[Tooltip ("Amount of force the enemy shoves the player with.")]
	public float knockbackAmount;

	[Range (0.1f, 1.0f)]
	public float difficulty = 0.5f;

	string currentState = "wander";

	bool diving = false;		// currently attacking
	bool divable = false;	// able to attack (within range)

	GameObject playerObj;
	Vector3 playerPos;

	Animator animator;

	float chaseTimer = 15.0f;
	float chaseTimerMax = 15.0f;

	float chaseCooldown = 0.0f;
	float chaseCooldownMax = 5.0f; 

	public int maxHealth = 25;

	void Start () {

		// Set Objects
		playerObj = GameObject.FindWithTag ("Player");
		animator = transform.GetChild(0).GetComponent<Animator> ();

		navPointOrigin = navPoint.transform.position;

		health = maxHealth;

		StartCoroutine (Diving (diveTimer));

	}


	void Update () {

		currentState = GetComponent<Enemy_States> ().STATE;

		playerPos = playerObj.transform.position;

		// Find the player's forward vector
		Vector3 playerForward = Vector3.zero;
		foreach (Transform child in playerObj.transform)
		if (child.name == "RotateMesh") playerForward = child.forward;

		// Keep the awayPoint a specific distance away from the player
		Vector3 navToEnemyDirection = playerPos - navPoint.transform.position;
		navToEnemyDirection.Normalize();
		awayPoint.transform.position = navPoint.transform.position - playerForward * 4;
		awayPoint.transform.position = new Vector3 (awayPoint.transform.position.x, navPoint.transform.position.y, awayPoint.transform.position.z);


		Ray ray = new Ray (navPoint.transform.position, -Vector3.up);
		RaycastHit hit;

		switch (currentState.ToLower()) {

		#region WANDER
		case "wander":

			// NavPoint position
			if (Physics.Raycast (ray, out hit, 100)) {

				if (hit.transform.tag != "Player" && hit.transform.tag != "Enemy")
					navPoint.transform.position = hit.point + (Vector3.up * 5);

			}

			// Enemy movement
			if(!diving){

				if(chaseCooldown > 0)
					chaseCooldown -= Time.deltaTime;

				animator.SetBool ("Moving", true);

				transform.LookAt(navPoint.transform);

				BaseMovement ();

				if(Vector3.Distance(transform.position, playerPos) < 8 && chaseCooldown <= 0){
					
					GetComponent<Enemy_States> ().STATE = "chase";

					chaseTimer = chaseTimerMax;

				}

			}

			break;
			#endregion

		case "chase":

			// NavPoint position
			navPoint.transform.position = playerObj.transform.position + Vector3.up * 4.5f;

			// Enemy movement
			if (!diving) {

				if(chaseTimer > 0)
					chaseTimer -= Time.deltaTime;

				if (chaseTimer <= 0) {
					
					GetComponent<Enemy_States> ().STATE = "wander";

					chaseCooldown = chaseCooldownMax;

					// Enemy goes back to original area
					navPoint.transform.position = navPointOrigin;

					break;	// leave early, we're done here!

				}

				animator.SetBool ("Moving", true);

				transform.LookAt (navPoint.transform);

				BaseMovement ();

			}

			navPoint.transform.position = new Vector3 (playerPos.x, navPoint.transform.position.y, playerPos.z);

			break;

		}





	}

	void BaseMovement(){

		float distanceToNavPoint = Vector3.Distance (transform.position, navPoint.transform.position);

		// guily until proven innocent!
		divable = false;

		if (distanceToNavPoint > 4){

			transform.position = Vector3.Lerp (transform.position, navPoint.transform.position, 1.5f * Time.deltaTime);

		}
		else if (distanceToNavPoint < 2) {

			transform.position = Vector3.Lerp (transform.position, awayPoint.transform.position, 0.8f * Time.deltaTime);

		}
		else {

			// ...found innocent!
			if(currentState.ToLower() == "chase")
				divable = true;

			// Look perpendicular to where navPoint is
			transform.rotation = Quaternion.Euler(new Vector3(0,transform.rotation.eulerAngles.y,transform.rotation.eulerAngles.z));
			transform.Rotate(0,-90,0);

			transform.RotateAround (navPoint.transform.position, Vector3.up, rotaAngle * Time.deltaTime);

		}

		// If the enemy should go outside of height bounds, reign it back in smoothly
		if(transform.position.y > navPoint.transform.position.y + 0.25f ||
			transform.position.y < navPoint.transform.position.y - 0.25f){

			transform.position = Vector3.Lerp(transform.position, awayPoint.transform.position, moveToSpeed * Time.deltaTime);

		}

	}


	// Restore Health
	public override void HealEnemy(int healAmount){

		if (health + healAmount <= maxHealth)
			health += healAmount;
		else
			health = maxHealth;

	}


	public override void DamageEnemy(int damageAmount){

		health -= damageAmount;

		DamageCheck ();

	}

	public override void KillEnemy(){

		health -= maxHealth;

		DamageCheck ();

	}

	void DamageCheck(){

		// Stuff

	}


	IEnumerator Diving(float time){

		yield return new WaitForSeconds (time);

		// DIVING

		// Stop until allowed
		while (!divable) {
			yield return new WaitForEndOfFrame ();
		}

		diving = true;
		animator.SetBool ("Diving", true);

		float counter = 0.0f;

		// Dive down
		while (counter < diveHeight) {

			// Look at Player
			transform.LookAt (playerObj.transform);
			transform.rotation = Quaternion.Euler (new Vector3 (0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));

			float distance = Vector3.Distance (transform.position, navPoint.transform.position);

			if (distance < 1)
				distance = 1;

			distance *= difficulty;

			transform.position = Vector3.MoveTowards (transform.position, navPoint.transform.position - (Vector3.up * counter), diveSpeed * Time.deltaTime * distance);

			counter += diveSpeed * Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		float delay = 3.0f;
		while (delay > 0) {

			delay -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}


		// Dive up
		while (counter > 0) {

			// Look at Player
			transform.LookAt (playerObj.transform);
			transform.rotation = Quaternion.Euler (new Vector3 (0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));

			float distance = Vector3.Distance (transform.position, navPoint.transform.position);

			if (distance < 1)
				distance = 1;

			distance *= difficulty;

			transform.position = Vector3.MoveTowards (transform.position, awayPoint.transform.position - (Vector3.up * counter), diveSpeed * Time.deltaTime * distance);

			counter -= diveSpeed * Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		diving = false;
		animator.SetBool ("Diving", false);

		// Stop until allowed
		while (!divable) {
			yield return new WaitForEndOfFrame ();
		}

		// Restart Coroutine
		StartCoroutine (Diving (time));

	}

	void HitPlayer()
	{
		if (PUSHABLE)
		{
			Vector3 pushDir = playerObj.transform.position - transform.position;
			pushDir.y = 0; pushDir.Normalize();
			if(HealthManager.instance)
				HealthManager.instance.LoseALifeAndPushAway(pushDir, knockbackAmount);
		}
	}

	void OnTriggerEnter(Collider col){

		// Hurt player when enemies get touched
		if (col.transform.tag == "Player") {
			HitPlayer ();
		}

	}

	Vector3 FindOppositePoint(Vector3 posA, Vector3 posB){

		Vector3 directionOfLocation = posA - posB;

		Vector3 oppositePoint = directionOfLocation + posA;

		return oppositePoint;

	}

	Vector3 FindDistantPoint(Vector3 posA, Vector3 posB){

		Vector3 directionOfLocation = posB - posA;

		Vector3 distantPoint = directionOfLocation * 2;

		return distantPoint;

	}
}
