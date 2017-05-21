using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using VoxelBusters.NativePlugins;
//using VoxelBusters.Utility;

public class Options : MonoBehaviour {

	public Button musicOn;
	public Button musicOff;

	// Related to mail
	[SerializeField]
	private string					m_plainTextMailBody	= "This is plain text mail";
	[SerializeField]
	private string[] 				m_mailRecipients;


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

	}
	
	// Update is called once per frame
	void Update () {
	
	}

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

	public void RateMyApp()
	{
//		if (NPSettings.Utility.RateMyApp.IsEnabled)
//		{
//			NPBinding.Utility.RateMyApp.AskForReviewNow();
//		}
//		else
//		{
//			//AddNewResult("Enable RateMyApp in NPSettings.");
//		}
	}


	//private bool IsMailServiceAvailable()
//	{
//		return NPBinding.Sharing.IsMailServiceAvailable();
//	}
//	
//	private void SendPlainTextMail()
//	{
//		NPBinding.Sharing.SendPlainTextMail("Demo", m_plainTextMailBody, m_mailRecipients, FinishedSharing);
//	}

	//private void FinishedSharing (eShareResult _result)
	//{
		//AddNewResult("Finished sharing");
		//AppendResult("Share Result = " + _result);
	//}



}
