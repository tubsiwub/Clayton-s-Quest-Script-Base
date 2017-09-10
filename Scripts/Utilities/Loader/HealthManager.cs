using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
	public static HealthManager instance = null;

	[SerializeField] GameObject hudFaceGuyObj;

	PlayerHandler playerHandler;
	PlayerHealthBlink playerHealthBlink;
	HUDFaceGuy hudFaceGuy;
	HealthPie healthPie;

	public static int totalLives = 5;
	int lives = totalLives;
	public int Lives { get { return lives; } }

	bool waitingForScreenTrans = false;

	enum HealthState { Normal, Invincible, Paused };
	HealthState healthState;

	public enum AnimType { Default, Drown, Splat, BallToHuman, PuzzleFail, None }

	public bool IsInvincible { get { return healthState == HealthState.Invincible; } }
	// health state is paused while the death animation is playing
	public bool IsPaused { get { return healthState == HealthState.Paused; } }
	public bool HUDActive { get { return healthPie.gameObject.activeInHierarchy; } }
	public bool CanGetHurt { get { return healthState == HealthState.Normal && 
		!externalIsInvincible && !playerHandler.IsFrozen && HUDActive; } }
	const float respawnWaitTime = 0.3f;

	bool externalIsInvincible = false;
	public void SetExternalIsInvincible(bool set) { externalIsInvincible = set; }

	public delegate void DieEvent();
	public event DieEvent OnDeath;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SceneLoaded;

		ScreenTransition.OnDoneForward += ScreenTransitionDone;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Minus))
			LoseALife();
	}

	void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex != 0)
			SavingLoading.instance.LoadLives();

		FindRefs();

		if (healthPie)
			healthPie.LoseBars(totalLives - lives);

		hudFaceGuy.SetFace(lives);
	}

	void FindRefs()
	{
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
		playerHealthBlink = GameObject.FindWithTag("Player").GetComponent<PlayerHealthBlink>();

		GameObject pie = GameObject.Find("HealthPie");
		if (pie) healthPie = pie.GetComponent<HealthPie>();

		if (hudFaceGuy == null)
		{
			hudFaceGuy = Instantiate(hudFaceGuyObj, new Vector3(0, 1000, 0),
				Quaternion.identity).GetComponent<HUDFaceGuy>();
		}
	}

	public void DoneBlinking()
	{
		healthState = HealthState.Normal;
	}

	void InternalLifeLose(int amount)
	{
		if (!CanGetHurt) return;

		lives -= amount;
		if (lives < 0) lives = 0;

		if (healthPie)
			healthPie.LoseBars(totalLives - lives);

		healthState = HealthState.Invincible;

		if (lives > 0)
		{
			playerHealthBlink.StartBlinking(lives >= 1);
			SoundManager.instance.PlayClip("GettingHurt0" + Random.Range(1, 3));
		}
		else StartCoroutine(PauseThenRespawn(AnimType.Default));

		hudFaceGuy.SetFace(lives);
		SavingLoading.instance.SaveLives(lives);
		SavingLoading.instance.SaveData();
	}

	public void LoseALife()
	{
		InternalLifeLose(1);
	}

	public void LoseLives(int amount)
	{
		InternalLifeLose(amount);
	}

	public void LoseALifeAndPushAway(Vector3 direction, float force)
	{
		if (!CanGetHurt) return;

		LoseALife();

		if (lives > 0)
			playerHandler.PushAway(direction, force);
	}

	public void LoseLivesAndPushAway(Vector3 direction, float force, int amount)
	{
		if (!CanGetHurt) return;

		InternalLifeLose(amount);

		if (lives > 0)
			playerHandler.PushAway(direction, force);
	}

	public void LoseAllLives(AnimType animType = AnimType.Default)
	{
		if (!CanGetHurt)
		{
			if (animType != playerHandler.LastDeathAnim)
				playerHandler.SetDeathAnimation(animType);

			return;
		}

		StartCoroutine(PauseThenRespawn(animType));
		lives = 0;

		if (healthPie) healthPie.LoseAllBars();
		hudFaceGuy.SetFace(lives);
		SavingLoading.instance.SaveLives(lives);
		SavingLoading.instance.SaveData();
	}

	public void RegainLives(int amount)
	{
		lives += amount;
		if (lives > totalLives) lives = totalLives;

		if (healthPie) healthPie.RegainBars(lives-1);
		hudFaceGuy.SetFace(lives);
		SavingLoading.instance.SaveLives(lives);
		SavingLoading.instance.SaveData();
	}

	public void SetLives(int lives)
	{
		this.lives = lives;
	}

	IEnumerator PauseThenRespawn(AnimType animType)
	{
		if (OnDeath != null)
			OnDeath();

		healthState = HealthState.Paused;
		playerHealthBlink.StopBlinking();

		if (playerHandler.CurrentState == PlayerHandler.PlayerState.Ball && animType == AnimType.Default)
			animType = AnimType.BallToHuman;

		if (animType != AnimType.None)
			playerHandler.SetDeathAnimation(animType);
		else
		{
			playerHealthBlink.SetTint(true, false);
			playerHandler.SetFrozen(true, false);
		}
		
		// if we can't turn into death mesh, handle transition through timer
		yield return new WaitForSeconds(respawnWaitTime);
		if (animType == AnimType.None) DoRespawnStuff();
	}

	public void DoRespawnStuff()
	{
		Camera.main.GetComponent<ScreenTransition>().Forward(2, "circle_pattern");
		waitingForScreenTrans = true;
	}

	void ScreenTransitionDone()
	{
		if (!waitingForScreenTrans) return;

		lives = totalLives;
		if (healthPie) healthPie.ResetBars();
		hudFaceGuy.SetFace(lives);
		SavingLoading.instance.SaveLives(lives);
		SavingLoading.instance.SaveData();

		playerHandler.SetDeathAnimation(AnimType.None);
		playerHandler.Respawn();

		healthState = HealthState.Invincible;
		playerHealthBlink.StartBlinking(false);

		GameObject cam = Camera.main.gameObject;
		cam.GetComponent<CameraControlDeluxe>().SetToPlayer();
		cam.GetComponent<ScreenTransition>().WaitThenBackward(0.25f, 2, "topbottom_pattern");
		waitingForScreenTrans = false;
	}
}
