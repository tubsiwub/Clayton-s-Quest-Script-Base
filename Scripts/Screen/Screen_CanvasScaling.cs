using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Screen_CanvasScaling : MonoBehaviour {

	Resolution currentResolution;

	float ResolutionWidth, ResolutionHeight;

	public Vector2 GetResolution{ get { return new Vector2 (ResolutionWidth, ResolutionHeight); } }

	void Awake () {

		currentResolution = Screen.currentResolution;

		GetComponent<CanvasScaler> ().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		GetComponent<CanvasScaler> ().referenceResolution = new Vector2 (currentResolution.width, currentResolution.height);

		//SetScreenWidthAndHeightFromEditorGameViewViaReflection();

		if (GetAspectMainCamera().x == 4 && GetAspectMainCamera().y == 3) {
			ResolutionWidth = 1280;
			ResolutionHeight = 960;
		} else if (GetAspectMainCamera().x == 3 && GetAspectMainCamera().y == 2) {
			ResolutionWidth = 1280;
			ResolutionHeight = 854;
		} else if (GetAspectMainCamera().x == 5 && GetAspectMainCamera().y == 4) {
			ResolutionWidth = 1280;
			ResolutionHeight = 1024;
		} else if (GetAspectMainCamera().x == 16 && GetAspectMainCamera().y == 9) {
			ResolutionWidth = 1280;
			ResolutionHeight = 720;
		} else if (GetAspectMainCamera().x == 16 && GetAspectMainCamera().y == 10) {
			ResolutionWidth = 1280;
			ResolutionHeight = 800;
		} else {
			ResolutionWidth = currentResolution.width;
			ResolutionHeight = currentResolution.height;
		}
			
		GetComponent<CanvasScaler> ().referenceResolution = new Vector2 (ResolutionWidth, ResolutionHeight);

	}

	Vector2 GetAspect(float a, float b)
	{
		int w, h;

		if (a/b == 1.7)
		{
			w = 16;
			h = 9;
		}
		else if (a/b == 1.6)
		{
			w = 16;
			h = 10;
		}
		else if (a/b == 1.5)
		{
			w = 3;
			h = 2;
		}
		else if (a/b == 1.25)
		{
			w = 5;
			h = 4;
		}
		else
		{
			w = 4;
			h = 3;
		}

		return new Vector2(w,h);
	}

	Vector2 GetAspectMainCamera()
	{
		int w, h;

		if (Camera.main.aspect == 1.7)
		{
			w = 16;
			h = 9;
		}
		else if (Camera.main.aspect == 1.6)
		{
			w = 16;
			h = 10;
		}
		else if (Camera.main.aspect == 1.5)
		{
			w = 3;
			h = 2;
		}
		else if (Camera.main.aspect == 1.25)
		{
			w = 5;
			h = 4;
		}
		else
		{
			w = 4;
			h = 3;
		}

		return new Vector2(w,h);
	}

}
