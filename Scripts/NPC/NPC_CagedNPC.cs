using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_CagedNPC : MonoBehaviour {

	public enum CagedStatus{
		Trapped, Free
	}

	[SerializeField]
	CagedStatus cagedStatus;
	CagedStatus oldCageStatus;

	public enum EmotionalState{
		Angry, Sad, Neutral
	}

	[SerializeField]
	EmotionalState emotionalState;
	EmotionalState oldEmotionalState;

	Transform playerObj;

	// Specific cage the NPC resides in
	public CageExplode cageExplodeScript;

	Animator anim;
	public Animator faceAnim;
	Animator friendFreed;

	//counters
	public float angerCounter = 0;
	public float sadnessCounter = 0;

	bool npcSeePlayer = false;
	bool npcLosePlayer = false;

	void Start () {

		// events
		cageExplodeScript.OnCageExplode += ZoomAway;

		playerObj = GameObject.FindWithTag ("Player").transform;
		friendFreed = GameObject.Find ("FriendFreed").GetComponent<Animator>();

		anim = GetComponent<Animator> ();

		Init ();

	}

	void Init(){
		
		if (cagedStatus == CagedStatus.Trapped) {

		}

		if (cagedStatus == CagedStatus.Free) {

		}

		if (emotionalState == EmotionalState.Neutral) {
			anim.SetBool ("Angry", false);
			anim.SetBool ("Sad", false);
			faceAnim.SetTrigger ("Frown");
		}

		if (emotionalState == EmotionalState.Angry) {
			anim.SetBool ("Angry", true);
			anim.SetBool ("Sad", false);
			faceAnim.SetTrigger ("Angry");
		}

		if (emotionalState == EmotionalState.Sad) {
			anim.SetBool ("Angry", false);
			anim.SetBool ("Sad", true);
			faceAnim.SetTrigger ("Distraught");
		}

	}

	void Update () 
	{
		float distanceToPlayer = Vector3.Distance (playerObj.position, this.transform.position);

		if (cagedStatus == CagedStatus.Trapped) 
		{
			if (distanceToPlayer < 3) 
			{
				npcSeePlayer = true;

				// The moment you walk into view
				if (npcSeePlayer == npcLosePlayer) {

					emotionalState = EmotionalState.Neutral;
					anim.SetBool ("Thrilled", true);
					anim.SetBool ("Angry", false);
					anim.SetBool ("Sad", false);
					faceAnim.SetTrigger ("Overjoyed");

					angerCounter = 0;
					sadnessCounter = 0;
				}

				npcLosePlayer = false;

				// Follow the player's direction
				transform.LookAt (playerObj.position);
				Vector3 rotation = transform.rotation.eulerAngles;
				rotation.x = 0;
				transform.rotation = Quaternion.Euler (rotation);
			} 
			else 
			{
				npcSeePlayer = false;

				// The moment you walk away
				if (npcSeePlayer == npcLosePlayer) {

					emotionalState = EmotionalState.Sad;
					anim.SetBool ("Thrilled", false);
					anim.SetBool ("Angry", false);
					anim.SetBool ("Sad", true);
					faceAnim.SetTrigger ("Distraught");

					angerCounter = 0;
					sadnessCounter = 0;
				}

				npcLosePlayer = true;
			}
		}

		DetectStatusChange ();

		Behaviors ();

		Counters ();
	}

	bool zoomFinish = false;
	float zoomXScale = 0.1f;
	float zoomYScale = 5.0f;
	float zoomZScale = 0.1f;

	void ZoomAway(){

		// clean up event
		cageExplodeScript.OnCageExplode -= ZoomAway;

		StartCoroutine (ZoomAway_e ());
	}

	// Vanish NPC - add to town
	IEnumerator ZoomAway_e()
	{
		while (!zoomFinish)
		{
			if (transform.localScale.x > zoomXScale)
			{
				Vector3 tempScale = transform.localScale;
				tempScale.x -= 0.1f;
				transform.localScale = tempScale;
			}

			if (transform.localScale.y < zoomYScale)
			{
				Vector3 tempScale = transform.localScale;
				tempScale.y += 0.4f;
				transform.localScale = tempScale;
			}
			else zoomFinish = true;

			if (transform.localScale.z > zoomZScale)
			{
				Vector3 tempScale = transform.localScale;
				tempScale.z -= 0.1f;
				transform.localScale = tempScale;
			}

			yield return new WaitForEndOfFrame ();
		}

		// Andrew here. Lets wait a little while before starting the HUD animation.
		yield return new WaitForSeconds(0.75f);

		friendFreed.SetTrigger ("START");

		// freeze the player for this animation, we unfreeze when it's done
		// (in TransitionAfterFriendFreed)
		playerObj.GetComponent<PlayerHandler>().SetFrozen(true, true);

		// Add to town and NPC count

		Destroy (this.transform.parent.gameObject);
	}

	void DetectStatusChange()
	{
		if (oldCageStatus != cagedStatus) {
			// changes
		}
		oldCageStatus = cagedStatus;

		if (oldEmotionalState != emotionalState) {

			if (emotionalState == EmotionalState.Neutral) {
				anim.SetBool ("Angry", false);
				anim.SetBool ("Sad", false);
				faceAnim.SetTrigger ("Frown");
			}

			if (emotionalState == EmotionalState.Angry) {
				anim.SetBool ("Angry", true);
				anim.SetBool ("Sad", false);
				faceAnim.SetTrigger ("Angry");
			}

			if (emotionalState == EmotionalState.Sad) {
				anim.SetBool ("Angry", false);
				anim.SetBool ("Sad", true);
				faceAnim.SetTrigger ("Distraught");
			}

		}
		oldEmotionalState = emotionalState;

	}

	void Behaviors(){

		if (cagedStatus == CagedStatus.Trapped) {

		}

		if (cagedStatus == CagedStatus.Free) {

		}

		if (emotionalState == EmotionalState.Neutral) {

		}

		if (emotionalState == EmotionalState.Angry) {

		}

		if (emotionalState == EmotionalState.Sad) {

		}

	}

	void Counters(){
		
		if(sadnessCounter < 50)
			angerCounter += Random.Range (0, 3) * Time.deltaTime;

		if(angerCounter < 50)
			sadnessCounter += Random.Range (0, 3) * Time.deltaTime;

		if (sadnessCounter >= 100) {
			emotionalState = EmotionalState.Sad;
			sadnessCounter = 0;
			angerCounter = 0;
		} else if (angerCounter >= 100) {
			emotionalState = EmotionalState.Angry;
			sadnessCounter = 0;
			angerCounter = 0;
		} else if( angerCounter > 50 || sadnessCounter > 50) {
			emotionalState = EmotionalState.Neutral;
		}
	}
}
