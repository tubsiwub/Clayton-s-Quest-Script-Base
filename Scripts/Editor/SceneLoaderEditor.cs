using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SceneLoader), true)]
public class SceneLoaderEditor : Editor
{
	//SceneLoader sceneLoader;
	SerializedProperty scenePaths;
	SerializedProperty showLoadScreen;
	SerializedProperty loaderId;

	const float smallSpacing = 8;
	const float sectionSpacing = 16;

	void OnEnable()
	{
		//sceneLoader = (SceneLoader)target;

		scenePaths = serializedObject.FindProperty("scenePaths");
		showLoadScreen = serializedObject.FindProperty("showLoadScreen");
		loaderId = serializedObject.FindProperty("loaderId");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontStyle = FontStyle.Bold;

		GUILayout.Space(sectionSpacing);
		EditorGUILayout.LabelField("Load Scenes", titleStyle);

		ArrayGUI(scenePaths);
		GUILayout.Space(smallSpacing);

		EditorGUILayout.PropertyField(loaderId);
		EditorGUILayout.PropertyField(showLoadScreen);

		serializedObject.ApplyModifiedProperties();
	}

	void ArrayGUI(SerializedProperty property)
	{
		SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");

		GUILayout.BeginHorizontal("Box");
		AddButton(property, arraySizeProp);
		AddActive(property, arraySizeProp);
		ClearList(property, arraySizeProp);
		GUILayout.EndHorizontal();

		int markForRemove = -1;
		GUILayout.BeginVertical("Box");
		for (int i = 0; i < arraySizeProp.intValue; i++)
		{
			GUILayout.BeginHorizontal("Box");

			SceneAsset oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.GetArrayElementAtIndex(i).stringValue);

			EditorGUI.BeginChangeCheck();
			SceneAsset newScene = (SceneAsset)EditorGUILayout.ObjectField(oldScene,
				typeof(SceneAsset), false);

			if (EditorGUI.EndChangeCheck())
			{
				bool isDuplicate = false;
				for (int j = 0; j < arraySizeProp.intValue; j++)
				{
					if (property.GetArrayElementAtIndex(j).stringValue == AssetDatabase.GetAssetPath(newScene))
						isDuplicate = true;
				}

				if (!isDuplicate)
					property.GetArrayElementAtIndex(i).stringValue = AssetDatabase.GetAssetPath(newScene);
			}

			GUI.enabled = (arraySizeProp.intValue > 1);
			if (GUILayout.Button("Remove"))
				markForRemove = i;
			GUI.enabled = true;

			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();

		if (markForRemove != -1)
		{
			property.DeleteArrayElementAtIndex(markForRemove);
		}
	}

	void AddButton(SerializedProperty property, SerializedProperty arraySizeProp)
	{
		GUI.enabled = (property.GetArrayElementAtIndex(arraySizeProp.intValue-1).stringValue != "");
		if (GUILayout.Button("Add new"))
		{
			property.InsertArrayElementAtIndex(arraySizeProp.intValue);

			// when adding a new element after an element that is not null, it, uhh, gets duplicated? I dunno
			// If the new element is a duplicate, clear it
			if (property.GetArrayElementAtIndex(arraySizeProp.intValue-1).stringValue != "")
				property.GetArrayElementAtIndex(arraySizeProp.intValue-1).stringValue = "";
		}
		GUI.enabled = true;
	}

	void AddActive(SerializedProperty property, SerializedProperty arraySizeProp)
	{
		bool allow = true;
		for (int j = 0; j < arraySizeProp.intValue; j++)
		{
			if (property.GetArrayElementAtIndex(j).stringValue == SceneManager.GetActiveScene().path)
				allow = false;
		}

		GUI.enabled = allow;
		if (GUILayout.Button("Add Active Scene"))
		{
			if (property.GetArrayElementAtIndex(arraySizeProp.intValue-1).stringValue == "")
			{
				//property.InsertArrayElementAtIndex(arraySizeProp.intValue);
				property.GetArrayElementAtIndex(0).stringValue = SceneManager.GetActiveScene().path;
			}
			else
			{
				property.InsertArrayElementAtIndex(arraySizeProp.intValue);
				property.GetArrayElementAtIndex(arraySizeProp.intValue-1).stringValue = SceneManager.GetActiveScene().path;
			}
		}
		GUI.enabled = true;
	}

	void ClearList(SerializedProperty property, SerializedProperty arraySizeProp)
	{
		GUI.enabled = (arraySizeProp.intValue > 1);
		if (GUILayout.Button("Clear list"))
		{
			property.ClearArray();
			property.InsertArrayElementAtIndex(arraySizeProp.intValue); // make sure we have at least 1 item

			return;
		}
		GUI.enabled = true;
	}
}
