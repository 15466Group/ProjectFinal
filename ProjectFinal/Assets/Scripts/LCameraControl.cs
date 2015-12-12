using UnityEngine;
using System.Collections;

public class LCameraControl : MonoBehaviour {
	
	public GameObject player;
	private float ratio;
	// Use this for initialization
	void Start () {
//		transform.position = player.transform.position + Vector3.up * 980f;
//		transform.LookAt (Vector3.zero);
		float w = (float)Screen.width;
		float h = (float)Screen.height;
		ratio = w / h;
	}
	
	// Update is called once per frame
	void Update () {
//		transform.position = player.transform.position + Vector3.up * 980f;
//		transform.LookAt (Vector3.zero);
		float w = (float)Screen.width;
		float h = (float)Screen.height;
		ratio = w / h;
		this.GetComponent <Camera> ().fieldOfView = (21f) * (-1f) * ratio + 76f; 
	}
}
