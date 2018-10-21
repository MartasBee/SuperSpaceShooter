using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerBoundary : Boundary
{
	public PlayerBoundary ()
	{
		Init ();
	}
	public bool Init ()
	{
		xMin =  -8.5f;
		xMax =   8.5f;

		zMin = -11.0f;
		zMax =  12.0f;

		return true;
	}
}


public class PlayerController : MonoBehaviour
{
	//==================================================================================================

	public CoreSystems 	 nesCoreSys;
	public EngineSystems nesEngiSys;
	public WeaponSystems nesWeapSys;


	public  PlayerBoundary		playerBoundary;
	// have to be set up from Unity Editor
	public  GameObject			refExplosionPlayer;


	private ShieldController	refShieldControll;
	private WeaponController[]	refAllWeaponControlls;


#if UNITY_ANDROID
	// accelerometer input variables
	private Vector3 zeroAc;
	private Vector3 curAc;
	private float 	sensH 	 = 5.0f;
	private float 	sensV 	 = 5.0f;
	private float 	smooth 	 = 0.5f;
#endif

	//==================================================================================================
	
	void Awake ()
	{
		// get shield controller reference
		refShieldControll = gameObject.GetComponentInChildren <ShieldController>();
		if (refShieldControll == null)
		{
			Debug.LogError ("PLAYER >> Shield Controller Object was not found...");
		}

		// get all weapon controller references
		refAllWeaponControlls = gameObject.GetComponentsInChildren <WeaponController>();
		if (refAllWeaponControlls.Length <= 0)
		{
			Debug.LogError ("PLAYER >> No Weapon Controllers was found...");
		}

		// game- OR level- controller have to initialize the ship and set it to active mode
		gameObject.SetActive (false);

		Debug.Log ("PLAYER >> Player ship was just created...");
	}

	void Start ()
	{
		// show actualized gui texts
		RuntimeContext.GetInst ().StorePlayerCore (nesCoreSys.nesLifes.GetLifes (),
		                                           nesCoreSys.nesHealth.GetHealth (),
		                                           nesCoreSys.nesShield.GetShield (),
		                                           nesCoreSys.nesEnergy.RechargeEnergy (),
		                                           nesCoreSys.nesEnergy.actualRechargeType);
		
		// store flags for bonus generation into global context container
		RuntimeContext.GetInst ().StorePlayerCoreMax (nesCoreSys.nesHealth.IsMaxHealth (),
		                                              nesCoreSys.nesShield.IsMaxShield ());

		nesWeapSys.SetReferenceShipCoreEnergy (ref nesCoreSys.nesEnergy);
		nesWeapSys.SetReferenceShipAllWeapons (ref refAllWeaponControlls);

		// activate weapons
		if (nesWeapSys.ActivateWeapons ())
		{
			// store current state of ship systems for next-time loading and bonus generation system
			RuntimeContext.GetInst ().StorePlayerWeapon(nesWeapSys.actualWeaponType,
			                                            nesWeapSys.actualAmmoType);
		}
		else
		{
			Debug.LogError ("PLAYER >> FAILED to activate weapons (!!)");
			Application.LoadLevel (eGameLevels.Screen_MainMenu.ToString ());
		}


#if UNITY_ANDROID
		ResetAccelerometerAxes ();
#endif


		Debug.Log ("PLAYER >> Player ship is ready to GO (!!): "
		           + "\nLifes: "  + nesCoreSys.nesLifes.GetLifes ()
		           + "\nHealth: " + nesCoreSys.nesHealth.GetHealth ()
		           + "\nShield: " + nesCoreSys.nesShield.GetShield ()
		           + "\nEnergy: " + nesCoreSys.nesEnergy.GetEnergy ()
		           + "\nEnergy Recharge: " 	+ nesCoreSys.nesEnergy.actualRechargeType
		           + "\nWeapon Type: "  	+ nesWeapSys.actualWeaponType
		           + "\nAmmo Type: " 		+ nesWeapSys.actualAmmoType);
	}

	void Update ()
	{
#if UNITY_ANDROID
#else
		if (Input.GetKeyDown (KeyCode.M))
		{
			ChangeMoveStyle ();

			rigidbody.drag = nesEngiSys.shipDrag;

			Debug.Log ("PLAYER >> CONTROL >> arcadeMovement: " + !nesEngiSys.physicalMoves
			           + " realSpeed: " + nesEngiSys.actualSpeed);
		}
#endif

		// recharge energy & show actualized gui texts
		RuntimeContext.GetInst ().StorePlayerCoreEnergy (nesCoreSys.nesEnergy.RechargeEnergy ());
	}

	void FixedUpdate()
	{
		// get MOVEMENT input from InputManager
		float moveHorizontal = 0.0f;
		float moveVertical   = 0.0f;

#if UNITY_ANDROID
		curAc = Vector3.Lerp (curAc, Input.acceleration-zeroAc, Time.deltaTime / smooth);

		moveVertical 	= Mathf.Clamp (curAc.y * sensV, -1.0f, 1.0f);
		moveHorizontal 	= Mathf.Clamp (curAc.x * sensH, -1.0f, 1.0f);

		// now use GetAxisV and GetAxisH instead of Input.GetAxis vertical and horizontal
		// If the horizontal and vertical directions are swapped, swap curAc.y and curAc.x
		// in the above equations. If some axis is going in the wrong direction, invert the
		// signal (use -curAc.x or -curAc.y)
#else
		moveHorizontal = Input.GetAxis ("Horizontal");
		moveVertical   = Input.GetAxis ("Vertical");
#endif

		if ((moveHorizontal != 0.0f || moveVertical != 0.0f) && (nesEngiSys.actualSpeed != 0.0f))
		{
			// MOVEMENT
			MoveTheShip (new Vector3 (moveHorizontal, 0.0f, moveVertical));


			/*
			// looks great, but cause problems in gameplay: rotated ship = rotated bullets = lower
			// probability to hit the enemy
			// left OR rigth tilt ROTATION - during movement to left or right side of the arena
			rigidbody.rotation = Quaternion.Euler (0.0f,
			                                       0.0f,
			                                       rigidbody.velocity.x * -nesEngiSys.actualTilt);
			*/
		}


		// BOUNDARY --> player ship cannot go out of playing scene
		rigidbody.position = new Vector3
		(
			Mathf.Clamp (rigidbody.position.x, playerBoundary.xMin, playerBoundary.xMax),
			0.0f,
			Mathf.Clamp (rigidbody.position.z, playerBoundary.zMin, playerBoundary.zMax)
		);
	}

	public void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Boundary" || other.tag == "Shield")
		{
			// 1) ignore BOUNDARY --> boundary will destroy THIS after
			//	  OnTriggerExit of Destroy By Boundary script is called
			// 2) ignore SHIELD --> for unknown reason, shield is collieding
			//	  with the ship in the moment, when the shield is activated...
			return;
		}


		// player ship is hit by ENEMY BULLET
		if (other.tag == "Bullet")
		{
			// fail-safe
			float bullDamage = 20.0f;
			
			BulletController refBullCtrl = other.gameObject.GetComponent <BulletController>();
			if (refBullCtrl != null)
			{
				bullDamage = refBullCtrl.nesBulletSys.GetDamageCollision ();
			}
			else
			{
				Debug.LogWarning ("PLAYER >> Unable to obtain damage of bullet, which just hit the player...");
			}

			// destroy enemy's bullet
			Destroy (other.gameObject);

			// show explosion caused by hit
			//Instantiate (refExplosionPlayer, transform.position, transform.rotation);

			bool isAlive = HitShip (bullDamage);

			// show actualized gui texts
			RuntimeContext.GetInst ().StorePlayerCore (nesCoreSys.nesLifes.GetLifes (),
													   nesCoreSys.nesHealth.GetHealth (),
													   nesCoreSys.nesShield.GetShield ());
			
			// store flags for bonus generation into global context container
			RuntimeContext.GetInst ().StorePlayerCoreMax (nesCoreSys.nesHealth.IsMaxHealth (),
			                                              nesCoreSys.nesShield.IsMaxShield ());
			
			if (!isAlive)
				PlayerDestruction ();
		}
		// player ship is hit by BONUS
		else if (other.tag == "Bonus")
		{
			BonusController refBonCtrl = other.gameObject.GetComponent <BonusController>();
			if (refBonCtrl != null)
			{
				if (refBonCtrl.bonusType == "Energy")
				{
					// recharge energy & show actualized gui texts
					RuntimeContext.GetInst ().StorePlayerCoreEnergy (
						nesCoreSys.nesEnergy.RechargeEnergy (refBonCtrl.bonusValue),
						nesCoreSys.nesEnergy.actualRechargeType);
				}
				else if (refBonCtrl.bonusType == "Shield")
				{
					// shield was initialized properlly
					if (refShieldControll != null)
					{
						// activate shield's GameObject
						refShieldControll.gameObject.SetActive (true);

						// recharge shield & show actualized gui texts
						RuntimeContext.GetInst ().StorePlayerCoreShield (
							nesCoreSys.nesShield.RechargeShield (refBonCtrl.bonusValue),
							nesCoreSys.nesShield.IsMaxShield ());
					}
					else
					{
						Debug.LogWarning ("PLAYER >> Shield was not initialized properlly, no bonus added...");
					}
				}
				else if (refBonCtrl.bonusType == "Health")
				{
					// recharge health & show actualized gui texts
					RuntimeContext.GetInst ().StorePlayerCoreHealth (
						nesCoreSys.nesHealth.RechargeHealth (refBonCtrl.bonusValue),
						nesCoreSys.nesHealth.IsMaxHealth ());
				}
				else
				{
					Debug.Log ("PLAYER >> Unknown bonus");
				}

				//Debug.Log ("PLAYER >> Bonus >> " + refBonCtrl.bonusType + " >> +" + refBonCtrl.bonusValue + " units");
			}

			// destroy bonus object
			Destroy (other.gameObject);
		}
		//
		else if (other.tag == "Weapon")
		{
			BonusController refBonCtrl = other.gameObject.GetComponent <BonusController>();
			if (refBonCtrl != null)
			{
				bool anyChange = false;

				if (refBonCtrl.bonusType == "AmmoType")
				{
					bool ammoTypeOk = true;

					switch (refBonCtrl.bonusMode)
					{
					case "Advanced":
						// check, if `advanced` ammo type can be activated
						if (nesWeapSys.actualAmmoType == Weapon.eAmmoTypes.Better)
						{
							if (!nesWeapSys.Init (nesWeapSys.actualWeaponType,
							                      Weapon.eAmmoTypes.Advanced,
							                      false))
							{
								ammoTypeOk = false;
								break;
							}

							anyChange = true;
						}
						break;

					case "Hardcore":
						// check, if `hardcore` ammo type can be activated
						if (nesWeapSys.actualAmmoType == Weapon.eAmmoTypes.Advanced)
						{
							if (!nesWeapSys.Init (nesWeapSys.actualWeaponType,
							                      Weapon.eAmmoTypes.Hardcore,
							                      false))
							{
								ammoTypeOk = false;
								break;
							}
							
							anyChange = true;
						}
						break;

					case "Armagedon":
						// check, if `armagedon` ammo type can be activated
						if (nesWeapSys.actualAmmoType == Weapon.eAmmoTypes.Hardcore)
						{
							if (!nesWeapSys.Init (nesWeapSys.actualWeaponType,
							                      Weapon.eAmmoTypes.Armagedon,
							                      false))
							{
								ammoTypeOk = false;
								break;
							}
							
							anyChange = true;
						}
						break;

					default:
						Debug.LogWarning ("PLAYER >> Unknown Ammo Type");
						break;
					}

					if (!ammoTypeOk)
						// initialization of new ammo type failed...
						Debug.LogWarning ("PLAYER >> Unable to initialize eAmmoTypes::" + refBonCtrl.bonusMode);
				}
				else if (refBonCtrl.bonusType == "Weapon")
				{
					bool weaponSystemOk = true;

					switch (refBonCtrl.bonusMode)
					{
					case "ForwardBetter":
						// check, if `forward better` weapon type can be activated
						if (nesWeapSys.actualWeaponType == WeaponSystems.eWeaponTypes.ForwardBasic)
						{
							if (!nesWeapSys.Init (WeaponSystems.eWeaponTypes.ForwardBetter,
							                      Weapon.eAmmoTypes.Better,
							                      false))
							{
								weaponSystemOk = false;
								break;
							}
							
							anyChange = true;
						}
						break;

					case "ForwardAdvanced":
						// check, if `forward advanced` weapon type can be activated
						if (nesWeapSys.actualWeaponType == WeaponSystems.eWeaponTypes.ForwardBetter)
						{
							if (!nesWeapSys.Init (WeaponSystems.eWeaponTypes.ForwardAdvanced,
							                      Weapon.eAmmoTypes.Better,
							                      false))
							{
								weaponSystemOk = false;
								break;
							}
							
							anyChange = true;
						}
						break;
					
					case "DirectionalBasic":
						// check, if `directional basic` weapon type can be activated
						if (nesWeapSys.actualWeaponType == WeaponSystems.eWeaponTypes.ForwardAdvanced)
						{
							if (!nesWeapSys.Init (WeaponSystems.eWeaponTypes.DirectionalBasic,
							                      Weapon.eAmmoTypes.Better,
							                      false))
							{
								weaponSystemOk = false;
								break;
							}
							
							anyChange = true;
						}
						break;

					default:
						Debug.LogWarning ("PLAYER >> Unknown Weapon System");
						break;
					}

					if (!weaponSystemOk)
						// initialization of new weapon type failed...
						Debug.LogWarning ("PLAYER >> Unable to initialize eWeaponTypes::" + refBonCtrl.bonusMode);
				}
				
				if (anyChange)
				{
					if (!nesWeapSys.ActivateWeapons ())
					{
						Debug.LogError ("PLAYER >> FAILED to activate weapon systems (!!)");
						anyChange = false;
					}
				}

				if (anyChange)
				{
					// store current state of ship systems for next-time loading and bonus generation system
					RuntimeContext.GetInst ().StorePlayerWeapon(nesWeapSys.actualWeaponType,
					                                            nesWeapSys.actualAmmoType);
				}
			}
			else
			{
				Debug.LogError ("PLAYER >> FAILED to obtain reference to `BonusController`");
			}

			// destroy bonus object
			Destroy (other.gameObject);
		}
		// player ship is hit by ASTEROID or ENEMY SHIP
		else if (other.tag == "Asteroid" || other.tag == "Enemy")
		{
			bool isAlive = HitShip (GetCollisionDamageRate ());

			// show actualized gui texts
			RuntimeContext.GetInst ().StorePlayerCore (nesCoreSys.nesLifes.GetLifes (),
			                                           nesCoreSys.nesHealth.GetHealth (),
			                                           nesCoreSys.nesShield.GetShield ());

			// store flags for bonus generation into global context container
			RuntimeContext.GetInst ().StorePlayerCoreMax (nesCoreSys.nesHealth.IsMaxHealth (),
													      nesCoreSys.nesShield.IsMaxShield ());
			
			if (!isAlive)
				PlayerDestruction ();
		}
		else
		{
			Debug.Log ("PLAYER >> Something just hit the ship (!!) >> " + other.name);
		}
	}

	void OnDestroy ()
	{
		Debug.Log ("PLAYER >> Ship was destroyed (!!)");
	}

	//==================================================================================================

	/**
	 * @param initHealth
	 * @param initShield
	 * @param initEnergy
	 * @param rechargeType
	 * @param weaponType
	 * @param ammoType
	 */
	public bool InitShip (int   initLifes,
	                      float initHealth,
	                      float initShield,
	                      float initEnergy,
	                      CoreSystems.Energy.eRechargeTypes rechargeType,
	                      WeaponSystems.eWeaponTypes 		weaponType,
	                      Weapon.eAmmoTypes 				ammoType)
	{
		bool ret = true;
		
		ret = nesCoreSys.Init (initLifes, initHealth, initShield, initEnergy, rechargeType) && ret;
		ret = nesWeapSys.Init (weaponType, ammoType, false) && ret;
		
		ret = SetMoveStyle (false) && ret;
		
		nesCoreSys.collisionDamageRate = 1000.0f;
		
		return ret;
	}
	
	
	/**
	 * Damages caused by other players when THIS is hit
	 * 
	 * @param damage
	 */
	bool HitShip (float damage)
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
	 * 
	 */
	bool MoveTheShip (Vector3 velocity)
	{
		// set velocity to player ship
		if (!nesEngiSys.physicalMoves)
		{
			// arcade movement style
			rigidbody.velocity = velocity * nesEngiSys.actualSpeed;
			// * (nesCoreSys.nesEnergy.GetEnergy () * 0.01f);
		}
		else
		{
			// physical based movement style
			rigidbody.AddForce (velocity * nesEngiSys.actualSpeed * Time.deltaTime, ForceMode.Force);
		}

		return true;
	}
	/**
	 * @param phyMove
	 */
	bool SetMoveStyle (bool phyMove)
	{
		if (phyMove)
			return nesEngiSys.Init (500.0f, 0.75f, true);
		else
			return nesEngiSys.Init ( 10.0f, 0.0f, false);
	}
	/**
	 * 
	 */
	bool ChangeMoveStyle ()
	{
		if (!nesEngiSys.physicalMoves)
			return nesEngiSys.Init (500.0f, 0.6f, true);
		else
			return nesEngiSys.Init ( 10.0f, 0.0f, false);
	}
	/**
	 * 
	 */
	void ResetAccelerometerAxes ()
	{
#if UNITY_ANDROID
		zeroAc 	= Input.acceleration;
		curAc 	= Vector3.zero;
#endif
	}
	
	/**
	 *
	 */
	public void PlayerDestruction ()
	{
		Instantiate (refExplosionPlayer, transform.position, transform.rotation);
		// called automaticly by explosion animation
		//audio.Play ();
		
		// ship was destroyed, actualize global context of remaining lifes
		RuntimeContext.GetInst ().nesActPlayer.actLifes = nesCoreSys.nesLifes.GetLifes ();
		
		SendDestructionNotifToController ();
		
		// have to be last call (!!)
		Destroy (gameObject);
	}
	
	/**
	 * 
	 */
	void SendDestructionNotifToController ()
	{
		// OK, now we have to find reference to GameController object to tell the controller, that game is over
		GameObject 		tmpGCO = GameObject.FindWithTag ("GameController");
		GameController	refGameControl;
		
		// get game controller reference
		if (tmpGCO != null)
		{
			// get reference to class "GameController"
			refGameControl = tmpGCO.GetComponent <GameController>();
			
			if (refGameControl == null)
			{
				Debug.LogError ("PLAYER >> Game Controller Class was not found...");
				Application.LoadLevel (eGameLevels.Screen_MainMenu.ToString ());
				return;
			}
		}
		else
		{
			Debug.LogError ("PLAYER >> Game Controller Object was not found...");
			Application.LoadLevel (eGameLevels.Screen_MainMenu.ToString ());
			return;
		}
		
		
		// tell GameController about destruction of player's ship
		if (nesCoreSys.nesLifes.IsAlive ())
			refGameControl.ShipDestroyed ();
		else
			refGameControl.GameOver ();
	}
	
	//==================================================================================================
}
