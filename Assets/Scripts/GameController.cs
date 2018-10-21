using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
	//==================================================================================================

	public GameObject 	refPlayerShipDefault;
	public GameObject 	refPlayerShipAK5;
	public GameObject	refHazardType1;
	public GameObject	refHazardType2;
	public GameObject	refHazardType3;
	public GameObject 	refEnemyShipUWingBasic;
	public GameObject 	refEnemyShipUWingMedium;

	public GameObject   refBonusHealth;
	public GameObject   refBonusEnergy;
	public GameObject   refBonusShield;

	public GameObject	refWeaponForwardBasic;
	public GameObject	refWeaponForwardBetter;
	public GameObject	refWeaponForwardAdvanced;
	public GameObject	refWeaponDirectionalBasic;
	public GameObject	refFireModeAdvanced;
	public GameObject	refFireModeHardcore;
	public GameObject	refFireModeArmagedon;

	// init posision of each enemy
	public Vector3 		spawnValues;
	// how many enemies will be in each wave
	public int 			hazardCount;

	// waitings: on start of game, between instance of enemies and between waves
	public float 		startWait;
	public float 		spawnWait;
	public int	 		wavesWaitCount;
	public float		gameOverWait;
	public int			shipDestroyedWaitCount;

	// texts tu be drawn
	public GUIText 		refTextGameOver;
	public GUIText		refTextBonus;

	public GUIText 		refTextWave;
	public GUIText 		refTextScore;
	public GUIText  	refTextLifes;
	public GUIText 		refTextHealth;
	public GUIText		refTextShield;
	public GUIText 		refTextEnergy;

	private bool 		flagGameOver;
	private bool 		flagRestartLevel;
	private bool		flagShipDestroyed;

	private int 		levelCounterWave;

	private GameObject 	refPlayerInstance;

	//==================================================================================================

	void Awake ()
	{
		flagGameOver 	  = false;
		flagRestartLevel  = false;
		flagShipDestroyed = false;
	}

	void Start ()
	{
		refTextLifes.text	= "";
		refTextHealth.text 	= "";
		refTextShield.text 	= "";
		refTextEnergy.text 	= "";
		refTextScore.text	= "";
		refTextWave.text	= "";
		refTextBonus.text	= "";

		levelCounterWave  	= RuntimeContext.GetInst ().nesActLevel.levelActWave;

		InitLevel ();	
	}

	//==================================================================================================

	/**
	 * 
	 */
	bool InitLevel ()
	{
		if (flagShipDestroyed)
			RuntimeContext.GetInst ().nesActPlayer.ReinitDestroyedShip ();

		flagGameOver 	  = false;
		flagRestartLevel  = false;
		flagShipDestroyed = false;

		refTextGameOver.text = "";

		GameObject tmpShipObj;
		switch (RuntimeContext.GetInst ().nesActPlayer.actShip)
		{
		case ePlayerShips.AK5:
			tmpShipObj = refPlayerShipAK5;
			break;

		default:
			tmpShipObj = refPlayerShipDefault;
			break;
		}

		refPlayerInstance = Instantiate (tmpShipObj, new Vector3 (0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
		if (refPlayerInstance != null)
		{
			PlayerController playCtrl = refPlayerInstance.GetComponent <PlayerController>();
			if (playCtrl == null)
			{
				Debug.LogError ("LEVEL >> FAILED to obtain reference to PlayerController Class");
				Application.LoadLevel (eGameLevels.Screen_MainMenu.ToString ());
			}
			
			bool playerOK = playCtrl.InitShip (RuntimeContext.GetInst ().nesActPlayer.actLifes,
			                                   RuntimeContext.GetInst ().nesActPlayer.actHealth,
			                                   RuntimeContext.GetInst ().nesActPlayer.actShield,
			                                   RuntimeContext.GetInst ().nesActPlayer.actEnergy,
			                                   RuntimeContext.GetInst ().nesActPlayer.actEnergyType,
			                                   RuntimeContext.GetInst ().nesActPlayer.actWeaponType,
			                                   RuntimeContext.GetInst ().nesActPlayer.actAmmoType);
			if (playerOK)
				playCtrl.gameObject.SetActive (true);
			else
			{
				Debug.LogError ("LEVEL >> FAILED to init player");
				Application.LoadLevel (eGameLevels.Screen_MainMenu.ToString ());
			}
		}
		else
		{
			Debug.LogError ("LEVEL >> FAILED to instantiate player GameObject");
			Application.LoadLevel (eGameLevels.Screen_MainMenu.ToString ());
		}

		StartCoroutine (SpawnWaves ());

		return true;
	}

	/**
	 * 
	 */
	void StoreRuntimePlayer ()
	{
		PlayerController playCtrl = refPlayerInstance.GetComponent <PlayerController>();
		if (playCtrl == null)
		{
			Debug.LogError ("LEVEL >> FAILED to obtain reference to PlayerController --> player ship will not be saved...");
			return;
		}

		Debug.Log ("LEVEL >> Storing current player ship's status: "
		           + "\nLifes: "  + playCtrl.nesCoreSys.nesLifes.GetLifes ()
		           + "\nHealth: " + playCtrl.nesCoreSys.nesHealth.GetHealth ()
		           + "\nShield: " + playCtrl.nesCoreSys.nesShield.GetShield ()
		           + "\nEnergy: " + playCtrl.nesCoreSys.nesEnergy.GetEnergy ()
		           + "\nEnergy Recharge: " 	+ playCtrl.nesCoreSys.nesEnergy.actualRechargeType
		           + "\nWeapon Type: "  	+ playCtrl.nesWeapSys.actualWeaponType
		           + "\nAmmo Type: " 		+ playCtrl.nesWeapSys.actualAmmoType);

		// store current state of ship systems for next-time loading
		RuntimeContext.GetInst ().StorePlayerCore (playCtrl.nesCoreSys.nesLifes.GetLifes (),
		                                           playCtrl.nesCoreSys.nesHealth.GetHealth (),
		                                           playCtrl.nesCoreSys.nesShield.GetShield (),
		                                           playCtrl.nesCoreSys.nesEnergy.GetEnergy (),
		                                           playCtrl.nesCoreSys.nesEnergy.actualRechargeType);

		RuntimeContext.GetInst ().StorePlayerWeapon(playCtrl.nesWeapSys.actualWeaponType,
		                                            playCtrl.nesWeapSys.actualAmmoType);
	}
	/**
	 * 
	 */
	void StoreRuntimeLevel ()
	{
		RuntimeContext.GetInst ().nesActLevel.levelActWave = levelCounterWave;
	}

	//==================================================================================================

	void Update ()
	{
		if (flagRestartLevel)
		{
#if UNITY_ANDROID
#else
			if (Input.GetKeyDown (KeyCode.R))
			{
				StoreRuntimeLevel ();
				Statistics.GetInst ().ActualizeTotalScore ();
				Statistics.GetInst ().ActualizeBestScoreList ();
				GamePlaySave.GetInst ().ClearSavedScoreProgress ();

				if (GamePlaySave.GetInst ().LoadNewGame (RuntimeContext.GetInst ().nesActLevel.levelName))
					Application.LoadLevel (RuntimeContext.GetInst ().nesActLevel.levelName);
			}
#endif
		}
#if UNITY_ANDROID
		if (Input.GetKey (KeyCode.Escape))
#else
		if (Input.GetKeyDown (KeyCode.Escape))
#endif
		{
			if (!flagGameOver)
			{
				StoreRuntimeLevel ();
				StoreRuntimePlayer ();
				GamePlaySave.GetInst ().SaveProgress ();
			}
			else
			{
				// player ship was destroyed, the game is over
				// actualize total statistics
				// write new best score
				// --> do not save level progress
				StoreRuntimeLevel ();
				Statistics.GetInst ().ActualizeTotalScore ();
				Statistics.GetInst ().ActualizeBestScoreList ();
				// and because the game is over, just clear score stats in last saved game, BUT do not delete the save
				// this is because of this situation:
				// * player has score of 25000 (not important)
				// * player saved the game
				// * player loaded the game and was destroyed (game over happened)
				// * total stats was updated from this game over --> this is important
				// * top 20 stats was updated from this game over --> this is important
				// * --->>> if the saved game score is not cleaded, player can load the game
				//		(yes the same game he just lost = game over), with the previous score
				//		of 25000...
				// * therefore saved score is cleaned --> this is important
				// * player loaded last saved game - ship is the same, level is the same,
				//		wave is the same, BUT score is ZERO
				// * player loose the game again (game over) --> JUST THE SCORE OBTAINED FROM
				//		LAST LOAD IS ADDED TO TOTAL SCORE and TOP 20 ;)
				GamePlaySave.GetInst ().ClearSavedScoreProgress ();
			}

			Application.LoadLevel (eGameLevels.Screen_MainMenu.ToString ());
		}

		UpdateTextLifes (RuntimeContext.GetInst ().nesActPlayer.actLifes);
		UpdateTextHealth(RuntimeContext.GetInst ().nesActPlayer.actHealth);
		UpdateTextShield(RuntimeContext.GetInst ().nesActPlayer.actShield);
		UpdateTextEnergy(RuntimeContext.GetInst ().nesActPlayer.actEnergy);

		UpdateTextScore (RuntimeContext.GetInst ().nesActLevel.levelScore);
	}

	/**
	 * Corutine
	 * Return value type IEnumerator is because of "return waiting"
	 */
	IEnumerator SpawnWaves ()
	{
		yield return new WaitForSeconds (startWait);

		while (true)
		{
			UpdateTextActualWave (levelCounterWave);

			for (int i = 0; i < (hazardCount + levelCounterWave); ++i)
			{
				Vector3    spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x),
				                                        spawnValues.y,
				                                        spawnValues.z);
				Quaternion spawnRotation = Quaternion.Euler (0.0f, 180.0f, 0.0f);

				int spaceObjProb = Random.Range (0, 99);

				if (spaceObjProb < (55 - levelCounterWave))
				{
					GameObject tmpAsteroid = null;

					int asterObjProb = Random.Range (1, 99);

					if (asterObjProb <= 33)
						tmpAsteroid = Instantiate (refHazardType1, spawnPosition, spawnRotation) as GameObject;
					else if (asterObjProb > 33 && asterObjProb <= 66)
						tmpAsteroid = Instantiate (refHazardType2, spawnPosition, spawnRotation) as GameObject;
					else
						tmpAsteroid = Instantiate (refHazardType3, spawnPosition, spawnRotation) as GameObject;

					AsteroidController astroController = tmpAsteroid.GetComponent <AsteroidController>();
					if (astroController != null)
					{
						if (astroController.InitAteroid (levelCounterWave))
						{
							// run the ship (!!)
							tmpAsteroid.SetActive (true);
						}
						else
						{
							Destroy (tmpAsteroid);
							Debug.LogWarning ("LEVEL >> Unable to init asteroid");
						}
					}
					else
					{
						Destroy (tmpAsteroid);
						Debug.LogWarning ("LEVEL >> Unable to obtain reference to asteroid");
					}
				}
				else
				{
					GameObject tmpEnemy = null;
					eScoreObjectTypes tmpEnemyType;

					int enemyObjProb = Random.Range (0, 99);

					if (enemyObjProb < (55 - levelCounterWave))
					{
						tmpEnemy = Instantiate (refEnemyShipUWingBasic, spawnPosition, spawnRotation) as GameObject;
						tmpEnemyType = eScoreObjectTypes.EnemyUWingBasic;
					}
					else
					{
						tmpEnemy = Instantiate (refEnemyShipUWingMedium, spawnPosition, spawnRotation) as GameObject;
						tmpEnemyType = eScoreObjectTypes.EnemyUWingMedium;
					}

					EnemyController enemyController = tmpEnemy.GetComponent <EnemyController>();
					if (enemyController != null)
					{
						if (enemyController.InitEnemy (tmpEnemyType, levelCounterWave))
						{
							// run the ship (!!)
							tmpEnemy.SetActive (true);
						}
						else
						{
							Destroy (tmpEnemy);
							Debug.LogWarning ("LEVEL >> Unable to init enemy ship");
						}
					}
					else
					{
						Destroy (tmpEnemy);
						Debug.LogWarning ("LEVEL >> Unable to obtain reference to enemy ship");
					}
				}

				// 4% probability of health bonus
				// --> generate health bonus ONLY if actualHealth is less then max
				if (!RuntimeContext.GetInst ().nesActPlayer.isActHealthMax
					&& Random.Range (0, 99) <= (4 - (levelCounterWave / 10)))
				{
					spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x),
					                             spawnValues.y,
					                             spawnValues.z);

					Instantiate (refBonusHealth, spawnPosition, spawnRotation);
				}
				// 8% probability of shield bonus
				// --> generate shield bonus ONLY if actualShield is less then max
				else if (!RuntimeContext.GetInst ().nesActPlayer.isActShieldMax
						 && Random.Range (0, 99) <= (8 - (levelCounterWave / 8)))
				{
					spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x),
					                             spawnValues.y,
					                             spawnValues.z);
					
					Instantiate (refBonusShield, spawnPosition, spawnRotation);
				}
				// 12% probability of energy bonus
				else if (Random.Range (0, 99) <= (12 - (levelCounterWave / 6)))
				{
					spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x),
					                             spawnValues.y,
					                             spawnValues.z);

					Instantiate (refBonusEnergy, spawnPosition, spawnRotation);
				}
				// 3% probability of generating better ammo type of current weapon type
				if ((RuntimeContext.GetInst ().nesActPlayer.actAmmoType
				     < RuntimeContext.GetInst ().nesActPlayer.topAmmoType)
				    && Random.Range (0, 99) <= 3)
				{
					// --> generate new ammo type ONLY if actualAmmoType is less then max ammo type of the selected ship

					spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x),
					                             spawnValues.y,
					                             spawnValues.z);

					if (RuntimeContext.GetInst ().nesActPlayer.actAmmoType <= Weapon.eAmmoTypes.Better)
						// ADVANCED AMMO
						Instantiate (refFireModeAdvanced, spawnPosition, spawnRotation);
					else if (RuntimeContext.GetInst ().nesActPlayer.actAmmoType <= Weapon.eAmmoTypes.Advanced)
						// HARDCORE AMMO
						Instantiate (refFireModeHardcore, spawnPosition, spawnRotation);
					else if (RuntimeContext.GetInst ().nesActPlayer.actAmmoType <= Weapon.eAmmoTypes.Hardcore)
						// ARMAGEDON AMMO
						Instantiate (refFireModeArmagedon, spawnPosition, spawnRotation);
					else
					{
						// you got the best of the best ammo type
					}
				}
				// 1% probability of generating better weapon type
				if ((RuntimeContext.GetInst ().nesActPlayer.actAmmoType
				     >= (RuntimeContext.GetInst ().nesActPlayer.topAmmoType - 1))
				    && (RuntimeContext.GetInst ().nesActPlayer.actWeaponType
				    	< (RuntimeContext.GetInst ().nesActPlayer.topWeaponType))
				    && Random.Range (0, 99) <= 1)
				{
					// --> generate new weapon type ONLY if actualAmmoType is at least second best ammo type of ship
					// --> generate new weapon type ONLY if actualWeaponType is less top weapons of the ship

					spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x),
					                             spawnValues.y,
					                             spawnValues.z);
					
					if (RuntimeContext.GetInst ().nesActPlayer.actWeaponType <= WeaponSystems.eWeaponTypes.ForwardBasic)
						// FORWARD BASIC
						Instantiate (refWeaponForwardBetter, spawnPosition, spawnRotation);
					else if (RuntimeContext.GetInst ().nesActPlayer.actWeaponType <= WeaponSystems.eWeaponTypes.ForwardBetter)
						// FORWARD BETTER
						Instantiate (refWeaponForwardAdvanced, spawnPosition, spawnRotation);
					else if (RuntimeContext.GetInst ().nesActPlayer.actWeaponType <= WeaponSystems.eWeaponTypes.ForwardAdvanced)
						// DIRECTIONAL BASIC
						Instantiate (refWeaponDirectionalBasic, spawnPosition, spawnRotation);
					else
					{
						// you got the best of the best weapon type
					}
				}

				if (flagGameOver || flagShipDestroyed)
					break;

				float realWait = spawnWait - (0.1f * levelCounterWave);
				if (realWait < 0.5f)
					realWait = 0.5f;

				yield return new WaitForSeconds (realWait);
			}

			if (flagGameOver)
			{
				// text "Game Over" is already shown

				yield return new WaitForSeconds (gameOverWait);

				// show text "press `R` for restart, activate listening to key R
				RestartLevel ();
				break;
			}
			else if (flagShipDestroyed)
			{
				int counter = shipDestroyedWaitCount;
				while (counter > 0)
				{
					// show text "Your ship was destroyed..."
					UpdateTextShipDestroyed (counter);
					yield return new WaitForSeconds (1.0f);
					counter -= 1;
				}
				// clear text "Your ship was destroyed..."
				UpdateTextShipDestroyed (0);

				InitLevel ();
				break;
			}
			else
			{
				levelCounterWave += 1;

				int counter = wavesWaitCount;
				while (counter > 0)
				{
					// show text "prepare for next wave X"
					UpdateTextNewWaveInfo (counter);
					yield return new WaitForSeconds (1.0f);
					counter -= 1;
				}
				// clear text "prepare for next wave X"
				UpdateTextNewWaveInfo (0);
			}		
		}
	}

	//==================================================================================================

	/**
	 * @param score
	 */
	void UpdateTextScore (int score)
	{
		refTextScore.text = "Score: " + score;
	}

	/**
	 * @param bonus
	 */
	public void UpdateTextBonus (string bonus)
	{}

	//==================================================================================================

	/**
	 * @param newLifesCount
	 */
	public void ShipDestroyed ()
	{
		// update current lifes count in settings class --> important for creating new ship
		flagShipDestroyed = true;
	}
	/**
	 * @param beReady
	 */
	void UpdateTextShipDestroyed (int beReady)
	{
		if (beReady <= 0)
			refTextGameOver.text = "";
		else
			refTextGameOver.text = "Your ship was destroyed !\nNew ship will be ready in: " + beReady;
	}

	/**
	 * 
	 */
	public void GameOver ()
	{
		UpdateTextGameOver (false);
		UpdateTextLifes (0);
		flagGameOver = true;
	}
	/**
	 * @param clear
	 */
	void UpdateTextGameOver (bool clear)
	{
		if (clear)
			refTextGameOver.text = "";
		else
			refTextGameOver.text = "Game Over !\nYou survived " + (levelCounterWave - 1) + " waves !";
	}

	//==================================================================================================

	/**
	 * 
	 */
	void RestartLevel ()
	{
		UpdateTextRestartLevel (false);
		flagRestartLevel = true;
	}
	/**
	 * @param clear
	 */
	void UpdateTextRestartLevel (bool clear)
	{
		if (clear)
		{
			refTextGameOver.text = "";
			refTextGameOver.alignment = TextAlignment.Center;
			refTextGameOver.fontStyle = FontStyle.Normal;
		}
		else
		{
#if UNITY_ANDROID
			refTextGameOver.text = "Press:\n> `Back button` to Exit";
#else
			refTextGameOver.text = "Press:\n> `R` to Restart\n> `Esc` to Exit";
#endif
			refTextGameOver.alignment = TextAlignment.Left;
			refTextGameOver.fontStyle = FontStyle.Italic;
		}
	}

	//==================================================================================================

	/**
	 * @param beReady
	 */
	void UpdateTextNewWaveInfo (int beReady)
	{
		if (beReady <= 0)
			refTextGameOver.text = "";
		else
			refTextGameOver.text = "Be ready for the wave number " + levelCounterWave
									+ "\nEnemies will be here in: " + beReady;
	}
	/**
	 * @param wave
	 */
	void UpdateTextActualWave (int wave)
	{
		if (wave <= 0)
			refTextWave.text = "";
		else
			refTextWave.text = "Wave: " + wave;
	}

	//==================================================================================================

	/**
	 * @param lifes
	 */
	public void UpdateTextLifes (int lifes)
	{
		refTextLifes.text = "Lifes: " + lifes;
	}
	/**
	 * @param energy
	 */
	public void UpdateTextEnergy (float energy)
	{
		refTextEnergy.text = "Energy: " + string.Format ("{0:0.0}", energy);
	}
	/**
	 * @param shield
	 */
	public void UpdateTextShield (float shield)
	{
		refTextShield.text = "Shield: " + shield;
	}
	/**
	 * @param health
	 */
	public void UpdateTextHealth (float health)
	{
		refTextHealth.text = "Health: " + health;
	}

	//==================================================================================================
}
