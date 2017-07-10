using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRControllerCheck : MonoBehaviour {
    public int numberOfFramesTested;
	public Animator chronolensAnimator;
	public GameObject leftHand, rightHand;
	public ChronolensController chronolensController;
	private bool set = false;

    // Use this for initialization
	void Update () {
        if (numberOfFramesTested > 0) {
            if (OVRInput.IsControllerConnected(OVRInput.Controller.LTouch) &&
                    OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) && !set) {
				GameManager.oculusControllerMode = true;
				chronolensAnimator.enabled = false;
				chronolensController.InitializeHapticsClips ();
                set = true;
                numberOfFramesTested = 0;
            } else {
                numberOfFramesTested--;
            }
		} else {
			if (!GameManager.oculusControllerMode) {
				GameObject.Destroy (leftHand);
				GameObject.Destroy (rightHand);
			}
		}
	}
}
