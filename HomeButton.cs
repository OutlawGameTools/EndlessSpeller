using UnityEngine;
//using MadLevelManager;

public class HomeButton : MonoBehaviour {

	private SceneManager sceneMgr;
	
	// Use this for initialization
	void Start () {
		sceneMgr = GameObject.Find("SceneManager").GetComponent<SceneManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void GoBack()
	{
		Application.LoadLevel ("Main");
	}

	public void GoToMain()
	{
		// if in the middle of a game, ask if they're sure
		string state = sceneMgr.gameState;

		//if (state == "waiting" || state == "gameover" || state == "starting") 
		//{
			sceneMgr.CleanUpBeforeLeaving ();
			Application.LoadLevel ("Main");
			//MadLevel.LoadLevelByName("Main");
		//}
		//else
		//{
			//Time.timeScale = 0;
		//}

	}
}
