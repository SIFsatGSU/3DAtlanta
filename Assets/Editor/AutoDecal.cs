using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class AutoDecal : ScriptableWizard {
	public GameObject objectToDecal;
	public Transform[] bounds = new Transform[4];
	public Material[] materials;
	public GameObject[] decalPrefabs;
	public float cornerAvoid; // 0: decal can touch corners, 1 decals always in the middle.
	public int minDecals, maxDecals;
	public float minScale, maxScale;
	public float minArea, maxArea;

	private const float SURFACE_DISTANCE = .006f;
	[MenuItem("Custom/Auto Decal")]
	static void CreateWizard() {
		ScriptableWizard.DisplayWizard(
			"Auto Decal", typeof(AutoDecal), "Apply");
	}

	void OnWizardCreate() {
		HashSet<Material> materialSet = new HashSet<Material> ();
		foreach (Material mat in materials) {
			materialSet.Add (mat);
		}
		Mesh mesh = objectToDecal.GetComponent<MeshFilter> ().sharedMesh;
		Vector3[] vertices = mesh.vertices;
		Debug.Log (mesh.subMeshCount);
		if (materials.Length > 0) {
			Debug.Log ("meh");
			Material[] meshMaterials = objectToDecal.GetComponent<MeshRenderer> ().sharedMaterials;
			for (int i = 0; i < mesh.subMeshCount; i++) {
				if (materialSet.Contains (meshMaterials [i])) {
					Decal (mesh.GetTriangles (i), vertices, objectToDecal.transform);
					Debug.Log(meshMaterials [i].name);
				}
			}
		} else {
			//Debug.Log ("woah");
			Decal (mesh.triangles, vertices, objectToDecal.transform);
		}
	}

	void Decal(int[] triangles, Vector3[] vertices, Transform parent) {
		float minA = float.MaxValue, maxA = float.MinValue;
		for (int i = 0; i < triangles.Length; i += 3) {
			Vector3 v1 = parent.TransformPoint(vertices [i]);
			Vector3 v2 = parent.TransformPoint(vertices [i + 1]);
			Vector3 v3 = parent.TransformPoint(vertices [i + 2]);
			Vector3 normal = Vector3.Cross (v2 - v1, v3 - v1);
			float area = normal.magnitude;
			if (area < minA)
				minA = area;
			if (area > maxA)
				maxA = area;
			
			/*if (area < minArea)
				continue;
			
			int numOfDecal = (int) (Mathf.Lerp(minDecals, maxDecals,
					(area - minArea) / (maxArea - minArea)));

			for (int j = 1; j <= numOfDecal; j++) {
				float w1 = UnityEngine.Random.Range(cornerAvoid, 1f);
				float w2 = UnityEngine.Random.Range(cornerAvoid, 1f);
				float w3 = UnityEngine.Random.Range(cornerAvoid, 1f);
				float weightSum = w1 + w2 + w3;

				Vector3 position = (w1 * v1 + w2 * v2 + w3 * v3) / weightSum
						+ normal.normalized * SURFACE_DISTANCE;
				bool inBound = true;
				for (int k = 0; k < bounds.Length; k++) {
					if (Vector3.Dot (Vector3.up, Vector3.Cross (
						    bounds [(k + 1) % bounds.Length].position - bounds [k].position,
						    bounds [k].position - position)) > 0) {
						inBound = false;
						break;
					}
				}
				if (!inBound)
					continue;

				GameObject newDecal = GameObject.Instantiate (
					decalPrefabs [UnityEngine.Random.Range (0, decalPrefabs.Length)]);
				newDecal.transform.position = position;
				newDecal.transform.localScale *= UnityEngine.Random.Range (minScale, maxScale);
				newDecal.transform.parent = parent;
						newDecal.transform.rotation = Quaternion.FromToRotation (Vector3.up, normal)
					* Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360),0))
					* newDecal.transform.rotation;
			}*/
		}
		Debug.Log (minA + ", " + maxA);
	}
}