using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Platform_Movement))]
[CanEditMultipleObjects]
public class Platform_MovementEditor : Editor {

	Platform_Movement mainScript;

	List<Transform> positionsList;

	SerializedProperty startType;

	SerializedProperty  tutorial;

	SerializedProperty  looping; 
	SerializedProperty  lerping;
	SerializedProperty  pausing;

	SerializedProperty  reversing;
//	SerializedProperty  startOnLoad;

	SerializedProperty  puzzleButton;

	SerializedProperty  moveSpeed;

	SerializedProperty  pauseTime;

	SerializedProperty startPauseTime;
	SerializedProperty waitDelay;

//	SerializedProperty Positions;
	SerializedProperty startingPosition;



	void OnEnable(){

		mainScript = target as Platform_Movement;
		positionsList = mainScript.Positions;

		startType = serializedObject.FindProperty ("startType");

		tutorial = serializedObject.FindProperty ("tutorial");
	
		looping = serializedObject.FindProperty ("looping");
		lerping = serializedObject.FindProperty ("lerping");
		pausing = serializedObject.FindProperty ("pausing");
	
		reversing = serializedObject.FindProperty ("reversing");
//		startOnLoad = serializedObject.FindProperty ("startOnLoad");
	
		puzzleButton = serializedObject.FindProperty ("puzzleButton");
	
		moveSpeed = serializedObject.FindProperty ("moveSpeed");
	
		pauseTime = serializedObject.FindProperty ("pauseTime");
	
		startPauseTime = serializedObject.FindProperty ("startPauseTime");
		waitDelay = serializedObject.FindProperty ("waitDelay");
	
//		Positions = serializedObject.FindProperty ("Positions");
		startingPosition = serializedObject.FindProperty ("startingPosition");
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

		GUIStyle infoStyle = new GUIStyle();
		infoStyle.fontSize = 14;
		infoStyle.fontStyle = FontStyle.Bold;
		infoStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		infoStyle.alignment = TextAnchor.MiddleCenter;
		infoStyle.normal.textColor = new Color (0.95f, 0.3f, 0.1f);


		if (mainScript.saveLoad == null)
			mainScript.saveLoad = mainScript.GetComponent<SavingLoading_StorageKeyCheck> ();


		#region TUTORIAL
	
		EditorGUILayout.LabelField("TUTORIAL", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField(tutorial, new GUIContent("Tutorial: "));

		if (tutorial.boolValue) {

			string tutorial = "";
			tutorial += "Platform Movement Tutorial -"; 																				tutorial += "\n\n";
			tutorial += "Mouse over each item below to see a brief description."; 														tutorial += "\n\n";
			tutorial += "Platform control type defines how the platform will start moving.  "; 											
			tutorial += "This can be player-initiated in some way, or automatic."; 														tutorial += "\n\n";
			tutorial += "There is a max number of positions that the platform can move between.  "; 									
			tutorial += "The platform will begin moving from wherever it is toward the first position, "; 								
			tutorial += "then it will proceed to each position in order based on the rules set below."; 								
			EditorGUILayout.HelpBox(tutorial, MessageType.None, true);

		}

		#endregion

		GUILayout.Space(24.0f);


		#region PLATFORM POSITIONS

		EditorGUILayout.LabelField("PLATFORM POSITIONS", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.HelpBox("Positions that the platform will move between.", MessageType.None);

		GUILayout.Space(6.0f);

		GUILayout.BeginHorizontal ("Box");

		if (positionsList.Count <= 20) {
			if (GUILayout.Button ("Add New Position")) {
				positionsList.Add (null);
			}
		}

		if (positionsList.Count > 0) {
			if (GUILayout.Button ("Remove Last")) {
				positionsList.Remove (positionsList [positionsList.Count - 1]);
			}
		}

		GUILayout.EndHorizontal ();

		// If list is full, display text
		if(positionsList.Count > 20)
			EditorGUILayout.LabelField("Position List is Full");


		GUILayout.BeginVertical("Box");

		if (positionsList.Count > 0){
			for (int i = 0; i < positionsList.Count; i++) {

				GUILayout.BeginHorizontal ("Box");

				// Click to set the position of the input gameObject
				if (GUILayout.Button ("Set Pos")) {
					positionsList[i].transform.position = mainScript.transform.position;
				}

				// gameObject that contains the exact position that the platform will move to
				positionsList[i] = (Transform)EditorGUILayout.ObjectField (positionsList[i], typeof(Transform), true);

				GUILayout.EndHorizontal ();

			}
		}

		GUILayout.EndVertical();


		// Set actual list to the new list (update list)
		mainScript.Positions = positionsList;


		GUILayout.Space(12.0f);

		// #Seg - Creates a list based on position names using an alphabet to let devs alteer starting platform destination
		GUIContent[] startingPositionValues = new GUIContent[positionsList.Count];
		int[] optionValues = new int[positionsList.Count];

		for (int i = 0; i < positionsList.Count; i++) {

			startingPositionValues[i] = new GUIContent("Position " + ((ALPHABET)i).ToString());
			optionValues[i] = i;

		}

		EditorGUILayout.PropertyField(startingPosition, new GUIContent("Starting Position: "));
		// #Seg - end

		#endregion


		GUILayout.Space(24.0f);


		#region PLATFORM CONTROL

		EditorGUILayout.LabelField("PLATFORM CONTROL", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField(moveSpeed, new GUIContent("Move Speed: "));

		GUILayout.Space(12.0f);

		// Look at the states and list them in a dropdown, then allow the user to set the state for the platform
		string[] enumType = new string[(int)PLATFORMTYPE.COUNT];
		for(int i = 0; i < (int)PLATFORMTYPE.COUNT; i++) { enumType[i] = ((PLATFORMTYPE)i).ToString(); }
		EditorGUILayout.PropertyField(startType, new GUIContent("Start Type: "));

		// Show only relevant controls for selected type
		switch(startType.enumValueIndex){

		case 1:

			EditorGUILayout.HelpBox("Platform will automatically start moving with current rules.", MessageType.None);
			puzzleButton.objectReferenceValue = null;

			break;

		case 0:

			EditorGUILayout.PropertyField(puzzleButton, new GUIContent("Puzzle Button: "));

			break;

		case 2:

			break;

		case 3:

			EditorGUILayout.HelpBox("Platform will wait for the player to get on.  If the player leaves, the platform will reset after a time.", MessageType.None);
			puzzleButton.objectReferenceValue = null;

			EditorGUILayout.PropertyField(waitDelay, new GUIContent("Wait Delay: ", "The time before a Wait platform disappears after the player exits."));
			break;


		} 

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField(looping, new GUIContent("Looping: "));

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField(lerping, new GUIContent("Lerping: "));

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField(pausing, new GUIContent("Pausing: "));

		if(mainScript.pausing) {

			EditorGUILayout.PropertyField(pauseTime, new GUIContent("Pause Time: "));

			EditorGUILayout.PropertyField(startPauseTime, new GUIContent("Start Pause Time: "));

		}

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField(reversing, new GUIContent("Reversing: "));

		#endregion


		GUILayout.Space(24.0f);


		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();

	}

}