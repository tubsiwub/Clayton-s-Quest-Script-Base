using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public class MenuManager : MonoBehaviour
{

	public static MenuManager instance = null;

	public GameObject music;
	public GameObject sfx;
	public GameObject mSlider;
	public GameObject sfxSlider;

	public GameObject mainMenu;
	public GameObject optionsMenu;
	public GameObject soundMenu;
	public GameObject videoMenu;
	public GameObject cameraMenu;
	public GameObject quitMenu;
	public GameObject mainSelected;
	public GameObject optionsSelected;
	public GameObject soundSelected;
	public GameObject videoSelected;
	public GameObject cameraSelected;
	public GameObject quitSelected;
	//temp mail object
	public GameObject tempMail;
	public GameObject tempMailSelected;

	public GameObject musicOn;
	public GameObject musicOff;
	public GameObject sfxOn;
	public GameObject sfxOff;

	public Slider[] volumeSliders;

	public Toggle[] resolutionToggles;
	public Toggle[] aliasToggles;
	public Toggle vSyncToggle;
	public Toggle muteMusic;
	public Toggle muteSfx;
	public Toggle fullScreenToggle;

	public int[] screenWidths;
	public int[] screenHeights;

	//int activeScreenResIndex;
	int activeAliasIndex;

	float mouseSpeed;
	float maxMusicVolume = 1.0f;
	float maxSFXVolume = 1.0f;
	float currentMusicVolume;
	float currentSFXVolume;
	float musicPercent;
	public float soundPercent;
	public float currentSensitivity;
	public float sensitivityPercent;
	public float cameraSpeed = 1.0f;

	bool menuActive = false;
	bool hasController;
	public bool vLook = false;
	public bool hLook = false;
	public bool cRotate = false;
	public bool musicIsMute = false;
	public bool soundIsMute = false;
	//public bool fullScreen;
	public bool vSyncOn = true;

	public Dropdown resolutionDropdown;

	public Text musicPercentText;
	public Text soundPercentText;
	public Text sensitivityPercentText;

	void Awake()
	{
		if (instance == null)
			instance = this;

		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
		if (!GameObject.Find("EventSystem"))
			Debug.LogWarning("Hi! Roger here! You are missing an event system in your scene. \n Please put one in for the menus to work properly. Thank you! Have a Nice Day!!!");

		mainMenu.SetActive(false);
		soundMenu.SetActive(false);
		videoMenu.SetActive(false);
		cameraMenu.SetActive(false);

		CheckController();
	}

	void Update()
	{
		if (!GameObject.Find("Start Menu") && Input.GetKeyDown("escape"))
		{
			PauseMenu();
		}

		if (!GameObject.Find("Start Menu") && Input.GetButtonDown("Start"))
		{
			PauseMenu();
		}

		if (Input.GetButtonDown("Cancel"))
		{
			MenuBack();
		}

		if (Screen.fullScreen)
			resolutionDropdown.interactable = false;
		else
			resolutionDropdown.interactable = true;

		if (vSyncToggle.isOn == true)
			QualitySettings.vSyncCount = 1;

		//checking if fullscreen or not
		if (fullScreenToggle.isOn == true)
			Screen.fullScreen = true;
		else if (fullScreenToggle.isOn == false)
			Screen.fullScreen = false;

		//camera stuff
		if (vLook)
			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().InvertY = true;
		else
			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().InvertY = false;

		if (hLook)
			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().InvertX = true;
		else
			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().InvertX = false;

		if (cRotate)
			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().AutoRotate = true;
		else
			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().AutoRotate = false;

		//GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().CameraSpeed = cameraSpeed;

	}

	//Main Menu stuff
	public void ReturnToGame()
	{
		PauseMenu();
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void OptionsMenu()
	{
		SoundManager.instance.PlayClip("CQMenuSelect", 1);

		mainMenu.SetActive(false);
		optionsMenu.SetActive(true);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(optionsSelected);
	}

	public void Quit()
	{
		SoundManager.instance.PlayClip("CQMenuSelect", 1);

		mainMenu.SetActive(false);
		quitMenu.SetActive(true);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(quitSelected);
	}
	
	//Quit menu stuff
	public void QuitYes()
	{
		SoundManager.instance.PlayClip("meow", 1);
		Application.Quit();
	}

	public void QuitNo()
	{
		SoundManager.instance.PlayClip("CQMenuDeselect", 1);

		mainMenu.SetActive(true);
		quitMenu.SetActive(false);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
	}


	// options menu stuff

	public void OptionsReturn()
	{
		SoundManager.instance.PlayClip("CQMenuDeselect", 1);

		mainMenu.SetActive(true);
		optionsMenu.SetActive(false);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
	}
	public void SoundMenu()
	{
		SoundManager.instance.PlayClip("CQMenuSelect", 1);

		optionsMenu.SetActive(false);
		soundMenu.SetActive(true);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(soundSelected);

		mSlider.GetComponent<Slider>().value = SoundManager.instance.GetMusicVolume;
		sfxSlider.GetComponent<Slider>().value = SoundManager.instance.GetSfxVolume;

		if (musicIsMute)
		{
			music.GetComponent<Toggle>().isOn = true;
			musicOn.SetActive(false);
			musicOff.SetActive(true);
		}

		else
		{
			music.GetComponent<Toggle>().isOn = false;
			musicOn.SetActive(true);
			musicOff.SetActive(false);
		}

		if (soundIsMute)
		{
			sfx.GetComponent<Toggle>().isOn = true;
			sfxOn.SetActive(false);
			sfxOff.SetActive(true);
		}

		else
		{
			sfx.GetComponent<Toggle>().isOn = false;
			sfxOn.SetActive(true);
			sfxOff.SetActive(false);
		}

		//Get Current sfx %
		currentSFXVolume = SoundManager.instance.GetSfxVolume;
		soundPercent = (currentSFXVolume / maxSFXVolume) * 100;
		soundPercentText.text = soundPercent.ToString("F0") + "%";

		//Get Current Music %
		currentMusicVolume = SoundManager.instance.GetMusicVolume;
		musicPercent = (currentMusicVolume / maxMusicVolume) * 100;
		musicPercentText.text = musicPercent.ToString("F0") + "%";
	}

	public void VideoMenu()
	{
		SoundManager.instance.PlayClip("CQMenuSelect", 1);

		optionsMenu.SetActive(false);
		videoMenu.SetActive(true);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(videoSelected);

		//checking current resolution
		for (int i = 0; i < 15; i++)
		{
			if (Screen.width == screenWidths[i] && Screen.height == screenHeights[i])
			{
				resolutionDropdown.value = i;
			}
		}

		//checking if vsync on
		if (vSyncOn)
		{
			vSyncToggle.isOn = true;
			QualitySettings.vSyncCount = 1;
		}

		else
		{
			vSyncToggle.isOn = false;
			QualitySettings.vSyncCount = 0;
		}

		//checking level of antialiasing
		if (QualitySettings.antiAliasing == 0)
			aliasToggles[0].isOn = true;

		if (QualitySettings.antiAliasing == 2)
			aliasToggles[1].isOn = true;

		if (QualitySettings.antiAliasing == 4)
			aliasToggles[2].isOn = true;

		if (QualitySettings.antiAliasing == 8)
			aliasToggles[3].isOn = true;
	}

	public void CameraMenu()
	{
		SoundManager.instance.PlayClip("CQMenuSelect", 1);

		optionsMenu.SetActive(false);
		cameraMenu.SetActive(true);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(cameraSelected);
	}

	// Sound Menu Stuff
	public void SoundReturn()
	{
		SoundManager.instance.PlayClip("CQMenuDeselect", 1);

		optionsMenu.SetActive(true);
		soundMenu.SetActive(false);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(optionsSelected);
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
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			SoundManager.instance.SetMusicMute(true);
			musicIsMute = true;

			musicOn.SetActive(false);
			musicOff.SetActive(true);

			PlayerPrefs.SetInt("music muted", 1);
			PlayerPrefs.Save();
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			SoundManager.instance.SetMusicMute(false);
			musicIsMute = false;

			musicOn.SetActive(true);
			musicOff.SetActive(false);

			PlayerPrefs.SetInt("music muted", 0);
			PlayerPrefs.Save();
		}
	}

	public void MuteSfx()
	{
		if (muteSfx.isOn)
		{
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			SoundManager.instance.SetSfxMute(true);
			soundIsMute = true;

			sfxOn.SetActive(false);
			sfxOff.SetActive(true);

			PlayerPrefs.SetInt("sfx muted", 1);
			PlayerPrefs.Save();
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			SoundManager.instance.SetSfxMute(false);
			soundIsMute = false;

			sfxOn.SetActive(true);
			sfxOff.SetActive(false);

			PlayerPrefs.SetInt("sfx muted", 0);
			PlayerPrefs.Save();
		}
	}

	//Video Menu stuff
	public void VideoReturn()
	{
		SoundManager.instance.PlayClip("CQMenuDeselect", 1);

		optionsMenu.SetActive(true);
		videoMenu.SetActive(false);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(optionsSelected);
	}

	public void SetScreenResolution(int i)
	{
		//activeScreenResIndex = i;

		//SoundManager.instance.PlayClip("CQMenuSelect", 1);

		Screen.SetResolution(screenWidths[i], screenHeights[i], false);
		resolutionDropdown.value = i;

		//PlayerPrefs.SetInt("screen res index", activeScreenResIndex);
		//PlayerPrefs.Save();
	}

	//public void SetScreenResolutionFromFullscreen(int i)
	//{
	//	Screen.SetResolution(screenWidths[i], screenHeights[i], false);
	//	resolutionDropdown.value = i;
	//}

	public void SetFullScreen(bool isFullscreen)
	{
		if (isFullscreen)
		{
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			Resolution[] allResolutions = Screen.resolutions;
			Resolution maxResolution = allResolutions[allResolutions.Length - 1];
			Screen.SetResolution(maxResolution.width, maxResolution.height, true);
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			SetScreenResolution(resolutionDropdown.value);

		}

		//PlayerPrefs.SetInt("fullscreen", ((isFullscreen) ? 1 : 0));
		//PlayerPrefs.Save();
	}

	public void SetAntialiasing(int i)
	{
		//SoundManager.instance.PlayClip("CQMenuSelect", 1);

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
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			QualitySettings.vSyncCount = 1;
			vSyncOn = true;
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			QualitySettings.vSyncCount = 0;
			vSyncOn = false;
		}

		PlayerPrefs.SetInt("vSync", ((isVSync) ? 1 : 0));
		PlayerPrefs.Save();
	}

	//Gameplay Options
	public void CameraReturn()
	{
		SoundManager.instance.PlayClip("CQMenuDeselect", 1);

		optionsMenu.SetActive(true);
		cameraMenu.SetActive(false);

		if (hasController)
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(optionsSelected);
	}

	public void InvertY()
	{
		if (!vLook)
		{
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().InvertY = true;
			vLook = true;
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().InvertY = false;
			vLook = false;
		}
	}

	public void InvertX()
	{
		if (!hLook)
		{
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().InvertX = true;
			hLook = true;
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().InvertX = false;
			hLook = false;
		}
	}

	public void AutoRotate()
	{
		if (!cRotate)
		{
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().AutoRotate = true;
			cRotate = true;
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().AutoRotate = false;
			cRotate = false;
		}
	}

	public void Rumble(bool isRumble)
	{
		if (isRumble)
		{
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			PlayerHandler.AllowVibration = true;
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			PlayerHandler.AllowVibration = false;
		}

	}

	public void ClicktoJump(bool isJump)
	{
		if (isJump)
		{
			//SoundManager.instance.PlayClip("CQMenuSelect", 1);

			PlayerHandler.SetClickToJump(true);
		}

		else
		{
			//SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			PlayerHandler.SetClickToJump(false);
		}
	}

	public void SetSensetivity(float value)
	{
		cameraSpeed = value;
		//GameObject.FindWithTag ("MainCamera").GetComponent<CameraControlDeluxe>().CameraSpeed = value;

		currentSensitivity = value;

		sensitivityPercent = (currentSensitivity * 100);
		sensitivityPercentText.text = sensitivityPercent.ToString("F0") + "%";
	}

	//function for opening menu
	private void PauseMenu()
	{
		if (menuActive == false)
		{
			mainMenu.SetActive(true);
			soundMenu.SetActive(false);
			videoMenu.SetActive(false);
			cameraMenu.SetActive(false);
			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().SetFreeze(true);

			SoundManager.instance.PlayClip("CQMenuSelect", 1);

			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);

			if (hasController)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}

			MarbleManager.instance.ShowText();
			menuActive = true;

			Time.timeScale = 0;
		}

		else if (menuActive == true && mainMenu.activeSelf)
		{
			SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			mainMenu.SetActive(false);
			GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().SetFreeze(false);
			MarbleManager.instance.HideText();
			menuActive = false;

			Time.timeScale = 1;
		}
	}

	private void MenuBack()
	{
		//if (menuActive == true && mainMenu.activeSelf)
		//{
		//	mainMenu.SetActive(false);
		//	GameObject.FindWithTag("MainCamera").GetComponent<CameraControlDeluxe>().SetFreeze(false);
		//	MarbleManager.instance.HideText();
		//	menuActive = false;

		//	Time.timeScale = 1;
		//}

		if (optionsMenu.activeSelf)
		{
			SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			mainMenu.SetActive(true);
			optionsMenu.SetActive(false);

			if (hasController)
				GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
		}

		else if (soundMenu.activeSelf)
		{
			SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			optionsMenu.SetActive(true);
			soundMenu.SetActive(false);

			if (hasController)
				GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(optionsSelected);
		}

		else if (videoMenu.activeSelf)
		{
			SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			optionsMenu.SetActive(true);
			videoMenu.SetActive(false);

			if (hasController)
				GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(optionsSelected);
		}

		else if (cameraMenu.activeSelf)
		{
			SoundManager.instance.PlayClip("CQMenuDeselect", 1);

			optionsMenu.SetActive(true);
			cameraMenu.SetActive(false);

			if (hasController)
				GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(optionsSelected);
		}
	}

	void CheckController()
	{
		GamePadState gamePadState = GamePad.GetState(0);
		hasController = gamePadState.IsConnected;
	}

	//TEMP - Feedback stuff
	public void EmailUs()
	{
		mainMenu.SetActive(false);
		tempMail.SetActive(true);
		GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(tempMailSelected);
		Cursor.lockState = CursorLockMode.None;
	}
}
