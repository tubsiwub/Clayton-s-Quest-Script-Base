using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

// TEST

// =======  ============    ======  ==== ===  ======   ============ ====== ============  ===
// =========================================================================================
// =     MAKE SURE TO CHECK InteractPopup_DistanceCheck TO MAKE SURE NOTHING CONFLICTS     =
// =========================================================================================
// ===== ========   ========== ======= = ========= = = ==============  ======= = == ==== = =

// ENUM


[Serializable]
public enum NPCTYPE{

	NORMAL, QUEST

}

// ENUM
[Serializable]
public enum BEHAVIORS
{
	WANDER,
	IDLE,
	SIT,
	TALK,
	STOP
}

public class NPC_Behavior : MonoBehaviour {

	public NPCTYPE npcType;

	public TRAVELAREA travelArea = TRAVELAREA.INSIDE;
	public TRAVELAREA insideArea = TRAVELAREA.INSIDE;

	public GameObject faceObj;

	// DECLARATIONS
	Animator animator;
	Animator faceAnimator;

	float rotSpeed = 60f;
	float defaultSpeed;

	float sightDistance = 3;

	[SerializeField] Transform target;
	public Transform destination;

	public string NPCName;

	// NavMesh
	NavMeshAgent agent;

	Vector3[] vertices;
	int[] triangles; 

	Mesh mesh;

	Vector3 specificLoc;

	bool waveHello = false;
	float waveCooldownTimer = 5.0f;		// time until NPC can wave again
	float lostWaveTargetTimer = 2.0f;	// time until NPC gives up on waving

	Transform playerObj;

	bool NPCLock = false;

	public BEHAVIORS currentBehavior = BEHAVIORS.WANDER;
	BEHAVIORS lastBehavior = BEHAVIORS.WANDER;

	// Fun idea:  NPC should rarely, randomly, follow the player around.

	// Motivation Timers
	[SerializeField] [Tooltip ("When 0, NPC will crave being outside.")] [Range (-5, 105)] 
	public float yearnOUTSIDE = 100;		// While inside, NPC begins to wish to be outside	-	when 0, NPC targets non-buildings
	[SerializeField] [Tooltip ("When 0, NPC will crave being inside.")] [Range (-5, 105)] 
	public float yearnINSIDE = 100;			// While outside, NPC begins to wish to see his house	-	when 0, NPC targets buildings
	//	[SerializeField] [Tooltip ("When 0, NPC will crave attention and seek out NPCs.")] [Range (-5, 105)] 
	//	float yearnCONTACT = 100;		// After a while, NPC wishes to have contact with friends	-	when 0, NPC targets other NPCs
	[SerializeField] [Tooltip ("When 0, NPC will try to sit down somewhere.")] [Range (-5, 105)] 
	public float yearnREST = 100;			// NPC can get tired from walking too much	-	when 0, "too tired" activates
	[SerializeField] [Tooltip ("When 0, NPC will idle about.")] [Range (-5, 105)] 
	public float boredomTimer = 100;		// NPC can get bored from repetitive tasks and will begin to slack off for a while when this meter runs low.

	public float disinterestSpeed = 3.0f;	// How fast the NPC grows bored.
	public float exhaustionSpeed = 2.5f;	// How fast the NPC grows tired.
	public float hysteriaSpeed = 4.0f;		// How fast the NPC get irritated by surroundings.
	public float restSpeed = 5.0f;			// How fast the NPC gets rested from sitting on the ground / bench (with bench being 2x the rate).

	public float boredomBonus = 140.0f;	// Bonus time added when wave / talk action occurs for NPC.
	public float boredomAttack = 5.0f;	// Chunk lost when a NavPoint is found / new target is set.

	[SerializeField] float satisfiedTimer = 25.0f;		// Mini-timer until NPC is allowed to get sick of current location (SECONDS)
	public float satisfiedTimerDefault = 25.0f;				// Match the timer above to this as a standard.
	public bool satisfiedWithLocation = false;
	public bool sitting = false;
	public bool idling = false;

	float faceShiftCooldown = 2.0f;		// Time until a face shift may occur again

	public GameObject lookAtObj;

	bool inDialogue = false;

	void Start () {

		Initialize ();


	}

	void Initialize(){
		
		animator = GetComponent<Animator> ();
		faceAnimator = faceObj.GetComponent<Animator> ();
		playerObj = GameObject.FindWithTag ("Player").transform;

		if (npcType == NPCTYPE.QUEST) {

			animator.SetBool ("Moving", false);

		} else {
			
			agent = GetComponent<NavMeshAgent> ();
			mesh = new Mesh ();

			// assign mesh information
			vertices = NavMesh.CalculateTriangulation ().vertices;
			triangles = NavMesh.CalculateTriangulation ().indices;

			// assign mesh info to the stored mesh
			mesh.vertices = vertices;
			mesh.triangles = triangles;

			defaultSpeed = agent.speed;

		}
	}

	void Update () {

		Behavior ();

		if (npcType != NPCTYPE.QUEST) {
			
			MotivationTimer ();

		} else {

			if (!inDialogue) {
				
				// Look at the right spot
				transform.LookAt (lookAtObj.transform.position);
				transform.rotation = Quaternion.Euler (new Vector3 (0, transform.rotation.eulerAngles.y, 0));

			} else {

				// Look at the player
				transform.LookAt (playerObj.position);
				transform.rotation = Quaternion.Euler (new Vector3 (0, transform.rotation.eulerAngles.y, 0));

			}

		}



	}

	public void ChangeDialogueStatus(bool status){
		inDialogue = status;
	}

	// Motivation Timer - Built and Designed to change NPC motivation based on several factors.
	//	-	How long since contact with fellow NPCs?
	//	-	How long since NPC has been inside of a building?
	//	-	How long since NPC has been outside?
	//	-	How long since the NPC has had rest?	(Bench, Sitting on Ground)
	void MotivationTimer(){


		#region DETERMINE WHERE WE ARE / WHICH SECTION OF NAVMESH

		// NPC will wander between regions without care, 
		//	but once an NPC hasn't been somewhere for a length of time he will begin to crave that specific region.

		NavMeshHit navHit;
		RaycastHit rayHit;
		Vector3 hitPoint = Vector3.zero;

		// Send a ray downward to use for NavMesh area checking
		if (Physics.Raycast (transform.position + Vector3.up, Vector3.down, out rayHit, 8)) {
			hitPoint = rayHit.point;
		}

		// Check if that position is on the mesh (and then, specific area)
		NavMesh.SamplePosition (hitPoint, out navHit, 8, NavMesh.AllAreas);

		// navHit.mask == 	64 	// inside
		// navHit.mask == 	32 	// forbidden
		// navHit.mask == 	16 	// TraverseBetween
		// navHit.mask == 	8 	// NPC_Town
		// navHit.mask == 	1 	// walkable

		// Checks which area the Sample lands on.  Makes sure it aligns allowed areas (travel and inside)
		if(navHit.mask != (int)travelArea && navHit.mask != (int)insideArea){
			return;
		}

		// are we outside (in our allowed area(s))
		if(navHit.mask == (int)travelArea && !satisfiedWithLocation){	// if we aren't satisfied, starting getting tired.

			yearnOUTSIDE = 100;

			// Start wanting to go inside while outside
			if(yearnINSIDE > -5)
				yearnINSIDE -= hysteriaSpeed * Time.deltaTime;
		}

		// are we inside a building?
		if(navHit.mask == (int)insideArea && !satisfiedWithLocation){	// ditto above

			yearnINSIDE = 100;

			// Start wanting to go outside while inside
			if(yearnOUTSIDE > -5)
				yearnOUTSIDE -= hysteriaSpeed * Time.deltaTime;
		}

		if(yearnOUTSIDE <= 0 || yearnINSIDE <= 0){

			satisfiedWithLocation = true;
		}

		// If we're satisfied at the moment, that will change soon enough...
		if(satisfiedWithLocation){

			satisfiedTimer -= Time.deltaTime;	// actual timer, in seconds

			if(satisfiedTimer <= 0){

				satisfiedWithLocation = false;
				satisfiedTimer = satisfiedTimerDefault;
			}

		}

		//Debug.Log("YEARN: Inside - " + yearnINSIDE + ", Outside - " + yearnOUTSIDE);

		#endregion

		#region EXHAUSTION TIMER - NPC GETS TIRED OF WANDERING AROUND AND MUST REST FOR A BIT
		// Check if tired.  If yes, ->
		// Check if a bench is anywhere within _num_ distance, then if yes sit on bench.  If no, sit on ground immediately.
		//	If no, ->
		//	Continue the wear and tear of the NPC.  If idle, the deterioration speed is halved.

		if (yearnREST < 0 && !sitting) {

			// CHANGE:  check for bench is not in yet.

			sitting = true;

			currentBehavior = BEHAVIORS.SIT;
		} 

		if (sitting){

			// CHANGE:  check if on bench / ground and add specific speed bonus
			if(yearnREST < 105)
				yearnREST += restSpeed * Time.deltaTime;

			if(yearnREST >= 100){

				currentBehavior = BEHAVIORS.WANDER;

				sitting = false;

			}

		}

		// Get tired from being up and active
		if(yearnREST > 0 && !sitting) {

			if(currentBehavior == BEHAVIORS.WANDER)
			if(yearnREST > -5) yearnREST -= exhaustionSpeed * Time.deltaTime * 1.0f;
			if(currentBehavior == BEHAVIORS.IDLE)
			if(yearnREST > -5) yearnREST -= exhaustionSpeed * Time.deltaTime * 0.5f;	// half rate for idle

		}

		#endregion

		#region BOREDOM TIMER - NPC GETS BORED FROM MONOTONOUS TASKS
		// Idle timer / boredom timer
		// NPC will walk for a bit, but for each target he retrieves he loses a chunk from his remaining boredom timer
		// NPC gradually grows bored from wandering and attempts to slack off / go idle
		// NPC will gain more time / reduce boredom when he waves at someone or talks to someone.

		if (!idling && (currentBehavior != BEHAVIORS.SIT || currentBehavior != BEHAVIORS.TALK)) {
			if(boredomTimer > -5) boredomTimer -= disinterestSpeed * Time.deltaTime;
		} else {
			if(boredomTimer < 105) boredomTimer += disinterestSpeed * Time.deltaTime;
		}

		if (boredomTimer <= 0) {

			idling = true;

			if(currentBehavior == BEHAVIORS.WANDER){

				currentBehavior = BEHAVIORS.IDLE;	

			}


		}
		else if (boredomTimer >= 100) {

			idling = false;

			if(currentBehavior == BEHAVIORS.IDLE){

				currentBehavior = BEHAVIORS.WANDER;

			}

		}
		#endregion

		// expressions:  Smile, Frown, Overjoyed, Angry, Confused, Distraught, Shocked
	}

	// Controller for NPC actions / behaviors based on what they're currently doing.  Controls actions.
	void Behavior(){

		// The moment we stop talking...
		if (lastBehavior != currentBehavior && currentBehavior != BEHAVIORS.TALK) {

			animator.ResetTrigger ("StopTalk");
			animator.SetTrigger ("StopTalk");

		}

		// Store last behavior
		lastBehavior = currentBehavior;

		RaycastHit hit;

		if(faceShiftCooldown > 0) {

			faceShiftCooldown -= Time.deltaTime;

		}

		// Disallow NPCs to be seen by others if they are locked
		foreach (Collider col in GetComponents<Collider>()) {
			if(col.isTrigger)
				col.enabled = !NPCLock;
		}


		// States
		switch (currentBehavior) {

			#region WANDER
		case BEHAVIORS.WANDER:

			NPCLock = false;

			if (faceShiftCooldown <= 0) {

				AnimateFace ("Smile");

			}

			if(agent)
			if (!agent.enabled)
				agent.enabled = true;


			#region Animator Resets

			animator.ResetTrigger("Sit");
			animator.ResetTrigger("Talk1");
			animator.ResetTrigger("Talk2");

			#endregion


			#region Animator Toggles
			if(animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_Talking")
				|| animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_CrossArm_Stance"))
			{
				animator.SetBool("Talking", false);
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Sitting_Ground"))
			{
				animator.SetTrigger("Stand");
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Idle"))
			{
				animator.SetBool("Moving", true);
			}
			#endregion



			#region CHOOSE A NEW LOCATION TO WANDER TO
			// If the NPC gets close to their destination, we move it.
			if (Vector3.Distance (this.transform.position, destination.position) < 2) {

				specificLoc = NavMesh_CalculateSurfaceArea.GetRandomPositionOnMesh(mesh);

				NavMeshHit navHit;
				NavMesh.SamplePosition (specificLoc, out navHit, 1.0f, NavMesh.AllAreas);

				//print("(int)Nav Mask: " + navHit.mask + " at " + navHit.position);

				//Debug.Log ("SamplePos: " + locationCheck + " : " + navHit.mask);

				// NavMask Values
//				HOUSE13 	= 524288,
//				HOUSE12 	= 262144,
//				HOUSE11 	= 131072,
//				HOUSE10 	= 65536,
//				HOUSE9 		= 32768,
//				HOUSE8 		= 16384,
//				HOUSE7 		= 8192,
//				HOUSE6 		= 4096,
//				HOUSE5 		= 2048,
//				HOUSE4 		= 1024,
//				HOUSE3 		= 512,
//				HOUSE2 		= 256,
//				HOUSE1 		= 128,
//				INSIDE 		= 64,
//				FORBIDDEN 	= 32,
//				TRAVERSE 	= 16,
//				NPCTOWN 	= 8,
//				WALKABLE 	= 1

				// Checks which area the Sample lands on.  Makes sure it aligns allowed areas (travel and inside)
				// Also makes sure to
				if(yearnINSIDE > 0 && yearnOUTSIDE > 0){

					// Simple check for allowed areas
					if(navHit.mask != (int)travelArea && navHit.mask != (int)insideArea){
						return;
					}
				}
				else if(yearnINSIDE < 0){

					// Specifically looking for buildings
					if(navHit.mask != (int)insideArea){
						return;
					}
				}
				else{

					// Specifically looking for any non-building area
					if(navHit.mask != (int)travelArea){
						return;
					}
				}

				boredomTimer -= boredomAttack;	// input the penalty AFTER we confirm that a new location has been set

				destination.position = specificLoc;
			}
			#endregion

			#region CHANGE TARGET IF PATH IS EVEN PARTIALLY BLOCKED
			// If the target position becomes completely blocked for one reason or another (at all) change target
			if(agent)
			if(agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid){

				specificLoc = NavMesh_CalculateSurfaceArea.GetRandomPositionOnMesh(mesh);

				NavMeshHit navHit;
				NavMesh.SamplePosition (specificLoc, out navHit, 1.0f, NavMesh.AllAreas);

				if(navHit.mask != (int)travelArea){
					return;
				}

				destination.position = specificLoc;

			}
			#endregion

			// Move to destination object location
			if(agent)
			agent.SetDestination (destination.position);

			// If the NPC has a reason to greet someone, they will rotate and greet that person if they are still nearby
			if (!waveHello && !NPCLock) {

				// This function rotates the NPC toward whoever else they meet, and then they wave hello
				if (Physics.BoxCast(transform.position, Vector3.one, transform.forward, out hit, Quaternion.identity, sightDistance)){
					if (hit.transform.tag == "Player") {

						LockNPC(hit.transform.gameObject, "wave");
						TalkTimerStart(2.0f);
						//StartCoroutine (CheckVision (currentBehavior, "wave"));

						currentBehavior = BEHAVIORS.STOP;

					}

					if (hit.transform.tag == "NPC") {

						string actionChosen = RandomAction();

						float talkDuration = 1.0f;

						if(actionChosen.ToLower() == "talk")
							talkDuration = UnityEngine.Random.Range(8.0f, 16.0f);
						if(actionChosen.ToLower() == "wave")
							talkDuration = 2.0f;

						LockNPC(hit.transform.gameObject, actionChosen);
						hit.transform.gameObject.GetComponent<NPC_Behavior>().LockNPC(this.gameObject, actionChosen);

						TalkTimerStart(talkDuration);
						hit.transform.gameObject.GetComponent<NPC_Behavior>().TalkTimerStart(talkDuration);

					}
				}
			}


			break;
			#endregion

			#region TALK
		case BEHAVIORS.TALK:

			if(agent)
			if (agent.enabled)
				agent.enabled = false;


			#region Animator Resets

			animator.ResetTrigger ("StopTalk");
			animator.ResetTrigger("Sit");
			animator.ResetTrigger("Talk2");

			#endregion


			#region Animator Toggles
			if(animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_NoTalk"))
			{
				//animator.SetBool("Talking", true);
				animator.SetTrigger("Talk1");
			}

			if(animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_CrossArm_Stance"))
			{
				//animator.SetBool("Talking", true);
				animator.SetTrigger("Talk1");
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Walk"))
			{
				animator.SetBool("Moving", false);
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Sitting_Ground"))
			{
				animator.SetTrigger("Stand");
			}
			#endregion



			NPCLock = true;

			if (faceShiftCooldown <= 0) {

				AnimateFace ("Overjoyed");

			}


			if (Vector3.Distance (transform.position, playerObj.transform.position) >= 3 && target == playerObj){
				target = null;
				currentBehavior = BEHAVIORS.WANDER;
			}


			// NPC must have a target
			if (target != null) {
				RotateTowards(target.position);
			}

			break;
			#endregion

			#region SIT
		case BEHAVIORS.SIT:

			if (faceShiftCooldown <= 0) {

				AnimateFace ("Frown");

			}


			if(agent)
			if (agent.enabled)
				agent.enabled = false;


			#region Animator Resets

			animator.ResetTrigger("Stand");
			animator.ResetTrigger("Talk1");
			animator.ResetTrigger("Talk2");

			#endregion


			#region Animator Toggles
			if(animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_Talking")
				|| animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_CrossArm_Stance"))
			{
				animator.SetBool("Talking", false);
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Walk"))
			{
				animator.SetBool("Moving", false);
				animator.SetTrigger("Sit");
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Idle"))
			{
				animator.SetTrigger("Sit");
			}
			#endregion



			// This function rotates the NPC toward whoever else they meet, and then they wave hello
			if (Physics.BoxCast(transform.position, Vector3.one, transform.forward, out hit, Quaternion.identity, sightDistance) ){

				if (hit.transform.tag == "Player") {

					StartCoroutine (WaveAtPerson (currentBehavior));

				}

			}

			break;
			#endregion

			#region STOP
		case BEHAVIORS.STOP:

			if (faceShiftCooldown <= 0) {

				AnimateFace ("Confused");

			}

			// Just... face the target.  It's easier.
			FaceTarget(target.transform.position);

			// Stop movement
			if(agent)
			if (agent.enabled)
				agent.enabled = false;

			#region Animator Resets

			animator.ResetTrigger("Sit");
			animator.ResetTrigger("Talk1");
			animator.ResetTrigger("Talk2");

			#endregion


			#region Animator Toggles
			if(animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_Talking")
				|| animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_CrossArm_Stance"))
			{
				//animator.SetBool("Talking", false);
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Walk"))
			{
				animator.SetBool("Moving", false);
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Sit"))
			{
				animator.SetTrigger("Stand");
			}
			#endregion


			break;
			#endregion

			#region IDLE
		case BEHAVIORS.IDLE:

			if (faceShiftCooldown <= 0) {

				AnimateFace ("Frown");

			}

			// Stop movement
			if(agent)
			if (agent.enabled)
				agent.enabled = false;

			#region Animator Resets

			animator.ResetTrigger("Sit");
			animator.ResetTrigger("Talk1");
			animator.SetTrigger("Talk2");

			#endregion


			#region Animator Toggles
			if(animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_Talking")
				|| animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_UpperBody_CrossArm_Stance"))
			{
				//animator.SetBool("Talking", false);
			}

			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Walk"))
			{
				animator.SetBool("Moving", false);
			}

//			if(animator.GetCurrentAnimatorStateInfo(0).IsName("NPC_Sit"))
//			{
//				animator.SetTrigger("Stand");
//			}
			#endregion


			break;
			#endregion

		}

		//Debug.Log ("Current Behavior is " + currentBehavior.ToString() + " for " + transform.name);

	}

	public void SetTarget(Transform newTarget){
		target = newTarget;
	}

	string RandomAction(){

		int randomAction = UnityEngine.Random.Range(0,100);

		string actionType = "";

		if(randomAction <= 50){
			actionType = "wave";
		} else {
			actionType = "talk";
		}

		return actionType;

	}

	public void LockNPC(GameObject otherNPC, string actionChosen){

		if (!NPCLock && !waveHello) {

			if (actionChosen.ToLower () == "wave") {

				NPCLock = true;

				target = otherNPC.transform;

				FaceTarget (target.position);

				currentBehavior = BEHAVIORS.STOP;

				agent.speed = 0;

				boredomTimer = 200;

				if(animator.GetCurrentAnimatorStateInfo(2).IsName("NPC_NoWave"))
				{
					int randomGesture = UnityEngine.Random.Range (0, 100);

					if (randomGesture < 50) {

						animator.ResetTrigger ("Wave1");
						animator.SetTrigger ("Wave1");

					} else {

						animator.ResetTrigger ("Wave2");
						animator.SetTrigger ("Wave2");

					}
				}
			}

			if (actionChosen.ToLower () == "talk") {

				NPCLock = true;

				target = otherNPC.transform;

				FaceTarget (target.position);

				currentBehavior = BEHAVIORS.TALK;

				agent.speed = 0;

				boredomTimer = 200;

				if(this.animator.GetCurrentAnimatorStateInfo(1).IsName("NPC_NoTalk"))
				{
					int randomGesture = UnityEngine.Random.Range (0, 100);

					if (randomGesture < 50) {
						
						animator.ResetTrigger ("Talk1");
						animator.SetTrigger ("Talk1");

					} else {
						
						animator.ResetTrigger ("Talk2");
						animator.SetTrigger ("Talk2");

					}
				}

			}

		}
	}

	void RotateTowards(Vector3 thisTarget){

		Quaternion q = Quaternion.LookRotation (thisTarget - transform.position);
		transform.rotation = Quaternion.RotateTowards (transform.rotation, q, rotSpeed * Time.deltaTime);

		Vector3 tempRot = transform.eulerAngles;
		tempRot.x = 0;
		transform.rotation = Quaternion.Euler (tempRot);

		AnimateFace ("Confused");

	}

	// Wave is broken because I force the NPCs to instantly face each other and I don't call CheckVision.  Easy fix.
	// Problem is still the no talking issue.

	IEnumerator CheckVision(BEHAVIORS behaviorType, string actionChosen){

		Debug.Log ("CheckVision");

		while (!waveHello) {

			if (actionChosen.ToLower () == "wave") {

				RotateTowards (target.position);

				Ray ray = new Ray (transform.position + new Vector3 (0, 1, 0), transform.forward * (sightDistance + 1));
				RaycastHit directSightHit;

				Debug.DrawRay (transform.position + new Vector3 (0, 1, 0), transform.forward * sightDistance, Color.blue);

				// If directly looking at target...
				if (Physics.Raycast (ray, out directSightHit, sightDistance + 1)) {

					if (directSightHit.transform.tag == target.tag) {

						StartCoroutine (WaveAtPerson (behaviorType));
						lostWaveTargetTimer = 2.0f;

					}
				} 

				// If the wave target is out of range for too long, give up
				lostWaveTargetTimer -= Time.deltaTime;
				if (lostWaveTargetTimer <= 0) {

					lostWaveTargetTimer = 2.0f;
					waveHello = true;

					AnimateFace ("Angry");

					currentBehavior = behaviorType;

					StartCoroutine (WaveCooldown (waveCooldownTimer));
				}

			}

			yield return null;
		}

	}

	void FaceTarget(Vector3 thisTarget){

		// Faces the NPC without messing up their posture
		Quaternion q = Quaternion.LookRotation (new Vector3(
			thisTarget.x - transform.position.x,
			0,
			thisTarget.z - transform.position.z));
		
		transform.rotation = q;

	}

	public void TalkTimerStart(float timer){

		StartCoroutine (TalkTimer (timer));

	}

	IEnumerator TalkTimer(float timer){

		yield return new WaitForSeconds (timer);

		waveHello = true;

		agent.speed = defaultSpeed;

		// S'fine.  Prevents interactions in general.  Named rather poorly due to old rules.
		StartCoroutine (WaveCooldown (waveCooldownTimer));

		currentBehavior = BEHAVIORS.WANDER;

	}

	IEnumerator WaveAtPerson(BEHAVIORS behaviorType){

		if (agent.speed > 0) 
		{
			agent.speed = 0;
		}
		else 
		{

			if(UnityEngine.Random.Range(0,100) > 80)
				animator.SetTrigger("Wave1");
			else
				animator.SetTrigger("Wave2");

			waveHello = true;

			AnimateFace ("Overjoyed");

			if (boredomTimer + boredomBonus <= 200)
				boredomTimer += boredomBonus;	// yay! less bored!
			else
				boredomTimer = 200;
		}

		yield return new WaitForSeconds (2.0f);

		agent.speed = defaultSpeed;

		currentBehavior = behaviorType;

		StartCoroutine (WaveCooldown (waveCooldownTimer));

	}

	IEnumerator WaveCooldown(float time){

		yield return new WaitForSeconds (time);

		waveHello = false;

	}

	void ResetFaceTriggers(){

		faceAnimator.ResetTrigger ("Smile");
		faceAnimator.ResetTrigger ("Frown");
		faceAnimator.ResetTrigger ("Angry");
		faceAnimator.ResetTrigger ("Shocked");
		faceAnimator.ResetTrigger ("Confused");
		faceAnimator.ResetTrigger ("Overjoyed");
		faceAnimator.ResetTrigger ("Distraught");

	}

	void AnimateFace(string faceType){

		faceShiftCooldown = 2.0f;
		ResetFaceTriggers ();
		faceAnimator.SetTrigger(faceType);

	}

}
