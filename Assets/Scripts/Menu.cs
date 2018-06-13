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

	public IEnumerable<PlayerMenu> playerMenuList { get { return GameManager.Instance.PlayerList.Select (p => p.PlayerMenu); } }


	public void Show() {
		gameObject.SetActive (true);
		foreach (var player in GameManager.Instance.PlayerList) {
			GameManager.Instance.CreateShipForPlayer (player);
			CreateUIForPlayer(player);
		}
	}

	public void CreateUIForPlayer(PlayerData data) {
		Debug.Assert (data != null);
		Debug.Assert (data.PlayerMenu == null);

		PlayerMenu menu = Instantiate (PlayerUIPrefab, transform).GetComponentInChildren<PlayerMenu> ();
		data.PlayerMenu = menu;
		menu.Initialise (data);
		RepositionAllPlayerMenus ();
	}
		
	public void RepositionAllPlayerMenus () {
		int playerMenuWidth = (Screen.width - 4 * BorderBetweenPlayerUIs) / 3; 
		int playerMenuHeight = (Screen.height - 3 * BorderBetweenPlayerUIs) / 2 - SpaceReservedOnTop;

		for (int indexInList = 0; indexInList < GameManager.Instance.PlayerList.Count; indexInList++) {
			if (GameManager.Instance.PlayerList [indexInList].PlayerMenu == null) continue;
			// The game should allow a maximum of six players. Hence their PlayerMenus are
			//   positioned in a (3 cols x 2 rows) grid.
			Debug.Assert(0 <= indexInList && indexInList < 6);
			int x = indexInList % 3; // col index
			int y = indexInList < 3 ? 0 : 1; // row index

			RectTransform playerMenuRect = GameManager.Instance.PlayerList[indexInList].PlayerMenu.rectTransform; 

			playerMenuRect.position = new Vector3 (
				x * (playerMenuWidth + BorderBetweenPlayerUIs) + (playerMenuWidth / 2) + BorderBetweenPlayerUIs,
				y * (playerMenuHeight + BorderBetweenPlayerUIs) + (playerMenuHeight / 2) + BorderBetweenPlayerUIs
			);

			playerMenuRect.sizeDelta = new Vector2 (playerMenuWidth, playerMenuHeight);
		}


	}

	void Awake () {
		Debug.Assert (Instance == null);
		Instance = this;
		AddPlayerButton.onClick.AddListener (() => { GameManager.Instance.CreatePlayer(); });
		EndGameButton.onClick.AddListener (() => { Application.Quit (); });
		StartGameButton.onClick.AddListener (StartGame);
	}

	void StartGame () {
		GameManager.Instance.PlayerList.ForEach (p => p.DeleteUI());
		GameManager.Instance.isPaused = false;
		gameObject.SetActive (false);
	}

}
