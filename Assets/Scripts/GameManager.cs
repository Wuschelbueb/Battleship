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

	public List<PlayerData> PlayerList = new List<PlayerData>();

	public int N { get { return PlayerList.Count; } }

	public IEnumerable<Ship> Ships { get { return PlayerList.Select (d => d.Ship); } }

	private bool _isPaused;
	public bool isPaused { 
		get { return _isPaused; } 
		set { 
			_isPaused = value;
			Time.timeScale = value ? 0 : 1; // freezes / unfreezes the physics engine.
		} 
	}

	private float spawnRadius;

	public void CreatePlayer () {
		if (PlayerList.Count >= 6) {
			Debug.Log ("tried to add more than 6 players. not allowed");
			return;
		}

		PlayerData data = new PlayerData ();
		PlayerList.Add (data);

		CreateShipForPlayer (data);
		Menu.Instance.CreateUIForPlayer (data);
		data.UpdateFaction ();
	}
		

	public void CreateShipForPlayer (PlayerData player) {
		if (player.Ship != null) {
			player.DeleteShip ();
		}
		GameObject ShipGameObj = Instantiate (MediumShip);
		player.Ship = ShipGameObj.GetComponentInChildren<Ship> ();
		player.Ship.Initialise (player);
		GameManager.Instance.RepositionShips ();
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
		
	public void RepositionShips () {
		// TODO this should be written almost ship-agnostic only taking players into account.
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
			playerPositions[i] = locations [factionsInGame.Take (PlayerList[i].FactionCode).Where (b => b).Count()];
		}

		Debug.Assert (playerPositions.Length == Ships.Count());

		for (int i = 0; i < PlayerList.Count; i++) {
			PlayerList [i].Ship.gameObject.transform.position = playerPositions [i];

			Vector3 radialDirection = centerOfMap.position- PlayerList [i].Ship.transform.position ;

			Quaternion shipRotation = Quaternion.LookRotation (radialDirection);

			PlayerList [i].Ship.transform.rotation = shipRotation;

			PlayerList [i].Ship.TargetDirectionAngle = Vector3.SignedAngle (Vector3.forward, radialDirection, Vector3.up);
		}


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
