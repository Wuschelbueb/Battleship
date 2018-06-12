using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

	[SerializeField] GameObject PlayerUIPrefab;
	[SerializeField] Button AddPlayerButton;
	[SerializeField] Button StartGame;
	[SerializeField] Button EndGame;

	public List<PlayerSelectionUI> playerScripts;
	public List<Ship> playersShips;

	public void DestroyMe (PlayerSelectionUI playerUi) {

	}

	public static Menu Instance;


	void Awake () {
		Debug.Assert (Instance == null);
		Instance = this;

		AddPlayerButton.onClick.AddListener (() => {
			if (playerScripts.Count <= 6) {
				GameObject newObj = GameObject.Instantiate(PlayerUIPrefab, transform);
				playerScripts.Add(newObj.GetComponentInChildren<PlayerSelectionUI>());
			}
		});

		EndGame.onClick.AddListener (() => {
			Application.Quit ();
		});

		StartGame.onClick.AddListener (() => {
			
		});

	}

	void UpdateShip () {

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
