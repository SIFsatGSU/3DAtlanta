using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

[ExecuteInEditMode]
public class HandAnimator : MonoBehaviour {
	public float animationSpeed; // Normalized speed.
	[HideInInspector]
	public float pinky, ring, middle, index, thumb;
	private float currentPinky = 0, currentRing = 0, 
			currentMiddle = 0, currentIndex = 0, currentThumb = 0;
	private Animator handAnimator;

	// Use this for initialization
	void Start () {
		handAnimator = GetComponentInChildren<Animator>();
	}

	// Update is called once per frame
	void Update () {
		//print (GameManager.oculusControllerMode);
		if (GameManager.oculusControllerMode) {
			MoveToTarget (ref currentPinky, pinky);
			MoveToTarget (ref currentRing, ring);
			MoveToTarget (ref currentMiddle, middle);
			MoveToTarget (ref currentIndex, index);
			MoveToTarget (ref currentThumb, thumb);
			handAnimator.Play ("Pinky", 0, currentPinky);
			handAnimator.Play ("Ring", 1, currentRing);
			handAnimator.Play ("Middle", 2, currentMiddle);
			handAnimator.Play ("Index", 3, currentIndex);
			handAnimator.Play ("Thumb", 4, currentThumb);
		}
	}

	void MoveToTarget(ref float current, float target) {
		if (Mathf.Abs(target - current) > animationSpeed) {
			current += animationSpeed * (target - current) / Mathf.Abs(target - current);
		} else {
			current = target;
		}
	}

	public void SnapAnimations() {
		currentPinky = pinky;
		currentRing = ring;
		currentMiddle = middle;
		currentIndex = index;
		currentThumb = thumb;
	}
}
