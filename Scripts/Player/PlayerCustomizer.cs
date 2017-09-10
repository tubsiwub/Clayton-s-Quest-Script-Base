using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCustomizer : MonoBehaviour
{
	[SerializeField] bool recieveEventsFromHandler = true;
	[SerializeField] bool saveToFile = true;

	[SerializeField] SkinnedMeshRenderer humanRend;
	[SerializeField] MeshRenderer ballRend;
	[SerializeField] Texture[] _humanTextures;
	[SerializeField] Texture[] _ballTextures;
	[SerializeField] GameObject[] _hats;
	[SerializeField] GameObject[] _altBallModels;
	[SerializeField] Transform hatContainer;
	[SerializeField] Transform hatContainerBall;
	[SerializeField] Transform altBallModelContainer;

	private Dictionary<string, Texture> humanTextures;
	private Dictionary<string, Texture> ballTextures;
	private Dictionary<string, GameObject> hats;
	private Dictionary<string, GameObject> altBallModels;

	private string[,] humanBallMapping;

	PlayerHandler playerHandler;
	StorePreviewController storePreviewController;

	public string CurrentPlayerTexture { get { return humanRend.material.GetTexture("_MainTex").name; } }

	public string CurrentPlayerHat
	{ get {
		if (hatContainer.childCount != 0)
			return hatContainer.GetChild(0).name;
		else if (hatContainerBall.childCount != 0)
			return hatContainerBall.GetChild(0).name;
		else return "";
	} }

	public string CurrentBallModel { get { if (altBallModelContainer.childCount != 0) return altBallModelContainer.GetChild(0).name; else return ""; } }

	private void Awake()
	{
		humanTextures = new Dictionary<string, Texture>();
		ballTextures = new Dictionary<string, Texture>();

		humanBallMapping = new string[_humanTextures.Length, 2];

		for (int i = 0; i < _humanTextures.Length; i++)
		{
			humanBallMapping[i, 0] = _humanTextures[i].name;
			humanBallMapping[i, 1] = _ballTextures[i].name;
		}

		for (int i = 0; i < _humanTextures.Length; i++)
			humanTextures.Add(_humanTextures[i].name, _humanTextures[i]);

		for (int i = 0; i < _ballTextures.Length; i++)
			ballTextures.Add(_ballTextures[i].name, _ballTextures[i]);

		hats = new Dictionary<string, GameObject>();
		for (int i = 0; i < _hats.Length; i++)
			hats.Add(_hats[i].name, _hats[i]);

		altBallModels = new Dictionary<string, GameObject>();
		for (int i = 0; i < _altBallModels.Length; i++)
			altBallModels.Add(_altBallModels[i].name, _altBallModels[i]);

		playerHandler = GetComponent<PlayerHandler>();
		SceneManager.sceneLoaded += SceneLoaded;

		if (recieveEventsFromHandler)
		{
			PlayerHandler.OnBallToHuman += SetHatToHuman;
			PlayerHandler.OnHumanToBallFinish += SetHatToBall;
		}
		else
		{
			storePreviewController = GetComponent<StorePreviewController>();

			storePreviewController.OnBallToHuman += SetHatToHuman;
			storePreviewController.OnHumanToBallFinish += SetHatToBall;
		}
	}

	private void RemoveEvents()
	{
		if (recieveEventsFromHandler)
		{
			PlayerHandler.OnBallToHuman -= SetHatToHuman;
			PlayerHandler.OnHumanToBallFinish -= SetHatToBall;
		}
		else
		{
			storePreviewController.OnBallToHuman -= SetHatToHuman;
			storePreviewController.OnHumanToBallFinish -= SetHatToBall;
		}
	}

	private void OnDestroy()
	{
		RemoveEvents();
	}

	private void OnDisable()
	{
		RemoveEvents();
	}

	private void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex != 0)
		{
			if (saveToFile)
			{
				SavingLoading.instance.LoadPlayerTexture();
				SavingLoading.instance.LoadPlayerHat();
				SavingLoading.instance.LoadBallModel();
			}
		}
	}

	private void Update()
	{
		//if (Input.GetKeyDown(KeyCode.P))
		//	ChangeBallModel("PushyBallBallModel");

		//if (Input.GetKeyDown(KeyCode.O))
		//	ChangeBallModel("MarbleBallModel");

		//if (Input.GetKeyDown(KeyCode.I))
		//	ChangeBallModel("CoconutBallModel");

		//if (Input.GetKeyDown(KeyCode.U))
		//	ChangeBallModel("WaterPotionBallModel");

		//if (Input.GetKeyDown(KeyCode.Y))
		//	ChangeBallModel("ButtonBallModel");

		//if (Input.GetKeyDown(KeyCode.T))
		//	ChangeBallModel("");
	}

	// "claytonBlue_D"
	// "claytonGreen_D"
	// "claytonOrange_D"
	// "claytonPurple_D"
	// "claytonRed_D"
	// "claytonWhite_D"
	public void ChangeTexture(string texture)
	{
		if (humanTextures.ContainsKey(texture))     // passed in human texture! use that
		{
			humanRend.material.SetTexture("_MainTex", humanTextures[texture]);
			ballRend.material.SetTexture("_MainTex", ballTextures[HumanTextureToBall(texture)]);
		}
		else if (ballTextures.ContainsKey(texture)) // nope, it's a ball texture! use this
		{
			ballRend.material.SetTexture("_MainTex", ballTextures[texture]);
			humanRend.material.SetTexture("_MainTex", humanTextures[BallTextureToHuman(texture)]);
		}

		if (saveToFile)
		{
			SavingLoading.instance.SavePlayerTexture(texture);
			SavingLoading.instance.SaveData();
		}
	}

	// "baseballHat"
	// "bunnyEars"
	// "newsboyHat"
	// "partyHat"
	// "topHat"
	public void ChangeHat(string hatName)
	{
		if (hats.ContainsKey(hatName))
		{
			RemoveHat();

			GameObject hat = Instantiate(hats[hatName]);
			hat.name = hatName;

			bool isHuman = (playerHandler != null) && (playerHandler.CurrentState == PlayerHandler.PlayerState.Human);
			if (playerHandler == null)
				isHuman = storePreviewController.IsHuman;

			if (isHuman)
				SetHatToHuman(hat);
			else
				SetHatToBall(hat);

			if (saveToFile)
			{
				SavingLoading.instance.SavePlayerHat(hatName);
				SavingLoading.instance.SaveData();
			}
		}
		else
		{
			RemoveHat();

			if (saveToFile)
			{
				SavingLoading.instance.SavePlayerHat("");
				SavingLoading.instance.SaveData();
			}
		}
	}

	// "PushyBallBallModel"
	// "MarbleBallModel"
	// "CoconutBallModel"
	// "WaterPotionBallModel"
	// "ButtonBallModel"
	public void ChangeBallModel(string modelName)
	{
		if (altBallModels.ContainsKey(modelName))
		{
			RemoveBallModel();

			GameObject model = Instantiate(altBallModels[modelName]);
			model.name = modelName;
			SetBallModel(model);

			if (saveToFile)
			{
				SavingLoading.instance.SaveBallModel(modelName);
				SavingLoading.instance.SaveData();
			}
		}
		else
		{
			RemoveBallModel();

			if (saveToFile)
			{
				SavingLoading.instance.SaveBallModel("");
				SavingLoading.instance.SaveData();
			}
		}
	}

	private void RemoveHat()
	{
		if (hatContainer.childCount != 0)
		{
			int amount = hatContainer.childCount;
			for (int i = amount - 1; i >= 0; i--)
				Destroy(hatContainer.GetChild(i).gameObject);
		}

		if (hatContainerBall.childCount != 0)
		{
			int amount = hatContainerBall.childCount;
			for (int i = amount - 1; i >= 0; i--)
				Destroy(hatContainerBall.GetChild(i).gameObject);
		}
	}

	private void SetHatToHuman() { SetHatToHuman(null); }
	private void SetHatToHuman(GameObject hat)
	{
		if (hatContainerBall.childCount == 1)
			hat = hatContainerBall.GetChild(0).gameObject;

		if (hat != null)
		{
			Vector3 scale = hat.transform.localScale;
			hat.transform.SetParent(hatContainer, true);
			hat.transform.localScale = scale;

			hat.transform.rotation = Quaternion.LookRotation(hatContainer.forward);
			hat.transform.localRotation = Quaternion.identity;

			hat.transform.localPosition = Vector3.zero;
		}
	}

	private void SetHatToBall() { SetHatToBall(null); }
	private void SetHatToBall(GameObject hat)
	{
		if (hatContainer.childCount == 1)
			hat = hatContainer.GetChild(0).gameObject;

		if (hat != null)
		{
			hat.transform.position = hatContainerBall.position;

			Vector3 scale = hat.transform.localScale;
			hat.transform.SetParent(hatContainerBall, true);
			hat.transform.localScale = scale;
			hat.transform.localRotation = Quaternion.identity;
		}
	}

	private string HumanTextureToBall(string texture)
	{
		for (int i = 0; i < _humanTextures.Length; i++)
		{
			if (humanBallMapping[i, 0] == texture)
				return humanBallMapping[i, 1];
		}

		return "";
	}

	private string BallTextureToHuman(string texture)
	{
		for (int i = 0; i < _ballTextures.Length; i++)
		{
			if (humanBallMapping[i, 1] == texture)
				return humanBallMapping[i, 0];
		}

		return "";
	}

	private void RemoveBallModel()
	{
		if (altBallModelContainer.childCount != 0)
		{
			int amount = altBallModelContainer.childCount;
			for (int i = amount - 1; i >= 0; i--)
				Destroy(altBallModelContainer.GetChild(i).gameObject);
		}

		ballRend.enabled = true;
	}

	private void SetBallModel(GameObject model)
	{
		if (model != null)
		{
			model.transform.SetParent(altBallModelContainer, true);
			model.transform.localPosition = Vector3.zero;
			model.transform.localRotation = Quaternion.identity;

			ballRend.enabled = false;
		}
	}
}
