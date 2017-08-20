using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuestObject))]
[CanEditMultipleObjects]
public class QuestObjectEditor : Editor {

	SerializedProperty objectWithMaterial;
	SerializedProperty activeMaterial;
	SerializedProperty inactiveMaterial;
	SerializedProperty destroyOnFinish;

	void OnEnable() {

		objectWithMaterial = serializedObject.FindProperty ("objectWithMaterial");
		activeMaterial = serializedObject.FindProperty ("activeMaterial");
		inactiveMaterial = serializedObject.FindProperty ("inactiveMaterial");
		destroyOnFinish = serializedObject.FindProperty ("destroyOnFinish");

	}

	public override void OnInspectorGUI(){

		// MUST INCLUDE
		serializedObject.Update ();

		QuestObject mainScript = target as QuestObject;


		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);


		GUILayout.Space(12.0f);

		EditorGUILayout.LabelField("Quest Status: " + mainScript.QuestStatus.ToString());

		GUILayout.Space(18.0f);

		mainScript.questObjectType = (QUESTOBJECTTYPE)EditorGUILayout.EnumPopup(new GUIContent("Quest Obj. Type: ", "This determines what kind of quest object this is.  Necessary for when a quest requires a specific type."), mainScript.questObjectType);

		GUILayout.Space(18.0f);


		if (mainScript.questObjectType == QUESTOBJECTTYPE.collectible) {

			mainScript.collectibleType = (COLLECTIBLETYPE)EditorGUILayout.EnumPopup(new GUIContent("Collectible Type: ", "Specific collectible object type used by QuestType."), mainScript.collectibleType);

		}



		GUILayout.Space(18.0f);

		EditorGUILayout.LabelField ("ACTIVE / INACTIVE MATERIALS", titleStyle);

		GUILayout.Space(18.0f);


		EditorGUILayout.PropertyField (objectWithMaterial, new GUIContent ("Mat Obj: ", "Body containing changeable material."));

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField (activeMaterial, new GUIContent ("Active: ", "Material of object when quest is active."));

		EditorGUILayout.PropertyField (inactiveMaterial, new GUIContent ("Inactive: ", "Material of object when quest is not active."));

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField (destroyOnFinish, new GUIContent ("Destroy on Fin: ", "If checked, the object will be removed if the quest is Finished - Does not wait for Complete status."));



		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();

	}

}