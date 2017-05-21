using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour {

	public float xSpeed = -0.4f;
	public float ySpeed = 0;

	public int pointValue = 0;

	private Vector2 newSpeed;
	private Rigidbody2D rb2D;
	private Renderer rndr;

	private SceneManager sceneMgr;

	void Awake()
	{
		rb2D = GetComponent<Rigidbody2D>();
		rndr = GetComponent<Renderer>();
		sceneMgr = GameObject.Find ("SceneManager").GetComponent<SceneManager>();
	}

	// Use this for initialization
	void Start () 
	{
		newSpeed = new Vector2(sceneMgr.platformSpeed, ySpeed);
		//rb2D.AddForce(newSpeed);
		if (!rndr.isVisible)
			sceneMgr.numPlatformsOffScreen++;
	}

	void FixedUpdate()
	{
		bool stoppedMoving = false;

		if (sceneMgr.gameState == "playing")
		{
			stoppedMoving = false;
			Vector3 move = new Vector3(sceneMgr.platformSpeed, 0, 0);
			transform.position += move * Time.deltaTime;
		}
		else if (!stoppedMoving && sceneMgr.gameState != "playing")
		{
			stoppedMoving = true;
			rb2D.AddForce(-newSpeed);
			rb2D.isKinematic = false;
			rb2D.gravityScale = Random.Range(-0.2f, -0.5f);
			if (sceneMgr.howLevelEnded == "felltodeath")
				rb2D.gravityScale *= -1;
			gameObject.GetComponent<EdgeCollider2D>().enabled = false;
		}
//		else if (!stoppedMoving && sceneMgr.gameState == "felltodeath")
//		{
//			stoppedMoving = true;
//			rb2D.AddForce(-newSpeed);
//			rb2D.isKinematic = false;
//			rb2D.gravityScale = Random.Range(-0.2f, -0.5f);
//		}
	}

	// after we scroll off the screen
	void OnBecameInvisible () 
	{
		//if (sceneMgr.platforms.Count > 0)
		//	sceneMgr.platforms.RemoveAt(0);
		Destroy(gameObject);
		sceneMgr.numPlatforms--; //decrement this when player jumps
	}

	void OnBecameVisible()
	{
		sceneMgr.numPlatformsOffScreen--;
	}
}
