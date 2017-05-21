using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class ShowPicture : MonoBehaviour {

	bool isBig = false;
	Vector3 origPos;
	Vector3 origSize;
	Vector3 centerPos = new Vector3(0f, 0f, 0f);
	
	public void ToggleButtonPic(Button thePic)
	{
		if (isBig)
		{
			ShrinkButtonPic (thePic);
			isBig = false;
		}
		else
		{
			GrowButtonPic (thePic);
			isBig = true;
		}
	}
	

	public void TogglePic(GameObject thePic)
	{
		if (isBig)
		{
			ShrinkPic (thePic);
			isBig = false;
		}
		else
		{
			GrowPic (thePic);
			isBig = true;
		}
	}

	public void GrowButtonPic(Button thePic)
	{
		origSize = thePic.transform.localScale;
		origPos = thePic.transform.position;
		//thePic.transform.DoMove(centerPos, 0.3f);
		thePic.transform.DOScale(4.5f, 0.3f);
		thePic.transform.position = new Vector3(0f, 0f, 0f);
		thePic.transform.SetAsLastSibling();
	}

	public void GrowPic(GameObject thePic)
	{
		origSize = thePic.transform.localScale;
		origPos = thePic.transform.position;
		//thePic.transform.DoMove(new Vector3(0f, 0f, 0f), 0.3f);
		thePic.transform.position = centerPos;
		thePic.transform.DOScale(0.5f, 0.3f);
	}

	public void ShrinkButtonPic(Button thePic)
	{
		thePic.transform.DOScale(origSize, 0.3f);
		thePic.transform.position = origPos;
		thePic.transform.DOMove(origPos, 0.3f);
		GUI.depth = 0;
	}

	public void ShrinkPic(GameObject thePic)
	{
		thePic.transform.DOScale(origSize, 0.3f);
		thePic.transform.position = origPos;
		thePic.transform.DOMove(origPos, 0.3f);
	}

}
