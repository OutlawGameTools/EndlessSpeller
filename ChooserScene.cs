﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class ChooserScene : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
		if (PlayerPrefs.GetInt("MusicOn") == 0)
			SoundManager.StopMusic();
		
		LogAnalytics("Scene Chooser");
	}
	
	void LogAnalytics(string name, string evtName="", object evtData=null)
	{
		Analytics.CustomEvent(name, new Dictionary<string, object> {{ evtName, evtData }});
	}
	
}
