using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRControllerCheck : MonoBehaviour {
    public int numberOfFramesTested;
	public Animator chronolensAnimator;

	private bool set = false;

    // Use this for initialization
	void Update () {
        if (numberOfFramesTested > 0) {
            if (OVRInput.IsControllerConnected(OVRInput.Controller.LTouch) &&
                    OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) && !set) {
				GameManager.oculusControllerMode = true;
				chronolensAnimator.enabled = false;
                set = true;
                numberOfFramesTested = 0;
            } else {
                numberOfFramesTested--;
            }
        }
	}
}
