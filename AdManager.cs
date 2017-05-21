using System;
using UnityEngine;
using UnityEngine.UI;

public class AdManager : MonoBehaviour {

	SceneManager sceneMgr;
	public bool canShowAds = true;

	public Button showAdsButton;

	// UnityAds IDs
	string iosGameID = "nnnn";
	string androidGameID = "nnnn";

	#if UNITY_IOS
		string gameAdID = "nnnn";
	#elif UNITY_ANDROID
		string gameAdID = "nnnn";
	#else
		string gameAdID = "0";
	#endif

	void Awake() {

		#if !UNITY_STANDALONE
		if (Advertisement.isSupported) {
			Advertisement.Initialize (gameAdID);
		} else {
			Debug.Log("Platform not supported");
			//canShowAds = false;
		}
		#endif

		sceneMgr = GameObject.Find ("SceneManager").GetComponent<SceneManager>();
	}

	void Update() 
	{
		// Advertisement.IsReady() &&

		#if !UNITY_STANDALONE
		if (canShowAds && Advertisement.IsReady() && sceneMgr.gameState == "waiting" && sceneMgr.swapsAvailable < 3)  // took out || sceneMgr.gameState == "gameover"
		{
			showAdsButton.gameObject.SetActive(true);
			//showAdsButton.GetComponentInChildren<Text>().text = "Watch Ad for Swap Tokens!";
			showAdsButton.interactable = canShowAds;
		}
		else {
			// fix this so not setting it on Update
			showAdsButton.gameObject.SetActive(false);
		}
		#endif

		#if UNITY_STANDALONE
		showAdsButton.gameObject.SetActive(false);
		#endif
	}

	public void ShowAd() {

		#if !UNITY_STANDALONE
		Advertisement.Show(null, new ShowOptions {
			resultCallback = result => {
				Debug.Log("Ad view: " + result.ToString());
				//sceneMgr.swapsAvailable = 3;
				if (result.ToString() == "Finished") {
					sceneMgr.watchedAd = true;
					sceneMgr.swapsAvailable = 3;
					//showAdsButton.GetComponentInChildren<Text>().text = "You Have 3 Swap Tokens!";
					canShowAds = false;
					sceneMgr.RefreshSwapTokens();
					showAdsButton.interactable = false;
				}
				else
				{
					// switch this to false for release version
					sceneMgr.watchedAd = false;
//					// and ditch the rest of this stuff
//					sceneMgr.watchedAd = true;
//					sceneMgr.swapsAvailable = 3;
//					//showAdsButton.GetComponentInChildren<Text>().text = "You Have 3 Swap Tokens!";
//					canShowAds = false;
//					sceneMgr.RefreshSwapTokens();
//					showAdsButton.interactable = false;
				}

			}
		});
		#endif
	}
	
//	void OnGUI() {
//		if (sceneMgr.gameState == "waiting" || sceneMgr.gameState == "gameover" && sceneMgr.howLevelEnded != "")
//		{
//			if(GUI.Button(new Rect(10, 200, 150, 50), Advertisement.IsReady() ? "Fill Swaps" : "Waiting...")) 
//			{
//				// Show with default zone, pause engine and print result to debug log
//				Advertisement.Show(null, new ShowOptions {
//					resultCallback = result => {
//						Debug.Log(result.ToString());
//						//sceneMgr.swapsAvailable = 3;
//						sceneMgr.watchedAd = true;
//					}
//				});
//			}
//		}
//	}

}
