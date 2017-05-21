using UnityEngine;
using System.Collections;

public class GoToSceneButton : MonoBehaviour {
	
	public void GoToScene(string sceneName)
	{
		Application.LoadLevel (sceneName);
	}

}
