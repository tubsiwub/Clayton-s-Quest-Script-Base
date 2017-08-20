using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

public class StartMenuScript : MonoBehaviour
{
	GameObject player;
	GameObject cam;
	public GameObject mManager;

	public GameObject mainSelected;
	public GameObject soundSelected;
	public GameObject videoSelected;
	public GameObject gameplaySelected;
	public GameObject mainMenu;
	public GameObject soundMenu;
	public GameObject videoMenu;
	//public GameObject gameplayMenu;

	public Slider[] volumeSliders;

	public Toggle[] resolutionToggles;
	public Toggle[] aliasToggles;
	public Toggle vSyncToggle;
	public Toggle muteMusic;
	public Toggle muteSfx;
	public Toggle fullScreenToggle;

	public Dropdown resolutionDropdown;

	public Text musicPercentText;
	public Text soundPercentText;

	public int[] screenWidths;
	public int[] screenHeights;

	//int activeScreenResIndex;
	int activeAliasIndex;

	float maxMusicVolume = 1.0f;
	float maxSFXVolume = 1.0f;
	float currentMusicVolume;
	float currentSFXVolume;
	float musicPercent;
	float soundPercent;

	bool vLook = false;
	bool hLook = false;
	bool cRotate = false;
	bool hasController;

private void Awake()
	{
		player = GameObject.FindWithTag("Player");
		cam = GameObject.FindWithTag("MainCamera");

		CheckController();
	}

	void Start()
	{
		mainMenu.SetActive(true);
		soundMenu.SetActive(false);
		videoMenu.SetActive(false);

		cam.GetComponent<CameraControlDeluxe>().SetFreeze(true);

		mManager = GameObject.Find("In-Game Menus(Clone)");

		if (hasController)
		{
			Cursor.lockState = CursorLockMode.Locked;
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
		}

		if (Screen.fullScreen)
		{
			fullScreenToggle.isOn = true;
			mManager.GetComponent<MenuManager>().fullScreenToggle.isOn = true;
		}

		else
		{
			fullScreenToggle.isOn = false;
			mManager.GetComponent<MenuManager>().fullScreenToggle.isOn = false;
		}

		//if (fullScreenToggle.isOn == true)
		//	Screen.fullScreen = true;
		//else if (fullScreenToggle.isOn == false)
		//	Screen.fullScreen = false;


		//loading player prefs stuff
		bool isVSync = (PlayerPrefs.GetInt("vSync") == 1) ? true : false;
		bool musicIsMuted = (PlayerPrefs.GetInt("music muted") == 1) ? true : false;
		bool sfxIsMuted = (PlayerPrefs.GetInt("sfx muted") == 1) ? true : false;

		float musicVolume = PlayerPrefs.GetFloat("music volume");
		float sfxVolume = PlayerPrefs.GetFloat("sfx volume");

		activeAliasIndex = PlayerPrefs.GetInt("antialias index");

		//setting player prefs stuff
		//setting antialias on start
		for (int i = 0; i < aliasToggles.Length; i++)
		{
			aliasToggles[i].isOn = i == activeAliasIndex;
		}

		//setting vsync at start
		vSyncToggle.isOn = isVSync;

		//setting if music is muted
		muteMusic.isOn = musicIsMuted;
		mManager.GetComponent<MenuManager>().muteMusic.isOn = musicIsMuted;

		//setting if sfx are muted
		muteSfx.isOn = sfxIsMuted;
		mManager.GetComponent<MenuManager>().muteSfx.isOn = sfxIsMuted;

		//setting music/sfx volumes
		SoundManager.instance.SetMusicVolume(musicVolume);
		volumeSliders[0].value = musicVolume;
		mManager.GetComponent<MenuManager>().volumeSliders[0].value = musicVolume;

		SoundManager.instance.SetSfxVolume(sfxVolume);
		volumeSliders[1].value = sfxVolume;
		mManager.GetComponent<MenuManager>().volumeSliders[1].value = sfxVolume;

		//checking resolutoin at start
		for (int i = 0; i < 15; i++)
		{
			if (Screen.width == screenWidths[i] && Screen.height == screenHeights[i])
			{
				resolutionDropdown.value = i;
				mManager.GetComponent<MenuManager>().resolutionDropdown.value = i;
			}
		}

		if (muteMusic.isOn)
		{
			SoundManager.instance.SetMusicMute(true);
			mManager.GetComponent<MenuManager>().musicIsMute = true;
		}

		else
		{
			SoundManager.instance.SetMusicMute(false);
			mManager.GetComponent<MenuManager>().musicIsMute = false;
		}

		if (muteSfx.isOn)
		{
			SoundManager.instance.SetSfxMute(true);
			mManager.GetComponent<MenuManager>().soundIsMute = true;
		}

		else
		{
			SoundManager.instance.SetSfxMute(false);
			mManager.GetComponent<MenuManager>().soundIsMute = true;
		}
	}

	private void Update()
	{
		// to delay player freeze (janky but it works!!! YAY!)
		if (Time.frameCount == 1)
			player.GetComponent<PlayerHandler>().SetFrozen(true, true);

		if (Screen.fullScreen)
		{
			resolutionDropdown.interactable = false;
		}

		else
			resolutionDropdown.interactable = true;

		if (Input.GetButtonDown("Cancel"))
		{
			MenuBack();
		}
	}

	//Main Menu
	public void StartGame()
	{
		SceneManager.LoadScene(1);
	}

	public void LoadGame()
	{
		SavingLoading.instance.LoadAllData();
	}

	public void Quit()
	{
		Application.Quit();
	}

	//Sound Menu
	public void SoundMenu()
	{
		mainMenu.SetActive(false);
		soundMenu.SetActive(true);
		videoMenu.SetActive(false);
		GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(soundSelected);

		//Get Current sfx %
		currentSFXVolume = SoundManager.instance.GetSfxVolume;
		soundPercent = (currentSFXVolume / maxSFXVolume) * 100;
		soundPercentText.text = soundPercent.ToString("F0") + "%";

		//Get Current Music %
		currentMusicVolume = SoundManager.instance.GetMusicVolume;
		musicPercent = (currentMusicVolume / maxMusicVolume) * 100;
		musicPercentText.text = musicPercent.ToString("F0") + "%";

	}

	public void SetMusicVolume(float value)
	{
		SoundManager.instance.SetMusicVolume(value);

		currentMusicVolume = SoundManager.instance.GetMusicVolume;
		musicPercent = (currentMusicVolume / maxMusicVolume) * 100;
		musicPercentText.text = musicPercent.ToString("F0") + "%";

		PlayerPrefs.SetFloat("music volume", currentMusicVolume);
		PlayerPrefs.Save();
	}

	public void SetSfxVolume(float value)
	{
		SoundManager.instance.SetSfxVolume(value);

		currentSFXVolume = SoundManager.instance.GetSfxVolume;
		soundPercent = (currentSFXVolume / maxSFXVolume) * 100;
		soundPercentText.text = soundPercent.ToString("F0") + "%";

		PlayerPrefs.SetFloat("sfx volume", currentSFXVolume);
		PlayerPrefs.Save();
	}

	public void MuteMusic()
	{
		if (muteMusic.isOn)
		{
			SoundManager.instance.SetMusicMute(true);
			mManager.GetComponent<MenuManager>().musicIsMute = true;

			mManager.GetComponent<MenuManager>().musicOn.SetActive(false);
			mManager.GetComponent<MenuManager>().musicOn.SetActive(true);

			PlayerPrefs.SetInt("music muted", 1);
			PlayerPrefs.Save();
		}


		else
		{
			SoundManager.instance.SetMusicMute(false);
			mManager.GetComponent<MenuManager>().musicIsMute = false;

			mManager.GetComponent<MenuManager>().musicOn.SetActive(true);
			mManager.GetComponent<MenuManager>().musicOn.SetActive(false);

			PlayerPrefs.SetInt("music muted", 0);
			PlayerPrefs.Save();
		}

	}

	public void MuteSfx()
	{
		if (muteSfx.isOn)
		{
			SoundManager.instance.SetSfxMute(true);
			mManager.GetComponent<MenuManager>().soundIsMute = true;

			mManager.GetComponent<MenuManager>().sfxOn.SetActive(false);
			mManager.GetComponent<MenuManager>().sfxOn.SetActive(true);

			PlayerPrefs.SetInt("sfx muted", 1);
			PlayerPrefs.Save();
		}

		else
		{
			SoundManager.instance.SetSfxMute(false);
			mManager.GetComponent<MenuManager>().soundIsMute = false;

			mManager.GetComponent<MenuManager>().sfxOn.SetActive(true);
			mManager.GetComponent<MenuManager>().sfxOn.SetActive(false);

			PlayerPrefs.SetInt("sfx muted", 0);
			PlayerPrefs.Save();
		}
	}

	public void SoundReturn()
	{
		mainMenu.SetActive(true);
		soundMenu.SetActive(false);
		videoMenu.SetActive(false);
		GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
	}

	//Video Menu
	public void VideoMenu()
	{
		mainMenu.SetActive(false);
		soundMenu.SetActive(false);
		videoMenu.SetActive(true);
		GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(videoSelected);
	}

	public void SetScreenResolution(int i)
	{
		//old toggle stuff
		//if (resolutionToggles[i].isOn)
		//{
		//    activeScreenResIndex = i;

		//    Screen.SetResolution(screenWidths[i], screenHeights[i], false);

		//    PlayerPrefs.SetInt("screen res index", activeScreenResIndex);
		//    PlayerPrefs.Save();
		//}

		//activeScreenResIndex = i;

		Screen.SetResolution(screenWidths[i], screenHeights[i], false);
		resolutionDropdown.value = i;

		//PlayerPrefs.SetInt("screen res index", activeScreenResIndex);
		//PlayerPrefs.Save();
	}

	public void SetFullScreen(bool isFullscreen)
	{
		for (int i = 0; i < resolutionToggles.Length; i++)
		{
			resolutionToggles[i].interactable = !isFullscreen;
		}

		if (isFullscreen)
		{
			Resolution[] allResolutions = Screen.resolutions;
			Resolution maxResolution = allResolutions[allResolutions.Length - 1];
			Screen.SetResolution(maxResolution.width, maxResolution.height, true);
			mManager.GetComponent<MenuManager>().fullScreenToggle.isOn = true;
		}

		else
		{
			SetScreenResolution(resolutionDropdown.value);
			mManager.GetComponent<MenuManager>().fullScreenToggle.isOn = false;
		}

		//PlayerPrefs.SetInt("fullscreen", ((isFullscreen) ? 1 : 0));
		//PlayerPrefs.Save();
	}

	public void VideoReturn()
	{
		mainMenu.SetActive(true);
		soundMenu.SetActive(false);
		videoMenu.SetActive(false);
		GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
	}

	public void SetAntialiasing(int i)
	{
		if (aliasToggles[0].isOn)
		{
			activeAliasIndex = 0;
			QualitySettings.antiAliasing = 0;

			PlayerPrefs.SetInt("antialias index", activeAliasIndex);
			PlayerPrefs.Save();
		}

		if (aliasToggles[1].isOn)
		{
			activeAliasIndex = 1;
			QualitySettings.antiAliasing = 2;

			PlayerPrefs.SetInt("antialias index", activeAliasIndex);
			PlayerPrefs.Save();
		}

		if (aliasToggles[2].isOn)
		{
			activeAliasIndex = 2;
			QualitySettings.antiAliasing = 4;

			PlayerPrefs.SetInt("antialias index", activeAliasIndex);
			PlayerPrefs.Save();
		}

		if (aliasToggles[3].isOn)
		{
			activeAliasIndex = 3;
			QualitySettings.antiAliasing = 8;

			PlayerPrefs.SetInt("antialias index", activeAliasIndex);
			PlayerPrefs.Save();
		}
	}

	public void SetvSync(bool isVSync)
	{
		if (isVSync)
		{
			QualitySettings.vSyncCount = 1;
			mManager.GetComponent<MenuManager>().vSyncOn = true;
		}

		else
		{
			QualitySettings.vSyncCount = 0;
			mManager.GetComponent<MenuManager>().vSyncOn = false;
		}

		PlayerPrefs.SetInt("vSync", ((isVSync) ? 1 : 0));
		PlayerPrefs.Save();
	}

	////gameplay menu stuff
	public void GamePlayMenu()
	{
		mainMenu.SetActive(false);
		soundMenu.SetActive(false);
		videoMenu.SetActive(false);
		//gameplayMenu.SetActive(true);
		GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(gameplaySelected);
	}

	public void GamePlayReturn()
	{
		mainMenu.SetActive(false);
		soundMenu.SetActive(false);
		videoMenu.SetActive(false);
		//gameplayMenu.SetActive(false);
		GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
	}

	public void InvertY()
	{
		if (!vLook)
		{

			mManager.GetComponent<MenuManager>().vLook = true;
			vLook = true;
		}

		else
		{
			mManager.GetComponent<MenuManager>().vLook = false;
			vLook = false;
		}

	}

	public void InvertX()
	{
		if (!hLook)
		{

			mManager.GetComponent<MenuManager>().hLook = true;
			hLook = true;
		}

		else
		{
			mManager.GetComponent<MenuManager>().hLook = false;
			hLook = false;
		}
	}

	public void AutoRotate()
	{
		if (!cRotate)
		{
			mManager.GetComponent<MenuManager>().cRotate = true;
			cRotate = true;
		}

		else
		{
			mManager.GetComponent<MenuManager>().cRotate = false;
			cRotate = false;
		}
	}

	public void Rumble(bool isRumble)
	{
		if (isRumble)
			PlayerHandler.AllowVibration = true;

		else
			PlayerHandler.AllowVibration = false;
	}

	public void ClicktoJump(bool isJump)
	{
		if (isJump)
			PlayerHandler.SetClickToJump(true);

		else
			PlayerHandler.SetClickToJump(false);
	}

	public void SetSensetivity(float value)
	{
		mManager.GetComponent<MenuManager>().cameraSpeed = value;

		mManager.GetComponent<MenuManager>().currentSensitivity = value;

		mManager.GetComponent<MenuManager>().sensitivityPercent = (mManager.GetComponent<MenuManager>().currentSensitivity * 100);
		mManager.GetComponent<MenuManager>().sensitivityPercentText.text = mManager.GetComponent<MenuManager>().sensitivityPercent.ToString("F0") + "%";
	}

	private void MenuBack()
	{
		if (soundMenu.activeSelf)
		{
			mainMenu.SetActive(true);
			soundMenu.SetActive(false);
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
		}

		else if (videoMenu.activeSelf)
		{
			mainMenu.SetActive(true);
			videoMenu.SetActive(false);
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
		}

		//else if (gameplayMenu.activeSelf)
		//{
		//	mainMenu.SetActive(true);
		//	gameplayMenu.SetActive(false);
		//	GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
		//}
	}

	void CheckController()
	{
		GamePadState gamePadState = GamePad.GetState(0);
		hasController = gamePadState.IsConnected;
	}
}
