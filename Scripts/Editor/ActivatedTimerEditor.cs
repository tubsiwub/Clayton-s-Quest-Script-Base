using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActivatedTimer))]
public class ActivatedTimerEditor : Editor {

	ActivatedTimer mainScript;

	SerializedProperty tutorial;
	SerializedProperty usesCutscene;

	SerializedProperty winZone;
	SerializedProperty puzzleButton;
	SerializedProperty questNPC;
	SerializedProperty triggerObj;
	SerializedProperty startZone;
	SerializedProperty scriptStateManager;

	SerializedProperty timeDefault;


	void OnEnable(){

		mainScript = target as ActivatedTimer;

		tutorial = serializedObject.FindProperty ("tutorial");
		usesCutscene = serializedObject.FindProperty ("usesCutscene");

		winZone = serializedObject.FindProperty ("winZone");
		puzzleButton = serializedObject.FindProperty ("puzzleButton");
		questNPC = serializedObject.FindProperty ("questNPC");
		triggerObj = serializedObject.FindProperty ("triggerObj");
		startZone = serializedObject.FindProperty ("startZone");
		scriptStateManager = serializedObject.FindProperty ("scriptStateManager");

		timeDefault = serializedObject.FindProperty ("timeDefault");

	}

	public override void OnInspectorGUI(){


		// MUST INCLUDE
		serializedObject.Update ();


		GUILayout.Space(24.0f);

		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);

		TIMERTYPE TimerType = (TIMERTYPE)mainScript.timerType;

		#region TUTORIAL

		EditorGUILayout.LabelField("TUTORIAL", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField(tutorial, new GUIContent("Tutorial: "));

		if (mainScript.tutorial) {

			string tutorial = "";
			tutorial += "Activated Timer Tutorial -  \n\n"; 	
			tutorial += "The timer component has multiple versions.  "; 	
			tutorial += "Choose the version you want to use in the dropdown list.\n\n"; 	
			tutorial += "Depending on the version chosen, new options and setting will appear below.\n\n"; 	
			tutorial += "BUTTON - \n\n"; 	
			tutorial += "Giving the timer a button reference, the timer will wait for the button to get pushed before enabling itself.  "; 	
			tutorial += "When the timer runs out, the events the button put into effect will turn off or reset.  "; 	
			tutorial += "Button events and event objects will most likely require a timer reference.  Make sure you read the used components for more information.  "; 	
			tutorial += "BOWLING - \n\n"; 	
			tutorial += "Giving the timer a bowling reference, the timer will wait for the bowling event to start.  "; 	
			tutorial += "A bowling event starts once the player walks into the start trigger and ends once the player touches a pin.  "; 	
			tutorial += "The timer will force-reset the bowling event should it be allowed to run out.\n\n"; 	
			EditorGUILayout.HelpBox(tutorial, MessageType.None, true);

		}

		GUILayout.Space(24.0f);

		#endregion



		#region Actions

		EditorGUILayout.LabelField("VERSIONS", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.HelpBox("Specific usage of the timer relies on which version is used.",MessageType.None);

		GUILayout.Space(12.0f);

		mainScript.timerType = (TIMERTYPE)EditorGUILayout.EnumPopup(new GUIContent("TImer Version: ", "This determines what the timer will be used for."), mainScript.timerType);

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField(winZone, new GUIContent("Win Zone: ", "Win zone cancels the timer"));

		GUILayout.Space(12.0f);

		if(TimerType == TIMERTYPE.QUEST){

			EditorGUILayout.PropertyField(questNPC, new GUIContent("Quest NPC: ", "The puzzle button used to activate timer events."));

			startZone.objectReferenceValue = null;
			triggerObj.objectReferenceValue = null;
			puzzleButton.objectReferenceValue = null;

		}

		if(TimerType == TIMERTYPE.BUTTON){

			EditorGUILayout.PropertyField(puzzleButton, new GUIContent("Button: ", "The puzzle button used to activate timer events."));

			startZone.objectReferenceValue = null;
			triggerObj.objectReferenceValue = null;
			questNPC.objectReferenceValue = null;

		}

		if(TimerType == TIMERTYPE.BOWLING){

			EditorGUILayout.PropertyField(startZone, new GUIContent("Start Zone: ", "The area object containing the trigger to start the bowling events."));

			questNPC.objectReferenceValue = null;
			puzzleButton.objectReferenceValue = null;
			triggerObj.objectReferenceValue = null;

		}

		if(TimerType == TIMERTYPE.TRIGGER){

			EditorGUILayout.PropertyField(triggerObj, new GUIContent("Trigger Obj: ", "Trigger box that starts the quest."));

			questNPC.objectReferenceValue = null;
			puzzleButton.objectReferenceValue = null;
			startZone.objectReferenceValue = null;

		}

		EditorGUILayout.PropertyField(usesCutscene, new GUIContent("Uses cutscene?", "Check this if the timer should wait for a cutscene to finish."));

		if(mainScript.usesCutscene){

			EditorGUILayout.PropertyField(scriptStateManager, new GUIContent("Cutscene Manager: ", "The cutscene manager that provides data as to whether the cutscene is in progress or not."));

		}
		else mainScript.scriptStateManager = null;

		EditorGUILayout.PropertyField(timeDefault, new GUIContent("Time: ", "Timer start time - Resets and Starts using this time (in seconds)"));
		 
		#endregion


		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();


	}

}