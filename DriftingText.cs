using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class DriftingText : MonoBehaviour {

	public GameObject canvas;
	public GameObject textPrefab;
	public float driftTime = 1f;
	public float relativeYEndPos = 1.5f;

	private Text msgTxt;
	
	void Awake()
	{
		if (canvas == null)
			canvas = GameObject.Find("Canvas");
	}
	
	public void MakeDriftingText (string msg, Vector2 pos) 
	{
		GameObject ptTxt = Instantiate(textPrefab) as GameObject;
		msgTxt = ptTxt.GetComponent<Text>();
		ptTxt.transform.SetParent(canvas.transform, false);
		ptTxt.transform.position = pos;
		msgTxt.text = msg;
		
		float endYPos = pos.y + relativeYEndPos;

		ptTxt.transform.DOMoveY(endYPos, driftTime).SetEase(Ease.OutQuint);
		msgTxt.DOFade(0f, driftTime);
		Destroy(ptTxt, driftTime);
	}

}
