using UnityEngine;
using System.Collections;

public class CameraDragMoveZoom : MonoBehaviour {

	private Vector3 ResetCamera;
	private Vector3 Origin;
	private Vector3 Difference;
	private bool Drag = false;


	private float cameraDistanceMax = 300f;
	private float cameraDistanceMin = 9f;
	private float cameraDistance;
	private float scrollSpeed = 20.0f;

	public static bool trigger = false;


	void Start () {
		ResetCamera = Camera.main.transform.position;

		if (SetCameraPosAndZoom.untrigger) {
			cameraDistance = MainController.currentZoomLevel;
			Camera.main.transform.position = MainController.currentPos;

		} else {
			cameraDistance = 300f;
		}
	}

	void Update() {

	}

	void LateUpdate () {
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


		//special
		if (cameraDistance <10f) {
			trigger = true;


			//Application.LoadLevel("TestUI2");
		}


		//RESET CAMERA TO STARTING POSITION WITH RIGHT CLICK
		if (Input.GetMouseButton (1)) {
			Camera.main.transform.position=ResetCamera;
		}
	}
}
