#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class GameManager : MonoBehaviour {

	[SerializeField] GameObject MediumShip;

	// centerOfMap and spawnRadius (defined by the distance from center to point of circle)
	// are to determine the spawn location of the ships. 
	// all factions have their ships together, on the circle defined by those parameters, 
	// of equal distance. 
	[SerializeField] Transform centerOfMap;
	[SerializeField] Transform pointOnSpawnCircle;


	public static GameManager Instance;

	public List<PlayerData> playerDataList = new List<PlayerData>();

	public IEnumerable<Ship> Ships { get { return playerDataList.Select (d => d.Ship); } }

	private bool _isPaused;
	public bool isPaused { 
		get { return _isPaused; } 
		set { 
			_isPaused = value;
			Time.timeScale = value ? 0 : 1; // freezes / unfreezes the physics engine.
		} 
	}

	private float spawnRadius;
	/* ----- METHODS ----- */

	/* Public */ 

	public PlayerData CreatePlayer () {
		PlayerData data = new PlayerData ();

		GameObject ShipGameObj = Instantiate (MediumShip);
		data.Ship = ShipGameObj.GetComponentInChildren<Ship> ();

		data.Ship.Initialise (data);

		playerDataList.Add (data);

		data.OnFactionChange += RepositionShips;
		data.OnDelete += RepositionShips;
		return data;
	}

	public void DeletePlayer () {

	}

	public void CreateShipForPlayer (PlayerData player) {
		if (player.Ship != null) {
			player.Ship.Destory ();
		}
		GameObject ShipGameObj = Instantiate (MediumShip);
		player.Ship = ShipGameObj.GetComponentInChildren<Ship> ();
		player.Ship.Initialise (player);
	}

	public void CheckForWinner () {
		int[] shipsPerFaction = new int[Factions.List.Count()];

		foreach (var ship in Ships) {
			if (!ship.isSinking) {
				shipsPerFaction [ship.playerData.FactionCode]++;
			}
		}

		// one or less Teams left!
		int teamsLeft = shipsPerFaction.Count (i => i != 0);
		if (teamsLeft <= 1) {
			if (teamsLeft < 1) {
				Debug.Log ("Draw!!");
			} else {
				
				int winningFaction = 0;
				for (int i = 0; i < Factions.List.Count (); i++) {
					if (shipsPerFaction [i] > 0) {
						winningFaction = i;
						break;
					}
				}

				string[] winners = Ships.Where (s => s.playerData.FactionCode == winningFaction).Select (s => s.playerData.Name).ToArray();

				EndScreen.Instance.Show (winningFaction, winners);

				isPaused = true;
			}
		}

	}
	/*
	public void AddShip (int playerIndex) {
		GameObject newObj = GameObject.Instantiate (MediumShip);
		newObj.name = Menu.Instance.playerMenuList [playerIndex].playerName;
		if (newObj.name == "") newObj.name = "Player_" + playerIndex;

		Ship ship = newObj.GetComponentInChildren<Ship> ();
		ship.factionNumber = Menu.Instance.playerMenuList [playerIndex].FactionNumber;
		Ships.Add (ship);

		ship.SetCompassColor (Factions.Get (ship.factionNumber).color);

		RepositionShips ();
	}
	*/
		
	public void RepositionShips () {
		Vector3[] locations = getSpawnLocations ();
		Debug.Assert (locations.Length == Ships.Count());


		for (int i = 0; i < playerDataList.Count; i++) {
			playerDataList[i].Ship.gameObject.transform.position = locations [i];
		}
	}

	private Vector3[] getSpawnLocations () {

		int[] shipsPerFaction = new int[Factions.List.Count];

		foreach (var ship in Ships) shipsPerFaction [ship.playerData.FactionCode]++;

		int[] shipsPerExistingFaction = shipsPerFaction.Where (i => i != 0).ToArray ();

		int noExistingFactions = shipsPerExistingFaction.Count();

		Vector3[] locations = new Vector3[noExistingFactions];

		for (int i = 0; i < noExistingFactions; i++) {
			locations [i] = centerOfMap.position + spawnRadius * new Vector3 (Mathf.Cos (i * 2 * Mathf.PI / noExistingFactions), 0f, Mathf.Sin (i * 2 * Mathf.PI / noExistingFactions));
		}

		bool[] factionsInGame = Factions.List.Select ((f, i) => shipsPerFaction [i] > 0).ToArray();

		Vector3[] playerPositions = new Vector3[Ships.Count()];

		for (int i = 0; i < Ships.Count(); i++) {
			playerPositions[i] = locations [factionsInGame.Take (playerDataList[i].FactionCode).Where (b => b).Count()];
		}
			
		return playerPositions;
	}
		
	void Awake () {
		Debug.Assert (Instance == null);
		Instance = this;
		isPaused = true;
		spawnRadius = Vector3.Distance (pointOnSpawnCircle.position, centerOfMap.position);
	}

	void Start() {
		Menu.Instance.Show ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Escape)) {
			isPaused = !isPaused;
		}
	}


}
