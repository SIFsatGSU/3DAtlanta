using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class HandController : MonoBehaviour {
	public VRNode hand;
	HandAnimator handAnimator;
	public float fingerAnimationStart;
	public float grabThreshold;
	public Collider bodyCollider;
	public Collider feetCollider;

	private OVRInput.RawNearTouch triggerTouch, thumbTouch;
	private OVRInput.RawAxis1D triggerAxis, fistAxis;
	private bool grabMode = false;
	private HashSet<GameObject> grabObjectSet = new HashSet<GameObject>();
	private bool previouslyGrabbed = false;
	private GameObject currentlyGrabbed;

	// Use this for initialization
	void Start () {
		// Ignore collision with body.
		Physics.IgnoreCollision(GetComponentInChildren<Collider>(), bodyCollider);

		handAnimator = GetComponent<HandAnimator> ();
		if (hand == VRNode.RightHand) {
			triggerTouch = OVRInput.RawNearTouch.RIndexTrigger;
			thumbTouch = OVRInput.RawNearTouch.RThumbButtons;
			triggerAxis = OVRInput.RawAxis1D.RIndexTrigger;
			fistAxis = OVRInput.RawAxis1D.RHandTrigger;
		} else if (hand == VRNode.LeftHand) {
			triggerTouch = OVRInput.RawNearTouch.LIndexTrigger;
			thumbTouch = OVRInput.RawNearTouch.LThumbButtons;
			triggerAxis = OVRInput.RawAxis1D.LIndexTrigger;
			fistAxis = OVRInput.RawAxis1D.LHandTrigger;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.oculusControllerMode) {
			Vector3 handPosition = InputTracking.GetLocalPosition (hand);
			if (handPosition.sqrMagnitude != 0) { // If hands are being tracked, hand position != (0, 0, 0).
				transform.localPosition = InputTracking.GetLocalPosition (hand);
			}
			transform.localRotation = InputTracking.GetLocalRotation (hand);

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
				handAnimator.thumb = OVRInput.Get (thumbTouch) ? 1 : 0;

				bool currentGrab = OVRInput.Get (fistAxis) >= grabThreshold;
				if (currentGrab && !previouslyGrabbed) {
					if (grabObjectSet.Count > 0) {
						float minDistance = float.MaxValue;
						float currentDistance;
						foreach (GameObject currentObject in grabObjectSet) {
							currentDistance = (currentObject.transform.position - transform.position).sqrMagnitude;
							if (currentDistance < minDistance) {
								currentlyGrabbed = currentObject;
								minDistance = currentDistance;
							}
						}
						currentlyGrabbed.GetComponent<GrabbableObject> ().Grab (transform);
					}
				}
				if (!currentGrab && previouslyGrabbed) {
					if (currentlyGrabbed != null) {
						currentlyGrabbed.GetComponent<GrabbableObject> ().Release ();
						currentlyGrabbed = null;
					}
				}
			}

			previouslyGrabbed = OVRInput.Get (fistAxis) >= grabThreshold;
		}
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "Grabbable") {
			grabObjectSet.Add (col.gameObject);
		}
	}

	void OnTriggerExit(Collider col) {
		if (grabObjectSet.Contains(col.gameObject)) {
			grabObjectSet.Remove(col.gameObject);
		}
	}
}
