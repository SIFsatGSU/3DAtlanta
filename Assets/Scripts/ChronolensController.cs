using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChronolensController : MonoBehaviour {
	public Material chronolenseMaterial;
	public Animator lenseAnimator;
	public float alphaChangeSpeed;
	public AudioSource initiateAudio;
	public AudioSource closeAudio;

	private bool keydown = false;
	private bool lenseState = false;
	private string nextState;
	private string currentState;
	private int currentStateLayer;
	private int nextStateLayer;

	void Start() {
		lenseAnimator.Play ("Alpha 0", 2, 1);
	}

	void Update() {
		if (Time.timeScale > 0) {
			if (Input.GetAxisRaw("Chronolens Action") > 0 && !keydown) {
				lenseState = !lenseState;
				if (lenseState) {
					lenseAnimator.Play ("Show", 0, 0);
				} else {
					lenseAnimator.Play ("Hide", 0, 0);
				}
			}
		}
		keydown = Input.GetAxisRaw ("Chronolens Action") > 0;

		if (nextState != "" &&
				lenseAnimator.GetCurrentAnimatorStateInfo (currentStateLayer).normalizedTime >= 1 &&
				lenseAnimator.GetCurrentAnimatorStateInfo (currentStateLayer).IsName(currentState)) {
			lenseAnimator.Play (nextState, nextStateLayer);
			nextState = "";
		}
	}

	public void AreaEnter(ChronolensArea area) {
		chronolenseMaterial.SetTexture ("_CubeMap", area.hdri);
		chronolenseMaterial.SetFloat ("_YawOffset", area.yawOffset);

		currentState = "Initiate";
		currentStateLayer = 1;
		nextState = "Alpha 1";
		nextStateLayer = 2;
		PlayCurrentState ();
		Audios.PlayAudio (initiateAudio);
	}

	public void AreaExit() {
		currentState = "Alpha 0";
		currentStateLayer = 2;
		PlayCurrentState ();
		nextState = "Close";
		nextStateLayer = 1;
		Audios.PlayAudio (closeAudio);
	}

	void PlayCurrentState() {
		lenseAnimator.Play (currentState, currentStateLayer, 0);
	}
}
