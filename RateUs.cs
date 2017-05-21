using UnityEngine;
using System.Collections;

public class RateUs : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void RateTheGame()
	{
		IOSRateUsPopUp rate = IOSRateUsPopUp.Create("Like this game?", "Please rate to support future updates!");
		rate.OnComplete += onRatePopUpClose;
	}

	private void onRatePopUpClose(IOSDialogResult result) {
		switch(result) {
		case IOSDialogResult.RATED:
			Debug.Log ("Rate button pressed");
			IOSNativeUtility.RedirectToAppStoreRatingPage();
			break;
		case IOSDialogResult.REMIND:
			Debug.Log ("Remind button pressed");
			break;
		case IOSDialogResult.DECLINED:
			Debug.Log ("Decline button pressed");
			break;
			
		}

		IOSNativePopUpManager.dismissCurrentAlert();
		//IOSNativePopUpManager.showMessage("Result", result.ToString() + " button pressed");
	}

}
