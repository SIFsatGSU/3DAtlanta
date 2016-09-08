using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour {
    public float acceleration;
    public float walkingSpeed;
    public float runningSpeed;
    public float jumpStrenth;
    private new Rigidbody rigidbody;
    private bool jumpable = true;
    private float groundTime = 0;

	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.timeScale > 0) {
            // Movement input
            if (jumpable) { // Player also needs to be grounded to move
                float verticalMovement = Input.GetAxisRaw("Vertical");
                Vector3 forwardVector = transform.forward * verticalMovement;
                float horizontalMovement = Input.GetAxisRaw("Horizontal");
                Vector3 rightVector = transform.right * horizontalMovement;

<<<<<<< HEAD
            Vector3 movementVector = forwardVector + rightVector;
            movementVector.Normalize();
            if (movementVector.magnitude > 0) {
                rigidbody.velocity += movementVector * acceleration / Time.deltaTime;
                float currentVelocity = Mathf.Sqrt(
                    rigidbody.velocity.x * rigidbody.velocity.x
                    + rigidbody.velocity.z * rigidbody.velocity.z);
                float maxSpeed = (Input.GetAxisRaw("Run") > 0) ? runningSpeed : walkingSpeed;
                if (currentVelocity > maxSpeed) {
                    Vector3 currentVector = rigidbody.velocity;
                    currentVector.x = currentVector.x * maxSpeed / currentVelocity;
                    currentVector.z = currentVector.z * maxSpeed / currentVelocity;
                    rigidbody.velocity = currentVector;
=======
                Vector3 movementVector = forwardVector + rightVector;
                movementVector.Normalize();
                if (movementVector.magnitude > 0) {
                    camera.GetComponent<Animator>().enabled = true;
                    rigidbody.velocity += movementVector * acceleration / Time.deltaTime;
                    float currentVelocity = Mathf.Sqrt(
                        rigidbody.velocity.x * rigidbody.velocity.x
                        + rigidbody.velocity.z * rigidbody.velocity.z);
                    float maxSpeed = (Input.GetAxisRaw("Run") > 0) ? runningSpeed : walkingSpeed;
                    if (currentVelocity > maxSpeed) {
                        Vector3 currentVector = rigidbody.velocity;
                        currentVector.x = currentVector.x * maxSpeed / currentVelocity;
                        currentVector.z = currentVector.z * maxSpeed / currentVelocity;
                        rigidbody.velocity = currentVector;
                    }
                }
                else {
                    camera.GetComponent<Animator>().enabled = false;
                    camera.GetComponent<Animator>().SetTime(0);
>>>>>>> aeaa80bce83fb6acb04043e4e1edf2688d4a5fec
                }
            }

            if (Input.GetAxisRaw("Jump") > 0 && jumpable) {
                rigidbody.AddForce(transform.up * jumpStrenth);
                jumpable = false;
                groundTime = 0;
            }

            if (Mathf.Abs(rigidbody.velocity.y) < 0.01) {
                groundTime += Time.deltaTime;
                if (groundTime >= .1) {
                    jumpable = true;
                }
            }
        } else {
            camera.GetComponent<Animator>().enabled = false;
        }
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag.Contains("Ground")) {
            jumpable = true;
        }
    }
}
