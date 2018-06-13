#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Ship : MonoBehaviour {

    [SerializeField] float Speed = 55f;
    [SerializeField] float ShipTurnSpeed = 50f;
    [SerializeField] float InputTurnSpeed = 200f;
    [SerializeField(), Range(-180f, 180f)] float TargetDirectionAngle = 0f;
    [SerializeField] List<Transform> LeftGuns, RightGuns;
    [SerializeField] List<AudioClip> GunSounds;
    [SerializeField] GameObject CannonBall;
	[SerializeField] float MaxFireDelay;
    [SerializeField] float ReloadTime;
    [SerializeField] Material CompassMaterial;
    [SerializeField] int HitPoints = 40;

	public PlayerData playerData;

	//public int factionNumber;
	//public KeyCode LeftKey, RightKey, FireKey;
	public bool isSinking;
	//public string playerName;

    private List<AudioSource> LeftGunAudioSources, RightGunAudioSources;
    private float CurrentDirectionAngle, DeltaAngle;
    private float LastFireTime = float.MinValue;
    private System.Random random = new System.Random();
    private GameObject Compass;

	public void Initialise (PlayerData playerData) {
		this.playerData = playerData;

		playerData.OnFactionChange += OnFactionChange;

		playerData.OnDelete += Destory;

	}
		
	void OnFactionChange () {
		SetCompassColor (Factions.List [playerData.FactionCode].color);
	}

	public void Destory () {
		playerData.Ship = null;
		playerData.OnDelete -= Destory;
		playerData.OnFactionChange -= OnFactionChange;
		GameObject.Destroy (gameObject);
	}

    private Vector3 TargetDirectionVector { get
        {
            return new Vector3(
                Mathf.Sin(Mathf.Deg2Rad * TargetDirectionAngle), 
                0f, 
                Mathf.Cos(Mathf.Deg2Rad * TargetDirectionAngle));
        }
    }

	public void TakeDamage (int amount) {
		HitPoints -= amount;
		if (HitPoints < 1)
		{
			isSinking = true;
			Destroy(transform.GetComponent<Rigidbody>());
			GameManager.Instance.CheckForWinner ();
		}
	}

	public void SetCompassColor (Color color) {
		Compass.GetComponentInChildren<MeshRenderer> ().material.color = color;
	}
		
    private void InitialiseAudioSources () {
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
		
    private IEnumerator DelayedCannonBallInstantiation (float delay, Transform gunPosition, bool LeftSide) {
        yield return new WaitForSeconds(delay);

        GameObject cannonBall = Instantiate(CannonBall, gunPosition.position, Quaternion.identity);
        Vector3 fireDirection = LeftSide ? -transform.right : transform.right;
        cannonBall.GetComponent<Rigidbody>().AddForce(4000 * (4 * fireDirection + 0.08f *Vector3.up), ForceMode.Force);
    }

    private void Fire (bool LeftSide) {

        if (Mathf.Abs(Time.time - LastFireTime) >= ReloadTime) {
            LastFireTime = Time.time;

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

    }
		
    void Awake () {
        InitialiseAudioSources();
        Compass = MeshGenerator.CreateCompass(50f, 2f, 20f, 40);
        Compass.transform.parent = transform;
        Compass.transform.position = transform.position; // testing...
        Compass.transform.position += 1.5f * Vector3.up;
        Compass.GetComponent<MeshRenderer>().material = CompassMaterial;
        AddTriggers();
    }
		
    void FixedUpdate()
    {
		if (GameManager.Instance.isPaused) return;

        if (isSinking) {
            this.transform.position +=  transform.forward * Speed * Time.deltaTime;
            this.transform.position += -transform.up * 20 * Time.deltaTime;


            if (transform.position.y <= -100f) {
                GameObject.Destroy(gameObject);
            }

            return;
        }

		if (Input.GetKey(playerData.FireKey))
        {
			if (Input.GetKeyDown(playerData.LeftKey)) {
                Fire(true);
            }
			if (Input.GetKeyDown(playerData.RightKey)) {
                Fire(false);
            }
        } else  {
			if (!Input.GetKey(playerData.LeftKey) && Input.GetKey(playerData.RightKey))
            {
                TargetDirectionAngle += InputTurnSpeed * Time.deltaTime;
                if (TargetDirectionAngle > 180f)
                {
                    TargetDirectionAngle -= 360f;
                }
            }
			if (!Input.GetKey(playerData.RightKey) && Input.GetKey(playerData.LeftKey))
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

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Ship")
        {
            Debug.Log(gameObject.name + " collided with " + collider.transform.parent.parent.name);

            GameObject other = collider.transform.parent.parent.gameObject;

            Ship otherShip = other.GetComponent<Ship>();

            if (this.gameObject.GetHashCode() < other.GetHashCode())
            {
                int min = Mathf.Min(this.HitPoints, otherShip.HitPoints);
                this.TakeDamage(min);
                otherShip.TakeDamage(min);
            }
        }
    }

}
