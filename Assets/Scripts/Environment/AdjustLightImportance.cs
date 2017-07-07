using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustLightImportance : MonoBehaviour {
	public Transform cameraTransform;
	public float importantLightRadius;
	public float updateInterval;
	public float importanceConeAngle;
	public float importanceConeMinDistance;
	private float updateTimeElapsed;
	private Light thisLight;
	private float cosMaxAngle;

	void Start() {
		thisLight = GetComponent<Light> ();
		cosMaxAngle = Mathf.Cos (Mathf.Deg2Rad * importanceConeAngle / 2);
	}

	void Update () {
		// cosMaxAngle = Mathf.Cos (Mathf.Deg2Rad * importanceConeAngle / 2);
		// ^ That was for testing.
		if (updateTimeElapsed >= updateInterval) {
			updateTimeElapsed = 0;
			ControlLight ();
		} else {
			updateTimeElapsed += Time.deltaTime;
		}
	}

	void ControlLight() {
		Vector3 camToLight = transform.position - cameraTransform.position;
		float distance = camToLight.magnitude;
		// If light lays in the cone or closer than min distance.
		if (distance < importantLightRadius) {
			if (distance <= importanceConeMinDistance ||
					Vector3.Dot(camToLight, cameraTransform.forward) / distance >= cosMaxAngle) {
				thisLight.enabled = true;
				return;
			}
		}
		thisLight.enabled = false;
	}
}
