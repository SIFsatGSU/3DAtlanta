using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour {
    public float acceleration;
    public float maxSpeed;
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
        // Movement input
        if (jumpable) { // Player also needs to be grounded to move
            float verticalMovement = Input.GetAxisRaw("Vertical");
            Vector3 forwardVector = transform.forward * verticalMovement;
            float horizontalMovement = Input.GetAxisRaw("Horizontal");
            Vector3 rightVector = transform.right * horizontalMovement;

            Vector3 movementVector = forwardVector + rightVector;
            movementVector.Normalize();

            rigidbody.AddForce(movementVector * acceleration / Time.deltaTime);
        }

        if (Input.GetAxisRaw("Jump") > 0 && jumpable) {
            rigidbody.AddForce(transform.up * jumpStrenth);
            jumpable = false;
            groundTime = 0;
        }

        float currentVelocity = Mathf.Sqrt(
            rigidbody.velocity.x * rigidbody.velocity.x
            + rigidbody.velocity.z * rigidbody.velocity.z);
        if (currentVelocity > maxSpeed) {
            Vector3 currentVector = rigidbody.velocity;
            currentVector.x = currentVector.x * maxSpeed / currentVelocity;
            currentVector.z = currentVector.z * maxSpeed / currentVelocity;
            rigidbody.velocity = currentVector;
        }

        if (Mathf.Abs(rigidbody.velocity.y) < 0.01) {
            groundTime += Time.deltaTime;
            if (groundTime >= .1) {
                jumpable = true;
            }
        }
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag.Contains("Ground")) {
            jumpable = true;
        }
    }
}
