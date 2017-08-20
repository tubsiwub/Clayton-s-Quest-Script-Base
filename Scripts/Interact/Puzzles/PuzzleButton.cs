using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ButtonType{

	SLAM,		// Requires Player to use Ball Slam
	NORMAL,		// Simply stand on the button
	WEIGHT,		// Requires a specific weight to unlock
	KEY,		// Requires specific key object
	LOCKSWITCH,
	PUSHYBALL	// Sets its target ball based on a radius and reset whichever ball was assigned last

}


public class PuzzleButton : MonoBehaviour {

	public ButtonType buttonType;

	// Events
	public delegate void Button_Activate();
	public event Button_Activate OnButtonActivated;

	public delegate void Button_Reset();
	public event Button_Reset OnButtonReset;



	#region WEIGHT BUTTON VARIABLES
	// WEIGHT BUTTON VARS
	List<GameObject> weightObjects;

	public float targetWeightToUnlock;

	public float currentWeightAchieved;
	#endregion

	#region SLAM BUTTON VARIABLES
	// SLAM BUTTON VARS

	// the button will reset after that timer is finished
	ActivatedTimer resetTimer;
	public ActivatedTimer ResetTimer { get { return resetTimer; } }

	public bool timerToggle;
	public float timerTime;

	public WinZone winZone;

	bool winState = false;
	#endregion

	#region KEY BUTTON VARIABLES
	// KEY BUTTON VARS

	public bool destroyKeyObj;

	public bool removeKeyObj;

	// If you want to use a specific key object, you can input that here
	public bool useSpecificObject;

	#endregion

	#region PUSHY BALL VARS

	Vector3 startingPosition;
	Quaternion startingRotation;
	Vector3 startingScale;

	#endregion

	// GENERAL VARS
	[SerializeField]
	public GameObject keyObj;

	public Checkpoint checkpoint;

	bool buttonActivated = false;

	Animator anim;

	GameObject playerObj;

	PlayerHolder playerHolder;



	void Start () {

		playerObj = GameObject.FindWithTag ("Player");

		playerHolder = GameObject.FindWithTag ("Player").GetComponentInChildren<PlayerHolder> ();

		resetTimer = GameObject.Find ("TimerUI").GetComponent<ActivatedTimer>();

		if (buttonType == ButtonType.PUSHYBALL) {

			startingPosition = keyObj.transform.position;
			startingRotation = keyObj.transform.rotation;
			startingScale = keyObj.transform.localScale;

		}

		if (buttonType == ButtonType.SLAM) {

			if (resetTimer) {

				resetTimer.GetComponent<ActivatedTimer> ().OnTimerRunOut += ButtonReset;

				if (transform.parent.transform.parent) {
					
					if (GameObject.Find (transform.parent.transform.parent.name + "/WinZone")) {

						// References
						if(winZone)
						winZone.OnWinZoneActivate += Complete;

					} 

				}
				else if(transform.parent){

					if (GameObject.Find (transform.parent.name + "/WinZone")) {

						// References
						if (winZone) {
							winZone = GameObject.Find (transform.parent.transform.parent.name + "/WinZone").GetComponent<WinZone> ();
							winZone.OnWinZoneActivate += Complete;
						}

					} 

				}

			}

		}

		if (buttonType == ButtonType.WEIGHT) {

			weightObjects = new List<GameObject> ();

   		}

		if(GetComponent<Animator> ())
			anim = GetComponent<Animator> ();

	}

	void Update () {

		if (buttonType == ButtonType.WEIGHT) {

			// If we hit the correct weight the first time...
			if (currentWeightAchieved >= targetWeightToUnlock && !buttonActivated) {

				if(OnButtonActivated != null)
					OnButtonActivated ();	// Fire the event cannons!
				buttonActivated = true;

			} 

			// If we lose the correct weight after having had it...
			if(currentWeightAchieved < targetWeightToUnlock && buttonActivated){

				if(OnButtonReset != null)
					OnButtonReset ();	// Fire the event cannons!
				buttonActivated = false;

			}

		}

		if (buttonType == ButtonType.PUSHYBALL) {

			if (keyObj != null) {
				if (!keyObj.GetComponent<PushyBall> ()) {
					anim.SetTrigger ("Push");
					anim.ResetTrigger ("Reset");
				}
			}

		}

	}

	void OnTriggerEnter(Collider col){

		if (buttonType == ButtonType.NORMAL) {
			
			if (col.transform.tag == "Player" && !buttonActivated) {

				Debug.Log ("Normal ON");

				buttonActivated = true;

				if (OnButtonActivated != null)
					OnButtonActivated ();

			}

		}

		if (buttonType == ButtonType.WEIGHT) {

			if (!weightObjects.Contains (col.gameObject) && col.transform.GetComponent<WeightObject>()) {

				weightObjects.Add (col.gameObject);

				currentWeightAchieved += col.transform.GetComponent<WeightObject> ().weightValue;

				Debug.Log ("Added: " + col.gameObject.name);

			}

		}

		if (buttonType == ButtonType.KEY) {

			if (col.GetComponent<PushyBall> ()) {
				col.GetComponent<PushyBall> ().RemoveAllReferences ();
			}

			if (!buttonActivated){

				// If you're using a specific object, check for it first.
				if (useSpecificObject) {

					if (col == keyObj) {	// Note: was checking "if name" before

						if(playerHolder.IsHolding)
						if (playerHolder.HeldObject.gameObject == col.gameObject) {
							return;
						}

						if (OnButtonActivated != null)
							OnButtonActivated ();	// Fire the event cannons!

						buttonActivated = true;

					}
				}
				else if (col.GetComponent<Puzzle_KeyObject> ()) {

					if(playerHolder.IsHolding)
					if (playerHolder.HeldObject.gameObject == col.gameObject) {
						return;
					}
						
					if (OnButtonActivated != null)
						OnButtonActivated ();	// Fire the event cannons!
		
					buttonActivated = true;

					if (destroyKeyObj)
						Destroy (col.gameObject);

					// shift object 50000 units away into what most likely is unused space
					if (removeKeyObj)
						col.gameObject.transform.position -= Vector3.one * 50000;	
					
				}
			}
		}

		if (buttonType == ButtonType.LOCKSWITCH) {

			if(col != null && keyObj != null)
			if (col.transform.name == keyObj.transform.name) {

				if (OnButtonActivated != null)
					OnButtonActivated ();	// Fire the event cannons!

				buttonActivated = true;

				// Destroy held key
				Destroy (col.gameObject);

				// Show the key getting used
				anim.SetTrigger ("Success");

			}

		}

	}

	void OnTriggerExit(Collider col){

		if (buttonType == ButtonType.NORMAL) {
			
			if (col.transform.tag == "Player" && buttonActivated) {

				Debug.Log ("Normal OFF");

				buttonActivated = false;

				if (OnButtonReset != null)
					OnButtonReset ();

			}

		}

		if (buttonType == ButtonType.WEIGHT) {

			if (weightObjects.Contains (col.gameObject) && col.transform.GetComponent<WeightObject>()) {

				weightObjects.Remove (col.gameObject);

				currentWeightAchieved -= col.transform.GetComponent<WeightObject> ().weightValue;

				Debug.Log ("Removed: " + col.gameObject.name);

			}

		}

		if (buttonType == ButtonType.KEY) {

			if (buttonActivated) {

				// If you're using a specific object, check for it first.
				if (useSpecificObject) {

					if (col.transform.name == keyObj.transform.name) {

						if (OnButtonActivated != null)
							OnButtonActivated ();	// Fire the event cannons!

						buttonActivated = true;

					}

				}
				else if (col.GetComponent<Puzzle_KeyObject> ()) {

					if (OnButtonReset != null)
						OnButtonReset ();	// Fire the event cannons!

					buttonActivated = false;
				}

			}

		}

	}
		
	void OnCollisionEnter(Collision col){

		if (buttonType == ButtonType.SLAM) {

			if (col.transform.tag == "Player" && playerObj.GetComponent<BallController> ().IsSlamming && !winState) {

				// TIMER SETUP
				if (timerToggle) {
					if (winZone) {
						resetTimer.ClaimTimer (checkpoint, TIMERTYPE.BUTTON, this.gameObject, timerTime, winZone);
						winZone.PuzzleON ();
					}
					else
						resetTimer.ClaimTimer (checkpoint, TIMERTYPE.BUTTON, this.gameObject, timerTime);
				}
				// ----

				ButtonReset ();

				playerObj.GetComponent<BallController> ().CancelBallSlam (true);

				StartCoroutine (ButtonPressed ());

				if(winZone)
				if (!winZone.gameObject.activeSelf)
					winZone.gameObject.SetActive (true);

			}

		}

		if (buttonType == ButtonType.PUSHYBALL) {

			if (col.transform.tag == "Player" && playerObj.GetComponent<BallController> ().IsSlamming && !winState && keyObj != null) {

				playerObj.GetComponent<BallController> ().CancelBallSlam (true);

				StartCoroutine (ButtonPressed ());

				ResetBall ();

			}

		}

	}

	public void ResetKeyObj(GameObject obj){
		keyObj = null;
		anim.SetTrigger ("Push");
		anim.ResetTrigger ("Reset");
	}

	public void SetBall(GameObject obj, GameObject respawnPoint){

		if (buttonType == ButtonType.PUSHYBALL) {

			keyObj = obj;

			startingPosition = respawnPoint.transform.position;
			startingRotation = respawnPoint.transform.rotation;
			startingScale = obj.transform.localScale;

		}

	}

	void ResetBall(){

		// Reset ball to orig. values
		keyObj.transform.position = startingPosition;
		keyObj.transform.rotation = startingRotation;
		keyObj.transform.localScale = startingScale;

		if(keyObj.GetComponent<Rigidbody> ())
		keyObj.GetComponent<Rigidbody> ().velocity = Vector3.zero;

		StartCoroutine (ResetButton ());

	}

	IEnumerator ResetButton(){

		yield return new WaitForSeconds (0.4f);

		ButtonReset ();

	}

	#region SLAM BUTTON FUNCTIONS

	void Complete(){

		winState = true;

	}

	IEnumerator ButtonPressed(){

		anim.ResetTrigger ("Reset");
		anim.SetTrigger ("Push");

		// Make sure event isn't null, then cast.
		if(OnButtonActivated != null)
			OnButtonActivated ();

		yield return new WaitForSeconds (1.0f);

	}

	// On an event, but can be publicly called
	public void ButtonReset(){

		if (!winState && buttonType != ButtonType.LOCKSWITCH) {
			anim.ResetTrigger ("Push");
			anim.SetTrigger ("Reset");
		}

		// Make sure event isn't null, then cast.
		if(OnButtonReset != null)
			OnButtonReset ();

		if(winZone)
		if (winZone.gameObject.activeSelf)
			winZone.gameObject.SetActive (false);

	}

	#endregion

}
