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
	public KeyCode FireKey;
	public KeyCode LeftKey;
	public KeyCode RightKey;

	public Ship Ship;

	/* rest */
	public Action OnKeyChange;
	public Action OnFactionChange;
	public Action OnDelete;

	public Faction getFaction () {
		return Factions.List [FactionCode];
	}

	public void setFactionCode(int code) {
		FactionCode = code;
		if (OnFactionChange.GetInvocationList().Count() > 0) OnFactionChange.Invoke();
	}

	public void setKeyCodes (KeyCode Fire, KeyCode Left, KeyCode Right) {
		FireKey = Fire; LeftKey = Left; RightKey = Right;
		if (OnKeyChange.GetInvocationList ().Count () > 0) OnKeyChange.Invoke();
	}

	public void Delete() {
		if (OnDelete.GetInvocationList ().Count () > 0)	OnDelete.Invoke ();
	}

}