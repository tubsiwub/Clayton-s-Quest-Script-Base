using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager instance = null;

	[SerializeField] AudioSource soundSource;
	[SerializeField] AudioSource musicSource;

	Dictionary<string, AudioClip> sounds;

	public void SetSfxMute(bool mute) { soundSource.mute = mute; }
	public void SetMusicMute(bool mute) { musicSource.mute = mute; }

	public void SetSfxVolume(float volume) { soundSource.volume = volume; }
	public float GetSfxVolume { get { return soundSource.volume; } }

	public void SetMusicVolume(float volume) { musicSource.volume = volume; }
	public float GetMusicVolume { get { return musicSource.volume; } }

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
		LoadAudio();

		musicSource.volume = 0.15f;
	}

	void LoadAudio()
	{
		AudioClip[] loadedSounds = Resources.LoadAll<AudioClip>("Sounds");
		sounds = new Dictionary<string, AudioClip>();

		for (int i = 0; i < loadedSounds.Length; i++)
		{
			sounds.Add(loadedSounds[i].name, loadedSounds[i]);
		}

		RandomizeMusic();
	}

	void RandomizeMusic()
	{
		string[] musics = new string[] { "CamdenWorlds_LOOP", "DustyHours_LOOP", "FaithfulMusings_LOOP",
			"LunarPrince_LOOP", "SweetExpansion_LOOP" };

		int r = Random.Range(0, musics.Length);
		SwitchMusic(musics[r]);
	}

	void Update()
	{
		//if (Input.GetKeyDown(KeyCode.N))
		//	soundSource.mute = !soundSource.mute;
		//
		//if (Input.GetKeyDown(KeyCode.M))
		//	musicSource.mute = !musicSource.mute;

		if (Input.GetKeyDown(KeyCode.F11))
		{
			RandomizeMusic();
			print("Music randomized: " + musicSource.clip.name);
		}
	}

	public void PlayClip(string clip, float volumeScale = 1)
	{
		soundSource.PlayOneShot(sounds[clip], volumeScale);
	}

	public void SwitchMusic(string clip)
	{
		AudioClip musicClip = Resources.Load<AudioClip>("Music/" + clip);
		musicSource.clip = musicClip;
		musicSource.Play();
	}
}
