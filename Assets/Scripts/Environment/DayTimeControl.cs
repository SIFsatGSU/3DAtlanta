/* This script manipulate the time of the day, the animation of the directional light
 * as well as the intensity of the ambient light and street light. We manually control
 * the directional light's animation time instead of letting it flow by itself.
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

//[ExecuteInEditMode]
public class DayTimeControl : MonoBehaviour {
    public float timeFlowingRate; // Measured as (In game) Hours per (Real life) second
	public float timeFlowingRateDelta;
	public float maxTimeFlowingRate;

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
	public Color ambientColor;
	public Color fogColor;
    public float currentTime;
    public Light sunLight;
    public Light moonLight;
    public float sunToFogRatio;

	public Material redNeon;
	public Material whiteNeon;

	private Color redNeonColor, whiteNeonColor;
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
		ColorUtility.TryParseHtmlString ("#991600", out redNeonColor);
		ColorUtility.TryParseHtmlString ("#999999", out whiteNeonColor);
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Period)) {
			currentTime += .5f;
		}
		if (Input.GetKeyDown (KeyCode.Comma)) {
			currentTime -= .5f;
		}
		if (Input.GetKeyDown (KeyCode.RightBracket)) {
			timeFlowingRate = Mathf.Clamp(timeFlowingRate + timeFlowingRateDelta, - maxTimeFlowingRate, maxTimeFlowingRate);
		}
		if (Input.GetKeyDown (KeyCode.LeftBracket)) {
			timeFlowingRate = Mathf.Clamp(timeFlowingRate - timeFlowingRateDelta, - maxTimeFlowingRate, maxTimeFlowingRate);
		}

        currentTime += timeFlowingRate * Time.deltaTime;
		currentTime = (currentTime % 24 + 24) % 24;
		timeTextBox.GetComponent<Text> ().text = (int)(currentTime) + ":" + (int)(currentTime % 1 * 60) + " >> " + (timeFlowingRate * 60).ToString ("F2");

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

        // Apply animated ambient light.
        RenderSettings.ambientLight = ambientColor;
		// Apply animated fog color.
		RenderSettings.fogColor = fogColor;

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

    void turnStreetLight(bool turn) {
        if (streetLightTurned != turn) { // Only execute when there's a change
            streetLightTurned = turn;
			whiteNeon.SetColor ("_EmissionColor", turn ? whiteNeonColor : Color.black);
			redNeon.SetColor ("_EmissionColor", turn ? redNeonColor : Color.black);
            foreach (GameObject light in streetLights) {
				light.GetComponent<AdjustLightImportance> ().enabled = turn;
				if (!turn) {
					light.GetComponent<Light>().enabled = false;
				}
            }
        }
    }

    // Linear function. The return value is a when alpha = 0, b when alpha = 1, between a and b when 0 < alpha < 1.
    float linear(float a, float b, float alpha) {
        return a * (1 - alpha) + b * alpha;
    }
}
