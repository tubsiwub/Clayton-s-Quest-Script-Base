using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScriptStateManager))]
public class ScriptStateManagerEditor : Editor {

	SerializedProperty StateType;

	SerializedProperty cutsceneCameras;
	SerializedProperty cutsceneComponents;
	SerializedProperty cameraAnimationDurations;

	SerializedProperty puzzleButton;
	SerializedProperty triggerBox;
	SerializedProperty punchBox;

	SerializedProperty useBackgroundAudio;
	SerializedProperty tutorial;
	SerializedProperty hasButton;
	SerializedProperty hasTrigger;
	SerializedProperty hasPunch;

	SerializedProperty cutsceneBackgroundAudio;
	SerializedProperty CBAVolume;

	void OnEnable(){

		StateType = serializedObject.FindProperty ("StateType");

		cutsceneCameras = serializedObject.FindProperty ("cutsceneCameras");
		cutsceneComponents = serializedObject.FindProperty ("cutsceneComponents");
		cameraAnimationDurations = serializedObject.FindProperty ("cameraAnimationDurations");

		puzzleButton = serializedObject.FindProperty ("puzzleButton");
		triggerBox = serializedObject.FindProperty ("triggerBox");
		punchBox = serializedObject.FindProperty ("punchBox");

		useBackgroundAudio = serializedObject.FindProperty ("useBackgroundAudio");
		tutorial = serializedObject.FindProperty ("tutorial");
		hasButton = serializedObject.FindProperty ("hasButton");
		hasTrigger = serializedObject.FindProperty ("hasTrigger");
		hasPunch = serializedObject.FindProperty ("hasPunch");

		cutsceneBackgroundAudio = serializedObject.FindProperty ("cutsceneBackgroundAudio");
		CBAVolume = serializedObject.FindProperty ("CBAVolume");

	}

	public override void OnInspectorGUI(){

		// MUST INCLUDE
		serializedObject.Update ();

		GUILayout.Space(24.0f);

		EditorGUILayout.PropertyField (StateType, new GUIContent ("State: "));

		GUILayout.Space(24.0f);

		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);

		if (StateType.enumValueIndex == 0) {

			#region TUTORIAL

			EditorGUILayout.LabelField("TUTORIAL", titleStyle);

			GUILayout.Space(12.0f);

			EditorGUILayout.PropertyField(tutorial, new GUIContent("Tutorial?"));

			if (tutorial.boolValue) {

				string tutorial = "";
				tutorial += "Script State Manager Tutorial -"; 																													tutorial += "\n\n";
				tutorial += "This cutscene manager works by utilizing animations applied to a series of cameras.";  															tutorial += "\n\n";
				tutorial += "Using how it is setup (default state), you can add additional cameras by duplicating the CutsceneCamera object."; 									tutorial += "\n\n";
				tutorial += "For each camera, create an Animation using Unity's Mechanim system."; 																				tutorial += "\n\n";
				tutorial += "The cutscene manager will play each animation for each camera in order, moving on to the next camera only when the first camera has finished."; 	tutorial += "\n\n";
				tutorial += "For every camera added to the cutscene manager, a new Animation Duration option fills itself in."; 												tutorial += "\n\n";
				tutorial += "You can match the animation lengths for each duration, or you can give them some extra time as pause."; 											tutorial += "\n\n";
				tutorial += "This isn't magic, though!  Plan your animations accordingly."; 	 																				tutorial += "\n\n";
				tutorial += "Using the duration list, it's possible to loop the same camera animation more than once.";  														tutorial += "\n\n";
				tutorial += "The cutscene components list contains all interactive cutscene objects that will do something during the cutscene.";  								tutorial += "\n\n";
				tutorial += "(See 'Cutscene_CameraEventObjects' on one of the cameras to learn more about these components)";  													tutorial += "\n\n";
				tutorial += "The cutscene audio section allows for background audio to play for the duration of the entire cutscene."; 											tutorial += "\n\n";
				tutorial += "Script State Manager for Cutscenes requires these scripts:"; 																						tutorial += "\n\n";
				tutorial += "Cutscene_CameraEventObjects.cs on Cam";  																											tutorial += "\n\n";
				tutorial += "Cutscene_TriggerScript on CutsceneTrigger";																										tutorial += "\n\n";
				tutorial += "Cutscene_LetterboxScreenFit on Letterbox_Top and Bottom";  																						tutorial += "\n\n";
				tutorial += "CutsceneComponent on all interactive CutsceneObjects";  																							tutorial += "\n\n";
				tutorial += "MovingPlatform_Actions on any moving platform CutsceneObjects for cutscene"; 
				EditorGUILayout.HelpBox(tutorial, MessageType.None, true);

			}

			GUILayout.Space(24.0f);

			#endregion


			#region CUTSCENE CAMERAS

			EditorGUILayout.LabelField("CUTSCENE CAMERAS", titleStyle);

			GUILayout.Space(12.0f);

			EditorGUILayout.LabelField("Cameras used in cutscene:");
			EditorGUILayout.LabelField(" - In Order");

			GUILayout.Space(6.0f);

			GUILayout.BeginHorizontal ("Box");

			if (cutsceneCameras.arraySize <= 5) {
				if (GUILayout.Button ("Add New Camera")) {
					cutsceneCameras.arraySize += 1;
					cutsceneCameras.GetArrayElementAtIndex(cutsceneCameras.arraySize-1).objectReferenceValue = null;
					serializedObject.ApplyModifiedProperties ();
				}
			}

			if (cutsceneCameras.arraySize > 1) {
				if (GUILayout.Button ("Remove Last")) {
					cutsceneCameras.arraySize -= 1;
					serializedObject.ApplyModifiedProperties ();
				}
			}

			GUILayout.EndHorizontal ();

			// If list is full, display text
			if(cutsceneCameras.arraySize > 5)
				EditorGUILayout.LabelField("Camera List is Full");


			GUILayout.BeginVertical("Box");

			for (int i = 0; i < cutsceneCameras.arraySize; i++) {
				EditorGUILayout.PropertyField(cutsceneCameras.GetArrayElementAtIndex(i), new GUIContent("Cam: "));
			}

			GUILayout.EndVertical();


			#endregion


			GUILayout.Space(24.0f);


			#region CUTSCENE DURATIONS

			EditorGUILayout.LabelField("ANIMATION DURATIONS", titleStyle);

			GUILayout.Space(12.0f);

			EditorGUILayout.LabelField("Duration for each camera animation:");
			EditorGUILayout.LabelField(" - In Order");

			GUILayout.Space(6.0f);

			GUILayout.BeginVertical("Box");



			if(cameraAnimationDurations.arraySize != cutsceneCameras.arraySize){

				if(cutsceneCameras.arraySize > cameraAnimationDurations.arraySize){
					cameraAnimationDurations.arraySize += 1;
					cameraAnimationDurations.GetArrayElementAtIndex(cameraAnimationDurations.arraySize-1).floatValue = 0;
				}

				if(cutsceneCameras.arraySize < cameraAnimationDurations.arraySize){
					cameraAnimationDurations.arraySize -= 1;
				}

				serializedObject.ApplyModifiedProperties ();

			}



			for (int i = 0; i < cameraAnimationDurations.arraySize; i++) {
				EditorGUILayout.PropertyField(cameraAnimationDurations.GetArrayElementAtIndex(i), new GUIContent("Duration " + (i+1) + ": "));
			}



			GUILayout.EndVertical();

			#endregion


			GUILayout.Space(24.0f);


			#region CUTSCENE COMPONENTS

			EditorGUILayout.LabelField("CUTSCENE COMPONENTS", titleStyle);

			GUILayout.Space(12.0f);

			EditorGUILayout.LabelField("Objects used in cutscene");

			GUILayout.Space(6.0f);

			GUILayout.BeginHorizontal ("Box");

			if (cutsceneComponents.arraySize <= 8) {
				if (GUILayout.Button ("Add New Component")) {
					cutsceneComponents.arraySize += 1;
				}
			}

			if (cutsceneComponents.arraySize > 0) {
				if (GUILayout.Button ("Remove Last")) {
					cutsceneComponents.arraySize -= 1;
				}
			}

			GUILayout.EndHorizontal ();

			// If list is full, display text
			if(cutsceneComponents.arraySize > 8)
				EditorGUILayout.LabelField("Component List is Full");

			if (cutsceneComponents.arraySize > 0)
				GUILayout.BeginVertical("Box");

			for (int i = 0; i < cutsceneComponents.arraySize; i++) {
				EditorGUILayout.PropertyField(cutsceneComponents.GetArrayElementAtIndex(i),  new GUIContent("Comp: "));
			}

			if (cutsceneComponents.arraySize > 0)
				GUILayout.EndVertical();

			#endregion


			GUILayout.Space(24.0f);


			#region CUTSCENE AUDIO

			EditorGUILayout.LabelField("CUTSCENE AUDIO", titleStyle);

			GUILayout.Space(12.0f);

			// AUDIO
			EditorGUILayout.PropertyField(useBackgroundAudio, new GUIContent("BG Audio?"));

			if (useBackgroundAudio.boolValue) {

				EditorGUILayout.PropertyField(cutsceneBackgroundAudio, new GUIContent("BG Audio: "));

				EditorGUILayout.PropertyField(CBAVolume, new GUIContent("Volume: "));

			}
			else {

				cutsceneBackgroundAudio.objectReferenceValue = null;

			}

			#endregion

			GUILayout.Space(24.0f);


			#region ADDITIONAL OBJECTS

			EditorGUILayout.LabelField("ADDITIONAL OBJECTS", titleStyle);

			GUILayout.Space(12.0f);

			// BUTTON
			EditorGUILayout.PropertyField(hasButton, new GUIContent("Puzzle Button?"));

			if (hasButton.boolValue) {

				EditorGUILayout.PropertyField(puzzleButton, new GUIContent("Puzzle Button: "));

			}
			else {

				puzzleButton.objectReferenceValue = null;

			}

			GUILayout.Space(12.0f);

			// PUNCH BOX
			EditorGUILayout.PropertyField(hasPunch, new GUIContent("Punchbox?"));

			if (hasPunch.boolValue) {

				EditorGUILayout.PropertyField(punchBox , new GUIContent("Punch Box: "));

			}
			else {

				punchBox.objectReferenceValue = null;

			}

			GUILayout.Space(12.0f);

			// TRIGGER
			EditorGUILayout.PropertyField(hasTrigger, new GUIContent("Trigger?"));

			if (hasTrigger.boolValue) {

				EditorGUILayout.PropertyField(triggerBox, new GUIContent("Trigger Box: "));

			}
			else {

				triggerBox.objectReferenceValue = null;

			}

			#endregion

			GUILayout.Space(24.0f);


			// MUST INCLUDE
			serializedObject.ApplyModifiedProperties ();

		}

	}

}