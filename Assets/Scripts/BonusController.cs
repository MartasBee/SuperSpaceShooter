using UnityEngine;
using System.Collections;

public class BonusController : MonoBehaviour
{
	//==================================================================================================

	public string bonusType  = "";
	public string bonusMode	 = "";
	public int	  bonusValue = 25;

	public bool	  rotateObj  = true;

	public EngineSystems nesMove;
	public EnemyBoundary boundary;

	//==================================================================================================

	// Use this for initialization
	void Start ()
	{
		nesMove.Init (4.0f, 2.0f, 5.0f, 0.0f, false);

		rigidbody.velocity = transform.forward * nesMove.actualSpeed;
		if (rotateObj)
			rigidbody.angularVelocity = Random.insideUnitSphere * nesMove.randomRotation;
	}

	//==================================================================================================
}
