using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum STATE
{
	CUTSCENE
}


public class ScriptStateManager : MonoBehaviour {

	// Events
	public delegate void CancelCutscene();
	public event CancelCutscene OnCutsceneCancel;

	public STATE StateType;

	Camera mainCam;

	// Toggles
	public bool useBackgroundAudio;
	public bool tutorial;
	public bool hasButton;
	public bool hasTrigger;
	public bool hasPunch;

	// Cutscenes

	[Tooltip("Plays this audio throughout the cutscene.")]
	public AudioClip cutsceneBackgroundAudio;

	// cutscene background audio
	public int CBAVolume;

	public List<Camera> cutsceneCameras;
	public List<GameObject> cutsceneComponents;
	public List<float> cameraAnimationDurations;

	Canvas mainHUDCanvas;
	ActivatedTimer timerObj;
	YouDidItScript youDidIt;

	public GameObject puzzleButton;
	public GameObject triggerBox;
	public GameObject punchBox;

	GameObject bottomLetterbox;
	GameObject topLetterbox;

	GameObject playerObj;

	private float cutsceneCounter = 0;
	private float cutsceneCounterTotal = 0;
	private int totalCameras;
	private float currentTime;
	private float cutsceneDelaySpeed = 1.0f;	// how long we wait before actual cutscene

	public float CutsceneCounter { get { return cutsceneCounter; } }

	bool cutsceneActive = false;
	public bool CutsceneActive { get { return cutsceneActive; } }

	// If true, cutscene stops
	bool cancelCutscene = false;

	private AudioSource audiosource;

	void Start () {

		mainCam = Camera.main;
		mainHUDCanvas = GameObject.Find ("MainHUDCanvas").GetComponent<Canvas>();
		timerObj = GameObject.Find ("TimerUI").GetComponent<ActivatedTimer> ();
		youDidIt = GameObject.Find ("WinZone_TextEffect").GetComponent<YouDidItScript> ();

		bottomLetterbox = GameObject.Find ("LetterboxCanvas/PlayerLetterbox_Bottom");
		topLetterbox = GameObject.Find ("LetterboxCanvas/PlayerLetterbox_Top");

		// Events
		if (puzzleButton) {
			puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated += TriggerCutscene;
		}

		if (triggerBox != null) {
			triggerBox.GetComponent<Cutscene_TriggerScript> ().OnTriggered += TriggerCutscene;
		}

		if (punchBox != null) {
			punchBox.GetComponent<Cutscene_PunchBox> ().OnTriggered += TriggerCutscene;
		}

		// Initialize
		Init();

		// Start functionality
		RESET_CUTSCENE ();

	}

	void Init(){

		cutsceneComponents = new List<GameObject> ();

		// Set the variables
		audiosource = GetComponent<AudioSource> ();

		playerObj = GameObject.FindWithTag ("Player");

	}

	public void START_CUTSCENE(){

		// Set the flag
		cutsceneActive = true;

		youDidIt.SetCutsceneCheck (true);

		timerObj.PauseTimer ();

		RESET_CUTSCENE ();

		if (cancelCutscene) {
			cutsceneActive = false;
			cancelCutscene = false;
		}

		// Play cutscene audio
		if(!audiosource.isPlaying)
			audiosource.PlayOneShot (cutsceneBackgroundAudio, (float)(CBAVolume/100));

	}

	public void STOP_CUTSCENE(){

		abortable = false;

		youDidIt.SetCutsceneCheck (false);

		timerObj.UnpauseTimer ();

		// Unfreeze camera
		mainCam.GetComponent<CameraControlDeluxe>().SetFreeze(false, false);

		// Remove the flag
		cutsceneActive = false;

		bottomLetterbox.GetComponent<Cutscene_LetterboxScreenFit> ().CutsceneActive = cutsceneActive;
		topLetterbox.GetComponent<Cutscene_LetterboxScreenFit> ().CutsceneActive = cutsceneActive;

		mainHUDCanvas.enabled = true;

		// Stop Cutscene trigger
		if (OnCutsceneCancel != null)
			OnCutsceneCancel ();

		// turn off cutscene cameras, enable main camera
		foreach (Camera cam in cutsceneCameras)
			cam.gameObject.SetActive (false);
		foreach (GameObject obj in cutsceneComponents)
			obj.GetComponent<CutsceneComponent> ().gameObject.SetActive (false);

		if (audiosource.isPlaying) {
			audiosource.Stop ();
		}

		StopAllCoroutines ();

		if (triggerBox != null)
			StartCoroutine (TurnOnTriggerBox ());
		
		// Allow the player to move after the cutscene plays
		playerObj.GetComponent<PlayerHandler>().SetFrozen(false, true);

	}

	// Just a toggle, not turning camera on and off
	bool mainCameraToggle = true;

	void PLAY_CUTSCENE(){

		// Run cutscene until duration end
		if (cutsceneActive) {

			if (mainCam.gameObject.activeSelf && mainCameraToggle) {

				mainCameraToggle = false;
				cutsceneCameras [0].gameObject.SetActive (true);
				cutsceneCameras [0].GetComponent<Cutscene_CameraEventObjects> ().SETACTIVE (true);

			}

			while (cutsceneCounter > 0) {

				cutsceneCounter -= Time.deltaTime;

				// Total time of current animation
				currentTime = cameraAnimationDurations [totalCameras];

				if (cutsceneCounter > cutsceneCounterTotal - currentTime) {

					return;

				} else {

					cutsceneCounterTotal = cutsceneCounter;

					cutsceneCameras [totalCameras].gameObject.SetActive (false);

					if (totalCameras < cameraAnimationDurations.Count - 1)
						totalCameras += 1;

					cutsceneCameras [totalCameras].gameObject.SetActive (true);
					cutsceneCameras [totalCameras].GetComponent<Cutscene_CameraEventObjects> ().SETACTIVE (true);

				}

			}

			STOP_CUTSCENE ();

			//StartCoroutine (ExitCutscene (2.0f, cutsceneCameras [totalCameras]));

		}	// playCutscene bool
		else {

			// turn off cutscene cameras, enable main camera
			mainCameraToggle = true;
			foreach (Camera cam in cutsceneCameras) {
				cam.gameObject.SetActive (false);
				cam.GetComponent<Cutscene_CameraEventObjects> ().SETACTIVE (false);
			}

		}

	}

	IEnumerator ExitCutscene(float time, Camera camera){

		yield return new WaitForSeconds (time);

		STOP_CUTSCENE ();

		RESET_CUTSCENE ();

	}

	IEnumerator TurnOnTriggerBox(){

		yield return new WaitForSeconds (2.0f);

		if (triggerBox != null) 
			triggerBox.SetActive (true);

	}

	public void RESET_CUTSCENE(){

		cutsceneCounter = 0;
		cutsceneCounterTotal = 0;

		switch (StateType) {

		// CUTSCENE
		case STATE.CUTSCENE:

			CalculateCutsceneCounter ();

			totalCameras = 0;

			// If turned on, we enable the first camera
			if (cutsceneActive) {

				cutsceneCameras [0].gameObject.SetActive (true);

				foreach (GameObject obj in cutsceneComponents)
					obj.GetComponent<CutsceneComponent> ().gameObject.SetActive (true);
				
			} else {

				mainCameraToggle = true;

				foreach (GameObject obj in cutsceneComponents)
					obj.GetComponent<CutsceneComponent> ().gameObject.SetActive (false);
				
			}

			break;	//cutscene

		}

	}


	void CalculateCutsceneCounter(){
		for (int i = 0; i < cameraAnimationDurations.Count; i++) {
			cutsceneCounter += cameraAnimationDurations [i];
			cutsceneCounterTotal += cameraAnimationDurations [i];
		}
	}



	// True when a cutscene is specifically triggered
	//bool cutsceneTrigger = false;

	bool abortable = false;

	public void TriggerCutscene(){

		mainHUDCanvas.enabled = false;

		// Freeze camera
		mainCam.GetComponent<CameraControlDeluxe>().SetFreeze(true, false);

		// Reset status until cancel is pressed again
		cancelCutscene = false;
		abortable = true;

		// Disallow the player to move while the cutscene plays
		playerObj.GetComponent<PlayerHandler>().SetFrozen(true, true);

		if (triggerBox != null) {
			triggerBox.SetActive (false);
		}

		CalculateCutsceneCounter ();

		// Here's where we handle the letterboxing
		bottomLetterbox.GetComponent<Cutscene_LetterboxScreenFit> ().CutsceneActive = true;
		topLetterbox.GetComponent<Cutscene_LetterboxScreenFit> ().CutsceneActive = true;
		bottomLetterbox.GetComponent<Cutscene_LetterboxScreenFit> ().Triggered ();
		topLetterbox.GetComponent<Cutscene_LetterboxScreenFit> ().Triggered ();

		StartCoroutine (DelayStart (cutsceneDelaySpeed));

	}

	IEnumerator DelayStart(float time){

		yield return new WaitForSeconds (time);

		if (!cancelCutscene) {
			START_CUTSCENE ();
		} 

		cancelCutscene = false;
	}

	void Update () {

		if (Input.GetButtonDown ("Abort") && abortable) {
			StopAllCoroutines ();
			cancelCutscene = true;
			STOP_CUTSCENE ();
		}

		switch (StateType) {

		// CUTSCENE
		case STATE.CUTSCENE:

			PLAY_CUTSCENE ();

			break;	//cutscene

		}

	}


	// Window Example
//	public Rect windowRect = new Rect(20, 20, 120, 50);
//	void OnGUI() {
//		windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "My Window");
//	}
//	void DoMyWindow(int windowID) {
//		if (GUILayout.Button("Hello World"))
//			print("Got a click");
//
//	}


}

