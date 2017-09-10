using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Needs a trigger collision box
// Needs two position Transforms
// Needs Platform_TrackDistance script
// Needs a collision box

public enum PLATFORMTYPE {

	BUTTON, AUTO, TIMER, WAIT, COUNT

}

public enum ALPHABET { A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z }

public class Platform_Movement : MonoBehaviour {


	public PLATFORMTYPE startType = PLATFORMTYPE.AUTO;

	public List<Transform> Positions;


	// INTS

	// The position the platform goes to first, moving onward from that point to the next in order
	public int startingPosition;

	int positionCounter = 0;



	// FLOATS

	[Range(0.1f, 3.0f)]
	public float moveSpeed = 1;

	public float pauseTime = 1.0f;

	public float startPauseTime = 1.0f;

	public float waitDelay;



	// BOOLS

	bool reverseBool = false;

	bool moving = false;

	// Inspector Bools
	public bool tutorial;

	// If looping, continue the cycle
	public bool looping = true;

	// Enabled / Disable lerping - elastic movement
	public bool lerping = true;

	// Enable / Disable waiting when platform reaches one of its positional goals
	public bool pausing = true;

	// Enable / Disable path reversing between positions
	public bool reversing = true;

	public bool startOnLoad = false;


	// GAMEOBJECTS

	public GameObject puzzleButton;

	GameObject timerObj;


	public List<GameObject> collidingObjects;
	public List<GameObject> CollidingObjects { get { return collidingObjects; } }

	PlayerHandler.PlayerState playerState;

	IEnumerator coroutine;



	// VECTORS

	Vector3 playerPosition; // world space; bottom of player's feet

	Vector3 worldStartingPosition;	// literally where the platform is in world space; set at start

	float distanceValue;
	//int oldObjectListCount = 0;

	public SavingLoading_StorageKeyCheck saveLoad;

	void Start () {

		distanceValue = transform.localScale.x * 20 * 6;

		// Event Triggers
		if(saveLoad)
			saveLoad.OnKeyCheck += KeyCheck;

		timerObj = GameObject.Find ("TimerUI");

		worldStartingPosition = transform.position;

		if (startType == PLATFORMTYPE.BUTTON) {
			if (puzzleButton) {

				puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated += ForceStart;
				puzzleButton.GetComponent<PuzzleButton> ().OnButtonReset += ForceEnd;

			}
		}

		if (startType == PLATFORMTYPE.TIMER) {
			if (timerObj) {

				timerObj.GetComponent<ActivatedTimer> ().OnTimerStart += ForceStart;
				timerObj.GetComponent<ActivatedTimer> ().OnTimerRunOut += ForceEnd;

			}
		}

		positionCounter = startingPosition;

		collidingObjects = new List<GameObject> ();

		if (startType == PLATFORMTYPE.AUTO)
			ForceStart ();

	}

	void Update(){

		//if (oldObjectListCount > collidingObjects.Count)
		//{
		//	print ("Removed object");
		//}
		//else if (oldObjectListCount < collidingObjects.Count)
		//{
		//	print ("Added object");
		//}
		//oldObjectListCount = collidingObjects.Count;

		foreach (GameObject obj in collidingObjects) {

			if (Vector3.Distance (obj.transform.position, transform.position) > distanceValue) {
				
				StartCoroutine(RemoveObjectFromList(obj));

			}
		}

		if (startType == PLATFORMTYPE.WAIT) {

		}

	}

	// If storageKey marks this as completed, perform these actions
	void KeyCheck(){
		GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
		ForceStart ();	// start regardless of puzzle requirements if storage key is met
	}

	public void ForceStart(){
		
		collidingObjects = new List<GameObject> ();
		StopAllCoroutines ();
		positionCounter = startingPosition;

		StartCoroutine (MoveToward(Positions[positionCounter]));

		moving = true;

	}

	public void ForceStart(int positionIndex){
		
		collidingObjects = new List<GameObject> ();
		StopAllCoroutines ();
		positionCounter = positionIndex;

		StartCoroutine (MoveToward(Positions[positionCounter]));

		moving = true;

	}

	public void ForceEnd(){
		
		StopAllCoroutines ();
		positionCounter = startingPosition;

		StartCoroutine (MoveTowardOnce(Positions[positionCounter]));

		moving = false;

	}

	void OnTriggerEnter(Collider col){

		if (col.gameObject.tag != "Player" && !collidingObjects.Contains(col.gameObject) && !col.isTrigger) {	// no player's for you and no duplicates
			collidingObjects.Add (col.gameObject);
		}

		// Wait for the player to jump onto the platform, then start
		if (startType == PLATFORMTYPE.WAIT && col.gameObject.tag == "Player") {

			// stop the cooldown that resets the platform if player is on platform
			StopCoroutine ("WaitTypeCooldown");

			if(!moving)
				ForceStart ();

		}
		 
	}

	void OnTriggerStay(Collider col){

		if (col.gameObject.tag != "Player" && !collidingObjects.Contains(col.gameObject) && !col.isTrigger) {	// no player's for you and no duplicates
			collidingObjects.Add (col.gameObject);
		}

	}

	void OnTriggerExit(Collider col){

		StartCoroutine(RemoveObjectFromList (col.gameObject));

		if(col.tag == "PushBall")

		if (startType == PLATFORMTYPE.WAIT && col.gameObject.tag == "Player") {

			// start a cooldown to reset the platform
			StartCoroutine("WaitTypeCooldown");

		}
	}

	IEnumerator WaitTypeCooldown(){

		//dsfgsf

		float cooldown = waitDelay;

		while (cooldown > 0) {
			
			cooldown -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		transform.position = worldStartingPosition;

		ForceEnd ();

	}

	IEnumerator RemoveObjectFromList(GameObject obj){

		yield return new WaitForEndOfFrame ();

		collidingObjects.Remove (obj);

	}

	IEnumerator MoveTowardOnce(Transform newPos){

		Vector3 distance = Vector3.zero;

		if (!lerping)
			distance = newPos.position - transform.position;

		while (Vector3.Distance (transform.position, newPos.position) > 0.1f) {

			Vector3 lastPosition = transform.position;

			while (Time.timeScale == 0)
				yield return new WaitForEndOfFrame ();

			if (lerping) {

				transform.position = Vector3.Lerp (transform.position, newPos.position, Time.deltaTime * moveSpeed);

			} else {

				transform.position += (distance / 100) * moveSpeed;

			}

			Vector3 changeInDistance = transform.position - lastPosition;

			foreach (GameObject obj in collidingObjects) {
				obj.transform.position += changeInDistance;
			}

			yield return new WaitForEndOfFrame ();
		}
	}

	IEnumerator MoveToward(Transform newPos){

		Vector3 distance = Vector3.zero;

		if (!lerping)
			distance = newPos.position - transform.position;

		while (Vector3.Distance (transform.position, newPos.position) > 0.1f) {

			while (Time.timeScale == 0)
				yield return new WaitForEndOfFrame ();

			Vector3 lastPosition = transform.position;

			if (lerping) {

				transform.position = Vector3.Lerp (transform.position, newPos.position, Time.deltaTime * moveSpeed);

			} else {

				transform.position += (distance / 100) * moveSpeed;

			}

			Vector3 changeInDistance = transform.position - lastPosition;

			foreach (GameObject obj in collidingObjects) {
				obj.transform.position += changeInDistance;
			}

			yield return new WaitForEndOfFrame ();
		}

		coroutine = RepeatLoop ();

		StartCoroutine (coroutine);

	}

	IEnumerator RepeatLoop(){

		float counter = 0;

		if (positionCounter + 1 >= Positions.Count || positionCounter == 0)
			counter = startPauseTime;
		else 
			counter = pauseTime;

		while (counter > 0 && pausing) {

			counter -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();
		}

		if (reversing) {

			if (reverseBool) {

				if (positionCounter - 1 > 0) {
					positionCounter -= 1;
				} else if (positionCounter - 1 <= 0 && looping) {	// only loop if you are allowed to
					positionCounter = 0;
					reverseBool = false;
				}
				else
					yield return null;

			} else {

				if (positionCounter + 1 < Positions.Count) {
					positionCounter += 1;
				}
				else if (positionCounter + 1 >= Positions.Count) {
					positionCounter -= 1;
					reverseBool = true;
				}

			}

		} else {

			if (positionCounter + 1 < Positions.Count) {
				positionCounter += 1;
			}
			else if (positionCounter + 1 >= Positions.Count && looping) {	// only loop if you are allowed to
				positionCounter = 0;
			}
			else 
				yield return null;

		}

		coroutine = MoveToward (Positions [positionCounter]);

		StartCoroutine (coroutine);

	}
}
