using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour {

    [SerializeField] GameObject GunWaterImpactParticleSystem;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /*
    void OnTriggerEnter (Collider collider) {

        Debug.Log(collider.tag);

        Debug.Log("Entry detected: " + collider.gameObject.name);

        GameObject impact = GameObject.Instantiate(GunWaterImpactParticleSystem, collider.transform.position, Quaternion.identity);

        GameObject.Destroy(impact, 2f);

        GameObject.Destroy(collider.gameObject);

    }
    */



}
