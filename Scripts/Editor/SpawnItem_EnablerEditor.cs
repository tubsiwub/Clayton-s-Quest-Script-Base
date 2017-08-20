using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnItem_Enabler))]
public class SpawnItem_EnablerEditor : Editor {


	SerializedProperty spawnType;

	SerializedProperty puzzleButton;
	SerializedProperty questNPC;

	SerializedProperty useWinZone;
	SerializedProperty useCutscene;

	SerializedProperty winZone;

	SerializedProperty itemPermanence;

	SerializedProperty cutscene;


	void OnEnable(){

		spawnType = serializedObject.FindProperty ("spawnType");

		puzzleButton = serializedObject.FindProperty ("puzzleButton");
		questNPC = serializedObject.FindProperty ("questNPC");

		useWinZone = serializedObject.FindProperty ("useWinZone");
		useCutscene = serializedObject.FindProperty ("useCutscene");

		winZone = serializedObject.FindProperty ("winZone");

		itemPermanence = serializedObject.FindProperty ("itemPermanence");

		cutscene = serializedObject.FindProperty ("cutscene");

	}

	public override void OnInspectorGUI(){

		// MUST INCLUDE
		serializedObject.Update ();

		GUILayout.Space (6);

		EditorGUILayout.PropertyField (spawnType, new GUIContent ("Spawn type: "));

		GUILayout.Space (6);

		if (spawnType.enumValueIndex == 0) {

		}

		if (spawnType.enumValueIndex == 1) {

			EditorGUILayout.PropertyField (useCutscene, new GUIContent ("Use Cutscene: "));

			if(useCutscene.boolValue)
				EditorGUILayout.PropertyField (cutscene, new GUIContent ("Cutscene: "));

			EditorGUILayout.PropertyField (puzzleButton, new GUIContent ("Puzzle Button: "));
			questNPC.objectReferenceValue = null;
		}

		if (spawnType.enumValueIndex == 2) {
			EditorGUILayout.PropertyField (questNPC, new GUIContent ("Quest NPC: "));
			puzzleButton.objectReferenceValue = null;
		}

		EditorGUILayout.PropertyField (itemPermanence, new GUIContent ("Item Perm?"));

		if (!itemPermanence.boolValue) {

			EditorGUILayout.PropertyField (useWinZone, new GUIContent ("Win Zone?"));

		} else {

			useWinZone.boolValue = false;

			EditorGUILayout.HelpBox ("With Item Permanence toggled, a Win Zone would have no effect on spawned items.", MessageType.None);

		}

		if (useWinZone.boolValue)
			EditorGUILayout.PropertyField (winZone, new GUIContent ("Win Zone:"));
		else
			winZone.objectReferenceValue = null; 

		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();

	}

}