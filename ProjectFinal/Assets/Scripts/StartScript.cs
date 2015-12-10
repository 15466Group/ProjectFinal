using UnityEngine;
using System.Collections;

public class StartScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		GUILayout.BeginHorizontal ();
		{
			GUILayout.BeginVertical (GUILayout.Width (Screen.width/3));
			{
				GUILayout.Label ("", GUILayout.Width (Screen.width/3));
			}
			GUILayout.EndVertical ();

			GUILayout.BeginVertical (GUILayout.Width (Screen.width/3));
			{
				GUILayout.BeginVertical ();
				{
					GUILayout.Label ("", GUILayout.Height (Screen.height/3));
				}
				GUILayout.EndVertical ();
				GUILayout.Label ("WELCOME TO GAME");
				if (GUILayout.Button ("Tutorial 1")) {
					Application.LoadLevel ("Tutorial1");
				}
				if (GUILayout.Button ("Tutorial 2")) {
					Application.LoadLevel ("Tutorial2");
				}
				if (GUILayout.Button ("Tutorial 3")) {
					Application.LoadLevel ("Tutorial3");
				}
				if (GUILayout.Button ("Mission 1")) {
					Application.LoadLevel ("Aesthetics 2");
				}
			}
			GUILayout.EndVertical ();
		}
		GUILayout.EndHorizontal ();
	}
}
