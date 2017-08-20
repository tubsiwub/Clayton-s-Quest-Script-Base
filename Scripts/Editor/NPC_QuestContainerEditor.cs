using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(NPC_QuestContainer))]
public class NPC_QuestContainerEditor : Editor {


	SerializedProperty dialogueType;
	SerializedProperty simpleShift;

	SerializedProperty specificQuest;
	SerializedProperty rewardObject;
	SerializedProperty rewardAmount;
	SerializedProperty useTimer;
	SerializedProperty timerTime;
	SerializedProperty checkpoint;

	SerializedProperty startingDialogue;
	SerializedProperty questStartedDialogue;
	SerializedProperty questFailedDialogue;
	SerializedProperty questCompleteDialogue; 
	SerializedProperty endingDialogue; 


	void OnEnable(){
		dialogueType = serializedObject.FindProperty ("dialogueType");
		simpleShift = serializedObject.FindProperty ("simpleShift");

		specificQuest = serializedObject.FindProperty ("specificQuest");
		rewardObject = serializedObject.FindProperty ("rewardObject");
		rewardAmount = serializedObject.FindProperty ("rewardAmount");
		useTimer = serializedObject.FindProperty ("useTimer");
		timerTime = serializedObject.FindProperty ("timerTime");
		checkpoint = serializedObject.FindProperty ("checkpoint");

		startingDialogue = serializedObject.FindProperty ("startingDialogue");
		questStartedDialogue = serializedObject.FindProperty ("questStartedDialogue");
		questFailedDialogue = serializedObject.FindProperty ("questFailedDialogue");
		questCompleteDialogue = serializedObject.FindProperty ("questCompleteDialogue");
		endingDialogue = serializedObject.FindProperty ("endingDialogue");
	}

	public override void OnInspectorGUI(){

		// MUST INCLUDE
		serializedObject.Update ();

		NPC_QuestContainer mainScript = target as NPC_QuestContainer;


		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);


		GUILayout.Space(12.0f);

		EditorGUILayout.LabelField("Quest Status: " + mainScript.QuestStatus.ToString());

		GUILayout.Space(12.0f);

		EditorGUILayout.LabelField("NPC QUEST SETTINGS", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField (simpleShift, new GUIContent ("Simple Shift: ", "Toggle this on if you want to simply move a reward object to the player instead of instantiating a new one."));

		EditorGUILayout.PropertyField(specificQuest, new GUIContent ("Quest Obj: ", "Please place your quest here that the NPC will be giving to the player.  Thank you."));

		EditorGUILayout.PropertyField (dialogueType, new GUIContent("Dialogue Object: ", "Located deeper within the NPC hierarchy.  'InteractObject' containing the correct script."));

		EditorGUILayout.PropertyField (rewardObject, new GUIContent("Reward Object: ", "A prefab of the reward that gets spawned.  Usually a marble."));

		EditorGUILayout.PropertyField (rewardAmount, new GUIContent ("Reward Amount: ", "Specifically marbles.  This is how many marbles are given to the player when a quest is completed."));

		EditorGUILayout.PropertyField (useTimer, new GUIContent ("Use Timer: "));

		if (useTimer.boolValue) {
			EditorGUILayout.PropertyField (timerTime, new GUIContent ("Timer Amt: ", "The time the timer starts at should a timer be used."));
			EditorGUILayout.PropertyField (checkpoint, new GUIContent ("Checkpoint: ", "If you use a specific checkpoint for this quest as a respawn location, set it here."));
		}

		if (mainScript.specificQuest != null)
		if (!mainScript.specificQuest.GetComponent<NPC_QuestType> ()) {
			mainScript.specificQuest = null;
			Debug.LogError ("Quest Object must have component: <b>NPC_QuestType</b> attached.", mainScript.gameObject);
		}

		GUILayout.Space(12.0f);

		titleStyle.fontSize = 16;
		EditorGUILayout.LabelField("DIALOGUE SETTINGS", titleStyle);

		GUILayout.Space(6.0f);

		titleStyle.fontSize = 13;
		EditorGUILayout.LabelField("( XML FILE NAMES )", titleStyle);

		GUILayout.Space(12.0f);


		EditorGUILayout.PropertyField (startingDialogue, new GUIContent ("Quest Initial: ", "Dialogue explaining the quest to the player."));

		EditorGUILayout.PropertyField (questStartedDialogue, new GUIContent ("Quest Started: ", "Dialogue for once a quest is started"));

		EditorGUILayout.PropertyField (questFailedDialogue, new GUIContent ("Quest Failed: ", "Dialogue presented instead if the player fails once."));

		EditorGUILayout.PropertyField (questCompleteDialogue, new GUIContent ("Quest Complete: ", "When the quest has been completed, talking to the NPC will display this text."));

		EditorGUILayout.PropertyField (endingDialogue, new GUIContent ("After Quest: ", "Quest is done, NPC has no more functionality... this is the dialogue text that is displayed."));

		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();

	}	// OnInspectorGUI
}	// Editor
#endif