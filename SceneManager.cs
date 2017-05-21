using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine.Analytics;

public class SceneManager : MonoBehaviour {

	private GameObject runner;

	private int highestLevelFinished = 0;
	private int numLevelsPlayedThisGame= 0;
	private int numPlatformsCreatedThisLevel;
	public int numTimesJumpedThisLevel;
	private int numPlatformsCreatedThisGame;
	private int numTimesJumpedThisGame;
	private int numPoints = 0;
	private string highestScoringWordThisGame = "";
	private int highestScoringWordPointThisGame = 0;
	public bool playerRelaxed = false; // relaxed if there are enough platforms to get to the goal
	public bool playerStressed = false; // if runner is on the last platform, freak out

	public string gameState = "starting";	// playing, goalreached, felltodeath, gameover
	public string howLevelEnded = "";
	public int numLivesLeft = 1;
	public int swapsAvailable = 3;
	public int levelNum = 1;
	public bool watchedAd = false;

	public float platformsXStart = -5.6f;
	public float platformsYStart = 1f;
	public float platformSpacing = 2.6f;
	public float platformWidth = 1f;
	public GameObject platformObj;
	public int numPlatformsOffScreen = 0;
	private int startingNumOfPlatforms = 6;
	private int numPlatformsWithSwap = 2;
	public float platformSpeed = -0.4f;

	public GameObject[] playerPrefab;
	public Sprite[] livesIcon;
	[HideInInspector] public int currPlayerPrefab;
	private Sprite platformSprite;
	private float platformYOffset = 0f;

	public GameObject tileExplodePrefab;
	//public GameObject playerPrefab;
	public GameObject runnerGoalPrefab;
	public GameObject enoughPlatformsPrefab;
	private GameObject canvas;
	public GameObject letterTileGlow;
	public GameObject socialPanel;
	public GameCenter gcMgr;

	[HideInInspector] public SwapTiles swapMgr;
	[HideInInspector] public AdManager adMgr;
	public Text levelNumText;
	public Text newHighScore;
	public Text points;
	public Text highestScore;
	public Text currWord;
	public Text platformsOffscreen;
	public Text livesLeft;
	public Button restartButton;
	public Button goButton;
	public Button delButton;
	public Button trashButton;
	public Button swapButton;
	public Button startGameButton;
	public Button finishEarlyButton;
	public List<string> allWords;
	public List<string> sevenLetterWords;
	private int sevenLetterWordsIdx = 0;

	[HideInInspector] public AudioSource stressSnd;

	public GameObject letterPrefab;
	private int numLetters = 7;
	char[] currLetters;
	public List<GameObject> currLetterTiles = new List<GameObject>(); // game objects making up the letters for this level
	public List<Button> usedLetterTiles = new List<Button>(); // game objects making up the word being spelled
	private int[] chosenLettersIdx;

	bool useLetterSets = false;

	private List<string> usedWords; // words made during this level

	//GameObject[] platforms;
	public List<GameObject> platforms = new List<GameObject>();
	public int numPlatforms;

	void Awake()
	{
		LoadWordList ("ospd.txt");
		LoadSevenLetterWords ();

		currPlayerPrefab = PlayerPrefs.GetInt("RunnerIndex");
		if (currPlayerPrefab == null)
			currPlayerPrefab = 0;
	}

	//public GameObject letter;
	//public Sprite J;

	void Start () 
	{
		SetUpRunnerScene();

		if (PlayerPrefs.GetInt("MusicOn") == 0)
			SoundManager.StopMusic();
		else
			SwitchMusic ();

		adMgr = GameObject.Find("AdManager").GetComponent<AdManager>();
		gcMgr = GameObject.Find("GCManager").GetComponent<GameCenter>();
		canvas = GameObject.Find("Canvas");

		swapMgr = canvas.GetComponentInChildren<SwapTiles>();

		// start them where they left off last time
		sevenLetterWordsIdx = PlayerPrefs.GetInt ("Seven Letter Words Index");

		print ("WAX " + allWords.Contains("WAX"));

		currWord.text = "";

		//List<string> foundWords = findValidWords(allWords, "FZMHITE");

		//gameState = "playing";

		// turn off the buttons that we shouldn't see at the start
		goButton.gameObject.SetActive(false);
		delButton.gameObject.SetActive(false);
		swapButton.gameObject.SetActive(false);
		trashButton.gameObject.SetActive(false);
		runnerGoalPrefab.gameObject.SetActive(false);

		usedWords = new List<string>();

		highestLevelFinished = PlayerPrefs.GetInt("Highest Level Finished");

		//print ("All Time Total Jumps: " + PlayerPrefs.GetInt("Total Jumps"));

		LogAnalytics("Scene Play");
	}

	void Update()
	{
		if (gameState != "waiting")
		{
		CheckForOffscreenPlatforms();	// updates the +14 text to show how many platforms are coming
		CheckForGoalReached();			// if we reached goal, cleanup stuff
		CheckForDeath();
		CheckForStress();
		}

		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
		if (gameState == "playing" && Input.anyKeyDown)
		{
			for (int x = 0; x < currLetters.Length; x++)
			{
				string oneLetter = currLetters[x].ToString().ToLower();
				if (Input.GetKeyDown(oneLetter))
				{
					//print ("Pressed letter: " + oneLetter);
					LetterTyped(currLetters[x]);
					break;
				}
			}
			// check for enter key
			if (Input.GetKeyDown("return") && goButton.interactable)
				GoButtonClicked();

			// check for backspace key
			if (Input.GetKeyDown("backspace") && delButton.interactable)
				DelButtonClicked();

			// check for delete key
			if (Input.GetKeyDown("delete") && trashButton.interactable)
				DeleteWord();

			//check for escape key (pause)
			if (Input.GetKeyDown("escape") && Time.timeScale == 0)
			{
				gameObject.GetComponentInParent<PauseButton>().UnpauseGame();
			}
			else if (Input.GetKeyDown("escape") && Time.timeScale != 0)
			{
				gameObject.GetComponentInParent<PauseButton>().PauseGame();
			}

			// check for space key (Next Level)
			if (Input.GetKeyDown("space") && gameState == "waiting" || gameState == "starting")
				StartGame();

			// check for Finish Early key
			if (Input.GetKeyDown("end") && finishEarlyButton.gameObject.activeSelf)
				FinishEarly();
			
			// check for Swap Tokens key
			if (Input.GetKeyDown("tab") && swapsAvailable > 0)
			{
				swapMgr.UseSwapToken();
				SwapTiles();
			}
		}
		#endif
	}


	void LogAnalytics(string name, string evtName="", object evtData=null)
	{
		Analytics.CustomEvent(name, new Dictionary<string, object> {{ evtName, evtData }});
	}


	public void ChooseRunner(string runner)
	{
		if (runner == "idaho")
		{
			currPlayerPrefab = 0;
		}
		else if (runner == "crash")
		{
			currPlayerPrefab = 1;
		}

		SetUpRunnerScene();

		GameObject.Find("Canvas/StartGame").SetActive(false);
		GameObject.Find("Canvas/GenericMsg").GetComponent<Text>().text = "";

		PlayerPrefs.SetInt("RunnerIndex", currPlayerPrefab);
		PlayerPrefs.Save();

		//StartGame();
		Invoke("StartGame", 0.1f);
	}

	// set up background and such based on which runner we're using
	void SetUpRunnerScene()
	{
		string[] runners = {"idaho", "crash"};

		GameObject bgObj = GameObject.Find ("Background");
		SpriteRenderer spr = bgObj.GetComponent<SpriteRenderer>();
		Sprite newSprite = Resources.Load("Sprites/" + runners[currPlayerPrefab] + "-background", typeof(Sprite)) as Sprite;
		spr.sprite = newSprite;
		
		platformSprite = Resources.Load("Sprites/" + runners[currPlayerPrefab] + "-platform", typeof(Sprite)) as Sprite;
		
		if (currPlayerPrefab == 0)
		{
			bgObj.transform.position = new Vector3(0f, 3.28f, 0);
			platformYOffset = 0.29f;
		}
		else if (currPlayerPrefab == 1)
		{
			bgObj.transform.position = new Vector3(0f, 0.63f, 0);
			platformYOffset = 0.23f;
		}

		// set the lives left icon to match the current player
		GameObject.Find ("LivesIcon").GetComponent<SpriteRenderer>().sprite = livesIcon[currPlayerPrefab];

	}

	void CheckForStress() {

	}

	public void CleanUpBeforeLeaving()
	{
		PlayerPrefs.SetInt ("Seven Letter Words Index", sevenLetterWordsIdx);
		PlayerPrefs.Save ();

		// clear out all lists...

		DitchPlatforms();
		DitchLetterTiles();

	}

	void SetToWaiting()
	{
		gameState = "waiting";
		SwitchMusic ();
	}

	void CheckForDeath()
	{
		if (gameState == "felltodeath")
		{
			howLevelEnded = "felltodeath";
			currWord.text = "";
			DitchLetterTiles(true);
			goButton.interactable = false;
			//GameObject.Find ("Main Camera").GetComponent<AudioSource>().Stop();
			restartButton.gameObject.SetActive(true);
			DitchPlatforms();
			runnerGoalPrefab.gameObject.SetActive(false);
			delButton.interactable = false;
			trashButton.interactable = false;
			goButton.gameObject.SetActive(false);
			delButton.gameObject.SetActive(false);
			trashButton.gameObject.SetActive(false);
			HideFinishEarlyButton();	//edge case, but just in case it needs to be hidden
			//Invoke("SetToWaiting", 0.3f);

			adMgr.canShowAds = true;

			AddToRunningTotal();	// they get the points even if they let the runner plunge

			numLivesLeft--;
			livesLeft.text = numLivesLeft.ToString();
			if (numLivesLeft == 0) {
				// do some game over stuff here!
				gameState = "gameover";
				restartButton.GetComponentInChildren<Text> ().text = "New Game";
				SaveScore ();
				swapButton.gameObject.SetActive(false);
				socialPanel.gameObject.SetActive(true);
				// put in final text
				GameObject.Find("Canvas/Social Panel/LevelsCompleted").GetComponent<Text>().text = "Levels Completed: " + numLevelsPlayedThisGame;
				GameObject.Find("Canvas/Social Panel/TotalPoints").GetComponent<Text>().text = "Total Score: " + numPoints;
				highestScore.text = "Best Word: " + highestScoringWordThisGame + " for " + highestScoringWordPointThisGame + " points";
				#if UNITY_IOS	
				gcMgr.SetGCScore(highestScoringWordPointThisGame, "bestwords");
				#endif
				#if UNITY_STANDALONE_OSX
				//gcMgr.SetGCScore(highestScoringWordPointThisGame, "bestwordsmac");
				#endif
				//LogAnalytics("EndGame", "Highest Level`", numLevelsPlayedThisGame);

				string lettersStr = new string(currLetters);
				AnalyticsResult result = Analytics.CustomEvent("End Game", new Dictionary<string, object> {
					{ "Highest Level", numLevelsPlayedThisGame },
					{ "Final Letters", lettersStr },
					{ "Swaps Available", swapsAvailable }});

				print ("AnalyticsResult=" + result.ToString());
			} else {
				gameState = "waiting";
				restartButton.GetComponentInChildren<Text> ().text = "Try Again";
				levelNumText.text = "Try Again on Level " + levelNum;
			}

			numPlatformsCreatedThisGame += numPlatformsCreatedThisLevel;
			SwitchMusic ();
		}
	}

	void CheckForGoalReached()
	{
		if (gameState == "goalreached")
		{
			howLevelEnded = "goalreached";
			currWord.text = "";
			DitchLetterTiles(false);
			goButton.interactable = false;
			//GameObject.Find ("Main Camera").GetComponent<AudioSource>().Stop();
			restartButton.gameObject.SetActive(true);
			DitchPlatforms();
			runnerGoalPrefab.SetActive(false);
			delButton.interactable = false;
			trashButton.interactable = false;
			goButton.gameObject.SetActive(false);
			delButton.gameObject.SetActive(false);
			//swapButton.gameObject.SetActive(false);
			trashButton.gameObject.SetActive(false);
			gameState = "waiting";
			//Invoke("SetToWaiting", 0.3f);

			adMgr.canShowAds = true;

			restartButton.GetComponentInChildren<Text>().text = "Next Level";

			numPlatformsCreatedThisGame += numPlatformsCreatedThisLevel;

			levelNum++;
			levelNumText.text = "Get Ready for Level " + levelNum;

			SwitchMusic ();

			CheckForHighestLevel();

			AddToRunningTotal();
		}
	}

	void AddToRunningTotal()
	{
		// add newly earned points to the running total
		int allPoints = PlayerPrefs.GetInt("TotalScore") + numPoints;
		PlayerPrefs.SetInt("TotalScore", allPoints);
		PlayerPrefs.Save();
	}
	
	private void CheckForHighestLevel()
	{
		int savedHighLevel = PlayerPrefs.GetInt("Highest Level Finished");
		if (levelNum - 1 > savedHighLevel)
		{
			highestLevelFinished = levelNum - 1;
			PlayerPrefs.SetInt("Highest Level Finished", highestLevelFinished);
			PlayerPrefs.Save();
		}
	}

	public void StartGame()
	{
		int prevSwapsAvailable = swapsAvailable;

		//if (levelNum == 1 && highestLevelFinished == 0)
		if (levelNum == 1)
			gameObject.GetComponentInParent<DriftingText>().MakeDriftingText("Make words of 3-letters or more!", startGameButton.gameObject.transform.position);

		// first 3 levels make sure the letters given are "known good"
		// until the player has played at least three levels, then always random
		if (levelNum < 4 && highestLevelFinished < 4)
			useLetterSets = true;
		GetRandomLetters();
		useLetterSets = false;

		socialPanel.gameObject.SetActive(false);

		numPlatformsOffScreen = 0;
		numPlatformsCreatedThisLevel = 0;
		numTimesJumpedThisLevel = 0;

		levelNumText.text = "";

		playerRelaxed = false;
		playerStressed = false;

		//--------------------------------------------------------
		// starting a brand new game here, not just another level
		// if the actual game is over, reset other things
		if (gameState == "gameover" || gameState == "starting") 
		{
			highestScoringWordThisGame = "";
			highestScoringWordPointThisGame = 0;

			AddToPoints (-numPoints);
			numLivesLeft = 3;
			numLevelsPlayedThisGame = 0;
			levelNum = 1;

			swapsAvailable = 3;
		} else
			numLevelsPlayedThisGame++;


		// every five levels give the player one swap token
		print ("levelNum % 5 " + levelNum % 5);
		if (levelNum % 5 == 0)
			swapsAvailable++;
		if (swapsAvailable > 3)
			swapsAvailable = 3;

		//print("-=> " + (numLevelsPlayedThisGame == 0) + (gameState == "gameover") + (gameState == "starting") );
		//print("-=> " + (numLevelsPlayedThisGame) + (gameState) );

		if (numLevelsPlayedThisGame == 0 || gameState == "gameover" || gameState == "starting" || watchedAd || swapsAvailable > prevSwapsAvailable)
		{
			RefreshSwapTokens();
			swapButton.GetComponent<Button> ().interactable = true;
		}

		// if they got more swaps, have a party
		if (swapsAvailable > prevSwapsAvailable)
		{
			print ("*** We got more swaps!");
		}

		usedWords.Clear ();	//empty out the last words created
		usedWords = new List<string>();	// shouldn't have to, but seeing if it fixes bug...

		// change speed of platforms -- higher the level, faster the speed
		if (numLevelsPlayedThisGame < 5)
			platformSpeed = -0.4f;
		else if (numLevelsPlayedThisGame >= 5 && numLevelsPlayedThisGame < 10 )
			platformSpeed = -0.55f;
		else
			platformSpeed = -0.8f;

//		if (numLevelsPlayedThisGame < 5)
//			platformSpeed = -0.4f;
//		else if (numLevelsPlayedThisGame >= 5)
//			platformSpeed = -0.55f;


		livesLeft.text = numLivesLeft.ToString();
		newHighScore.text = "";

		gameState = "playing";
		startGameButton.gameObject.SetActive(false);
		MakeLetterButtons(numLetters);
		MakeSomePlatforms(startingNumOfPlatforms, 0);
		MakePlayer();
		//GameObject.Find ("Main Camera").GetComponent<AudioSource>().Play();
		//restartButton.gameObject.SetActive(false);

		runnerGoalPrefab.gameObject.SetActive(true);
		goButton.gameObject.SetActive(true);
		delButton.gameObject.SetActive(true);
		swapButton.gameObject.SetActive(true);
		print("swapsAvailable " + swapsAvailable);
		swapButton.interactable = swapsAvailable > 0;
		trashButton.gameObject.SetActive(true);
		restartButton.gameObject.SetActive(false);

		howLevelEnded = "";

		print ("numPlatformsOffScreen: " + numPlatformsOffScreen);
		SwitchMusic ();
	}

	void SwitchMusic(string songName = "")
	{
		if (PlayerPrefs.GetInt("MusicOn") == 0)
			return;

		if (gameState == "playing")
		{
			SoundManager.PlayConnection("Play");
		}
		else //if (gameState == "waiting" || gameState == "gameover" || gameState == "paused")
		{
			SoundManager.PlayConnection("Main");
		}
	}

	public void RefreshSwapTokens() {
		// if they watched an ad, fill the swap tokens
		if (watchedAd)
			swapsAvailable = 3;
		print ("Put In Swap Tokens " + swapsAvailable);
		swapButton.GetComponent<SwapTiles> ().ShowSwapTokens (swapsAvailable);
		watchedAd = false;
	}
	
	void SaveScore()
	{
		int savedScore = PlayerPrefs.GetInt ("High Score");
		if (numPoints > savedScore)
		{
			PlayerPrefs.SetInt ("High Score", numPoints);
			PlayerPrefs.Save ();
			newHighScore.text = "New High Score! " + numPoints;
			SaveToGameCenter(numPoints);
		}
		else
		{
			//newHighScore.text = "Points needed for new High Score: " + (savedScore - numPoints);
		}
		#if UNITY_IOS	
		gcMgr.SetGCScore(numPoints, "topscores");
		#endif
		#if UNITY_STANDALONE_OSX
		//gcMgr.SetGCScore(numPoints, "topscoresmac");
		#endif
	}

	void SaveToGameCenter(int theScore)
	{
//		GameCenterManager.OnScoreSubmitted += OnScoreSubmitted;
//		GameCenterManager.ReportScore((long)theScore, "topscore");
	}

//	private void OnScoreSubmitted (ISN_Result result) {
//		GameCenterManager.OnScoreSubmitted -= OnScoreSubmitted;
//		if(result.IsSucceeded)  {
//			Debug.Log("Score Submitted");
//		} else {
//			Debug.Log("Score Submit Failed");
//		}
//	}


	public void SwapTiles()
	{
		if (gameState != "playing")
			return;

		currWord.text = "";
		GetRandomLetters();
		MakeSomePlatforms(numPlatformsWithSwap, 0);
		DitchLetterTiles(false);
		MakeLetterButtons(numLetters);
		delButton.interactable = false;
		trashButton.interactable = false;
		goButton.interactable = false;
		usedLetterTiles.Clear();
		usedLetterTiles = new List<Button>();
		usedWords = new List<string>();
	}

	void MakePlayer()
	{
		runner = Instantiate (playerPrefab[currPlayerPrefab], new Vector3(-5.9f, 2.89f, 0f), Quaternion.identity) as GameObject;
	}

	//-------------------------------------------------
	// pass true to show explosions when they vanish
	//-------------------------------------------------
	void DitchLetterTiles(bool explode = false)
	{
		for (int x = currLetterTiles.Count-1; x > -1 ; x--)
		{
			GameObject tile = currLetterTiles[x];
			if (explode)
			{
				GameObject instance = CFX_SpawnSystem.GetNextObject(tileExplodePrefab);
				instance.transform.position = tile.transform.position;
				//Instantiate(tileExplodePrefab, tile.transform.position, Quaternion.identity);
			}
			Destroy(tile);
			currLetterTiles.RemoveAt(currLetterTiles.Count-1);
		}

	}

//	void DitchUsedLetterTiles()
//	{
//		for (int x = usedLetterTiles.Count-1; x > -1 ; x--)
//		{
//			Button tile = usedLetterTiles[x];
//			Destroy(tile.gameObject);
//			usedLetterTiles.RemoveAt(usedLetterTiles.Count-1);
//		}
//		
//	}

	void EnableLetterTiles()
	{
		for (int x = 0; x < currLetterTiles.Count; x++)
			EnableOneLetterTile(x);
	}

	void EnableOneLetterTile(int idx)
	{
		currLetterTiles[idx].GetComponent<Button>().interactable = true;
	}

	public void HideLetterTiles()
	{
		for (int x = 0; x < currLetterTiles.Count; x++)
			currLetterTiles[x].gameObject.SetActive(false);
	}

	public void ShowLetterTiles()
	{
		for (int x = 0; x < currLetterTiles.Count; x++)
			currLetterTiles[x].gameObject.SetActive(true);
	}

	//-----------------------------------------------
	// if there are platforms offscreen to the right,
	// show how many there are.
	//-----------------------------------------------

	void CheckForOffscreenPlatforms()
	{
		if (numPlatformsOffScreen > 0 && gameState == "playing")
		{
			platformsOffscreen.text = "+" + numPlatformsOffScreen;
		}
		else
			platformsOffscreen.text = "";
	}

	void MakeSomePlatforms(int howMany, int ptValue = 1)
	{
		int pointValue = ptValue;
		if (ptValue > 0)
			pointValue = 1 << (howMany-1);
		//print ("howMany & pointValue: " + (howMany) + "  " + pointValue);

		// if we're stressed, give some breathing room
		if (playerStressed && !playerRelaxed)
		{
			ChangeTileColors(Color.white);
			// stop stress sound if playing
			//SoundManager.PlaySFX(SoundManager.Load("Stress"));
			SoundManager.StopSFXObject(stressSnd);
		}

		for (int x = 0; x < howMany; x++)
			AddNewPlatform(ptValue);
	}

	void AddNewPlatform(int ptValue)
	{
		//Debug.Log ("AddNewPlatform platforms.Count: " + platforms.Count);

		float startX;

		if (platforms.Count == 0)
		{
			startX = platformsXStart;
			//Debug.Log ("AddNewPlatform at left side: " + startX);
		}
		else
		{
			GameObject thePlatform = platforms[platforms.Count-1];
			if (thePlatform == null)
				startX = platformsXStart;
			else
				startX = thePlatform.transform.position.x + platformSpacing;
			//Debug.Log ("AddNewPlatform at calculation: " + startX);
		}

		GameObject aPlatform = Instantiate (platformObj) as GameObject;
		aPlatform.layer = LayerMask.NameToLayer("Platform");
		aPlatform.transform.position = new Vector3(startX, platformsYStart, 0);
		//print ("AddNewPlatform() X/Y " + aPlatform.transform.position.x + "  " + aPlatform.transform.position.y);

		// sprite image based on chosen runner
		aPlatform.GetComponent<SpriteRenderer>().sprite = platformSprite;
		aPlatform.GetComponent<EdgeCollider2D>().offset = new Vector2(0, platformYOffset);

		aPlatform.GetComponent<Platform> ().pointValue = ptValue;
		platforms.Add(aPlatform);
		numPlatformsCreatedThisLevel++;
		// if we can reach the goal, let the player know
		if (numPlatformsCreatedThisLevel == 13)
			PlayerCanRelax();
	}

	void PlayerCanRelax()
	{
		playerRelaxed = true;
		playerStressed = false;

		GameObject instance = CFX_SpawnSystem.GetNextObject(enoughPlatformsPrefab);
		instance.transform.position = runnerGoalPrefab.transform.position;
		SoundManager.PlaySFX(SoundManager.Load("Relax"));

		ShowFinishEarlyButton();

		ChangeTileColors(Color.green);
	}

	// we're not changing the actual tile, but the lettertileglow sprite under it
	public void ChangeTileColors(Color c) 
	{
		//print ("ChangeTileColors " + c.ToString());

		for (int x = 0; x < currLetterTiles.Count; x++)
		{
			foreach (var child in currLetterTiles[x].GetComponentsInChildren<SpriteRenderer>(true))
			{
				//print ("SpriteRendererRenderer in " + x);
				if (child.gameObject.name == "LetterTileGlow")
				{
					if (c == Color.white)
					{
						child.gameObject.SetActive(false);
					}
					else
					{
						child.gameObject.SetActive(true);
						child.color = c;
						//child.material.color = c;
						//glowSprite.GetComponent<SpriteRenderer>().color = c;
					}
				}
			}
				
			//Sprite glowSprite = currLetterTiles[x].GetComponentInChildren<Sprite>() as Sprite;
			//SpriteRenderer spRend = glowSprite.
		}
	}

	void DitchPlatforms()
	{
		if (platforms.Count == 0)
			return;

		for (int x = platforms.Count-1; x > -1; x--)
			platforms.RemoveAt(x);
	}

	//--------------------------------------------------------------------
	// make the Scrabble-like letter tiles at the bottom of the screen.
	//--------------------------------------------------------------------

	void MakeLetterButtons(int numTiles)
	{
		float tileSpace = 1.5f;
		float totalSpace = numTiles * tileSpace;
		float startLeft = -totalSpace/2 + 0.2f;

		for (int x = 0; x < numTiles; x++)
		{
			// test making a new letter
			float xPos = startLeft + (tileSpace * (x-1)) + (tileSpace/2);

			GameObject letterTile = (GameObject) Instantiate(letterPrefab);
			//GameObject parent = GameObject.Find("Canvas");
			letterTile.transform.SetParent(canvas.transform, false);
			letterTile.transform.position = new Vector2(xPos, -6.5f);
			letterTile.transform.DOMoveY(-3.5f, 0.5f);

			//char newLetter = (char)('A' + Random.Range (0,26));
			char newLetter = currLetters[x];
			Sprite newSprite = Resources.Load("Letters/letter_" + newLetter, typeof(Sprite)) as Sprite;

			// set the point value of the letter
			int[] letterPts = {1,3,3,2,1,4,2,4,1,8,5,1,3,1,1,3,10,1,1,1,1,4,4,8,4,10};
			int idx = (int)newLetter - 65;
			//print ("Letter, idx: " + newLetter + ", " +  idx);
			letterTile.GetComponent<Letter>().myPointValue = letterPts[idx];
			letterTile.GetComponent<Letter>().theLetter = newLetter;
			//print ("Letter Pt: " + letterPts[idx]);

			Image images = letterTile.GetComponent<Image>();
			images.sprite = newSprite;

			Button btn = letterTile.GetComponent<Button>();
			btn.onClick.AddListener(delegate {
				ButtonTapped(btn, newLetter); });

			// keep track of all the letters available for this level.
			currLetterTiles.Add(letterTile);
		}

		// if we were already relaxed, we're still relaxed
		if (playerRelaxed)
			ChangeTileColors(Color.green);

	}

	void GetRandomLetters()
	{
		currLetters = new char[numLetters];

		if (useLetterSets)
		{
			currLetters = sevenLetterWords.ElementAt( sevenLetterWordsIdx++ ).ToCharArray ();

			// if we reached the end, roll over and start at the beginning again
			if (sevenLetterWordsIdx > sevenLetterWords.Count - 1)
				sevenLetterWordsIdx = 0;
		}
		else
		{
			bool grabbedQ = false;
			bool grabbedU = false;

			string consonants = "ABCDEFGHIJKLMNOPQRSTUVWXYZBCDFGHLMNPRSTUVWYETAOINSHRDL";
			string vowels = "AEIOU";
			// get two vowels
			currLetters[0] = vowels[Random.Range (0,5)];
			currLetters[1] = vowels[Random.Range (0,5)];
			// get the rest consonants
			for (int i = 2; i != numLetters; i++)
			{
				currLetters[i] = consonants[Random.Range (0,consonants.Length)];

				if (grabbedQ && currLetters[i] == 'Q')
					currLetters[i] = 'R';
				else if (currLetters[i] == 'Q')
					grabbedQ = true;

				if (currLetters[i] == 'U')
					grabbedU = true;
			}

			// make sure we don't have a Q without a U
			//if (grabbedQ && !grabbedU)

		}
		// now shuffle the chars so vowels are mixed in with consonants
		//print ("currLetters.Length " + currLetters.Length);
		ShuffleArray(currLetters);
		//print ("currLetters.Length " + currLetters.Length);

		//print (new string(currLetters));
	}

	// if actual letter tile is pressed, do this...
	void ButtonTapped(Button me, char theLetter)
	{
		usedLetterTiles.Add(me);
		//Debug.Log(theLetter);
		AddToWord(theLetter);
		CheckForWord();
	}
	// but if we typed a letter, do this instead...
	void LetterTyped(char theLetter)
	{
		// find the matching letter button
		GameObject matchingTile = null;
		Letter letterScript;
		for (int x = 0; x < currLetterTiles.Count; x++)
		{
			letterScript = currLetterTiles[x].GetComponent<Letter>(); 
			if ( !letterScript.beingUsed && letterScript.theLetter == theLetter )
			{
				matchingTile = currLetterTiles[x];
				letterScript.beingUsed = true;
				break;
			}
		}

		if (matchingTile != null)
		{
			matchingTile.GetComponent<Button>().interactable = false;
			usedLetterTiles.Add(matchingTile.GetComponent<Button>());
			AddToWord(theLetter);
			CheckForWord();
		}
	}

	void CheckForWord()
	{
		string theWord = currWord.text.Trim();
//		if (allWords.Contains(theWord)  && !usedWords.Contains(theWord))
//			print (theWord + "  " + allWords.Contains(theWord) + "  " + !usedWords.Contains(theWord));
		goButton.interactable = allWords.Contains(theWord) && !usedWords.Contains(theWord);
	}

	public void GoButtonClicked () 
	{
		int wordLength = currWord.text.Length;
		int points = WordlengthToPoints(wordLength);
		int letterPoints = LetterPoints();
		string theWord = currWord.text;

		print(currWord.text);
		// see if we got a higher score/longer word (or matched the last one)
		if (letterPoints >= highestScoringWordPointThisGame)
		{
			highestScoringWordThisGame = theWord;
			highestScoringWordPointThisGame = letterPoints;
		}
		usedWords.Add (theWord);
		currWord.text = "";
		// create new platforms based on length of new word
		MakeSomePlatforms(wordLength-2, points);

		// now change points from num platforms-based, to letter tiles 
		//points = LetterPoints();
		AddToPoints (letterPoints);
		goButton.GetComponent<DriftingText>().MakeDriftingText(letterPoints + " Pts", goButton.transform.position);
		EnableLetterTiles();
		// ditch the used letter tiles
		//for (int x = 0; x < usedLetterTiles.Count; x++)
		//	Destroy(usedLetterTiles[x]);
		//print ("Calling ResetBeingUsed from GoButton");
		ResetBeingUsed();
		usedLetterTiles.Clear();
		usedLetterTiles = new List<Button>();
		delButton.interactable = currWord.text != "";
		trashButton.interactable = currWord.text != "";
		goButton.interactable = false;
	}

	//----------------------------------------------------------
	// add up the points from the current word we're submitting
	int LetterPoints()
	{
		print ("In LetterPoints()");

		int points = 0;
		for (int x = 0; x < usedLetterTiles.Count; x++)
		{
			if (usedLetterTiles[x] != null)
				points += usedLetterTiles[x].GetComponent<Letter>().myPointValue;
		}
		return (points);
	}

	public void DelButtonClicked()
	{
		string theWord = currWord.text;

		if (theWord != "")
		{
			currWord.text = theWord.Remove(theWord.Length-1);

			// now make the correct button interactable again
			usedLetterTiles[usedLetterTiles.Count-1].interactable = true;
			usedLetterTiles[usedLetterTiles.Count-1].GetComponent<Letter>().beingUsed = false;
			usedLetterTiles.RemoveAt(usedLetterTiles.Count-1);

			// if we now have a word, allow it to be submitted
			//goButton.interactable = allWords.Contains(currWord.text) && !usedWords.Contains(theWord);
			CheckForWord();
		}

		delButton.interactable = currWord.text != "";
		trashButton.interactable = currWord.text != "";
	}

	public void DeleteWord()
	{
		currWord.text = "";
		EnableLetterTiles();
		print ("Calling ResetBeingUsed from DeleteWord");
		ResetBeingUsed();
		usedLetterTiles.Clear();
		usedLetterTiles = new List<Button>();
		delButton.interactable = false;
		trashButton.interactable = false;
		goButton.interactable = false;
	}

	// reset the beingUsed flag in each letter tile
	void ResetBeingUsed()
	{
		print ("usedLetterTiles.Count=" + usedLetterTiles.Count);
		for (int x = 0; x < usedLetterTiles.Count; x++)
		{
			if (usedLetterTiles[x] == null)
				print ("ResetBeingUsed null = " + x);

			if (usedLetterTiles[x] != null)
				usedLetterTiles[x].GetComponent<Letter>().beingUsed = false;
		}
	}

	public void AddToWord(char newChar)
	{
		currWord.text = currWord.text + newChar;
		delButton.interactable = true;
		trashButton.interactable = true;
	}

	public void AddToPoints(int newPoints)
	{
		numPoints += newPoints;
		points.text = numPoints + " Points";

	}

	//---------------------------------------------------------------
	// pass in wordlength and get back how many points for that word.
	// 3-letter word = 1
	// 4-letter word = 2
	// 5-letter word = 4
	// 6-letter word = 8
	// 7-letter word = 16

	int WordlengthToPoints(int num)
	{
		int pointValue = num-2;
		if (num > 3)
			pointValue = 1 << (num-3);
	
		return (pointValue);
	}

	public void ShowFinishEarlyButton()
	{
		finishEarlyButton.gameObject.SetActive(true);
	}

	public void HideFinishEarlyButton()
	{
		finishEarlyButton.gameObject.SetActive(false);
	}

	public void FinishEarly()
	{
		runner.transform.position = runnerGoalPrefab.transform.position;
	}

	//=============================================================
	// utility functions below here
	//=============================================================

//	List<String> findValidWords(List<String> dict, char[] letters)
//	{
//		int []avail = new int[26];
//		for(char c : letters){
//			int index = c - 'a';
//			avail[index]++;
//		}
//		List<String> result = new ArrayList();
//		for(String word: dict){
//			int count[] = new int[26];
//			boolean ok = true;
//			for(char c : word.toCharArray()){
//				int index = c - 'a';
//				count[index]++;
//				if(count[index] > avail[index]){
//					ok = false;
//					break;
//				}
//			}
//			if(ok){
//				result.Add(word);
//			}
//		}
//		return result;
//	}

	public static void ShuffleArray<T>(T[] arr) {
		for (int i = arr.Length - 1; i > 0; i--) {
			int r = Random.Range(0, i + 1);
			T tmp = arr[i];
			arr[i] = arr[r];
			arr[r] = tmp;
		}
	}

	void LoadSevenLetterWords()
	{
		sevenLetterWords = new List<string>();

		TextAsset ta = Resources.Load ("sevenletterwords", typeof(TextAsset)) as TextAsset;
		string[] lines = ta.text.Split('\n');

		foreach (string oneLine in lines)
		{
			sevenLetterWords.Add(oneLine.ToUpper());
			//print (oneLine.ToUpper ());
		}

	}

	void LoadWordList(string filename, int minLength=3)
	{

		allWords = new List<string>();

		//print (filename);
		//TextAsset ta = Resources.Load<TextAsset>(filename);
		TextAsset ta = Resources.Load ("wordjumperlist", typeof(TextAsset)) as TextAsset;
		//string[] lines = ta.text.Split (new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
		//print (ta);
		string[] lines = ta.text.Split('\n');

		foreach (string oneLine in lines)
		{
			// donly grab words with a minimum length
			if (oneLine.Length >= minLength)
			{
				allWords.Add(oneLine.ToUpper());
			}
		}

//		using (StreamReader r = new StreamReader(filename))
//		{
//			string oneLine = "";
//			while ((oneLine = r.ReadLine()) != null)
//			{
//				// don't grab 1- or 2-letter words
//				if (oneLine.Length > minLength)
//				{
//					allWords.Add(oneLine.ToUpper());
//				}
//			}
//		}
	}

}
