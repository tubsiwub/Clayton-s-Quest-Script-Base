using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PuzzleButton))]
[CanEditMultipleObjects]
public class PuzzleButtonEditor : Editor {


	SerializedProperty targetWeightToUnlock;
	SerializedProperty currentWeightAchieved;
	SerializedProperty timerToggle;
	SerializedProperty timerTime;
	SerializedProperty destroyKeyObj;
	SerializedProperty removeKeyObj;
	SerializedProperty useSpecificObject;
	SerializedProperty keyObj;
	SerializedProperty winZone;
	SerializedProperty checkpoint;
	SerializedProperty ballWinZone;


	void OnEnable(){
		
		targetWeightToUnlock = serializedObject.FindProperty ("targetWeightToUnlock");
		currentWeightAchieved = serializedObject.FindProperty ("currentWeightAchieved");
		timerToggle = serializedObject.FindProperty ("timerToggle");
		timerTime = serializedObject.FindProperty ("timerTime");
		destroyKeyObj = serializedObject.FindProperty ("destroyKeyObj");
		removeKeyObj = serializedObject.FindProperty ("removeKeyObj");
		useSpecificObject = serializedObject.FindProperty ("useSpecificObject");
		keyObj = serializedObject.FindProperty ("keyObj");
		winZone = serializedObject.FindProperty ("winZone");
		checkpoint = serializedObject.FindProperty ("checkpoint");
		ballWinZone = serializedObject.FindProperty ("ballWinZone");

	}

	public override void OnInspectorGUI(){

		// MUST INCLUDE
		serializedObject.Update ();

		PuzzleButton mainScript = target as PuzzleButton;

		ButtonType buttonType = (ButtonType)mainScript.buttonType;
		mainScript.buttonType = (ButtonType) EditorGUILayout.EnumPopup ("Button Type", buttonType);

		if (buttonType == ButtonType.WEIGHT) {

			EditorGUILayout.PropertyField (targetWeightToUnlock, new GUIContent ("Target Weight: ", "Target Weight to send events"));
			EditorGUILayout.PropertyField (currentWeightAchieved, new GUIContent ("Current Weight: ", "Current Weight of button"));

		}

		if (buttonType == ButtonType.SLAM) {

			EditorGUILayout.PropertyField (timerToggle, new GUIContent ("Have Timer? ", "If you use a timer, set the Obj Ref here"));
			EditorGUILayout.PropertyField (timerTime, new GUIContent ("Time Amt:", "Float value - counts down with timer start"));

			GUILayout.Space (12.0f);

			EditorGUILayout.PropertyField (winZone, new GUIContent ("Win Zone: "));

		}

		if (buttonType == ButtonType.PUSHYBALL) {

			EditorGUILayout.PropertyField (keyObj, new GUIContent("Push Ball: ", "Place the first target here; all later ones will override this slot.  This object is what gets reset when pushed."));
			EditorGUILayout.PropertyField (ballWinZone, new GUIContent("Ball Win Zone: ", "The win zone - disables button when activated"));

		}

		if (buttonType == ButtonType.KEY) {

			EditorGUILayout.PropertyField (removeKeyObj, new GUIContent ("Remove Key Object: ", "If checked, moves key object to a distance place when it's used up."));

			if(!mainScript.removeKeyObj)
				EditorGUILayout.PropertyField (destroyKeyObj, new GUIContent ("Destroy Key Object: ", "If checked, destroys key object when it's used up."));

			EditorGUILayout.PropertyField (useSpecificObject, new GUIContent (" - Use Specific KEY Object", "If you have a specific key object you'd like to use, check this and set it"));

			if (mainScript.useSpecificObject) {
				EditorGUILayout.PropertyField (keyObj, new GUIContent ("Key Object: "));
			}
		}

		if (buttonType == ButtonType.LOCKSWITCH) {

			EditorGUILayout.PropertyField (keyObj, new GUIContent("Key Obj: ", "Set specific key object here."));

		}

		GUILayout.Space (6.0f);

		EditorGUILayout.PropertyField (checkpoint, new GUIContent("Checkpoint: ", "If you have a specific respawn checkpoint location, set it here."));

		GUILayout.Space (6.0f);

		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();

	}

}