using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class Look : MonoBehaviour {
    public float sensitivityX;
    public float sensitivityY;
    public float limitY;
    public new GameObject camera;
    private new Rigidbody rigidbody;
    private float rotationY = 0;
	private float yEnable = 1;
    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
        if (Time.timeScale > 0) {
			
			// Ray cast
			Ray forwardRay;
			if (VRDevice.isPresent) {
				Vector3 forward = InputTracking.GetLocalRotation(VRNode.Head) * new Vector3(0, 0, 1);
				forward = transform.rotation * forward;
				forwardRay = new Ray(camera.transform.position, forward);
			} else {
				forwardRay = new Ray(camera.transform.position, camera.transform.forward);
			}
			
			GetComponent<Movement>().forwardVector = new Vector3(forwardRay.direction.x, 0, forwardRay.direction.z).normalized;
			
			if (VRDevice.isPresent) {
				yEnable = 0;
			} else {
				yEnable = 1;
			}
            float deltaX = Input.GetAxisRaw("Mouse X") + Input.GetAxisRaw("Look X");
            float deltaY = Input.GetAxisRaw("Mouse Y") + Input.GetAxisRaw("Look Y");
			deltaY *= yEnable;
			transform.Rotate(0, sensitivityX * deltaX, 0);
			rotationY += deltaY * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, -limitY / 2, limitY / 2);
            camera.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
        }
    }
}
