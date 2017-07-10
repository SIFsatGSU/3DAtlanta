using UnityEngine;
using UnityEngine.VR;
using UnityEngine.PostProcessing;
using System.Collections;

public class Look : MonoBehaviour {
    public float sensitivityX;
    public float sensitivityY;
    public float limitY;
	public float depthOfFieldSpeed;
    public GameObject playerCamera;
	public Transform holsterContainer;

	private GameObject cameraContainer;
    private new Rigidbody rigidbody;
    private float rotationY = 0;
	private float yEnable = 1;
	private PostProcessingProfile cameraProfile;
	private Movement movementComponent;
	private float currentHolsterAngle;

    // Use this for initialization
    void Start () {
		cameraContainer = playerCamera.transform.parent.gameObject;
		cameraProfile = playerCamera.GetComponent<PostProcessingBehaviour> ().profile;
		movementComponent = GetComponent<Movement> ();
    }

    // Update is called once per frame
    void Update () {
        if (Time.timeScale > 0) {
			// Ray cast
			Ray forwardRay;
			if (VRDevice.isPresent) {
				//Vector3 forward = InputTracking.GetLocalRotation (VRNode.Head) * new Vector3 (0, 0, 1);
				//forward = transform.rotation * forward;
				forwardRay = new Ray (playerCamera.transform.position, playerCamera.transform.forward);
			} else {
				forwardRay = new Ray (cameraContainer.transform.position, cameraContainer.transform.forward);
			}
		
			movementComponent.forwardVector = new Vector3 
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
			cameraContainer.transform.localEulerAngles = new Vector3 (-rotationY, 0, 0);

			// Rotate the holsters.
			float holsterAngle = Mathf.Atan2(playerCamera.transform.right.x, playerCamera.transform.right.z) * Mathf.Rad2Deg - 90;
			holsterContainer.rotation = Quaternion.Euler (new Vector3 (0, holsterAngle, 0));
        }
    }
}
