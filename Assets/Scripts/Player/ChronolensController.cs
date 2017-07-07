﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChronolensController : MonoBehaviour {
	public Material chronolenseMaterial;
	public Animator lensActionAnimator, lensShowHideAnimator;
	public float alphaChangeSpeed;
	public AudioSource initiateAudio;
	public AudioSource closeAudio;
	public GameObject chronolens;
	public Transform rightHolster;
	public float chronolensReturnRate;
	private bool keydown = false;
	private bool lenseState = false;
	private string nextState;
	private string currentState;
	private int currentStateLayer;
	private int nextStateLayer;
	private GrabbableObject chronolensGrabbable;

	void Start() {
		lensActionAnimator.Play ("Alpha 0", 1, 1);
		chronolensGrabbable = chronolens.GetComponent<GrabbableObject> ();
	}

	void Update() {
		if (!GameManager.oculusControllerMode) {
			if (Time.timeScale > 0) {
				if (Input.GetAxisRaw ("Chronolens Action") > 0 && !keydown) {
					lenseState = !lenseState;
					if (lenseState) {
						lensShowHideAnimator.Play ("Show", 0, 0);
					} else {
						lensShowHideAnimator.Play ("Hide", 0, 0);
					}
				}
			}
			keydown = Input.GetAxisRaw ("Chronolens Action") > 0;
		}

		if (nextState != "" &&
				lensActionAnimator.GetCurrentAnimatorStateInfo (currentStateLayer).normalizedTime >= 1 &&
				lensActionAnimator.GetCurrentAnimatorStateInfo (currentStateLayer).IsName (currentState)) {
			lensActionAnimator.Play (nextState, nextStateLayer);
			nextState = "";
		}

		if (GameManager.oculusControllerMode) {
			if (!chronolensGrabbable.beingGrabbed) {
				chronolens.transform.position = Vector3.Lerp(chronolens.transform.position,
						rightHolster.position, chronolensReturnRate);
				chronolens.transform.rotation = Quaternion.Lerp (chronolens.transform.rotation,
						rightHolster.rotation, chronolensReturnRate);
			}
		}
	}

	public void AreaEnter(ChronolensArea area) {
		closeAudio.Stop (); //Stop the closing audio if not already.
		chronolenseMaterial.SetTexture ("_CubeMap", area.hdri);
		chronolenseMaterial.SetFloat ("_YawOffset", area.yawOffset);

		currentState = "Initiate";
		currentStateLayer = 0;
		nextState = "Alpha 1";
		nextStateLayer = 1;
		PlayCurrentState ();
		Audios.PlayAudio (initiateAudio);
	}

	public void AreaExit() {
		initiateAudio.Stop (); //Stop the init audio if not already.
		currentState = "Alpha 0";
		currentStateLayer = 1;
		PlayCurrentState ();
		nextState = "Close";
		nextStateLayer = 0;
		Audios.PlayAudio (closeAudio);
	}

	void PlayCurrentState() {
		lensActionAnimator.Play (currentState, currentStateLayer, 0);
	}
}
