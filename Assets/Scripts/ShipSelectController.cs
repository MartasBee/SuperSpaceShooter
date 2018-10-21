//#define SHIP_SELECT_ROTATE_PLAYER

using UnityEngine;
using System.Collections;

public class ShipSelectController : MonoBehaviour
{
	private GameObject 		 refPlayerInstance;
	private PlayerController refPlayerController;
	private Transform 		 refTargetTransform;
	private GameObject		 refMainCamera;


	public  GameObject 	refPlayerShipDefault;
	public  GameObject 	refPlayerShipAK5;


	// camera rotation properties
	private float x = 0.0f;
	private float y = 0.0f;

	public float distance 	=  10.0f;
	public float xSpeed 	= 120.0f;
	public float ySpeed 	= 120.0f;
	
	public float yMinLimit 	= -70.0f;
	public float yMaxLimit 	=  70.0f;
	
	public float distanceMin =  4.0f;
	public float distanceMax = 25.0f;

#if UNITY_ANDROID
    // accelerometer input variables
    private Vector3 zeroAc;
    private Vector3 curAc;
    private float   sensH   = 15.0f;
    private float   sensV   = 15.0f;
    private float   smooth  =  0.5f;
#endif

	// displayed texts
	public GUIText refTextUnlock;
	public GUIText refTextLifes;
	public GUIText refTextHealth;
	public GUIText refTextShield;
	public GUIText refTextAmmo;
	public GUIText refTextWeapons;


	public GameObject background;
	public GameObject spotlight;


	// resolution of screen
	private int screenWidth;
	private int screenHeight;
	
	// count of currently shown buttons
	private int showedButtons;
	
	// array with rectangles of all buttons
	private Rect [] realButtonPos;


	void Awake ()
	{
		refMainCamera = GameObject.FindGameObjectWithTag ("MainCamera");
		if (refMainCamera == null)
		{
			Debug.LogError ("SHIP SELECTION >> FAILED to obtain reference to Main Camera");
		}
		
		// Make the rigid body not change rotation
		if (refMainCamera.rigidbody)
			refMainCamera.rigidbody.freezeRotation = true;

		CalcButtonRects ();
	}

	void Start ()
	{
		Statistics.GetInst ().LoadStatistics ();
		ePlayerShips selectedShip = ePlayerShips.Default;
		if (!RuntimeContext.GetInst ().nesActPlayer.InitPlayerShip (selectedShip) || !LoadAndShowShip ())
		{
			// WE FAILED TO INIT SHIP (!!)
			Debug.LogError ("SHIP SELECTION >> FAILED to init or load ship...");
		}

#if UNITY_ANDROID
        ResetAccelerometerAxes();
#endif

#if SHIP_SELECT_ROTATE_PLAYER
		// get actual player's angles
		x = refPlayerInstance.transform.eulerAngles.y;
		y = refPlayerInstance.transform.eulerAngles.x;
#else
		// get actual camera angles
		x = refMainCamera.transform.eulerAngles.y;
		y = refMainCamera.transform.eulerAngles.x;
#endif

		MoveAndRotateCam (0.0f, 0.0f, 0.0f);
	}

	void OnGUI ()
	{
		if ((Screen.width != screenWidth) || (Screen.height != screenHeight))
			CalcButtonRects ();	

		ePlayerShips selectedShip = RuntimeContext.GetInst ().nesActPlayer.actShip;

		if (selectedShip > (ePlayerShips.NO_SHIP + 1))
		{
			if (GUI.Button (realButtonPos[0], "<<"))
			{
				if (refPlayerInstance != null)
				{
					Destroy (refPlayerInstance);

					refPlayerInstance   = null;
					refPlayerController = null;
					refTargetTransform  = null;
				}

				// select previous ship from ship's list
				selectedShip -= 1;
				// init ship systems
				if (!RuntimeContext.GetInst ().nesActPlayer.InitPlayerShip (selectedShip) || !LoadAndShowShip ())
				{
					// WE FAILED TO INIT SHIP (!!)
					Debug.LogError ("SHIP SELECTION >> FAILED to init or load ship...");
				}
			}
		}
		if (selectedShip < (ePlayerShips.END_PLAYER_SHIPS - 1))
		{
			if (GUI.Button (realButtonPos[1], ">>"))
			{
				if (refPlayerInstance != null)
				{
					Destroy (refPlayerInstance);

					refPlayerInstance   = null;
					refPlayerController = null;
					refTargetTransform  = null;
				}
								
				// select previous ship from ship's list
				selectedShip += 1;
				// init ship systems
				if (!RuntimeContext.GetInst ().nesActPlayer.InitPlayerShip (selectedShip) || !LoadAndShowShip ())
				{
					// WE FAILED TO INIT SHIP (!!)
					Debug.LogError ("SHIP SELECTION >> FAILED to init or load ship...");
				}
			}
		}
	}
	
	void Update ()
	{
#if UNITY_ANDROID
		// it should work on android too...
		if (Input.GetKey (KeyCode.Escape))
#else
		if (Input.GetKeyDown (KeyCode.Escape))
#endif
		{
			Application.LoadLevel (eGameLevels.Screen_MainMenu.ToString ());
		}

		// is the player object selected ?
		if (refTargetTransform)
		{
			// get X and Y axis input --> rotation
			float tmpX = 0.0f;
			float tmpY = 0.0f;
#if UNITY_ANDROID
            curAc = Vector3.Lerp(curAc, Input.acceleration - zeroAc, Time.deltaTime / smooth);

            tmpY  = Mathf.Clamp(curAc.y * sensV, -1.0f, 1.0f);
            tmpX  = Mathf.Clamp(curAc.x * sensH, -1.0f, 1.0f);
#else
			tmpX  = Input.GetAxis("Horizontal") * xSpeed * Time.deltaTime;
			tmpY  = Input.GetAxis("Vertical") * ySpeed * Time.deltaTime;
#endif

			// get Z axis input --> zoom
			float tmpZ = 0.0f;
#if UNITY_ANDROID
			if (Input.touchCount == 2)
			{
				// store both touches
				Touch touchZero = Input.GetTouch(0);
				Touch touchOne  = Input.GetTouch(1);
				
				// calculate the position of all touches in the previous frame
				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos  = touchOne.position  - touchOne.deltaPosition;
				
				// find the magnitude of the vector (the distance) between the touches in each frame
				float prevTouchDeltaMag  = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float actTouchDeltaMag   = (touchZero.position - touchOne.position).magnitude;
				
				// find the difference in the distances between each frame
				float deltaMagnitudeDiff = prevTouchDeltaMag - actTouchDeltaMag;

				tmpZ = deltaMagnitudeDiff * 0.20f;
			}
#else
			if (Input.GetKey (KeyCode.PageDown))
				tmpZ =  0.20f;
			else if (Input.GetKey (KeyCode.PageUp))
				tmpZ = -0.20f;
#endif

			// do not perform unneccessary calculations...
			if (tmpX == 0.0f && tmpY == 0.0f && tmpZ == 0.0f)
				return;

			MoveAndRotateCam (tmpX, tmpY, tmpZ);
		}
	}

	/**
	 * @param select
	 */
	bool LoadAndShowShip ()
	{
		GameObject tmpShip = null;
		ePlayerShips selectedShip = RuntimeContext.GetInst ().nesActPlayer.actShip;
		switch (selectedShip)
		{
		case ePlayerShips.AK5:
			tmpShip = refPlayerShipAK5;
			break;
			
		default:
			tmpShip = refPlayerShipDefault;
			break;
		}
		
		refPlayerInstance = Instantiate (tmpShip, new Vector3 (0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
		if (refPlayerInstance != null)
		{
			refPlayerController = refPlayerInstance.GetComponent <PlayerController>();
			if (refPlayerController == null)
			{
				Debug.LogError ("SHIP SELECTION >> FAILED to obtain reference to PlayerController Class");
				return false;
			}
			
			bool playerOK = refPlayerController.InitShip (
											RuntimeContext.GetInst ().nesActPlayer.actLifes,
			                                RuntimeContext.GetInst ().nesActPlayer.actHealth,
			                                0.0f,//RuntimeContext.GetInst ().nesActPlayer.actShield,
			                                RuntimeContext.GetInst ().nesActPlayer.actEnergy,
			                                RuntimeContext.GetInst ().nesActPlayer.actEnergyType,
			                                RuntimeContext.GetInst ().nesActPlayer.topWeaponType,
			                                RuntimeContext.GetInst ().nesActPlayer.topAmmoType);
			if (playerOK)
			{
				refPlayerController.gameObject.SetActive (true);

				// just show, do not move the ship
				refPlayerController.nesEngiSys.Init (0.0f, 0.0f, false);

				// disable engines particle system --> it does not look good in this mode
				GameObject [] engines = GameObject.FindGameObjectsWithTag ("Engine");
				for (int i = 0; i < engines.Length; i++)
					engines[i].SetActive (false);

				if (Statistics.GetInst ().nesTotScore.totalScore >= RuntimeContext.GetInst ().nesActPlayer.scoreToUnlock)
					refTextUnlock.text = "The ship is UNLOCKED";
				else
					refTextUnlock.text = "Score to unlock: " + RuntimeContext.GetInst ().nesActPlayer.scoreToUnlock;
				refTextLifes.text   = "Max Lifes: "  + RuntimeContext.GetInst ().nesActPlayer.actLifes;
				refTextHealth.text  = "Max Health: " + RuntimeContext.GetInst ().nesActPlayer.topHealth;
				refTextShield.text  = "Max Shield: " + RuntimeContext.GetInst ().nesActPlayer.topShield;
				refTextAmmo.text 	= "Top Ammo: " 	 + RuntimeContext.GetInst ().nesActPlayer.topAmmoType;
				refTextWeapons.text = "Top Weapons: " + RuntimeContext.GetInst ().nesActPlayer.topWeaponType;
			}
			else
			{
				refTextUnlock.text	= "";
				refTextLifes.text   = "";
				refTextHealth.text  = "";
				refTextShield.text  = "";
				refTextAmmo.text 	= "";
				refTextWeapons.text = "";

				Debug.LogError ("SHIP SELECTION >> FAILED to init player");
				return false;
			}
		}
		else
		{
			Debug.LogError ("SHIP SELECTION >> FAILED to instantiate player GameObject");
			return false;
		}

		// get position of target object
		refTargetTransform = refPlayerInstance.transform;

		return true;
	}

	/**
	 * @param tmpX
	 * @param tmpY
	 * @param tmpZ
	 */
	void MoveAndRotateCam (float tmpX, float tmpY, float tmpZ)
	{
		x += tmpX;
		y += tmpY;
		
		// ensure correct vertical angle of camera
		y = ClampAngle(y, yMinLimit, yMaxLimit);
		
		// create rotation from x and y
		Quaternion rotation = Quaternion.Euler(y, x, 0);

		// calculate zoom distance
		distance = Mathf.Clamp(distance - tmpZ, distanceMin, distanceMax);
		
		// calculate distance of camera from the object
		RaycastHit hit;
		if (Physics.Linecast (refTargetTransform.position, refMainCamera.transform.position, out hit))
		{
			distance -=  hit.distance;
		}
		
		Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
		Vector3 position = rotation * negDistance + refTargetTransform.position;

#if SHIP_SELECT_ROTATE_PLAYER
		// set new rotation of player
		refTargetTransform.rotation = rotation;
#else
		// set new rotation and position of camera
		refMainCamera.transform.rotation = rotation;
		refMainCamera.transform.position = position;

		// set new rotation and position of background
		background.transform.position   = position * -2.0f;
		background.transform.rotation   = rotation;
		background.transform.localScale = new Vector3 (distance * 2.0f, distance * 4.0f, 1.0f);

		// set
		spotlight.transform.position = position;
		spotlight.transform.rotation = rotation;
#endif
	}

    /**
	 * 
	 */
    void ResetAccelerometerAxes()
    {
#if UNITY_ANDROID
        zeroAc  = Input.acceleration;
        curAc   = Vector3.zero;
#endif
    }
	
	/**
	 * @param angle
	 * @param min
	 * @param max
	 */
	static float ClampAngle (float angle, float min, float max)
	{
		if (angle  < -360.0f)
			angle +=  360.0f;
		if (angle  >  360.0f)
			angle -=  360.0f;

		return Mathf.Clamp(angle, min, max);
	}

	/**
	 * 
	 */
	void CalcButtonRects ()
	{
		screenWidth  = Screen.width;
		screenHeight = Screen.height;

		// right now, we have 3 buttons
		showedButtons  	 = 2;

		int buttonWidth  = (int)(screenWidth * 0.075);
		int buttonHeight = (int)(screenHeight * 0.40);;
		
		int buttonPosX	 = (int)(screenWidth * 0.025);
		int buttonPosY	 = (int)(screenHeight * 0.30);

		// calculate final rectangles of all buttons
		realButtonPos    = new Rect[showedButtons];
		realButtonPos[0] = new Rect (buttonPosX, 							   buttonPosY, buttonWidth, buttonHeight);
		realButtonPos[1] = new Rect (screenWidth - (buttonWidth + buttonPosX), buttonPosY, buttonWidth, buttonHeight);
	}
}

