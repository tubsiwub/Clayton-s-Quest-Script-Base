// Tristan

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Placement:  _name_ enemy type, parent body 

// Purpose:  Controls enemy movement and behavior as governed by current states

public class NewSmallEnemyScript : Enemy { 

	public bool PUSHABLE = false;

	public GameObject originMarker;
	public GameObject radiusMarker;

	Enemy_States stateScript;

	public Renderer materialRend;

	float 
	origRed,
	origGreen,
	origBlue;

	Rigidbody rb;
	Animator animator;

	GameObject
	playerObj;

	// positioning
	public float 
	f_maxWanderRadius = 15,
	f_distanceToAttack = 1.2f,
	f_distanceToLoseTarget = 14,
	f_detectionRange = 7;

	// speeds
	public float wanderMoveSpeed = 500000;

	public float forceDownValue;

//	float
//	wanderRotateSpeed = 140,
//	chaseMoveSpeed = 4,
//	chaseRotateSpeed = 270;

	// Timers
	float
	f_movingTimer = 1,
	f_invincibilityTime = 1.0f,
	f_lastInvincTime,
	f_invincibilityTimeINIT = 1.0f,
	f_chaseCounter = 1.0f,
	f_playerDetectionTimeout = 0,
	f_detectPlayerSurprise = 0,
	waitBeforeFreeze = 3.0f;

	Vector3 
	v3_startPosition;

	public bool
	b_moving = false,
	b_foundPlayer = false,
	b_isJumping = false;

	public int maxHealth = 25; 

	string storageKey = "";

	void Start () {

		// Set parent values
		health = maxHealth;

		// Script-Specific Components
		stateScript = GetComponent<Enemy_States> ();

		// Components
		rb = GetComponent<Rigidbody> ();
		animator = GetComponentInChildren<Animator> ();

		// GameObjects
		playerObj = GameObject.FindWithTag ("Player");

		origRed = materialRend.material.color.r; 
		origGreen = materialRend.material.color.g; 
		origBlue = materialRend.material.color.b;

		// initialization
		v3_startPosition = radiusMarker.transform.position;
		f_maxWanderRadius = radiusMarker.transform.localScale.x / 2;
		radiusMarker.SetActive (false);


		// Save Data Check
		if(GetComponent<SavingLoading_StorageKeyCheck>()){

			storageKey = GetComponent<SavingLoading_StorageKeyCheck>().storageKey;

			if (SavingLoading.instance.CheckStorageKeyStatus (storageKey))
				Destroy (this.gameObject);
		}
	}

	void MoveTarget(){

		b_moving = true;

		originMarker.transform.position = new Vector3(
			Random.Range(v3_startPosition.x - f_maxWanderRadius*2, v3_startPosition.x + f_maxWanderRadius*2),
			v3_startPosition.y,
			Random.Range(v3_startPosition.z - f_maxWanderRadius*2, v3_startPosition.z + f_maxWanderRadius*2));

		// slight lag?
		while (Vector3.Distance (originMarker.transform.position, v3_startPosition) < 3 || Vector3.Distance (originMarker.transform.position, transform.position) < 1.5f) {

			originMarker.transform.position = new Vector3(
				Random.Range(v3_startPosition.x - f_maxWanderRadius*2, v3_startPosition.x + f_maxWanderRadius*2),
				v3_startPosition.y,
				Random.Range(v3_startPosition.z - f_maxWanderRadius*2, v3_startPosition.z + f_maxWanderRadius*2));

		}
	}

	void MoveTarget(Vector3 position){

		originMarker.transform.position = position;

		if (Vector3.Distance (v3_startPosition, position) > f_maxWanderRadius * 2) {

			stateScript.STATE = "wander";

			MoveTarget ();
		}

	}

	void PlaceOnGround(){

		if (!b_isJumping) {
			Ray groundRay = new Ray (transform.position + Vector3.up, Vector3.down * 2);
			RaycastHit groundHit;

			Debug.DrawRay (groundRay.origin, groundRay.direction, Color.red);

			if (Physics.Raycast (groundRay, out groundHit)) {
				if(groundHit.transform.tag != "Enemy")
				if (Vector3.Distance (transform.position, groundHit.point) < 2) {
					Vector3 yPosSet = transform.position;
					yPosSet.y = groundHit.point.y;
					transform.position = yPosSet;
				}
			}
		}

	}

	Vector3 newPosJ;

	Vector3 newRotation;


	void Update () {
		
		float enemyDistance = Vector3.Distance (transform.position, playerObj.transform.position);

		if(stateScript.STATE != "dead") {

			#region Invicibility Timer
			f_lastInvincTime = f_invincibilityTime;

			if (f_invincibilityTime > 0)
				f_invincibilityTime -= Time.deltaTime;
			else if(f_invincibilityTime != 0)	
				f_invincibilityTime = 0;

			// When the enemy loses invincibility, change the alpha
			if (f_lastInvincTime != f_invincibilityTime && f_invincibilityTime == 0) {

				Color tempColor = materialRend.material.color;
				tempColor.a = 1.0f;
				materialRend.material.color = tempColor;

			}

			if (f_invincibilityTime > 0) {
				if (rb.velocity != Vector3.zero)
					rb.velocity -= rb.velocity / 10;
			} 
			#endregion

			if (waitBeforeFreeze > 0)
				waitBeforeFreeze -= Time.deltaTime;

			// CANCEL THE UPDATE SHOULD THE ENEMIES BE TOO FAR AWAY
			if (enemyDistance > 30 && waitBeforeFreeze <= 0) {

				rb.constraints = RigidbodyConstraints.FreezeAll;

				PlaceOnGround ();

				CorrectAngle ();

				return;

			} else {

				rb.constraints = RigidbodyConstraints.None;

			}

			animator.SetFloat ("Distance", enemyDistance);


			f_playerDetectionTimeout -= Time.deltaTime;

			// If you've fallen a bit below the defined 'ground'; destroy yourself
			if (transform.position.y < radiusMarker.transform.position.y - 50) {
				Destroy (this.transform.parent.gameObject);
			}

			// View angle - draws (and calculates) the angle at which the enemy can see the player (looks for player)
			if (!b_foundPlayer && f_playerDetectionTimeout <= 0 && enemyDistance <= f_detectionRange) {

				// Roughly 6 rays
				for (int i = -30; i < 30; i += 10) {
		
					Vector3 degreeDiff = Quaternion.AngleAxis (i, transform.up) * transform.forward;
					b_foundPlayer = false;
	
					Ray ray = new Ray(transform.position, degreeDiff * f_detectionRange);
					RaycastHit hit;

					Debug.DrawRay (ray.origin, ray.direction * f_detectionRange, Color.red);
	
					// Did you find the player?
					if (Physics.Raycast (ray, out hit, f_detectionRange)) {
						if (hit.collider.tag == "Player" && !hit.collider.isTrigger) {
	
							// sends out notification to nearby enemies
							{
								GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
	
								// cycle through nearby enemies
								foreach (GameObject en in enemies) {	// all
									if (Vector3.Distance (this.transform.position, en.transform.position) < 4) // within distance
									{
										// set nearby enemies to 'found player' state
										en.GetComponent<NewSmallEnemyScript> ().FoundPlayer (hit.transform.gameObject);
									}
								}
							}
	
							// Found player; set data
							FoundPlayer (hit.transform.gameObject);
	
	
							break;
						}
					}
				}
			}

			PlaceOnGround ();

		}

		// Hacky attempt to make recovery easier for the enemy
		if (f_playerDetectionTimeout > 0) {
			newPosJ = transform.position;
		}

		// Overall movement decision check
		switch (stateScript.STATE.ToLower()) {

		#region WANDER
		case "wander":
			
			animator.SetBool("Attacking", false);
			animator.SetBool("Moving", true);
			animator.SetBool("Chasing", false);


			// Jumping (?)
			if (!b_isJumping && f_playerDetectionTimeout <= 0) {

				Ray wallRay = new Ray (transform.position + (Vector3.up * 0.5f), transform.forward * 1);
				Ray newHeightRay = new Ray (transform.position + (transform.forward * 1.5f) + (Vector3.up * 8), -transform.up * 99999);

				// Go through about 8 checks to get the highest point on a hill
				RaycastHit checkHit;
				Vector3 storeHit = Vector3.zero;
				for(int p = 0; p < 8; p++){
					Ray checkRay = new Ray (transform.position + (transform.forward * (p/2)) + (Vector3.up * 8), -transform.up * 99999);
					if (Physics.Raycast (checkRay, out checkHit)) {
						if(p == 0) storeHit = checkHit.point;
						else {
							if(checkHit.point.y > storeHit.y){
								storeHit = checkHit.point;
								newHeightRay = checkRay;
							}
						}
					}
				}

				RaycastHit hit2;

				Debug.DrawRay (wallRay.origin, wallRay.direction, Color.red);
				Debug.DrawRay (newHeightRay.origin, newHeightRay.direction, Color.red);

				if (Physics.Raycast (wallRay, out hit2) && enemyDistance > 2) {
					if (hit2.distance < 1 && !hit2.collider.isTrigger && hit2.transform.tag != "Enemy") {

						if (Physics.Raycast (newHeightRay, out hit2)) {
							if (hit2.distance < 8 && !hit2.collider.isTrigger) {

								newPosJ = hit2.point;
								b_isJumping = true;

							} else {
								MoveTarget ();
							}
						}

					} 
				}
			} else {

				if (Vector3.Distance (transform.position, newPosJ) > 0.01) {
					Debug.DrawRay(newPosJ + Vector3.up * 5, -Vector3.up * 8, Color.green);
					transform.position = Vector3.MoveTowards (transform.position, newPosJ, 0.5f);
				} else
					b_isJumping = false;

			}



			// If you're jumping, you cannot move.  Constantly set due to timers that can reset it.
			if(b_isJumping)
				b_moving = false;
			

			// If the enemy has seen and is in the first stages of noticing the player,
			//		play a special animation and pause movement for now
			if(f_detectPlayerSurprise <= 0){

				if (b_foundPlayer) { 

					stateScript.STATE = "chase";

				}

			}
			else {

				f_detectPlayerSurprise -= Time.deltaTime;

				animator.SetTrigger("Alerted");


			}

			// Change enemy wander target if target is reachable for any reason
			if (Mathf.Abs (Vector3.Distance (originMarker.transform.position, v3_startPosition)) < 3f) {	// push marker away from the wander radius

				MoveTarget ();

			}

			// Change enemy wander target if target is reachable for any reason
			if(Mathf.Abs (Vector3.Distance (originMarker.transform.position, transform.position)) < 2f){	// push marker away from the dust bunny

				MoveTarget ();

			}


			#region EDGE DETECTION

			// Too high fall detection
			RaycastHit hit;
			Ray ledgeRay = new Ray(transform.position + transform.forward * 3 + Vector3.up, -Vector3.up * 999999);
			Ray ledgeRay2 = new Ray(transform.position + (transform.forward * 2.25f) + (transform.right * 0.75f) + Vector3.up, -Vector3.up * 99999);	// lil to the right
			Ray ledgeRay3 = new Ray(transform.position + (transform.forward * 2.25f) + (-transform.right * 0.75f) + Vector3.up, -Vector3.up * 99999);	// lil to the left

			Debug.DrawRay(ledgeRay.origin, ledgeRay.direction, Color.red);
			Debug.DrawRay(ledgeRay2.origin, ledgeRay2.direction, Color.red);
			Debug.DrawRay(ledgeRay3.origin, ledgeRay3.direction, Color.red);

			if (Physics.Raycast (ledgeRay, out hit)) {
				if (hit.distance > 2.5f && !hit.collider.isTrigger) {
					if(b_moving) {
						b_moving = false;
						MoveTarget ();
					}
				} 
			}
			if (Physics.Raycast (ledgeRay2, out hit)) {
				if (hit.distance > 2.5f & !hit.collider.isTrigger) {
					if(b_moving) {
						b_moving = false;
						MoveTarget ();
					}
				} 
			}
			if (Physics.Raycast (ledgeRay3, out hit)) {
				if (hit.distance > 2.5f && !hit.collider.isTrigger) {
					if(b_moving) {
						b_moving = false;
						MoveTarget ();
					}
				} 
			}


			// Check for water (death zone)
			for(float i = 1.0f; i < 10.0f; i += 1.0f){

				Ray waterRay = new Ray(transform.position + transform.forward * (i/4.0f) + Vector3.up, -Vector3.up * 999999);

				Debug.DrawRay(waterRay.origin, waterRay.direction, Color.blue);

				if (Physics.Raycast (waterRay, out hit)) {
					if (hit.transform.tag == "Water") {
						MoveTarget ();
						break;
					} 
				}
			}

			#endregion

			#region SLOW TO STOP - MOVEMENT
			// Not moving?  Stop moving.
			if (!b_moving) {

				RotateTowardTarget(originMarker.transform.position, 0.1f);

			}
			else {

				RotateTowardTarget(originMarker.transform.position, 0.1f);

				transform.position += transform.forward * wanderMoveSpeed * Time.deltaTime;

			}
			#endregion

			CorrectAngle();

			#region TIMERS
			// Timer toggle for enemy movement
			f_movingTimer -= Time.deltaTime;
			if (f_movingTimer <= 0) {

				f_movingTimer = Random.Range (5, 9);

				b_moving = !b_moving;

			}

			#endregion


			// Origin Marker Positioning Constant
			Vector3 newOriginMarkerPos = originMarker.transform.position;
			newOriginMarkerPos.y = transform.position.y + 1;
			originMarker.transform.position = newOriginMarkerPos;


			break;
			#endregion

			#region ATTACK
		case "attack":

			animator.SetTrigger("Grr");

			attackCooldown -= Time.deltaTime;

			if(attackCooldown <= 0){
				//animator.Play("smallenemy_attack");
				animator.SetBool("Attacking", true);
				animator.SetBool("Moving", false);
				animator.SetBool("Chasing", false);
			}

			AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
			float animCurrentTime = 0;
			if(currentState.IsName("DustBunny_Attack")){

				float attackAnimLength = currentState.length;
				animCurrentTime = currentState.normalizedTime % 1;

			}

			if(animCurrentTime > 0.9f) {

				attackCooldown = 1.5f;

				animator.SetBool("Attacking", false);
				animator.SetBool("Moving", true);
				animator.SetBool("Chasing", true);

				HitPlayer();

			}




			// Face the player without tilting
			transform.LookAt(new Vector3(playerObj.transform.position.x, transform.position.y, playerObj.transform.position.z));




			if (!b_foundPlayer) { 

				stateScript.STATE = "wander";

			}

			if(enemyDistance >= f_distanceToAttack){

				animator.SetBool("Attacking", false);
				animator.SetBool("Moving", true);
				animator.SetBool("Chasing", true);

				stateScript.STATE = "chase";

			}


			break;
			#endregion

		#region CHASE
		case "chase":

			b_moving = true;

			// Set gaze to player
			originMarker.transform.position = playerObj.transform.position;

			//animator.Play("smallenemy_run");
			animator.SetBool("Attacking", false);
			animator.SetBool("Moving", true);
			animator.SetBool("Chasing", true);

			#region CHASE SPECIFICS
			if(enemyDistance > 2)
				animator.ResetTrigger("Grr");

			// Distance checks
			if (enemyDistance > f_distanceToLoseTarget || 
				Vector3.Distance (transform.position, v3_startPosition) > f_maxWanderRadius * 3) {

				stateScript.STATE = "wander";

				b_foundPlayer = false;
				b_isJumping = false;

				f_playerDetectionTimeout = 2.0f;
			}

			// Must be after SetDestination
			if (enemyDistance < f_distanceToAttack) {

				stateScript.STATE = "attack";

			}
			else {

				animator.SetBool("Attacking", false);

			}
			#endregion


			// Jumping (?)
			if (!b_isJumping && f_playerDetectionTimeout <= 0) {

				Ray wallRay = new Ray (transform.position + (Vector3.up * 0.5f), transform.forward * 1);
				Ray newHeightRay = new Ray (transform.position + (transform.forward * 1.5f) + (Vector3.up * 8), -transform.up * 99999);
				RaycastHit hit2;

				Debug.DrawRay (wallRay.origin, wallRay.direction, Color.red);
				Debug.DrawRay (newHeightRay.origin, newHeightRay.direction, Color.red);

				if (Physics.Raycast (wallRay, out hit2) && enemyDistance > 2) {
					if (hit2.distance < 1 && !hit2.collider.isTrigger) {

						if (Physics.Raycast (newHeightRay, out hit2)) {
							if (hit2.distance < 8 && !hit2.collider.isTrigger && hit2.collider.tag != "Player") {

								newPosJ = hit2.point;
								b_isJumping = true;

							} else {
								
								stateScript.STATE = "wander";

							}
						}

					} 
				}
			} else {

				if (f_chaseCounter > 0) {

					f_chaseCounter -= Time.deltaTime;

					transform.position = Vector3.MoveTowards (
						transform.position, 
						new Vector3(playerObj.transform.position.x, newPosJ.y, playerObj.transform.position.z), 
						0.1f);
					
				} else{
					
					f_chaseCounter = 1.0f;

					b_isJumping = false;

				}

			}


			// If you're jumping, you cannot move.  Constantly set due to timers that can reset it.
			if(b_isJumping)
				b_moving = false;



			#region SLOW TO STOP - MOVEMENT
			// Not moving?  Stop moving.
			if (!b_moving) {

				RotateTowardTarget(originMarker.transform.position, 0.2f);

			}
			else {

				RotateTowardTarget(originMarker.transform.position, 0.2f);

				transform.position += transform.forward * wanderMoveSpeed * 3 * Time.deltaTime;

			}
			#endregion


			CorrectAngle();

			break;
			#endregion

			#region DEAD
		case "dead":

			// constantly force the enemy down so that it bounces comically
			rb.AddForce(Vector3.down * forceDownValue, ForceMode.Acceleration);

			break;
			#endregion



		}

		animator.SetBool ("Moving", b_moving);

	}

	void CorrectAngle(){

		//Correct angle - no tilting
		newRotation = transform.rotation.eulerAngles;
		newRotation.x = 0;
		transform.rotation = Quaternion.Euler(newRotation);

	}

	// Rotate toward the target
	void RotateTowardTarget(Vector3 targetPos, float speed){

		Vector3 targetDir = targetPos - transform.position;
		Vector3 newDir = Vector3.RotateTowards (transform.forward, targetDir, speed, 0);
		transform.rotation = Quaternion.LookRotation (newDir);

	}

	public void FoundPlayer(GameObject player){

		playerObj = player;
		Transform newT = playerObj.transform;

		b_moving = true;
		b_foundPlayer = true;	// Yes
		f_detectPlayerSurprise = 0.5f;

	}

	float attackCooldown = 1.5f;

	void HitPlayer()
	{
		if (PUSHABLE)
		{
			Vector3 pushDir = playerObj.transform.position - transform.position;
			pushDir.y = 0; pushDir.Normalize();
			if(HealthManager.instance)
				HealthManager.instance.LoseALifeAndPushAway(pushDir, 10);
		}
	}

	bool firedEvent = false;

	IEnumerator KillEnemy(float time){

		if (GetComponent<SphereCollider> ())
			GetComponent<SphereCollider> ().enabled = false;
		
		yield return new WaitForSeconds (0.05f);

		if (!firedEvent) {
			// handle quest events
			if (GetComponent<QuestObject> ()) {	   // do you have the comp. at all?
				if (GetComponent<QuestObject> ().questObjectType == QUESTOBJECTTYPE.basicenemy)	// is it correct?
					GetComponent<QuestObject> ().Defeated ("basic");

				if (GetComponent<QuestObject> ().questObjectType == QUESTOBJECTTYPE.bossenemy)	// is it correct?
					GetComponent<QuestObject> ().Defeated ("boss");
			}

			firedEvent = true;
		}

		stateScript.STATE = "dead";

		animator.StopPlayback();

		animator.SetBool("Dead", true);


		Vector3 direction = this.transform.position - playerObj.transform.position;
		direction.y = 0;
		direction = direction.normalized;

		rb.AddForce ((direction * smackForceF) + (Vector3.up * smackForceU), ForceMode.Impulse);

		float counter = time;

		while (counter > 0) {
			// Do the stuff
			counter -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();
		}

		// Hide the body
		Destroy(transform.parent.transform.gameObject);

	}

	// Restore Health
	public override void HealEnemy(int healAmount){

		if (health + healAmount <= maxHealth)
			health += healAmount;
		else
			health = maxHealth;

	}

	// Remove Health
	public override void DamageEnemy(int damageAmount){

		if (f_invincibilityTime <= 0) {

			f_invincibilityTime = f_invincibilityTimeINIT;

			// Set color to semi-transparent so the player knows the enemy is currently invincible
			Color tempColor = materialRend.material.color;
			tempColor.a = damageOpacity;
			materialRend.material.color = tempColor;

			// Set the enemy to Alert status
			FoundPlayer (playerObj);

			Vector3 pushDir = transform.position - playerObj.transform.position;
			pushDir.y = 0;
			pushDir.Normalize ();
			rb.AddForce (pushDir * 40, ForceMode.Impulse);

			health -= damageAmount;

			DamageCheck ();
		}
	}

	// Flat out kill enemy
	public override void KillEnemy(){

		health -= maxHealth;

		DamageCheck ();

	}

	void DamageCheck(){

		materialRend.material.color = new Color (origRed, origGreen, origBlue);

		// Kill enemy if health drops to zero
		if (health <= 0 && stateScript.STATE != "dead") {

			StartCoroutine (KillEnemy (2.0f));

		} else {
			
			StartCoroutine (DamageFlash(new Color(1.0f,0.0f,0.0f), 0.2f));

		}

	}

	public float damageOpacity;
	public bool isDamaged = false;

	IEnumerator DamageFlash(Color color, float speed){	// 0.0f - 1.0f, with 1.0f being brightest

		isDamaged = true;

		float 
		redTracker = materialRend.material.color.r, 
		greenTracker = materialRend.material.color.g, 
		blueTracker = materialRend.material.color.b;

		// flash to new color
		while (Mathf.Abs(redTracker - color.r) > 0.001f
			|| Mathf.Abs(greenTracker - color.g) > 0.001f
			|| Mathf.Abs(blueTracker - color.b) > 0.001f) {	

			float changeValue = 0;

			if (redTracker != color.r) {
				float newRed = color.r - redTracker;
				changeValue = newRed * speed;
				if (Mathf.Abs (changeValue) < speed)
					changeValue = newRed;
				redTracker += changeValue;
			}
			
			if (greenTracker != color.g) {
				float newGreen = color.g - greenTracker;
				changeValue = newGreen * speed;
				if (Mathf.Abs (changeValue) < speed)
					changeValue = newGreen;
				greenTracker += changeValue;
			} 

			if (blueTracker != color.b) {
				float newBlue = color.b - blueTracker;
				changeValue = newBlue * speed;
				if (Mathf.Abs (changeValue) < speed)
					changeValue = newBlue;
				blueTracker += changeValue;
			} 

			materialRend.material.color = new Color (redTracker, greenTracker, blueTracker, 1.0f);

			yield return new WaitForEndOfFrame ();

		}
			
		// make sure we reach the exact red we want
		materialRend.material.color = color;

		// flash to original color
		while (Mathf.Abs(redTracker - origRed) > 0.001f
			|| Mathf.Abs(greenTracker - origGreen) > 0.001f
			|| Mathf.Abs(blueTracker - origBlue) > 0.001f) {

			float changeValue = 0;

			if (redTracker != origRed) {
				float newRed = origRed - redTracker;
				changeValue = newRed * speed;
				if (Mathf.Abs (changeValue) < speed / 10)
					changeValue = newRed;
				redTracker += changeValue;
			}

			if (greenTracker != origGreen) {
				float newGreen = origGreen - greenTracker;
				changeValue = newGreen * speed;
				if (Mathf.Abs (changeValue) < speed)
					changeValue = newGreen;
				greenTracker += changeValue;
			} 

			if (blueTracker != origBlue) {
				float newBlue = origBlue - blueTracker;
				changeValue = newBlue * speed;
				if (Mathf.Abs (changeValue) < speed)
					changeValue = newBlue;
				blueTracker += changeValue;
			} 

			materialRend.material.color = new Color (redTracker, greenTracker, blueTracker, damageOpacity);

			yield return new WaitForEndOfFrame ();

		}

		// make sure we reach the original color
		materialRend.material.color = new Color (origRed, origGreen, origBlue, damageOpacity);

		isDamaged = false;

	}

	public float smackForceF = 20;
	public float smackForceU = 8;

	void OnTriggerEnter(Collider col){

		// Hurt player when enemies get touched
		if (col.transform.tag == "Player") {
			HitPlayer ();
		}

		// Hurt player when enemies get touched
		if (col.transform.tag == "Water") {
			KillEnemy ();
		}

	}
}
