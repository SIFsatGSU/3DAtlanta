using UnityEngine;
using System.Collections;

public class DimLenseFlare : MonoBehaviour {
	public float brightness;
    public float nearest;
    public float farthest;
    private GameObject player;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
    }
	
	// Update is called once per frame
	void Update () {
        float distance = (transform.position - player.transform.position).magnitude;
		if (distance < nearest) GetComponent<LensFlare>().brightness = brightness;
        else if (distance >= nearest) {
            float alpha = (distance - nearest) / (farthest - nearest);
			GetComponent<LensFlare>().brightness = (1 - alpha * alpha) * brightness;
        }
		GetComponent<LensFlare> ().enabled = GetComponent<Light> ().enabled;
    }
}
