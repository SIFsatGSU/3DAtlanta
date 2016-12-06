using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour {

	public static float currentZoomLevel = 0f;
	public static Vector3 currentPos = new Vector3 ();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		currentZoomLevel = Camera.main.orthographicSize;
		currentPos = Camera.main.transform.position;

		Debug.Log (currentZoomLevel);

		if (Application.loadedLevelName == "TestUI") {

			if (CameraDragMoveZoom.trigger) {
				SetCameraPosAndZoom.untrigger = false;
				SceneManager.LoadScene ("TestUI2");		
			}
		}

		else if (Application.loadedLevelName == "TestUI2") {

			if (SetCameraPosAndZoom.untrigger) {
				CameraDragMoveZoom.trigger = false;
				SceneManager.LoadScene ("TestUI");		
			}

			if (SetCameraPosAndZoom.trigger3d) {

				SceneManager.LoadScene ("TestUI3");		
			}
		}




	}
}
