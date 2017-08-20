using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BallDoorControl))]
public class BallDoorControlEditor : Editor
{
	//BallDoorControl ballDoorControl;

	SerializedProperty colliderToToggle;
	SerializedProperty loadPath;
	SerializedProperty doorType;

	const float smallSpacing = 8;
	const float sectionSpacing = 16;

	void OnEnable()
	{
		//ballDoorControl = (BallDoorControl)target;

		colliderToToggle = serializedObject.FindProperty("colliderToToggle");
		loadPath = serializedObject.FindProperty("loadPath");
		doorType = serializedObject.FindProperty("doorType");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		GUILayout.Space(smallSpacing);
		EditorGUILayout.PropertyField(colliderToToggle);

		GUILayout.Space(smallSpacing);
		EditorGUILayout.PropertyField(doorType);
		
		if (doorType.enumValueIndex == (int)BallDoorControl.DoorType.Enter)
		{
			var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(loadPath.stringValue);

			EditorGUI.BeginChangeCheck();
			SceneAsset newScene = (SceneAsset)EditorGUILayout.ObjectField("Scene", oldScene, typeof(SceneAsset), false);

			if (EditorGUI.EndChangeCheck())
				loadPath.stringValue = AssetDatabase.GetAssetPath(newScene);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
