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

	public List<PlayerMenu> playerUIList;

	public void DestroyMe (PlayerMenu playerUi) {
		Debug.Assert(playerUIList.Contains(playerUi));
		int index = playerUIList.IndexOf (playerUi);
		playerUIList.RemoveAt (index);
		for (int i = index; i < playerUIList.Count; i++) {
			PositionPlayerMenu (i);
		}
		GameManager.Instance.RemoveShip (index);
	}


	void PositionPlayerMenu (int indexInList) {
		// The game should allow a maximum of six players. Hence their PlayerMenus are
		//   positioned in a (3 cols x 2 rows) grid.
		Debug.Assert(0 <= indexInList && indexInList < 6);
		int x = indexInList % 3; // col index
		int y = indexInList < 3 ? 0 : 1; // row index
		int playerMenuWidth = (Screen.width - 4 * BorderBetweenPlayerUIs) / 3; 
		int playerMenuHeight = (Screen.height - 3 * BorderBetweenPlayerUIs) / 2 - SpaceReservedOnTop;

		RectTransform playerMenuRect = playerUIList [indexInList].rectTransform;

		playerMenuRect.position = new Vector3 (
			x * (playerMenuWidth + BorderBetweenPlayerUIs) + (playerMenuWidth / 2) + BorderBetweenPlayerUIs,
			y * (playerMenuHeight + BorderBetweenPlayerUIs) + (playerMenuHeight / 2) + BorderBetweenPlayerUIs
		);

		playerMenuRect.sizeDelta = new Vector2 (playerMenuWidth, playerMenuHeight);

	}



	void Awake () {
		Debug.Assert (Instance == null);
		Instance = this;

		AddPlayerButton.onClick.AddListener (() => {
			// TODO, make those separate funcs.
			if (playerUIList.Count <= 6) {
				GameObject newObj = GameObject.Instantiate(PlayerUIPrefab, transform);
				playerUIList.Add(newObj.GetComponentInChildren<PlayerMenu>());

				int newIndex = playerUIList.Count - 1;
				PositionPlayerMenu(newIndex);
				GameManager.Instance.AddShip(newIndex);
				//Menu.Instance.playerUIList[newIndex].OnFactionButtonClick();
			}
		});

		EndGameButton.onClick.AddListener (() => {
			Application.Quit ();
		});

		StartGameButton.onClick.AddListener (() => {
			GameManager.Instance.isPaused = false;
			gameObject.SetActive(false);
		});

	}
}
