using UnityEngine;
using System.Collections;

public class LevelMenu2DInit : MonoBehaviour {

	public SceneManager sceneMgr;
	
	void Awake()
	{
	}
	
	void Start()
	{
		sceneMgr = GameObject.Find ("SceneManager").GetComponent<SceneManager>();
		int idx = PlayerPrefs.GetInt("RunnerIndex");
		print ("idx: " + idx);
		LevelMenu2D.I.initialItemNumber = idx;
	}
	
}
