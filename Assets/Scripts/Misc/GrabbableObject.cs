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
		print (handPoint.position);
		transform.rotation = deltaQuaternion(transform.rotation, grabbingPoint.rotation)
				* handPoint.rotation;
		transform.position = handPoint.position -
				(grabbingPoint.position - transform.position);
		transform.position = handPoint.position;
	}

	private Quaternion deltaQuaternion(Quaternion q1, Quaternion q2) {
		return q1 * Quaternion.Inverse (q2);
	}

	public void Grab(Transform handTransform) {
		handPoint = handTransform;
	}

	public void Release() {
		handPoint = null;
	}
}
