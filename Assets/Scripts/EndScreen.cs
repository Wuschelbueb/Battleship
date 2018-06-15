#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;


public class EndScreen : MonoBehaviour {

	[SerializeField] Button RematchButton;
	[SerializeField] Text WinnerTitle;
	[SerializeField] Image WinnerFlagDisplay;
	[SerializeField] Text StatDisplay;

	public static EndScreen Instance;

	public void Show (int WinnerFaction, string[] WinningPlayers) {

		WinnerFlagDisplay.sprite = Factions.List [WinnerFaction].UIFlag;
		string winningplayerNames = String.Join (" & Captain ", WinningPlayers);
		WinnerTitle.text = "Captain " + winningplayerNames + " Of " + Factions.List[WinnerFaction].Name + " Won!";
		gameObject.SetActive (true);
	}

	void Awake () {
		Debug.Assert (Instance == null);
		Instance = this;
		RematchButton.onClick.AddListener (OnClose);
		gameObject.SetActive(false);
	}

	void OnClose () {
		gameObject.SetActive (false);
		GameManager.Instance.PlayerList.ForEach (p => p.DeleteShip ());
        /*foreach (var item in GameManager.Instance.PlayerList) {
			item.Ship.SelfDestory (item);
		}*/
        GameManager.Instance.music.Stop();
		Menu.Instance.Show ();
	}
		
}
