using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlatformMovement_Oscillating))]
[CanEditMultipleObjects]
public class PlatformMovement_OscillatingEditor : Editor {

	SerializedProperty pointA;

	SerializedProperty pointB;

	SerializedProperty moveSpeed;

	void OnEnable(){

		pointA = serializedObject.FindProperty ("pointA");
		pointB = serializedObject.FindProperty ("pointB");
		moveSpeed = serializedObject.FindProperty ("moveSpeed");

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

		EditorGUILayout.HelpBox ("Object moves back and forth between values; works off of the Y value.", MessageType.Info);

		GUILayout.Space(8.0f);

		EditorGUILayout.LabelField("PLATFORM:  OSCILLATING", titleStyle);

		PlatformMovement_Oscillating mainScript = target as PlatformMovement_Oscillating;

		GUILayout.Space(16.0f);

		if (GUILayout.Button ("Set Pos: ")) {mainScript.pointA = mainScript.transform.localPosition;}
		EditorGUILayout.PropertyField (pointA);

		GUILayout.Space(8.0f);

		if (GUILayout.Button ("Set Pos: ")) {mainScript.pointB = mainScript.transform.localPosition;}
		EditorGUILayout.PropertyField (pointB);

		GUILayout.Space(8.0f);

		EditorGUILayout.PropertyField (moveSpeed, new GUIContent ("Move Speed: "));


		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();


	}

}