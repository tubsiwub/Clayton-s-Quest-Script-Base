using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PropPainter))]
public class PropPainterEditor : Editor
{
	PropPainter propPainter;
	
	SerializedProperty props;
	SerializedProperty randomYaw;
	SerializedProperty randomRotation;
	SerializedProperty brushSize;
	SerializedProperty brushDensity;

	const float cursorUpOffset = 35;
	const float smallSpacing = 8;
	const float sectionSpacing = 16;

	int frameCount = 0;

	void OnEnable()
	{
		propPainter = (PropPainter)target;
		
		props = serializedObject.FindProperty("props");
		randomYaw = serializedObject.FindProperty("randomYaw");
		randomRotation = serializedObject.FindProperty("randomRotation");
		brushSize = serializedObject.FindProperty("brushSize");
		brushDensity = serializedObject.FindProperty("brushDensity");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontStyle = FontStyle.Bold;

		GUILayout.Space(sectionSpacing);
		EditorGUILayout.LabelField("Prop List", titleStyle);

		string propString = "The painter will randomly select objects in this list for painting. ";
		propString += "You can have as many objects to select from as you'd like, but there must be at least one. Empty list elements will be ignored.";
		EditorGUILayout.HelpBox(propString, MessageType.None);

		ArrayGUI(props);

		GUILayout.Space(sectionSpacing);
		EditorGUILayout.LabelField("Settings", titleStyle);

		GUI.enabled = !randomRotation.boolValue;
		EditorGUILayout.PropertyField(randomYaw, new GUIContent("Random Yaw", "Randomly rotates each object on the yaw axis. Recommended."));
		GUI.enabled = true;

		GUI.enabled = !randomYaw.boolValue;
		EditorGUILayout.PropertyField(randomRotation, new GUIContent("Random Rotation", "Randomly rotates each object on every axis"));
		GUI.enabled = true;

		GUILayout.Space(smallSpacing);
		EditorGUILayout.Slider(brushSize, 1, 10, new GUIContent("Brush Size", "Size of the brush used to paint"));
		EditorGUILayout.Slider(brushDensity, 1, propPainter.MaxBrushDensity, 
			new GUIContent("Brush Density", "Amount of paint that comes out. Just set it to the max, this feature sucks."));

		GUILayout.Space(sectionSpacing);

		string warningString = "Warning: This will destroy all child objects on this transform. ";
		warningString += "If you have other childed objects here, they will be destroyed too";
		EditorGUILayout.HelpBox(warningString, MessageType.None);

		StartCenter();
		if (GUILayout.Button("Destroy All Props", GUILayout.MaxWidth(180), GUILayout.MinHeight(30)))
		{
			List<GameObject> children = new List<GameObject>();
			foreach (Transform child in propPainter.transform)
				children.Add(child.gameObject);

			children.ForEach(child => Undo.DestroyObjectImmediate(child));
		}
		EndCenter();
		GUI.enabled = true;

		serializedObject.ApplyModifiedProperties();
	}

	void StartCenter()
	{
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
	}

	void EndCenter()
	{
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	void ArrayGUI(SerializedProperty property)
	{
		SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");

		GUILayout.BeginHorizontal("Box");
		if (GUILayout.Button("Add new"))
		{
			property.InsertArrayElementAtIndex(arraySizeProp.intValue);

			// when adding a new element after an element that is not null, it, uhh, gets duplicated? I dunno
			// If the new element is a duplicate, clear it
			if (property.GetArrayElementAtIndex(arraySizeProp.intValue-1).objectReferenceValue != null)
				property.DeleteArrayElementAtIndex(arraySizeProp.intValue-1);
		}

		GUI.enabled = (arraySizeProp.intValue > 1);
		if (GUILayout.Button("Clear list"))
		{
			property.ClearArray();
			property.InsertArrayElementAtIndex(arraySizeProp.intValue);	// make sure we have at least 1 item

			return;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();

		int markForRemove = -1;
		GUILayout.BeginVertical("Box");
		for (int i = 0; i < arraySizeProp.intValue; i++)
		{
			GUILayout.BeginHorizontal("Box");

			EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), new GUIContent());

			GUI.enabled = (arraySizeProp.intValue > 1);
			if (GUILayout.Button("Remove"))
				markForRemove = i;
			GUI.enabled = true;

			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();

		if (markForRemove != -1)
		{
			bool hasObject = property.GetArrayElementAtIndex(markForRemove).objectReferenceValue != null;

			property.DeleteArrayElementAtIndex(markForRemove);
			// if the deleted object wasn't null, it'll just clear the element, without removing it. 
			// calling this function twice actually removes the element
			if (hasObject) property.DeleteArrayElementAtIndex(markForRemove);
		}
	}

	void OnSceneGUI()
	{
		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

		frameCount++;
		int density = Mathf.RoundToInt(Mathf.Abs(propPainter.BrushDensity-propPainter.MaxBrushDensity)+1);
		if (frameCount % density != 0) return;

		if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
			&& Event.current.button == 0 && !Event.current.control && 
			!Event.current.alt && Camera.current != null)
		{
			RaycastHit hit;

			Vector2 mousePos = Event.current.mousePosition;
			mousePos.y = Screen.height - mousePos.y - cursorUpOffset;

			if (mousePos.x < 0 || mousePos.x > Screen.width) return;
			if (mousePos.y < 0 || mousePos.y > Screen.height - cursorUpOffset) return;

			Ray ray = Camera.current.ScreenPointToRay(mousePos);

			if (propPainter.BrushSize > 1)
			{
				Vector3 ran = Random.insideUnitSphere * propPainter.BrushSize; ran.y = 0;
				ray.origin += ran;
			}

			if (Physics.Raycast(ray, out hit, 100000, -1, QueryTriggerInteraction.Ignore))
				PaintProp(hit.point);
		}
	}

	void PaintProp(Vector3 pos)
	{
		if (!NonNullObjects()) return;

		GameObject randomObject = GetRandomObject();
		GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(randomObject);

		if (obj == null)
			obj = Instantiate(randomObject);
		
		if (obj != null)
		{
			obj.transform.SetParent(propPainter.transform);
			obj.transform.position = pos;

			if (propPainter.RandomYaw)
			{
				obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
			}

			if (propPainter.RandomRotation)
			{
				obj.transform.rotation = Quaternion.Euler(Random.Range(0, 360),
					Random.Range(0, 360), Random.Range(0, 360));
			}

			Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
		}
	}

	bool NonNullObjects()
	{
		for (int i = 0; i < propPainter.Props.Length; i++)
		{
			if (propPainter.Props[i] != null)
				return true;
		}

		return false;
	}

	GameObject GetRandomObject()
	{
		GameObject obj = null;
		while (obj == null)
		{
			int r = Random.Range(0, propPainter.Props.Length);
			obj = propPainter.Props[r];
		}

		return obj;
	}
}
