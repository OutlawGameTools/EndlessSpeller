using UnityEngine;
using System.Collections;

public class BumperBottom : MonoBehaviour {

	AudioSource audio;

	// Use this for initialization
	void Start () 
	{
		audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.CompareTag("Player"))
		{
			audio.Play();
		}
	}
}
