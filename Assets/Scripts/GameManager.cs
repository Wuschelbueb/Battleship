using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum InputKeys
{
	Left, Right, Fire
}


public class GameManager : MonoBehaviour {

	[SerializeField] GameObject MediumShip;

	// centerOfMap and spawnRadius (defined by the distance from center to point of circle)
	// are to determine the spawn location of the ships. 
	// all factions have their ships together, on the circle defined by those parameters, 
	// of equal distance. 
	[SerializeField] Transform centerOfMap;
	[SerializeField] Transform pointOnSpawnCircle;


	public static GameManager Instance;
	public List<Ship> Ships = new List<Ship> ();

	private bool _isPaused;
	public bool isPaused { 
		get { return _isPaused; } 
		set { 
			_isPaused = value;
			Time.timeScale = value ? 0 : 1; // freezes / unfreezes the physics engine.
		} 
	}

	private float spawnRadius;
	bool isInMenu = true;

	/* ----- METHODS ----- */

	/* Public */ 

	public void AddShip (int playerIndex) {
		GameObject newObj = GameObject.Instantiate (MediumShip);
		newObj.name = Menu.Instance.playerUIList [playerIndex].playerName;
		if (newObj.name == "") newObj.name = "Player_" + playerIndex;

		Ship ship = newObj.GetComponentInChildren<Ship> ();
		ship.factionNumber = Menu.Instance.playerUIList [playerIndex].FactionNumber;
		Ships.Add (ship);

		UpdateAllShips ();
	}



	public void UpdateAllShips () {
		for (int i = 0; i < Ships.Count; i++) {
			UpdateShip (i);
		}
	}

	// TODO, we never update a single ship, so i makes sense to merge this function into the above.
	public void UpdateShip (int playerIndex) {
		Vector3[] locations = getSpawnLocations ();

		Debug.Assert (locations.Length == Ships.Count);

		Ships [playerIndex].gameObject.transform.position = locations [playerIndex];

	}

	public void RemoveShip (int playerIndex) {
		GameObject obj = Ships [playerIndex].gameObject;
		Ships.RemoveAt (playerIndex);
		GameObject.Destroy (obj);
	}
		
	/* Private */

	private Vector3[] getSpawnLocations () {
		// TODO: this function gives garbage so far.

		int[] shipsPerFaction = new int[Factions.Count()];
		foreach (var ship in Ships) shipsPerFaction [ship.factionNumber]++;

		int[] shipsPerExistingFaction = shipsPerFaction.Where (i => i != 0).ToArray ();
		int n = shipsPerExistingFaction.Count();

		Vector3[] locs = new Vector3[n];

		for (int i = 0; i < n; i++) {
			locs [i] = centerOfMap.position + spawnRadius * new Vector3 (Mathf.Cos (i * 360 / n), 0f, Mathf.Sin (i * 360 / n));
		}

		List<Vector3> locsWithMultiplicities = new List<Vector3> ();

		for (int i = 0; i < n; i++) {
			locsWithMultiplicities.AddRange (Enumerable.Repeat (locs [i], shipsPerExistingFaction [i]));
		}

		return locsWithMultiplicities.ToArray();
	}
		

	void Awake () {
		Debug.Assert (Instance == null);
		Instance = this;
		isPaused = true;
		spawnRadius = Vector3.Distance (pointOnSpawnCircle.position, centerOfMap.position);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Escape)) {
			isPaused = !isPaused;
		}
	}


}
