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
				if (GUILayout.Button ("START LEVEL")) {
					Application.LoadLevel (1);
				}
			}
			GUILayout.EndVertical ();
		}
		GUILayout.EndHorizontal ();
	}
}
