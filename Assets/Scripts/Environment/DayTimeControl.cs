/* This script manipulate the time of the day, the animation of the directional light
 * as well as the intensity of the ambient light and street light. We manually control
 * the directional light's animation time instead of letting it flow by itself.
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class DayTimeControl : MonoBehaviour {
    public float timeFlowingRate; // Measured as (In game) Hours per (Real life) second
	public GameObject timeTextBox;
	public GameObject mainCamera;
	public GameObject sunMoonContainer;
    //public Tonemapping cameraTonemapping; // To turn off at night.

	// 0 <= sunRiseTime < noonTime < sunSetTime < 24
    public float sunRiseTime;
    public float noonTime;
    public float sunSetTime;

    public float streetLightOnTime;
    public float streetLightOffTime;
    public float sunLightToAmbientCoefficient;
    public float moonLightToAmbientCoefficient;
    public float currentTime;
    public Light sunLight;
    public Light moonLight;
    public float sunToFogRatio;
    public float importantLightRadius;
	public float updateLightInteval;

    private GameObject[] streetLights; // For use in turning on/off and importance setting functions
    private Animator dayTimeAnimator;
    // To prevent the script from keeping looping while there's nothing to change
    private bool streetLightTurned = true;
	private PostProcessingProfile cameraProfile;
	private float lightUpdateElapse = 0;
    // Use this for initialization
	void Start () {
        dayTimeAnimator = GetComponent<Animator>();
        streetLights = GameObject.FindGameObjectsWithTag("Street Light");
		cameraProfile = mainCamera.GetComponent<PostProcessingBehaviour> ().profile;
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
		if (currentTime < sunRiseTime) {
			// Animate until sunrise
			alpha = (currentTime + 24 - sunSetTime) / (24 + sunRiseTime - sunSetTime);
			dayTimeAnimator.Play("Daytime", 0, linear (.5f, 1, alpha));
            //cameraTonemapping.enabled = false;
			cameraProfile.eyeAdaptation.enabled = false;
        } else if (currentTime < noonTime) { // When time is between sunrise and noon.
			alpha = (currentTime - sunRiseTime) / (noonTime - sunRiseTime);
			// When currentTime = sunRiseTime, the animation time is 0.
			// When currentTime = noonTime, the animation time is .25 (where we set the sun at 90 degrees).
			dayTimeAnimator.Play("Daytime", 0, linear (0, .25f, alpha));
        }
        else if (currentTime < sunSetTime) {
			alpha = (currentTime - noonTime) / (sunSetTime - noonTime);
			// When currentTime = noonTime, the animation time is .25.
			// When currentTime = sunSetTime, the animation time is .5 (where the sun sets).
			dayTimeAnimator.Play("Daytime", 0, linear (.25f, .5f, alpha));
        }
        else {
			// Animate until sunrise. In conjunction with the top case.
			alpha = (currentTime - sunSetTime) / (24 + sunRiseTime - sunSetTime);
			dayTimeAnimator.Play("Daytime", 0, linear (.5f, 1, alpha));
        }

        // Change the intensity of the ambient light according to the intensity of the sunlight or moonlight.
        if (currentTime >= sunRiseTime && currentTime <= sunSetTime) {
            float ambientIntensity = sunLight.intensity * sunLightToAmbientCoefficient;
            RenderSettings.ambientLight = sunLight.color * ambientIntensity;
        } else {
            float ambientIntensity = moonLight.intensity * moonLightToAmbientCoefficient;
            RenderSettings.ambientLight = moonLight.color * ambientIntensity;
        }
        // Change the fog color based on the intensity of the sunlight.
		//RenderSettings.fogColor = new Color(sunLight.intensity * sunToFogRatio, sunLight.intensity * sunToFogRatio, sunLight.intensity * sunToFogRatio);
		RenderSettings.fogColor = sunLight.color * sunLight.intensity;

		// Turn the street light on or off
        if (currentTime >= streetLightOnTime || currentTime <= streetLightOffTime) {
            turnStreetLight(true);
            //cameraTonemapping.enabled = false;
			cameraProfile.eyeAdaptation.enabled = false;
        }
        else {
            turnStreetLight(false);
            //cameraTonemapping.enabled = true;
			cameraProfile.eyeAdaptation.enabled = true;
        }
			
        // To keep sun at the same place for player.
        sunMoonContainer.transform.position = mainCamera.transform.position;
    }

    // Called to change street lights' importance to save on performance.
    void changeStreetLightImportance() {
        if (streetLightTurned) { // When street lights are on.
            foreach (GameObject light in streetLights) {
                if ((light.transform.position - mainCamera.transform.position).sqrMagnitude < importantLightRadius * importantLightRadius) {
                    ((Light)light.GetComponent(typeof(Light))).renderMode = LightRenderMode.ForcePixel;
                } else {
                    ((Light)light.GetComponent(typeof(Light))).renderMode = LightRenderMode.ForceVertex;
                }
            }
        }
    }

    void turnStreetLight(bool turn) {
        if (streetLightTurned != turn) { // Only execute when there's a change
            streetLightTurned = turn;
            foreach (GameObject light in streetLights) {
				light.GetComponent<AdjustLightImportance> ().enabled
						= light.GetComponent<Light>().enabled = turn;
            }
        }
    }

    // Linear function. The return value is a when alpha = 0, b when alpha = 1, between a and b when 0 < alpha < 1.
    float linear(float a, float b, float alpha) {
        return a * (1 - alpha) + b * alpha;
    }
}
