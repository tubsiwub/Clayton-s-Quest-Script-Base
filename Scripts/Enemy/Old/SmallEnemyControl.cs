// Tristan

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Placement:  _name_ enemy type, parent body 

// Purpose:  Controls enemy movement and behavior as governed by current states

public class SmallEnemyControl : Enemy { 

	public bool PUSHABLE = false;
	 
	public GameObject originMarker;
	public GameObject radiusMarker;

	Enemy_States stateScript;

	NavMeshAgent navAgent;
	NavMeshAgentScript navAgentScript;
	Rigidbody rb;
	Animator animator;

	GameObject
	ledgeCheck,
	playerObj;

	// positioning
	float 
	f_maxWanderRadius = 15,
	f_distanceToAttack = 1.2f,
	f_distanceToLoseTarget = 14,
	f_detectionRange = 7;

	// speeds
	float
	wanderMoveSpeed = 2,
	wanderRotateSpeed = 140,
	chaseMoveSpeed = 4,
	chaseRotateSpeed = 270;

	// Timers
	float
	f_movingTimer = 1,
	f_stuckTimer = 5.0f,
	f_playerDetectionTimeout = 0,
	f_detectPlayerSurprise = 0;

	Vector3 
	v3_startPosition;

	bool
	b_moving = false,
	b_ledgeCollision = false,
	b_foundPlayer = false;

	public int maxHealth = 25; 

	void Start () {

		// Set parent values
		health = maxHealth;

		// Script-Specific Components
		stateScript = GetComponent<Enemy_States> ();

		// Components
		navAgent = GetComponent<NavMeshAgent> ();
		navAgentScript = GetComponent<NavMeshAgentScript> ();
		rb = GetComponent<Rigidbody> ();
		animator = GetComponent<Animator> ();

		// GameObjects
		foreach (Transform child in this.transform)
			if (child.name == "LedgeCheck")
				ledgeCheck = child.gameObject;
		playerObj = GameObject.FindWithTag ("Player");
		
		// initialization
		v3_startPosition = radiusMarker.transform.position;
		f_maxWanderRadius = radiusMarker.transform.localScale.x / 2;
		radiusMarker.SetActive (false);
	}

	void MoveTarget(){

		b_moving = true;
		//animator.Play("smallenemy_walk");

		originMarker.transform.position = new Vector3(
			Random.Range(v3_startPosition.x - f_maxWanderRadius*2, v3_startPosition.x + f_maxWanderRadius*2),
			v3_startPosition.y,
			Random.Range(v3_startPosition.z - f_maxWanderRadius*2, v3_startPosition.z + f_maxWanderRadius*2));

	}

	void MoveTarget(Vector3 position){

		originMarker.transform.position = position;

		if (Vector3.Distance (v3_startPosition, position) > f_maxWanderRadius * 2) {
			
			stateScript.STATE = "wander";

			MoveTarget ();
		}

	}

	void Update () {


		float enemyDistance = Vector3.Distance (transform.position, playerObj.transform.position);

		animator.SetFloat ("Distance", enemyDistance);


		f_playerDetectionTimeout -= Time.deltaTime;

		// If you've fallen a bit below the defined 'ground'; destroy yourself
		if (transform.position.y < radiusMarker.transform.position.y - 600) {
			navAgent.enabled = false;
			navAgentScript.enabled = false;
			Destroy (this.gameObject);
		}

		// View angle - draws (and calc	ulates) the angle at which the enemy can see the player (looks for player)
		if(!b_foundPlayer && f_playerDetectionTimeout <= 0 && stateScript.STATE != "dead"){
			
			for (int i = -30; i < 31; i++) {
				
				Vector3 degreeDiff = Quaternion.AngleAxis (i, transform.up) * transform.forward;
				b_foundPlayer = false;

				Ray ray = new Ray(transform.position, degreeDiff * f_detectionRange);
				RaycastHit hit;

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
									en.GetComponent<SmallEnemyControl> ().FoundPlayer (hit.transform.gameObject);
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


		// Overall movement decision check
		switch (stateScript.STATE.ToLower()) {

		#region WANDER
		case "wander":

			animator.SetBool("Attacking", false);
			animator.SetBool("Moving", true);
			animator.SetBool("Chasing", false);


			// If the enemy has seen and is in the first stages of noticing the player,
			//		play a special animation and pause movement for now
			if(f_detectPlayerSurprise <= 0){

				navAgent.enabled = true;
				navAgentScript.enabled = true;

				if (b_foundPlayer) { 
					
					stateScript.STATE = "chase";

					//animator.Play("smallenemy_run");

					navAgent.speed = chaseMoveSpeed;
					navAgent.angularSpeed = chaseRotateSpeed;
				}

			}
			else {
				
				f_detectPlayerSurprise -= Time.deltaTime;

				navAgent.enabled = false;
				navAgentScript.enabled = false;

				//animator.Play("smallenemy_noticeplayer");
				animator.SetTrigger("Alerted");


			}

			// Change enemy wander target if target is reachable for any reason
			if (Mathf.Abs (Vector3.Distance (originMarker.transform.position, v3_startPosition)) < 3) {

				if (f_stuckTimer > 0) {

					MoveTarget ();

				}
			}

			// Change enemy wander target if target is reachable for any reason
			if(Mathf.Abs (Vector3.Distance (originMarker.transform.position, transform.position)) < 1){

				// If you get close, you aren't stuck
				f_stuckTimer = 5.0f;

				MoveTarget ();

				b_moving = !b_moving;

				//if(!b_moving)
					//animator.Play("smallenemy_idle");

			}

			#region EDGE DETECTION
			RaycastHit hit;

			float rayLength = 50;

			if (Physics.Raycast (ledgeCheck.transform.position, Vector3.down, out hit, rayLength)) {

				if(!b_ledgeCollision){
					if (hit.distance > 5 || hit.transform.tag == "JumpZone" && !hit.collider.isTrigger) {

						if(b_moving)
							MoveTarget ();

					} 
				}
			}
			#endregion

			#region MOVEMENT

			// position away from starting point
//			f_currentWanderPosition = Vector3.Distance (transform.position, v3_startPosition);

			#endregion

			#region SLOW TO STOP - MOVEMENT
			// Not moving?  Stop moving.
			if (!b_moving) {

				//animator.Play("smallenemy_idle");

				navAgent.enabled = false;
				navAgentScript.enabled = false;
			}
			#endregion

			#region TIMERS
			// Timer toggle for enemy movement
			f_movingTimer -= Time.deltaTime;
			if (f_movingTimer <= 0) {

				f_movingTimer = Random.Range (3, 6);

				b_moving = !b_moving;

				if(!b_moving){
					//animator.Play("smallenemy_idle");
					navAgent.enabled = false;
					navAgentScript.enabled = false;
				}
				else {
					//animator.Play("smallenemy_walk");
					navAgent.enabled = true;
					navAgentScript.enabled = true;
				}

			}

			// Timer in case enemy gets stuck
			if(b_moving) f_stuckTimer -= Time.deltaTime;
			if (f_stuckTimer <= 0) {

				originMarker.transform.position = v3_startPosition;

			}

			#endregion

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
//			else {
//				animator.SetTrigger("Alerted");
//			}




			// Face the player without tilting
			transform.LookAt(new Vector3(playerObj.transform.position.x, transform.position.y, playerObj.transform.position.z));




			if (!b_foundPlayer) { 

				stateScript.STATE = "wander";

				navAgent.enabled = true;
				navAgentScript.enabled = true;

				navAgent.speed = wanderMoveSpeed;
				navAgent.angularSpeed = wanderRotateSpeed;
			}

			if(enemyDistance >= f_distanceToAttack){

				animator.SetBool("Attacking", false);
				animator.SetBool("Moving", true);
				animator.SetBool("Chasing", true);

				stateScript.STATE = "chase";

				navAgent.enabled = true;
				navAgentScript.enabled = true;

				navAgent.speed = chaseMoveSpeed;
				navAgent.angularSpeed = chaseRotateSpeed;
			}
			

		break;
		#endregion

		#region CHASE
		case "chase":

			b_moving = true;
			//animator.Play("smallenemy_run");
			animator.SetBool("Attacking", false);
			animator.SetBool("Moving", true);
			animator.SetBool("Chasing", true);

			if(navAgent.pathStatus == NavMeshPathStatus.PathPartial || navAgent.pathStatus == NavMeshPathStatus.PathInvalid){
				stateScript.STATE = "wander";

				//animator.Play("smallenemy_walk");

				navAgent.enabled = true;
				navAgentScript.enabled = true;

				navAgent.speed = wanderMoveSpeed;
				navAgent.angularSpeed = wanderRotateSpeed;

				navAgent.SetDestination (originMarker.transform.position);

				b_foundPlayer = false;

				f_playerDetectionTimeout = 2.0f;
			}

			if (enemyDistance > f_distanceToLoseTarget || 
				Vector3.Distance (transform.position, v3_startPosition) > f_maxWanderRadius * 3) {

				stateScript.STATE = "wander";

				//animator.Play("smallenemy_walk");

				navAgent.enabled = true;
				navAgentScript.enabled = true;

				navAgent.speed = wanderMoveSpeed;
				navAgent.angularSpeed = wanderRotateSpeed;

				navAgent.SetDestination (originMarker.transform.position);

				b_foundPlayer = false;

				f_playerDetectionTimeout = 2.0f;
			}

			// Must be after SetDestination
			if (enemyDistance < f_distanceToAttack) {

				stateScript.STATE = "attack";

				navAgent.enabled = false;
				navAgentScript.enabled = false;
			}
			else {
				
				animator.SetBool("Attacking", false);

			}

			break;
			#endregion

		#region DEAD
		case "dead":

			break;
			#endregion



		}

		animator.SetBool ("Moving", b_moving);

	}


	public void FoundPlayer(GameObject player){

		playerObj = player;
		Transform[] newTList = new Transform[1];
		newTList [0] = playerObj.transform;
		GetComponent<NavMeshAgentScript> ().SetTarget = newTList;

		navAgent.enabled = true;
		navAgentScript.enabled = true;
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

	IEnumerator KillEnemy(float time, Vector3 direction){

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

		navAgent.enabled = false;
		navAgentScript.enabled = false;
		rb.constraints = RigidbodyConstraints.None;

		float counter = time;

		while (counter > 0) {
			// Do the stuff
			counter -= Time.deltaTime;

			rb.AddForce (direction * 2600, ForceMode.Impulse);

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

		health -= damageAmount;

		DamageCheck();

	}

	// Flat out kill enemy
	public override void KillEnemy(){

		health -= maxHealth;

		DamageCheck ();

	}

	void DamageCheck(){

		// Kill enemy if health drops to zero
		if (health <= 0 && stateScript.STATE != "dead") {

			Vector3 direction = this.transform.position - playerObj.transform.position;
			direction.y = 0;
			direction = direction.normalized;

			rb.AddForce (direction * 1000 + Vector3.up * 1400, ForceMode.Impulse);

			StartCoroutine(KillEnemy(2.0f, direction));

		}

	}

	public void ENABLE_NAVAGENT(){

		navAgent.enabled = true;
		navAgentScript.enabled = true;

	}

	public void LedgeCheck_Collision(bool value){

		b_ledgeCollision = value;

	}

	void LateUpdate()
	{
//		// d'aw, you fall off?
//		if (!navAgent.isOnNavMesh) {
//			health -= maxHealth;
//		}
	}

	void OnTriggerEnter(Collider col){

		// Hurt player when enemies get touched
		if (col.transform.tag == "Player") {
			HitPlayer ();
		}

	}
}
