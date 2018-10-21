using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;
using System.Xml;
using System.Text;

//using System.Reflection;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;


//==============================================================================
// ARENA BOUNDARY
//==============================================================================
[System.Serializable]
public class Boundary
{
	public float xMin = -10.0f, xMax = 10.0f;
	public float zMin = -20.0f, zMax = 20.0f;
}


//==========================================================================
// AVAILABLE PLAYER SHIPS
//==========================================================================
public enum ePlayerShips
{
	NO_SHIP	= 0,
	
	Default	= 1,	// default ship, score to unlock: 0
	AK5		= 2,	// AK-5 ship, score to unlock: 25000
	
	END_PLAYER_SHIPS
}

//==========================================================================
// AVAILABLE PLAYER SHIPS
//==========================================================================
public enum ePlayerShipsUnlockScore
{
	Default	= 0,		// default ship, score to unlock: 0
	AK5		= 25000,	// AK-5 ship, score to unlock: 25000

    END_PLAYER_SHIPS_SCORE
}

//==========================================================================
// AVAILABLE LEVELS
//==========================================================================
public enum eGameLevels
{
	Screen_MainMenu		= 0,
	Screen_ShipSelect	= 1,
	Level_1stContact,
	
	END_GAME_LEVELS
}

//==========================================================================
// E SCORE OBJECT TYPES
//==========================================================================
public enum eScoreObjectTypes
{
	AsteroidStandard	= 10,
	EnemyUWingBasic		= 20,
	EnemyUWingMedium	= 40,
	
	END_SCORE_OBJECT_TYPES
}

//==========================================================================
// E GAME TAGS
//==========================================================================
public enum eGameTags
{
	GameController,
	MainCamera,
	Player,

	Asteroid,
	Enemy,

	Background,
	Boundary,
	Fader,

	BulletPlayer,
	Bullet,

	Bonus,

	Shield,
	Weapon,
	Engine
}


//==============================================================================
// GAME CONFIGURATION
//==============================================================================
[System.Serializable]
public sealed class GamePlaySave
{
	//==========================================================================
	// Singleton part of class
	//==========================================================================
	private static volatile GamePlaySave 	  instance;
	private static 			System.Object syncRoot = new System.Object();
	
	private GamePlaySave()
	{
		// create empty player
		nesLoadPlayer = new PlayerConfig ();
		// create empty level
		nesLoadLevel  = new LevelConfig ();
	}
	
	private static GamePlaySave Instance
	{
		get 
		{
			if (instance == null) 
			{
				lock (syncRoot) 
				{
					if (instance == null) 
						instance = new GamePlaySave();
				}
			}
			
			return instance;
		}
	}

	public static GamePlaySave GetInst ()
	{
		return Instance;
	}


	//==========================================================================
	// PLAYER CONFIG
	// This class is used for loading player's config from a persistent file
	// AND for saving actual player's runtime context to a persistent file.
	// Class has an ability to convert runtime context to config.
	//==========================================================================
	[System.Serializable]
	public class PlayerConfig
	{
		public ePlayerShips actShip;

		public int 	  actLifes;
		public float  actHealth;
		public float  actShield;
		public float  actEnergy;
		
		public CoreSystems.Energy.eRechargeTypes actEnergyType;
		
		public WeaponSystems.eWeaponTypes 	actWeaponType;
		public Weapon.eAmmoTypes 			actAmmoType;

		public PlayerConfig ()
		{
			// empty, no special thing to do here
		}

		/**
		 * @param stat
		 */
		public bool StatusToConfig (RuntimeContext.PlayerStatus stat)
		{
			actShip			= stat.actShip;

			actLifes		= stat.actLifes;
			actHealth		= stat.actHealth;
			actShield		= stat.actShield;
			actEnergy		= stat.actEnergy;
			
			actEnergyType 	= stat.actEnergyType;
			
			actWeaponType 	= stat.actWeaponType;
			actAmmoType   	= stat.actAmmoType;
			
			return true;
		}

		/**
		 * 
		 */
		public void ClearConfig ()
		{
			actShip			= ePlayerShips.NO_SHIP;

			actLifes		= 0;
			actHealth		= 0.0f;
			actShield		= 0.0f;
			actEnergy		= 0.0f;

			actEnergyType	= CoreSystems.Energy.eRechargeTypes.NoRecharge;

			actWeaponType	= WeaponSystems.eWeaponTypes.NoWeapon;
			actAmmoType		= Weapon.eAmmoTypes.NoShoting;
		}
	}

	//==========================================================================
	// LEVEL CONFIG
	// This class is used for loading level's config from a persistent file
	// AND for saving actual level runtime context to a persistent file.
	// Class has an ability to convert runtime context to config.
	//==========================================================================
	[System.Serializable]
	public class LevelConfig
	{
		public string levelName;

		public int    levelActWave;

		public int	  levelScore;
		public int	  levelDestroyedEnemies;
		public int	  levelDestroyedAsteroids;

		public LevelConfig ()
		{
			// empty, no special thing to do here
		}

		/**
		 * @param stat
		 */
		public bool StatusToConfig (RuntimeContext.LevelStatus stat)
		{
			levelName	 = stat.levelName;

			levelActWave = stat.levelActWave;

			levelScore   = stat.levelScore;
			levelDestroyedEnemies   = stat.levelDestroyedEnemies;
			levelDestroyedAsteroids = stat.levelDestroyedAsteroids;

			return true;
		}

		/**
		 * 
		 */
		public void ClearConfig ()
		{
			levelName	 = eGameLevels.Screen_MainMenu.ToString ();

			levelActWave = 1;

			levelScore	 = 0;
			levelDestroyedEnemies   = 0;
			levelDestroyedAsteroids = 0;
		}
	}

	//==========================================================================

	// last saved progress
	public System.DateTime  lastGameSaveTime;

	[XmlElement("PlayerConfig")]
	public PlayerConfig nesLoadPlayer;
	[XmlElement("LevelConfig")]
	public LevelConfig  nesLoadLevel;

	//==========================================================================

	/**
	 * Initialization of completly new game
	 */
	public bool LoadNewGame ()
	{
		Statistics.GetInst ().LoadStatistics ();

		nesLoadLevel  = InitLevel (eGameLevels.Level_1stContact.ToString ());
		if (Statistics.GetInst ().nesTotScore.totalScore >= (int)ePlayerShipsUnlockScore.AK5)
			nesLoadPlayer = InitShipSystems (ePlayerShips.AK5);
		else
			nesLoadPlayer = InitShipSystems (ePlayerShips.Default);

		if (nesLoadLevel == null || nesLoadPlayer == null)
			return false;
		else
			return true;
	}
	/**
	 * Initialization of completly new game
	 * 
	 * @param ship
	 */
	public bool LoadNewGame (ePlayerShips ship)
	{
		nesLoadLevel  = InitLevel (eGameLevels.Level_1stContact.ToString ());
		nesLoadPlayer = InitShipSystems (ship);

		if (nesLoadLevel == null || nesLoadPlayer == null)
			return false;
		else
			return true;
	}
	/**
	 * Initialization of completly new game
	 * 
	 * @param level
	 */
	public bool LoadNewGame (string level)
	{
		Statistics.GetInst ().LoadStatistics ();

		nesLoadLevel  = InitLevel (eGameLevels.Level_1stContact.ToString ());
		if (Statistics.GetInst ().nesTotScore.totalScore >= (int)ePlayerShipsUnlockScore.AK5)
			nesLoadPlayer = InitShipSystems (ePlayerShips.AK5);
		else
			nesLoadPlayer = InitShipSystems (ePlayerShips.Default);

		if (nesLoadLevel == null || nesLoadPlayer == null)
			return false;
		else
			return true;
	}
	/**
	 * Initialization of completly new game
	 * 
	 * @param ship
	 * @param level
	 */
	public bool LoadNewGame (ePlayerShips ship, string level)
	{
		nesLoadLevel  = InitLevel (level);
		nesLoadPlayer = InitShipSystems (ship);

		if (nesLoadLevel == null || nesLoadPlayer == null)
			return false;
		else
			return true;
	}

	//==========================================================================

	/**
	 * Create complete new ship
	 * 
	 * @param ship
	 */
	public PlayerConfig InitShipSystems (ePlayerShips ship)
	{
		if (!RuntimeContext.GetInst ().nesActPlayer.InitPlayerShip (ship))
			return null;

		PlayerConfig retPlayer = new PlayerConfig ();

		if (retPlayer.StatusToConfig (RuntimeContext.GetInst ().nesActPlayer))
			return retPlayer;
		else
			return null;
	}

	//==========================================================================

	/**
	 * @param level
	 */
	public LevelConfig InitLevel (string level)
	{
		if (!CheckIfLevelExists (level))
			return null;

		LevelConfig retLevel = new LevelConfig ();

		retLevel.levelName 	 			 = level;
		retLevel.levelActWave	 		 = 1;
		
		retLevel.levelScore				 = 0;
		retLevel.levelDestroyedEnemies 	 = 0;
		retLevel.levelDestroyedAsteroids = 0;
		
		if (RuntimeContext.GetInst ().nesActLevel.ConfigToStatus (retLevel))
			return retLevel;
		else
			return null;
	}
	/**
	 * @param level
	 */
	public bool CheckIfLevelExists (string level)
	{
		if ((level != "Screen_MainMenu") 	&&
		    (level != "Screen_ShipSelect") 	&&
		    (level != "Level_1stContact"))
		{
			Debug.LogWarning ("GAME CONFIG >> Unknown level name `" + level + "`");
			return false;
		}
		else
			return true;
	}

	//==========================================================================

	/**
	 * Currentlly just set flag, that runtime values are stored
	 */
	public bool SaveProgress ()
	{
		// we are saving the game -->> get actual runtime context and set config nested classes
		nesLoadPlayer.StatusToConfig (RuntimeContext.GetInst ().nesActPlayer);
		nesLoadLevel.StatusToConfig (RuntimeContext.GetInst ().nesActLevel);

		lastGameSaveTime = System.DateTime.Now;

		StringWriter  sw = new StringWriter ();
		XmlSerializer xs = new XmlSerializer (this.GetType ());
		xs.Serialize (sw, Instance);

		PlayerPrefs.SetString ("lastGame", sw.ToString ());
		PlayerPrefs.Save ();

		Debug.Log ("GAME CONFIG SAVE >> \n" + sw.ToString ());

		return true;
	}
	/**
	 * 
	 */
	public bool ExistsLastGameSave ()
	{
		return PlayerPrefs.HasKey ("lastGame");
	}
	/**
	 * Loading state from last game --> currentlly no code (just take last stored runtime values)
	 * in future load from persistent file
	 */
	public bool LoadProgress ()
	{
		if (!ExistsLastGameSave ())
		{
			Debug.LogWarning ("GAME CONFIG LOAD >> No save was found. New Game will be loaded...");
			return LoadNewGame ();
		}

		try
		{
			string serialized = PlayerPrefs.GetString ("lastGame");
	
			Debug.Log ("GAME CONFIG LOAD >> Loaded progress: \n" + serialized);

			// loading last level config
			{
				StringReader  srLevel = new StringReader (serialized);
				XmlTextReader xrLevel = new XmlTextReader (srLevel);
				if (xrLevel.ReadToDescendant ("LevelConfig"))
				{
					XmlSerializer xsLevel = new XmlSerializer (typeof (LevelConfig));
					instance.nesLoadLevel = (LevelConfig)xsLevel.Deserialize (xrLevel.ReadSubtree ());
				}
				else
					Debug.LogError ("GAME CONFIG LOAD >> LevelConfig element was not found...");
				xrLevel.Close ();
				srLevel.Close ();
			}
			// loading saved ship config
			{
				StringReader  srPlayer = new StringReader (serialized);
				XmlTextReader xrPlayer = new XmlTextReader (srPlayer);
				if (xrPlayer.ReadToDescendant ("PlayerConfig"))
				{
					XmlSerializer xsPlayer = new XmlSerializer (typeof (PlayerConfig));
					instance.nesLoadPlayer = (PlayerConfig)xsPlayer.Deserialize (xrPlayer.ReadSubtree ());
				}
				else
					Debug.LogError ("GAME CONFIG LOAD >> PlayerConfig element was not found...");
				xrPlayer.Close ();
				srPlayer.Close ();
			}

			Debug.Log ("GAME CONFIG LOAD >> Deserialized: "
			           + "\nShip: "   + nesLoadPlayer.actShip
			           + "\nLifes: "  + nesLoadPlayer.actLifes
			           + "\nHealth: " + nesLoadPlayer.actHealth
			           + "\nShield: " + nesLoadPlayer.actShield
			           + "\nEnergy: " + nesLoadPlayer.actEnergy
			           + "\nEnergy Recharge: " 	+ nesLoadPlayer.actEnergyType
			           + "\nWeapon Type: "  	+ nesLoadPlayer.actWeaponType
			           + "\nAmmo Type: " 		+ nesLoadPlayer.actAmmoType
			           + "\n\n"
			           + "\nLevel Name: " + nesLoadLevel.levelName
			           + "\nLevel Wave: " + nesLoadLevel.levelActWave
			           + "\nLevel Score: " + nesLoadLevel.levelScore
			           + "\nLevel Enemies: " + nesLoadLevel.levelDestroyedEnemies
			           + "\nLevel Asteroids: " + nesLoadLevel.levelDestroyedAsteroids);		
		}
		catch (PlayerPrefsException ex)
		{
			Debug.LogWarning ("GAME CONFIG LOAD >> PlayerPrefs exception occured: " + ex.Message);
		}
		catch (ArgumentNullException ex)
		{
			Debug.LogWarning ("GAME CONFIG LOAD >> Argument Null exception occured: " + ex.Message);
		}
		catch (InvalidOperationException ex)
		{
			Debug.LogWarning ("GAME CONFIG LOAD >> Invalid Operation exception occured: " + ex.Message);
		}
		catch (MemberAccessException ex)
		{
			Debug.LogWarning ("GAME CONFIG LOAD >> Member Access exception occured: " + ex.Message);
		}
		catch (Exception ex)
		{
			Debug.LogWarning ("GAME CONFIG LOAD >> Unknown exception occured: " + ex.GetType () + " " + ex.Message);
		}

		if (!CheckIfLevelExists (nesLoadLevel.levelName))
			return false;

		return (RuntimeContext.GetInst ().nesActPlayer.ConfigToStatus (nesLoadPlayer) &&
		        RuntimeContext.GetInst ().nesActLevel.ConfigToStatus (nesLoadLevel));
	}

	//==========================================================================

	/**
	 * 
	 */
	public bool ClearSavedScoreProgress ()
	{
		if (!ExistsLastGameSave ())
			return false;

		if (!LoadProgress ())
			return false;

		RuntimeContext.GetInst ().ClearScore ();

		return SaveProgress ();
	}
}


//==============================================================================
// STATISTICS
//==============================================================================
[System.Serializable]
public sealed class Statistics
{
	//==========================================================================
	// Singleton part of class
	//==========================================================================
	private static volatile Statistics 		instance;
	private static 			System.Object  	syncRoot = new System.Object();
	
	private Statistics()
	{
		nesTotScore   = new TotalScore ();
		nesBestScores = new BestScoresList ();
	}
	
	private static Statistics Instance
	{
		get 
		{
			if (instance == null) 
			{
				lock (syncRoot) 
				{
					if (instance == null) 
						instance = new Statistics();
				}
			}
			
			return instance;
		}
	}
	
	public static Statistics GetInst ()
	{
		return Instance;
	}

	//==========================================================================
	// TOTAL SCORE
	//==========================================================================
	[System.Serializable]
	public class TotalScore
	{
		public int totalScore;
		public int totalSurvivedWaves;
		public int totalDestroyedEnemies;
		public int totalDestroyedAsteroids;
		
		public TotalScore ()
		{}
	}

	//==========================================================================
	// BEST SCORE
	//==========================================================================
	[System.Serializable]
	public class BestScore : IComparable
	{
		public System.DateTime bestScoreTime;

		public int bestScore;
		public int bestSurvivedWaves;
		public int bestDestroyedEnemies;
		public int bestDestroyedAsteroids;
		
		public BestScore ()
		{
			bestScoreTime = new System.DateTime();
		}

		public int CompareTo (System.Object obj)
		{
			if (obj == null)
				return 1;

			BestScore other = obj as BestScore;
			if (other == null)
				return 1;

			return -(bestScore - other.bestScore);
		}
	}

	//==========================================================================
	// BEST SCORE
	//==========================================================================
	[System.Serializable]
	public class BestScoresList
	{
		public List<BestScore> listBestScores;
		public int maxElements;

		public BestScoresList ()
		{
			listBestScores  = new List<BestScore>();
			maxElements		= 20;
		}
	}

	//==========================================================================

	// last saved progress
	public System.DateTime  lastStatsSaveTime;

	[XmlElement("BestScoresList")]
	public BestScoresList 	nesBestScores;

	[XmlElement("TotalScore")]
	public TotalScore  		nesTotScore;

	//==========================================================================

	/**
	 * 
	 */
	public bool InitEmptyStats ()
	{
		nesTotScore.totalScore = 0;
		nesTotScore.totalSurvivedWaves = 0;
		nesTotScore.totalDestroyedEnemies = 0;
		nesTotScore.totalDestroyedAsteroids = 0;

		nesBestScores.listBestScores.Clear ();

		return true;
	}


	/**
	 * 
	 */
	public bool ActualizeTotalScore ()
	{
		LoadStatistics ();

		nesTotScore.totalScore 		+= RuntimeContext.GetInst().nesActLevel.levelScore;
		nesTotScore.totalSurvivedWaves += RuntimeContext.GetInst().nesActLevel.levelActWave;
		nesTotScore.totalDestroyedEnemies += RuntimeContext.GetInst().nesActLevel.levelDestroyedEnemies;
		nesTotScore.totalDestroyedAsteroids  += RuntimeContext.GetInst().nesActLevel.levelDestroyedAsteroids;

		SaveStatistics ();

		return true;
	}

	/**
	 * 
	 */
	public bool ActualizeBestScoreList ()
	{
		LoadStatistics ();

		BestScore tmpBest = new BestScore ();

		tmpBest.bestScoreTime  = System.DateTime.Now;
		tmpBest.bestScore 		 = RuntimeContext.GetInst().nesActLevel.levelScore;
		tmpBest.bestSurvivedWaves  = RuntimeContext.GetInst().nesActLevel.levelActWave;
		tmpBest.bestDestroyedEnemies = RuntimeContext.GetInst().nesActLevel.levelDestroyedEnemies;
		tmpBest.bestDestroyedAsteroids = RuntimeContext.GetInst().nesActLevel.levelDestroyedAsteroids;

		nesBestScores.listBestScores.Add (tmpBest);
		nesBestScores.listBestScores.Sort ();
		while (nesBestScores.maxElements < nesBestScores.listBestScores.Count)
			nesBestScores.listBestScores.RemoveAt (nesBestScores.listBestScores.Count - 1);

		string scoreList = "SCORE LIST >> \n";
		for (int i = 0; i < nesBestScores.listBestScores.Count; i++)
		{
			scoreList += "\n SCORE: " + nesBestScores.listBestScores[i].bestScore;
		}

		Debug.Log (scoreList);

		SaveStatistics ();

		return true;
	}


	/**
	 * Currentlly just set flag, that runtime values are stored
	 */
	public bool SaveStatistics ()
	{
		lastStatsSaveTime = System.DateTime.Now;
		
		StringWriter  sw = new StringWriter ();
		XmlSerializer xs = new XmlSerializer (this.GetType ());
		xs.Serialize (sw, this);
		
		PlayerPrefs.SetString ("statistics", sw.ToString ());
		PlayerPrefs.Save ();

		Debug.Log ("STATS SAVE >> \n" + sw.ToString ());
		
		return true;
	}
	/**
	 * 
	 */
	public bool ExistsSavedStatistics ()
	{
		return PlayerPrefs.HasKey ("statistics");
	}
	/**
	 * Loading state from last game --> currentlly no code (just take last stored runtime values)
	 * in future load from persistent file
	 */
	public bool LoadStatistics ()
	{
		if (!ExistsSavedStatistics ())
		{
			return InitEmptyStats ();
		}
		
		string serialized = PlayerPrefs.GetString ("statistics");
		
		Debug.Log ("STATS LOAD >> \n" + serialized);

		// loading total score
		{
			StringReader  srStat = new StringReader (serialized);
			XmlTextReader xrStat = new XmlTextReader (srStat);
			if (xrStat.ReadToDescendant ("TotalScore"))
			{
				XmlSerializer xsStat = new XmlSerializer (typeof (TotalScore));
				instance.nesTotScore = (TotalScore)xsStat.Deserialize (xrStat.ReadSubtree ());
			}
			else
				Debug.LogError ("STATS LOAD >> TotalScore element was not found...");
			xrStat.Close ();
			srStat.Close ();
		}
		// loading best score list
		{
			StringReader  srStat = new StringReader (serialized);
			XmlTextReader xrStat = new XmlTextReader (srStat);
			if (xrStat.ReadToDescendant ("BestScoresList"))
			{
				XmlSerializer xsStat   = new XmlSerializer (typeof (BestScoresList));
				instance.nesBestScores = (BestScoresList)xsStat.Deserialize (xrStat.ReadSubtree ());
			}
			else
				Debug.LogError ("STATS LOAD >> BestScoresList element was not found...");
			xrStat.Close ();
			srStat.Close ();
		}

		return true;
	}
}


//==============================================================================
// RUNTIME CONTEXT
//==============================================================================
[System.Serializable]
public sealed class RuntimeContext
{
	//==========================================================================
	// Singleton part of class
	//==========================================================================
	private static volatile RuntimeContext instance;
	private static 			System.Object  syncRoot = new System.Object();
	
	private RuntimeContext()
	{
		nesActPlayer = new PlayerStatus ();
		nesActLevel  = new LevelStatus ();
	}
	
	private static RuntimeContext Instance
	{
		get 
		{
			if (instance == null) 
			{
				lock (syncRoot) 
				{
					if (instance == null) 
						instance = new RuntimeContext();
				}
			}
			
			return instance;
		}
	}
	
	public static RuntimeContext GetInst ()
	{
		return Instance;
	}

	//==========================================================================
	// PLAYER CONFIG
	//==========================================================================
	[System.Serializable]
	public class PlayerStatus
	{
		public ePlayerShips actShip;


		public int 	  actLifes;
		public float  actHealth;
		public float  actShield;
		public float  actEnergy;
		
		public CoreSystems.Energy.eRechargeTypes actEnergyType;
		
		public WeaponSystems.eWeaponTypes 	actWeaponType;
		public Weapon.eAmmoTypes 			actAmmoType;


		public bool   isActHealthMax;
		public bool   isActShieldMax;


		public int 	  topLifes;
		public float  topHealth;
		public float  topShield;
		public float  topEnergy;

		public CoreSystems.Energy.eRechargeTypes topEnergyType;
		
		public WeaponSystems.eWeaponTypes 	topWeaponType;
		public Weapon.eAmmoTypes 			topAmmoType;


		public int scoreToUnlock;


		public PlayerStatus ()
		{
			// nothing special to do here
		}
		public PlayerStatus (ePlayerShips shipType)
		{
			InitPlayerShip (shipType);
		}

		/**
		 * @param conf
		 */
		public bool ConfigToStatus (GamePlaySave.PlayerConfig conf)
		{
			// init "top" parts of class
			if (!InitPlayerShip (conf.actShip))
				return false;

			//actShip			= conf.actShip;

			actLifes		= conf.actLifes;
			actHealth		= conf.actHealth;
			actShield		= conf.actShield;
			actEnergy		= conf.actEnergy;

			actEnergyType 	= conf.actEnergyType;

			actWeaponType 	= conf.actWeaponType;
			actAmmoType   	= conf.actAmmoType;

			return true;
		}
		/**
		 * @param conf
		 */
		public bool ConfigToStatus (ePlayerShips shipType)
		{
			return InitPlayerShip (shipType);
		}

		/**
		 * 
		 */
		public void ClearStatus ()
		{
			actShip			= ePlayerShips.NO_SHIP;
			
			actLifes		= 0;
			actHealth		= 0.0f;
			actShield		= 0.0f;
			actEnergy		= 0.0f;
			
			actEnergyType	= CoreSystems.Energy.eRechargeTypes.NoRecharge;
			
			actWeaponType	= WeaponSystems.eWeaponTypes.NoWeapon;
			actAmmoType		= Weapon.eAmmoTypes.NoShoting;


			isActHealthMax	= false;
			isActShieldMax	= false;
			
			
			topLifes 		= 0;
			topHealth		= 0.0f;
			topShield		= 0.0f;
			topEnergy		= 0.0f;
			
			topEnergyType	= CoreSystems.Energy.eRechargeTypes.NoRecharge;
			
			topWeaponType	= WeaponSystems.eWeaponTypes.NoWeapon;
			topAmmoType		= Weapon.eAmmoTypes.NoShoting;


			scoreToUnlock	= 0;
		}

		/**
		 * @param shipType
		 */
		public bool InitPlayerShip (ePlayerShips shipType)
		{
			switch (shipType)
			{
			case ePlayerShips.AK5:
				actLifes 		=   4;
				actHealth		= 100.0f;
				actShield		=  50.0f;
				actEnergy		=  75.0f;
				
				actEnergyType	= CoreSystems.Energy.eRechargeTypes.Medium;
				
				actWeaponType	= WeaponSystems.eWeaponTypes.ForwardBasic;
				actAmmoType		= Weapon.eAmmoTypes.Better;


				isActHealthMax	= true;
				isActShieldMax	= false;


				topLifes 		=  10;
				topHealth		= 100.0f;
				topShield		= 100.0f;
				topEnergy		= 100.0f;
				
				topEnergyType	= CoreSystems.Energy.eRechargeTypes.SuperFast;
				
				topWeaponType	= WeaponSystems.eWeaponTypes.DirectionalBasic;
				topAmmoType		= Weapon.eAmmoTypes.Armagedon;


				scoreToUnlock	= (int)ePlayerShipsUnlockScore.AK5;
				break;
				
			case ePlayerShips.Default:
				actLifes 		=   3;
				actHealth		= 100.0f;
				actShield		=  40.0f;
				actEnergy		=  75.0f;
				
				actEnergyType	= CoreSystems.Energy.eRechargeTypes.Medium;
				
				actWeaponType	= WeaponSystems.eWeaponTypes.ForwardBasic;
				actAmmoType		= Weapon.eAmmoTypes.Better;


				isActHealthMax	= true;
				isActShieldMax	= false;
				
				
				topLifes 		=   7;
				topHealth		= 100.0f;
				topShield		= 100.0f;
				topEnergy		= 100.0f;
				
				topEnergyType	= CoreSystems.Energy.eRechargeTypes.Fast;
				
				topWeaponType	= WeaponSystems.eWeaponTypes.ForwardAdvanced;
				topAmmoType		= Weapon.eAmmoTypes.Hardcore;


				scoreToUnlock	= (int)ePlayerShipsUnlockScore.Default;
				break;
				
			default:
				ClearStatus ();
				return false;
			}
			
			actShip = shipType;
			
			return true;
		}

		/**
		 * 
		 */
		public bool ReinitDestroyedShip ()
		{
			WeaponSystems.eWeaponTypes oldWeap = actWeaponType;
			Weapon.eAmmoTypes oldAmmo = actAmmoType;
			int oldLife = actLifes;

			bool ret = InitPlayerShip (actShip);

			actLifes = oldLife;
			if ((oldWeap - 1) > actWeaponType)
				actWeaponType = (oldWeap - 1);
			if ((oldAmmo - 1) > actAmmoType)
				actAmmoType   = (oldAmmo - 1);

			return ret;
		}
	}

	//==========================================================================
	// LEVEL CONFIG
	//==========================================================================
	[System.Serializable]
	public class LevelStatus
	{
		public string levelName;
		
		public int    levelActWave;
		
		public int	  levelScore;
		public int	  levelDestroyedEnemies;
		public int	  levelDestroyedAsteroids;
		
		public LevelStatus ()
		{
			// nothing special to do here
		}

		/**
		 * @param conf
		 */
		public bool ConfigToStatus (GamePlaySave.LevelConfig conf)
		{
			levelName	 = conf.levelName;

			levelActWave = conf.levelActWave;
			
			levelScore   = conf.levelScore;
			levelDestroyedEnemies   = conf.levelDestroyedEnemies;
			levelDestroyedAsteroids = conf.levelDestroyedAsteroids;
			
			return true;
		}

		/**
		 * 
		 */
		public void ClearStatus ()
		{
			levelName	 = eGameLevels.Screen_MainMenu.ToString ();
			
			levelActWave = 1;
			
			levelScore	 = 0;
			levelDestroyedEnemies   = 0;
			levelDestroyedAsteroids = 0;
		}
	}

	//==========================================================================

	public PlayerStatus nesActPlayer;
	public LevelStatus  nesActLevel;
	
	//==========================================================================

	/**
	 * Store runtime values into this containter
	 * 
	 * @param lifes
	 * @param health
	 * @param shield
	 * @param energy
	 * @param energyRecharge
	 */
	public bool StorePlayerCore (int   lifes,
	                             float health,
	                             float shield,
	                             float energy, CoreSystems.Energy.eRechargeTypes energyRecharge)
	{
		nesActPlayer.actLifes  = lifes;
		nesActPlayer.actHealth = health;
		nesActPlayer.actShield = shield;
		nesActPlayer.actEnergy = energy;
		nesActPlayer.actEnergyType = energyRecharge;
		
		return true;
	}
	/**
	 * Store runtime values into this containter
	 * 
	 * @param lifes
	 * @param health
	 * @param shield
	 */
	public bool StorePlayerCore (int   lifes,
	                             float health,
	                             float shield)
	{
		nesActPlayer.actLifes  = lifes;
		nesActPlayer.actHealth = health;
		nesActPlayer.actShield = shield;

		return true;
	}
	/**
	 * @param isMaxHealth
	 * @param isMaxShield
	 */
	public bool StorePlayerCoreMax (bool isMaxHealth, bool isMaxShield)
	{
		nesActPlayer.isActHealthMax = isMaxHealth;
		nesActPlayer.isActShieldMax = isMaxShield;

		return true;
	}
	/**
	 * @param health
	 * @param isMaxHealth
	 */
	public bool StorePlayerCoreHealth (float health, bool isMaxHealth)
	{
		nesActPlayer.actHealth = health;
		nesActPlayer.isActHealthMax = isMaxHealth;

		return true;
	}
	/**
	 * @param shield
	 * @param isMaxShield
	 */
	public bool StorePlayerCoreShield (float shield, bool isMaxShield)
	{
		nesActPlayer.actShield = shield;
		nesActPlayer.isActShieldMax = isMaxShield;
		
		return true;
	}
	/**
	 * @param energy
	 */
	public bool StorePlayerCoreEnergy (float energy)
	{
		nesActPlayer.actEnergy = energy;
		
		return true;
	}
	/**
	 * @param energy
	 * @param energyRecharge
	 */
	public bool StorePlayerCoreEnergy (float energy, CoreSystems.Energy.eRechargeTypes energyRecharge)
	{
		nesActPlayer.actEnergy = energy;
		nesActPlayer.actEnergyType = energyRecharge;
		
		return true;
	}
	/**
	 * Store runtime values into this containter
	 */
	public bool StorePlayerEngine ()
	{
		return true;
	}
	/**
	 * Store runtime values into this containter
	 * 
	 * @param weapon
	 * @param ammo
	 */
	public bool StorePlayerWeapon (WeaponSystems.eWeaponTypes weapon, Weapon.eAmmoTypes ammo)
	{
		nesActPlayer.actWeaponType 	= weapon;
		nesActPlayer.actAmmoType	= ammo;
		
		return true;
	}


	/**
	 * @param name
	 * @param wave
	 * @param score
	 * @param enemies
	 * @param asteroids
	 */
	public bool StoreLevelStatus (string name, int wave, int score, int enemies, int asteroids)
	{
		nesActLevel.levelName 	 = name;
		nesActLevel.levelActWave = wave;

		nesActLevel.levelScore 	 = score;
		nesActLevel.levelDestroyedEnemies 	= enemies;
		nesActLevel.levelDestroyedAsteroids = asteroids;
		
		return true;
	}


	/**
	 * @param score
	 */
	public void AddScore (eScoreObjectTypes score)
	{
		nesActLevel.levelScore += (int)score;

		switch (score)
		{
		case eScoreObjectTypes.AsteroidStandard:
			nesActLevel.levelDestroyedAsteroids += 1;
			break;
			
		case eScoreObjectTypes.EnemyUWingBasic:
		case eScoreObjectTypes.EnemyUWingMedium:
			nesActLevel.levelDestroyedEnemies += 1;
			break;
			
		default:
			break;
		}
	}

	/**
	 * 
	 */
	public void ClearScore ()
	{
		nesActLevel.levelScore = 0;
		nesActLevel.levelDestroyedEnemies = 0;
		nesActLevel.levelDestroyedAsteroids = 0;
	}
}

