//----------------------------------------------------------------------------------------------------
//    3D Atlanta Project : Student Innovation Fellowship
//		Georgia State University
//
//		Player Controls: This script handles all player controls that affect interaction, movement, 
//			and other player-based activities
//
//
//----------------------------------------------------------------------------------------------------




using UnityEngine;
using System.Collections;

public class Vehicle_Movement : MonoBehaviour {

	public Vector3 targetDirection = Vector3.zero;
	public Vector3 destinationNode = Vector3.zero;

	public GameObject vehicleTrackNodes;
	public int currentNodeIndex;

	public float vehicleSpeed = 15f;

	// Use this for initialization
	void Start () {
		destinationNode = vehicleTrackNodes.transform.GetChild (0).transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		//Update Movement Every Update and Move the Vehicle towards the current node point
		//targetDirection = transform.TransformDirection (destinationNode - transform.position);
		//Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, Time.deltaTime * 2, 0.0F);
		//Debug.DrawRay(transform.position, newDir, Color.red);
		//transform.rotation = Quaternion.LookRotation(newDir);
		transform.LookAt (destinationNode);
		gameObject.GetComponent<CharacterController>().Move(transform.forward * vehicleSpeed * Time.deltaTime);





	}

	public void goToNextTrackNode(){
		if (currentNodeIndex < vehicleTrackNodes.transform.childCount - 1)
			currentNodeIndex++;

		else
			currentNodeIndex = 0;

		destinationNode = vehicleTrackNodes.transform.GetChild (currentNodeIndex).transform.position;
					
	}

	public void OnTriggerEnter(Collider node){
		if (node.tag == "railTrackNode")
			goToNextTrackNode ();

	}
}
