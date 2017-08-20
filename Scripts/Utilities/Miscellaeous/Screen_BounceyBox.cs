using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Screen_BounceyBox : MonoBehaviour {

	GetAudioVolume_CurrentPlaying loudnessScript;

	Vector3 bottomPosition, topPosition;

	public GameObject current, high, low;

	public GameObject playerObj, playerFaceObj;

	public float minimumForExpression = 30;

	Animator faceAnim;

	void Start () {

		faceAnim = playerFaceObj.GetComponent<Animator> ();

		loudnessScript = GetComponent<GetAudioVolume_CurrentPlaying> ();

		bottomPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
		topPosition = new Vector3 (transform.localPosition.x, -transform.localPosition.y, transform.localPosition.z);

	}


	void Update () {

		float NewPositionY = 0;

		float currentLoudness = ConvertNumberRange (
			loudnessScript.GetLoudness,
			loudnessScript.GetBottomLoudness,
			loudnessScript.GetTopLoudness,
			0,
			100);

		NewPositionY = ConvertNumberRange (
			currentLoudness,
			0,
			100,
			bottomPosition.y,
			topPosition.y);

		current.GetComponent<Text> ().text = currentLoudness.ToString ();
		low.GetComponent<Text> ().text = loudnessScript.GetBottomLoudness.ToString ();
		high.GetComponent<Text> ().text = loudnessScript.GetTopLoudness.ToString ();

		AnimateFace (currentLoudness);

		if(NewPositionY.ToString() != "NaN") 
			transform.localPosition = new Vector3 (transform.localPosition.x, NewPositionY, transform.localPosition.z);

	}

	void AnimateFace(float percentLoudness){

		if (percentLoudness < minimumForExpression) {
			//faceAnim.ResetTrigger ("Angry");
			faceAnim.SetTrigger ("Smile");
		} else {
			//faceAnim.ResetTrigger ("Smile");
			faceAnim.SetTrigger ("Angry");
		}

	}


	float ConvertNumberRange(float baseValue, float oldMin, float oldMax, float newMin, float newMax){

		float OldRange = oldMax - oldMin;
		float NewRange = newMax - newMin;
		float NewValue = (((baseValue - oldMin) * NewRange) / OldRange) + newMin;

		return NewValue;

	}


}
