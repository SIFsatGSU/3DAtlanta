using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetCollider : MonoBehaviour {
	GameObject currentObject;

	void OnCollisionEnter(Collision col) {
		print ("wab " + col.gameObject);
		currentObject = col.gameObject;
		transform.parent.gameObject.GetComponent<Movement> ().LandOnGround ();
	}

	void OnCollisionExit(Collision col) {
		print (col.gameObject);
		if (col.gameObject == currentObject) {
			transform.parent.gameObject.GetComponent<Movement> ().grounded = false;
		}
	}
}
