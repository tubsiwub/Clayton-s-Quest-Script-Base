using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Cutscene_CameraEventObjects))]
public class Cutscene_CameraEventObjectsEditor : Editor {

	Cutscene_CameraEventObjects cameraEventScript;
	List<GameObject> componentsList;

	void OnEnable(){

		cameraEventScript = target as Cutscene_CameraEventObjects;
		componentsList = cameraEventScript.sceneObjects;

	}

	public override void OnInspectorGUI(){

		GUILayout.Space(24.0f);

		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);


		#region TUTORIAL

		EditorGUILayout.LabelField("TUTORIAL", titleStyle);

		GUILayout.Space(12.0f);

		cameraEventScript.tutorial = EditorGUILayout.Foldout (cameraEventScript.tutorial, new GUIContent(" Tutorial", "Check this to view information regarding this component"));

		if (cameraEventScript.tutorial) {

			string tutorial = "";
			tutorial += "Cutscene Camera Event Tutorial -\n\n";
			tutorial += "This script must be placed on any camera used in a cutscene.\n\n";
			tutorial += "The camera may contain the Cutscene Objects used in the cutscene.\n\n";
			tutorial += "All objects attached to a camera will be activated during that camera's scene.\n\n";
			tutorial += "If no camera utilizes a specific object, it won't get activated.\n\n";
			tutorial += "Camera's may also have scene audio.\n\n";
			tutorial += "This audio will play on top of the cutscene audio, overlapping.\n\n";
			tutorial += "Camera audio can be used for scene effects or dialogue.\n\n";
			tutorial += "Camera audio delay is used to delay the start of the scene audio.\n\n";
			tutorial += "Important to note:  If a puzzle button activates a cutscene, ";
			tutorial += "then it's a must that the letterboxes attached to the camera have a reference to the button.\n\n";

			EditorGUILayout.HelpBox(tutorial, MessageType.None, true);

		}

		GUILayout.Space(24.0f);

		#endregion


		#region SCENE OBJECTS

		EditorGUILayout.LabelField("CUTSCENE COMPONENTS", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.LabelField("Objects used only in the first scene / cam");

		GUILayout.Space(6.0f);

		GUILayout.BeginHorizontal ("Box");

		if (componentsList.Count <= 8) {
			if (GUILayout.Button ("Add New Component")) {
				componentsList.Add (null);
			}
		}

		if (componentsList.Count > 0) {
			if (GUILayout.Button ("Remove Last")) {
				componentsList.Remove (componentsList [componentsList.Count - 1]);
			}
		}

		GUILayout.EndHorizontal ();

		// If list is full, display text
		if(componentsList.Count > 8)
			EditorGUILayout.LabelField("Component List is Full");


		if (componentsList.Count > 0)
			GUILayout.BeginVertical("Box");

		for (int i = 0; i < componentsList.Count; i++) {
			componentsList[i] = (GameObject)EditorGUILayout.ObjectField (componentsList[i], typeof(GameObject), true);
		}

		if (componentsList.Count > 0)
			GUILayout.EndVertical();


		// Set actual list to the new list (update list)
		cameraEventScript.sceneObjects = componentsList;

		#endregion


		GUILayout.Space(24.0f);


		#region CUTSCENE AUDIO

		EditorGUILayout.LabelField("CUTSCENE AUDIO", titleStyle);

		GUILayout.Space(12.0f);

		// AUDIO
		cameraEventScript.useSpecificAudio = GUILayout.Toggle (cameraEventScript.useSpecificAudio, new GUIContent(" - Use Scene-Specific Audio?", "Check this if you want some layered audio for this scene."));

		if (cameraEventScript.useSpecificAudio) {

			cameraEventScript.specificAudio = (AudioClip)EditorGUILayout.ObjectField (cameraEventScript.specificAudio, typeof(AudioClip), true);

			cameraEventScript.audioDelay = EditorGUILayout.FloatField(new GUIContent("Audio Delay: ", "How long into the animation before audio starts playing."), cameraEventScript.audioDelay);

			cameraEventScript.audioVolume = EditorGUILayout.IntSlider ("Volume: ", cameraEventScript.audioVolume, 0, 100);

		}

		#endregion


		GUILayout.Space(24.0f);


	}

}