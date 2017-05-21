using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;


public class Player : MonoBehaviour {

	public float playerXStart = -6.15f;
	public float playerYStart = 1.89f;

	public float xSpeed = 50;
	public float ySpeed = 0;
	public float jumpXSpeed = 2;
	public float jumpYSpeed = 2;

	private Vector2 newSpeed;
	private Rigidbody2D rb2D;
	public bool isJumping = false;
	public bool isRunning = false;
	private Animator animator;

	public int numJumps = 0;

//	private GameObject ptTxt;
	public GameObject pointText;
//	public Text pointTxtCmp;
	public GameObject groundHit;
	public GameObject goalHit;

	private SceneManager sceneMgr;
	private GameObject canvas;

	private bool firstHit = true;

	public bool modeEndless = false;

	void Awake()
	{
		rb2D = GetComponent<Rigidbody2D>();
		sceneMgr = GameObject.Find ("SceneManager").GetComponent<SceneManager>();
		canvas = GameObject.Find("Canvas");

	
		firstHit = false;
		GameObject ptTxt = Instantiate(pointText) as GameObject;
		ptTxt.transform.SetParent(canvas.transform, false);
		ptTxt.transform.position = transform.position;
		ptTxt.GetComponent<Text>().text = "   ";
		Destroy(ptTxt, 1f);
	
	}
	
	// Use this for initialization
	void Start () 
	{
		//newSpeed = new Vector2(xSpeed, ySpeed);
		//rb2D.AddForce(newSpeed);

		animator = GetComponent<Animator>();

	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		if (!isRunning && !isJumping)
		{
			if (modeEndless)
			{
				animator.Play ("Land");
				Vector3 move = new Vector3(sceneMgr.platformSpeed, 0, 0);
				transform.position += move * Time.deltaTime;
			}
			//print("!isRunning");
		}
		else
		{
			//animator.SetInteger("AnimState", 1);
			if (!isJumping && isRunning)
			{
				animator.Play("Run");

				// check to see if we're at the edge of a platform
				float toX = transform.position.x + 1f;
				float toY = transform.position.y + 1.5f;
				
				Vector2 dir = (transform.forward + transform.right).normalized;
				dir = new Vector2(0,-1);
				RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1, 1 << LayerMask.NameToLayer ("Platform"));
				if (hit.collider == null && !isJumping) 
				{
					//animator.SetInteger("AnimState", 2);
					animator.Play("Jump");
					isJumping = true;
					isRunning = false;
					//print("No platform! Jump!");
					rb2D.isKinematic = false;
					rb2D.AddForce(new Vector2(jumpXSpeed, jumpYSpeed), ForceMode2D.Impulse);
					sceneMgr.numPlatforms--;	// get rid of platform we just jumped from (get rid of count, anyway)
					
				}
			
			}
		}

//		Vector3 dir2 = transform.TransformDirection(Vector3.forward) * 10;
//		dir2 = (transform.forward + transform.right).normalized;
//		Debug.DrawRay(transform.position, dir, Color.white, 0, false);
	}

	void UpdateScoreStuff()
	{
		// keep track of the lifetime number of jumps here
		int allJumps = PlayerPrefs.GetInt("Total Jumps") + numJumps;
		PlayerPrefs.SetInt("Total Jumps", allJumps);
		PlayerPrefs.Save();
		numJumps = 0;
	}

	void StartRunning()
	{
		isJumping = false;
		isRunning = true;
		rb2D.isKinematic = true;
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.CompareTag("Platform"))
		{

			int points = coll.gameObject.GetComponent<Platform> ().pointValue;
			string ptEnding = " Pts";
			if (points == 1)
				ptEnding = " Pt";

			isJumping = false;
			//rb2D.isKinematic = true;
//			if (animator.GetInteger("AnimState") == 2)
//			{
//				animator.SetInteger("AnimState", 1);
//				animator.Play("adv_run");
//			}
//			else
//				if (sceneMgr.gameState == "playing")

			if (modeEndless)
			{
				animator.Play("Land");
				Invoke("StartRunning", 0.5f);
			}
			else
			{
				isJumping = false;
				isRunning = true;
				//rb2D.isKinematic = true;
			}

			sceneMgr.numTimesJumpedThisLevel++;
			sceneMgr.AddToPoints (points);
			if (points > 0)
			{
				GameObject ptTxt = Instantiate(pointText) as GameObject;
				ptTxt.transform.SetParent(canvas.transform, false);
				ptTxt.transform.position = coll.gameObject.transform.position;
				Text textComponent = ptTxt.GetComponent<Text>();
				textComponent.text = points + ptEnding;
				//pointTxtCmp.text = points + ptEnding;
				//ptTxt.GetComponent<Text>().text = points + ptEnding;

				float endPos = coll.gameObject.transform.position.y + 1.5f;
				float driftTime = 2f;
				ptTxt.transform.DOMoveY(endPos, driftTime).SetEase(Ease.OutQuint);
				textComponent.DOFade(0f, driftTime);
				//ptTxt.gameObject.GetComponent<Text>().DOFade(0f, driftTime);
				//pointTxtCmp.DOFade(0f, driftTime);
				Destroy(ptTxt, driftTime);
				Destroy(textComponent, driftTime);

			}
			else if (firstHit)
			{
			}

			numJumps++;

			// see if we just landed on the last platform available
			if (coll.gameObject == sceneMgr.platforms[sceneMgr.platforms.Count-1] && !sceneMgr.playerRelaxed)
			{
				sceneMgr.playerStressed = true;
				sceneMgr.ChangeTileColors(Color.red);
				sceneMgr.stressSnd = SoundManager.PlaySFX(SoundManager.Load("Stress"));
			}
		}
	}


	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.CompareTag("RunnerGoal"))
		{
			sceneMgr.gameState = "goalreached";
			Destroy (gameObject);
			//Instantiate(goalHit, transform.position, Quaternion.identity);
			GameObject instance = CFX_SpawnSystem.GetNextObject(goalHit);
			instance.transform.position = transform.position;


			// update score stuff
			UpdateScoreStuff();

		}
		else if (coll.gameObject.CompareTag("Bumper"))
		{
			//Instantiate(groundHit, gameObject.transform.position, Quaternion.identity);
			GameObject instance = CFX_SpawnSystem.GetNextObject(groundHit);
			instance.transform.position = transform.position;
			isJumping = true;
			rb2D.isKinematic = false;
			//transform.position = new Vector2(playerXStart, playerYStart);
			sceneMgr.gameState = "felltodeath";
			Destroy(gameObject);
			
			// update score stuff
			UpdateScoreStuff();
		}
		else if (coll.gameObject.CompareTag("FallBoundary"))
		{
			rb2D.isKinematic = false;
			gameObject.GetComponent<BoxCollider2D>().isTrigger = true; // so he can't hit platforms on the way down.
			sceneMgr.gameState = "falling";
			SoundManager.PlaySFX(SoundManager.Load("Falling"));
		}

	}

}
