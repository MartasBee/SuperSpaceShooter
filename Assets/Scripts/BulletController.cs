using UnityEngine;
using System.Collections;


[System.Serializable]
public class Bullet
{
	public float   	 bulletInitSpeed
	{
		get;
		private set;
	}
	public Vector3 	 shipSpeedDuringShot
	{
		get;
		private set;
	}
	public Transform bulletInitSpawnPoint
	{
		get;
		private set;
	}
	public float   	 bulletInitDamageEnergy
	{
		get;
		private set;
	}
	public float   	 bulletEffectiveShotRange
	{
		get;
		private set;
	}


	private Vector3  actualSpeed;

	//==========================================================================

	/**
	 * @param initSpawnPoint
	 * @param initSpeed
	 * @param shipVelocity
	 * @param initDamage
	 * @param shotRange
	 */
	public bool Init (Transform	initSpawnPoint,
	           		  float 	initSpeed,
	                  Vector3 	shipVelocity,
	                  float		initDamage,
	                  float		shotRange)
	{
		bulletInitSpawnPoint 	 = initSpawnPoint;
		bulletInitSpeed		 	 = initSpeed;
		shipSpeedDuringShot		 = shipVelocity;
		bulletInitDamageEnergy	 = initDamage;
		bulletEffectiveShotRange = shotRange;

		return true;
	}


	/**
	 * 
	 */
	public float GetDamageCollision ()
	{
		return bulletInitDamageEnergy;
	}
	/**
	 * TODO
	 * 
	 * @param currentLocation
	 */
	public float GetDamageCollision (Vector3 currentLocation)
	{
		// flyRange = ABS (currentLocation - bulletInitSpawnPoint)
		// percentOfShotRange = flyRange / bulletEffectiveShotRange
		// if (percentOfShotRange > 1.0) percentOfShotRange = 1.0
		
		return bulletInitDamageEnergy;
	}
}


public class BulletController : MonoBehaviour
{
	//==================================================================================================

	public Bullet nesBulletSys;

	//==================================================================================================

	void Awake ()
	{
		gameObject.SetActive (false);
	}

	void Start ()
	{
		rigidbody.velocity = transform.forward *
			(nesBulletSys.bulletInitSpeed + (nesBulletSys.shipSpeedDuringShot.z / 2.0f));
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Bullet" || other.tag == "BulletPlayer")
		{
			Destroy (other.gameObject);
			Destroy (this.gameObject);
		}
	}

	//==================================================================================================
}
