using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour {
	public Transform grabbingPoint;
	public float grabbingPinky, grabbingRing, grabbingMiddle, grabbingIndex, grabbingThumb;

	private Transform handPoint = null;
	private bool mirrorX; // Position of grab point is mirrored on the X axis if grabbed by left hand.
	private Transform mirroredGrabbingPoint;

	public float velocityAlpha;
	public float angularVelocityAlpha;
	public bool enableMovementTracking;
	private Vector3 lastPosition;
	private Quaternion lastRotation;
	private Vector3 currentVelocity;
	private Vector3 currentAngularVelocity;
	private Rigidbody rigidBody;
	private bool beenGrabbed;

	// Use this for initialization
	void Start () {
		// Create mirrored grabbing point for the left hand.
		GameObject mirroredGrabbingPointContainer = new GameObject ("Mirrored Grabbing Point Container");
		GameObject mirroredGrabbingPointObject = new GameObject ("Mirrored Grabbing Point");
		mirroredGrabbingPointObject.transform.position = grabbingPoint.transform.position;
		mirroredGrabbingPointObject.transform.rotation = grabbingPoint.transform.rotation;

		mirroredGrabbingPointContainer.transform.parent = transform;
		mirroredGrabbingPointContainer.transform.localPosition = Vector3.zero;
		mirroredGrabbingPointContainer.transform.localRotation = Quaternion.identity;

		mirroredGrabbingPointContainer.transform.localScale = new Vector3 (1, 1, 1);
		mirroredGrabbingPointObject.transform.parent = mirroredGrabbingPointContainer.transform;
		mirroredGrabbingPointContainer.transform.localScale = new Vector3 (-1, 1, 1);
		mirroredGrabbingPointObject.transform.parent = transform;
		GameObject.Destroy (mirroredGrabbingPointContainer);
		mirroredGrabbingPoint = mirroredGrabbingPointObject.transform;
		// End creating mirrored grabbing point.

		if (enableMovementTracking) {
			rigidBody = GetComponent<Rigidbody> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (beingGrabbed) {
			SnapToHand ();
			if (enableMovementTracking) {
				Vector3 targetVelocity = (transform.localPosition - lastPosition) / Time.deltaTime;
				Quaternion deltaRotation = transform.rotation * Quaternion.Inverse (lastRotation);
				Vector3 deltaEuler = deltaRotation.eulerAngles;
				Vector3 targetAngularVelocity = new Vector3 (Mathf.DeltaAngle (0, deltaEuler.x), Mathf.DeltaAngle (0, deltaEuler.y), Mathf.DeltaAngle (0, deltaEuler.z))
				                               * Mathf.PI / 180 / Time.deltaTime;
				currentVelocity = (1 - velocityAlpha) * rigidBody.velocity + velocityAlpha * targetVelocity;
				currentAngularVelocity = (1 - angularVelocityAlpha) * rigidBody.angularVelocity + angularVelocityAlpha * targetAngularVelocity;
				lastPosition = transform.position;
				lastRotation = transform.rotation;
			}
		}
		beenGrabbed = beingGrabbed;
	}

	private void SnapToHand() {
		Transform grabbingPointTransform = mirrorX ? mirroredGrabbingPoint : grabbingPoint;

		transform.rotation = handPoint.rotation;
		transform.rotation = deltaQuaternion (grabbingPointTransform.rotation, transform.rotation) * transform.rotation;
		transform.position = handPoint.position -
			(grabbingPointTransform.position - transform.position);
	}

	private Quaternion deltaQuaternion(Quaternion q1, Quaternion q2) {
		return q2 * Quaternion.Inverse (q1);
	}

	public void Grab(Transform handTransform, bool mirrored) {
		handPoint = handTransform;
		mirrorX = mirrored;

		SnapToHand ();
		if (enableMovementTracking) {
			lastPosition = transform.position;
			lastRotation = transform.rotation;
			rigidBody.velocity = new Vector3 (0, 0, 0);
			rigidBody.angularVelocity = new Vector3 (0, 0, 0);
			rigidBody.isKinematic = true;
		}
	}

	public void Release() {
		handPoint = null;
		if (enableMovementTracking) {
			rigidBody.isKinematic = false;
			rigidBody.velocity = currentVelocity;
			rigidBody.angularVelocity = currentAngularVelocity;
		}
	}

	public bool beingGrabbed {
		get { return handPoint != null; }
	}
}
