using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
	public static SoundManager instance = null;

	[SerializeField] AudioSource soundSource;
	[SerializeField] AudioSource musicSource;
	[SerializeField] AudioSource soundSourceLooping;

	Dictionary<string, AudioClip> sounds;

	public void SetSfxMute(bool mute) { soundSource.mute = mute; soundSourceLooping.mute = mute; }
	public void SetMusicMute(bool mute) { musicSource.mute = mute; }

	public void SetSfxVolume(float volume) { soundSource.volume = volume; soundSourceLooping.volume = volume; }
	public float GetSfxVolume { get { return soundSource.volume; } }

	public void SetMusicVolume(float volume) { musicSource.volume = volume; }
	public float GetMusicVolume { get { return musicSource.volume; } }

	public bool HasLoopingClip { get { return soundSourceLooping.clip != null; } }

	string[,] LevelToSong = new string[,] { { "StartScene", "CamdenWorlds_LOOP" }, { "Puzzle_PlatformMayhem", "CamdenWorlds_LOOP" }, 
		{ "Puzzle_PushyBallAdventure", "CamdenWorlds_LOOP" }, { "Puzzle_RushHour", "CamdenWorlds_LOOP" }, { "Tutorial Zone", "DustyHours_LOOP" }, 
		{ "Level Shell", "FaithfulMusings_LOOP" }, { "Pickup Zone", "LunarPrince_LOOP" }, { "Mountain Zone", "SweetExpansion_LOOP" } };

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		LoadAudio();
		SceneManager.sceneLoaded += SceneLoaded;

		DontDestroyOnLoad(gameObject);
	}

	void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		for (int i = 0; i < LevelToSong.GetLength(0); i++)
		{
			if (scene.name == LevelToSong[i, 0])
			{
				SwitchMusic(LevelToSong[i, 1]);
				return;
			}
		}
	}

	void LoadAudio()
	{
		AudioClip[] loadedSounds = Resources.LoadAll<AudioClip>("Sounds");
		sounds = new Dictionary<string, AudioClip>();

		for (int i = 0; i < loadedSounds.Length; i++)
		{
			sounds.Add(loadedSounds[i].name, loadedSounds[i]);
		}
	}

	void RandomizeMusic()
	{
		string[] musics = new string[] { "CamdenWorlds_LOOP", "DustyHours_LOOP", "FaithfulMusings_LOOP",
			"LunarPrince_LOOP", "SweetExpansion_LOOP" };

		int r = Random.Range(0, musics.Length);
		SwitchMusic(musics[r]);
	}

	public void PlayClip(string clip, float volumeScale = 1)
	{
		if (clip.Contains("CQ") && PlayerHandler.JumpString == "JumpSecondary")
			return;

		soundSource.PlayOneShot(sounds[clip], volumeScale);
	}

	public void SwitchMusic(string clip)
	{
		AudioClip musicClip = Resources.Load<AudioClip>("Music/" + clip);
		musicSource.clip = musicClip;
		musicSource.Play();
	}

	public void PlayClipLooping(string clip, float volumeScale = 1)
	{
		StopCoroutine("_FadeOutLoopingClip");
		StartCoroutine("LoopingClipWaitForPause");

		soundSourceLooping.clip = sounds[clip];
		soundSourceLooping.volume = soundSource.volume;		// set this as soundSource volume to change with setting they may have changed
		soundSourceLooping.Play();
	}

	public void EndClipLooping()
	{
		StopCoroutine("_FadeOutLoopingClip");
		StopCoroutine("LoopingClipWaitForPause");

		soundSourceLooping.Stop();
		soundSourceLooping.clip = null;
	}

	public void FadeOutLoopingClip()
	{
		StopCoroutine("_FadeOutLoopingClip");
		StartCoroutine("_FadeOutLoopingClip");
	}

	IEnumerator _FadeOutLoopingClip()
	{
		while (soundSourceLooping.volume > 0.2f)
		{
			soundSourceLooping.volume -= 5 * Time.deltaTime;
			yield return null;
		}

		soundSourceLooping.volume = 0;
		EndClipLooping();
	}

	IEnumerator LoopingClipWaitForPause()
	{
		while (Time.timeScale != 0)
		{
			yield return null;
		}

		soundSourceLooping.Pause();

		while (Time.timeScale == 0)
		{
			yield return null;
		}

		soundSourceLooping.volume = soundSource.volume;     // set this as soundSource volume to change with setting they may have changed
		soundSourceLooping.Play();

		StartCoroutine("LoopingClipWaitForPause");
	}
}
