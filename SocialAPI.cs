using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

public class SocialAPI : MonoBehaviour {

	#if UNITY_IPHONE || UNITY_STANDALONE_OSX

	ILeaderboard m_Leaderboard;

	public bool isAuthenticated = false;
	public bool showSocialWindow = false;
	private string whichOne = "l"; // l or a

	// Use this for initialization
	void Start () {
	
		Social.localUser.Authenticate(ProcessAuthentication);
	}

	void ProcessAuthentication(bool a_success)
	{
		if (a_success)
		{
			isAuthenticated = true;
		}
		else
		{
			isAuthenticated = false;
		}
		Debug.Log ("SocialAPI: isAuthenticated=" + isAuthenticated +
		           ", userName=" + Social.localUser.userName + 
		           ", id=" + Social.localUser.id + 
		           ", underage=" + Social.localUser.underage);
	}

	void Update()
	{
		if (showSocialWindow)
		{
			showSocialWindow = false;
			if (whichOne == "a")
				Social.LoadAchievements (ProcessLoadAchievements);
			else if (whichOne == "l")
				Social.LoadScores ("topscores", ProcessLoadScores);
		}
	}

	void ProcessLoadAchievements(IAchievement[] a_achievements)
	{
		if (a_achievements.Length > 0)
		{
			Debug.Log ("Got " + a_achievements.Length + " achievement instances.");
			string myAchievements = "My Achievements: \n";
			foreach (IAchievement achievement in a_achievements)
			{
				myAchievements += "\t" +
					achievement.id + " " +
						achievement.percentCompleted + " " +
						achievement.completed + " " + 
						achievement.lastReportedDate + "\n";
			}
			Debug.Log (myAchievements);
		}
		else
		{
			Debug.Log ("No achievements returned.");
		}

		if (false)
		{
			// ui here
		}
		else
		{
			Social.ShowAchievementsUI();
		}

	}

	void ProcessLoadScores(IScore[] a_scores)
	{

	}

	public void DoLeaderboard () {
		m_Leaderboard = Social.Active.CreateLeaderboard();
		m_Leaderboard.id = "topscores";
		m_Leaderboard.LoadScores(result => DidLoadLeaderboard(result));
	}
	
	void DidLoadLeaderboard (bool result) {
		Debug.Log("Received " + m_Leaderboard.scores.Length + " scores " + result.ToString());
		foreach (IScore score in m_Leaderboard.scores)
			Debug.Log(score);

		Social.ReportScore (17, "topscores", success => {
			Debug.Log(success ? "Reported score successfully" : "Failed to report score");
		});
	}

	public void ShowLeaderboard()
	{
		whichOne = "l";
		showSocialWindow = true;
	}

	public void ShowAchievements()
	{
		whichOne = "a";
		showSocialWindow = true;
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(5, Screen.height - 32, 100, 20), "Achievement"))
		{
			showSocialWindow = true;
		}
	}


	#endif

}
