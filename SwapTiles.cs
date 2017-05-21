using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SwapTiles : MonoBehaviour {

	public GameObject poof;
	public GameObject swapIndicator;
	private List<GameObject> swapTokens = new List<GameObject>();
	GameObject parent;

	bool stillPressing = false;

	SceneManager sceneMgr;

	// Use this for initialization

	void Awake()
	{
		sceneMgr = GameObject.Find ("SceneManager").GetComponent<SceneManager>();
	}

	void Start () {
	
		//parent = GameObject.Find ("UIPlaying").GetComponentInChildren<SwapTiles> ();
		parent = GameObject.Find ("/Canvas/UIPlaying/SwapTiles");
	}
	
	public void ResetSwapTokens()
	{
		for (int i = swapTokens.Count-1; i > -1; i--)
			if (swapTokens[i] != null)
				Destroy (swapTokens[i]);

		swapTokens.Clear ();
	}

	public void ShowSwapTokens(int howMany)
	{
		ResetSwapTokens();
		print ("show swap tokens");
		float xStart = parent.transform.position.x - 0.969f + 0.25f;
		float between = 0.73f;

		for (int x = 0; x < howMany; x++)
		{
			GameObject swapToken = Instantiate (swapIndicator) as GameObject;
			swapToken.transform.SetParent(parent.transform, false);
			swapToken.transform.position = new Vector2(xStart+(x*between), parent.transform.position.y+0.09f); // 4.458f
			swapToken.transform.localScale = new Vector3(100, 100, 0);
			swapTokens.Add (swapToken);

			ThrowConfetti(x, swapToken.transform.position);
		}
	}

	void ThrowConfetti(int idx, Vector3 pos)
	{
		GameObject instance = CFX_SpawnSystem.GetNextObject(poof);
		instance.transform.position = pos;

	}

	public void UseSwapToken()
	{
		// don't allow frantically pushing swap if it's just too late
		if (stillPressing || sceneMgr.gameState != "playing" || swapTokens.Count < 1)
			return;
		stillPressing = true;
		sceneMgr.swapsAvailable--;
		Destroy (swapTokens[swapTokens.Count - 1]);
		swapTokens.RemoveAt (swapTokens.Count - 1);
		if (swapTokens.Count == 0)
			gameObject.GetComponent<Button>().interactable = false;
		stillPressing = false;
	}

}
