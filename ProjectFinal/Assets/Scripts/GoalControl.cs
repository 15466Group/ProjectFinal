using UnityEngine;
using System.Collections;

public class GoalControl : MonoBehaviour {

	public string idle;
	public string walking;
	public string running;
	public string dying;
	public string alertRunning;
	public float colliderScalar;

	private Animation anim;
	private float smooth;
	private float maxSpeed;
	private Vector3 velocity;
	private float walkingSpeed;

	private Vector3 previousValidPos;
	private float radius;
	private int obstacleLayer;
	private int soldierLayer;
	private int health;

	private float noiseRadius;
	public AudioSource noise;
	public GameObject noiseSphere;
	private GameObject clonedNoiseSphere;
	private Material mat;
	private float startAlpha;

	public bool isDead { get; set; } 
	private Texture2D healthTex;

	private bool gamePaused;

	public bool enemiesAlerted { get; set; }

	public bool won { get; set; }
	public GameObject endPoint;
	
	void Start()
	{
		noiseRadius = noiseSphere.transform.lossyScale.x / 2f;
		startAlpha = 55f;
		radius = 2.0f;
		smooth = 5.0f;
		maxSpeed = 20.0f;
		walkingSpeed = 5.0f;
		velocity = Vector3.zero;
		anim = GetComponent<Animation> ();
		anim.CrossFade (idle);
		previousValidPos = transform.position;
		soldierLayer = 1 << (LayerMask.NameToLayer("Soldier"));
		obstacleLayer = 1 << (LayerMask.NameToLayer("Obstacles"));
		health = 10;
		healthTex = new Texture2D(1, 1);
		healthTex.SetPixel(0,0,Color.green);
		healthTex.Apply();
		gamePaused = false;
		won = false;
		enemiesAlerted = false;
	}

	void Update()
	{
		if (Vector3.Distance (endPoint.transform.position, transform.position) < 5f) {
			won = true;
		}
		if (won) {
			return;
		}
		if (isDead)
			return; // do end scene
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		velocity = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		velocity = velocity * maxSpeed;
		velocity = velocity.normalized * (Mathf.Min (velocity.magnitude, maxSpeed));

		Vector3 targetPosition = transform.position + velocity * Time.deltaTime;
		if (velocity != new Vector3())
			RotateTo (targetPosition);
		if (checkCollisions ()) {
			previousValidPos = transform.position;
			transform.position += velocity * Time.deltaTime;
		}


		checkForMakingSound ();

		doAnimation ();
		
	}

	//player can make noise to try and attract those nearby her
	void checkForMakingSound(){
		//spawn sphere for length of audio component
		if (Input.GetKeyDown (KeyCode.Q) && !noise.isPlaying && Time.timeScale > 0f) {
			noise.Play();
			clonedNoiseSphere = (GameObject)Instantiate(noiseSphere, transform.position, Quaternion.identity);
			mat = clonedNoiseSphere.GetComponent<MeshRenderer>().material;
			mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, startAlpha/255f);
			Destroy(clonedNoiseSphere, noise.clip.length);
			Collider[] hits = Physics.OverlapSphere(transform.position, noiseRadius, soldierLayer);
			foreach (Collider soldier in hits){
				soldier.GetComponent<MasterBehaviour>().hearsNoise(transform.position);
			}
		}
		if (gamePaused && Time.timeScale > 0f) {
				gamePaused = false;
				noise.UnPause();
		}
		if (noise.isPlaying) {
			if (Time.timeScale == 0f){
				gamePaused = true;
				noise.Pause();
			} else {
				float targetAlpha = 0f;
				float ratio = Time.deltaTime / noise.clip.length;
				float a = (startAlpha - targetAlpha) * ratio;
				mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, mat.color.a - a/255f);
			}
		}
	}

	//check for crouching
	void doAnimation(){
		float mag = velocity.magnitude;
		if (mag > 0.0f && mag <= walkingSpeed) {
			anim.CrossFade (walking);
		} else if (mag > walkingSpeed) {
			if (!enemiesAlerted)
				anim.CrossFade (running);
			else
				anim.CrossFade(alertRunning);
		} else {
			anim.CrossFade (idle);
		}
	}

	void RotateTo(Vector3 targetPosition){
		//maxDistance is the maximum ray distance
		Quaternion destinationRotation;
		Vector3 relativePosition;
		relativePosition = targetPosition - transform.position;
		//		Debug.DrawRay(transform.position,relativePosition*10,Color.red);
		//		Debug.DrawRay(transform.position,velocity.normalized*20,Color.green);
		//		Debug.DrawRay(transform.position,acceleration.normalized*10,Color.blue);
		destinationRotation = Quaternion.LookRotation (relativePosition);
		transform.rotation = Quaternion.Slerp (transform.rotation, destinationRotation, Time.deltaTime * smooth);
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.magenta / 3f;
		Gizmos.DrawSphere (transform.position + (transform.up * transform.localScale.y*colliderScalar), radius);
		Gizmos.DrawSphere (transform.position, noiseRadius);
	}

	bool checkCollisions(){
		Collider[] hits = Physics.OverlapSphere (transform.position + (transform.up * transform.localScale.y*colliderScalar), radius, obstacleLayer | soldierLayer);
		if (hits.Length > 0) {
			transform.position = previousValidPos;
			return false;
		}
		return true;
	}

	public void getHit() {
		health --;
		if (health == 0) {
			die ();
		}
	}

	public void die() {
		health = 0;
		isDead = true;
		anim.CrossFade (dying);
		//end game
	}

	void OnGUI () {
		for (int i = 0; i < health; i++) {
			DrawQuad (new Rect(15 + 20*i, Screen.height - 50, 0.5f, 30.0f));
		}
	}
	void DrawQuad(Rect position) {
		GUI.skin.box.normal.background = healthTex;
		GUI.Box(position, GUIContent.none);
	}
}
