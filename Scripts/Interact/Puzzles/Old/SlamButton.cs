using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// SWITCH SCRIPT TO 'PuzzleButton.cs' INSTEAD



public class SlamButton : MonoBehaviour {
	 
//	// Events
//	public delegate void SlamButton_Action();
//	public event SlamButton_Action OnSlamButtonActivated;
//
//	public delegate void SlamButton_Reset();
//	public event SlamButton_Reset OnSlamButtonReset;
//
//
//	// If you have a timer attached, the button will reset after that timer is finished
//	public GameObject resetTimer;
//
//
//	GameObject playerObj;
//
//	Animator anim;
//
//
//	Puzzle_WinZone winZone;
//
//	bool winState = false;
//
//
//	void Start () {
//
//		playerObj = GameObject.FindWithTag ("Player");
//
//		anim = GetComponent<Animator> ();
//
//		if (resetTimer) {
//		
//			resetTimer.GetComponent<ActivatedTimer> ().OnTimerRunOut += ButtonReset;
//		
//			if (GameObject.Find (transform.parent.transform.parent.name + "/WinZone")) {
//
//				// References
//				winZone = GameObject.Find (transform.parent.transform.parent.name + "/WinZone").GetComponent<Puzzle_WinZone> ();
//				winZone.OnWinZoneActivate += Complete;
//
//			} else {
//
//				Debug.Log ("No Winzone Found on - " + transform.name);
//
//			}
//
//		}
//
//	}
//
//	void Complete(){
//
//		winState = true;
//
//	}
//
//	void OnCollisionEnter(Collision col){
//
//		if (col.transform.tag == "Player" && playerObj.GetComponent<BallController> ().IsSlamming && !winState) {
//
//			playerObj.GetComponent<BallController> ().CancelBallSlam (true);
//
//			StartCoroutine (ButtonPressed ());
//
//		}
//
//	}
//
//	IEnumerator ButtonPressed(){
//
//		anim.SetTrigger ("Push");
//
//		// Make sure event isn't null, then cast.
//		if(OnSlamButtonActivated != null)
//			OnSlamButtonActivated ();
//
//		yield return new WaitForSeconds (1.0f);
//
//	}
//
//	// On an event, but can be publicly called
//	public void ButtonReset(){
//		
//		if(!winState)
//			anim.SetTrigger ("Reset");
//
//		// Make sure event isn't null, then cast.
//		if(OnSlamButtonReset != null)
//			OnSlamButtonReset ();
//		
//	}
}
