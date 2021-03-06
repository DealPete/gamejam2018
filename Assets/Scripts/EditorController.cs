﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

enum Mode {
	Selecting,
	PlacingJunction
}

public class EditorController : MonoBehaviour {
	[SerializeField]
	Prefabs prefabs;

	public GameLevel gameLevel;

	private const float SELECTION_RADIUS_PIXELS = 20.0f;
	private const float WIRE_SELECTION_DISTANCE = 10.0f;
	private Junction selectedJunction = null;
	private Mode mode = Mode.Selecting;
	private WireType SelectedWireType;

	[SerializeField]
	EventSystem eventSystem;

	void Start () {
		if (File.Exists("level.tmp")) {
			gameLevel = Serializer.LoadTemporaryLevel(prefabs);
		} else {
			gameLevel = new GameLevel(prefabs);
		}
	}
	
	void Update () {
		Vector3 position = getWorldPosition (Input.mousePosition);
		if (eventSystem.currentSelectedGameObject != null) {
			return;
		}
		switch (mode) {
			case Mode.Selecting:
				if (Input.GetButtonDown("Fire1")) {
					selectedJunction = null;
					foreach (Junction junction in gameLevel.Junctions) {
						if (nearby(getScreenPosition(junction.transform.position),
							Input.mousePosition)) {
							selectedJunction = gameLevel.addJunction(position);
							gameLevel.hookup(junction, selectedJunction, SelectedWireType);
							mode = Mode.PlacingJunction;
							break;
						}
					} 
					if (selectedJunction == null) {
						Wire wire = getWireUnderCursor();
						if (wire == null) {
							Junction startNode = gameLevel.addJunction(position);
							Junction endNode = gameLevel.addJunction(position);
							gameLevel.hookup(startNode, endNode, SelectedWireType);
							selectedJunction = endNode;
							mode = Mode.PlacingJunction;
						} else {
							gameLevel.remove(wire);
						}
					}
				}
				if (Input.GetButtonDown("Fire2")) {
					foreach (Junction junction in gameLevel.Junctions) {
						if (nearby(getScreenPosition(junction.transform.position),
							Input.mousePosition)) {
							selectedJunction = junction;
							mode = Mode.PlacingJunction;
						}
					} 
				}

				break;
			
			case Mode.PlacingJunction:
				selectedJunction.transform.position = position;
				foreach (Wire wire in selectedJunction.Wires) {
					wire.refreshPosition();
				}
				if (Input.GetButtonDown("Fire1")) {
					Junction combiningJunction = null;
					foreach (Junction junction in gameLevel.Junctions) {
						if (junction != selectedJunction &&
							nearbyWorld(position, junction.transform.position)) {
							combiningJunction = junction;
							break;
						}
					}
					if (combiningJunction != null) {
						foreach (Wire wire in selectedJunction.Wires) {
							if (wire.startNode == selectedJunction) {
								wire.startNode = combiningJunction;
							} else {
								wire.endNode = combiningJunction;
							}
							combiningJunction.addWire(wire);
						}
						gameLevel.remove(selectedJunction);
						removeRedundantWiresFrom(combiningJunction);
					}
					mode = Mode.Selecting;
				}
				break;
		}
	}

	public void SaveTemporaryLevel() {
		Serializer.SaveTemporaryLevel(gameLevel);
	}

	public void clearLevel() {
		gameLevel.selfDestruct();
		gameLevel = new GameLevel(prefabs);
	}

	void removeRedundantWiresFrom(Junction junction) {
		bool removedWire;
		do {
			List<Junction> connectedNodes = new List<Junction>();
			connectedNodes.Add(junction);
			removedWire = false;
			foreach (Wire wire in junction.Wires) {
				Junction otherNode;
				if (wire.startNode == junction)
					otherNode = wire.endNode;
				else
					otherNode = wire.startNode;
				if (connectedNodes.Contains(otherNode)) {
					gameLevel.remove(wire);
					removedWire = true;
					break;
				} else {
					connectedNodes.Add(otherNode);
				}
			}
		} while (removedWire);
	}

	Wire getWireUnderCursor() {
		Vector3 cursorPosition = Input.mousePosition;
		Wire closestWire = null;
		float closestWireDistance = float.PositiveInfinity;

		foreach (Wire wire in gameLevel.Wires) {
			Vector3 start = getScreenPosition(wire.startNode.transform.position);
			Vector3 end = getScreenPosition(wire.endNode.transform.position);
			Vector3 normal = end - start;
			Vector3 projectee = cursorPosition - start;
			Vector3 projection = Vector3.Project(projectee, normal);
			Vector3 drop = projection - projectee;
			if (drop.magnitude < closestWireDistance) {
				float projDotNormal = Vector3.Dot(projection, normal);
				if (projDotNormal > 0 &&
					projDotNormal < normal.magnitude * normal.magnitude) {
					closestWire = wire;
					closestWireDistance = drop.magnitude;
				}
			}
		}

		if (closestWireDistance < WIRE_SELECTION_DISTANCE) {
			return closestWire;
		}

		return null;
	}

	public void SelectWireType(WireType wireType) {
		this.SelectedWireType = wireType;
	}

	bool nearbyWorld(Vector3 vec1, Vector3 vec2) {
		return nearby(getScreenPosition(vec1), getScreenPosition(vec2));
	}

	bool nearby(Vector3 vec1, Vector3 vec2) {
		float distance = Vector3.Distance(vec1, vec2);
		return distance <= SELECTION_RADIUS_PIXELS;
	}

	Vector3 getScreenPosition(Vector3 worldPosition) {
		Camera camera = Camera.main;
		Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);
		return new Vector3(screenPosition.x, screenPosition.y, 0);
	}

	Vector3 getWorldPosition(Vector3 mousePosition) {
        Camera camera = Camera.main;
        Vector3 worldPosition = camera.ScreenToWorldPoint(mousePosition);
		return new Vector3(worldPosition.x, worldPosition.y, 0);
	}
}
