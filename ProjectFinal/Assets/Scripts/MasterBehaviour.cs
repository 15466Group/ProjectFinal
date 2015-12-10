using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterBehaviour : MonoBehaviour {

	private Animation anim;
	public Vector3 poi { get; set; }  //point of interest that he reaches goal on
	public int health { get; set; }
	public bool seesPlayer { get; set; }
	public Vector3 lastSeen { get; set; }
	public Vector3 lastSeenForward { get; set; }
	public bool seesDeadPeople { get; set; }
	private List<Vector3> deadPeopleSeen;

	public bool isGoingToHeardPos { get; set; }
	public bool isDead { get; set; }
	public bool addToDeadSet { get; set; }
	public bool isShooting { get; set; }
	public bool disturbed { get; set; }
	public bool sniperPosKnown { get; set; }
	public Vector3 sniperPos { get; set; }
	public int alertLevel { get; set; }
	public int maxAlertLevel { get; set; }
	public bool needsToRaiseAlertLevel { get; set; }
	public bool takingCover { get; set; }
	public Vector3 coverSpot { get; set; }
	public bool reachedCover { get; set; }
	public bool isReloading { get; set; }
	public bool isGoaling { get; set; }
	public bool isGoingToCover { get; set; }
	public bool isGoingToSeenPlayerPos { get; set; }
	public int ammoCount { get; set; }
	public int wanderDir { get; set; }

	public float dirSearchCountDown { get; set; }

	public ReachGoal reachGoal { get; set; }
	private Wander wander;
	private StandStill standstill;
	private Patrol patrol;
	public string defaultBehaviour;
	private Vector3 velocity;
	public TakeCover takeCover { get; set; }
	private BoxCollider bc;
	private float bcCenterYNorm;

	public string idle;
	public string walking;
	public string running;
	public string dying;
	public string hit;
	public string crouchIdle;
	public string crouchRun;

	public float seenTime;

	private float walkingSpeed;
	public GameObject player;
	private GoalControl gc;
	private LineRenderer lr;

	private bool fixedDeadCollider;


	public AudioSource gunShot;
	public AudioSource alert;

	private gun gunThingy;
	float fireCycle;


	// Use this for initialization
	public void Starta (GameObject plane, float nodeSize, Vector3 sP) {

		fixedDeadCollider = false;
		poi = Vector3.zero;
		health = 3;
		seesPlayer = false;
		lastSeen = Vector3.zero;
		lastSeenForward = Vector3.zero;
		seesDeadPeople = false;
		isGoingToHeardPos = false;
		disturbed = false;
		isDead = false;
		addToDeadSet = false;
		takingCover = false;
		coverSpot = Vector3.zero;
		reachedCover = false;
		sniperPosKnown = false;
		sniperPos = sP;
		isGoaling = false;
		isGoingToSeenPlayerPos = false;
		isGoingToCover = false;
		dirSearchCountDown = 0.0f;

		reachGoal = GetComponent<ReachGoal> ();
		wander = GetComponent<Wander> ();
		standstill = GetComponent<StandStill> ();
		patrol = GetComponent<Patrol> ();
		gc = player.GetComponent<GoalControl> ();

		gunThingy = GetComponentInChildren <gun> ();

		reachGoal.plane = plane;
		reachGoal.nodeSize = nodeSize;
		reachGoal.goalPos = poi;
		reachGoal.sniperPos = sniperPos;
		reachGoal.Starta ();
		wander.Starta ();
		patrol.Starta ();
		standstill.Starta ();
		takeCover = new TakeCover (reachGoal.state.sGrid.hiddenSpaceCost, reachGoal.state.sGrid.initSpaceCost,
		                           reachGoal.state.sGrid.grid, reachGoal.state.sGrid.spaceCostScalars);
		bc = GetComponent<BoxCollider>();
		bcCenterYNorm = bc.center.y;
		anim = GetComponent<Animation> ();
		anim.CrossFade (idle);
		walkingSpeed = 10.0f;
		lr = this.GetComponentInParent<LineRenderer> ();
		seenTime = 0f;
		alertLevel = 0;
		maxAlertLevel = 3;
		needsToRaiseAlertLevel = false;
		isReloading = false;
		isShooting = false;
		ammoCount = 0;
//		Debug.Log (transform.name);
		fireCycle = 0f;
		gunShot.volume = 0.3f;
		alert.volume = 0.6f;
		wanderDir = 0;
	}

	public void Updatea(){
		//decision tree later for different combination of senses being true
		lr.enabled = false;
		if (isDead) {
			if (!fixedDeadCollider){
				transform.gameObject.layer = LayerMask.NameToLayer("Dead"); //now dead so avoid this space;
				transform.gameObject.tag = "Dead";
				bc.center = new Vector3(0f, -0.5f, 0f);
				fixedDeadCollider = true;
			}
			return;
		}

		if (seesPlayer) {
			Quaternion destinationRotation;
			Vector3 relativePosition;
			relativePosition = player.transform.position - transform.position;
			destinationRotation = Quaternion.LookRotation (relativePosition);
			transform.rotation = Quaternion.Slerp (transform.rotation, destinationRotation, Time.deltaTime * 500f);
		}
		//and if the character is facing the character
		if (isShooting && !gc.isDead && !isReloading && seesPlayer) {
			//&& !gunShot.isPlaying
			fireCycle += Time.deltaTime;
			if(fireCycle < 0.5f && Time.timeScale > 0f)
				shoot ();
			else if(fireCycle < 1.2f){}
				//donothing
			else 
				fireCycle = 0f;
//			gunThingy.SendMessage ("Fire");
			//gunThingy.Fire();
			//gunThingy.SendMessage("Fire");
//			this.SendMessage ("Fire");
		}
//		if (!(seesPlayer || seesDeadPeople || hearsSomething)) {
		if (!isReachingGoal()) {
//			if (!takingCover){
			if (takingCover && dirSearchCountDown <= 0) {
				//donothing;
				wanderDir = 0;
				standstill.Updatea ();
				velocity = standstill.velocity;
			}
			else if(disturbed) {
				if(wanderDir == -1 && dirSearchCountDown > 0f) {
					float tempT = Mathf.Acos (lastSeenForward.x);
					if(lastSeenForward.z < 0) {
						tempT = 2*Mathf.PI - tempT;
					}
					wander.minT = tempT;
					wander.maxT = tempT + (2f*Mathf.PI / 3f);
					dirSearchCountDown -= Time.deltaTime;
				}
				else if(wanderDir == 1 && dirSearchCountDown > 0f) {
					float tempT = Mathf.Acos (lastSeenForward.x);
					if(lastSeenForward.z < 0) {
						tempT = 2*Mathf.PI - tempT;
					}
					wander.maxT = tempT;
					wander.minT = tempT - (2f*Mathf.PI / 3f);
					dirSearchCountDown -= Time.deltaTime;
				}
				else {
					float tempT = Mathf.Acos (transform.forward.normalized.x);
					if(transform.forward.z < 0) {
						tempT = 2*Mathf.PI - tempT;
					}
					wander.minT = tempT - Mathf.PI/4f;
					wander.maxT = tempT + Mathf.PI/4f;
				}
				wander.Updatea();
				velocity = wander.velocity;
			}
			else {
				doDefaultBehaviour();
			}
//			}
		} else {
//			takingCover = false;
//			Debug.Log("Update GoalPos to: " + reachGoal.goalPos);
			reachGoal.goalPos = poi;
			velocity = reachGoal.velocity;
		}
		//reaching goal is done with pathfinding which is handled by the pathfinder schedule and the masterscheduler

		doAnimation ();
	}

	void doDefaultBehaviour(){
		if (string.Compare("StandStill", defaultBehaviour) == 0) {
			standstill.Updatea ();
			velocity = standstill.velocity;
		} else if (string.Compare("Patrol", defaultBehaviour) == 0) {
//			Debug.Log (transform.gameObject.name + ": defaulting");
			patrol.Updatea ();
			velocity = patrol.velocity;
		} else {
			wander.Updatea();
			velocity = wander.velocity;
		}

	}

	public bool isReachingGoal(){
		//		return (seesPlayer || seesDeadPeople || hearsSomething) && !isDead;
		//		return (seesPlayer || hearsSomething) && !isDead;
		return ((isGoingToCover || isGoingToSeenPlayerPos || isGoingToHeardPos) && !isDead);
	}

	public void getHit(int damage) {
		if (isDead) {
			return;
		}
		health -= damage;
		if (health <= 0) {
			isDead = true;
			addToDeadSet = true;
			Transform l = transform.Find("Spotlight");
			l.gameObject.SetActive(false);
			anim.CrossFade (dying);
			//need to make a noise when dying
		}
		Debug.Log ("hit for " + damage);
	}

	public void shoot () {
		gunShot.Play ();
		//tells the gun to shoot
		gunThingy.SendMessage("Fire");
		//lr.SetPosition (0, transform.position + Vector3.up);
		//lr.SetPosition (1, player.transform.position + Vector3.up);
		//lr.SetWidth (1f, 1f);
		//lr.enabled = true;
		//gc.getHit ();

	}

	public void doAnimation(){
//		Debug.Log ("doinganimation");
		if (isDead) {
			return;
		}
		float mag = velocity.magnitude;
		if (takingCover){
			bc.center = new Vector3(0f, bcCenterYNorm * 0.5f, 0f);
			if (mag > 0.0f && mag <= walkingSpeed) {
				anim.CrossFade (crouchRun);
			} else if (mag > walkingSpeed) {
				anim.CrossFade (crouchRun);
			} else {
				anim.CrossFade (crouchIdle);
			}
		} else {
			if (sniperPosKnown == true){
				bc.center = new Vector3(0f, bcCenterYNorm, 0f);
			}
			if (mag > 0.0f && mag <= walkingSpeed) {
				anim.CrossFade (walking);
			} else if (mag > walkingSpeed) {
				anim.CrossFade (running);
			} else {
				anim.CrossFade (idle);
			}
		}
	}

	public void updateDeadSet(List<Vector3> seenDeadSet){
		//do the animation or draw the alert thing
		seesDeadPeople = true;
		deadPeopleSeen = seenDeadSet;
	}

	public void updateSniperPos(){
		sniperPosKnown = true;
//		reachGoal.state.sGrid.sniperPosKnown = true;
		reachGoal.updateGridSniperPos ();
		takeCover.sniperPosKnown = true;
		Debug.Log ("knows sniper pos");
//		Debug.Break ();
	}

	public void raiseAlertLevel(){
		return;
	}

	//either from hearing, seeing, or knowing another guard was taken out
	public bool knowsOfSniper(){
		return (sniperPosKnown || seesDeadPeople);
	}

	public void hearsNoise(Vector3 pos){
		if (!seesPlayer){
			//what to do if in cover and what to do if knowsOfSniper and what to do if seesSniper, probability stuff?
			isGoingToHeardPos = true;
			poi = pos;
		}
	}
}
