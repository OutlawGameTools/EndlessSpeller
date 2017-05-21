using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour {

	public GameObject pauseWindow;
	public GameObject pauseUI;
	public Text currWord;
	public CanvasGroup playingUI;
	public CanvasGroup uiWaiting;
	public Button pause;
	public Button home;
	public Button restart;

	private SceneManager sceneMgr;

	private bool wasWaiting = false;

	//static string prevState = "";
	
	// Use this for initialization
	void Start () {
		sceneMgr = GameObject.Find("SceneManager").GetComponent<SceneManager>();
	}
	

	public void ShowPauseWindow()
	{
		pauseWindow.SetActive(true);
		pauseUI.SetActive(true);
	}

	public void HidePauseWindow()
	{
		pauseWindow.SetActive(false);
		pauseUI.SetActive(false);
	}

	public void PauseGame()
	{
		ShowPauseWindow();
		playingUI.interactable = false;
		uiWaiting.gameObject.SetActive(false);
		home.interactable = false;
		pause.interactable = false;
		Time.timeScale = 0;
		currWord.gameObject.SetActive(false);
//		if (restart.gameObject.activeInHierarchy)
//		{
//			wasWaiting = true;
//			restart.gameObject.SetActive(false);
//		}
		sceneMgr.HideLetterTiles();
	}

	public void UnpauseGame()
	{
		// start game again
		HidePauseWindow();
		playingUI.interactable = true;
		uiWaiting.gameObject.SetActive(true);
		home.interactable = true;
		pause.interactable = true;
		Time.timeScale = 1;
		currWord.gameObject.SetActive(true);
//		if (wasWaiting)
//		{
//			restart.gameObject.SetActive(true);
//			wasWaiting = false;
//		}
		sceneMgr.ShowLetterTiles();
	}
	
}
