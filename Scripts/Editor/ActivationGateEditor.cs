using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActivationGate))]
[CanEditMultipleObjects]
public class ActivationGateEditor : Editor {

	SerializedProperty buttonObj;
	SerializedProperty questObj;
	SerializedProperty questNPC;
	SerializedProperty triggerObj;
	SerializedProperty winZone;
	SerializedProperty timerToggle;
	SerializedProperty openAfterWin;
	SerializedProperty openBeforeWin;
	SerializedProperty questObject;

	void OnEnable(){
		
		buttonObj = serializedObject.FindProperty ("buttonObj");
		questObj = serializedObject.FindProperty ("questObj");
		questNPC = serializedObject.FindProperty ("questNPC");
		triggerObj = serializedObject.FindProperty ("triggerObj");
		winZone = serializedObject.FindProperty ("winZone");
		timerToggle = serializedObject.FindProperty ("timerToggle");
		openAfterWin = serializedObject.FindProperty ("openAfterWin");
		openBeforeWin = serializedObject.FindProperty ("openBeforeWin");
		questObject = serializedObject.FindProperty ("questObject");
	}

	public override void OnInspectorGUI(){

		// MUST INCLUDE
		serializedObject.Update ();

		GUILayout.Space(16.0f);

		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);

		ActivationGate mainScript = target as ActivationGate;

		EditorGUILayout.LabelField("ACTIVATION GATE", titleStyle);

		GUILayout.Space(16.0f);

		EditorGUILayout.PropertyField (timerToggle, new GUIContent("Timer Toggle: ", "If there is no timer, use Button as reference to shut gate"));

		if (timerToggle.boolValue) {

			buttonObj.objectReferenceValue = null;

			EditorGUILayout.HelpBox ("If you use a button, uncheck the box above.", MessageType.None);

			GUILayout.Space(16.0f);

			EditorGUILayout.PropertyField (winZone, new GUIContent ("Win Zone: "));

			if (winZone.objectReferenceValue != null) {
				
				EditorGUILayout.PropertyField (openAfterWin, new GUIContent ("Open After Win: "));
				EditorGUILayout.PropertyField (openBeforeWin, new GUIContent ("Open Before Win: "));

				if (openBeforeWin.boolValue)
					openAfterWin.boolValue = false;

				if(openAfterWin.boolValue)
					openBeforeWin.boolValue = false;
			}

			GUILayout.Space(4.0f);

		}
		else {

			GUILayout.Space(16.0f);

			if (questObj.objectReferenceValue == null && questNPC.objectReferenceValue == null && triggerObj.objectReferenceValue == null) {
				EditorGUILayout.PropertyField (buttonObj, new GUIContent ("Button Obj: "));
				GUILayout.Space (16.0f);
			}

			if(buttonObj.objectReferenceValue == null && questNPC.objectReferenceValue == null && triggerObj.objectReferenceValue == null){
				EditorGUILayout.PropertyField (questObj, new GUIContent ("Quest Type: ", "Object with QuestType - opens AFTER completion"));
				GUILayout.Space (16.0f);
			}

			if(buttonObj.objectReferenceValue == null && questObj.objectReferenceValue == null && triggerObj.objectReferenceValue == null){
				EditorGUILayout.PropertyField (questNPC, new GUIContent ("Quest NPC: ", "Object with QuestContainer - opens BEFORE completion"));
				GUILayout.Space (16.0f);
			}

		}

		if(buttonObj.objectReferenceValue == null && questObj.objectReferenceValue == null && questNPC.objectReferenceValue == null){
			EditorGUILayout.PropertyField (triggerObj, new GUIContent ("Trigger Obj: ", "Object with trigger - starts cutscene immediately"));
			GUILayout.Space (16.0f);
		}

		if (mainScript.x == 0 && mainScript.y == 0 && mainScript.z == 0) {

			EditorGUILayout.HelpBox ("Click to set the current position as the 'Open' position.", MessageType.None);

			GUILayout.Space (4.0f);
		}

		GUILayout.BeginHorizontal ();

		if(GUILayout.Button("Set Open Pos")){

			mainScript.x = mainScript.transform.localPosition.x;
			mainScript.y = mainScript.transform.localPosition.y;
			mainScript.z = mainScript.transform.localPosition.z;

		}

		mainScript.x = EditorGUILayout.FloatField (mainScript.x);
		mainScript.y = EditorGUILayout.FloatField (mainScript.y);
		mainScript.z = EditorGUILayout.FloatField (mainScript.z);

		mainScript.openPosTemp = new Vector3 (mainScript.x, mainScript.y, mainScript.z);

		mainScript.openLocation = mainScript.openPosTemp;

		GUILayout.EndHorizontal ();

		GUILayout.Space(4.0f);

		EditorGUILayout.PropertyField (questObject, new GUIContent ("Quest Obj: ", "Check this on if the gate should open based on quest status 'Finished'"));

		GUILayout.Space(16.0f);


		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();


	}

}