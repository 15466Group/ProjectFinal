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
		GUI.skin.label.wordWrap = true;
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
				GUI.skin.label.fontSize = 50;
				GUILayout.BeginVertical ();
				{
					GUILayout.Label ("", GUILayout.Height (Screen.height/4));
				}
				GUILayout.EndVertical ();
				GUILayout.Label ("SNEAKER", GUILayout.Height(65));
				GUI.skin.label.fontSize = 40;
				GUILayout.BeginHorizontal ();
				{
					GUILayout.BeginHorizontal (GUILayout.Width (50));
					GUILayout.Label ("");
					GUILayout.EndHorizontal ();
					GUILayout.Label ("AND", GUILayout.Height(65));
				}
				GUILayout.EndHorizontal ();
				GUI.skin.label.fontSize = 50;
				GUILayout.BeginHorizontal ();
				{
					GUILayout.BeginHorizontal (GUILayout.Width (100));
					GUILayout.Label ("");
					GUILayout.EndHorizontal ();
					GUILayout.Label ("SNIPER", GUILayout.Height(65));
				}
				GUILayout.EndHorizontal ();
				GUILayout.Label ("", GUILayout.Height (Screen.height/20));
				GUILayout.BeginHorizontal ();
				{
					GUILayout.BeginVertical ();
					{
						if (GUILayout.Button (new GUIContent("Tutorial_1", "Move_the_Sneaker_to_the_green_gem_using_WASD!"))) {
							Application.LoadLevel ("Tutorial1");
						}
						if (GUILayout.Button (new GUIContent("Tutorial_2", "Sniper_was_unprepared_and_brought_no_ammo!\nUse_\"Q\"_to_whistle_and_lure_guards_to_your\nlocation."))) {
							Application.LoadLevel ("Tutorial2");
						}
						if (GUILayout.Button (new GUIContent("Tutorial_3", "Provide_cover_with_Sniper!_Use_the_mouse_to\naim,_left_click_to_shoot._Keep_an_eye_on_your\nammo_and_reload_with_right_click!"))) {
							Application.LoadLevel ("Tutorial3");
						}
						GUILayout.Label("", GUILayout.Height (10));
						if (GUILayout.Button (new GUIContent("Mission_1", "This_is_what_you've_been_training_for!_Use_all\nyour_skills_to_capture_the_green_gem."))) {
							Application.LoadLevel ("Aesthetics 2");
						}
						if (GUILayout.Button (new GUIContent("Mission_2", "We've_detected_another_gem_in_an_enemy\nshipment!_You_know_the_drill."))) {
							Application.LoadLevel ("Aesthetics 3");
						}
						GUILayout.Label("", GUILayout.Height (10));
						if (GUILayout.Button(new GUIContent("Quit", "You_want_to_quit_already?_Coward."))){
							Application.Quit();
						}
					}
					GUILayout.EndVertical ();
					GUI.skin.label.fontSize = 15;
					GUILayout.Label ("", GUILayout.Width (100));
					GUILayout.Label(GUI.tooltip, GUILayout.Width(450), GUILayout.Height (400));
					Debug.Log (GUI.tooltip);
					GUI.skin.label.fontSize = 50;
				}
				GUILayout.EndHorizontal ();
			}
			GUILayout.EndVertical ();
		}
		GUILayout.EndHorizontal ();
	}
}
