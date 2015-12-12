using UnityEngine;
using System.Collections;

public class StartScript : MonoBehaviour {

	public Font myFont;
	public GUIStyle buttonStyle;
	private Texture2D bTexN;
	private Texture2D bTexH;
	// Use this for initialization
	void Start () {
		bTexN = new Texture2D(1, 1);
		bTexN.SetPixel(0,0,Color.black);
		bTexN.Apply();

		bTexH = new Texture2D(1, 1);
		bTexH.SetPixel(0,0,Color.white);
		bTexH.Apply();

		//buttonStyle.onHover.background = bTexH;
		buttonStyle.hover.background = bTexH;
		//buttonStyle.onHover.textColor = Color.black;
		buttonStyle.hover.textColor = Color.black;
		
		
		buttonStyle.normal.background = bTexN;
		buttonStyle.normal.textColor = Color.white;
		buttonStyle.stretchWidth = false;
		buttonStyle.active.background = bTexN;
		buttonStyle.active.textColor = Color.black;
		
		buttonStyle.fontSize = 20;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		GUI.skin.font = myFont;
		GUI.skin.button = buttonStyle;
		GUI.skin.label.normal.background = bTexN;
		GUI.skin.label.stretchWidth = false;
		GUI.skin.label.stretchHeight = false;
		//GUI.contentColor = Color.black;
		GUILayout.BeginHorizontal ();
		{
			GUILayout.BeginVertical (GUILayout.Width (Screen.width/4));
			{
				GUILayout.Label ("");
			}
			GUILayout.EndVertical ();

			GUILayout.BeginVertical ();
			{
				GUILayout.BeginVertical ();
				{
					GUILayout.Label ("", GUILayout.Height (Screen.height/4));
				}
				GUILayout.EndVertical ();
				GUILayout.Label ("SNEAKER");
				GUILayout.BeginHorizontal ();
				{
					GUILayout.BeginHorizontal (GUILayout.Width (50));
					GUILayout.Label ("");
					GUILayout.EndHorizontal ();

					GUILayout.Label ("AND");
				}
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				{
					GUILayout.BeginHorizontal (GUILayout.Width (100));
					GUILayout.Label ("");
					GUILayout.EndHorizontal ();
					GUILayout.Label ("SNIPER");
				}
				GUILayout.EndHorizontal ();
				GUILayout.Label ("", GUILayout.Height (Screen.height/10));


				if (GUILayout.Button ("Tutorial_1")) {
					Application.LoadLevel ("Tutorial1");
				}

				if (GUILayout.Button ("Tutorial_2")) {
					Application.LoadLevel ("Tutorial2");
				}
				if (GUILayout.Button ("Tutorial_3")) {
					Application.LoadLevel ("Tutorial3");
				}
				if (GUILayout.Button ("Mission_1")) {
					Application.LoadLevel ("Aesthetics 2");
				}
				if (GUILayout.Button("Quit")){
					Application.Quit();
				}
			}
			GUILayout.EndVertical ();
		}
		GUILayout.EndHorizontal ();
	}
}
