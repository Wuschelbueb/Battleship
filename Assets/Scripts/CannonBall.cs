#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour {

    [SerializeField] GameObject GunWaterImpactParticleSystem;

    void OnTriggerEnter (Collider collider) {
		if (GameManager.Instance.isPaused)
			return;
		
		if (collider.tag == "Ship")
        {
            if (collider.transform.parent.parent.gameObject.GetComponent<Ship>() != null)
            {
                collider.transform.parent.parent.gameObject.GetComponent<Ship>().TakeDamage(1); 

                // Self destroy
                GameObject.Destroy(gameObject);
            }
        }


        if (collider.tag == "Water")
        {
            GameObject impact = GameObject
                .Instantiate(GunWaterImpactParticleSystem, transform.position, Quaternion.identity);
            GameObject.Destroy(impact, 2f);

            // Self destroy
            GameObject.Destroy(gameObject);

        }
    }
}
