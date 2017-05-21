using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameCenter : MonoBehaviour {

	public Button scoreButton;

	private string leaderBoardId =  "topscores";

	private static bool gcIsInitialized = false;
	private static bool gcIsAuthorized = false;

	void Awake() {
		if(!gcIsInitialized) {
			
			//Achievement registration. If you skip this step GameCenterManager.achievements array will contain only achievements with reported progress 
			GameCenterManager.RegisterAchievement ("wjfirstswap");
			//GameCenterManager.RegisterAchievement (TEST_ACHIEVEMENT_2_ID);

			//Listen for the Game Center events
//			GameCenterManager.OnAchievementsProgress += HandleOnAchievementsProgress;
//			GameCenterManager.OnAchievementsReset += HandleOnAchievementsReset;
//			GameCenterManager.OnPlayerScoreLoaded += HandleOnPlayerScoreLoaded;
//			GameCenterManager.OnPlayerScoreLoaded += OnPlayerScoreLoaded;
			GameCenterManager.OnAuthFinished += OnAuthFinished;
//			GameCenterManager.OnAchievementsLoaded += OnAchievementsLoaded;
			
			//Initializing Game Center class. This action will trigger authentication flow
			GameCenterManager.Init();
			gcIsInitialized = true;
		}
	}


	void OnAuthFinished (ISN_Result res) {
		if (res.IsSucceeded) {
			gcIsAuthorized = true;
			//IOSNativePopUpManager.showMessage("Player Authorizeded ", "ID: " + GameCenterManager.Player.Id + "\n" + "Alias: " + GameCenterManager.Player.Alias);
		} else {
			//IOSNativePopUpManager.showMessage("Game Center ", "Player authorization failed.");
		}
		if (scoreButton != null)
			scoreButton.interactable = res.IsSucceeded;
	}

	public void SetGCScore(int score, string leaderboard = "topscores")
	{
		GameCenterManager.OnScoreSubmitted += OnScoreSubmitted;
		GameCenterManager.ReportScore(score, leaderboard);
	}

	private void OnScoreSubmitted (ISN_Result result) {
		GameCenterManager.OnScoreSubmitted -= OnScoreSubmitted;
		if(result.IsSucceeded)  {
			Debug.Log("Score Submitted");
		} else {
			Debug.Log("Score Submit Failed");
		}
	}

	public void ShowScores(string leaderboardId) {
		//ShowLeaderboard(leaderboardId, GK_TimeSpan.ALL_TIME);
		GameCenterManager.ShowLeaderboard(leaderBoardId);
	}


}
