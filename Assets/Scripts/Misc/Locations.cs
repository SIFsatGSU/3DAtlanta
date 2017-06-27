using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locations {
	public static Vector3 LatLngToPosition(Vector3 latLng) {
		Vector3[] vectors = GetVectorsFromReferencePoint ();
		return ConvertSpaces (latLng, vectors [2], vectors [3], vectors [0], vectors [1]);
	}

	public static Vector3 PositionToLatLng(Vector3 position) {
		Vector3[] vectors = GetVectorsFromReferencePoint ();
		return ConvertSpaces (position, vectors [0], vectors [1], vectors [2], vectors [3]);
	}

	/* Get reference and game space vectors.
	 * Vector 1, 2: game space.
	 * Vector 3, 4: refence space.
	 */
	private static Vector3[] GetVectorsFromReferencePoint() {
		Vector3[] result = new Vector3 [4];
		GameObject[] referencePoints = GameObject.FindGameObjectsWithTag ("Reference Point");
		result [0] = referencePoints [0].transform.position;
		result [1] = referencePoints [1].transform.position;
		result [2] = referencePoints [0].GetComponent<ReferencePoint> ().latLng;
		result [3] = referencePoints [1].GetComponent<ReferencePoint> ().latLng;
		return result;
	}

	private static Vector3 ConvertSpaces(Vector3 target, Vector3 targetSpace1, Vector3 targetSpace2,
			Vector3 referenceSpace1, Vector3 referenceSpace2) {
		Vector3 targetSpaceReciprocal = (targetSpace2 - targetSpace1);
		targetSpaceReciprocal = new Vector3 (1 / targetSpaceReciprocal.x,
			1 / targetSpaceReciprocal.y, 1 / targetSpaceReciprocal.z);
		Vector3 normalizedVector = Vector3.Scale (target - targetSpace1,
			targetSpaceReciprocal);
		Vector3 result = referenceSpace1 + Vector3.Scale(normalizedVector,
			(referenceSpace2 - referenceSpace1));
		result.y = 0;
		return result;
	}
}
