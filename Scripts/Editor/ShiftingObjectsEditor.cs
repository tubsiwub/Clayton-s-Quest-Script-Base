using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShiftingObjects))]
public class ShiftingObjectsEditor : Editor {

	public override void OnInspectorGUI(){

		ShiftingObjects mainScript = target as ShiftingObjects;

		GUILayout.Space(24.0f);

		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);

		mainScript.ACTIVE = EditorGUILayout.Toggle ("ACTIVE: ", mainScript.ACTIVE);

		#region TUTORIAL

		EditorGUILayout.LabelField("TUTORIAL", titleStyle);

		GUILayout.Space(12.0f);

		string tutorial = "";
		mainScript.tutorial = EditorGUILayout.Foldout(mainScript.tutorial, "Expand", true);

		if (mainScript.tutorial) {

			tutorial += "Shifting Objects Tutorial - \n\n"; 			
			tutorial += "Summary:  The basic idea behind the Shifting Objects component is to allow "; 			
			tutorial += "the user a way to set up objects sequentially that can interact with one another.\n\n"; 			
			tutorial += "Usage:  The first object in a chain of reactions needs to have the 'FIRST' option selected. "; 			
			tutorial += "All other objects in a chain do not.  Any objects within the same chain will need to set "; 			
			tutorial += "the 'FIRST' object as their 'LISTENER'.\n\n";	
			tutorial += "Now once an object's conditions are met, the next object will be active.";	
			EditorGUILayout.HelpBox(tutorial, MessageType.None, true);

		}

		#endregion




		GUILayout.Space(12.0f);





		#region INTERACTION

		EditorGUILayout.LabelField("INTERACTION", titleStyle);

		GUILayout.Space(18.0f);

		mainScript.FIRST = EditorGUILayout.Toggle(new GUIContent("FIRST: ", "If this is the first object in a chain, check this."), mainScript.FIRST);

		GUILayout.Space(12.0f);

		mainScript.multipleListeners = EditorGUILayout.Toggle(new GUIContent("Mult. Listeners: ","Check if you want to wait for multiple objects to conclude."), mainScript.multipleListeners);

		if(!mainScript.FIRST && !mainScript.multipleListeners)
			mainScript.listener = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Listener: ", "The listener is the object that fires an event to this object when it completes its function."), mainScript.listener, typeof(GameObject), true);
		else
			mainScript.listener = null;

		if(mainScript.multipleListeners) {

			EditorGUILayout.HelpBox("List of Listeners", MessageType.None, true);

			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Add Listener")){
				mainScript.listeners.Add(null);
			}

			if(mainScript.listeners.Count > 0)	// make sure you have some before you remove any
			if(GUILayout.Button("Remove Listener")){
				mainScript.listeners.RemoveAt(mainScript.listeners.Count-1); // removes last
			}

			EditorGUILayout.EndHorizontal();

			for(int i = 0; i < mainScript.listeners.Count; i++){

				mainScript.listeners[i] = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Listener: ", "The listener is the object that fires an event to this object when it completes its function."), mainScript.listeners[i], typeof(GameObject), true);

			}

		}

		if(mainScript.FIRST)
			EditorGUILayout.HelpBox("The first object acts immediately and has no prerequisites.", MessageType.None, true);

		GUILayout.Space(12.0f);

		mainScript.puzzleButton = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Puzzle Button: ", "Puzzle button that causes this piece of the chain to complete and move on."), mainScript.puzzleButton, typeof(GameObject), true);

		GUILayout.Space(12.0f);

		mainScript.delayTime = EditorGUILayout.FloatField(new GUIContent("Delay Bef. Active: ", "Time, in seconds, before an object will be marked as Active once it receives the signal."), mainScript.delayTime);



		// used for the transition buttons
		GUIStyle transitionButtonLayout = new GUIStyle("button");
		transitionButtonLayout.stretchWidth = false;
		//transitionButtonLayout.overflow = new RectOffset(-40,-40,0,0);


		// POSITION
		GUILayout.Space(18.0f);

		titleStyle.fontSize = 14;
		titleStyle.normal.textColor = new Color (0.7f, 0.2f, 0.3f);
		EditorGUILayout.LabelField("SPEEDS", titleStyle);

		GUILayout.Space(18.0f);

		// Transition Speeds
		EditorGUILayout.HelpBox("Speed at which the object shifts between active and inactive states.", MessageType.None, true);

		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();

		// pure values
		GUILayout.Label("Pos: ");
		mainScript.transitionPosition = EditorGUILayout.FloatField(mainScript.transitionPosition);
		GUILayout.Label("Rot: ");
		mainScript.transitionRotation = EditorGUILayout.FloatField(mainScript.transitionRotation);
		GUILayout.Label("Scale: ");
		mainScript.transitionScale = EditorGUILayout.FloatField(mainScript.transitionScale);

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();

		if(GUILayout.Button("Very Slow", transitionButtonLayout)) 	{ 
			mainScript.transitionPosition = 1.0f;
			mainScript.transitionRotation = 20.0f;
			mainScript.transitionScale = 0.2f;
		}
		if(GUILayout.Button("Slow", transitionButtonLayout)) 		{ 
			mainScript.transitionPosition = 4.0f;
			mainScript.transitionRotation = 80.0f;
			mainScript.transitionScale = 1.0f;
		}
		if(GUILayout.Button("Normal", transitionButtonLayout)) 		{ 
			mainScript.transitionPosition = 8.0f;
			mainScript.transitionRotation = 200.0f;
			mainScript.transitionScale = 3.0f;
		}
		if(GUILayout.Button("Fast", transitionButtonLayout)) 		{ 
			mainScript.transitionPosition = 14.0f;
			mainScript.transitionRotation = 400.0f;
			mainScript.transitionScale = 5.0f;
		}
		if(GUILayout.Button("Very Fast", transitionButtonLayout)) 	{
			mainScript.transitionPosition = 20.0f;
			mainScript.transitionRotation = 800.0f;
			mainScript.transitionScale = 10.0f;
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		GUILayout.Space(12.0f);


		// used for the clear buttons
		GUIStyle clearButtStyle = new GUIStyle("button");
		clearButtStyle.overflow = new RectOffset(-40,-40,0,0);

		#region POSITION, ROTATION, SCALE
		{
			// POSITION
			GUILayout.Space(18.0f);

			titleStyle.fontSize = 14;
			titleStyle.normal.textColor = new Color (0.6f, 0.2f, 0.4f);
			EditorGUILayout.LabelField("POSITIONS", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Set Pos")){
				mainScript.inactivePosition = mainScript.gameObject.transform.position;
			}
			mainScript.inactivePosition = EditorGUILayout.Vector3Field(new GUIContent("Inactive: ", "The position of the object while inactive."), mainScript.inactivePosition);

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Set Pos")){
				mainScript.activePosition = mainScript.gameObject.transform.position;
			}
			mainScript.activePosition = EditorGUILayout.Vector3Field(new GUIContent("Active: ", "The position of the object while active."), mainScript.activePosition);

			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button("Clear", clearButtStyle)){
				mainScript.activePosition = Vector3.zero;
				mainScript.inactivePosition = Vector3.zero;
			}

			EditorGUILayout.EndVertical();

			// ROTATION
			GUILayout.Space(18.0f);

			titleStyle.fontSize = 14;
			titleStyle.normal.textColor = new Color (0.4f, 0.3f, 0.5f);
			EditorGUILayout.LabelField("ROTATIONS", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Set Rot")){
				mainScript.inactiveRotation = mainScript.gameObject.transform.rotation;
			}
			mainScript.inactiveRotation.eulerAngles = EditorGUILayout.Vector3Field(new GUIContent("Inactive: ", "The rotation of the object while inactive."), mainScript.inactiveRotation.eulerAngles);

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Set Rot")){
				mainScript.activeRotation = mainScript.gameObject.transform.rotation;
			}
			mainScript.activeRotation.eulerAngles = EditorGUILayout.Vector3Field(new GUIContent("Active: ", "The rotation of the object while active."), mainScript.activeRotation.eulerAngles);

			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button("Clear", clearButtStyle)){
				mainScript.activeRotation.eulerAngles = Vector3.zero;
				mainScript.inactiveRotation.eulerAngles = Vector3.zero;
			}

			EditorGUILayout.EndVertical();

			// SCALE
			GUILayout.Space(18.0f);

			titleStyle.fontSize = 14;
			titleStyle.normal.textColor = new Color (0.1f, 0.5f, 0.1f);
			EditorGUILayout.LabelField("SCALE", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Set Scale")){
				mainScript.inactiveScale = mainScript.gameObject.transform.localScale;
			}
			mainScript.inactiveScale = EditorGUILayout.Vector3Field(new GUIContent("Inactive: ", "The scale of the object while inactive."), mainScript.inactiveScale);

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Set Scale")){
				mainScript.activeScale = mainScript.gameObject.transform.localScale;
			}
			mainScript.activeScale = EditorGUILayout.Vector3Field(new GUIContent("Active: ", "The rotation of the object while active."), mainScript.activeScale);

			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button("Clear", clearButtStyle)){
				mainScript.activeScale = Vector3.zero;
				mainScript.inactiveScale = Vector3.zero;
			}

			EditorGUILayout.EndVertical();
		}
		#endregion

		#endregion

	}

}