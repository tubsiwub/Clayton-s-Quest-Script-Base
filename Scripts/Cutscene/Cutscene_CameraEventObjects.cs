using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene_CameraEventObjects : MonoBehaviour {

	public List<GameObject> sceneObjects;

	// toggles
	public bool useSpecificAudio;
	public bool tutorial;

	[Tooltip("Delay before audio begins playing.")]
	public float audioDelay = 0.1f;

	[Tooltip("Plays only the first slotted audio for now.")]
	public AudioClip specificAudio;

	public int audioVolume;

	AudioSource audiosource;

	void Awake(){
		audiosource = GetComponent<AudioSource> ();
	}


	public void SETACTIVE(bool value){
		
		audiosource = GetComponent<AudioSource> ();

		foreach (GameObject obj in sceneObjects) {
			if(value == true) obj.SetActive (value);
			obj.GetComponent<CutsceneComponent> ().INITIALIZE ();
			if(value == false) obj.SetActive (value);
		}

		if (value) {
			StartCoroutine (PLAYAUDIO ());
		}

	}


	IEnumerator PLAYAUDIO(){
		
		yield return new WaitForSeconds (audioDelay);

		if(specificAudio != null && !audiosource.isPlaying)
			audiosource.PlayOneShot (specificAudio, (float)(audioVolume/100));
		
	}


}

