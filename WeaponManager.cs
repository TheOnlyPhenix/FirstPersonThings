using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour {

	public Weapon[] Weapons;
	public Weapon CurrentlySelected;

	void Start () {
		DisableAllWeapons ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.G)) {
			ActivateWeapon (Weapons [0]);
		}
	}

	void ActivateWeapon(Weapon IWeapon){
		IWeapon.gameObject.SetActive (true);
		IWeapon.enabled = true;
		CurrentlySelected = IWeapon;
		IWeapon.IsSelected = true;
	}

	void DisableAllWeapons(){

		if (Weapons.Length > 0) {
			foreach (Weapon IWeapon in Weapons) {
				if(IWeapon != null){
					IWeapon.IsSelected = false;
					IWeapon.gameObject.SetActive (false);
					print (IWeapon.WeaponName + " has been disabled");
				}
			}
		}else{
			Debug.Log ("There are no weapons in the WeaponManager Weapon Array!");
		}

		Debug.Log ("All weapons have been disabled");
	}

}
