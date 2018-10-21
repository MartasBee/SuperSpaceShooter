using UnityEngine;
using System.Collections;


//==============================================================================
// CORE SYSTEMS
//==============================================================================
[System.Serializable]
public class CoreSystems
{
	//==========================================================================
	// LIFE
	//==========================================================================
	[System.Serializable]
	public class Life
	{
		private int actualLifes;

		private static int lifesMin =  0;
		private static int lifesMax = 10;

		//======================================================================

		public Life ()
		{
			Init (3, 0, 10);
		}

		/**
		 * @param lInit
		 */
		public bool Init (int lInit)
		{
			return Init (lInit, 0, 10);
		}
		/**
		 * @param lInit
		 * @param lMin
		 * @param lMax
		 */
		public bool Init (int lInit, int lMin, int lMax)
		{
			lifesMin = lMin;
			lifesMax = lMax;
			
			if (lifesMax < lifesMin)
			{
				int tmp  = lifesMax;
				lifesMax = lifesMin;
				lifesMin = tmp;
			}
			if (lifesMin < 0)
				lifesMin = 0;
			
			actualLifes = lInit;
			
			if (actualLifes < lifesMin)
				actualLifes = lifesMin;
			else if (actualLifes > lifesMax)
				actualLifes = lifesMax;
			
			return true;
		}

		/**
		 * 
		 */
		public int GetLifes ()
		{
			return actualLifes;
		}
		/**
		 * 
		 */
		public bool IsAlive ()
		{
			return (actualLifes > lifesMin);
		}

		/**
		 * 
		 */
		public int AddLife ()
		{
			actualLifes += 1;
			if (actualLifes > lifesMax)
				actualLifes = lifesMax;
			return actualLifes;
		}
		/**
		 * 
		 */
		public int ConsumeLife ()
		{
			actualLifes -= 1;
			if (actualLifes < lifesMin)
				actualLifes = lifesMin;
			return actualLifes;
		}
	}

	//==========================================================================
	// HEALTH
	//==========================================================================
	[System.Serializable]
	public class Health
	{
		private float actualHealth;
		
		// pre-set values for Unity Editor
		private static float healthMin 	=   0.0f;
		private static float healthMax 	= 100.0f;

		//======================================================================

		public Health ()
		{
			Init (100.0f, 0.0f, 100.0f);
		}
		/**
	 	 * @param hInit
	 	 */
		public bool Init (float hInit)
		{
			return Init (hInit, 0.0f, 100.0f);
		}
		/**
	 	 * @param hInit
		 * @param hMin
		 * @param hMax
	 	 */
		public bool Init (float hInit, float hMin, float hMax)
		{
			healthMin = hMin;
			healthMax = hMax;
			
			if (healthMax < healthMin)
			{
				float tmp = healthMax;
				healthMax = healthMin;
				healthMin = tmp;
			}
			if (healthMin < 0.0f)
				healthMin = 0.0f;
			
			actualHealth = hInit;
			
			if (actualHealth < healthMin)
				actualHealth = healthMin;
			else if (actualHealth > healthMax)
				actualHealth = healthMax;
			
			return true;
		}

		/**
		 * @return actual health
		 */
		public float GetHealth ()
		{
			return actualHealth;
		}
		/**
		 * 
		 */
		public bool IsMaxHealth ()
		{
			return (actualHealth == healthMax);
		}
		/**
		 * @return is actual health highee then minumum health ?
		 */
		public bool  IsAlive ()
		{
			return (actualHealth > healthMin);
		}

		/**
		 * MANUAL health recharge method
		 * 
		 * @param recharge bonus to health
		 * @return actual health after recharge
		 */
		public float RechargeHealth (float recharge)
		{
			actualHealth = actualHealth + (recharge < 0 ? -recharge : recharge);
			if (actualHealth > healthMax)
				actualHealth = healthMax;
			
			return actualHealth;
		}
		
		/**
		 * Just consume health
		 * 
		 * @param consume = <0, inf)
		 * @return how much health remains
		 */
		public float ConsumeHealth (float consume)
		{
			actualHealth = actualHealth + (consume < 0 ? consume : -consume);
			if (actualHealth < healthMin)
				actualHealth = healthMin;
			
			return actualHealth;
		}
		/**
		 * Consume health and return if ship is alive
		 * 
		 * @param consume = <0, inf)
		 * @return IsShipAlive
		 */
		public bool IsAliveAfterConsumeHealth (float consume)
		{
			actualHealth = actualHealth + (consume < 0 ? consume : -consume);
			if (actualHealth <= healthMin)
			{
				actualHealth = healthMin;
				return false;
			}
			else
				return true;
		}
	}

	//==========================================================================
	// SHIELD
	//==========================================================================
	[System.Serializable]
	public class Shield
	{
		private float actualShield;
		
		// pre-set values for Unity Editor
		private static float shieldMin 	=   0.0f;
		private static float shieldMax 	= 100.0f;

		//======================================================================

		public Shield ()
		{
			Init (100.0f, 0.0f, 100.0f);
		}
		/**
	 	 * @param sInit
	 	 */
		public bool Init (float sInit)
		{
			return Init (sInit, 0.0f, 100.0f);
		}
		/**
	 	 * @param sInit
	 	 * @param sMin
	 	 * @param sMax
	 	 */
		public bool Init (float sInit, float sMin, float sMax)
		{
			shieldMin = sMin;
			shieldMax = sMax;
			
			if (shieldMax < shieldMin)
			{
				float tmp = shieldMax;
				shieldMax = shieldMin;
				shieldMin = tmp;
			}
			if (shieldMin < 0.0f)
				shieldMin = 0.0f;
			
			actualShield = sInit;
			
			if (actualShield < shieldMin)
				actualShield = shieldMin;
			else if (actualShield > shieldMax)
				actualShield = shieldMax;
			
			return true;
		}

		/**
		 * @return actual shield status
		 */
		public float GetShield ()
		{
			return actualShield;
		}
		/**
		 * 
		 */
		public bool IsMaxShield ()
		{
			return (actualShield == shieldMax);
		}
		/**
		 * @return is actual shield status more then minimum ?
		 */
		public bool  IsShieldActive ()
		{
			return (actualShield > shieldMin);
		}
		/**
		 * MANUAL shield recharge
		 * 
		 * @param recharge bonus to shields
		 * @return shield status
		 */
		public float RechargeShield (float recharge)
		{
			actualShield = actualShield + (recharge < 0 ? -recharge : recharge);
			if (actualShield > shieldMax)
				actualShield = shieldMax;
			
			return actualShield;
		}
		
		/**
		 * Method consumes shields after a hit. Return part of hit energy, which was not covered by shield
		 * 
		 * @param consume how much of shield will be consumed: <0, inf)
		 * @return how much of shot energy was NOT covered by shield and should by drawn from health
		 */
		public float ConsumeShield (float consume)
		{
			actualShield = actualShield + (consume < 0 ? consume : -consume);
			
			if (actualShield <= shieldMin)
			{
				// how much shot energy is not covered by shield
				float tmp = (actualShield < 0 ? -actualShield : actualShield) + shieldMin;
				
				actualShield = shieldMin;
				return tmp;
			}
			else
				return 0.0f;
		}
	}

	//==========================================================================
	// ENERGY
	//==========================================================================
	[System.Serializable]
	public class Energy
	{
		private float actualEnergy;
		private float nextRecharge = 0.0f;
		private float lastConsume  = 0.0f;

		//======================================================================
		// E RECHARGE TYPES
		//======================================================================
		public enum eRechargeTypes
		{
			NoRecharge 	= 0,
			Custom		= 1,
			VerySlow	= 2,
			Slow,
			Medium,
			Fast,
			SuperFast
		}

		//======================================================================

		public eRechargeTypes actualRechargeType
		{
			get;
			private set;
		}
		
		// pre-set values for Unity Editor
		private static float energyMin 	=   0.0f;
		private static float energyMax 	= 100.0f;
		
		// how much energy will be recharged
		private float rechargeCapacity;
		// how often will be the energy recharged
		private float rechargeRate;
		// how long have to wait (from last energy consumption time) before recharge can begin
		private float rechargeWait;

		//======================================================================
		
		public Energy ()
		{
			Init (eRechargeTypes.Medium, 100.0f, 0.0f, 100.0f);
			
			nextRecharge = 0.0f;
			lastConsume  = 0.0f;
		}
		/**
	 	 * @param reType
	 	 * @param eInit
	 	 */
		public bool Init (eRechargeTypes reType, float eInit)
		{
			return Init (reType, eInit, 0.0f, 100.0f);
		}
		/**
	 	 * @param reType
	 	 * @param eInit
	 	 * @param eMin
	 	 * @param eMax
	 	 */
		public bool Init (eRechargeTypes reType, float eInit, float eMin, float eMax)
		{
			energyMin = eMin;
			energyMax = eMax;
			
			if (energyMax < energyMin)
			{
				float tmp = energyMax;
				energyMax = energyMin;
				energyMin = tmp;
			}
			if (energyMin < 0.0f)
				energyMin = 0.0f;
			
			actualEnergy = eInit;
			
			if (actualEnergy < energyMin)
				actualEnergy = energyMin;
			else if (actualEnergy > energyMax)
				actualEnergy = energyMax;
			
			return SetRechargeType (reType);
		}
		
		
		/**
		 * @param reType
		 */
		public bool SetRechargeType (eRechargeTypes reType)
		{
			switch (reType)
			{
			case eRechargeTypes.NoRecharge:
				rechargeCapacity 	=    0.0f;
				rechargeRate 		= 1000.0f;
				rechargeWait		= 1000.0f;

				lastConsume  		=    0.0f;	// all energy was consumed in moment of creation
				nextRecharge		= 1000.0f;	// first recharge is allowed after 1000 seconds of object creation
				break;

				// 2 units per second
			case eRechargeTypes.VerySlow:
				rechargeCapacity 	=    1.0f;
				rechargeRate 		=    0.33f;
				rechargeWait		=    5.0f;
				break;

				// 3 units per second
			case eRechargeTypes.Slow:
				rechargeCapacity 	=    1.0f;
				rechargeRate 		=    0.2f;
				rechargeWait		=    3.0f;
				break;

				// 5 units per second
			case eRechargeTypes.Medium:
				rechargeCapacity 	=    1.0f;
				rechargeRate 		=    0.1f;
				rechargeWait		=    2.0f;
				break;

				// 10 units per second
			case eRechargeTypes.Fast:
				rechargeCapacity 	=    1.0f;
				rechargeRate 		=    0.05f;
				rechargeWait		=    1.0f;
				break;

				// 20 units per second
			case eRechargeTypes.SuperFast:
				rechargeCapacity 	=    1.0f;
				rechargeRate 		=    0.02f;
				rechargeWait		=    1.0f;
				break;
				
			default:
				Debug.LogWarning ("PLAYER SETT >> Unknown recharge >> Current recharge: " + actualRechargeType);
				return false;
			}
			
			actualRechargeType = reType;
			return true;
		}
		/**
		 * @param howMuch
		 * @param howfast
		 * @param waiting
		 */
		public bool SetCustomRecharge (float howMuch, float howFast, float waiting)
		{
			bool anyChange = false;
			
			if (howMuch >= 0.0f)
			{
				rechargeCapacity = howMuch;
				anyChange 		 = true;
			}
			if (howFast >= 0.0f)
			{
				rechargeRate	 = howFast;
				anyChange 		 = true;
			}
			if (waiting >= 0.0f)
			{
				rechargeWait	 = waiting;
				anyChange 		 = true;
			}
			
			if (anyChange)
				actualRechargeType = eRechargeTypes.Custom;
			
			return anyChange;
		}
		/**
		 * @param level
		 * @param wave
		 */
		public virtual bool CustomizeRecharge (int level, int wave)
		{
			bool anyChange = false;
			
			return anyChange;
		}
		

		/**
		 * @return actual energy status
		 */
		public float GetEnergy ()
		{
			return actualEnergy;
		}
		/**
		 * @return is actual energy higher then minimum ?
		 */
		public bool HasAnyEnergy ()
		{
			return (actualEnergy > energyMin);
		}
		
		
		/**
		 * AUTO recharge energy method
		 * 
		 * @return actualEnergy
		 */
		public float RechargeEnergy ()
		{
			// recharge is possible even if these conditions are true:
			// 1) aktualny cas je vacsi, ako (posledne dobitie + rychlost dobijania)
			// 2) energia nebola konzumovana aspon po dobu rechargeWait
			if ((Time.time > nextRecharge) && (Time.time > (lastConsume + rechargeWait)))
			{
				actualEnergy = actualEnergy + rechargeCapacity;
				if (actualEnergy > energyMax)
					actualEnergy = energyMax;
				
				// set up NEXT RECHARGE TIME
				nextRecharge = Time.time + rechargeRate;
			}
			return actualEnergy;
		}
		/**
		 * MANUAL recharge energy method
		 * 
		 * @param recharge amount of energy for one-time recharge
		 * @return actualEnergy
		 */
		public float RechargeEnergy (float recharge)
		{
			if (recharge == 0.0f)
				return actualEnergy;
			
			actualEnergy = actualEnergy + (recharge < 0 ? -recharge : recharge);
			if (actualEnergy > energyMax)
				actualEnergy = energyMax;
			
			// set up NEXT RECHARGE TIME
			nextRecharge = Time.time + rechargeRate;
			
			return actualEnergy;
		}
		
		
		/**
		 * Method checks, if ship has enough energy for some energy drawing action (like shoting)
		 * This method have to be called before energy will be consumed (!!)
		 * 
		 * @param costs
		 */
		public bool HasEnoughEnergy (float costs)
		{
			if ((actualEnergy + (costs < 0 ? costs : -costs)) >= energyMin)
				return true;
			else
				return false;
		}
		/**
		 * You HAVE TO call HasEnoughEnergy (consume) FIRST
		 * 
		 * @param consume
		 */
		public void ConsumeEnergy (float consume)
		{
			actualEnergy = actualEnergy + (consume < 0 ? consume : -consume);
			lastConsume  = Time.time;
		}
		
		
		/**
		 * Method will check, if there is enough energy, and if so,
		 * method will immediatelly consume the energy.
		 * 
		 * @param consume
		 * @return if energy was consumed
		 */
		public bool ConsumeEnergyIfCan (float consume)
		{
			if ((actualEnergy + (consume < 0 ? consume : -consume)) >= energyMin)
			{
				actualEnergy = actualEnergy + (consume < 0 ? consume : -consume);
				lastConsume  = Time.time;
				return true;
			}
			else
				return false;
		}
	}

	//==========================================================================

	public Life		nesLifes;
	public Health   nesHealth;
	public Shield	nesShield;
	public Energy   nesEnergy;
	
	public float 	collisionDamageRate = 0.0f;

	//==========================================================================

	public CoreSystems ()
	{
		// generates strange errors in unity editor
		//Init (100.0f, 0.0f, 80.0f, CoreSystems.Energy.eRechargeTypes.Medium);

		nesLifes  = new Life ();
		nesHealth = new Health ();
		nesShield = new Shield ();
		nesEnergy = new Energy ();

		collisionDamageRate = 10.0f;
	}

	/**
	 * @param initHealth
	 * @param initShield
	 * @param initEnergy
	 */
	public bool Init (int 	initLifes,
	                  float initHealth,
	                  float initShield,
	                  float initEnergy, Energy.eRechargeTypes eRecharge)
	{
		bool ret = true;

		ret = nesLifes.Init  (initLifes)  && ret;
		ret = nesHealth.Init (initHealth) && ret;
		ret = nesShield.Init (initShield) && ret;
		ret = nesEnergy.Init (eRecharge, initEnergy) && ret;	

		return ret;
	}
	
	/**
	 * Damages caused by other players when THIS is hit
	 * 
	 * @param damage
	 */
	public bool Hit (float damage)
	{
		float restOfHit = nesShield.ConsumeShield (damage);
		bool  isAlive   = nesHealth.IsAliveAfterConsumeHealth (restOfHit);
		if (!isAlive)
			nesLifes.ConsumeLife ();

		return isAlive;
	}
	/**
	 * 
	 */
	public bool TakeShipsLife ()
	{
		nesLifes.ConsumeLife ();
		return nesLifes.IsAlive ();
	}
	/**
	 * Damages caused by collision to other objects
	 * 
	 * MASS x VELOCITY x COLLISION_DAMAGE_RATE
	 */
	public virtual float GetCollisionDamageRate ()
	{
		return collisionDamageRate;
	}	
}


//==============================================================================
// WEAPON SYSTEMS
//==============================================================================
[System.Serializable]
public class WeaponSystems
{
	//==========================================================================
	// E WEAPON TYPES
	//==========================================================================
	public enum eWeaponTypes
	{
		NoWeapon 		= 0,
		Custom 			= 1,
		ForwardBasic	= 2,
		ForwardBetter,
		ForwardAdvanced,
		DirectionalBasic,
		//DirectionalAdvanced,
		//AllAroundBasic,
		//AllAroundAdvanced,
		
		END_WEAPON_TYPES
	}
	
	//==========================================================================
	
	public eWeaponTypes actualWeaponType
	{
		get;
		private set;
	}
	public Weapon.eAmmoTypes actualAmmoType
	{
		get;
		private set;
	}
	public bool automaticShoting
	{
		get;
		private set;
	}
	
	private WeaponController[] refShipAllWeapons;
	private CoreSystems.Energy refShipEnergy;
	
	//==========================================================================
	
	/**
	 * @param weaponType
	 * @param ammoType
	 */
	public bool Init (eWeaponTypes weaponType, Weapon.eAmmoTypes ammoType, bool autoShoting)
	{
		actualWeaponType = weaponType;
		actualAmmoType	 = ammoType;
		automaticShoting = autoShoting;
		
		return true;
	}
	
	/**
	 * @param AllWeapons
	 */
	public void SetReferenceShipAllWeapons (ref WeaponController[] AllWeapons)
	{
		refShipAllWeapons = AllWeapons;
	}
	
	/**
	 * @param Energy
	 */
	public void SetReferenceShipCoreEnergy (ref CoreSystems.Energy Energy)
	{
		refShipEnergy = Energy;
	}
	
	/**
	 *  
	 */
	public bool ActivateWeapons ()
	{
		bool weapRet = true;
		
		switch (actualWeaponType)
		{
		case eWeaponTypes.NoWeapon:		
			for (int i = 0; i < refShipAllWeapons.Length; i++)
			{
				if (!refShipAllWeapons[i].nesWeapon.Init (Weapon.eAmmoTypes.NoShoting, ref refShipEnergy))
				{
					weapRet = false;
					
					Debug.LogError ("WeaponSystems >> eWeaponTypes.NoWeapon >> Weapon `"
					                + refShipAllWeapons[i].name
					                + " (position: "
					                + refShipAllWeapons[i].weaponPosision
					                + ")` was NOT initialized !!");
				}
				
				refShipAllWeapons[i].gameObject.SetActive (false);
				refShipAllWeapons[i].autoShot = false;
			}
			break;
			
		case eWeaponTypes.Custom:
			break;
			
		case eWeaponTypes.ForwardBasic:
			for (int i = 0; i < refShipAllWeapons.Length; i++)
			{
				Weapon.eAmmoTypes tmp = Weapon.eAmmoTypes.NoShoting;
				
				if (refShipAllWeapons[i].weaponPosision == "FM")
					tmp = actualAmmoType;
				
				if (!refShipAllWeapons[i].nesWeapon.Init (tmp, ref refShipEnergy))
				{
					weapRet = false;
					
					Debug.LogError ("WeaponSystems >> eWeaponTypes.ForwardBasic >> Weapon `"
					                + refShipAllWeapons[i].name
					                + " (position: "
					                + refShipAllWeapons[i].weaponPosision
					                + ")` was NOT initialized !!");
					
					refShipAllWeapons[i].gameObject.SetActive (false);
					refShipAllWeapons[i].autoShot = false;
				}
				else
				{
					if ((tmp > Weapon.eAmmoTypes.Custom) && (tmp < Weapon.eAmmoTypes.END_AMMO_TYPES))
						refShipAllWeapons[i].gameObject.SetActive (true);
					else
						refShipAllWeapons[i].gameObject.SetActive (false);
					
					refShipAllWeapons[i].autoShot = automaticShoting;
				}
			}
			break;
			
		case eWeaponTypes.ForwardBetter:
			for (int i = 0; i < refShipAllWeapons.Length; i++)
			{
				Weapon.eAmmoTypes tmp = Weapon.eAmmoTypes.NoShoting;
				
				if (refShipAllWeapons[i].weaponPosision == "FL"
				    || refShipAllWeapons[i].weaponPosision == "FR")
					tmp = actualAmmoType;
				
				if (!refShipAllWeapons[i].nesWeapon.Init (tmp, ref refShipEnergy))
				{
					weapRet = false;
					
					Debug.LogError ("WeaponSystems >> eWeaponTypes.ForwardBetter >> Weapon `"
					                + refShipAllWeapons[i].name
					                + " (position: "
					                + refShipAllWeapons[i].weaponPosision
					                + ")` was NOT initialized !!");
					
					refShipAllWeapons[i].gameObject.SetActive (false);
					refShipAllWeapons[i].autoShot = false;
				}
				else
				{
					if ((tmp > Weapon.eAmmoTypes.Custom) && (tmp < Weapon.eAmmoTypes.END_AMMO_TYPES))
						refShipAllWeapons[i].gameObject.SetActive (true);
					else
						refShipAllWeapons[i].gameObject.SetActive (false);
					
					refShipAllWeapons[i].autoShot = automaticShoting;
				}
			}
			break;
			
		case eWeaponTypes.ForwardAdvanced:
			for (int i = 0; i < refShipAllWeapons.Length; i++)
			{
				Weapon.eAmmoTypes tmp = Weapon.eAmmoTypes.NoShoting;
				
				if (refShipAllWeapons[i].weaponPosision == "FM"
				    || refShipAllWeapons[i].weaponPosision == "FL"
				    || refShipAllWeapons[i].weaponPosision == "FR")
					tmp = actualAmmoType;
				
				if (!refShipAllWeapons[i].nesWeapon.Init (tmp, ref refShipEnergy))
				{
					weapRet = false;
					
					Debug.LogError ("WeaponSystems >> eWeaponTypes.ForwardAdvanced >> Weapon `"
					                + refShipAllWeapons[i].name
					                + " (position: "
					                + refShipAllWeapons[i].weaponPosision
					                + ")` was NOT initialized !!");
					
					refShipAllWeapons[i].gameObject.SetActive (false);
					refShipAllWeapons[i].autoShot = false;
				}
				else
				{
					if ((tmp > Weapon.eAmmoTypes.Custom) && (tmp < Weapon.eAmmoTypes.END_AMMO_TYPES))
						refShipAllWeapons[i].gameObject.SetActive (true);
					else
						refShipAllWeapons[i].gameObject.SetActive (false);
					
					refShipAllWeapons[i].autoShot = automaticShoting;
				}
			}
			break;
			
		case eWeaponTypes.DirectionalBasic:
			for (int i = 0; i < refShipAllWeapons.Length; i++)
			{
				Weapon.eAmmoTypes tmp = Weapon.eAmmoTypes.NoShoting;
				
				if (refShipAllWeapons[i].weaponPosision == "FM"
				    || refShipAllWeapons[i].weaponPosision == "FL"
				    || refShipAllWeapons[i].weaponPosision == "FR")
				{
					tmp = actualAmmoType;
				}
				else if (refShipAllWeapons[i].weaponPosision == "DL"
				         || refShipAllWeapons[i].weaponPosision == "DR")
				{
					if (actualAmmoType > Weapon.eAmmoTypes.Toy)
						tmp = (actualAmmoType - 1);
					else
						tmp = actualAmmoType;
				}
				
				if (!refShipAllWeapons[i].nesWeapon.Init (tmp, ref refShipEnergy))
				{
					weapRet = false;
					
					Debug.LogError ("WeaponSystems >> eWeaponTypes.DirectionalAdvanced >> Weapon `"
					                + refShipAllWeapons[i].name
					                + " (position: "
					                + refShipAllWeapons[i].weaponPosision
					                + ")` was NOT initialized !!");
					
					refShipAllWeapons[i].gameObject.SetActive (false);
					refShipAllWeapons[i].autoShot = false;
				}
				else
				{
					if ((tmp > Weapon.eAmmoTypes.Custom) && (tmp < Weapon.eAmmoTypes.END_AMMO_TYPES))
						refShipAllWeapons[i].gameObject.SetActive (true);
					else
						refShipAllWeapons[i].gameObject.SetActive (false);
					
					refShipAllWeapons[i].autoShot = automaticShoting;
				}
			}
			break;
			
		default:
			break;
		}
		
		if (!weapRet)
		{
			Debug.LogError ("WeaponSystems >> At least one weapon was not initialized properly");
		}
		
		return weapRet;
	}
}


//==============================================================================
// ENGINE SYSTEMS
//==============================================================================
[System.Serializable]
public class EngineSystems
{
	public bool  physicalMoves	= false;
	public float shipDrag		=  0.0f;
	
	public float actualSpeed 	=  0.0f;
	public float actualTilt	 	=  4.0f;	// naklon lode pre pohyboch do stran
	
	public float randomRotation =  0.0f;
	
	private float speedLimitMin =  0.0f;
	private float speedLimitMax =  0.0f;
	
	//==========================================================================
	
	public float 	dodge;
	public float 	smoothing;
	public Vector2 	startWait;
	public Vector2 	maneuverTime;
	public Vector2 	maneuverWait;
	
	public float 	targetManeuver;
	
	//==========================================================================
	
	public EngineSystems ()
	{
		startWait 	 = new Vector2 ();
		maneuverTime = new Vector2 ();
		maneuverWait = new Vector2 ();
	}
	
	/**
	 * Standard movement --> method is used for player ship
	 */
	public bool Init (float speedInit, float drag, bool phyMove)
	{
		physicalMoves  = phyMove;
		shipDrag 	   = drag;
		
		randomRotation = 0.0f;
		
		speedLimitMin  = 0.0f;
		speedLimitMax  = 0.0f;
		
		actualSpeed    = speedInit;
		actualTilt     = 4.0f;
		
		//-----------------------------------------------------
		dodge		= 0.0f;
		smoothing	= 0.0f;
		
		startWait.x = 0.0f;
		startWait.y = 0.0f;
		
		maneuverTime.x = 0.0f;
		maneuverTime.y = 0.0f;
		
		maneuverWait.x = 0.0f;
		maneuverWait.y = 0.0f;
		//-----------------------------------------------------
		
		return true;
	}
	/**
	 * RAMDOM generated speed between values of:
	 * @param speedMin
	 * @param speedMax
	 * --> Method is used for enemy ship
	 */
	public bool Init (float speedMin, float speedMax, float drag, bool phyMove)
	{
		physicalMoves  = phyMove;
		shipDrag	   = drag;
		
		randomRotation = 0.0f;
		
		speedLimitMin  = speedMin;
		speedLimitMax  = speedMax;
		
		if (speedLimitMax < speedLimitMin)
		{
			float tmp     = speedLimitMax;
			speedLimitMax = speedLimitMin;
			speedLimitMin = tmp;
		}
		if (speedLimitMin < 0.1f)
			speedLimitMin = 0.1f;
		
		actualSpeed = Random.Range (speedLimitMin, speedLimitMax);
		
		//-----------------------------------------------------
		actualTilt  = 10.0f;
		dodge		= 5.0f;
		smoothing	= 7.5f;
		
		startWait.x = 0.5f;
		startWait.y = 1.0f;
		
		maneuverTime.x = 1.0f;
		maneuverTime.y = 2.0f;
		
		maneuverWait.x = 1.0f;
		maneuverWait.y = 2.0f;
		//-----------------------------------------------------
		
		return true;
	}
	/**
	 * RANDOM ROTATOR --> used for asteroids
	 */
	public bool Init (float speedMin, float speedMax, float rotator, float drag, bool phyMove)
	{
		physicalMoves  = phyMove;
		shipDrag	   = drag;
		
		randomRotation = rotator;
		
		speedLimitMin  = speedMin;
		speedLimitMax  = speedMax;
		
		if (speedLimitMax < speedLimitMin)
		{
			float tmp     = speedLimitMax;
			speedLimitMax = speedLimitMin;
			speedLimitMin = tmp;
		}
		
		actualSpeed = Random.Range (speedLimitMin, speedLimitMax);
		
		//-----------------------------------------------------
		actualTilt  = 10.0f;
		dodge		= 5.0f;
		smoothing	= 7.5f;
		
		startWait.x = 0.5f;
		startWait.y = 1.0f;
		
		maneuverTime.x = 1.0f;
		maneuverTime.y = 2.0f;
		
		maneuverWait.x = 1.0f;
		maneuverWait.y = 2.0f;
		//-----------------------------------------------------
		
		return true;
	}
}


//==============================================================================
// FONT SIZER
//==============================================================================
[System.Serializable]
public static class DisplayMetricsUtil
{
    public enum ResolutionType
    {
        ldpi,
        mdpi,
        hdpi,
        xhdpi
    }

    private const float DEFAULT_DPI = 160.0f;

    private static bool isScreenSizeInitialized = false;

    private static Rect ScreenSize;

    //==========================================================================

    public static Vector2   DpToPixel (this Vector2 vector)
    {
        return new Vector2(vector.x.DpToPixel(), vector.y.DpToPixel());
    }

    public static Vector3   DpToPixel (this Vector3 vector)
    {
        return new Vector3(vector.x.DpToPixel(), vector.y.DpToPixel(), vector.z.DpToPixel());
    }

    public static Rect      DpToPixel (this Rect rect)
    {
        return new Rect(rect.x.DpToPixel(), rect.y.DpToPixel(), rect.width.DpToPixel(), rect.height.DpToPixel());
    }

    public static int       DpToPixel (this int dp)
    {
        // Convert the dps to pixels
        return (int)(dp * GetScale() + 0.5f);
    }

    public static int       DpToPixel (this float dp)
    {
        // Convert the dps to pixels
        return (int)(dp * GetScale() + 0.5f);
    }

    public static int       PixelToDp (this int px)
    {
        // Convert the pxs to dps
        return (int)(px / GetScale() - 0.5f);
    }

    public static int       PixelToDp (this float px)
    {
        // Convert the pxs to dps
        return (int)(px / GetScale() - 0.5f);
    }

    //==========================================================================

    public static GUIStyle  DpToPixel (this GUIStyle style)
    {
        GUIStyle stylePx        = new GUIStyle(style);

        stylePx.border          = stylePx.border.DpToPixel();
        stylePx.padding         = stylePx.padding.DpToPixel();
        stylePx.margin          = stylePx.margin.DpToPixel();
        stylePx.overflow        = stylePx.overflow.DpToPixel();
        stylePx.contentOffset   = stylePx.contentOffset.DpToPixel();
        stylePx.fixedWidth      = stylePx.fixedWidth.DpToPixel();
        stylePx.fixedHeight     = stylePx.fixedHeight.DpToPixel();
        stylePx.fontSize        = stylePx.fontSize.DpToPixel();

        return stylePx;
    }

    //==========================================================================

    public static RectOffset DpToPixel (this RectOffset rectOffset)
    {
        return new RectOffset(
            rectOffset.left.DpToPixel(),
            rectOffset.right.DpToPixel(),
            rectOffset.top.DpToPixel(),
            rectOffset.bottom.DpToPixel());
    }


    public static Rect ScreenSizeDpUnit ()
    {
        if (!isScreenSizeInitialized)
        {
            ScreenSize = new Rect (0, 0, Screen.width.PixelToDp(), Screen.height.PixelToDp());

            isScreenSizeInitialized = true;
        }

        return ScreenSize;
    }

    //==========================================================================

    public static ResolutionType GetResolutionType ()
    {
        float scale = GetScale ();

        ResolutionType res;

        //http://developer.android.com/guide/practices/screens_support.html
        if (scale > 1.5f)
        {
            res = DisplayMetricsUtil.ResolutionType.xhdpi;
        }
        else if (scale > 1f)
        {
            res = DisplayMetricsUtil.ResolutionType.hdpi;
        }
        else if (scale > 0.75f)
        {
            res = DisplayMetricsUtil.ResolutionType.mdpi;
        }
        else
        {
            res = DisplayMetricsUtil.ResolutionType.ldpi;
        }

        return res;
    }

    //==========================================================================

    private static float GetDPI ()
    {
        return Screen.dpi == 0 ? DEFAULT_DPI : Screen.dpi;
    }

    private static float GetScale ()
    {
        return GetDPI() / DEFAULT_DPI;
    }
}

//==============================================================================
//==============================================================================

public class GameSystems : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{}
}
