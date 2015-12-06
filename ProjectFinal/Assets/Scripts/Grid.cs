﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//lighter means avoid

public class Grid{

	public float nodeSize;
	public Vector3 goalPos { get; set; } //could be a dead body, last seen player pos, or player pos for example
	private GameObject plane;

	private int obstacleLayer;
	//for deadbodies
	private int deadLayer;
	private int dynamicLayer;

	public Node[,] grid;
	private int gridWidth;
	private int gridHeight;
	private float worldWidth;
	private float worldHeight;
	private Vector3 worldNW; //world north west, top left corner of map/plane

	private Vector3 sniperPos;
	public bool sniperPosKnown { get; set; }
	public float[,] spaceCostScalars { get; set; }

	private float initSpaceCost;
	private float overlapRadius;
	public float hiddenSpaceCost { get; set; }

	public Grid (GameObject p, Vector3 goalp, float nS, Vector3 sP) {
		plane = p;
		goalPos = goalp;
		nodeSize = nS;
		sniperPos = sP;
	}

	// Use this for initialization
	public void initStart () {
		initSpaceCost = 3.0f;
		overlapRadius = 10.0f;
		hiddenSpaceCost = 1.0f;
		worldWidth = plane.transform.lossyScale.x * 10.0f; //plane
		worldHeight = plane.transform.lossyScale.z * 10.0f; //plane

		gridWidth = Mathf.RoundToInt(worldWidth / nodeSize);
		gridHeight = Mathf.RoundToInt(worldHeight / nodeSize);

		grid = new Node[gridWidth, gridHeight];

		worldNW = plane.transform.position - (plane.transform.right * worldWidth / 2.0f) + (plane.transform.forward * worldHeight / 2.0f);

		obstacleLayer = 1 << LayerMask.NameToLayer ("Obstacles");
		deadLayer = 1 << LayerMask.NameToLayer ("Dead");
		dynamicLayer = 1 << LayerMask.NameToLayer ("Dynamic");

		//once the character knows where the sniper is, then he'll know 'if i can't see the sniper, then the sniper can't see me'
		//so he'll know that some spots will hide him from the sniper (ie behind tall buildings) and therefore will want
		//those those h values to be less. Thus, initially these scalars are 1 because they don't know where the sniper is,
		//but they'll be set to x when they do know where the sniper is.
		spaceCostScalars = new float[gridWidth, gridHeight];
		sniperPosKnown = false;
		setSpaceCostScalars ();
		initializeGrid ();
		updateGrid (goalPos);
//		drawShadeLines ();
	}

	public void setSpaceCostScalars(){
		RaycastHit hitL;
		RaycastHit hitR;
		for (int i = 0; i < gridWidth; i++) {
			for (int j = 0; j < gridHeight; j++) {
				float xp = i * nodeSize + (nodeSize/2.0f) + worldNW.x;
				float zp = -(j * nodeSize + (nodeSize/2.0f)) + worldNW.z;
				//hidden from the sniper so small value
				//float scalar = hiddenSpaceCost;
				float scalar = initSpaceCost;
				float charHeight = 5.0f;
				Vector3 nodeCenter = new Vector3(xp, charHeight, zp);

				Collider[] hits = Physics.OverlapSphere(nodeCenter, nodeSize/2.0f, obstacleLayer | deadLayer | dynamicLayer);
				int len = hits.Length;
				//cant see the sniper from this node, so vice versa
				Vector3 nodeRight = nodeCenter + Vector3.right * nodeSize/2f;
				Vector3 nodeLeft = nodeCenter - Vector3.right * nodeSize/2f;
				float camDist = 30.0f;
				if (Physics.Raycast(nodeRight, sniperPos - nodeRight, out hitR, camDist, obstacleLayer) && 
				    Physics.Raycast(nodeLeft, sniperPos - nodeLeft, out hitL, camDist, obstacleLayer)){
//				if (Physics.Raycast(nodeCenter, sniperPos - nodeCenter, out hit, Mathf.Infinity, obstacleLayer)){
					if (!hitR.collider.gameObject.CompareTag("MainCamera") &&
					    !hitL.collider.gameObject.CompareTag("MainCamera")){
//						Debug.DrawLine(nodeCenter, sniperPos, Color.magenta, 100f);
						scalar = hiddenSpaceCost;
					}
				}
				if(len == 0) {
					spaceCostScalars[i,j] = scalar;
				}
				else {
					spaceCostScalars[i,j] = 0;
				}
			}
		}
	}

	public void initializeGrid(){
		for (int i = 0; i < gridWidth; i++) {
			for (int j = 0; j < gridHeight; j ++) {
				float xp = i * nodeSize + (nodeSize/2.0f) + worldNW.x;
				float zp = -(j * nodeSize + (nodeSize/2.0f)) + worldNW.z;
				Vector3 nodeCenter = new Vector3(xp, 0.0f, zp);
				Collider[] hits = Physics.OverlapSphere(nodeCenter, nodeSize/2.0f, obstacleLayer | deadLayer | dynamicLayer);
				float h = Vector3.Distance(nodeCenter, goalPos);
				int len = hits.Length;
				if(len == 0) { 
					grid[i,j] = new Node(true, nodeCenter, i, j, h, spaceCostScalars[i,j]);
				}
				else {
					grid[i,j] = new Node(false, nodeCenter, i, j, h, spaceCostScalars[i,j]);
				}
			}
		}
	}
	
	public void updateGrid(Vector3 g){
		goalPos = g;
		for (int i = 0; i < gridWidth; i++) {
			for (int j = 0; j < gridHeight; j ++) {
				float xp = i * nodeSize + (nodeSize/2.0f) + worldNW.x;
				float zp = -(j * nodeSize + (nodeSize/2.0f)) + worldNW.z;
				Vector3 nodeCenter = new Vector3(xp, 0.0f, zp);
				Collider[] hits = Physics.OverlapSphere(nodeCenter, overlapRadius, obstacleLayer | deadLayer | dynamicLayer);
				float h = Vector3.Distance(nodeCenter, goalPos);
				int len = hits.Length;
				if (sniperPosKnown){
					if(len == 0) {
						updateNode(grid[i,j], h, true, spaceCostScalars[i,j]);
					}
					else {
						bool free = true;
						foreach(Collider c in hits){
							float dist = Vector3.Distance (c.ClosestPointOnBounds(nodeCenter), nodeCenter);
							//want to check everything in hits to see if contained in the cell
							if(dist < nodeSize/2.0f) {
								free = false;
							}
						}
						updateNode(grid[i,j], h, free, spaceCostScalars[i,j]);
					}
				}
				else {
					if(len == 0) {
						updateNode(grid[i,j], h, true, initSpaceCost);
					}
					else {
						bool free = true;
						bool foundDead = false;
						float spaceCost = initSpaceCost;
						float minDist = overlapRadius;
						foreach(Collider c in hits) {
							float dist = Vector3.Distance (c.ClosestPointOnBounds(nodeCenter), nodeCenter);
							//want to check everything in hits to see if contained in the cell
							if(dist < nodeSize/2.0f) {
								free = false;
							}
							if (c.CompareTag("Dead")){
								spaceCost = initSpaceCost; //stay away from here
								foundDead = true;
							}
							if (!foundDead){
								//only look at the obstacle closest to the node to weight the heuristic with spaceCost
								if (dist < minDist) {
									minDist = dist;
									spaceCost = (dist / overlapRadius) * (dist / overlapRadius) * initSpaceCost;
								}
							}
						}
						updateNode(grid[i,j], h, free, spaceCost);
					}
				}
			}
		}
//		drawShadeLines ();
	}

	void updateNode(Node n, float h, bool free, float spaceCost){
		n.free = free;
		n.h = h;
		n.spaceCost = spaceCost;
		n.g = Mathf.Infinity;
		n.f = Mathf.Infinity;
	}

	void drawShadeLines(){
		for (int i = 0; i < gridWidth; i++) {
			for (int j = 0; j < gridHeight; j ++) {
				float sc = grid[i,j].spaceCost;
				Color scaledGrey = Color.white * (sc/initSpaceCost);
				Debug.DrawLine (grid[i,j].loc + Vector3.right*(nodeSize/2.0f), grid[i,j].loc - Vector3.right*(nodeSize/2.0f), scaledGrey, 3);

			}
		}
	}

	public Vector3 getGridCoords(Vector3 location) {
		float newx = location.x + worldWidth / 2.0f;
		float newz = -location.z + worldHeight / 2.0f;

		int i = (int)(newx / nodeSize);
		int j = (int)(newz / nodeSize);

		if (i < 0)
			i = 0;
		if (i >= gridWidth)
			i = gridWidth - 1;
		if (j < 0)
			j = 0;
		if (j >= gridHeight)
			j = gridHeight - 1;

		return new Vector3(i, 0.0f, j);
	}
}
