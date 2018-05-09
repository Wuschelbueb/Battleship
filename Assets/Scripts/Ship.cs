using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 * Todo:
 * - Reload times
 * - Rock 'n' Roll
 * - Collision & Cannonball deletion & Healthbar & Sinking
 * - Smoke & destruction
 * - Compass around it
 */


public class Ship : MonoBehaviour {

    [SerializeField] KeyCode Left, Right;

    [SerializeField] float Speed = 1f;

    [SerializeField] float ShipTurnSpeed = 50f;

    [SerializeField] float InputTurnSpeed = 200f;

    [SerializeField] float RadiusOfGizmosCircle = 5f;

    [SerializeField] float DoublePressThresholdTime = 0.5f;

    [SerializeField(), Range(-180f, 180f)] float TargetDirectionAngle = 0f;

    [SerializeField] List<Transform> LeftGuns, RightGuns;

    [SerializeField] List<AudioClip> GunSounds;

    [SerializeField] GameObject CannonBall;

    [SerializeField] float ReloadTime;

    private List<AudioSource> Audio;

    private float CurrentDirectionAngle, DeltaAngle;

    private float TimeAtLastClick = float.MinValue;

    private System.Random random = new System.Random();

    private GameObject Compass;

    private Vector3 TargetDirectionVector { get
        {
            return new Vector3(
                Mathf.Sin(Mathf.Deg2Rad * TargetDirectionAngle), 
                0f, 
                Mathf.Cos(Mathf.Deg2Rad * TargetDirectionAngle));
        }
    }

    void InitialiseAudioSources () {
        Audio = new List<AudioSource>();
        foreach (var clip in GunSounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.clip = clip;
            Audio.Add(source);
        }
    }

    void Fire (bool LeftSide) {
        
        const float MaxDelay = 2f;

        List<Transform> Guns = LeftSide ? LeftGuns : RightGuns;
        Vector3 Direction = LeftSide ? -transform.right : transform.right;

        float[] delays = Guns.Select(g => (float)random.NextDouble() * MaxDelay).ToArray();

        for (int i = 0; i < Guns.Count; i++)
        {
            // TODO: this does not work as intended, as 1 Audiosource cannot play twice at the same time.
            Audio[random.Next(Audio.Count)].PlayDelayed(delays[i]);
        }

        foreach (var gun in Guns)
        {
            GameObject cannonBall = Instantiate(CannonBall, transform.position + Direction, Quaternion.identity);
            Rigidbody ballsRigidbody = cannonBall.AddComponent<Rigidbody>();
            ballsRigidbody.AddForce( 100 * (5 * Direction + Vector3.up), ForceMode.Force);
        }


    }

    void Awake () {
        InitialiseAudioSources();
        Compass = MeshGenerator.CreateCompass();
        Compass.transform.parent = transform;
    }

	// Use this for initialization
	void Start () {
        MeshGenerator.MeshGenTest();
	}

    // Update is called once per frame

    void Update()
    {
        // Fire!
        if (Input.GetKeyUp(Left) && !Input.GetKeyUp(Right))
        {
            if (Time.time - TimeAtLastClick <= DoublePressThresholdTime)
            {
                Fire(true);
                TimeAtLastClick = float.MinValue;
            } else {
                TimeAtLastClick = Time.time;
            }
        }

        if (Input.GetKeyUp(Right) && !Input.GetKeyUp(Left))
        {
            if (Time.time - TimeAtLastClick <= DoublePressThresholdTime)
            {
                Fire(false);
                TimeAtLastClick = float.MinValue;
            }
            else
            {
                TimeAtLastClick = Time.time;
            }
        }

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
