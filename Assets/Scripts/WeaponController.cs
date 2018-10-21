using UnityEngine;
using System.Collections;


//==============================================================================
// WEAPON
//==============================================================================
[System.Serializable]
public class Weapon
{
	// reference to class Energy --> needed for shoting
	private CoreSystems.Energy 	refShipEnergy;
	//reference to ship's rigidbody component --> for physically based shoting
	private Vector3 			varShipRigidbodyVelocity;

	// reference to weapon itself --> for bullet init position and rotation
	private GameObject 			refObjectWeapon;
	// reference to bullet game object
	private GameObject 			refObjectBullet;

	//==========================================================================
	// E AMMO TYPES
	//==========================================================================
	public enum eAmmoTypes
	{
		NoShoting 	= 0,
		Custom		= 1,
		Toy			= 2,
		Basic,
		Medium,
		Better,
		Advanced,
		Hardcore,
		Armagedon,
		
		END_AMMO_TYPES
	}

	//==========================================================================

	// shoting speed
	private float fireRate;
	// energy costs of every shot
	private float fireEnergyCost;
	// how much energy consumed by shot will be transformed into bullet damage
	private float fireEnergyToBulletDamage;


	// calculated fire damage of each bullet
	private float bulletInitDamage;
	// initial speed of bullet
	private float bulletInitSpeed;
	// effective shot range
	private float bulletEffectiveShotRange;


	// are all weapon systems initialized ???
	private bool  isWeaponInitialized;


	// next shot is possible after nextShot seconds
	private float nextShot = 0.0f;
	// current ammo type
	private eAmmoTypes actualAmmoType;	

	//==========================================================================

	/**
	 * Just check, if weapons are ready for firing (!!)
	 * This method does not perform shoting (!!)
	 */
	private bool PerformShotIfCan ()
	{
		if ((Time.time > nextShot) && (refShipEnergy.ConsumeEnergyIfCan (fireEnergyCost)))
		{
			nextShot = Time.time + fireRate;
			return true;
		}
		else
			return false;
	}	


	/**
	 * 
	 */
	public Weapon ()
	{
		SetAmmoType (eAmmoTypes.Toy);
		isWeaponInitialized = false;
	}


	/**
	 * @param ammoType
	 * @param refShipEnergy
	 */
	public bool Init (eAmmoTypes 			 ammoType,
	                  ref CoreSystems.Energy refShipEnergy)
	{
		// reference to weapon GameObject itself HAVE TO be set from Awake () method of WeaponController script
		bool ret = (refObjectWeapon != null);

		ret = SetAmmoType (ammoType) && ret;

		SetReferenceShipSystemsEnergy (ref refShipEnergy);
		varShipRigidbodyVelocity = new Vector3 (0.0f, 0.0f, 0.0f);

		isWeaponInitialized = ret;
		return ret;
	}


	/**
	 * @param ref refSSE
	 */
	public void SetReferenceShipSystemsEnergy (ref CoreSystems.Energy refSSE)
	{
		refShipEnergy = refSSE;
	}
	/**
	 * @param ref refGOW
	 */
	public void SetReferenceObjectWeapon (GameObject refGOW)
	{
		refObjectWeapon = refGOW;
	}
	/**
	 * @param ref refGOB
	 */
	public void SetReferenceObjectBullet (GameObject refGOB)
	{
		refObjectBullet = refGOB;
	}

	
	/**
 	 * @param ammoType
 	 */
	public bool SetAmmoType (eAmmoTypes ammoType)
	{
		switch (ammoType)
		{
			// UNARMED:
		case eAmmoTypes.NoShoting:
			fireRate 	   			 = 1000.0f;	// shot once per 1000 secs
			fireEnergyCost 			 =    0.0f;	// no shot, no energy
			fireEnergyToBulletDamage =    0.0f;	// magic damage multiplier

			bulletInitSpeed			 =    0.0f;
			bulletEffectiveShotRange =    0.0f;

			// first shot allowed after 1000 seconds
			nextShot = 1000.0f;
			break;
			
			// TOY: damage 10 units, 6.4 energy per shot, 1 shot each 2.5 seconds
		case eAmmoTypes.Toy:
			fireRate 	   			 =    2.5f;
			fireEnergyCost 			 =    4.0f;
			fireEnergyToBulletDamage =    2.5f;

			bulletInitSpeed			 =    5.0f;
			bulletEffectiveShotRange =   25.0f;

			nextShot = 0.0f;
			break;
			
			// BASIC: damage 20 units, 3.2 energy per shot, 1 shot each 1.5 second
		case eAmmoTypes.Basic:
			fireRate 	   			 =    1.5f;
			fireEnergyCost 			 =    2.0f;
			fireEnergyToBulletDamage =   10.0f;

			bulletInitSpeed			 =   10.0f;
			bulletEffectiveShotRange =   50.0f;

			nextShot = 0.0f;
			break;
			
			// MEDIUM: damage 25 units, 1.6 energy per shot, 2 shots per 1.5 seconds
		case eAmmoTypes.Medium:
			fireRate 	   			 =    0.75f;
			fireEnergyCost 			 =    1.0f;
			fireEnergyToBulletDamage =   25.0f;

			bulletInitSpeed			 =   15.0f;
			bulletEffectiveShotRange =   75.0f;

			nextShot = 0.0f;
			break;
			
			// BETTER: damage 40 units, 1.0 energy per shot, 3 shots per second
		case eAmmoTypes.Better:
			fireRate 	   			 =    0.33f;
			fireEnergyCost 			 =    0.5f;
			fireEnergyToBulletDamage =   80.0f;

			bulletInitSpeed			 =   20.0f;
			bulletEffectiveShotRange =  100.0f;

			nextShot = 0.0f;
			break;
			
			// ADVANCED: damage 55 units, 0.3 energy per shot, 4 shots per second
		case eAmmoTypes.Advanced:
			fireRate 	   			 =    0.25f;
			fireEnergyCost 			 =    0.3f;
			fireEnergyToBulletDamage =  183.34f;

			bulletInitSpeed			 =   23.0f;
			bulletEffectiveShotRange =  150.0f;

			nextShot = 0.0f;
			break;
			
			// HARDCORE: damage 75 units, 0.2 energy per shot, 7 shots per second
		case eAmmoTypes.Hardcore:
			fireRate 	   			 =    0.15f;
			fireEnergyCost 			 =    0.2f;
			fireEnergyToBulletDamage =  375.0f;

			bulletInitSpeed			 =   26.0f;
			bulletEffectiveShotRange =  200.0f;

			nextShot = 0.0f;
			break;
			
			// ARMAGEDON: damage 100 units, 0.1 energy per shot, 10 shots per second
		case eAmmoTypes.Armagedon:
			fireRate 	   			 =    0.1f;
			fireEnergyCost 			 =    0.1f;
			fireEnergyToBulletDamage = 1000.0f;

			bulletInitSpeed			 =   30.0f;
			bulletEffectiveShotRange =  400.0f;

			nextShot = 0.0f;
			break;
			
		default:
			Debug.LogWarning ("PLAYER SETT >> Unknown weapons >> Current weapon: " + actualAmmoType);
			return false;
		}
		
		bulletInitDamage = fireEnergyCost * fireEnergyToBulletDamage;
		
		actualAmmoType = ammoType;
		return true;
	}
		
	
	/**
	 * 
	 */
	public bool Shot ()
	{
		if (!isWeaponInitialized)
			return false;

		if ((refObjectBullet != null) && PerformShotIfCan ())
		{
			GameObject cloneShot = GameObject.Instantiate(refObjectBullet,
			                                              refObjectWeapon.transform.position,
			                                              refObjectWeapon.transform.rotation) as GameObject;
			
			BulletController cloneCtrl = cloneShot.GetComponent <BulletController>();
			if (cloneCtrl == null)
			{
				GameObject.Destroy (cloneShot);
				return false;
			}
			else
			{
				cloneCtrl.nesBulletSys.Init (refObjectWeapon.transform,
				                             bulletInitSpeed,
				                             varShipRigidbodyVelocity,
				                             bulletInitDamage,
				                             bulletEffectiveShotRange);
				cloneShot.SetActive (true);
			}
			
			return true;
		}
		
		return false;
	}
}


//==============================================================================
//==============================================================================

public class WeaponController : MonoBehaviour
{
	// auto-shoting weapon --> for enemy ships
	public bool   autoShot = false;
	// posision-based identifier
	public string weaponPosision;


	// this reference have to be set from unity editor
	public GameObject refObjectBullet;
	// Weapon class for performing shoting and all about shoting
	public Weapon 	  nesWeapon;


	void Awake ()
	{
		nesWeapon = new Weapon ();
		nesWeapon.SetReferenceObjectWeapon (gameObject);
		nesWeapon.SetReferenceObjectBullet (refObjectBullet);
	}

	void Update ()
	{
		if (autoShot)
			nesWeapon.Shot ();
#if UNITY_ANDROID
		else if ((Input.touchCount == 1) && (Input.GetTouch(0).phase != TouchPhase.Canceled))
#else
		else if (Input.GetButton ("Fire1"))
#endif
			nesWeapon.Shot ();
	}
}
