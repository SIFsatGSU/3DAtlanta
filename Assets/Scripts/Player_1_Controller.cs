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

public class Player_1_Controller : MonoBehaviour {


	//****************  ALL VARIABLES ****************//

	//PRIVATE VARIABLES
	float pressLeftOrRight;
	float pressUpOrDown;
	bool pressJump;
	bool pressLeftMouseButton;
	public bool isVRCamera;

	//PUBLIC VARIABLES
	public float buttonAlreadyPressedTimer = 0f;
	public float nextBookletAnimationFrameTime = 0f;
	public float movementSpeed = 10.0f;
	public float proximitySensorRange =1.5f;

	public GameObject[] interactiveObjects;
	public GameObject testPaper;
	public GameObject testPage;
	//public GameObject[] allInteractionObjects = new GameObject[10]; //TODO this array will need to be larger if many interaction objects are added in the future
	public GameObject currentBooklet;
	public GameObject currentBookletCameraTexture;
	public GameObject background;
	public GameObject mainCamera;

	public int currentBookletPageIndex = 1;

	public bool finishedNextPageTurn = true;
	public bool finishedPreviousPageTurn = true;
	public bool alreadyPushed = false;
	public bool notNearAnyInteractiveObjects = true;
	public bool characterControlsOn = true;
	public bool isABooklet = false;

	//These Vector 3 variables control the scale of Did You Know Pages and Animated Booklets
	public Vector3 interactiveObjectCameraPosition = new Vector3(0f, .72f, 2.5f);
	public Vector3 interactiveObjectCameraPositionVR = new Vector3(-0.0250f, 1.16054f, 1.9f);
	public Vector3 interactiveObjectCameraScale = new Vector3 (3f, 2f, 0.0005f);
	public Vector3 interactiveBookletObjectCameraPosition = new Vector3(-0.7f, 0.7f, 1.875143f);
	public Vector3 interactiveBookletObjectCameraPositionVR = new Vector3(-0.80012f, 0.489688f, 1.875143f);
	public Vector3 interactiveBookletObjectCameraScale = new Vector3 (0.02f, .714285f, 0.02f);

	public Vector3 tempPosition;

	RaycastHit hitInfo = new RaycastHit();
	
	CharacterController controller;

	//********************** END OF VARIABLES **********************//




	// INITIALIZATION FUNCTION
	void Start () {
		//	Everything in this function will occur before the game starts. It will initialize 
		//   	any variables not defined in the game engine, or run any tasks necessary
		controller = gameObject.GetComponent<CharacterController>();

		//this fills an array with all available interactive objects at the start of the game
		interactiveObjects = GameObject.FindGameObjectsWithTag ("interactiveObject");

		//this determined whether or not the game is being compiled for a VR headset or not. 
		//	to switch modes--simply disable / enable the different cameras in the engine--the
		//	script will figure out the rest
		if (GameObject.FindGameObjectWithTag ("MainCamera").name == "StandardCamera")
						isVRCamera = false;
				else
						isVRCamera = true;
	}



	// Update is called once per frame
	void Update () {

		//////////////////////////////////////////////////////////////////////
		/// 	CHECKING FOR INTERACTIVE OBJECTS
		//////////////////////////////////////////////////////////////////////

		// These functions will be called once per update--if the main active camera is standard,
		//		it will only search on proximity using the mouse cursor, otherwise it will default to proximity if
		//		the project is setup to do a VR Oculus Rift build

		if (isVRCamera)
			checkForProximityInteractionTrigger ();
		else
			MouseCursorCheckForInteractionTrigger ();




		//////////////////////////////////////////////////////////////////////
		/// 	ALL INPUT CONTROLS FOR BOTH MOVEMENT AND GUI
		//////////////////////////////////////////////////////////////////////


		//if we are in movement mode, and not interacting with any objects--allow the player to move around
		if(characterControlsOn){
		//------------------------------------------
		// Character Action Controls
		//-------------------------------------------




			//----------------------------------
			// Main movement controls
			//-----------------------------------

			//Get Left/Right
			pressLeftOrRight = 0.2f*Input.GetAxisRaw ("Horizontal");
			//Get Up/Down
			pressUpOrDown = 0.2f*Input.GetAxisRaw ("Vertical");
			
			//Get Jump Button
			pressJump = Input.GetButton ("Jump");
			//Get Left Mouse Input
			pressLeftMouseButton = Input.GetMouseButton (0);


			if(pressJump){
				controller.Move (Vector3.up * Time.deltaTime * 20);
			}

			if (pressLeftOrRight > 0) { //If Pressing Right
				controller.transform.Rotate(Vector3.up * 5);
			}

			if (pressLeftOrRight < 0) { //If Pressing Left
				controller.transform.Rotate(Vector3.down * 5);
			}

			if (pressUpOrDown > 0) { //if pressing up
				controller.Move (controller.transform.forward * Time.deltaTime * movementSpeed);
			}

			if (pressUpOrDown < 0) { //if pressing down
				controller.Move (controller.transform.forward *-1 * Time.deltaTime * movementSpeed);
			}

			//Apply Gravity
			controller.Move (Vector3.down * Time.deltaTime * movementSpeed);

		


		//////////////////////////////////////////////////////////////////////////////////
		/// if we are in a vr setting--this will default interaction controls to proximity
		/// or it will default to mouse interaction if in standard mode
		/// ///////////////////////////////////////////////////////////////////////////////
		
		if (isVRCamera){
			//this button activates any highlighted object
			if(Input.GetKey ("i") && !notNearAnyInteractiveObjects){
				activateInteractiveObject();
			}
			
		} else { // if it is a standard camera
			if(pressLeftMouseButton){
				if (Physics.Raycast(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,10)) - Camera.main.transform.position, out hitInfo)){
					if (hitInfo.transform.tag == "interactiveObject"){
						activateInteractiveObject();
					}
					Debug.Log (hitInfo.transform.tag + "   " + hitInfo.transform.position);
				}
			}
		}

		
		}
		//-------------------------------------
		//	end of characterControls
		//-------------------------------------



		//----------------------------------------------------------------------------------
		// Start of Interaction controls: These controls lock player movement, and activate 
		//	new buttons that interact with the particular object interacted with
		//-----------------------------------------------------------------------------------

		else {
			//This is the main "QUIT" key--it will return controls to the player to move around and close out any
			//	interactive objects currently open
			if(Input.GetKey ("p")){
				deactivateInteractiveObject();
			}


			//These controls will turn pages of a 'booklet' object if one is currently activated--otherwise they will do nothing
			//Get Left/Right
			pressLeftOrRight = Input.GetAxisRaw ("Horizontal");
			if(isABooklet){
			if (pressLeftOrRight > 0 && finishedPreviousPageTurn == true && buttonAlreadyPressedTimer < Time.time) { //If Pressing Right
				finishedPreviousPageTurn = false;
				buttonAlreadyPressedTimer = Time.time + .2f;
			}
			
			if (!finishedPreviousPageTurn){
				finishedPreviousPageTurn = TurnToPreviousPage();
			}
			
			if (pressLeftOrRight < 0 && finishedNextPageTurn == true && buttonAlreadyPressedTimer < Time.time) { //If Pressing Left
				finishedNextPageTurn = false;
				buttonAlreadyPressedTimer = Time.time + .2f;
			}

			if (!finishedNextPageTurn){
				finishedNextPageTurn = TurnToNextPage();
			}

			}

		}


	}



//###########################################################################################################################
//
//		END OF MAIN UPDATE LOOP and START OF OTHER FUNCTIONS
//
//###########################################################################################################################




	//--------------------------------------------------------------------------------------
	//	Will turn to the next page in sequence of a booklet interactive object
	//--------------------------------------------------------------------------------------
	public bool TurnToNextPage() {
		//If we're at page one(the cover), we need to move the booklet from center to right as the booklet opens into a full spread
		if(currentBookletPageIndex == 1){
		//for each animation frame
			if (currentBookletCameraTexture.transform.localPosition.x < 0){
				//move booklet to the right until it reaches the proper position
				currentBookletCameraTexture.transform.Translate(Vector3.right *2* Time.deltaTime);
				//nextBookletAnimationFrameTime = Time.time + .05f;
				Debug.Log ("moving");
				return false;
			//if finished moving then turn the first page
			} else if (currentBookletCameraTexture.transform.localPosition.x >= 0){
				PushPageHingeJoint(-500f, 50f, currentBookletCameraTexture.transform.GetChild(currentBookletPageIndex-1).gameObject);
				currentBookletPageIndex++;
				return true;

			}
		//if we're not at page 1 and not at the last page, just flip to the next page
		} else if (currentBookletPageIndex < currentBookletCameraTexture.transform.childCount) {
			PushPageHingeJoint(-500f, 50f, currentBookletCameraTexture.transform.GetChild(currentBookletPageIndex-1).gameObject);
			currentBookletPageIndex++;
			return true;
		//if we're at the last page, then do nothing
		} else {
			//do nothing
			Debug.Log ("Last Page so we can't turn any more");
			return true;
		}
		return false;
	}

	//--------------------------------------------------------------------------------------
	//  will turn to the previous page in sequence of a booklet interactive object
	//--------------------------------------------------------------------------------------
	public bool TurnToPreviousPage() {
		//If we're at page two(before the cover), we need to move the booklet from right to center as the booklet opens into a full spread
		if(currentBookletPageIndex == 2){
			//for each animation frame
			if (currentBookletCameraTexture.transform.localPosition.x > -0.725){
				//move booklet to the right until it reaches the proper position
				currentBookletCameraTexture.transform.Translate(Vector3.left *2* Time.deltaTime);
				//nextBookletAnimationFrameTime = Time.time + .05f;
				return false;
				//if finished moving then turn the first page
			} else {
				currentBookletPageIndex--;
				PushPageHingeJoint(500f, 50f, currentBookletCameraTexture.transform.GetChild(currentBookletPageIndex-1).gameObject);
				return true;
				
			}
			//if we're not at page 1 and not at the last page, just flip to the next page
		} else if (currentBookletPageIndex > 2) {
			currentBookletPageIndex--;
			PushPageHingeJoint(500f, 50f, currentBookletCameraTexture.transform.GetChild(currentBookletPageIndex-1).gameObject);
			return true;
			//if we're at the last page, then do nothing
		} else {
			//do nothing
			Debug.Log ("Cover so we can't turn any more");
			return true;
		}

	}


	//--------------------------------------------------------------------------------------
	//	this function is used by the booklet page turning functions to activate the physics
	//		that actually turns the pages physically
	//--------------------------------------------------------------------------------------
	public void PushPageHingeJoint(float mVelocity, float mForce, GameObject pageToTurn){
		JointMotor m = new JointMotor();
		m.force = mForce;
		m.targetVelocity = mVelocity;
		m.freeSpin = false;
		pageToTurn.GetComponent<HingeJoint>().motor = m; 
	
	}


	//--------------------------------------------------------------------------------------
	//  this function processes the point in 3d space that the mouse cursor is hovering over
	//		and sees if that point coincides with an object that is also an interactive object.
	//		If it is an interaction object, it will tell that object to activate or 'highlight'
	//--------------------------------------------------------------------------------------
	public void MouseCursorCheckForInteractionTrigger(){

		if (Physics.Raycast(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,10)) - Camera.main.transform.position, out hitInfo)){
			// If the object the mouse cursor is over is part of layer 8 (interaction trigger layer)
			if (hitInfo.transform.gameObject.layer == 8){
				//let's turn on the trigger object's mesh renderer to highlight it the interactive color
				hitInfo.transform.gameObject.transform.SendMessage("ActivateHighlightTrigger");
				currentBooklet = hitInfo.transform.gameObject;
				currentBookletCameraTexture = currentBooklet.transform.GetChild(0).gameObject;


				if(hitInfo.transform.GetComponent<Interface_InteractiveTrigger>().objectCode != 0) isABooklet = false;
				else isABooklet = true;
			} else {
				//hitInfo.transform.renderer.enabled = false;
			}
		}
	}


	//--------------------------------------------------------------------------------------
	// this function checks for all surrounding interaction objects within a specified radius
	//		of the player. If an interaction object is found, it will be told to highlight which
	//		will allow the player to activate it.
	//--------------------------------------------------------------------------------------
	public void checkForProximityInteractionTrigger(){
		bool notFoundAnObject = true;
		foreach (GameObject currentObject in interactiveObjects) {
			//if player is within proximity of any object in list

			if (getDistanceBetweenPlayerAndObject(currentObject)){
			//that object is set to flash
				currentObject.transform.SendMessage("ActivateHighlightTrigger");
				currentBooklet = currentObject;
				currentBookletCameraTexture = currentObject.transform.GetChild(0).gameObject;
				notFoundAnObject = false;

				//if the interaction object isn't coded as a booklet '0' then let's not activate the booklet controls
				if(currentObject.transform.GetComponent<Interface_InteractiveTrigger>().objectCode != 0) isABooklet = false;
				else isABooklet = true;

			//else if player is not in range of any object--it will turn off over time
			}

		}
		notNearAnyInteractiveObjects = notFoundAnObject;
	}


	//--------------------------------------------------------------------------------------
	// this is a helper function that simply returns true if the provided object is less than
	//		1.5 units of distance away from the player
	//--------------------------------------------------------------------------------------
	public bool getDistanceBetweenPlayerAndObject(GameObject currentObj){
		Vector3 playerXYZ = transform.position;
		Vector3 objXYZ = currentObj.transform.position;

		float distance = Mathf.Sqrt ( Mathf.Pow((objXYZ.x - playerXYZ.x),2f) + Mathf.Pow((objXYZ.y - playerXYZ.y),2) + Mathf.Pow((objXYZ.z - playerXYZ.z),2) );

		if (distance < proximitySensorRange)
						return true;
				else
						return false;
	}

	public void activateInteractiveObject() {

		characterControlsOn = false;
		currentBookletCameraTexture.SetActive(true);
		tempPosition = transform.position;
		background.SetActive(true);
		transform.Translate(30 * Vector3.down);
		
		//these move the picture of information attached to the interactive object in front of the current position
		//	of the camera and sets the necessary scale, rotation etc. The scale, position can be altered in the above variables.
		//	the rotation--which should be left at 180degrees for non booklets can be edited in the last line below if needed.
		if(isABooklet){ //if it's a booklet, use the booklet values
			currentBookletCameraTexture.transform.SetParent(transform);

			if (isVRCamera)currentBookletCameraTexture.transform.localPosition = interactiveBookletObjectCameraPositionVR;
			else currentBookletCameraTexture.transform.localPosition = interactiveBookletObjectCameraPosition;

			currentBookletCameraTexture.transform.localScale = interactiveBookletObjectCameraScale;
			currentBookletCameraTexture.transform.rotation = transform.rotation;
			//currentBookletCameraTexture.transform.RotateAround(currentBookletCameraTexture.transform.position, Vector3.up, 180f);
		} else { //otherwise use the did you know poster values
			currentBookletCameraTexture.transform.SetParent(transform);

			if (isVRCamera)currentBookletCameraTexture.transform.localPosition = interactiveObjectCameraPositionVR;
			else currentBookletCameraTexture.transform.localPosition = interactiveObjectCameraPosition;

			currentBookletCameraTexture.transform.localScale = interactiveObjectCameraScale;
			currentBookletCameraTexture.transform.rotation = transform.rotation;
			currentBookletCameraTexture.transform.RotateAround(currentBookletCameraTexture.transform.position, Vector3.up, 180f);
		}
		
	}

	public void deactivateInteractiveObject() {

		characterControlsOn = true;
		currentBookletCameraTexture.SetActive(false);
		currentBookletCameraTexture.transform.SetParent(currentBooklet.transform);
		background.SetActive(false);
		transform.position = tempPosition;
		
	}
}
