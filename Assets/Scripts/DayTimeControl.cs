using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class DayTimeControl : MonoBehaviour {
    public float initialTime;
    public float timeFlowingRate; // Measured as (In game) Hours per (Real life) second
    public float sunRiseTime;
    public float sunSetTime;
    public float streetLightOnTime;
    public float streetLightOffTime;
    public float maxSunIntensity;
    public float maxAmbientIntensity;
    public Color sunRiseColor;
    public Color sunNoonColor;
    public Color sunSetColor;
    public float currentTime;
    
    private Light sunLight;
    // Use this for initialization
	void Start () {
        sunLight = GetComponent<Light>();
        currentTime = initialTime;
    }
	
	// Update is called once per frame
	void Update () {
        currentTime += timeFlowingRate * Time.deltaTime;
        currentTime %= 24;

        float alpha;
        float sunAngle = 0;
        Color sunColor = sunRiseColor;

        if (currentTime < 12) {
            alpha = (currentTime - sunRiseTime) / (12 - sunRiseTime);
            sunAngle = linear(0, 90, alpha);
            sunColor.r = exponential(sunRiseColor.r, sunNoonColor.r, Mathf.Clamp01(alpha), 0.5f);
            sunColor.g = exponential(sunRiseColor.g, sunNoonColor.g, Mathf.Clamp01(alpha), 0.5f);
            sunColor.b = exponential(sunRiseColor.b, sunNoonColor.b, Mathf.Clamp01(alpha), 0.5f);
        }
        else {
            alpha = (currentTime - 12) / (sunSetTime - 12);
            sunAngle = linear(90, 180, alpha);
            sunColor.r = exponential(sunNoonColor.r, sunSetColor.r, Mathf.Clamp01(alpha), 2);
            sunColor.g = exponential(sunNoonColor.g, sunSetColor.g, Mathf.Clamp01(alpha), 2);
            sunColor.b = exponential(sunNoonColor.b, sunSetColor.b, Mathf.Clamp01(alpha), 2);
        }

        float sunIntensity = linear(0, maxSunIntensity, Mathf.Sin(sunAngle * Mathf.PI / 180));
        float ambientIntensity = exponential(0, maxAmbientIntensity, Mathf.Clamp01(Mathf.Sin(sunAngle * Mathf.PI / 180)), 0.5f); 

        sunLight.intensity = sunIntensity;
        sunLight.transform.localEulerAngles = new Vector3(sunAngle, 90, 0);
        sunLight.color = sunColor;
        RenderSettings.ambientIntensity = ambientIntensity;
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

    // Linear function. The return value is a when alpha = 0, b when alpha = 1, between a and b when 0 < alpha < 1.
    float linear(float a, float b, float alpha) {
        return a * (1 - alpha) + b * alpha;
    }

    float exponential(float a, float b, float alpha, float exp) {
        alpha = Mathf.Pow(alpha, exp);
        return a * (1 - alpha) + b * alpha;
    }
}
