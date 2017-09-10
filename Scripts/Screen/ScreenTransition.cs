using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenTransition : MonoBehaviour
{
	[SerializeField] Material material;
	[SerializeField] Texture[] textures;

	public delegate void DoneForward();
	public static event DoneForward OnDoneForward;

	public delegate void DoneBackward();
	public static event DoneBackward OnDoneBackward;

	public bool IsBlack { get { return material.GetTexture("_TransitionTex").name == "black_pattern"
		&& material.GetFloat("_Cutoff") == 1; } }

	Canvas hudCanvas;

	void Awake()
	{
		Material newMat = Instantiate(material);
		material = newMat;

		SceneManager.sceneLoaded += SceneLoaded;
	}

	void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		GameObject obj = GameObject.Find("MainHUDCanvas");
		if (obj != null) hudCanvas = obj.GetComponent<Canvas>();

		if (mode == LoadSceneMode.Single)
			SetDirectly(1, "black_pattern");
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit(src, dst, material);
	}

	Texture FindTexture(string texName)
	{
		for (int i = 0; i < textures.Length; i++)
		{
			if (textures[i].name == texName)
				return textures[i];
		}

		return null;
	}

	public void Forward(float speed, string texture) { StartCoroutine(TransitionForward(speed, texture)); }
	public void Backward(float speed, string texture) { StartCoroutine(TransitionBackward(speed, texture)); }
	public void WaitThenBackward(float waitTime, float speed, string texture)
		{ StartCoroutine(WaitBackward(waitTime, speed, texture)); }

	IEnumerator TransitionForward(float speed, string texture)
	{
		material.SetTexture("_TransitionTex", FindTexture(texture));

		hudCanvas.enabled = false;

		float value = 0;
		while (material.GetFloat("_Cutoff") < 1)
		{
			value += speed * Time.deltaTime;
			material.SetFloat("_Cutoff", value);
			yield return null;
		}
		material.SetFloat("_Cutoff", 1);
		material.SetTexture("_TransitionTex", FindTexture("black_pattern"));

		if (OnDoneForward != null)
			OnDoneForward();
	}

	IEnumerator TransitionBackward(float speed, string texture)
	{
		material.SetTexture("_TransitionTex", FindTexture(texture));

		float value = 1;
		while (material.GetFloat("_Cutoff") > 0)
		{
			value -= speed * Time.deltaTime;
			material.SetFloat("_Cutoff", value);
			yield return null;
		}
		material.SetFloat("_Cutoff", 0);
		hudCanvas.enabled = true;

		if (OnDoneBackward != null)
			OnDoneBackward();
	}

	IEnumerator WaitBackward(float waitTime, float speed, string texture)
	{
		yield return new WaitForSeconds(waitTime);
		Backward(speed, texture);
	}

	public void SetDirectly(float cutoff, string texture)
	{
		if (cutoff == 1)
			hudCanvas.enabled = false;

		material.SetTexture("_TransitionTex", FindTexture(texture));
		material.SetFloat("_Cutoff", cutoff);
	}
}
