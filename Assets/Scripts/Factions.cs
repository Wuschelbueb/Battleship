using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable] public struct Faction {
	public string Name;
	public Sprite UIFlag;
	public Color color;
}

/// <summary>
/// Factions. This is a gameobject that wrapps a List of Faction (so that Factions are editable form the unity editor.
/// </summary>
public class Factions : MonoBehaviour {

	[SerializeField] List<Faction> factions;

	private static List<Faction> _factions;
	private void Awake () { _factions = factions; }

	public static int Count () { return _factions.Count; }

	public static Faction Get (int index) { return _factions[index]; }
}
