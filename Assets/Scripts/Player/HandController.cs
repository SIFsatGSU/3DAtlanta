using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class HandController : MonoBehaviour {
	private const float MIN_POTENTIAL = -.5f;

	public UnityEngine.XR.XRNode hand;
	HandAnimator handAnimator;
	public float fingerAnimationStart;
	public float grabThreshold;
	public Collider bodyCollider;
	public Collider feetCollider;
	public Transform palmDirection; // To calculate grabbing potential.
	private int grabbableIndicatorIndex; // To seperate indicators of left and right hand on the object.

	private OVRInput.RawNearTouch triggerTouch, thumbTouch;
	private OVRInput.RawAxis1D triggerAxis, fistAxis;
	private	OVRHaptics.OVRHapticsChannel hapticsChannel;
	private bool grabMode = false;
	private HashSet<GameObject> grabObjectSet = new HashSet<GameObject>();
	private bool previouslyGrabbed = false;
	private GameObject currentlyGrabbed;
	private GrabbableObject currentGrabbableObject;
	private float grabbingPinky, grabbingRing, grabbingMiddle, grabbingIndex, grabbingThumb;
	// Use this for initialization
	void Start () {
		// Ignore collision with body.
		Physics.IgnoreCollision(GetComponentInChildren<Collider>(), bodyCollider);
		Physics.IgnoreCollision(GetComponentInChildren<Collider>(), feetCollider);

		handAnimator = GetComponent<HandAnimator> ();
		if (hand == UnityEngine.XR.XRNode.RightHand) {
			triggerTouch = OVRInput.RawNearTouch.RIndexTrigger;
			thumbTouch = OVRInput.RawNearTouch.RThumbButtons;
			triggerAxis = OVRInput.RawAxis1D.RIndexTrigger;
			fistAxis = OVRInput.RawAxis1D.RHandTrigger;
			hapticsChannel = OVRHaptics.RightChannel;
			grabbableIndicatorIndex = 0;
		} else if (hand == UnityEngine.XR.XRNode.LeftHand) {
			triggerTouch = OVRInput.RawNearTouch.LIndexTrigger;
			thumbTouch = OVRInput.RawNearTouch.LThumbButtons;
			triggerAxis = OVRInput.RawAxis1D.LIndexTrigger;
			fistAxis = OVRInput.RawAxis1D.LHandTrigger;
			hapticsChannel = OVRHaptics.LeftChannel;
			grabbableIndicatorIndex = 1;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.oculusControllerMode) {
			Vector3 handPosition = UnityEngine.XR.InputTracking.GetLocalPosition (hand);
			if (handPosition.sqrMagnitude != 0) { // If hands are being tracked, hand position != (0, 0, 0).
				transform.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition (hand);
			}
			transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation (hand);

			bool currentGrab = OVRInput.Get (fistAxis) >= grabThreshold;

			if (!grabMode) {
				if (!OVRInput.Get (triggerTouch)) {
					handAnimator.index = 0;
				} else {
					handAnimator.index = Mathf.Lerp (fingerAnimationStart, 1,
						OVRInput.Get (triggerAxis));
				}
				float fistValue = Mathf.Lerp (fingerAnimationStart, 1,
					                  OVRInput.Get (fistAxis));
				handAnimator.middle = handAnimator.ring = handAnimator.pinky = fistValue;
				if (OVRInput.Get (thumbTouch)) {
					if (OVRInput.Get (triggerAxis) > .9 && OVRInput.Get (fistAxis) > .9) {
						handAnimator.thumb = 1;
					} else {
						handAnimator.thumb = .55f;
					}
				} else {
					handAnimator.thumb = 0;
				}

				// Check for grabbable object candidates.
				GameObject candidateGrabbableObject = null;
				if (grabObjectSet.Count > 0) {
					float maxPotential = float.MinValue;
					float potential;
					Vector3 deltaPosition;
					foreach (GameObject currentObject in grabObjectSet) {
						deltaPosition = currentObject.transform.position - transform.position;
						currentObject.GetComponent<GrabbableObject> ().grabbableIndication[grabbableIndicatorIndex] = false;
						potential = Vector3.Dot (-palmDirection.forward, deltaPosition.normalized);
						if (potential >= MIN_POTENTIAL && potential > maxPotential && !currentObject.GetComponent<GrabbableObject>().beingGrabbed) {
							if (candidateGrabbableObject != null) 
								candidateGrabbableObject.GetComponent<GrabbableObject> ().grabbableIndication[grabbableIndicatorIndex] = false;
							candidateGrabbableObject = currentObject;
							candidateGrabbableObject.GetComponent<GrabbableObject> ().grabbableIndication[grabbableIndicatorIndex] = true;
							maxPotential = potential;
						}
					}
				}

				// Start grabbing.
				if (currentGrab && !previouslyGrabbed) {
					if (candidateGrabbableObject != null) {
						currentlyGrabbed = candidateGrabbableObject;
						currentGrabbableObject = currentlyGrabbed.GetComponent<GrabbableObject> ();
						if (!currentGrabbableObject.beingGrabbed) {
							currentGrabbableObject.grabbableIndication[grabbableIndicatorIndex] = false;
							currentGrabbableObject.Grab (transform, hand == UnityEngine.XR.XRNode.LeftHand, this);
							grabMode = true;

							// Official.
							handAnimator.pinky = currentGrabbableObject.grabbingPinky;
							handAnimator.ring = currentGrabbableObject.grabbingRing;
							handAnimator.middle = currentGrabbableObject.grabbingMiddle;
							handAnimator.index = currentGrabbableObject.grabbingIndex;
							handAnimator.thumb = currentGrabbableObject.grabbingThumb;
							handAnimator.SnapFingers ();
						}
					}
				}

			} else {
				// For testing
				/*handAnimator.pinky = currentGrabbableObject.grabbingPinky;
				handAnimator.ring = currentGrabbableObject.grabbingRing;
				handAnimator.middle = currentGrabbableObject.grabbingMiddle;
				handAnimator.index = currentGrabbableObject.grabbingIndex;
				handAnimator.thumb = currentGrabbableObject.grabbingThumb;*/
				// ^ End for testing

				currentGrabbableObject.HandUpdate();
				if (!currentGrab && previouslyGrabbed) {
					if (currentlyGrabbed != null) {
						currentGrabbableObject.Release ();
						currentlyGrabbed = null;
					}
					grabMode = false;
				}
			}

			previouslyGrabbed = OVRInput.Get (fistAxis) >= grabThreshold;
		}
	}

	public void Vibrate(OVRHapticsClip clip) {
		hapticsChannel.Preempt (clip);
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "Grabbable") {
			grabObjectSet.Add (col.gameObject);
		}
	}

	void OnTriggerExit(Collider col) {
		if (grabObjectSet.Contains(col.gameObject)) {
			grabObjectSet.Remove(col.gameObject);
			col.gameObject.GetComponent<GrabbableObject> ().grabbableIndication[grabbableIndicatorIndex] = false;
		}
	}
}
