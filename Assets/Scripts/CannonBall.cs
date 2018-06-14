using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour {

    [SerializeField] GameObject GunWaterImpactParticleSystem;

    void OnTriggerEnter (Collider collider) {

        if (collider.tag != "Water")
        {

            Debug.Log("Collided with " + collider.tag);
        }

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
        if (collider.tag == "Untagged")
        {
            GameObject.Destroy(gameObject);
        }



    }

}
