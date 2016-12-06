using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SetCameraPosAndZoom : MonoBehaviour {


	private Vector3 Origin;
	private Vector3 Difference;
	private bool Drag = false;


	private float cameraDistanceMax = 300f;
	private float cameraDistanceMin = 4f;
	private float cameraDistance;
	private float scrollSpeed = 20.0f;

	public static bool untrigger = false;

	public static bool trigger3d = false;

	// Use this for initialization
	void Start () {
		Camera.main.transform.position = MainController.currentPos;
		Camera.main.orthographicSize = MainController.currentZoomLevel;

		cameraDistance = MainController.currentZoomLevel;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate() {

		if (Input.GetMouseButton (0)) {
			Difference=(Camera.main.ScreenToWorldPoint (Input.mousePosition))- Camera.main.transform.position;
			if (Drag==false){
				Drag=true;
				Origin=Camera.main.ScreenToWorldPoint (Input.mousePosition);
			}
		} else {
			Drag=false;
		}

		if (Drag==true){
			Camera.main.transform.position = Origin-Difference;
		}


		//zoom function here
		cameraDistance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
		cameraDistance = Mathf.Clamp(cameraDistance, cameraDistanceMin, cameraDistanceMax);

		Camera.main.orthographicSize = cameraDistance;


		if (cameraDistance < 5f) {
			trigger3d = true;
		
		}


		if (cameraDistance > 10f) {
			
			untrigger = true;

			//SceneManager.LoadScene ("TestUI");

		}
	
	}
}
