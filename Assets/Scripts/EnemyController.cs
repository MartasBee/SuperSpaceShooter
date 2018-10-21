using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnemyBoundary : Boundary
{
	EnemyBoundary ()
	{
		Init ();
	}
	public bool Init ()
	{
		xMin =  -8.5f;
		xMax =   8.5f;
		
		zMin = -25.0f;
		zMax =  25.0f;

		return true;
	}
}


public class EnemyController : MonoBehaviour
{
	//==================================================================================================

	public CoreSystems 	 nesCoreSys;
	public EngineSystems nesEngiSys;
	public WeaponSystems nesWeapSys;
	
	public eScoreObjectTypes scoreType
	{
		get;
		protected set;
	}
	

	// this references are taken up automatically in method Awake ()
	private WeaponController[] 	refAllWeaponControlls;


	public EnemyBoundary		enemyBoundary;
	// have to be set up from Unity Editor
	public GameObject 			refExplosionEnemy;

	//==================================================================================================

	void Awake ()
	{
		// get all weapon controller references
		refAllWeaponControlls = gameObject.GetComponentsInChildren <WeaponController>();
		if (refAllWeaponControlls.Length <= 0)
		{
			Debug.LogError ("EnemyController: No Weapon Controllers was found...");
		}

		// newly create ship is not active till the enemyShipSett.Init is called (!!)
		gameObject.SetActive (false);
	}

	void Start ()
	{
		nesWeapSys.SetReferenceShipCoreEnergy (ref nesCoreSys.nesEnergy);
		nesWeapSys.SetReferenceShipAllWeapons (ref refAllWeaponControlls);

		nesWeapSys.ActivateWeapons ();

		rigidbody.velocity = transform.forward * nesEngiSys.actualSpeed;

		StartCoroutine (Evade ());
	}

	IEnumerator Evade ()
	{
		yield return new WaitForSeconds (Random.Range (nesEngiSys.startWait.x, nesEngiSys.startWait.y));
		
		while (true)
		{
			nesEngiSys.targetManeuver = Random.Range (1, nesEngiSys.dodge) * -Mathf.Sign (transform.position.x);
			yield return new WaitForSeconds (Random.Range (nesEngiSys.maneuverTime.x, nesEngiSys.maneuverTime.y));		
			
			nesEngiSys.targetManeuver = 0;
			yield return new WaitForSeconds (Random.Range (nesEngiSys.maneuverWait.x, nesEngiSys.maneuverWait.y));		
		}
	}

	void FixedUpdate ()
	{
		float newManeuver = Mathf.MoveTowards (rigidbody.velocity.x,
		                                       nesEngiSys.targetManeuver,
		                                       nesEngiSys.smoothing * Time.deltaTime);
		
		rigidbody.velocity = new Vector3 (newManeuver,
		                                  0.0f,
		                                  transform.forward.z * nesEngiSys.actualSpeed);	
		
		rigidbody.position = new Vector3
		(
			Mathf.Clamp(rigidbody.position.x, enemyBoundary.xMin, enemyBoundary.xMax), 
			0.0f, 
			Mathf.Clamp(rigidbody.position.z, enemyBoundary.zMin, enemyBoundary.zMax)
		);

		// left OR rigth tilt rotation - during movement to left or right side of the arena
		rigidbody.rotation = Quaternion.Euler (0.0f, 180.0f, rigidbody.velocity.x * -nesEngiSys.actualTilt);
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Boundary" || other.tag == "Bonus" || other.tag == "Weapon")
		{
			// 1) ignore BOUNDARY --> boundary will destroy THIS after
			//	  OnTriggerExit of Destroy By Boundary script is called
			// 2) ignore BONUS --> is just for player (!!)
			// 3) ignore WEAPON
			return;
		}


		if (other.tag == "BulletPlayer")
		{
			// enemy ship was hit by player (!!)

			// fail-safe
			float bullDamage = 50.0f;
			
			BulletController refBullCtrl = other.gameObject.GetComponent <BulletController>();
			if (refBullCtrl != null)
			{
				bullDamage = refBullCtrl.nesBulletSys.GetDamageCollision ();
			}
			else
			{
				Debug.LogWarning ("ENEMY >> Unable to obtain damage of bullet, which just hit the enemy...");
			}

			// destroy player's bullet
			Destroy (other.gameObject);

			// show explosion caused by hit
			Instantiate (refExplosionEnemy, transform.position, transform.rotation);

			bool isAlive = HitEnemy (bullDamage);

			if (!isAlive)
				EnemyDestruction (true);
		}
		else if (other.tag == "Bullet")
		{
			// enemy was hit by another enemy --> just show explosion, do not take live

			// destroy enemy's bullet
			Destroy (other.gameObject);

			// show explosion caused by hit
			Instantiate (refExplosionEnemy, transform.position, transform.rotation);
		}
		else if (other.tag == "Player")
		{
			// destruction of PLAYER is in Player Controller
			// destroy enemy's THIS object, add no score
			EnemyDestruction (false);
		}
		else if (other.tag == "Shield")
		{
			// Shield is not trigger, therefore this statement should not be called
		}
		else if (other.tag == "Asteroid" || other.tag == "Enemy")
		{
			// Asteroid nor Enemy are triggers, so this code should not be called
		}
		else
		{
			Debug.Log ("ENEMY >> Something just hit the ship (!!) >> " + other.name);
		}
	}

	void OnCollisionEnter (Collision collision)
	{
		bool bDestroy = false;
		
		if (collision.collider.tag == "Asteroid" || collision.collider.tag == "Enemy")
		{
			bDestroy = true;			
		}
		else if (collision.collider.tag == "Shield")
		{
			// we just hit the shield --> do nothing for now
		}
		else
		{
			Debug.Log ("UNKNOWN >> Collision >> Object \"Enemy\" collided with \""
			           + collision.collider.name
			           + "\" >> This \"Enemy\" will not be destroyed");
		}
				
		if (bDestroy)
			EnemyDestruction (false);
	}

	//==================================================================================================

	/**
	 * @param enType
	 * @param enWave
	 * 
	 */
	public bool InitEnemy (eScoreObjectTypes enType, int enWave)
	{
		bool initialized = true;
		
		switch (enType)
		{
		case eScoreObjectTypes.EnemyUWingBasic:
			{
				float tmpMove = (enWave / 5) * 0.05f;
				
				float tmpHeal =  40.0f;
				float tmpEner = 100.0f;
				
				CoreSystems.Energy.eRechargeTypes tmpERec = CoreSystems.Energy.eRechargeTypes.NoRecharge;
				WeaponSystems.eWeaponTypes 	tmpWeap = WeaponSystems.eWeaponTypes.NoWeapon;
				Weapon.eAmmoTypes 			tmpAmmo = Weapon.eAmmoTypes.NoShoting;
				
				bool tmpAuto = false;
				
				if (enWave >= 10)
				{
					tmpHeal = 60.0f;
					tmpERec = CoreSystems.Energy.eRechargeTypes.VerySlow;
					tmpWeap = WeaponSystems.eWeaponTypes.ForwardBasic;
					tmpAmmo = Weapon.eAmmoTypes.Toy;
					tmpAuto = true;
				}
				if (enWave >= 25)
				{
					tmpHeal = 80.0f;
					tmpERec = CoreSystems.Energy.eRechargeTypes.Slow;
					tmpAmmo = Weapon.eAmmoTypes.Basic;
				}
				
				// init lifes, health, shield, energy and weapons
				initialized = InitEnemy (1, tmpHeal, 0.0f, tmpEner, tmpERec, tmpWeap, tmpAmmo, tmpAuto);
				// init engines
				initialized = nesEngiSys.Init ((2.0f + tmpMove), (1.0f + tmpMove), 0.0f, false) && initialized;
			}
			break;
			
		case eScoreObjectTypes.EnemyUWingMedium:
			{
				float tmpMove = (enWave / 5)  * 0.1f;
				
				float tmpHeal =  60.0f;
				float tmpEner = 100.0f;
				
				CoreSystems.Energy.eRechargeTypes tmpERec = CoreSystems.Energy.eRechargeTypes.VerySlow;
				WeaponSystems.eWeaponTypes 	tmpWeap = WeaponSystems.eWeaponTypes.ForwardBasic;
				Weapon.eAmmoTypes 			tmpAmmo = Weapon.eAmmoTypes.Toy;
				
				bool tmpAuto = true;
				
				if (enWave >= 5)
				{
					tmpAmmo = Weapon.eAmmoTypes.Basic;
				}
				if (enWave >= 10)
				{
					tmpHeal = 80.0f;
					tmpERec = CoreSystems.Energy.eRechargeTypes.Slow;
					tmpWeap = WeaponSystems.eWeaponTypes.ForwardBetter;
					tmpAmmo = Weapon.eAmmoTypes.Toy;
				}
				if (enWave >= 20)
				{
					tmpAmmo = Weapon.eAmmoTypes.Basic;
				}
				if (enWave >= 30)
				{
					tmpHeal = 100.0f;
					tmpERec = CoreSystems.Energy.eRechargeTypes.Medium;
					tmpWeap = WeaponSystems.eWeaponTypes.ForwardAdvanced;
					tmpAmmo = Weapon.eAmmoTypes.Toy;
				}
				if (enWave >= 50)
				{
					tmpAmmo = Weapon.eAmmoTypes.Basic;
				}
				
				// init lifes, health, shield, energy and weapons
				initialized = InitEnemy (1, tmpHeal, 0.0f, tmpEner, tmpERec, tmpWeap, tmpAmmo, tmpAuto);
				// init engines
				initialized = nesEngiSys.Init ((3.0f + tmpMove), (2.0f + tmpMove), 0.0f, false) && initialized;
			}
			break;
			

		default:
			Debug.LogWarning ("ENEMY SETT >> Unknown enemy type");
			initialized = false;
			break;
		}

		if (initialized)
			scoreType = enType;

		nesCoreSys.collisionDamageRate = 1000.0f;
		
		return initialized;
	}
	
	/**
	 * @param initHealth
	 * @param initShield
	 * @param initEnergy
	 * @param rechargeType
	 * @param weaponType
	 * @param ammoType
	 */
	public bool InitEnemy (int   initLifes,
	                  	   float initHealth,
	                  	   float initShield,
	                  	   float initEnergy,
	                  	   CoreSystems.Energy.eRechargeTypes rechargeType,
	                  	   WeaponSystems.eWeaponTypes 		 weaponType,
	                  	   Weapon.eAmmoTypes 		 		 ammoType,
	                  	   bool	 autoShoting)
	{
		bool ret = true;
		
		ret = nesCoreSys.Init (initLifes, initHealth, initShield, initEnergy, rechargeType) && ret;
		ret = nesWeapSys.Init (weaponType, ammoType, autoShoting) && ret;
		
		return ret;
	}
	
	/**
	 * Damages caused by other players when THIS is hit
	 * 
	 * @param damage
	 */
	bool HitEnemy (float damage)
	{
		return nesCoreSys.Hit (damage);
	}
	
	/**
	 * Damages caused by collision to other objects
	 */
	float GetCollisionDamageRate ()
	{
		return nesCoreSys.GetCollisionDamageRate ();
	}

	/**
	 * @param addScore
	 */
	public void EnemyDestruction (bool addScore)
	{
		if (addScore)
			RuntimeContext.GetInst ().AddScore (scoreType);

		// show explosion caused by destruction
		Instantiate (refExplosionEnemy, transform.position, transform.rotation);

		// called automaticly by explosion animation
		//audio.Play ();

		// have to be last call (!!)
		Destroy (gameObject);
	}

	//==================================================================================================
}
