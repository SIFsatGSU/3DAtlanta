using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour {
	public Transform grabbingPoint;
	public Transform leftHandGrabbingPoint;
	public Renderer grabbableIndicator;
	public float grabbingPinky, grabbingRing, grabbingMiddle, grabbingIndex, grabbingThumb;

	public float velocityAlpha;
	public float angularVelocityAlpha;
	public bool enableMovementTracking;

	[HideInInspector]
	public bool[] grabbableIndication;

	private Transform handPoint = null;
	private bool mirrorX; // Position of grab point is mirrored on the X axis if grabbed by left hand.

	private Vector3 lastPosition;
	private Quaternion lastRotation;
	private Vector3 currentVelocity;
	private Vector3 currentAngularVelocity;
	private Rigidbody rigidBody;
	private HandController grabbingHand;

	// Use this for initialization
	void Start () {
		grabbableIndication = new bool[2];
		if (leftHandGrabbingPoint == null) {
			// Create mirrored grabbing point for the left hand if not specified.
			// Assuming object is symmetrical about the X axis.
			GameObject leftHandGrabbingPointContainer = new GameObject ("Left Hand Grabbing Point Container");
			GameObject leftHandGrabbingPointObject = new GameObject ("Left Hand Grabbing Point");
			leftHandGrabbingPointObject.transform.position = grabbingPoint.transform.position;
			leftHandGrabbingPointObject.transform.rotation = grabbingPoint.transform.rotation;

			leftHandGrabbingPointContainer.transform.parent = transform;
			leftHandGrabbingPointContainer.transform.localPosition = Vector3.zero;
			leftHandGrabbingPointContainer.transform.localRotation = Quaternion.identity;

			leftHandGrabbingPointContainer.transform.localScale = new Vector3 (1, 1, 1);
			leftHandGrabbingPointObject.transform.parent = leftHandGrabbingPointContainer.transform;
			leftHandGrabbingPointContainer.transform.localScale = new Vector3 (-1, 1, 1);
			leftHandGrabbingPointObject.transform.parent = transform;
			GameObject.Destroy (leftHandGrabbingPointContainer);
			leftHandGrabbingPoint = leftHandGrabbingPointObject.transform;
			// End creating mirrored grabbing point.
		}
		if (enableMovementTracking) {
			rigidBody = GetComponent<Rigidbody> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (grabbableIndicator != null && grabbableIndicator.enabled != (grabbableIndication[0] || grabbableIndication[1])) {
			grabbableIndicator.enabled = (grabbableIndication[0] || grabbableIndication[1]);
		}
	}

	// To be called by the hand controller to synchronize with the hand movement.
	public void HandUpdate() {
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

	private void SnapToHand() {
		Transform grabbingPointTransform = mirrorX ? leftHandGrabbingPoint : grabbingPoint;

		transform.rotation = Quaternion.identity;
		transform.rotation = handPoint.rotation * Quaternion.Inverse(grabbingPointTransform.rotation);
		transform.position = handPoint.position -
			(grabbingPointTransform.position - transform.position);
	}

	public void Grab(Transform handTransform, bool mirrored, HandController hand) {
		handPoint = handTransform;
		mirrorX = mirrored;
		grabbingHand = hand;

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
		grabbingHand = null;

		if (enableMovementTracking) {
			rigidBody.isKinematic = false;
			rigidBody.velocity = currentVelocity;
			rigidBody.angularVelocity = currentAngularVelocity;
		}
	}

	public bool beingGrabbed {
		get { return handPoint != null; }
	}

	public HandController hand {
		get { return grabbingHand; }
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "Off World Trigger") {
			Vector3 temp = transform.position;
			temp.y = .3f;
			transform.position = temp;
			GetComponent<Rigidbody> ().velocity = Vector3.zero;
		}
	}
}
