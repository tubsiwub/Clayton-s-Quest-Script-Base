using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public class StoreManagerScript : MonoBehaviour
{
	int colorSwap = 1;
	int hatSwap = 1;
	int ballSwap = 1;

	public bool storeActive;
	bool hasController;
	private bool[] colorSwapHasBought = new bool[5];
	private bool[] hatSwapHasBought = new bool[5];
	private bool[] ballSwapHasBought = new bool[5];

	//array of each item
	public GameObject[] items;

	public GameObject storeMenu;
	public GameObject storeSelected;
	public GameObject player;
	public GameObject previewGuy;
	public GameObject mManager;

	public Text texturePriceText;
	public Text hatPriceText;
	public Text ballModelPriceText;


	// Use this for initialization
	void Start()
	{
		CheckController();

		player = GameObject.FindWithTag("Player");
		mManager = GameObject.Find("In-Game Menus(Clone)");
		previewGuy = GameObject.Find("StorePreviewGuy");
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.frameCount % 120 == 0)
			CheckController();

		if (Input.GetKeyDown("c") && mManager.GetComponent<MenuManager>().menuActive == false)
			StoreMenu();

		if (Input.GetButtonDown("Cancel"))
			StoreBack();
	}

	public void ClickRight()
	{
		if (storeActive && items[0].activeSelf)
		{
			items[0].SetActive(false);
			items[1].SetActive(true);
			items[2].SetActive(false);

			texturePriceText.text = "";
			ballModelPriceText.text = "";
			hatPriceText.text = "";

		}

		else if (storeActive && items[1].activeSelf)
		{
			if (previewGuy.GetComponent<StorePreviewController>().CanTransform == true)
			{
				items[0].SetActive(false);
				items[1].SetActive(false);
				items[2].SetActive(true);

				texturePriceText.text = "";
				ballModelPriceText.text = "";
				hatPriceText.text = "";

				//to ball stuff
				previewGuy.GetComponent<StorePreviewController>().ShowBall();
			}
		}

		else if (storeActive && items[2].activeSelf)
		{
			if (previewGuy.GetComponent<StorePreviewController>().CanTransform == true)
			{
				items[0].SetActive(true);
				items[1].SetActive(false);
				items[2].SetActive(false);

				texturePriceText.text = "";
				ballModelPriceText.text = "";
				hatPriceText.text = "";

				//to human stuff
				previewGuy.GetComponent<StorePreviewController>().ShowHuman();
			}
		}
	}

	public void ClickLeft()
	{
		if (storeActive && items[0].activeSelf)
		{
			if (previewGuy.GetComponent<StorePreviewController>().CanTransform == true)
			{
				items[0].SetActive(false);
				items[1].SetActive(false);
				items[2].SetActive(true);

				texturePriceText.text = "";
				ballModelPriceText.text = "";
				hatPriceText.text = "";

				//to ball stuff
				previewGuy.GetComponent<StorePreviewController>().ShowBall();
			}
		}

		else if (storeActive && items[1].activeSelf)
		{
			items[0].SetActive(true);
			items[1].SetActive(false);
			items[2].SetActive(false);

			texturePriceText.text = "";
			ballModelPriceText.text = "";
			hatPriceText.text = "";
		}

		else if (storeActive && items[2].activeSelf)
		{
			if (previewGuy.GetComponent<StorePreviewController>().CanTransform == true)
			{
				items[0].SetActive(false);
				items[1].SetActive(true);
				items[2].SetActive(false);

				texturePriceText.text = "";
				ballModelPriceText.text = "";
				hatPriceText.text = "";

				//to human stuff
				previewGuy.GetComponent<StorePreviewController>().ShowHuman();
			}
		}
	}

	//Texture swaps

	public void Blue()
	{
		colorSwap = 1;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonBlue_D");
		texturePriceText.text = "Free!";
	}

	public void Red()
	{
		if (colorSwapHasBought[0])
		{
			colorSwap = 2;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonRed_D");
			texturePriceText.text = "Bought!";
		}

		else 
		{
			colorSwap = 2;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonRed_D");
			texturePriceText.text = "30";
		}
	}

	public void Orange()
	{
		if (colorSwapHasBought[1])
		{
			colorSwap = 3;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonOrange_D");
			texturePriceText.text = "Bought!";
		}

		else
		{
			colorSwap = 3;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonOrange_D");
			texturePriceText.text = "30";
		}
	}

	public void Green()
	{
		if (colorSwapHasBought[2])
		{
			colorSwap = 4;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonGreen_D");
			texturePriceText.text = "Bought!";
		}

		else 
		{
			colorSwap = 4;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonGreen_D");
			texturePriceText.text = "30";
		}
	}

	public void Purple()
	{
		if (colorSwapHasBought[3])
		{
			colorSwap = 5;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonPurple_D");
			texturePriceText.text = "Bought!";
		}

		else
		{
			colorSwap = 5;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonPurple_D");
			texturePriceText.text = "30";
		}
	}

	public void White()
	{
		if (colorSwapHasBought[4])
		{
			colorSwap = 6;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonWhite_D");
			texturePriceText.text = "Bought!";
		}

		else
		{
			colorSwap = 6;
			previewGuy.GetComponent<PlayerCustomizer>().ChangeTexture("claytonWhite_D");
			texturePriceText.text = "30";
		}
	}


	//Hat Swaps

	public void NoHat()
	{
		hatSwap = 1;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeHat("");
		hatPriceText.text = "Free!";
	}

	public void BaseBallHat()
	{
		hatSwap = 2;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeHat("baseballHat");
		hatPriceText.text = "60";
	}

	public void BunnyEars()
	{
		hatSwap = 3;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeHat("bunnyEars");
		hatPriceText.text = "60";
	}

	public void NewsboyHat()
	{
		hatSwap = 4;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeHat("newsboyHat");
		hatPriceText.text = "60";
	}

	public void PartyHat()
	{
		hatSwap = 5;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeHat("partyHat");
		hatPriceText.text = "60";
	}

	public void TopHat()
	{
		hatSwap = 6;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeHat("topHat");
		hatPriceText.text = "60";
	}

	//ball model stuff
	public void BaseBallModel()
	{
		ballSwap = 1;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeBallModel("");
		ballModelPriceText.text = "free!";
	}

	public void PushyBallModel()
	{
		ballSwap = 2;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeBallModel("PushyBallBallModel");
		ballModelPriceText.text = "90";
	}

	public void MarbleBallModel()
	{
		ballSwap = 3;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeBallModel("MarbleBallModel");
		ballModelPriceText.text = "90";
	}

	public void CoconutBallModel()
	{
		ballSwap = 4;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeBallModel("CoconutBallModel");
		ballModelPriceText.text = "90";
	}

	public void WaterPotionBallModel()
	{
		ballSwap = 5;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeBallModel("WaterPotionBallModel");
		ballModelPriceText.text = "90";
	}

	public void ButtonBallModel()
	{
		ballSwap = 6;
		previewGuy.GetComponent<PlayerCustomizer>().ChangeBallModel("ButtonBallModel");
		ballModelPriceText.text = "90";

	}

	public void LeaveStore()
	{
		SoundManager.instance.PlayClip("CQMenuDeselect", 1);

		storeMenu.SetActive(false);
		items[0].SetActive(false);
		items[1].SetActive(false);
		items[2].SetActive(false);

		GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().SetFreeze(false);
		player.GetComponent<PlayerHandler>().SetFrozen(false, false);

		MarbleManager.instance.HideText();

		storeActive = false;

	}

	public void Purchase()
	{
		if (items[0].activeSelf)
		{
			if (colorSwap == 1)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeTexture("claytonBlue_D");
					MarbleManager.instance.RemoveMarble();
				}

				else
					print("You can't afford this!");
			}

			else if (colorSwap == 2)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeTexture("claytonRed_D");
					MarbleManager.instance.RemoveMarble();
					colorSwapHasBought[0] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (colorSwap == 3)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeTexture("claytonOrange_D");
					MarbleManager.instance.RemoveMarble();
					colorSwapHasBought[1] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (colorSwap == 4)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeTexture("claytonGreen_D");
					MarbleManager.instance.RemoveMarble();
					colorSwapHasBought[2] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (colorSwap == 5)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeTexture("claytonPurple_D");
					MarbleManager.instance.RemoveMarble();
					colorSwapHasBought[3] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (colorSwap == 6)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeTexture("claytonWhite_D");
					MarbleManager.instance.RemoveMarble();
					colorSwapHasBought[4] = true;
				}

				else
					print("You can't afford this!");
			}
		}

		if(items[1].activeSelf)
		{
			if (hatSwap == 1)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeHat("");
					MarbleManager.instance.RemoveMarble();
				}

				else
					print("You can't afford this!");
			}

			else if (hatSwap == 2)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeHat("baseballHat");
					MarbleManager.instance.RemoveMarble();
					hatSwapHasBought[0] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (hatSwap == 3)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeHat("bunnyEars");
					MarbleManager.instance.RemoveMarble();
					hatSwapHasBought[1] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (hatSwap == 4)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeHat("newsboyHat");
					MarbleManager.instance.RemoveMarble();
					hatSwapHasBought[2] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (hatSwap == 5)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeHat("partyHat");
					MarbleManager.instance.RemoveMarble();
					hatSwapHasBought[3] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (hatSwap == 6)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeHat("topHat");
					MarbleManager.instance.RemoveMarble();
					hatSwapHasBought[4] = true;
				}

				else
					print("You can't afford this!");
			}
		}

		if (items[2].activeSelf)
		{
			if (ballSwap == 1)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeBallModel("");
					MarbleManager.instance.RemoveMarble();
				}

				else
					print("You can't afford this!");
			}

			else if (ballSwap == 2)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeBallModel("PushyBallBallModel");
					MarbleManager.instance.RemoveMarble();
					ballSwapHasBought[0] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (ballSwap == 3)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeBallModel("MarbleBallModel");
					MarbleManager.instance.RemoveMarble();
					ballSwapHasBought[1] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (ballSwap == 4)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeBallModel("CoconutBallModel");
					MarbleManager.instance.RemoveMarble();
					ballSwapHasBought[2] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (ballSwap == 5)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeBallModel("WaterPotionBallModel");
					MarbleManager.instance.RemoveMarble();
					ballSwapHasBought[3] = true;
				}

				else
					print("You can't afford this!");
			}

			else if (ballSwap == 6)
			{
				if (MarbleManager.instance.Collected >= 0)
				{
					player.GetComponent<PlayerCustomizer>().ChangeBallModel("ButtonBallModel");
					MarbleManager.instance.RemoveMarble();
					ballSwapHasBought[4] = true;
				}

				else
					print("You can't afford this!");
			}
		}
	}

	void StoreMenu()
	{
		if (storeActive == false)
		{
			SoundManager.instance.PlayClip("CQMenuSelect", 1);

			mManager.SetActive(false);
			previewGuy.GetComponent<StorePreviewController>().ShowHuman();

			storeMenu.SetActive(true);
			items[0].SetActive(true);
			items[1].SetActive(false);
			items[2].SetActive(false);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().SetFreeze(true);
			player.GetComponent<PlayerHandler>().SetFrozen(true, true);

			if(hasController)
				GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(storeSelected);

			MarbleManager.instance.ShowText();
			storeActive = true;

		}

		else if (storeActive == true && storeMenu.activeSelf)
		{
			SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			mManager.SetActive(true);

			storeMenu.SetActive(false);
			items[0].SetActive(false);
			items[1].SetActive(false);
			items[2].SetActive(false);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().SetFreeze(false);

			player.GetComponent<PlayerHandler>().SetFrozen(false, false);
			MarbleManager.instance.HideText();

			storeActive = false;
		}
	}

	private void StoreBack()
	{
		if (storeActive == true && storeMenu.activeSelf)
		{
			SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			storeMenu.SetActive(false);
			items[0].SetActive(false);
			items[1].SetActive(false);
			items[2].SetActive(false);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().SetFreeze(false);
			player.GetComponent<PlayerHandler>().SetFrozen(false, false);

			MarbleManager.instance.HideText();

			storeActive = false;

			StartCoroutine(WaitThenCanPause());
		}
	}

	IEnumerator WaitThenCanPause()
	{
		yield return 0;

		mManager.SetActive(true);
		
	}

	void CheckController()
	{
		GamePadState gamePadState = GamePad.GetState(0);
		hasController = gamePadState.IsConnected;
	}
}