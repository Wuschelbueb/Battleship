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

	[SerializeField] Button FactionButton, FireButton, LeftButton, RightButton, CloseButton;
	[SerializeField] Text FactionText;
	[SerializeField] InputField PlayerNameInput;

	public RectTransform rectTransform { get { return GetComponentInChildren<RectTransform>(); } }

	private bool waitingForFire, waitingForLeft, waitingForRight;

	public int myIndex { get { return Menu.Instance.playerMenuList.IndexOf (this); } }
	//public Ship myShip { get { return GameManager.Instance.Ships [myIndex]; } }

	PlayerData dataReference;


	/* ----- METHODS ------ */


	/*  Public  */

	public void Initialise (PlayerData data) {
		dataReference = data;
		PlayerNameInput.text = data.Name;

		data.OnKeyChange += OnKeyChange;
		data.OnFactionChange += OnFactionChange;

		CloseButton.onClick.AddListener (Destroy);
		FireButton.onClick.AddListener (() => { waitingForFire = true; });
		LeftButton.onClick.AddListener (() => { waitingForLeft = true; });
		RightButton.onClick.AddListener (() => { waitingForRight = true; });
		FactionButton.onClick.AddListener (IncrementFaction);

		OnKeyChange ();
		OnFactionChange ();
	}

	public void Destroy () {
		dataReference.OnKeyChange -= OnKeyChange;
		dataReference.OnFactionChange -= OnFactionChange;

		dataReference.Delete ();
		Menu.Instance.DestroyMe (this);
		GameObject.Destroy (gameObject);
	}
		
	public void OnKeyChange () {
		FireButton.GetComponentInChildren<Text> ().text = "Press " + dataReference.FireKey + " to Fire (Change)";
		LeftButton.GetComponentInChildren<Text> ().text = "Press " + dataReference.LeftKey + " to turn Left (Change)";
		RightButton.GetComponentInChildren<Text> ().text = "Press " + dataReference.RightKey + " to turn Right (Change)";
	}

	public void OnFactionChange () {
		FactionText.text = dataReference.getFaction ().Name;
		FactionButton.image.sprite = dataReference.getFaction ().UIFlag;
	}
		
	/*  Private  */

	private void IncrementFaction () {
		dataReference.setFactionCode ((dataReference.FactionCode + 1) % Factions.List.Count);
	}

    
	// Update is called once per frame
	private void Update () {
		if ((waitingForFire || waitingForLeft || waitingForRight) && Input.anyKey) 
		{
			KeyCode key = ((IEnumerable<KeyCode>)Enum.GetValues (typeof(KeyCode))).First (k => Input.GetKey(k));

			KeyCode left = dataReference.LeftKey, right = dataReference.RightKey, fire = dataReference.FireKey;

			if (waitingForFire) fire = key;
			else if (waitingForLeft) left = key;
			else if (waitingForRight) right = key;

			waitingForFire = false;
			waitingForLeft = false;
			waitingForRight = false;

			dataReference.setKeyCodes (fire, left, right);
		}
	}
}
