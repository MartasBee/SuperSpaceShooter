﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class AsteroidBoundary : Boundary
{
	AsteroidBoundary ()
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


public class AsteroidController : MonoBehaviour
{
	//==================================================================================================

	// asteroid needs "engines"
	public EngineSystems nesMove;

	// what type of score is generated by destroying this object ?
	public eScoreObjectTypes scoreType
	{
		get;
		protected set;
	}
	

	// boundaries of enemy motion
	public EnemyBoundary	boundary;
	// have to be set up from Unity Editor
	public GameObject 		refExplosionAsteroid;


	private float collisionDamages = 0.0f;

	//==================================================================================================

	void Awake ()
	{
		// newly create asteroid is not active till the astroSett.Init is called (!!)
		gameObject.SetActive (false);
	}

	void Start ()
	{
		rigidbody.velocity 			= transform.forward * nesMove.actualSpeed;
		rigidbody.angularVelocity 	= Random.insideUnitSphere * nesMove.randomRotation;
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
			// asteroid was hit by player (!!)

			// fail-safe
			float bullDamage = 50.0f;
			
			BulletController refBullCtrl = other.gameObject.GetComponent <BulletController>();
			if (refBullCtrl != null)
			{
				bullDamage = refBullCtrl.nesBulletSys.GetDamageCollision ();
			}
			else
			{
				Debug.LogWarning ("ASTEROID >> Unable to obtain damage of bullet, which just hit the asteroid...");
			}
			
			// destroy player's bullet
			Destroy (other.gameObject);

			// show explosion caused by hit
			Instantiate (refExplosionAsteroid, transform.position, transform.rotation);

			bool isAlive = HitAsteroid (bullDamage);
						
			if (!isAlive)
				AsteroidDestruction (true);
		}
		else if (other.tag == "Bullet")
		{
			// asteroid was hit by enemy, just show explosion ;)

			// destroy enemy's bullet
			Destroy (other.gameObject);

			// show explosion caused by hit
			Instantiate (refExplosionAsteroid, transform.position, transform.rotation);
		}
		else if (other.tag == "Player")
		{
			// destruction od PLAYER is in Player Controller
			// destroy asteroid's THIS object, add no score
			AsteroidDestruction (false);
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
			Debug.Log ("ASTEROID >> Something just hit the asteroid (!!) >> " + other.name);
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
			Debug.Log ("UNKNOWN >> Collision >> Object \"Asteroid\" collided with \""
			           + collision.collider.name
			           + "\" >> This \"Asteroid\" will not be destroyed");
		}
		
		if (bDestroy)
			AsteroidDestruction (false);
	}

	//==================================================================================================

	/**
	 * @param asWave
	 */
	public bool InitAteroid (int asWave)
	{
		scoreType = eScoreObjectTypes.AsteroidStandard;
		
		// every 5th wave, the asteroid will be faster
		float tmpMove = (asWave / 5) * 0.2f;
		nesMove.Init ((4.0f + tmpMove), (2.0f + tmpMove), 5.0f, 0.0f, false);
		
		collisionDamages = 1000.0f;
		
		return true;
	}
	
	/**
	 * Damages caused by other players when THIS is hit
	 * @param damage
	 */
	bool HitAsteroid (float damage)
	{
		// asteroid is totaly destructed after a hit
		return false;
	}
	
	/**
	 * Damages caused by collision to other objects
	 */
	float GetDamageCollision ()
	{
		return collisionDamages;
	}

	/**
	 * @param addScore
	 */
	public void AsteroidDestruction (bool addScore)
	{
		if (addScore)
			RuntimeContext.GetInst ().AddScore (scoreType);

		// show explosion caused by destruction
		Instantiate (refExplosionAsteroid, transform.position, transform.rotation);

		// called automaticly by explosion animation
		//audio.Play ();
		
		// have to be last call (!!)
		Destroy (gameObject);
	}

	//==================================================================================================
}