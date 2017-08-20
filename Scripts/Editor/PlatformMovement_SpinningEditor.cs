using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlatformMovement_Spinning))]
[CanEditMultipleObjects]
public class PlatformMovement_SpinningEditor : Editor {

	SerializedProperty spinDirection;

	void OnEnable(){

		spinDirection = serializedObject.FindProperty ("spinDirection");

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

		EditorGUILayout.HelpBox ("Object spins in place in the given direction (and speed).", MessageType.Info);

		GUILayout.Space(8.0f);

		EditorGUILayout.LabelField("PLATFORM:  SPINNING", titleStyle);

		GUILayout.Space(16.0f);

		EditorGUILayout.PropertyField (spinDirection, new GUIContent ("Spin Direction: "));


		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();


	}

}