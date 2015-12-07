﻿using UnityEngine;
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
	private int ammo;
	private bool reloading;
	private int fullClipSize;
	private int clipSize;
	private int reserveSize;

	public Texture tex;

	CursorLockMode wantedMode;


	void Start ()
	{
		rotationY = 0.0f;
		rotationX = 0.0f;
		FOV = 90.0f;
		gunShot = this.GetComponents<AudioSource> ()[0];
		reloadSound = this.GetComponents<AudioSource> () [1];
		wantedMode = CursorLockMode.Locked;
		sensitivityM = sensitivityMDefault;
		fullClipSize = 5;
		clipSize = fullClipSize;
		ammo = 20;
		reserveSize = ammo - clipSize;
		reloading = false;
	}

	
	void Update ()
	{
		rotationX += Input.GetAxis("Mouse X") * sensitivityM;
		rotationX = Mathf.Clamp (rotationX, minimumX, maximumX);
		rotationY += Input.GetAxis("Mouse Y") * sensitivityM;
		rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
		
		transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

		FOV -= Input.GetAxisRaw ("Mouse ScrollWheel") * sensitivityW;
		FOV = Mathf.Clamp (FOV, minimumFOV, maximumFOV);
		sensitivityM = sensitivityMDefault * FOV / maximumFOV;
		sensitivityM = Mathf.Clamp (FOV, 1f/8f * sensitivityMDefault, sensitivityMDefault);
		Camera.main.fieldOfView = FOV;

		//FIRE
		if (Input.GetMouseButtonDown (0) && !gunShot.isPlaying && clipSize > 0 && !reloadSound.isPlaying) {
			RaycastHit hit;
			if (Physics.Raycast (transform.position, transform.forward, out hit)) {
				if(hit.collider.tag == "Soldier") {
					//hit.collider.gameObject.GetComponent<ReachGoal>().kill();
					Debug.Log ("killed");
					hit.collider.gameObject.GetComponent<MasterBehaviour> ().getHit (3);
				}
			}
			gunShot.Play ();
			clipSize -= 1;
			ammo -= 1;
		}

		//RELOAD
		if (Input.GetMouseButtonDown (1) && clipSize < fullClipSize && reserveSize > 0 && !gunShot.isPlaying && !reloading) {
			reloading = true;
			reloadSound.Play ();
		}

		if (reloading) {
			if (!reloadSound.isPlaying){
				Debug.Log ("sound not playing");
				clipSize = Mathf.Min(reserveSize, fullClipSize);
				reserveSize = ammo - clipSize;
				reloading = false;
			}
		}

		if (Input.GetKeyDown (KeyCode.R))
			Application.LoadLevel(Application.loadedLevel);

	}

	
	// Apply requested cursor state
	void SetCursorState ()
	{
		Cursor.lockState = wantedMode;
		// Hide cursor when locking
		Cursor.visible = (CursorLockMode.Locked != wantedMode);
	}

	void OnGUI () {

		//Rect textureCrop = new Rect ((tex.width/2f-Screen.width/2f)/tex.width, (tex.height/2f-Screen.height/2f)/tex.height, Screen.width/tex.width, Screen.height/tex.height);
//		Rect textureCrop = new Rect ((tex.width/2f-Screen.width/2f)/tex.width, (tex.height/2f-Screen.height/2f)/tex.height, 1f, 1f);
//		Vector2 position = new Vector2 (Screen.width/2f, 0f);
//		GUI.BeginGroup (new Rect (position.x, position.y, tex.width * textureCrop.width, tex.height * textureCrop.height));
		GUI.DrawTexture (new Rect (Screen.width/2, 0, Screen.width/2, Screen.height), tex);
//		GUI.DrawTexture (new Rect (-tex.width * textureCrop.x, -tex.height * textureCrop.y, tex.width * textureCrop.width, tex.height * textureCrop.height), tex );
//		GUI.EndGroup ();

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Cursor.lockState = wantedMode = CursorLockMode.None;
		}

		GUILayout.BeginVertical ();
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
		
		switch (Cursor.lockState)
		{
		case CursorLockMode.None:
			GUILayout.Label ("Cursor is normal");
			if (GUILayout.Button ("Lock cursor"))
				wantedMode = CursorLockMode.Locked;
			if (GUILayout.Button ("Confine cursor"))
				wantedMode = CursorLockMode.Confined;
			break;
		case CursorLockMode.Confined:
			GUILayout.Label ("Cursor is confined");
			if (GUILayout.Button ("Lock cursor"))
				wantedMode = CursorLockMode.Locked;
			if (GUILayout.Button ("Release cursor"))
				wantedMode = CursorLockMode.None;
			break;
		case CursorLockMode.Locked:
			GUILayout.Label ("Cursor is locked");
			if (GUILayout.Button ("Unlock cursor"))
				wantedMode = CursorLockMode.None;
			if (GUILayout.Button ("Confine cursor"))
				wantedMode = CursorLockMode.Confined;
			break;
		}
		
		GUILayout.EndVertical ();
		
		SetCursorState ();

		showAmmoCount ();

	}

	void showAmmoCount(){
		int w = Screen.width, h = Screen.height;
		
		GUIStyle style = new GUIStyle();
		
		Rect rect = new Rect(0, 0, w, h);
		style.alignment = TextAnchor.LowerRight;
		style.fontSize = h * 5 / 100;
		style.normal.textColor = Color.white;
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