﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterScheduler : MonoBehaviour {

	public GameObject player;
	public GameObject characters;
	public GameObject sniper;

	private RCameraControl sniperScript;
	private GoalControl gc;

	public GameObject plane;
	public float nodeSize;

	private int numChars;

	private MasterBehaviour[] behaviourScripts;

	private PathfindingScheduler pathfinder;
	private LinkedList<GameObject> pathFindingChars;
	private LinkedListNode<GameObject> currCharForPath;
	private List<GameObject> deadSet;
	private List<Vector3> seenDeadSet;

	private float seenTime;
	private float shootDistance;
	private float sightDist;
	
	private NaiveBayes NB;
	private List<MasterBehaviour> currentlySearching;
	private float maxDist;

	private bool gamePaused;//handle pausing in middle of sounds
	public int alert;
	private int maxAlert;
	private int normAlert;
	private int notAlert;

	private AudioSource lowkeyBGM;
	private AudioSource hikeyBGM;
	private AudioSource deathSound;
	private AudioSource winSound;
	private bool deathPlayed;
	private bool winPlayed;


	// Use this for initialization
	void Start () {
		numChars = characters.transform.childCount;
//		Debug.Log (numChars);
		behaviourScripts = new MasterBehaviour[numChars];
		
		for (int i = 0; i < numChars; i++) {
			MasterBehaviour mb = characters.transform.GetChild(i).GetComponent<MasterBehaviour>();
			mb.Starta(plane, nodeSize, sniper.transform.position);
			behaviourScripts[i] = mb;
		}

		pathfinder = new PathfindingScheduler ();
		pathFindingChars = new LinkedList<GameObject> ();
		deadSet = new List<GameObject>();
		seenDeadSet = new List<Vector3> ();

		shootDistance = 10f;
		sightDist = 100.0f;

		maxDist = 100f;
		NB = new NaiveBayes (behaviourScripts [0].reachGoal.state.sGrid.hiddenSpaceCost, maxDist);
		NB.Starta ();
		currentlySearching = new List<MasterBehaviour> ();
		gamePaused = false;

		maxAlert = 2;
		normAlert = 1;
		notAlert = 0;
		alert = notAlert;

		sniperScript = sniper.GetComponent <RCameraControl> ();
		lowkeyBGM = GetComponents<AudioSource> () [0];
		hikeyBGM = GetComponents<AudioSource> () [1];
		deathSound = GetComponents<AudioSource> () [2];
		winSound = GetComponents<AudioSource> () [3];
		lowkeyBGM.volume = 0.7f;
		hikeyBGM.volume = 0.7f;
		lowkeyBGM.Play ();

		gc = player.GetComponent<GoalControl> ();

		deathPlayed = false;
		winPlayed = false;

	}

	void Awake(){
		DontDestroyOnLoad (NB); //want to save probabilities for each scene
	}
	
	// Update is called once per frame
	void Update () {
		//loop through the characters, update their status (ie senses, poi, etc)
		//if the character is not reaching a goal, then wipe it's search state clean
		//loop through chatacters again and do behavior.Updatea
		//finally, pass in array of characters to pathfinding scheduler and let it do its thing


		//update anything that has to do with guards with respect to other guards
		checkGuardRelationship ();

		if (sniperScript.fired > 0) {
			alert = maxAlert;
			if (lowkeyBGM.isPlaying && !hikeyBGM.isPlaying){
				lowkeyBGM.Stop();
				hikeyBGM.Play ();
			}
		}

		if (gc.isDead && !deathSound.isPlaying && !deathPlayed) {
			lowkeyBGM.Stop ();
			hikeyBGM.Stop ();
			if(Time.timeScale < 0.67f) {
				deathSound.Play();
				deathPlayed = true;
			}
		}
		if (gc.won && !winSound.isPlaying && !winPlayed) {
			lowkeyBGM.Stop ();
			hikeyBGM.Stop ();
			if(Time.timeScale < 0.6f) {
				winSound.volume = 0.65f;
				winSound.Play();
				winPlayed = true;
			}
		}



		//update guard status and check relationship between player and handle pathfinding
		for (int i = 0; i < numChars; i++){
			GameObject currChar = characters.transform.GetChild(i).gameObject;
			MasterBehaviour mb = behaviourScripts[i];
			updateStatus(currChar, mb);
			mb.Updatea();
			bool contained = pathFindingChars.Contains(currChar);
			bool findingAPath = mb.isReachingGoal();
			if (findingAPath && !contained){
				LinkedListNode<GameObject> c = new LinkedListNode<GameObject>(currChar);
				pathFindingChars.AddLast(c);
				if (currCharForPath == null)
					currCharForPath = c;
			} 
			else if (!findingAPath && contained){
				if (currCharForPath != null && (currChar.name == currCharForPath.Value.name)){
					currCharForPath = currCharForPath.Next;
				}
				pathFindingChars.Remove(currChar); //not sure if it finds the actualy character though
				//if we're doing continue search thing, need to reinit the characters search state
			}
		}

		//put the pathfinding characters in the pathfinder
		pathfinder.characters = pathFindingChars;
		pathfinder.currCharNode = currCharForPath;
		//do pathfinding stuff
		pathfinder.Updatea ();
		if (currCharForPath != null)
			currCharForPath = currCharForPath.Next;
		if (currCharForPath == null)
			currCharForPath = pathFindingChars.First;
	
	}

	void updateStatus(GameObject currChar, MasterBehaviour mb){
		if (mb.isDead) {
			if (mb.addToDeadSet) {
				//just died, need to make a noise when dying
				mb.addToDeadSet = false;
				deadSet.Add (currChar);
			}
			return;
		}
		mb.isShooting = false;
		int oldAlertLevel = mb.alertLevel;
		float playerAngle;
		if (mb.seesPlayer)
			playerAngle = 360.0f;
		else
			playerAngle = 30.0f + (10.0f * mb.alertLevel);
		updateSniperInfoForChar (currChar, mb);
		checkIfNearHeardNoise (currChar, mb);
		updatePlayerInfoForChar (currChar, mb, playerAngle);
		updateSearchingStatus ();
	}

	//sometimes player whistles and npc hears it
	void checkIfNearHeardNoise(GameObject currChar, MasterBehaviour mb){
		if (mb.isGoingToHeardPos) {
			Debug.Log ("leaving nowww");
			if (Vector3.Distance(currChar.transform.position, mb.poi) < nodeSize){
				mb.isGoingToHeardPos = false;
				Debug.Log("too close");
			} else {
				Debug.Log ("kk omw");
				mb.takingCover = false;
				mb.isGoaling = true;
				mb.reachedCover = false;
			}
		}
	}

	void updateSearchingStatus(){
		int len = currentlySearching.Count;
		if (len > 0) {
			List<int> toRemove = new List<int> ();
			for (int i = 0; i < len; i++) {
				//done searching and have a failure, remove him from this searching list
				if (currentlySearching [i].dirSearchCountDown <= 0f) {
					toRemove.Add (i);
				}
			}
			foreach (int j in toRemove) {
				currentlySearching.RemoveAt (j);
			}
			if (currentlySearching.Count <= 0) {
				//no one is searching anymore so player escaped so update with failure
				NB.updateInputs (false, Vector3.zero);
			}
		}
	}

	void updateSniperInfoForChar(GameObject currChar, MasterBehaviour mb){
		if (mb.dirSearchCountDown > 0)
			return;
		if (mb.knowsOfSniper ()) {
			if (!mb.seesPlayer)
				mb.takingCover = true;
			if (!mb.isGoingToSeenPlayerPos && !mb.isGoingToHeardPos && (!mb.reachedCover || sniperScript.firstSniperFired)) {
				//going to cover
				mb.poi = mb.takeCover.coverPoint (currChar.transform.position);
				mb.isGoaling = true;
				mb.isGoingToCover = true;
				mb.isGoingToSeenPlayerPos = false;
				mb.coverSpot = mb.poi;
			}
			if (Vector3.Distance (mb.coverSpot, currChar.transform.position) <= nodeSize && !mb.seesPlayer){
				//reached cover, not seeing player
				mb.reachedCover = true;
				mb.isGoingToCover = false;
//				Debug.Log(mb.gameObject.name + " reached cover");
				mb.isGoaling = false;
			} else {
//				Debug.Log (mb.gameObject.name + " not reached cover");
				mb.reachedCover = false;
			}
		} else {
			mb.takingCover = false;
		}
	}

	void updatePlayerInfoForChar(GameObject currChar, MasterBehaviour mb, float playerAngle){
		RaycastHit hit;
		Debug.DrawRay (currChar.transform.position, (Mathf.Sqrt (3) * currChar.transform.forward + currChar.transform.right).normalized * sightDist, Color.red);
		Debug.DrawRay (currChar.transform.position, (Mathf.Sqrt (3) * currChar.transform.forward - currChar.transform.right).normalized * sightDist, Color.red);
//		Debug.DrawLine (currChar.transform.position, player.transform.position, Color.black);
		if (Physics.Raycast (currChar.transform.position, player.transform.position - currChar.transform.position, out hit, sightDist)) {
			float angle = Vector3.Angle (currChar.transform.forward, player.transform.position - currChar.transform.position);
			if ((hit.collider.gameObject == player) && ((angle <= playerAngle) || Vector3.Distance (currChar.transform.position, player.transform.position) < 10f)) {
				if (!mb.seesPlayer && !mb.isGoingToSeenPlayerPos && !gc.isDead) {
					mb.alert.Play (); //alert is first
				}
				mb.lastSeen = player.transform.position;
				mb.lastSeenForward = player.transform.forward;
				mb.needsToRaiseAlertLevel = true;
				mb.seesPlayer = true;
				mb.seenTime += Time.deltaTime;
				if (mb.seenTime > 1f) {
					alert = Mathf.Max (alert, normAlert);
					mb.isShooting = true;
					//mb.seenTime = 0f;
				}
				mb.poi = player.transform.position;
				mb.isGoaling = true;
				mb.isGoingToSeenPlayerPos = true;
				mb.isGoingToCover = false;
				mb.disturbed = true;
				mb.takingCover = false;
				mb.isGoingToHeardPos = false;
				makeEveryoneStopSearching(mb.lastSeen);
			} else {
				if(mb.seesPlayer) {
					extendSearchInForwardDir(mb);
				}
				mb.seesPlayer = false;
				mb.seenTime = 0f;
			}
		} else {
			if(mb.seesPlayer) {
				extendSearchInForwardDir(mb);
			}
			mb.seesPlayer = false;
			mb.seenTime = 0f;
		}

		//reached the poi of where he last saw the player
		if (Vector3.Distance (mb.poi, currChar.transform.position) <= nodeSize && mb.isGoingToSeenPlayerPos && !mb.seesPlayer) {
			guessDirection(mb);
		} 
//		if (alert == normAlert) {
//			mb.disturbed = true;
//			mb.dirSearchCountDown = 1f;
//		}
	}

	void extendSearchInForwardDir(MasterBehaviour mb){
		Vector3 gridCoords = mb.reachGoal.state.sGrid.getGridCoords(player.transform.position + player.transform.forward * 20f);
		Node potentialNode = mb.reachGoal.state.sGrid.grid[(int)gridCoords.x, (int)gridCoords.z];
		if (potentialNode.free)
			mb.poi = potentialNode.loc;
	}

	void guessDirection(MasterBehaviour mb){
		mb.isGoingToSeenPlayerPos = false;
		mb.isGoaling = false;
		mb.seenTime = 0f;
		mb.wanderDir = NB.pointsToSearch (mb.lastSeenForward, mb.lastSeen, mb.reachGoal.state.sGrid.grid);
		mb.dirSearchCountDown = 6.0f;
		currentlySearching.Add (mb);
	}

	void makeEveryoneStopSearching(Vector3 playerPos){
		//someone has seen the player so stop searching everyone!
		if (currentlySearching.Count > 0) {
//			Debug.Log ("stop searching everyone!");
			foreach (MasterBehaviour mb in currentlySearching) {
				mb.dirSearchCountDown = 0f;
			}
			currentlySearching.Clear ();
			NB.updateInputs (true, playerPos);
		}
	}


	void checkGuardRelationship(){
		//add dead characters to seenDeadSet if an alive character sees a dead one
		float sightAngle = 30.0f;
		int updatedDeadSet = 0;
		for (int i = 0; i < numChars; i++) {
			GameObject currChar = characters.transform.GetChild (i).gameObject;
			MasterBehaviour mb = behaviourScripts [i];
			if (!mb.isDead){
				updatedDeadSet += checkToSeeDead(currChar, mb, sightAngle);
			}
			if (mb.alert.isPlaying){
				if (Time.timeScale == 0f){
					gamePaused = true;
					mb.alert.Pause();
				}
			}
			if (gamePaused && Time.timeScale > 0f){
				gamePaused = false;
				mb.alert.UnPause();
			}
		}
		
		//seenDeadSet now updated so pass this along to every character because assumed they are now notified of all dead positions
//		if (updatedDeadSet > 0) {
		if (alert > notAlert) {
			gc.enemiesAlerted = true;
			if (lowkeyBGM.isPlaying && !hikeyBGM.isPlaying){
				lowkeyBGM.Stop();
				hikeyBGM.Play ();
			}
			for (int i = 0; i < numChars; i++) {
				GameObject currChar = characters.transform.GetChild (i).gameObject;
				MasterBehaviour mb = behaviourScripts [i];
				if (!mb.isDead) {
					mb.updateDeadSet (seenDeadSet);
					//fixme
					if(alert == maxAlert && !mb.sniperPosKnown) {
						mb.updateSniperPos();
					}
					mb.needsToRaiseAlertLevel = true;
					//should stop and stare for a few frames
				}
			}
		}
		
		for (int i = 0; i < numChars; i++) {
			GameObject currChar = characters.transform.GetChild (i).gameObject;
			MasterBehaviour mb = behaviourScripts [i];
			if (mb.needsToRaiseAlertLevel && !mb.isDead)
				mb.raiseAlertLevel();
			mb.needsToRaiseAlertLevel = false;
		}
	}

	int checkToSeeDead(GameObject currChar, MasterBehaviour mb, float sightAngle){
		Vector3 deadPos;
		RaycastHit hit;
		int updatedSeen = 0;
		List<int> wasRemoved = new List<int> ();
		int i = 0;
		foreach (GameObject deadChar in deadSet) {
			deadPos = deadChar.transform.position;
			Debug.DrawRay(currChar.transform.position, deadPos - currChar.transform.position, Color.yellow);
			if (Vector3.Angle(currChar.transform.forward, deadPos - currChar.transform.position) <= sightAngle){
				if (Physics.Raycast(currChar.transform.position, deadPos - currChar.transform.position, out hit, sightDist)){
					if (hit.collider.gameObject == deadChar){
						seenDeadSet.Add(deadPos);
						wasRemoved.Add(i);
						updatedSeen += 1;
						//he's seen a dead patrol man so he starts going to hide, and while hes going he tells others to hide
						//this takes a while
					}
				}
			}
			i++;
		}
		foreach (int j in wasRemoved) {
			deadSet.RemoveAt(j);
		}
		return updatedSeen;
	}
}
