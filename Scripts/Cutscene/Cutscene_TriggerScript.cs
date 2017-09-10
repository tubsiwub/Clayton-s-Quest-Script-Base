using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Must place within the same parent as the scriptManager object

public class Cutscene_TriggerScript : MonoBehaviour {

	// Events
	public delegate void Triggerbox_Triggered();
	public event Triggerbox_Triggered OnTriggered;

	public ScriptStateManager scriptManager;
	public WinZone winZone;

	ActivatedTimer timerScript;
	public bool activateTimer = false;
	public float timerTime = 0;

	public Checkpoint checkpoint;

	[Tooltip("Checking this will delete the trigger after the cutscene plays once")]
	public bool PlayOnce = true;

	Vector3 origin;

	void Start()
	{
		if (activateTimer)
		{
			timerScript = GameObject.Find ("TimerUI").GetComponent<ActivatedTimer> ();
			timerScript.OnTimerRunOut += timerRunOut;
		}

		origin = transform.position;
	}

	void Update()
	{
		CutsceneTimer ();
	}

	void timerRunOut()
	{
		if (PlayOnce) {
			gameObject.SetActive (true);
			StartCoroutine (DestroyAfterTime (0.5f));
		}

		if (!timerScript.WinState)
			transform.position = origin;
		else
		{
			if (this.gameObject.activeSelf)
				StartCoroutine (DestroyAfterTime (0.5f));
		}
	}

	void OnDestroy(){

		if(activateTimer)
			timerScript.OnTimerRunOut -= timerRunOut;

	}

	IEnumerator DestroyAfterTime(float wait){

		yield return new WaitForSeconds (wait);

		Destroy (this.gameObject);

	}

	void CutsceneTimer(){

		if(!scriptManager.GetComponent<ScriptStateManager>().CutsceneActive)
			return;
		else 
			transform.position = origin;
		
	}

	void OnTriggerEnter(Collider col){

		if (col.transform.tag == "Player" && HealthManager.instance.Lives > 0) {

			if (activateTimer) {
				if(!winZone) timerScript.ClaimTimer (checkpoint, TIMERTYPE.TRIGGER, this.gameObject, timerTime, scriptManager);
				if (winZone) {
					timerScript.ClaimTimer (checkpoint, TIMERTYPE.TRIGGER, this.gameObject, timerTime, scriptManager, winZone);
					winZone.PuzzleON ();
				}
			}

			// Fly away, never to be touched again!
			transform.position += Vector3.up * 999;

			// Send out the event call
			if (OnTriggered != null) {
				OnTriggered ();
			}

			// If we use it once then who cares about it?
			if (PlayOnce && this.gameObject.activeSelf)
				StartCoroutine (DestroyAfterTime (0.5f));
		}

	}
}
