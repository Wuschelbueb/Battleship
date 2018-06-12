﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;

	[SerializeField] GameObject MediumShip;

	private bool _isPaused;
	public bool isPaused { 
		get { return _isPaused; } 
		set { 
			if (value) {
				Debug.Log ("Pausing");
				_isPaused = true;
				Time.timeScale = 0;
			} else {
				Debug.Log ("Unpausing");
				_isPaused = false;
				Time.timeScale = 1;
			}
		} 
	}

	bool isInMenu = true;


	void Awake () {
		Debug.Assert (Instance == null);
		Instance = this;
		isPaused = true;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Escape)) {
			isPaused = !isPaused;
		}
	}


}