using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class RCameraControl : MonoBehaviour {
	
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	private float sensitivityM;
	public float sensitivityMDefault;
	public float sensitivityW = 2f;

	public float minimumX = -60f;
	public float maximumX = 70f;
	
	public float minimumY = -60f;
	public float maximumY = 0f;

	public float minimumFOV = 20f;
	public float maximumFOV = 90f;

	private float rotationY;
	private float rotationX;
	private float FOV;

	private AudioSource gunShot;
	private AudioSource reloadSound;
	private AudioSource emptyClipSound;
	private AudioSource sniperGetDown;
	private int ammo;
	private bool reloading;
	private int fullClipSize;
	private int clipSize;
	private int reserveSize;

	public Texture tex;
	private Texture2D pauseTex;

	CursorLockMode wantedMode;

	public GameObject sneaker;
	private GoalControl gc;

	public int fired { get; set; }
	public bool firstSniperFired { get; set; }
	private float timer;

	void Start ()
	{
		rotationY = 0.0f;
		rotationX = 0.0f;
		FOV = 90.0f;
		AudioSource[] A = this.GetComponents<AudioSource> ();
		gunShot = A[0];
		gunShot.volume = 0.6f;
		reloadSound =  A[1];
		emptyClipSound = A[2];
		sniperGetDown = A[3];
		sniperGetDown.volume = 0.32f;
		wantedMode = CursorLockMode.Locked;
		sensitivityM = 0.5f;
		fullClipSize = 3;
		clipSize = fullClipSize;
		ammo = 6;
		if (string.Compare (Application.loadedLevelName, "Tutorial2") == 0) {
			ammo = 0;
			clipSize = 0;
		}
		reserveSize = ammo - clipSize;
		reloading = false;
		Time.timeScale = 1f;

		pauseTex = new Texture2D(1, 1);
		Color gray = new Color (0.1f, 0.1f, 0.1f, 0.8f);
		pauseTex.SetPixel(0,0,gray);
		pauseTex.Apply();
		fired = 0;
		firstSniperFired = false;
		timer = 120f;
		timer += 1f;
	}

	
	void Update ()
	{
		if (gc == null) {
			gc = sneaker.GetComponent <GoalControl> ();
		}
		if (Time.timeScale == 1f) {
			rotationX += Input.GetAxis ("Mouse X") * sensitivityM;
			rotationX = Mathf.Clamp (rotationX, minimumX, maximumX);
			rotationY += Input.GetAxis ("Mouse Y") * sensitivityM;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
		
			transform.localEulerAngles = new Vector3 (-rotationY, rotationX, 0);

			FOV -= Input.GetAxisRaw ("Mouse ScrollWheel") * sensitivityW;
			FOV = Mathf.Clamp (FOV, minimumFOV, maximumFOV);
			sensitivityM = sensitivityMDefault * FOV / maximumFOV;
			sensitivityM = Mathf.Clamp (FOV, 1f / 8f * sensitivityMDefault, sensitivityMDefault);
			Camera.main.fieldOfView = FOV;

			//FIRE
			if (Input.GetMouseButtonDown (0)){
				if (clipSize == 0 && !gunShot.isPlaying && !reloadSound.isPlaying && !emptyClipSound.isPlaying){
					emptyClipSound.Play();
				}
				else if (!gunShot.isPlaying && clipSize > 0 && !reloadSound.isPlaying) {
					RaycastHit hit;
					if (Physics.Raycast (transform.position, transform.forward, out hit)) {
						if (hit.collider.tag == "Soldier") {
							//hit.collider.gameObject.GetComponent<ReachGoal>().kill();
							Debug.Log ("killed");
							hit.collider.gameObject.GetComponent<MasterBehaviour> ().getHit (3);
						}
						if (hit.collider.tag == "Player") {
							hit.collider.gameObject.GetComponent<GoalControl> ().die ();
						}
					}
					gunShot.Play ();
					fired++;
					if (fired == 1){
						firstSniperFired = true;
						sniperGetDown.Play ();
					}
					else
						firstSniperFired = false;
					clipSize -= 1;
					ammo -= 1;
				}
			}

			//RELOAD
			if (Input.GetMouseButtonDown (1) && clipSize < fullClipSize && reserveSize > 0 && !gunShot.isPlaying && !reloading && !emptyClipSound.isPlaying) {
				reloading = true;
				reloadSound.Play ();
			}

			if (reloading) {
				if (!reloadSound.isPlaying) {
					clipSize = Mathf.Min (reserveSize, fullClipSize);
					reserveSize = ammo - clipSize;
					reloading = false;
				}
			}
		}
		if ((Input.GetKeyDown (KeyCode.Escape) && !gc.won && !gc.isDead) || gc.won) {
			//Cursor.lockState = wantedMode = CursorLockMode.None;
			//			if(Time.timeScale != 0f) {
			//				Debug.Log ("game paused");
			//				Time.timeScale = 0f;
			//			}
			//			else {
			//				Debug.Log ("game unpaused");
			//				Time.timeScale = 1f;
			//			}
			Time.timeScale = 0f;
		}
		if (gc.isDead && Time.timeScale > 0f) {
			//			Time.timeScale -=(Time.timeScale / 500f);
			Time.timeScale = Mathf.Min (Time.timeScale, 0.7f);
			Time.timeScale -= Time.timeScale/500f;
		}
		timer = Mathf.Max (0f, timer - Time.deltaTime);
		if (timer <= 0f) {
			gc.die();
		}
	}

	
	// Apply requested cursor state
	void SetCursorState ()
	{
		Cursor.lockState = wantedMode;
		// Hide cursor when locking
		Cursor.visible = (CursorLockMode.Locked != wantedMode);
	}

	void OnGUI () {
		if (gc == null) {
			gc = sneaker.GetComponent <GoalControl> ();
		}
		//Rect textureCrop = new Rect ((tex.width/2f-Screen.width/2f)/tex.width, (tex.height/2f-Screen.height/2f)/tex.height, Screen.width/tex.width, Screen.height/tex.height);
//		Rect textureCrop = new Rect ((tex.width/2f-Screen.width/2f)/tex.width, (tex.height/2f-Screen.height/2f)/tex.height, 1f, 1f);
//		Vector2 position = new Vector2 (Screen.width/2f, 0f);
//		GUI.BeginGroup (new Rect (position.x, position.y, tex.width * textureCrop.width, tex.height * textureCrop.height));
		GUI.DrawTexture (new Rect (Screen.width/2, 0, Screen.width/2, Screen.height), tex);
//		GUI.DrawTexture (new Rect (-tex.width * textureCrop.x, -tex.height * textureCrop.y, tex.width * textureCrop.width, tex.height * textureCrop.height), tex );
//		GUI.EndGroup ();

		if (Time.timeScale < 1f) {
			GUI.skin.box.normal.background = pauseTex;
			GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
			if(gc.won) {


				if(Time.timeScale < 0.6f) {
					Cursor.lockState = wantedMode = CursorLockMode.None;
					GUILayout.BeginHorizontal ();
					{
						GUILayout.BeginVertical (GUILayout.Width (Screen.width/3));
						{
							GUILayout.Label ("");
						}
						GUILayout.EndVertical ();
						
						GUILayout.BeginVertical (GUILayout.Width (Screen.width/3));
						{
							GUILayout.BeginVertical ();
							{
								GUILayout.Label ("", GUILayout.Height (Screen.height/3));
							}
							GUILayout.EndVertical ();
							GUILayout.Label ("YOU WON");
							if (GUILayout.Button ("MENU")) {
								Application.LoadLevel ("Start");
							}
							if (GUILayout.Button ("RESTART")) {
								Application.LoadLevel (Application.loadedLevel);
							}
						}
						GUILayout.EndVertical ();
					}
					GUILayout.EndHorizontal ();
				}
			}
			else if (gc.isDead) {
				if(Time.timeScale < 0.5f) {
					Cursor.lockState = wantedMode = CursorLockMode.None;
					GUILayout.BeginHorizontal ();
					{
						GUILayout.BeginVertical (GUILayout.Width (Screen.width/3));
						{
							GUILayout.Label ("");
						}
						GUILayout.EndVertical ();
						
						GUILayout.BeginVertical (GUILayout.Width (Screen.width/3));
						{
							GUILayout.BeginVertical ();
							{
								GUILayout.Label ("", GUILayout.Height (Screen.height/3));
							}
							GUILayout.EndVertical ();
							GUILayout.Label ("YOU DIED");
							if (GUILayout.Button ("MENU")) {
								Application.LoadLevel ("Start");
							}
							if (GUILayout.Button ("RESTART")) {
								Application.LoadLevel (Application.loadedLevel);
							}
						}
						GUILayout.EndVertical ();
					}
					GUILayout.EndHorizontal ();
				}
			}
			else {

	//			GUILayout.BeginVertical ();
	//			if (GUILayout.Button ("UNPAUSE")) {
	//				Time.timeScale = 1f;
	//			}
	//			if (GUILayout.Button ("QUIT")) {
	//				Application.LoadLevel (4);
	//			}
	//			if (GUILayout.Button ("RESTART")) {
	//				Application.LoadLevel (Application.loadedLevel);
	//			}
	//			GUILayout.EndVertical ();
				Cursor.lockState = wantedMode = CursorLockMode.None;
				GUILayout.BeginHorizontal ();
				{
					GUILayout.BeginVertical (GUILayout.Width (Screen.width/3));
					{
						GUILayout.Label ("");
					}
					GUILayout.EndVertical ();
					
					GUILayout.BeginVertical (GUILayout.Width (Screen.width/3));
					{
						GUILayout.BeginVertical ();
						{
							GUILayout.Label ("", GUILayout.Height (Screen.height/3));
						}
						GUILayout.EndVertical ();
						if (GUILayout.Button ("UNPAUSE")) {
							Time.timeScale = 1f;
						}
						if (GUILayout.Button ("MENU")) {
							Application.LoadLevel ("Start");
						}
						if (GUILayout.Button ("RESTART")) {
							Application.LoadLevel (Application.loadedLevel);
						}
					}
					GUILayout.EndVertical ();
				}
				GUILayout.EndHorizontal ();
			}

		} else {
			wantedMode = CursorLockMode.Locked;
		}

//		GUILayout.BeginVertical ();


//		 Release cursor on escape keypress
//		if (Input.GetKeyDown (KeyCode.Escape)) {
//
//			switch(Cursor.lockState) {
//			case CursorLockMode.None:
//				wantedMode = CursorLockMode.Locked;
//				break;
//			case CursorLockMode.Locked:
//				wantedMode = CursorLockMode.None;
//				break;
//			case CursorLockMode.Confined:
//				wantedMode = CursorLockMode.None;
//				break;
//			}
//
//		}
		
//		switch (Cursor.lockState)
//		{
//		case CursorLockMode.None:
//			GUILayout.Label ("Cursor is normal");
//			if (GUILayout.Button ("Lock cursor"))
//				wantedMode = CursorLockMode.Locked;
//			if (GUILayout.Button ("Confine cursor"))
//				wantedMode = CursorLockMode.Confined;
//			break;
//		case CursorLockMode.Confined:
//			GUILayout.Label ("Cursor is confined");
//			if (GUILayout.Button ("Lock cursor"))
//				wantedMode = CursorLockMode.Locked;
//			if (GUILayout.Button ("Release cursor"))
//				wantedMode = CursorLockMode.None;
//			break;
//		case CursorLockMode.Locked:
//			GUILayout.Label ("Cursor is locked");
//			if (GUILayout.Button ("Unlock cursor"))
//				wantedMode = CursorLockMode.None;
//			if (GUILayout.Button ("Confine cursor"))
//				wantedMode = CursorLockMode.Confined;
//			break;
//		}
		
//		GUILayout.EndVertical ();
		
		SetCursorState ();

		showAmmoCount ();

		showReloadHelper ();

		showTimer();

	}

	void showTimer(){
		string text;
		int intTime = Mathf.RoundToInt (timer);
		text = intTime.ToString ();

		int w = Screen.width, h = Screen.height;
		
		GUIStyle style = new GUIStyle();
		
		Rect rect = new Rect(0, 0, w, h);
		style.alignment = TextAnchor.UpperCenter;
		style.fontSize = h * 5 / 100;
		style.normal.textColor = Color.red;
		GUI.Label(rect, text, style);
	}

	void showReloadHelper(){
		string text;
		if (clipSize == 0){
			if (ammo > 0)
				text = "Right Click to Reload!";
			else
				text = "Out of Ammo!";
			int w = Screen.width, h = Screen.height;
			
			GUIStyle style = new GUIStyle();
			
			Rect rect = new Rect(0, 0, w, h);
			style.alignment = TextAnchor.UpperRight;
			style.fontSize = h * 5 / 100;
			style.normal.textColor = Color.red;
			GUI.Label(rect, text, style);
		}
	}

	void showAmmoCount(){
		int w = Screen.width, h = Screen.height;
		
		GUIStyle style = new GUIStyle();
		
		Rect rect = new Rect(w - 54, h - 38, 54, 40);
		style.alignment = TextAnchor.LowerRight;
		style.fontSize = h * 5 / 100;
		style.normal.background = new Texture2D(1, 1);
		style.normal.background.SetPixel(0,0,Color.white);
		style.normal.background.Apply ();
		style.normal.textColor = Color.black;
		string text = clipSize.ToString() + "/" + reserveSize.ToString();
		GUI.Label(rect, text, style);
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		RaycastHit hit;
		if (Physics.Raycast (transform.position, transform.forward, out hit)) {
			Gizmos.DrawCube (hit.point, new Vector3(5f, 5f, 5f));
		}
	}
}
//KEKEKEKKEKEKEKEKEKEKEKEKEKEKEKEKEKEKEKEK

