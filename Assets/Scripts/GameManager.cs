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
	[SerializeField] float angleBetweenTeammembers;
	[SerializeField] Camera mainCamera;


	public static GameManager Instance;

	public List<PlayerData> PlayerList = new List<PlayerData>();

	public int N { get { return PlayerList.Count; } }

	public IEnumerable<Ship> Ships { get { return PlayerList.Select (d => d.Ship); } }

	public float spawnRadius = 200;
	private float areaOfTrapezoid = -1;

	private bool _isPaused;
	public bool isPaused { 
		get { return _isPaused; } 
		set { 
			_isPaused = value;
			Time.timeScale = value ? 0 : 1; // freezes / unfreezes the physics engine.
		} 
	}




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
	
		int[] playerPerFaction = new int[Factions.List.Count];

		//foreach (var p in PlayerList) playerPerFaction [p.FactionCode]++;

		PlayerList.ForEach (p => playerPerFaction [p.FactionCode]++);

		int[] playerPerExistingFaction = playerPerFaction.Where (i => i != 0).ToArray ();

		int numberExistingFactions = playerPerExistingFaction.Count();

		float[] startingAngles = new float[numberExistingFactions];
		for (int i = 0; i < numberExistingFactions; i++) {
			startingAngles [i] = 2 * Mathf.PI * i / numberExistingFactions;
		}
			
		bool[] factionsInGame = Factions.List.Select ((f, i) => playerPerFaction [i] > 0).ToArray();

		float[] playerAngles = new float[PlayerList.Count];

		for (int i = 0; i < PlayerList.Count; i++) {
			playerAngles[i] = startingAngles [factionsInGame.Take (PlayerList[i].FactionCode).Where (b => b).Count()];
		}

		for (int i = 1; i < PlayerList.Count; i++) {
			while (playerAngles.Take (i - 1).Count (a => Math.Abs(a - playerAngles [i]) < angleBetweenTeammembers / 2 * Mathf.Deg2Rad) > 0) {
				playerAngles [i] += angleBetweenTeammembers * Mathf.Deg2Rad;
			}
		}

		Vector3[] playerPositions = playerAngles.Select (a => centerOfMap.position + spawnRadius * new Vector3 (Mathf.Cos(a), 0f, Mathf.Sin (a))).ToArray ();

		Debug.Assert (playerPositions.Length == PlayerList.Count);

		for (int i = 0; i < PlayerList.Count; i++) {
			if (PlayerList [i].Ship == null) continue;

			PlayerList [i].Ship.gameObject.transform.position = playerPositions [i];
			//Debug.Log ("Position player of " + PlayerList[i].FactionCode + " to " + playerPositions [i]);

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
		//CreatePlayFieldCollider ();
	}

	void Start() {
		Menu.Instance.Show ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Escape)) {
			isPaused = !isPaused;
		}
		if (Input.GetMouseButtonUp (0)) {
			Ray ray = mainCamera.ScreenPointToRay(new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0f));
			float LenghtToHitOcean = -ray.origin.y / ray.direction.y;
			Vector3 hitpoint = ray.origin + LenghtToHitOcean * ray.direction;
			InFieldChecker.Check (hitpoint);
		}
	}

	#region On-Screen-Checker

	/// <summary>
	/// A nested class with the purpose of checking if a position is on-Screen.
	///  
	/// Based on the following idea:
	///  let a,b,c,d be the trapezoid screen area. p be the point on screen to check, 
	///  then Area (a,b,p) + Area(b,c,p) + ... + Area(d,a,p) <= Area(Trapezoid) iff 
	///  p in Trapezoid.
	/// </summary>
	public static class InFieldChecker {
		const float epsilon = 20f;
		static Vector3[] corners = null;
		static float trapezoidArea;

		static void CalcCorners () {
			corners = new Vector3[] {
				GetHitPoint(0f, 0f),
				GetHitPoint(Screen.width, 0f),
				GetHitPoint(Screen.width, Screen.height),
				GetHitPoint(0f, Screen.height)
			};
			trapezoidArea = TrigArea (corners [0], corners [1], corners [2]) + TrigArea (corners [0], corners [2], corners [3]);
		}

		static Vector3 GetHitPoint (float x, float y) {
			Ray ray = GameManager.Instance.mainCamera.ScreenPointToRay (new Vector3 (x, y, 0f));
			float LenghtToHitOcean = -ray.origin.y / ray.direction.y;
			return ray.origin + LenghtToHitOcean * ray.direction;
		}

		static float TrigArea (Vector3 a, Vector3 b, Vector3 c) {
			Vector3 x = b - a, y = c - a;
			return Math.Abs(Vector3.Cross (x, y).magnitude / 2);
		}
			
		static public bool Check (Vector3 position) {
			if (corners == null) CalcCorners ();

			float newArea = 
				TrigArea (position, corners [0], corners [1])
				+ TrigArea (position, corners [1], corners [2])
				+ TrigArea (position, corners [2], corners [3])
				+ TrigArea (position, corners [3], corners [0]);


			trapezoidArea = TrigArea (corners [0], corners [1], corners [2]) + TrigArea (corners [0], corners [2], corners [3]);

			Debug.Log ("TrapezoidArea = " + trapezoidArea);
			Debug.Log ("NewArea = " + newArea);
			Debug.Log ("Result = " + (Mathf.Abs (newArea - trapezoidArea) <= epsilon));
			return Mathf.Abs (newArea - trapezoidArea) <= epsilon;
		}
	}

	#endregion

	// This is bullshit as shape does not register as convex...
	GameObject CreatePlayFieldCollider () {

		Func<float, float, Vector3> GetHitPoint = (x, y) => {
			Ray ray = mainCamera.ScreenPointToRay (new Vector3 (x, y, 0f));
			float LenghtToHitOcean = -ray.origin.y / ray.direction.y;
			return ray.origin + LenghtToHitOcean * ray.direction;
		};

		Vector3[] corners = new Vector3[] {
			GetHitPoint(0f, 0f),
			GetHitPoint(Screen.width, 0f),
			GetHitPoint(Screen.width, Screen.height),
			GetHitPoint(0f, Screen.height)
		};

		MeshGenerator mg = new MeshGenerator ();

		mg.AddRectangle (corners.Select(v => v + new Vector3(0f, 0.5f, 0f)).ToArray());

		GameObject newGO = new GameObject ("PlayFieldCollider");
		MeshFilter mf = newGO.AddComponent<MeshFilter> ();
		//MeshRenderer mr = newGO.AddComponent<MeshRenderer> ();

		mf.mesh = mg.GetMesh ();
		newGO.tag = "PlayField";

		MeshCollider mc = newGO.AddComponent<MeshCollider> ();

		mc.sharedMesh = mf.mesh;
		mc.convex = true;
		mc.inflateMesh = true;
		mc.isTrigger = true;

		return newGO;
	}

	void getCorners () {
		//Camera mainCamera = GetComponentInParent<Camera> ();
		//mainCamera.ScreenToWorldPoint ();
		if (Input.GetMouseButtonUp (0)) {
			//Vector3 position = mainCamera.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0f));
			//Debug.Log (position);

			Ray ray = mainCamera.ScreenPointToRay(new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0f));
			float LenghtToHitOcean = -ray.origin.y / ray.direction.y;
			Vector3 hitpoint = ray.origin + LenghtToHitOcean * ray.direction;
			Debug.Log (hitpoint);
		}
	}

}
