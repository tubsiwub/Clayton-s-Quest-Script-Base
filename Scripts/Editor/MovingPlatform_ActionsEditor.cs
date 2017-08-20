using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform_Actions))]
[CanEditMultipleObjects]
public class MovingPlatform_ActionsEditor : Editor {

	public override void OnInspectorGUI(){

		GUILayout.Space(16.0f);

		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);

		MovingPlatform_Actions mainScript = target as MovingPlatform_Actions;

		EditorGUILayout.LabelField("MOVING PLATFORM", titleStyle);

		GUILayout.Space(16.0f);

		if (mainScript.x == 0 && mainScript.y == 0 && mainScript.z == 0) {

			EditorGUILayout.HelpBox ("Click to set the current position as the 'Open' position.", MessageType.None);

			GUILayout.Space (4.0f);
		}

		GUILayout.BeginHorizontal ();

		if(GUILayout.Button("Set MoveTo Loc.")){

			mainScript.x = mainScript.transform.localPosition.x;
			mainScript.y = mainScript.transform.localPosition.y;
			mainScript.z = mainScript.transform.localPosition.z;

		}

		mainScript.x = EditorGUILayout.FloatField (mainScript.x);
		mainScript.y = EditorGUILayout.FloatField (mainScript.y);
		mainScript.z = EditorGUILayout.FloatField (mainScript.z);

		mainScript.openPosTemp = new Vector3 (mainScript.x, mainScript.y, mainScript.z);

		mainScript.openLocation = mainScript.openPosTemp;

		GUILayout.EndHorizontal ();

		GUILayout.Space(16.0f);

	}

}