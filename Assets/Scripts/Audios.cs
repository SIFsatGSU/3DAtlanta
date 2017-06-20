using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audios {
	public static void PlayAudio(AudioSource audio) { // Play audio if not already playing.
		if (!audio.isPlaying) {
			audio.Play ();
		}
	}
}
