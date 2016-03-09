using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class DayTimeControl : MonoBehaviour {
    public Light[] streetLight;
    public float startTime;
    public float timeFlowingRate;
    public float sunRiseTime;
    public float sunSetTime;
    public float maxIntensity;
    public float streetLightIntensity;

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

        float sunAngle = (currentTime - sunRiseTime) * 360 / 24;
        float sunIntensity = Mathf.Clamp(Mathf.Sin(sunAngle * Mathf.PI / 360), 0, 1) * maxIntensity;

        sunLight.intensity = sunIntensity;
        sunLight.transform.localEulerAngles = new Vector3(sunAngle, 90, 0);
        if (currentTime >= sunSetTime || currentTime <= sunRiseTime) {
            turnStreetLight(streetLightIntensity);
        } else {
            turnStreetLight(0);
        }
    }

    void turnStreetLight(float intensity) {
        for (int i = 0; i < streetLight.Length; i++) {
            streetLight[i].intensity = intensity;
        }
    }
}
