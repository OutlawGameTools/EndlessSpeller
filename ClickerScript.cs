using UnityEngine;
using System.Collections;

public class ClickerScript : MonoBehaviour {

	public SceneManager sceneMgr;

	void Awake()
	{
		//LevelMenu2D.I.gameObject.SetActive(true);
		LevelMenu2D.I.OnItemClicked += OnItemClicked; 
	}

	void Start()
	{
		sceneMgr = GameObject.Find ("SceneManager").GetComponent<SceneManager>();
		//LevelMenu2D.I.gotoItem(sceneMgr.currPlayerPrefab);
		//int idx = PlayerPrefs.GetInt("RunnerIndex");
		//print ("idx: " + idx);
		//LevelMenu2D.I.initialItemNumber = idx;
	}

	void OnItemClicked (int itemIndex, GameObject itemObject) 
	{
		Debug.Log("Item Clicked: " + itemIndex + " Name: " + itemObject.name); 
		sceneMgr.ChooseRunner(itemObject.name);
		//LevelMenu2D.I.gameObject.SetActive(false);
	}

}