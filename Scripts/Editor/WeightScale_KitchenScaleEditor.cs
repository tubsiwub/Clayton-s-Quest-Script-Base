using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeightScale_KitchenScale))]
[CanEditMultipleObjects]
public class WeightScale_KitchenScaleEditor : Editor {

	SerializedProperty targetWeight;
	SerializedProperty currentWeight;

	SerializedProperty lockObjects;
	SerializedProperty otherObject;
	SerializedProperty otherObjectSpeed;
	SerializedProperty otherObjectShiftPos;


	void OnEnable(){
		
		targetWeight = serializedObject.FindProperty ("targetWeight");
		currentWeight = serializedObject.FindProperty ("currentWeight");

		lockObjects = serializedObject.FindProperty ("lockObjects");
		otherObject = serializedObject.FindProperty ("otherObject");
		otherObjectSpeed = serializedObject.FindProperty ("otherObjectSpeed");
		otherObjectShiftPos = serializedObject.FindProperty ("otherObjectShiftPos");

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

		WeightScale_KitchenScale mainScript = target as WeightScale_KitchenScale;

		EditorGUILayout.HelpBox ("Click Set Pos when you move it to its NEW position.  Make sure to move Other Object to it's starting position when you're finished.", MessageType.Info);

		GUILayout.Space(12.0f);

		EditorGUILayout.LabelField("WEIGHT SCALE: KITCHEN", titleStyle);

		GUILayout.Space(16.0f);





		titleStyle.fontSize = 14;
		titleStyle.normal.textColor = new Color (0.5f, 0.1f, 0.3f);
		titleStyle.alignment = TextAnchor.MiddleLeft;
		EditorGUILayout.LabelField("Weight Objects", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField (lockObjects, new GUIContent ("Lock Objects: ", "Objects will be prevented from being removed with this checked."));

		GUILayout.Space(4.0f);

		EditorGUILayout.PropertyField (targetWeight, new GUIContent ("Target Weight: ", "Scale activates when this weight is achieved."));

		GUILayout.Space(4.0f);

		EditorGUILayout.LabelField ("Current Weight:        " + currentWeight.floatValue);

		GUILayout.Space (12.0f);




		EditorGUILayout.LabelField("Other Objects", titleStyle);

		GUILayout.Space(12.0f);

//		EditorGUILayout.PropertyField (otherObject, new GUIContent ("Other Object: ", "The object that is affected by the Weight Scale."));
//
//		GUILayout.Space(4.0f);
//
//		EditorGUILayout.PropertyField (otherObjectSpeed, new GUIContent ("Move Speed: ", "Movement speed of Other Object."));
//
//		GUILayout.Space(4.0f);
//
//		EditorGUILayout.BeginHorizontal ();
//		if (GUILayout.Button ("Set Pos")) { otherObjectShiftPos.vector3Value = mainScript.otherObject[0].transform.position; }
//		EditorGUILayout.PropertyField (otherObjectShiftPos, new GUIContent ("Shift Pos: ", "The position that Other Object will move to upon Weight Scale activation."));
//		EditorGUILayout.EndHorizontal ();

		GUIStyle otherObjectStyle = new GUIStyle ("Button");

		EditorGUILayout.BeginHorizontal ();
		if(GUILayout.Button("ADD") )
		{
			mainScript.otherObject.Add (null);
			mainScript.otherObjectShiftPos.Add (Vector3.zero);
			serializedObject.ApplyModifiedProperties ();
		}

		if(GUILayout.Button("REMOVE") && otherObject.arraySize > 0)
		{
			otherObject.DeleteArrayElementAtIndex (otherObject.arraySize - 1);
			otherObjectShiftPos.DeleteArrayElementAtIndex (otherObjectShiftPos.arraySize - 1);
			serializedObject.ApplyModifiedProperties ();
		}

		if(GUILayout.Button("CLEAR") )
		{
			otherObject.ClearArray ();
			otherObjectShiftPos.ClearArray ();
			serializedObject.ApplyModifiedProperties ();
		}
		EditorGUILayout.EndHorizontal ();

		for (int i = 0; i < otherObject.arraySize; i++) 
		{
			EditorGUILayout.BeginVertical (otherObjectStyle);
			EditorGUILayout.PropertyField (otherObject.GetArrayElementAtIndex(i), new GUIContent ("Other Obj: "));

			EditorGUILayout.BeginHorizontal ();					// VERT

			if(mainScript.otherObject [i]) {
				if (GUILayout.Button ("SET")) {
					otherObjectShiftPos.GetArrayElementAtIndex (i).vector3Value = mainScript.otherObject [i].transform.position;
				}

				EditorGUILayout.LabelField (
					"x: " + (otherObjectShiftPos.GetArrayElementAtIndex (i).vector3Value.x.ToString ("F2"))
					+ " y: " + (otherObjectShiftPos.GetArrayElementAtIndex (i).vector3Value.y.ToString ("F2"))
					+ " z: " + (otherObjectShiftPos.GetArrayElementAtIndex (i).vector3Value.z.ToString ("F2")));

				if (GUILayout.Button ("-")) {
					otherObject.DeleteArrayElementAtIndex (i);
					otherObjectShiftPos.DeleteArrayElementAtIndex (i);
					serializedObject.ApplyModifiedProperties ();
				}		
			}

			EditorGUILayout.EndHorizontal ();					// HORI
			EditorGUILayout.EndVertical ();
		}

		GUILayout.Space (6.0f);

		EditorGUILayout.PropertyField (otherObjectSpeed, new GUIContent ("Shift Speed: "));



		GUILayout.Space (14.0f);





		EditorGUILayout.LabelField("List of Objects:");
		GUILayout.Space(8.0f);
		for (int i = 0; i < mainScript.weightObjects.Count; i++) {

			if(mainScript.weightObjects[i] != null)
				EditorGUILayout.LabelField("Object " + i + ": " + mainScript.weightObjects[i].name);
			GUILayout.Space(4.0f);

		}
		if(mainScript.weightObjects.Count <= 0)
			EditorGUILayout.LabelField("Empty");


		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();


	}

}