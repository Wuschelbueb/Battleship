using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ship : MonoBehaviour {

    [SerializeField] KeyCode Left, Right;

    [SerializeField] float Speed = 1f;

    [SerializeField] float ShipTurnSpeed = 50f;

    [SerializeField] float InputTurnSpeed = 200f;

    [SerializeField] float RadiusOfGizmosCircle = 5f;

    [SerializeField(), Range(-180f, 180f)] float TargetDirectionAngle = 0f;

    private float CurrentDirectionAngle, DeltaAngle;

    private Vector3 TargetDirectionVector { get
        {
            return new Vector3(
                Mathf.Sin(Mathf.Deg2Rad * TargetDirectionAngle), 
                0f, 
                Mathf.Cos(Mathf.Deg2Rad * TargetDirectionAngle));
        }
    }

	// Use this for initialization
	void Start () {

	}

    // Update is called once per frame

    void Update()
    {

        // Treat Movement Input
        if (Input.GetKey(Left) && !Input.GetKey(Right)) {
            TargetDirectionAngle += InputTurnSpeed * Time.deltaTime;
            if (TargetDirectionAngle > 180f)
            {
                TargetDirectionAngle -= 360f;
            }
        }
        if (Input.GetKey(Right) && !Input.GetKey(Left))
        {
            TargetDirectionAngle -= InputTurnSpeed * Time.deltaTime;
            if (TargetDirectionAngle <= -180f)
            {
                TargetDirectionAngle += 360f;
            }
        }



        // Move forward
        this.transform.position = transform.position + transform.forward * Speed * Time.deltaTime;

        // Turn
        CurrentDirectionAngle = Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up);
        DeltaAngle = (TargetDirectionAngle - CurrentDirectionAngle);

        if (DeltaAngle >= 180f)
        {
            DeltaAngle -= Mathf.Abs(180f - DeltaAngle);
            DeltaAngle *= -1;
        }
        else if (DeltaAngle <= -180f)
        {
            DeltaAngle += Mathf.Abs(-180f - DeltaAngle);
            DeltaAngle *= -1;
        }

        transform.Rotate(Vector3.up,
                         Mathf.Clamp(DeltaAngle,
                                     -1 * ShipTurnSpeed * Time.deltaTime,
                                     +1 * ShipTurnSpeed * Time.deltaTime)
                        );

    }



    void OnDrawGizmos () {
        const int CircleN = 40;

        Gizmos.color = Color.black;

        var circle = Enumerable.Range(0, CircleN + 1)
                               .Select(i => new Vector3(Mathf.Sin(2 * Mathf.PI * i / CircleN), 0f, Mathf.Cos(2 * Mathf.PI * i / CircleN)))
                               .Select(v => RadiusOfGizmosCircle * v)
                               .Select(v => v + transform.position)
                               .ToList();
        

        for (int i = 0; i < CircleN; i++)
        {
            Gizmos.DrawLine(circle[i], circle[i+1]);
        }

        Gizmos.DrawLine(transform.position, transform.position);

        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, 1.1f * RadiusOfGizmosCircle * TargetDirectionVector + transform.position);

    }
}
