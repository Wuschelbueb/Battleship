#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable] 
public class Faction {
	public string Name;
	public Sprite UIFlag;
	public Color color;
}

/// <summary>
/// Factions. This is a gameobject that wrapps a List of Faction (so that Factions are editable form the unity editor.
/// </summary>
public class Factions : MonoBehaviour {

	[SerializeField] List<Faction> factions;

	public static List<Faction> List;

	private void Awake () { List = factions; }
}
