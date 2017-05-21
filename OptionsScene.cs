using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class OptionsScene : MonoBehaviour {

	public Button musicOn;
	public Button musicOff;
	public Button rateButton;

	// Use this for initialization
	void Start () {

		if (PlayerPrefs.GetInt("MusicOn") == 0)
		{
			SoundManager.StopMusic();
			musicOn.gameObject.SetActive(false);
			musicOff.gameObject.SetActive(true);
		}
		else
		{
			musicOn.gameObject.SetActive(true);
			musicOff.gameObject.SetActive(false);
		}

		LogAnalytics("Scene Options");

		#if UNITY_ANDROID || UNITY_WEBPLAYER || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
			rateButton.gameObject.SetActive(false);
		#endif

	}
	
	// 
	public void ToggleMusic()
	{
		int musicOn = PlayerPrefs.GetInt("MusicOn");
		if (musicOn == 1)// turn music off
		{
			SoundManager.StopMusic();
			musicOn = 0;
		}
		else
		{
			SoundManager.PlayConnection(Application.loadedLevelName);
			musicOn = 1;
		}
		
		PlayerPrefs.SetInt("MusicOn", musicOn);
		PlayerPrefs.Save();
	}

	void LogAnalytics(string name, string evtName="", object evtData=null)
	{
		Analytics.CustomEvent(name, new Dictionary<string, object> {{ evtName, evtData }});
	}


}
