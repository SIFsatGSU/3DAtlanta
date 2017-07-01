using UnityEngine;
using UnityEngine.VR;
using UnityEngine.PostProcessing;
using System.Collections;

public class Look : MonoBehaviour {
    public float sensitivityX;
    public float sensitivityY;
    public float limitY;
	public float depthOfFieldSpeed;
    public GameObject camera;
    private new Rigidbody rigidbody;
    private float rotationY = 0;
	private float yEnable = 1;
	private PostProcessingProfile cameraProfile;
    // Use this for initialization
    void Start () {
		cameraProfile = camera.GetComponentInChildren<PostProcessingBehaviour> ().profile;
		print (cameraProfile);
    }

    // Update is called once per frame
    void Update () {
        if (Time.timeScale > 0) {
			
			// Ray cast
			Ray forwardRay;
			if (VRDevice.isPresent) {
				Vector3 forward = InputTracking.GetLocalRotation (VRNode.Head) * new Vector3 (0, 0, 1);
				forward = transform.rotation * forward;
				forwardRay = new Ray (camera.transform.position, forward);
			} else {
				forwardRay = new Ray (camera.transform.position, camera.transform.forward);
			}
		
			GetComponent<Movement> ().forwardVector = new Vector3 
					(forwardRay.direction.x, 0, forwardRay.direction.z).normalized;

			/*RaycastHit hit;

			DepthOfFieldModel.Settings settings = cameraProfile.depthOfField.settings;
			if (Physics.Raycast (forwardRay, out hit)) {
				cameraProfile.depthOfField.enabled = true;
				settings.focusDistance += depthOfFieldSpeed *
					(hit.distance - settings.focusDistance);
				cameraProfile.depthOfField.settings = settings;
			} else {
				cameraProfile.depthOfField.enabled = false;
			}*/

			if (VRDevice.isPresent) {
				yEnable = 0;
			} else {
				yEnable = 1;
			}
			float deltaX = 0, deltaY = 0;
			if (!GameManager.oculusControllerMode) {
				deltaX = Input.GetAxisRaw ("Mouse X") + Input.GetAxisRaw ("Look X");
				deltaY = Input.GetAxisRaw ("Mouse Y") + Input.GetAxisRaw ("Look Y");
			} else {
				deltaX = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
			}
			deltaY *= yEnable;
			transform.Rotate (0, sensitivityX * deltaX, 0);
			rotationY += deltaY * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, -limitY / 2, limitY / 2);
			camera.transform.localEulerAngles = new Vector3 (-rotationY, 0, 0);
        }
    }
}
