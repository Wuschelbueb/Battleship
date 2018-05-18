using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour {

    [SerializeField] GameObject GunWaterImpactParticleSystem;

    public GameObject ShipThatFiredThisOne;

	// Use this for initialization
	void Start () {
        //var col = gameObject.AddComponent<MeshCollider>();
        //col.isTrigger = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter (Collider collider) {
        Debug.Log("Collided with " + collider.tag);


        if (collider.tag == "Water")
        {
            GameObject impact = GameObject
                .Instantiate(GunWaterImpactParticleSystem, transform.position, Quaternion.identity);
            GameObject.Destroy(impact, 2f);

            // SelfDestroy
            GameObject.Destroy(gameObject);

        }



    }

}
