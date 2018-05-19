using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 * Todo:
 * - Reload times
 * - Rock 'n' Pitch
 * - Collision & Cannonball deletion & Healthbar & Sinking
 * - Smoke & destruction
 */


public class Ship : MonoBehaviour {

    [SerializeField] KeyCode LeftKey, RightKey, FireKey;

    [SerializeField] float Speed = 1f;

    [SerializeField] float ShipTurnSpeed = 50f;

    [SerializeField] float InputTurnSpeed = 200f;

    [SerializeField] float RadiusOfGizmosCircle = 5f;

    [SerializeField] float DoublePressThresholdTime = 0.5f;

    [SerializeField(), Range(-180f, 180f)] float TargetDirectionAngle = 0f;

    [SerializeField] List<Transform> LeftGuns, RightGuns;

    [SerializeField] List<AudioClip> GunSounds;

    [SerializeField] GameObject CannonBall;

    [SerializeField] float MaxFireDelay;

    [SerializeField] float ReloadTime;

    [SerializeField] Material CompassMaterial;

    [SerializeField] float HitPoints = 100;

    private List<AudioSource> LeftGunAudioSources, RightGunAudioSources;

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
        LeftGunAudioSources = new List<AudioSource>();
        RightGunAudioSources = new List<AudioSource>();

        foreach (var leftGun in LeftGuns)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.clip = GunSounds[random.Next(GunSounds.Count)];
            LeftGunAudioSources.Add(source);
        }

        foreach (var rightGun in RightGuns)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.clip = GunSounds[random.Next(GunSounds.Count)];
            RightGunAudioSources.Add(source);
        }
    }


    IEnumerator DelayedCannonBallInstantiation (float delay, Transform gunPosition, bool LeftSide) {
        yield return new WaitForSeconds(delay);
        GameObject cannonBall = Instantiate(CannonBall, gunPosition.position, Quaternion.identity);

        cannonBall.name += "_Player1";

        //Rigidbody ballsRigidbody = cannonBall.AddComponent<Rigidbody>();

        Vector3 fireDirection = LeftSide ? -transform.right : transform.right;

        cannonBall.GetComponent<Rigidbody>().AddForce(2000 * (4 * fireDirection + 0.3f *Vector3.up), ForceMode.Force);
    }

    void Fire (bool LeftSide) {

        List<Transform> Guns = LeftSide ? LeftGuns : RightGuns;
        Vector3 Direction = LeftSide ? -transform.right : transform.right;
        List<AudioSource> sounds = LeftSide ? LeftGunAudioSources : RightGunAudioSources;

        float[] delays = Guns.Select(g => (float)random.NextDouble() * MaxFireDelay).ToArray();

        for (int i = 0; i < Guns.Count; i++)
        {
            sounds[i].PlayDelayed(delays[i]);

            StartCoroutine(DelayedCannonBallInstantiation(delays[i], Guns[i], LeftSide));
        }



    }

    void Awake () {
        InitialiseAudioSources();
        Compass = MeshGenerator.CreateCompass(50f, 5f, 20f, 40);
        Compass.transform.parent = transform;
        Compass.transform.position = transform.position; // testing...
        Compass.transform.position += 1.5f * Vector3.up;
        Compass.GetComponent<MeshRenderer>().material = CompassMaterial;

        AddTriggers();
    }

	// Use this for initialization
	void Start () {
        MeshGenerator.MeshGenTest();
	}

    // Update is called once per frame

    void FixedUpdate()
    {
        if (Input.GetKey(FireKey))
        {
            if (Input.GetKeyDown(LeftKey)) {
                Fire(true);
            }
            if (Input.GetKeyDown(RightKey)) {
                Fire(false);
            }
        } else  {
            if (!Input.GetKey(LeftKey) && Input.GetKey(RightKey))
            {
                TargetDirectionAngle += InputTurnSpeed * Time.deltaTime;
                if (TargetDirectionAngle > 180f)
                {
                    TargetDirectionAngle -= 360f;
                }
            }
            if (!Input.GetKey(RightKey) && Input.GetKey(LeftKey))
            {
                TargetDirectionAngle -= InputTurnSpeed * Time.deltaTime;
                if (TargetDirectionAngle <= -180f)
                {
                    TargetDirectionAngle += 360f;
                }
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

        Compass.transform.forward = TargetDirectionVector;


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

    void OnTriggerEnter (Collider collider) {
        //Debug.Log(gameObject.name + ": Detected collision with " + collider.name);
    }

    void AddTriggers () {



        var meshes = GameObject.FindObjectsOfType<MeshFilter>();

        foreach (var item in meshes
                 .Where(m=>m.gameObject.transform.parent != null)
                 .Where(m=>m.gameObject.transform.parent.parent != null)
                 .Where(m => m.gameObject.transform.parent.parent == gameObject.transform))
        {
            if (item.gameObject.name.Contains("color")) continue;

            var col = item.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = item.sharedMesh;
            col.convex = true;
            col.inflateMesh = true;
            col.isTrigger = true;
        }


    }


}
