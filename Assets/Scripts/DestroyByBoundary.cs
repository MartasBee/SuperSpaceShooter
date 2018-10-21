using UnityEngine;
using System.Collections;

/**
 * Tato trieda patri GameObjektu `Boundary`. Boundery pouziva Box Collider na identifikaciu
 * kolizii inych objektov s Boundary. Boundary je zaroven Trigger Collider, co znamena,
 * ze ine objekty mozu cez Boundery prechadzat (t.z. ze Boundary ma "transparentny material").
 * --> Pokial nastane kolizia ineho objektu (t.j. objektu `other` daneho ako parameter tejto metody)
 * s Boundary objektom, zavola sa metoda OnTriggerEnter.
 */
public class DestroyByBoundary : MonoBehaviour
{
	/**
	 * Po "skonceni" kolizneho stavu je zavolana metoda OnTriggerExit. Ta sposobi,
	 * ze objekt `other` bude zniceny --> Pretoze objekt 'other' opustil hernu scenu.
	 */
	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
			return;

		Destroy(other.gameObject);
	}
}
