using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	public enum WeaponType {MeleeOnly, ProjectileOnly, ProjectileAndMelee};//"projectile" basically means if it is a gun that shoots bullets. HINT: you can use this for bows too!
	public enum FireMode {Bolt, Semi, Auto};

	public KeyCode Shoot;//set this from your settings file.

	public bool AllowedToFire;//This is just so you can control if the player can fire via other scripts. Like in a safe zone or something, set this to false.

	public bool IsBursting;//Don't mind this.
	public bool IsRunning;//This only gets read in here, this does not get set. You have to set this manually from a player controller or something.

	[Header("You know what this is for")]
	public bool UseAmmo;
	public int MaxAmmoInClip, MaxAmmoCanCarry, CurrentAmmo, CurrentAmmoInClip;
	public int AmmoUsedPerShot;

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

	public string DebugReset;
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
	public string Anim_Reload;

	[Header ("Finally, this is for aiming!")]
	public Vector3 StartPos;
	public Quaternion StartRot;

	[Header ("This is where the gun will be by the time its completely aimed in")]
	[Tooltip ("This will be for the whole gameobject")]
	public Vector3 EndPos;
	public Quaternion EndRot;

	public bool CameraStuff = false;

	public float PercentAimedIn;//This should eventually be set to private.
	public float ActualAimInNum;
	public float ActualAimOutNum;
	public float DistanceOfAim;
	public float PercentCam;//This should only be for me and if CameraStuff == true;

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
	public float ROF;//if this is set to 0, you have your self a shotgun!

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
		//DrawWeapon ();//I Might need to call this later.
	}

	void Awake(){//I should eventually get rid of the awake function
		PreSetVariables();
		CheckReady ();
		DrawWeapon ();
	}

	void Update () {
		if (!IsBursting && !IsRunning) {
			ThisAnimatorController.SetBool (Anim_Idle, true);// I might need to do something with Animator.GetBool(Anim_Drawn)
		}


		//Animation Handling

		//Aiming Lerping handling
		if (Input.GetKey (KeyCode.Mouse1)) {//You should probably change this to load what keycode from your settings file.
			if (ActualAimInNum < DistanceOfAim) {

				AimStatus = WeaponAimStatus.Aiming;
				ActualAimInNum += Time.deltaTime * AimingSpeed;
				float finale = ActualAimInNum / DistanceOfAim;

				transform.localPosition = Vector3.Lerp (StartPos, EndPos, finale);
				ActualAimOutNum = DistanceOfAim - ActualAimInNum;
			}else if (ActualAimInNum > DistanceOfAim) {//might need to keep an eye on this
				ActualAimInNum = DistanceOfAim;
				ActualAimOutNum = 0.0f;
				AimStatus = WeaponAimStatus.Aimed;
			}
		} else {
			if (ActualAimOutNum < DistanceOfAim) {
				AimStatus = WeaponAimStatus.UnAiming;
				ActualAimOutNum += Time.deltaTime * AimingSpeed;
				float finale = ActualAimOutNum / DistanceOfAim;
				ActualAimInNum = DistanceOfAim - ActualAimOutNum;//Multipliying all these by the variable aimingspeed might be a bad idea. I'll find out later :P
				transform.localPosition = Vector3.Lerp(EndPos, StartPos, finale);

			} else if (ActualAimOutNum > DistanceOfAim) {
				ActualAimOutNum = DistanceOfAim;
				ActualAimInNum = 0.0f;
				AimStatus = WeaponAimStatus.UnAimed;
			}
		}
		PercentAimedIn = ActualAimInNum * 100;
		//Attack Handling
		if (Input.GetKeyDown (Shoot)) {
			if(AllowedToFire){
				//this is the burst handling if there is any.
				if (ShotsPerBurst > 1) {

					if (!IsBursting) {
						StartCoroutine ("Burst");
					}

				} else if (ShotsPerBurst == 1) {
					AttemptToFire ();
				}

			}
		}



	}

	IEnumerator Burst(){
		if (CurrentAmmoInClip <= MaxAmmoInClip && CurrentAmmoInClip > AmmoUsedPerShot) {
			IsBursting = true;
			for (int I = 0; I < ShotsPerBurst; I++) {// I will need to add a current ammo in clip checker to this.
				Debug.Log ("Shots Fired!");
				AttemptToFire ();

				yield return new WaitForSeconds (ROF);
			}
			if (ShotsPerBurst == AmmoUsedPerShot) {
				CurrentAmmoInClip -= AmmoUsedPerShot;
				Debug.Log ("Party animal");
			}
			IsBursting = false;
		}
	}

	void AttemptToFire(){
		if (ReadyToFire) {//
			if (AllowedToFire) {//3
			//Debug.Log("TacoBell is ass");
			if (UseAmmo){
				//Debug.Log("TacoBell is ass");
				if (CurrentAmmoInClip <= MaxAmmoInClip && CurrentAmmoInClip > AmmoUsedPerShot) {
					//Debug.Log("TacoBell is ass");
						Debug.Log("TacoBell is ass");
						//Debug.Log(this.name);
						ThisAnimatorController.SetTrigger(Anim_Fire);
						Fire ();//This needs to be a co-routine
					}
				}
			}//3


		}//
	}

	void Fire(){//This is where I will handle the bursts, maximum fire rates and whatnot
		RaycastHit ObjectHit;
		Debug.DrawRay (ExitBarrel.position, ExitBarrel.transform.TransformVector(Vector3.up) * DistanceB, Color.cyan, 60.0f, false);
		if (Physics.Raycast (ExitBarrel.position, Vector3.forward, out ObjectHit)) {
			//ThisAnimatorController.Set
			//Debug.DrawRay (ExitBarrel.position, ExitBarrel.transform.TransformVector(Vector3.forward), Color.cyan, 60.0f, false);
			//Debug.Log (WeaponName + " has hit " + ObjectHit.transform.name);
		}

	} 

	void DrawWeapon(){
		ThisAnimatorController.SetBool (Anim_Idle, true);
		if (!WeaponDrawn) {
			if (!ThisAnimatorController.GetBool (Anim_WeaponIsDrawn)) {
				ThisAnimatorController.SetBool (Anim_WeaponIsDrawn, true);
				ThisAnimatorController.SetBool (Anim_Idle, true);
				ThisAnimatorController.SetTrigger (Anim_Draw);
				WeaponDrawn = true;

			}
		} else {
			ThisAnimatorController.SetBool (Anim_Idle, true);
			ThisAnimatorController.SetTrigger (Anim_Draw);//THIS MIGHT NOT NEED TO BE HERE, BOTH OF THESE LINES ACTUALLY AHHHH
		}
	}

	void Reload(){
		if (CurrentAmmoInClip < MaxAmmoInClip) {
			if (CurrentAmmo > 0) {
				if (CurrentAmmo < MaxAmmoInClip) {
					ThisAnimatorController.SetTrigger (Anim_Reload);//I'm going to have to do something with making sure this gets completed
					CurrentAmmoInClip = CurrentAmmo;
					CurrentAmmo = 0;

				} else if (CurrentAmmo > MaxAmmoInClip) {
					ThisAnimatorController.SetTrigger (Anim_Reload);//I'm going to have to do something with making sure this gets completed
					CurrentAmmoInClip = MaxAmmoInClip;
					CurrentAmmo = CurrentAmmo - MaxAmmoInClip;
				}
			}

		} else {
			//You don't need to reload then.
		}
	}


	void CheckReady(){//I will add an If statement to check ammo count and probably another variable.
		ReadyToFire = true;
		ThisAnimatorController.SetTrigger (DebugReset);
	}

	void DisableExitBarrelRenderer(){
		if (ExitBarrel) {
			ExitBarrel.GetComponent<MeshRenderer> ().enabled = false;
			ExitBarrel.GetComponent<MeshCollider> ().enabled = false;
		}
	}

	void PreSetVariables(){//This is where object references are set, and where the aiming transforms are originally set.
		WM = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager> ();//I probably should just check the parents to get this. But this is good for now.
		if (GetComponent<Animator> ()) {
			ThisAnimatorController = GetComponent<Animator> ();
		} else {
			Debug.Log ("I don't have an Animator Controller Component attached to " + WeaponName);
		}
		StartPos = transform.localPosition;
		StartRot = transform.localRotation;
		DistanceOfAim = Vector3.Distance (StartPos, EndPos);





	}

}
