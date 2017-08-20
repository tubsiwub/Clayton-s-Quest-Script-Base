using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetAudioVolume_CurrentPlaying : MonoBehaviour {

	public AudioSource audioSource;

	public float updateStep = 0.1f;
	public int sampleDataLength = 1024;

	float currentUpdateTime = 0.0f;
	float clipLoudness = 0;
	float previousLoudness = 0;
	float[] clipSampleData;

	float bottomLoudness = 0;
	float topLoudness = 0.29f;

	public float maxDifferenceToSpawn = 0.01f;

	public float GetLoudness { get { return clipLoudness; } }
	public float GetBottomLoudness { get { return bottomLoudness; } }
	public float GetTopLoudness { get { return topLoudness; } }

	public GameObject redCircle;

	void Awake () {

		if (!audioSource)
			Debug.LogError (GetType () + ".Awake: No AudioSource set!");

		clipSampleData = new float[sampleDataLength];

	}



	void Update () {

		currentUpdateTime += Time.deltaTime;

		// Perform actions at updateStep intervals
		if (currentUpdateTime >= updateStep) {

			// reset counter
			currentUpdateTime = 0;

			// reads in 1024 samples (roughly 80ms on a 44khz stereo clip) starting at current position of clip
			audioSource.clip.GetData (clipSampleData, audioSource.timeSamples);

			// store past loudness
			previousLoudness = clipLoudness;
				
			// ???
			clipLoudness = 0;
			foreach (var sample in clipSampleData) 
			{
				clipLoudness += Mathf.Abs (sample);
			}
			clipLoudness /= sampleDataLength;

			// If we have a new base, set it
			if (clipLoudness < bottomLoudness)
				bottomLoudness = clipLoudness;

			// If we have a new max, set it
			if (clipLoudness > topLoudness)
				topLoudness = clipLoudness;

			// if, on it's way back down, it's a big enough change...
			if (previousLoudness - clipLoudness > maxDifferenceToSpawn) {

				GameObject circle = (GameObject)Instantiate (redCircle);
				circle.transform.SetParent (this.transform.parent);
				circle.transform.localPosition = new Vector3 (-450,0,1);
				StartCoroutine (MoveCircle(circle));

			}

		}

	}

	IEnumerator MoveCircle(GameObject circle){

		while (circle.transform.position.x < 800) {

			circle.transform.position += Vector3.right * 6;

			yield return new WaitForEndOfFrame ();

		}

		Destroy (circle);

	}

	float ConvertNumberRange(float baseValue, float oldMin, float oldMax, float newMin, float newMax){

		float OldRange = oldMax - oldMin;
		float NewRange = newMax - newMin;
		float NewValue = (((baseValue - oldMin) * NewRange) / OldRange) + newMin;

		return NewValue;

	}

}
