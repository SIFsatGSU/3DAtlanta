using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbeInterval : MonoBehaviour {
	public float interval;
	private DateTime lastProbed = new DateTime();
	private ReflectionProbe reflectionProbe;

	void Start() {
		reflectionProbe = GetComponent<ReflectionProbe> ();
	}

	// Update is called once per frame
	void Update () {
		if ((DateTime.UtcNow - lastProbed).TotalMilliseconds > interval) {
			reflectionProbe.RenderProbe ();
			print ("boop");
			lastProbed = DateTime.UtcNow;
		}
	}
}
