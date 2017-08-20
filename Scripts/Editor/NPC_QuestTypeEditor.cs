using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC_QuestType))]
[CanEditMultipleObjects]
public class NPC_QuestTypeEditor : Editor {

	// SURVIVE
	List<SurviveWave> surviveWave;

	bool[] DISP_emp;
	int[] DISP_numEn;
	string[] DISP_wavType;
	float[] DISP_timeLim;


	// COLLECT
	List<GameObject> collectObjects;
	List<int> amountCollectObjects;


	// Proprties
	SerializedProperty questTypeP;
	SerializedProperty tutorials;
	SerializedProperty useTimer;
	SerializedProperty useCutscene;

	#region SURVIVE
	SerializedProperty storedWaveListFileName;
	SerializedProperty numberOfWaves;
	SerializedProperty displayWaveInfo;
	SerializedProperty selectedWaveNumber;
	SerializedProperty numEnemies;
	SerializedProperty waveType;
	SerializedProperty timeLimit;
	#endregion

	#region GATHER
	SerializedProperty numberOfCandyRequired;
	//SerializedProperty candyContainer;
	#endregion

	#region PLATFORM
	SerializedProperty endPlatformGoal;
	SerializedProperty cutsceneScriptManager;
	#endregion

	#region BOSS
	SerializedProperty bossObject;
	#endregion

	#region EXPLORE
	SerializedProperty exploreCutsceneManager;
	//SerializedProperty exploreHintHUDObject;
	//SerializedProperty exploreHintText;
	SerializedProperty exploreUseCutscene;
	#endregion

	#region DEFEAT
	SerializedProperty defeatNumberOfEnemies;
	#endregion

	#region BALL

	SerializedProperty ballObj;
	SerializedProperty spawnLocationBall;

	#endregion



	void OnEnable() {

		questTypeP = serializedObject.FindProperty ("questType");
		tutorials = serializedObject.FindProperty ("tutorials");
		useTimer = serializedObject.FindProperty ("useTimer");
		useCutscene = serializedObject.FindProperty ("useCutscene");

		storedWaveListFileName = serializedObject.FindProperty ("storedWaveListFileName");
		numberOfWaves = serializedObject.FindProperty ("numberOfWaves");
		displayWaveInfo = serializedObject.FindProperty ("displayWaveInfo");
		selectedWaveNumber = serializedObject.FindProperty ("selectedWaveNumber");
		numEnemies = serializedObject.FindProperty ("numEnemies");
		waveType = serializedObject.FindProperty ("waveType");
		timeLimit = serializedObject.FindProperty ("timeLimit");

		numberOfCandyRequired = serializedObject.FindProperty ("numberOfCandyRequired");
		//candyContainer = serializedObject.FindProperty ("candyContainer");

		endPlatformGoal = serializedObject.FindProperty ("endPlatformGoal");
		cutsceneScriptManager = serializedObject.FindProperty ("cutsceneScriptManager");

		bossObject = serializedObject.FindProperty ("bossObject");

		defeatNumberOfEnemies = serializedObject.FindProperty ("defeatNumberOfEnemies");

		ballObj = serializedObject.FindProperty ("ballObj");
		spawnLocationBall = serializedObject.FindProperty ("spawnLocationBall");



		DISP_emp = new bool[5];
		DISP_numEn = new int[5];
		DISP_wavType = new string[5];
		DISP_timeLim = new float[5];

		// init surviveWave
		NPC_QuestType mainScript = target as NPC_QuestType;

		// Always load data
		mainScript.LoadData ();

		surviveWave = (List<SurviveWave>)mainScript.surviveWaveList;

		EditorApplication.update += Update;
	}

	void OnDisable() {
		EditorApplication.update -= Update;
	}

	void Update(){

		// init surviveWave
		NPC_QuestType mainScript = target as NPC_QuestType;

		bool loadFail = false;

		// Always load data
		if (!mainScript.LoadData ()) {
			loadFail = true;
		}

		surviveWave = (List<SurviveWave>)mainScript.surviveWaveList;

		if(!loadFail)
			for(int i = 0; i < 5; i++){

				DISP_emp [i] = surviveWave [i].empty;
				DISP_numEn [i] = surviveWave [i].numberOfEnemies;
				DISP_wavType [i] = surviveWave [i].enemyWaveType.ToString ();
				DISP_timeLim [i] = surviveWave [i].timeLimit;

			}
		else 
			for(int i = 0; i < 5; i++){

				DISP_emp [i] = true;
				DISP_numEn [i] = 0;
				DISP_wavType [i] = SurviveWave.EnemyWaveType.dustbunny.ToString();
				DISP_timeLim [i] = 0;

			}


	}



	public override void OnInspectorGUI(){

		// MUST INCLUDE
		serializedObject.Update ();

		NPC_QuestType mainScript = target as NPC_QuestType;

		surviveWave = (List<SurviveWave>)mainScript.surviveWaveList;


		// Style used for titles
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 16;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = new Color (0.2f, 0.4f, 0.1f);

		// Style used for titles
		GUIStyle warningStyle = new GUIStyle();
		warningStyle.fontSize = 16;
		warningStyle.fontStyle = FontStyle.Bold;
		warningStyle.font = (Font)Resources.Load ("Font/ConcertOne-Regular", typeof(Font));
		warningStyle.alignment = TextAnchor.MiddleCenter;
		warningStyle.normal.textColor = new Color (1.0f, 0.2f, 0.2f);


		GUILayout.Space(12.0f);

		EditorGUILayout.LabelField("Quest Status: " + mainScript.QuestStatus.ToString());

		GUILayout.Space(12.0f);

		EditorGUILayout.LabelField("QUESTS", titleStyle);

		GUILayout.Space(12.0f);

		EditorGUILayout.PropertyField (questTypeP, new GUIContent ("Quest Type: "));
		QUESTTYPE questType = (QUESTTYPE)mainScript.questType;

		// SURVIVE

		if (questType == QUESTTYPE.SURVIVE) {


			GUILayout.Space(24.0f);

			EditorGUILayout.LabelField ("DO NOT HAVE TWO OF THESE", warningStyle);
			EditorGUILayout.LabelField ("WITH THE SAME NAME", warningStyle);

			GUILayout.Space(24.0f);

			EditorGUILayout.PropertyField (storedWaveListFileName, new GUIContent ("Filename: "));


			// FILL WAVE LIST
			surviveWave = new List<SurviveWave> ();
			surviveWave.Insert(0, mainScript.SW1);
			surviveWave.Insert(1, mainScript.SW2);
			surviveWave.Insert(2, mainScript.SW3);
			surviveWave.Insert(3, mainScript.SW4);
			surviveWave.Insert(4, mainScript.SW5);



			GUILayout.Space(12.0f);

			EditorGUILayout.LabelField("WAVES", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (tutorials, new GUIContent ("Tutorial: "));

			if(mainScript.tutorials)
				EditorGUILayout.HelpBox ("This menu looks a bit scary, but it's really rather simple.\n\n" +
					"Starting off, you have Number of Waves.  This determines how many rounds of enemies the player will need to defeat before they complete the challenge.\n\n" +
					"One wave, or round, will be a group of enemies spawned in.  The next batch of enemies doesn't spawn until the first batch is completed.\n\n" +
					"The waves can be customized below the numbered selection.  Choose a specific wave to modify and, once completed, hit 'Add to Wave List' to add this data to the quest.\n\n" +
					"Feel free to click 'Reset Wave List' to dump all current data and start over, or 'Clear Spot in Wave List' if you've made a mistake and want to clear a single spot.\n\n" +
					"Finally, far below the settings you can see the wave information.  This displays your current wave setup so you know where you currently stand.", MessageType.Info);

			GUILayout.Space(18.0f);

			titleStyle.fontSize = 12;
			EditorGUILayout.LabelField("Number of Waves", titleStyle);

			GUILayout.Space(12.0f);


			EditorGUILayout.IntSlider (numberOfWaves, 1, mainScript.maxNumWaves, new GUIContent ("Num Waves: ","Number of waves for quest."));

			GUIContent[] displayedOptions = new GUIContent[5];
			displayedOptions [0] = new GUIContent ("One");
			displayedOptions [1] = new GUIContent ("Two");
			displayedOptions [2] = new GUIContent ("THree");
			displayedOptions [3] = new GUIContent ("Four");
			displayedOptions [4] = new GUIContent ("Five");
			int[] optionValues = new int[]{ 0, 1, 2, 3, 4 };
			EditorGUILayout.IntPopup (selectedWaveNumber, displayedOptions, optionValues, new GUIContent ("Select Wave: "));

			if(mainScript.numberOfWaves > 0) {

				GUILayout.Space(12.0f);

				titleStyle.fontSize = 14;
				titleStyle.normal.textColor = new Color (0.6f, 0.4f, 0.1f);
				EditorGUILayout.LabelField("Wave " + (mainScript.selectedWaveNumber + 1), titleStyle);

				GUILayout.Space(12.0f);

				EditorGUILayout.IntSlider (numEnemies, 1, 10, new GUIContent ("Num Enemies: ", "How many enemies the player fights during this wave."));

				EditorGUILayout.PropertyField (waveType, new GUIContent ("Wave Type: ", ""));

				EditorGUILayout.PropertyField (timeLimit, new GUIContent ("Time Limit: ", "Amount of time for the wave after which the quest fails."));

				if (GUILayout.Button ("ADD TO WAVE LIST")) {

					surviveWave.RemoveAt (mainScript.selectedWaveNumber);

					//SurviveWave survWave = new SurviveWave ();
					//survWave.empty = false;
					//survWave.numberOfEnemies = mainScript.numEnemies;
					//survWave.enemyWaveType = mainScript.waveType;
					//survWave.timeLimit = mainScript.timeLimit;
					//surviveWave.Insert (mainScript.selectedWaveNumber, survWave);

					if (mainScript.selectedWaveNumber == 0) {
						mainScript.SW1.empty = false;
						mainScript.SW1.numberOfEnemies = mainScript.numEnemies;
						mainScript.SW1.enemyWaveType = mainScript.waveType;
						mainScript.SW1.timeLimit = mainScript.timeLimit;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW1);
					}

					if (mainScript.selectedWaveNumber == 1) {
						mainScript.SW2.empty = false;
						mainScript.SW2.numberOfEnemies = mainScript.numEnemies;
						mainScript.SW2.enemyWaveType = mainScript.waveType;
						mainScript.SW2.timeLimit = mainScript.timeLimit;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW2);
					}

					if (mainScript.selectedWaveNumber == 2) {
						mainScript.SW3.empty = false;
						mainScript.SW3.numberOfEnemies = mainScript.numEnemies;
						mainScript.SW3.enemyWaveType = mainScript.waveType;
						mainScript.SW3.timeLimit = mainScript.timeLimit;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW3);
					}

					if (mainScript.selectedWaveNumber == 3) {
						mainScript.SW4.empty = false;
						mainScript.SW4.numberOfEnemies = mainScript.numEnemies;
						mainScript.SW4.enemyWaveType = mainScript.waveType;
						mainScript.SW4.timeLimit = mainScript.timeLimit;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW4);
					}

					if (mainScript.selectedWaveNumber == 4) {
						mainScript.SW5.empty = false;
						mainScript.SW5.numberOfEnemies = mainScript.numEnemies;
						mainScript.SW5.enemyWaveType = mainScript.waveType;
						mainScript.SW5.timeLimit = mainScript.timeLimit;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW5);
					}

					Debug.Log ("Inserted new wave into list for slot " + (mainScript.selectedWaveNumber+1) + ".");

					mainScript.surviveWaveList = surviveWave;
					mainScript.SaveData (); 

				}

				if (GUILayout.Button ("CLEAR SPOT IN WAVE LIST")) {

					surviveWave.RemoveAt (mainScript.selectedWaveNumber);

					//SurviveWave survWave = new SurviveWave ();
					//survWave.empty = false;
					//survWave.numberOfEnemies = mainScript.numEnemies;
					//survWave.enemyWaveType = mainScript.waveType;
					//survWave.timeLimit = mainScript.timeLimit;
					//surviveWave.Insert (mainScript.selectedWaveNumber, survWave);

					if (mainScript.selectedWaveNumber == 0) {
						mainScript.SW1.empty = true;
						mainScript.SW1.numberOfEnemies = 0;
						mainScript.SW5.enemyWaveType = SurviveWave.EnemyWaveType.dustbunny;
						mainScript.SW1.timeLimit = 0;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW1);
					}

					if (mainScript.selectedWaveNumber == 1) {
						mainScript.SW2.empty = true;
						mainScript.SW2.numberOfEnemies = 0;
						mainScript.SW5.enemyWaveType = SurviveWave.EnemyWaveType.dustbunny;
						mainScript.SW2.timeLimit = 0;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW2);
					}

					if (mainScript.selectedWaveNumber == 2) {
						mainScript.SW3.empty = true;
						mainScript.SW3.numberOfEnemies = 0;
						mainScript.SW5.enemyWaveType = SurviveWave.EnemyWaveType.dustbunny;
						mainScript.SW3.timeLimit = 0;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW3);
					}

					if (mainScript.selectedWaveNumber == 3) {
						mainScript.SW4.empty = true;
						mainScript.SW4.numberOfEnemies = 0;
						mainScript.SW5.enemyWaveType = SurviveWave.EnemyWaveType.dustbunny;
						mainScript.SW4.timeLimit = 0;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW4);
					}

					if (mainScript.selectedWaveNumber == 4) {
						mainScript.SW5.empty = true;
						mainScript.SW5.numberOfEnemies = 0;
						mainScript.SW5.enemyWaveType = SurviveWave.EnemyWaveType.dustbunny;
						mainScript.SW5.timeLimit = 0;
						surviveWave.Insert (mainScript.selectedWaveNumber, mainScript.SW5);
					}

					Debug.Log ("Deleted slot " + (mainScript.selectedWaveNumber+1) + ".");

					mainScript.surviveWaveList = surviveWave;
					mainScript.SaveData (); 

				}

				if (GUILayout.Button("RESET WAVE LIST")) {

					surviveWave = new List<SurviveWave> ();

					mainScript.SW1.empty = true;
					mainScript.SW2.empty = true;
					mainScript.SW3.empty = true;
					mainScript.SW4.empty = true;
					mainScript.SW5.empty = true;

					surviveWave.Insert(0, mainScript.SW1);
					surviveWave.Insert(1, mainScript.SW2);
					surviveWave.Insert(2, mainScript.SW3);
					surviveWave.Insert(3, mainScript.SW4);
					surviveWave.Insert(4, mainScript.SW5);

					Debug.Log ("Reset Wave List");

					// SAVE CHANGES
					mainScript.surviveWaveList = surviveWave;
					mainScript.SaveData (); 

				} 
			}


			GUILayout.Space(12.0f);

			titleStyle.fontSize = 12;
			EditorGUILayout.PropertyField (displayWaveInfo, new GUIContent ("Display Wave Information"));

			GUILayout.Space(12.0f);


			if (mainScript.displayWaveInfo) {
				for (int i = 0; i < 5; i++) {
					if (!DISP_emp [i]) {
						EditorGUILayout.LabelField ("Wave " + (i + 1));
						EditorGUILayout.LabelField ("Number of Enemies: " + DISP_numEn [i]);
						EditorGUILayout.LabelField ("Wave Type: " + DISP_wavType [i]);
						EditorGUILayout.LabelField ("Time Limit: " + DISP_timeLim [i]);
					} else {
						EditorGUILayout.LabelField ("Wave " + (i + 1));
						EditorGUILayout.LabelField ("EMPTY");
					}

					GUILayout.Space (6.0f);

				}
			}

		}

		if (questType == QUESTTYPE.COLLECT) {

			// dunno, just set it here...
			mainScript.numberOfDifferentObjects = 5;

			GUILayout.Space(24.0f);

			EditorGUILayout.LabelField("COLLECTABLE OBJECTS", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (tutorials, new GUIContent ("Tutorial: "));

			if(mainScript.tutorials)
				EditorGUILayout.HelpBox ("Drag in an object that identifies what you want the player to collect.\n\n" +
					"Example: Dragging in a coconut will tell the system that you want the player to find coconuts.\n\n" +
					"Reference objects must have QuestObject component attached to them, as well as any collectible versions of said object.", MessageType.Info);

			GUILayout.Space(18.0f);


			GUIStyle buttonStyle = new GUIStyle ("Button");
			buttonStyle.margin = new RectOffset (60, 60, 0, 0);

			EditorGUILayout.BeginHorizontal (buttonStyle);
			if(mainScript.collectObjects.Count < mainScript.numberOfDifferentObjects)
			if (GUILayout.Button ("Add New")) {

				mainScript.collectObjects.Add (null);
				mainScript.amountCollectObjects.Add (0);

			}

			if(mainScript.collectObjects.Count > 0)
			if (GUILayout.Button ("Remove Last")) {

				mainScript.collectObjects.RemoveAt (mainScript.collectObjects.Count - 1);
				mainScript.amountCollectObjects.RemoveAt (mainScript.amountCollectObjects.Count - 1);

			}
			EditorGUILayout.EndHorizontal ();

			// Check for correct object types
			for(int i = 0; i < mainScript.collectObjects.Count; i++) {

				if(mainScript.collectObjects[i] != null)
				if (mainScript.collectObjects[i].GetComponent<QuestObject> ()) {
					if (mainScript.collectObjects[i].GetComponent<QuestObject> ().questObjectType != QUESTOBJECTTYPE.collectible)
						mainScript.collectObjects[i] = null;
				}
				else mainScript.collectObjects[i] = null;

				for(int j = 0; j < mainScript.collectObjects.Count; j++) {

					if (mainScript.collectObjects [i] == mainScript.collectObjects [j] 
						&& mainScript.collectObjects[i] != null
						&& i != j)
						mainScript.collectObjects [j] = null;

				}
			}

			GUILayout.Space(6.0f);


			for (int i = 0; i < mainScript.collectObjects.Count; i++) {

				// GAMEOBJECTS

				GUIStyle buttonStyle3 = new GUIStyle ("Button");
				buttonStyle3.margin = new RectOffset (10, 10, 0, 0);

				EditorGUILayout.BeginVertical (buttonStyle3);


				// Label
				titleStyle.fontSize = 14;
				titleStyle.alignment = TextAnchor.MiddleLeft;
				EditorGUILayout.LabelField ("Collectible " + (i+1), titleStyle);


				EditorGUILayout.BeginHorizontal();

				mainScript.collectObjects [i] = (GameObject)EditorGUILayout.ObjectField (new GUIContent("Obj: ", "Specific type of object to collect.  Does not require the player to get the specific item selected; object serves as a reference."), mainScript.collectObjects[i], typeof(GameObject), true);

				GUIStyle buttonStyle2 = new GUIStyle ("Button");
				buttonStyle2.margin = new RectOffset (0, 0, 0, 0);
				buttonStyle2.fixedWidth = 20;
				if (GUILayout.Button (" - ", buttonStyle2)) {
					mainScript.collectObjects.RemoveAt (i);
					mainScript.amountCollectObjects.RemoveAt (i);
				}

				EditorGUILayout.EndHorizontal ();



				// NUMBERS

				mainScript.amountCollectObjects [i] = EditorGUILayout.IntField (new GUIContent("Num: ", "Number of specific object required."), mainScript.amountCollectObjects[i]);
				EditorGUILayout.EndVertical ();

				GUILayout.Space(4.0f);

			}
		}

		if (questType == QUESTTYPE.GATHER) {

			GUILayout.Space(24.0f);

			EditorGUILayout.LabelField("GATHER CANDY", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (tutorials, new GUIContent ("Tutorial: "));

			if(mainScript.tutorials)
				EditorGUILayout.HelpBox ("Gather Quests spawn candy for the player to collect.\n\n" + 
					"Once they collect enough, the spawned candy for that quest turn off and it gets completed.\n\n" +
					"ANY candy counts, though, so only one gather quest per zone is best.", MessageType.Info);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (numberOfCandyRequired, new GUIContent ("Candy Required: ", "The amount of candy required to turn in for quest completion."));

			//EditorGUILayout.PropertyField (candyContainer, new GUIContent ("Candy Container: ", "This gets turned on when quest starts; should contain the candy needed for quest."));

			GUILayout.Space(18.0f);

			GUILayout.Label ("Num in Container: " + mainScript.transform.GetChild(0).childCount);

			GUILayout.Space(18.0f);

		}

		if (questType == QUESTTYPE.EXPLORE) {

			GUILayout.Space(24.0f);

			EditorGUILayout.LabelField("EXPLORATION", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (tutorials, new GUIContent ("Tutorial: "));

			if(mainScript.tutorials)
				EditorGUILayout.HelpBox ("The goal of the 'Exploration' quest is to have the player explore around with little to no information as to where they need to go." +
					"\n\nYou need to position a trigger somewhere nearby in the map and have the player find it.  Below you set this trigger area under 'Hidden Location'" +
					"\n\nFor this quest, the player will have a hint displayed on-screen for them to get an idea of where to look.  You may also use a simple cutscene to provide a hint, although this makes it much easier as you can imagine.", MessageType.Info);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (endPlatformGoal, new GUIContent ("Hidden Location: ", "The 'Win Zone' location that is the goal of this quest."));

		}

		if (questType == QUESTTYPE.DEFEAT) {

			GUILayout.Space(24.0f);

			EditorGUILayout.LabelField("DEFEAT ENEMIES", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (tutorials, new GUIContent ("Tutorial: "));

			if(mainScript.tutorials)
				EditorGUILayout.HelpBox ("The player will need to defeat a specified number of 'QuestObject' of type 'basicenemy'" +
					"\n\nThese can be any enemies with the correct component attached, but if you require specific enemies to be defeat you can designate them further down." +
					"\n\nSpecific enemies will be force-required to be defeated as well as any remaining from the grand total." +
					"\n\nSpecific enemies count toward the overall total.", MessageType.Info);

			GUILayout.Space(18.0f);


			int maxEnemies = 50;

			EditorGUILayout.PropertyField(defeatNumberOfEnemies, new GUIContent ("Defeat to win: ", "Number of enemies player needs to defeat in order to beat quest."));

			if (mainScript.defeatNumberOfEnemies < 0)
				mainScript.defeatNumberOfEnemies = 0;
			else if (mainScript.defeatNumberOfEnemies > maxEnemies)
				mainScript.defeatNumberOfEnemies = maxEnemies;



			GUILayout.Space(16.0f);

			titleStyle.fontSize = 14;
			titleStyle.normal.textColor = new Color (0.5f, 0.3f, 0.4f, 1.0f);
			EditorGUILayout.LabelField("SPECIFIC ENEMIES", titleStyle);

			GUILayout.Space(16.0f);



			GUIStyle buttonStyle = new GUIStyle ("Button");
			buttonStyle.margin = new RectOffset (60, 60, 0, 0);



			EditorGUILayout.BeginHorizontal (buttonStyle);

			// add new blank
			if(mainScript.specificEnemiesList.Count < 20)	// max of twenty "bonus" enemies
			if (GUILayout.Button ("Add Slot")) {
				mainScript.specificEnemiesList.Add (null);
			}

			// remove last
			if(mainScript.specificEnemiesList.Count > 0)
			if (GUILayout.Button ("Remove")) {
				mainScript.specificEnemiesList.RemoveAt (mainScript.specificEnemiesList.Count - 1);
			}

			EditorGUILayout.EndHorizontal ();


			GUILayout.Space(16.0f);


			GUIStyle buttonStyle3 = new GUIStyle ("Button");
			buttonStyle3.margin = new RectOffset (10, 10, 0, 0);

			for (int i = 0; i < mainScript.specificEnemiesList.Count; i++) {

				EditorGUILayout.BeginHorizontal (buttonStyle3);



				GUIStyle labelStyle = new GUIStyle ("Button");
				labelStyle.padding = new RectOffset (-100, 0, 0, 0);
				EditorGUILayout.LabelField ("Enemy " + (i + 1) + ": ", labelStyle);

				GUILayout.Space(-110.0f);

				mainScript.specificEnemiesList[i] = (GameObject)EditorGUILayout.ObjectField (new GUIContent ("", "Extra enemy that must be defeated.  Counts toward the total."), 
					mainScript.specificEnemiesList[i], typeof(GameObject), true);


				GUIStyle buttonStyle2 = new GUIStyle ("Button");
				buttonStyle2.margin = new RectOffset (0, 0, 0, 0);
				buttonStyle2.fixedWidth = 20;

				if (GUILayout.Button (" - ", buttonStyle2)) {
					mainScript.specificEnemiesList.RemoveAt (i);
				}

				EditorGUILayout.EndHorizontal ();


				//GUILayout.Space(8.0f);

			}



		}

		if (questType == QUESTTYPE.BOSS) {



			GUILayout.Space(24.0f);

			EditorGUILayout.LabelField("BOSS", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (tutorials, new GUIContent ("Tutorial: "));

			if(mainScript.tutorials)
				EditorGUILayout.HelpBox ("Blah blah blah", MessageType.Info);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (bossObject, new GUIContent("Boss Object: ", "Place the enemy that the player must defeat here.  Quest completes once the enemy is defeated."));

			if(mainScript.bossObject != null)
			if (mainScript.bossObject.GetComponent<QuestObject> ()) {
				if (mainScript.bossObject.GetComponent<QuestObject> ().questObjectType != QUESTOBJECTTYPE.bossenemy)
					mainScript.bossObject = null;
			}
			else mainScript.bossObject = null;

			GUILayout.Space(6.0f);

			EditorGUILayout.PropertyField(useTimer, new GUIContent("Use Timer ? ", "Check this if you want to use a timer for the boss quest."));

		}

		if (questType == QUESTTYPE.PLATFORM) {


			GUILayout.Space(24.0f);

			EditorGUILayout.LabelField("PLATFORMING", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (tutorials, new GUIContent ("Tutorial: "));

			if(mainScript.tutorials)
				EditorGUILayout.HelpBox ("Player will see a cutscene showing them the path to the Goal Location and then they will need to get to the goal location to complete the quest.\n\nA time limit is entirely optional.", MessageType.Info);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (endPlatformGoal, new GUIContent("Goal Location: ", "Win-zone goal location of the quest.  Upon entering, quest is completed."));

			GUILayout.Space(6.0f);

			EditorGUILayout.PropertyField (useTimer, new GUIContent ("Use Timer: "));

		}

		if (questType == QUESTTYPE.BALL) {


			GUILayout.Space(24.0f);

			EditorGUILayout.LabelField("PLATFORMING", titleStyle);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (tutorials, new GUIContent ("Tutorial: "));

			if(mainScript.tutorials)
				EditorGUILayout.HelpBox ("Player will see a cutscene showing them the path to the Goal Location and then they will need to get the ball to the goal location to complete the quest.\n\nA time limit is entirely optional.", MessageType.Info);

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (ballObj, new GUIContent("Ball: ", "The ball used for the quest goal"));

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (spawnLocationBall, new GUIContent("Ball Spawn: ", "The place the ball spawns"));

			GUILayout.Space(18.0f);

			EditorGUILayout.PropertyField (endPlatformGoal, new GUIContent("Goal Location: ", "Win-zone goal location of the quest.  Upon ball entering, quest is completed."));

			GUILayout.Space(6.0f);

			EditorGUILayout.PropertyField (useTimer, new GUIContent ("Use Timer: "));

		}




		GUILayout.Space(6.0f);

		EditorGUILayout.PropertyField (useCutscene, new GUIContent ("Use Cutscene: "));

		// If there's a cutscene, place it here.
		if (mainScript.useCutscene) {
			GUILayout.Space (6.0f);
			EditorGUILayout.PropertyField (cutsceneScriptManager, new GUIContent ("Cutscene Manager: ", "Cutscene to show the end goal."));
		} else mainScript.cutsceneScriptManager = null;

		// MUST INCLUDE
		serializedObject.ApplyModifiedProperties ();



	}

}