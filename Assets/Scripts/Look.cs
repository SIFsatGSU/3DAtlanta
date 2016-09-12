using UnityEngine;
using System.Collections;

public class Look : MonoBehaviour {
    public float sensitivityX;
    public float sensitivityY;
    public float limitY;
    public new GameObject camera;
    private new Rigidbody rigidbody;
    private float rotationY = 0;
	private bool VRMode;
    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
        if (Time.timeScale > 0) {
            float mouseX = Input.GetAxisRaw("Mouse X") + Input.GetAxisRaw("Look X");
            float mouseY = Input.GetAxisRaw("Mouse Y") + Input.GetAxisRaw("Look Y");

            transform.Rotate(0, sensitivityX * mouseX, 0);
            rotationY += mouseY * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, -limitY / 2, limitY / 2);
            camera.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
        }
    }
}
