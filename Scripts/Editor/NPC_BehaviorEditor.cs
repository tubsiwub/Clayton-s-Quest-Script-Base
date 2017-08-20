using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC_Behavior))]
public class NPC_BehaviorEditor : Editor {
	
	SerializedProperty NPCName;
	SerializedProperty lookAtObj;
	SerializedProperty destination;

	SerializedProperty npcType;
	SerializedProperty currentBehavior;
	SerializedProperty faceObj;

	SerializedProperty travelArea;
	SerializedProperty insideArea;


	// Motivation Timers
	SerializedProperty yearnOUTSIDE;	
	SerializedProperty yearnINSIDE;		
	SerializedProperty yearnREST;		
	SerializedProperty boredomTimer;	
	SerializedProperty disinterestSpeed;	
	SerializedProperty exhaustionSpeed;	
	SerializedProperty hysteriaSpeed;	
	SerializedProperty restSpeed;	
	SerializedProperty boredomBonus;	
	SerializedProperty boredomAttack;
	SerializedProperty satisfiedTimerDefault;		
	SerializedProperty satisfiedWithLocation;
	SerializedProperty sitting;
	SerializedProperty idling;



	void OnEnable(){

		NPCName = serializedObject.FindProperty ("NPCName");
		lookAtObj = serializedObject.FindProperty ("lookAtObj");
		destination = serializedObject.FindProperty ("destination");

		npcType = serializedObject.FindProperty ("npcType");
		currentBehavior = serializedObject.FindProperty ("currentBehavior");
		faceObj = serializedObject.FindProperty ("faceObj");

		travelArea = serializedObject.FindProperty ("travelArea");
		insideArea = serializedObject.FindProperty ("insideArea");

		yearnOUTSIDE = serializedObject.FindProperty ("yearnOUTSIDE");	
		yearnINSIDE = serializedObject.FindProperty ("yearnINSIDE");		
		yearnREST = serializedObject.FindProperty ("yearnREST");		
		boredomTimer = serializedObject.FindProperty ("boredomTimer");
		disinterestSpeed = serializedObject.FindProperty ("disinterestSpeed");	
		exhaustionSpeed = serializedObject.FindProperty ("exhaustionSpeed");	
		hysteriaSpeed = serializedObject.FindProperty ("hysteriaSpeed");
		restSpeed = serializedObject.FindProperty ("restSpeed");	
		boredomBonus = serializedObject.FindProperty ("boredomBonus");
		boredomAttack = serializedObject.FindProperty ("boredomAttack");
		satisfiedTimerDefault = serializedObject.FindProperty ("satisfiedTimerDefault");		
		satisfiedWithLocation = serializedObject.FindProperty ("satisfiedWithLocation");
		sitting = serializedObject.FindProperty ("sitting");
		idling = serializedObject.FindProperty ("idling");
	}

	public override void OnInspectorGUI(){

		// MUST INCLUDE
		serializedObject.Update ();

		EditorGUILayout.PropertyField (npcType, new GUIContent("NPC Type: ", "The NPC type."));

		EditorGUILayout.PropertyField (currentBehavior, new GUIContent("NPC Behavior: ", "Current Behavior of NPC."));

		EditorGUILayout.PropertyField (faceObj, new GUIContent("NPC Face Ref: ", "Place the NPC's face object that's located on the neck node here for a fast reference."));

		if (npcType.enumValueIndex == 0) {	//normal

			EditorGUILayout.PropertyField (travelArea, new GUIContent("Travel Area: "));
			EditorGUILayout.PropertyField (insideArea, new GUIContent("Inside Area: "));

			EditorGUILayout.PropertyField (destination, new GUIContent("Destination Obj: ", "Place the NPC's destination object here.  This is where the NPC will constantly walk towards."));

			EditorGUILayout.PropertyField (NPCName, new GUIContent ("NPC Name: ", "This is the NPC's name.  Shows up in the dialogue and is used as a reference for the NPC Manager when deciding which NPCs are shown in town, etc.  IMPORTANT."));

			GUILayout.Label ("yearnOUTSIDE " + yearnOUTSIDE.floatValue);
			GUILayout.Label ("yearnINSIDE " + yearnINSIDE.floatValue);	
			GUILayout.Label ("yearnREST " + yearnREST.floatValue);		
			GUILayout.Label ("boredomTimer " + boredomTimer.floatValue);	
			GUILayout.Label ("disinterestSpeed " + disinterestSpeed.floatValue);	
			GUILayout.Label ("disinterestSpeed " + exhaustionSpeed.floatValue);	
			GUILayout.Label ("hysteriaSpeed " + hysteriaSpeed.floatValue);	
			GUILayout.Label ("restSpeed " + restSpeed.floatValue);	
			GUILayout.Label ("boredomBonus " + boredomBonus.floatValue);	
			GUILayout.Label ("boredomAttack " + boredomAttack.floatValue);
			GUILayout.Label ("satisfiedTimerDefault " + satisfiedTimerDefault.floatValue);		
			GUILayout.Label ("satisfiedWithLocation " + satisfiedWithLocation.boolValue);
			GUILayout.Label ("sitting " + sitting.boolValue);
			GUILayout.Label ("idling " + idling.boolValue);
		}

		if (npcType.enumValueIndex == 1) {	// quest


			EditorGUILayout.PropertyField (lookAtObj, new GUIContent("Look At Obj: ", "Place the NPC's Look At object here.  The NPC will rotate toward this object when not talking to the player."));

			EditorGUILayout.PropertyField (NPCName, new GUIContent("NPC Name: ", "This is the NPC's name.  Shows up in the dialogue and is used as a reference for the NPC Manager when deciding which NPCs are shown in town, etc.  IMPORTANT."));

		}

		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();

	}

}