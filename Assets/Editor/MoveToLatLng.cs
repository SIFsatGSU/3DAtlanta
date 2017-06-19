/* This wizard will replace a selection with an object or prefab.
 * Scene objects will be cloned (destroying their prefab links).
 * Original coding by 'yesfish', nabbed from Unity Forums
 * 'keep parent' added by Dave A (also removed 'rotation' option, using localRotation
 */
using UnityEngine;
using System.Text.RegularExpressions;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class MoveToLatLng : ScriptableWizard {
	public string latLngString;

    [MenuItem("GameObject/Move to Lat Lng")]
    static void CreateWizard() {
        ScriptableWizard.DisplayWizard(
			"Move to Lat Lng", typeof(MoveToLatLng), "Move");
    }

	void OnWizardUpdate() {
		if (latLngString == null)
			return;
		if (!Regex.IsMatch(latLngString, "[\\-0-9.]\\s*,\\s*[\\-0-9.]")) {
			errorString = "Please input lat lng string in the format: lat,lng";
		}
	}

    void OnWizardCreate() {
		string[] latLngStrings = Regex.Split (latLngString, "\\s*,\\s*");
		if (latLngStrings.Length != 2)
			return;
		float lat, lng;
		try {
			lat = float.Parse(latLngStrings[0]);
			lng = float.Parse(latLngStrings[1]);
		} catch (Exception e) {
			return;
		}
		Vector3 position = Locations.LatLngToPosition (new Vector3 (lng, 0, lat));
        Transform[] transforms = Selection.GetTransforms(
            SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);
		foreach (Transform transform in transforms) {
			transform.position = position;
		}
    }
}