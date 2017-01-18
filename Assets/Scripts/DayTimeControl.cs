/* This script manipulate the time of the day, the animation of the directional light
 * as well as the intensity of the ambient light and street light. We manually control
 * the directional light's animation time instead of letting it flow by itself.
*/

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class DayTimeControl : MonoBehaviour {
    public float timeFlowingRate; // Measured as (In game) Hours per (Real life) second
	public GameObject timeTextBox;
	public GameObject mainCamera;
	public GameObject sunMoonContainer;

	// 0 <= sunRiseTime < noonTime < sunSetTime < 24
    public float sunRiseTime;
    public float noonTime;
    public float sunSetTime;

    public float streetLightOnTime;
    public float streetLightOffTime;
    public float sunlightToAmbientCoefficient;
    public float currentTime;
    public Light sunLight;
	public float sunToFogRatio;

    private Animator animator;
    // To prevent the script from keeping looping while there's nothing to change
    private bool streetLightTurned = true;

    // Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Period))
			currentTime += .5f;
		if (Input.GetKeyDown (KeyCode.Comma))
			currentTime -= .5f;
        currentTime += timeFlowingRate * Time.deltaTime;
		currentTime = (currentTime % 24 + 24) % 24;
		timeTextBox.GetComponent<Text> ().text = (int)(currentTime) + ":" + (int)(currentTime % 1 * 60);

        float alpha;
		if (currentTime < sunRiseTime) { // When time is between sunrise and noon.
			// Animate until sunrise
			alpha = (currentTime + 24 - sunSetTime) / (24 + sunRiseTime - sunSetTime);
			animator.SetTime (linear (.5f, 1, alpha));
		} else if (currentTime < noonTime) { // When time is between sunrise and noon.
			alpha = (currentTime - sunRiseTime) / (noonTime - sunRiseTime);
			// When currentTime = sunRiseTime, the animation time is 0.
			// When currentTime = noonTime, the animation time is .25 (where we set the sun at 90 degrees).
			animator.SetTime (linear (0, .25f, alpha));
		} else if (currentTime < sunSetTime) {
			alpha = (currentTime - noonTime) / (sunSetTime - noonTime);
			// When currentTime = noonTime, the animation time is .25.
			// When currentTime = sunSetTime, the animation time is .5 (where the sun sets).
			animator.SetTime (linear (.25f, .5f, alpha));
		} else {
			// Animate until sunrise. In conjunction with the top case.
			alpha = (currentTime - sunSetTime) / (24 + sunRiseTime - sunSetTime);
			animator.SetTime (linear (.5f, 1, alpha));
		}

        // Change the intensity of the ambient light according to the intensity of the sunlight.
		float ambientIntensity = sunLight.intensity * sunlightToAmbientCoefficient;
		RenderSettings.ambientLight = sunLight.color * ambientIntensity;
        // Change the fog color based on the intensity of the sunlight.
		//RenderSettings.fogColor = new Color(sunLight.intensity * sunToFogRatio, sunLight.intensity * sunToFogRatio, sunLight.intensity * sunToFogRatio);
		RenderSettings.fogColor = sunLight.color * sunLight.intensity;

		// Turn the street light on or off
        if (currentTime >= streetLightOnTime || currentTime <= streetLightOffTime) {
            turnStreetLight(true);
        } else {
            turnStreetLight(false);
        }

		// To keep sun at the same place for player.
		sunMoonContainer.transform.position = mainCamera.transform.position;
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
