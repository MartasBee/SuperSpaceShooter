using UnityEngine;
using System.Collections;

public class ShieldController : MonoBehaviour
{
	//==================================================================================================

	// have to be set up from Unity editor - reference to player ship game object
	public GameObject refPlayerShip;

	public float shieldRotator = 15.0f;

	// reference to player controller class
	private PlayerController refPlayerCtrl;

	//==================================================================================================

	void Start ()
	{
		if (refPlayerShip != null)
		{
			refPlayerCtrl = refPlayerShip.GetComponent <PlayerController>();
			if (refPlayerCtrl == null)
			{
				Debug.LogError ("SHIELD >> Failed to obtain reference to PlayerController class");
				gameObject.SetActive (false);
				return;
			}
		}
		else
		{
			Debug.LogError ("SHIELD >> Reference to GameObject 'Player Ship' was not set");
			gameObject.SetActive (false);
			return;
		}

		if (!refPlayerCtrl.nesCoreSys.nesShield.IsShieldActive ())
			gameObject.SetActive (false);

		rigidbody.angularVelocity = Random.insideUnitSphere * shieldRotator;
	}

	void FixedUpdate ()
	{
		if (refPlayerCtrl == null)
		{
			gameObject.SetActive (false);
			return;
		}

		rigidbody.velocity = refPlayerCtrl.gameObject.rigidbody.velocity;
		rigidbody.position = refPlayerCtrl.gameObject.rigidbody.position;
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Boundary" || other.tag == "Bonus" || other.tag == "Weapon")
		{
			// 1) ignore BOUNDARY
			// 2) let BONUS go to ship
			// 3) let WEAPONS go to ship
			return;
		}

		if (other.tag == "Bullet")
		{
			// call player's ship collision method
			refPlayerCtrl.OnTriggerEnter (other);

			if (!refPlayerCtrl.nesCoreSys.nesShield.IsShieldActive ())
				gameObject.SetActive (false);
		}
	}

	void OnCollisionEnter (Collision collision)
	{
		if (collision.collider.tag == "Asteroid" || collision.collider.tag == "Enemy")
		{
			// something should happen here..
			//Debug.Log ("SHIELD >> Collision >> Player's SHIELD was hit by \"" + collision.collider.name + "\"");
		}
	}

	//==================================================================================================
}
