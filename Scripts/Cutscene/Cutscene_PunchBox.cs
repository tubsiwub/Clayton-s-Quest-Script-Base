using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tips:  Make sure to include a ref. in the cutscene letterboxes

// Tips:  While named "PunchBox", this script can go onto any object

public class Cutscene_PunchBox : MonoBehaviour {

	// Events
	public delegate void Triggerbox_Triggered();
	public event Triggerbox_Triggered OnTriggered;

	BalloonExplode balloonScript;

	GameObject playerObj;

	GameObject rightHand, leftHand;
	BallController ballControl;

	float cooldown = 0.0f;

	public float GetCooldown { get { return cooldown; } }

	public ScriptStateManager cutsceneManager;

	void Start () {

		balloonScript = GetComponent<BalloonExplode> ();

		playerObj = GameObject.FindWithTag ("Player");

		rightHand = GameObject.FindWithTag ("Player").GetComponent<PlayerAttack> ().RightHand;
		leftHand = GameObject.FindWithTag ("Player").GetComponent<PlayerAttack> ().LeftHand;
		ballControl = GameObject.FindWithTag ("Player").GetComponent<BallController> ();

	}



	void Update () {
		
	}


	void OnTriggerEnter(Collider col) {

		if (cooldown <= 0 && balloonScript.BalloonActive) {
			if (col.gameObject == rightHand || col.gameObject == leftHand || (col.tag == "Player" && ballControl.IsSlamming)) {

				// Disallow the player to move while the cutscene plays
				playerObj.GetComponent<PlayerHandler> ().SetFrozen (true, true);

				GameObject.Find ("LetterboxCanvas/PlayerLetterbox_Bottom").GetComponent<Cutscene_LetterboxScreenFit> ().Triggered ();
				GameObject.Find ("LetterboxCanvas/PlayerLetterbox_Top").GetComponent<Cutscene_LetterboxScreenFit> ().Triggered ();

				balloonScript.BreakBalloon ();

				// Send out the event call
				if (OnTriggered != null) {
					OnTriggered ();
				}

				StartCoroutine (Cooldown (3.0f));

			}
		}

	}

	IEnumerator Cooldown(float cdTime){

		cooldown = cdTime;

		while (cooldown > 0) {

			cooldown -= Time.deltaTime;

			if (cutsceneManager.CutsceneActive)	// while in cutscene, don't reduce cooldown
				cooldown = cdTime;

			yield return new WaitForEndOfFrame ();

		}

		cooldown = 0.0f;

	}

	// If you're using childed punch boxes, this allows them to work (make sure they have the child script)
	public void FireTrigger(GameObject balloonChild){
		
		balloonScript = balloonChild.GetComponent<BalloonExplode>();

		if (cooldown <= 0 && balloonScript.BalloonActive) {
			// Disallow the player to move while the cutscene plays
			playerObj.GetComponent<PlayerHandler> ().SetFrozen (true, true);

			GameObject.Find ("LetterboxCanvas/PlayerLetterbox_Bottom").GetComponent<Cutscene_LetterboxScreenFit> ().Triggered ();
			GameObject.Find ("LetterboxCanvas/PlayerLetterbox_Top").GetComponent<Cutscene_LetterboxScreenFit> ().Triggered ();

			balloonScript.BreakBalloon ();

			// Send out the event call
			if (OnTriggered != null) {
				OnTriggered ();
			}

			StartCoroutine (Cooldown (3.0f));
		}
	}


}
