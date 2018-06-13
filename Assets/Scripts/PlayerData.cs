#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class PlayerData {

	/* Serializable stuff */
	public int FactionCode;
	public string Name;
	public KeyCode FireKey, LeftKey, RightKey;

	public int ShotsFired, DamageDealt;

	/* References */
	public Ship Ship;
	public PlayerMenu PlayerMenu;


	public void DeleteShip () {
		if (Ship == null) {
			Debug.Log ("Warning: tried to delete a ship, but it's null");
			return;
		}
		GameObject.Destroy (Ship.gameObject);
		Ship = null;
		//GameManager.Instance.RepositionShips ();
		// somewhat counterintuitive that this is not necessary... TODO think!
	}

	public void DeleteUI () {
		if (PlayerMenu == null) {
			Debug.Log ("Warning: tried to delete player-menu, but it's null");
			return;
		}
		GameObject.Destroy (PlayerMenu.gameObject);
		PlayerMenu = null;
		Menu.Instance.RepositionAllPlayerMenus ();
	}

	public void DeleteAll () {
		GameManager.Instance.PlayerList.Remove (this);
		DeleteShip ();
		DeleteUI ();
	}

	public void UpdateFaction () {
		if (PlayerMenu != null) {
			PlayerMenu.FactionText.text = Factions.List [FactionCode].Name;
			PlayerMenu.FactionButton.image.sprite = Factions.List [FactionCode].UIFlag;
		}
		if (Ship != null) {
			Ship.SetCompassColor (Factions.List [FactionCode].color);
		}
		GameManager.Instance.RepositionShips ();
	}
}