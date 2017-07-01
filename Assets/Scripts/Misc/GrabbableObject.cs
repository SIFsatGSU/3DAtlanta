using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour {
	public Transform grabbingPoint;

	private Transform handPoint;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (handPoint != null) {
			SnapToHand ();
		}
	}

	private void SnapToHand() {
		//print (handPoint.position);
		transform.rotation = handPoint.rotation;
		transform.rotation = deltaQuaternion (transform.rotation, grabbingPoint.rotation) * transform.rotation;
		transform.position = handPoint.position -
				(grabbingPoint.position - transform.position);
	}

	private Quaternion deltaQuaternion(Quaternion q1, Quaternion q2) {
		return q2 * Quaternion.Inverse (q1);
	}

	public void Grab(Transform handTransform) {
		handPoint = handTransform;
	}

	public void Release() {
		handPoint = null;
	}
}
