#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

/// <summary>
/// The menu class is models the Menuscreen, that is shown when the game starts and 
/// between games.
/// It allows starting games, quiting the game and adding players.
/// Each player should have his/her own PlayerSelectionUI. This is positioned by the Menu.
/// The menu is not active during the actual game.
/// </summary>
public class Menu : MonoBehaviour {

	[SerializeField] int BorderBetweenPlayerUIs = 20;
	[SerializeField] int SpaceReservedOnTop = 200;

	[SerializeField] GameObject PlayerUIPrefab;
	[SerializeField] Button AddPlayerButton;
	[SerializeField] Button StartGameButton;
	[SerializeField] Button EndGameButton;

	public static Menu Instance;

	public List<PlayerMenu> playerMenuList;

	public void Show() {
		playerMenuList = new List<PlayerMenu>();
		gameObject.SetActive (true);
		foreach (var player in GameManager.Instance.playerDataList) {
			GameManager.Instance.CreateShipForPlayer (player);
			AddPlayerMenu (player);
		}
	}

	public void AddPlayerMenu (PlayerData data) {
		Debug.Assert (data != null);
		PlayerMenu menu = Instantiate (PlayerUIPrefab, transform).GetComponentInChildren<PlayerMenu> ();
		playerMenuList.Add (menu);
		menu.Initialise (data);
		PositionPlayerMenu (playerMenuList.IndexOf (menu));
		data.OnFactionChange ();
	}

	public void DestroyMe (PlayerMenu playerMenu) {
		Debug.Assert(playerMenuList.Contains(playerMenu));

		int index = playerMenuList.IndexOf (playerMenu);
		playerMenuList.RemoveAt (index);
		for (int i = index; i < playerMenuList.Count; i++) {
			PositionPlayerMenu (i);
		}
	}
		
	void PositionPlayerMenu (int indexInList) {
		// The game should allow a maximum of six players. Hence their PlayerMenus are
		//   positioned in a (3 cols x 2 rows) grid.
		Debug.Assert(0 <= indexInList && indexInList < 6);
		int x = indexInList % 3; // col index
		int y = indexInList < 3 ? 0 : 1; // row index
		int playerMenuWidth = (Screen.width - 4 * BorderBetweenPlayerUIs) / 3; 
		int playerMenuHeight = (Screen.height - 3 * BorderBetweenPlayerUIs) / 2 - SpaceReservedOnTop;

		//PlayerMenu playerMenu = playerMenuList.First (); // playerMenuList [indexInList];
		RectTransform playerMenuRect = playerMenuList[indexInList].rectTransform; 

		playerMenuRect.position = new Vector3 (
			x * (playerMenuWidth + BorderBetweenPlayerUIs) + (playerMenuWidth / 2) + BorderBetweenPlayerUIs,
			y * (playerMenuHeight + BorderBetweenPlayerUIs) + (playerMenuHeight / 2) + BorderBetweenPlayerUIs
		);

		playerMenuRect.sizeDelta = new Vector2 (playerMenuWidth, playerMenuHeight);

	}
		
	void AddPlayer() {
		if (playerMenuList.Count >= 6) return;
		PlayerData data = GameManager.Instance.CreatePlayer ();
		AddPlayerMenu (data);
	}

	void Awake () {
		Debug.Assert (Instance == null);
		Instance = this;
		AddPlayerButton.onClick.AddListener (AddPlayer);
		EndGameButton.onClick.AddListener (() => { Application.Quit (); });
		StartGameButton.onClick.AddListener (Close);
	}

	void Close () {
		foreach (var menu in playerMenuList) {
			//menu.Destroy ();
		}
		GameManager.Instance.isPaused = false;
		gameObject.SetActive (false);
	}

}
