using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class DayTimeControl : MonoBehaviour {
    public float startTime;
    public float timeFlowingRate;
    public float sunRiseTime;
    public float sunSetTime;
    public float streetLightOnTime;
    public float streetLightOffTime;
    public float maxIntensity;

    private Light sunLight;
    public float currentTime;
	// Use this for initialization
	void Start () {
        sunLight = GetComponent<Light>();
        currentTime = startTime;
    }
	
	// Update is called once per frame
	void Update () {
        currentTime += timeFlowingRate * Time.deltaTime;
        currentTime %= 24;

        float alpha;
        float sunAngle = 0;

        if (currentTime < 12) {
            alpha = (currentTime - sunRiseTime) / (12 - sunRiseTime);
            sunAngle = linear(0, 90, alpha);
        } else {
            alpha = (currentTime - 12) / (sunSetTime - 12);
            sunAngle = linear(90, 180, alpha);
        }

        float sunIntensity = linear(0, maxIntensity, Mathf.Sin(sunAngle * Mathf.PI / 180)); 
        sunLight.intensity = sunIntensity;
        sunLight.transform.localEulerAngles = new Vector3(sunAngle, 90, 0);
        if (currentTime >= streetLightOnTime || currentTime <= streetLightOffTime) {
            turnStreetLight(true);
        } else {
            turnStreetLight(false);
        }
    }

    void turnStreetLight(bool turn) {
        GameObject[] streetLight = GameObject.FindGameObjectsWithTag("Street Light");
        foreach (GameObject light in streetLight) {
            ((Light) light.GetComponent(typeof(Light))).enabled = turn;
        }
    }

    // Linear function. The return value is a when alpha = 0, b when alpha = 1, between a and be when 0 < alpha < 1.
    float linear(float a, float b, float alpha) {
        return a * (1 - alpha) + b * alpha;
    }
}
