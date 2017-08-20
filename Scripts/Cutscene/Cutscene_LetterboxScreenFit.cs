using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum POSITION {

	TOP, BOTTOM

}

public class Cutscene_LetterboxScreenFit : MonoBehaviour {

	public POSITION position;

	// self checked
	Transform scriptManager;
	GameObject triggerBox;

	RectTransform rt;

	GameObject playerObj;

	float shiftSpeed = 4.0f;

	float 
		letterboxHeight;

	Vector3	
		positionOnScreen,  
		positionOffScreen;

	bool 
		cancelLetterboxing = false,
		cutsceneActive = false;

	public bool CutsceneActive { get { return cutsceneActive; } set { cutsceneActive = value; } }

	void Start () {

		if (scriptManager != null) {
			scriptManager.GetComponent<ScriptStateManager>().OnCutsceneCancel += CancelLetterboxing;
		}

		if (triggerBox != null) {
			triggerBox.GetComponent<Cutscene_TriggerScript>().OnTriggered += Triggered;
		}

		rt = GetComponent<RectTransform> ();
		playerObj = GameObject.FindWithTag ("Player");


		float resolutionY = transform.parent.GetComponent<Screen_CanvasScaling> ().GetResolution.y;


		letterboxHeight = resolutionY / 10;


		// Set position variables
		if (position == POSITION.TOP) {
			positionOnScreen = Vector3.zero;
			positionOffScreen = new Vector3 (0, letterboxHeight, 0);
			//positionOffScreen = Vector3.up * (Camera.main.pixelHeight/2 + letterboxHeight);
		}
		if (position == POSITION.BOTTOM) {
			positionOnScreen = Vector3.zero;
			positionOffScreen = new Vector3 (0, -letterboxHeight, 0);
			//positionOffScreen = Vector3.up * (-Camera.main.pixelHeight/2 - letterboxHeight);
		}

		rt.anchoredPosition3D = positionOffScreen;

	}

	void Update(){

		if (Input.GetButtonDown ("Abort")) {
			CancelLetterboxing ();
		}

	}

	void CancelLetterboxing(){

		cancelLetterboxing = true;

		// Allow the player to move after the cutscene plays
		playerObj.GetComponent<PlayerHandler>().SetFrozen(false, true);

	}

	public void Triggered(){
		
		StopAllCoroutines ();

		cancelLetterboxing = false;

		StartCoroutine (Letterboxing ());

	}

	IEnumerator Letterboxing(){

		// Add in the letterboxing
		GetComponent<Image>().color = new Color(0,0,0,1);

		while(Mathf.Abs(Vector3.Distance(rt.anchoredPosition3D, positionOnScreen)) > 0.3f){

			if (cancelLetterboxing)
				break;

			rt.anchoredPosition3D = Vector3.Lerp (rt.anchoredPosition3D, positionOnScreen, shiftSpeed * Time.deltaTime);

			yield return new WaitForEndOfFrame();
		}

		rt.anchoredPosition3D = positionOnScreen;

//		// Pause
//		if(!cancelLetterboxing)
//			yield return new WaitForSeconds (cutsceneLength - 8);	// was 0.1f

		// Wait for cutscene to end
		while (cutsceneActive) {
			yield return new WaitForEndOfFrame();
		}


		while(Mathf.Abs(Vector3.Distance(rt.anchoredPosition3D, positionOffScreen)) > 0.3f){

			rt.anchoredPosition3D = Vector3.Lerp (rt.anchoredPosition3D, positionOffScreen, shiftSpeed * Time.deltaTime);

			yield return new WaitForEndOfFrame();
		}

		// Remove the letterboxing
		GetComponent<Image>().color = new Color(0,0,0,0);

		rt.anchoredPosition3D = positionOffScreen;

		// Make sure the letterboxing is able to function after stopping once.
		cancelLetterboxing = false;

	}

}