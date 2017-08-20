// http://wiki.unity3d.com/index.php?title=FramesPerSecond
using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
	bool displayFPS = false;
	bool CanDisplayFPS { get { return displayFPS && Time.timeScale >= 1; } }

	void Start()
	{
		DontDestroyOnLoad(gameObject);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F12))
		{
			displayFPS = !displayFPS;

			print("FPS Display: " + (displayFPS ? "ON" : "OFF"));
		}

		if (CanDisplayFPS)
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	string CalcFPS()
	{
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		return text;
	}

	void OnGUI()
	{
		if (!CanDisplayFPS) return;

		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperRight;
		style.fontSize = h * 6 / 100;
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.red;

		GUI.Label(rect, CalcFPS(), style);
	}
}
