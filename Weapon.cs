using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	public enum WeaponType {MeleeOnly, ProjectileOnly, ProjectileAndMelee};//"projectile" basically means if it is a gun that shoots bullets. HINT: you can use this for bows too!
	public enum FireMode {Bolt, Semi, Auto};

	[Tooltip ("This will be the name that shows up in-game " +
		"and what other scripts search for.")]
	[Header ("This is the weapon 'customization'")]
	public string WeaponName;
	[Tooltip ("Melee Only, projectile Shooter, or Both.")]
	public WeaponType ThisWeaponType;//if you select an option here, and select something thats further down that wont be possible to work, it wont even try to do it, so don't worry.
	[Tooltip ("How does this gun shoot?")]
	public FireMode ThisFireMode;


	public float DistanceB;

	[HideInInspector]
	public WeaponManager WM;//I might need to keep an eye on this to see if it always gets linked correctly

	[HideInInspector]
	public bool IsSelected = false;

	//[HideInInspector]
	public bool ReadyToFire = false;
	public bool WeaponDrawn = false;

	public enum WeaponAimStatus {Aimed, UnAimed, Aiming, UnAiming};
	public enum WeaponOtherStatus {Running, Walking, Idle};

	public WeaponAimStatus AimStatus = WeaponAimStatus.UnAimed;
	public WeaponOtherStatus WeaponStatus = WeaponOtherStatus.Idle;

	public Animator ThisAnimatorController;//I will need to make a readme on how to setup the animator controller stuff. Its the only way I know how to handle animations AND that is not depreciated!

	[Header ("These names are what you called the variables in the animator controller (these are booleans")]
	public string Anim_Idle;
	public string Anim_Walk;
	public string Anim_Run;
	public string Anim_WeaponIsDrawn;
	public string Anim_Aiming;
	[Header ("These are triggers")]
	public string Anim_Draw;
	public string Anim_UnDraw;
	public string Anim_Fire;
	public string Anim_Melee;
	public string Anim_Aim;

	[Header ("Finally, this is for aiming!")]
	public Transform StartingPos;//This wont need to be set, this will automatically be set. I probably should just make this private
	[Header ("This is where the gun will be by the time its completely aimed in")]
	[Tooltip ("This will be for the whole gameobject")]
	public Transform EndingPos;
	[Header("I shouldn't have to explain this one..")]
	[Tooltip("I really shouldn't...")]
	public float AimingSpeed;


	[Header("This is the FOV that will be loaded in when the game starts")]
	[Tooltip("You should have this set by your 'settings' file")]
	public float FieldOfView;
	[Header("This is the FOV that you set you want to be when you are fully aimed in")]
	public float AimingFieldOfView;


	[Tooltip ("This will only work if the mode is on semi")]
	[Header ("Number of shots per burst.")]
	public int ShotsPerBurst;//If the gun is set to more than one, then it will shoot accordingly. Please set the TimeBetweenShotBursts for burst fireing.

	[Tooltip ("The amount of time that is inbetween shots of the burst." +
		"This is set in seconds.")]
	[Header ("Seconds in between shots")]
	public float TimeBetweenShotBursts;//if this is set to 0, you have your self a shotgun!

	[Tooltip ("This is universal for all types. This is the weapon mass.")]
	[Header ("Mass in pounds")]
	public float WeaponMass;


	[Header ("This is where the bullet leaves the gun")]
	[Tooltip ("This can be a plane with an empty texture, " +
		"the mesh renderer will get disabled the gun gets enabled.")]
	public Transform ExitBarrel;

	void Start () {//Any weapon of any kind shouldn't be enabled or "IsSelected"(Which is the same thing)
		PreSetVariables();
		DisableExitBarrelRenderer();
		CheckReady ();
		DrawWeapon ();//I Might need to call this later.
	}

	void Update () {
		//Animation Handling
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			
			AttemptToFire ();

		}


		//Aim Handling
/*		if (Input.GetKeyDown (KeyCode.Mouse1)) {
			//ThisAnimatorController.SetBool (Anim_Aiming, true);                 This is all for me
			ThisAnimatorController.SetTrigger (Anim_Aim);
		}
		if (Input.GetKey (KeyCode.Mouse1)) {
			ThisAnimatorController.SetBool (Anim_Aiming, true);
		}
		if (!Input.GetKey (KeyCode.Mouse1)) {
			ThisAnimatorController.SetBool (Anim_Aiming, false);
		}
*/


		//Aiming Lerping handling


		//Attack Handling




	}

	void AttemptToFire(){
		if (ReadyToFire) {
			Fire ();
		}
	}

	void Fire(){//This is where I will handle the bursts, maximum fire rates and whatnot
		RaycastHit ObjectHit;
		Debug.DrawRay (ExitBarrel.position, ExitBarrel.transform.TransformVector(Vector3.up) * DistanceB, Color.cyan, 60.0f, false);
		if (Physics.Raycast (ExitBarrel.position, Vector3.forward, out ObjectHit)) {
			//ThisAnimatorController.Set
			//Debug.DrawRay (ExitBarrel.position, ExitBarrel.transform.TransformVector(Vector3.forward), Color.cyan, 60.0f, false);
			Debug.Log (WeaponName + " has hit " + ObjectHit.transform.name);
		}
	}

	void DrawWeapon(){
		if (!WeaponDrawn) {
			if (!ThisAnimatorController.GetBool (Anim_WeaponIsDrawn)) {
				ThisAnimatorController.SetBool(Anim_WeaponIsDrawn, true);
				ThisAnimatorController.SetTrigger (Anim_Draw);
				WeaponDrawn = true;

			}
		}
	}

	void CheckReady(){//I will add an If statement to check ammo count and probably another variable.
		ReadyToFire = true;
	}

	void DisableExitBarrelRenderer(){
		ExitBarrel.GetComponent<MeshRenderer>().enabled = false;
		ExitBarrel.GetComponent<MeshCollider>().enabled = false;
	}

	void PreSetVariables(){//This is where object references are set, and where the aiming transforms are originally set.
		WM = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager> ();//I probably should just check the parents to get this. But this is good for now.
		if (GetComponent<Animator> ()) {
			ThisAnimatorController = GetComponent<Animator> ();
		} else {
			Debug.Log ("I don't have an Animtor Controller Component attached to " + WeaponName);
		}
		StartingPos = transform;





	}

}
