#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class PlayerMenu : MonoBehaviour {

	public Button FactionButton, FireButton, LeftButton, RightButton, CloseButton;
	public Text FactionText;
	public InputField PlayerNameInput;

	public RectTransform rectTransform { get { return GetComponentInChildren<RectTransform>(); } }

	private bool waitingForFire, waitingForLeft, waitingForRight;

	public PlayerData dataReference;

	public void Initialise (PlayerData data) {
		dataReference = data;
		PlayerNameInput.text = data.Name;

		CloseButton.onClick.AddListener (() => { dataReference.DeleteAll (); });
		FireButton.onClick.AddListener (() => { waitingForFire = true; });
		LeftButton.onClick.AddListener (() => { waitingForLeft = true; });
		RightButton.onClick.AddListener (() => { waitingForRight = true; });
		FactionButton.onClick.AddListener (IncrementFaction);

		OnKeyChange ();
		data.UpdateFaction();
	}
		
	private void IncrementFaction () {
		dataReference.FactionCode = (dataReference.FactionCode + 1) % Factions.List.Count;
		dataReference.UpdateFaction ();
	}

	private void Update () {
		if ((waitingForFire || waitingForLeft || waitingForRight) && Input.anyKey) 
		{
			KeyCode key = ((IEnumerable<KeyCode>)Enum.GetValues (typeof(KeyCode))).First (k => Input.GetKey(k));

			if (waitingForFire) dataReference.FireKey = key;
			else if (waitingForLeft) dataReference.LeftKey = key;
			else if (waitingForRight) dataReference.RightKey = key;

			waitingForFire = false;
			waitingForLeft = false;
			waitingForRight = false;

			OnKeyChange ();
		}
	}

	public void OnKeyChange () {
		FireButton.GetComponentInChildren<Text> ().text = "Press " + dataReference.FireKey + " to Fire (Change)";
		LeftButton.GetComponentInChildren<Text> ().text = "Press " + dataReference.LeftKey + " to turn Left (Change)";
		RightButton.GetComponentInChildren<Text> ().text = "Press " + dataReference.RightKey + " to turn Right (Change)";
	}
}
