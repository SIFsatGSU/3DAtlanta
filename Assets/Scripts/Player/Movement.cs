using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour {
    public float acceleration;
    public float walkingSpeed;
    public float runningSpeed;
    public float jumpStrenth;
    public GameObject camera;
	public float cameraReturnRate;
	public Vector3 forwardVector;
	public AudioSource walkingAudio;
	public AudioSource runningAudio;
	public AudioSource jumpAudio;
	public AudioSource landAudio;
	private new Rigidbody rigidbody;
	[HideInInspector]
    public bool grounded = true;
    private float groundTime = 0;
    private bool gamepadRunMode;
	private Animator headBobbingAnimator;
	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody>();
		headBobbingAnimator = camera.GetComponent<Animator> ();
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.timeScale > 0) {
			// Movement input
            if (grounded) { // Player also needs to be grounded to move
                if (Input.GetAxisRaw("Run Gamepad") > 0) gamepadRunMode = true;
				Vector3 rightVector = Vector3.Cross (transform.up, forwardVector);

				float verticalMovement = Input.GetAxisRaw("Vertical");
				Vector3 forwardMovement = forwardVector * verticalMovement;
                
				float horizontalMovement = Input.GetAxisRaw("Horizontal");
				Vector3 rightMovement = rightVector * horizontalMovement;

                // If there's no movement input then stop gamepad running.
                if (horizontalMovement == 0 && verticalMovement == 0) gamepadRunMode = false;

                // Unified run mode taking input from both keyboard and gamepad.
                bool runMode = Input.GetAxisRaw("Run") > 0 || gamepadRunMode;
				Vector3 movementVector = forwardMovement + rightMovement;
                movementVector.Normalize();
                if (movementVector.magnitude > 0) {
					headBobbingAnimator.enabled = true;
                    rigidbody.velocity += movementVector * acceleration / Time.deltaTime;
                    float currentVelocity = Mathf.Sqrt(
                        rigidbody.velocity.x * rigidbody.velocity.x
                        + rigidbody.velocity.z * rigidbody.velocity.z);
                    float maxSpeed = runMode ? runningSpeed : walkingSpeed;
                    if (currentVelocity > maxSpeed) {
                        Vector3 currentVector = rigidbody.velocity;
                        currentVector.x = currentVector.x * maxSpeed / currentVelocity;
                        currentVector.z = currentVector.z * maxSpeed / currentVelocity;
                        rigidbody.velocity = currentVector;
                    }
					if (runMode) {
						walkingAudio.Stop ();
						Audios.PlayAudio(runningAudio);
					} else {
						runningAudio.Stop ();
						Audios.PlayAudio(walkingAudio);
					}
                } else {
					camera.transform.localPosition *= cameraReturnRate; // Move camera to (0, 0, 0) locally.
					headBobbingAnimator.enabled = false;
					headBobbingAnimator.Play ("Camera bobbing", 0, 0);
					StopFootStepAudios ();
                }

				if (Input.GetAxisRaw("Jump") > 0) {
					transform.position += transform.up * .01f; // Move up a bit to prevent being grounded after jumping.
					rigidbody.AddForce(transform.up * jumpStrenth);
					grounded = false;
					groundTime = 0;
					StopFootStepAudios ();
					Audios.PlayAudio (jumpAudio);
				}
            }

			//print (headBobbingAnimator.GetCurrentAnimatorStateInfo (0).normalizedTime % 1);

            if (Mathf.Abs(rigidbody.velocity.y) < 0.01) {
                groundTime += Time.deltaTime;
                if (groundTime >= .1) {
					LandOnGround ();
                }
            }

			/*Vector3 latLng = Locations.PositionToLatLng (transform.position);
			print (latLng.x + ", " + latLng.z);*/
        } else {
            camera.GetComponent<Animator>().enabled = false;
        }
    }

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "Chronolens Area") {
			GetComponent<ChronolensController> ().AreaEnter (col.gameObject.GetComponent<ChronolensArea> ());
		}
	}

	void OnTriggerExit(Collider col) {
		if (col.gameObject.tag == "Chronolens Area") {
			GetComponent<ChronolensController> ().AreaExit ();
		}
	}

	public void LandOnGround() {
		if (!grounded) { // Only play landing sound when just landed.
			Audios.PlayAudio (landAudio);
		}
		grounded = true;
	}

	void StopFootStepAudios() {
		walkingAudio.Stop ();
		runningAudio.Stop ();
	}
}
