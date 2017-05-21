using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Analytics;
//using MadLevelManager;

namespace Soomla.Store {

public class MainSceneManager : MonoBehaviour {

	private static bool gcIsInitialized = false;
	private static bool soomlaStoreInitialized = false;
		public Button scoreButton;


	// Use this for initialization
	void Start () {
	
		if (!soomlaStoreInitialized)
		{
			//SoomlaStore.Initialize(new WordJumperAssets());
		}

//		if (!gcIsInitialized)
//		{
//			GameCenterManager.OnAuthFinished += OnAuthFinished;
//			GameCenterManager.Init();
//			gcIsInitialized = true;
//		}

		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		Screen.orientation = ScreenOrientation.AutoRotation;

		if (PlayerPrefs.GetInt("MusicOn") == 0)
			SoundManager.StopMusic();

			GameObject.Find("GCScores").GetComponent<Button>().interactable = GameCenterManager.IsPlayerAuthenticated;

			LogAnalytics("Scene Main");

			#if UNITY_ANDROID || UNITY_WEBPLAYER || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
				scoreButton.gameObject.SetActive(false);
			#endif

		// if this is first time, start with music on
		if (!PlayerPrefs.HasKey("MusicOn"))
			{
				PlayerPrefs.SetInt("MusicOn", 1);
				PlayerPrefs.Save();
			}
	}

	void LogAnalytics(string name, string evtName="", object evtData=null)
		{
			Analytics.CustomEvent(name, new Dictionary<string, object> {{ evtName, evtData }});
		}

//	void OnAuthFinished (ISN_Result res) {
//		if (res.IsSucceeded) {
//			IOSNativePopUpManager.showMessage("Player Authored ", "ID: " + GameCenterManager.Player.Id + "\n" + "Alias: " + GameCenterManager.Player.Alias);
//		} else {
//			IOSNativePopUpManager.showMessage("Game Center ", "Player auth failed");
//		}
//	}

	// Update is called once per frame
	void Update () {
	
	}

	public void GoToOptions()
	{

	}

	public void GoToAbout()
	{
		
	}
	
	public void GoToPlay()
	{
		Application.LoadLevel("Play");
		//MadLevel.LoadLevelByName("Chooser");
	}
	
}

}

