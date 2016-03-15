/* This script manipulate the time of the day, the animation of the directional light
 * as well as the intensity of the ambient light and street light. We manually control
 * the directional light's animation time instead of letting it flow by itself.
*/

using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(Animator))]
public class DayTimeControl : MonoBehaviour {
    public float initialTime;
    public float timeFlowingRate; // Measured as (In game) Hours per (Real life) second
    // 0 <= sunRiseTime < noonTime < sunSetTime < 24
    public float sunRiseTime;
    public float noonTime;
    public float sunSetTime;

    public float streetLightOnTime;
    public float streetLightOffTime;
    public float sunlightToAmbientCoefficient;
    public float currentTime;

    private Animator animator;
    private Light sunLight;

    // To prevent the script from keeping looping while there's nothing to change
    private bool streetLightTurned = true;

    // Use this for initialization
	void Start () {
        sunLight = GetComponent<Light>();
        animator = GetComponent<Animator>();
        currentTime = initialTime;
    }
	
	// Update is called once per frame
	void Update () {
        currentTime += timeFlowingRate * Time.deltaTime;
        currentTime %= 24;

        float alpha;
        if (currentTime < noonTime) { // When time is between sunrise and noon.
            alpha = (currentTime - sunRiseTime) / (noonTime - sunRiseTime);
            // When currentTime = sunRiseTime, the animation time is 0.
            // When currentTime = noonTime, the animation time is .25 (where we set the sun at 90 degrees).
            animator.SetTime(linear(0, .25f, alpha));
        } else {
            alpha = (currentTime - noonTime) / (sunSetTime - noonTime);
            // When currentTime = noonTime, the animation time is .25.
            // When currentTime = sunSetTime, the animation time is .5 (where the sun sets).
            animator.SetTime(linear(.25f, .5f, alpha));
        }

        // Change the intensity of the ambient light according to the intensity of the sunlight.
        RenderSettings.ambientIntensity = sunLight.intensity * sunlightToAmbientCoefficient;

        // Turn the street light on or off
        if (currentTime >= streetLightOnTime || currentTime <= streetLightOffTime) {
            turnStreetLight(true);
        } else {
            turnStreetLight(false);
        }
    }

    void turnStreetLight(bool turn) {
        if (streetLightTurned != turn) { // Only execute when there's a change
            streetLightTurned = turn;
            GameObject[] streetLight = GameObject.FindGameObjectsWithTag("Street Light");
            foreach (GameObject light in streetLight) {
                ((Light) light.GetComponent(typeof(Light))).enabled = turn;
            }
        }
    }

    // Linear function. The return value is a when alpha = 0, b when alpha = 1, between a and b when 0 < alpha < 1.
    float linear(float a, float b, float alpha) {
        return a * (1 - alpha) + b * alpha;
    }
}
