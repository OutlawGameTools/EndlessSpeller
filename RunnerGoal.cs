using UnityEngine;
using System.Collections;

public class RunnerGoal : MonoBehaviour {

	AudioSource audio;
	public SceneManager sceneMgr;

	// Use this for initialization
	void Start () 
	{
		audio = GetComponent<AudioSource>();
		sceneMgr = GameObject.Find ("SceneManager").GetComponent<SceneManager> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.CompareTag("Player"))
		{
			//AudioSource.PlayClipAtPoint(audio.clip, transform.position);
			SoundManager.PlaySFX(SoundManager.Load("Goal Reached"));
			sceneMgr.AddToPoints (100);
			sceneMgr.HideFinishEarlyButton();
		
			//MakeDriftingText dText = gameObject.GetComponent<DriftingText>();
		}
	}
}
