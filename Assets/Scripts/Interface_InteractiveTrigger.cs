//----------------------------------------------------------------------------------------------------
//    3D Atlanta Project : Student Innovation Fellowship
//		Georgia State University
//
//		Interface Interaction Trigger: This script is attached to any interactive object. If the 
//			player controls script sends an activation 'message' to this script attached
//			attached to the object, then it will highlight the object. The process of this script is that
//			as long as the player controls are sending a message, it will indefinitely add .05 seconds to
//			a timer in this script "timeToNextCheckForCollision"  -- When no message is recieved, this script
//			will turn off the renderer for the "highlight box" that surrounds the object in .05 seconds.
//
//
//----------------------------------------------------------------------------------------------------




using UnityEngine;
using System.Collections;

public class Interface_InteractiveTrigger : MonoBehaviour {

	public bool mouseIsOverTrigger = false;
	public float timeToNextCheckForCollision;

	//this variable is defined in the game engine's sidebar. Each object type should have a unique identifier, 
	//	ie. Did you Knows are assigned to value 1, booklets '0'
	public int objectCode = 1;

	// Use this for initialization
	void Start () {
	//nothing to initialize yet--maybe in the future so it's simply left blank
	}
	
	// Update is called once per frame
	void Update () {
	
	//if the player controls mouse cursor or proximity sensor is sending a message that it's 
	//	touching the trigger, turn the renderer of the trigger on (or highlight it a specific color)
	if (timeToNextCheckForCollision > Time.time)
		GetComponent<Renderer>().enabled = true;
	//else turn the renderer off
	else 
		GetComponent<Renderer>().enabled = false;
	}


	public void ActivateHighlightTrigger(){
		timeToNextCheckForCollision = Time.time + .05f;
	}

}
