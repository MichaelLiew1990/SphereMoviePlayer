//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using MojingSample.CrossPlatformInput;

public class Mojing3rdController : MonoBehaviour 
{
	public float walkMaxAnimationSpeed = 0.75f;
	public float tropMaxAnimationSpeed = 1.0f;
	public float runMaxAnimationSpeed = 1.0f;
	public float jumpAnimationSpeed = 1.15f;
	public float landAnimationSpeed = 1.0f;
	public int division = 4;
	private Transform mojingMain;

	private Animator _animator;

	enum CharacterState
	{
		Idle = 0,
		Walking = 1,
		Jumping = 2,
	};

	private CharacterState _characterState;

	public float walkSpeed = 2.0f;

	public float inAirControlAcceleration = 3.0f;

	// How high do we jump when pressing jump and letting go immediately
	public float jumpHeight = 0.5f;

	// The gravity for the character
	public float gravity = 20.0f;
	// The gravity in controlled descent mode
	public float speedSmoothing = 10.0f;
	public float rotateSpeed = 500.0f;
	
	public bool canJump = true;

	private float jumpRepeatTime = 0.05f;
	private float jumpTimeout = 0.15f;
	private float groundedTimeout = 0.25f;

	// The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
	private float lockCameraTimer = 0.0f;

	// The current move direction in x-z
	private Vector3 moveDirection = new Vector3(0.0f, 0.0f, 0.0f);
	// The current vertical speed
	private float verticalSpeed = 0.0f;
	// The current x-z move speed
	private float moveSpeed = 0.0f;

	// The last collision flags returned from controller.Move
	private CollisionFlags collisionFlags; 

	// Are we jumping? (Initiated with jump button and not grounded yet)
	private bool jumping = false;
	private bool jumpingReachedApex = false;

	// Are we moving backwards (This locks the camera to not do a 180 degree spin)
	private bool movingBack = false;
	// Is the user pressing any keys?
	private bool isMoving = false;
	// Last time the jump button was clicked down
	private float lastJumpButtonTime = -10.0f;
	// Last time we performed a jump
	private float lastJumpTime = -1.0f;

	// the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
//	private float lastJumpStartHeight = 0.0f;

	private Vector3 inAirVelocity = new Vector3(0.0f, 0.0f, 0.0f);

	private float lastGroundedTime = 0.0f;

	private bool isControllable = true;

	private Transform _target;
	
	void Awake ()
	{
		mojingMain = GameObject.Find("MojingMain").transform;
		moveDirection = transform.TransformDirection(Vector3.forward);
		
		_animator = GetComponent(typeof(Animator)) as Animator;
		if (!_animator) {
			Debug.Log ("The character you would like to control doesn't have animator.");
		}
		if (!mojingMain) {
			Debug.Log ("There's no MojingMain.");
		}
	}

    private Vector2 GetInput()
    {
        float x = 0.0f;
        float y = 0.0f;
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
        x = CrossPlatformInputManager.GetAxis("Horizontal");
        y = CrossPlatformInputManager.GetAxis("Vertical");
#else
        //"TranslationType": 0---轴模式
        //x = CrossPlatformInputManager.GetAxis("Horizontal");
        //y = CrossPlatformInputManager.GetAxis("Vertical");

        //"TranslationType": 1---键模式
		if (CrossPlatformInputManager.GetButton ("UP")) {
			y = 1.0f;
		} 
        else if (CrossPlatformInputManager.GetButton ("DOWN")) {
			y = -1.0f;
		}
		else if (CrossPlatformInputManager.GetButton ("LEFT")) {
			x = -1.0f;		
		} 
        else if (CrossPlatformInputManager.GetButton ("RIGHT")) {
			x = 1.0f;
		}
#endif
        return new Vector2(x, y);
    }

	void UpdateSmoothedMovementDirection ()
	{
		bool grounded = IsGrounded();
		Vector3 forward = mojingMain.TransformDirection(Vector3.forward);
		forward.y = 0.0f;
		forward = forward.normalized;
		Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);
		float h = GetInput().x;
        float v = GetInput().y;
		if (v < -0.2f)
			movingBack = true;
		else
			movingBack = false;
		
		bool wasMoving = isMoving;
		isMoving = Mathf.Abs (h) > 0.1f || Mathf.Abs (v) > 0.1f;
			
		// Target direction relative to the camera
		Vector3 targetDirection = h * right + v * forward;

		if(Mathf.Abs(h)>0.0f)
		{
			targetDirection = h * right + division * forward;
		}

		// Grounded controls
		if (grounded)
		{
			// Lock camera for short period when transitioning moving & standing still
			lockCameraTimer += Time.deltaTime;
			if (isMoving != wasMoving)
				lockCameraTimer = 0.0f;

			//speed and direction are stored seperately

			Vector3 oVecZero = new Vector3(0.0f, 0.0f, 0.0f);
			if (!targetDirection.Equals(oVecZero))
			{
				// If we are really slow, just snap to the target direction
				if (moveSpeed < walkSpeed * 0.9f && grounded)
				{
					moveDirection = targetDirection.normalized;
				}
				// Otherwise smoothly turn towards it
				else
				{
					moveDirection = Vector3.RotateTowards(moveDirection, 
						targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
					moveDirection = moveDirection.normalized;
				}
			}
			
			// Smooth the speed based on the current target direction
			float curSmooth = speedSmoothing * Time.deltaTime;
			
			// Choose target speed
			//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
			float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);
		
			_characterState = CharacterState.Idle;
			
			// Pick speed modifier
			if (Mathf.Abs (h)>0.1f || Mathf.Abs (v)>0.1f)
			{
				targetSpeed *= walkSpeed;
				_characterState = CharacterState.Walking;
			}
			
			moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
			_animator.SetFloat ("Speed",moveSpeed);
		}
		// In air controls
		else
		{
			// Lock camera while in air
			if (jumping)
				lockCameraTimer = 0.0f;

			if (isMoving)
				inAirVelocity += targetDirection.normalized * Time.deltaTime * inAirControlAcceleration;
		}
	}

	float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2.0f * targetJumpHeight * gravity);
	}

	void ApplyJumping ()
	{
		// Prevent jumping too fast after each other
		if (lastJumpTime + jumpRepeatTime > Time.time)
			return;

		if (IsGrounded()) 
		{
			// Jump
			// - Only when pressing the button down
			// - With a timeout so you can press the button slightly before landing		
			if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) 
			{
				verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	void ApplyGravity ()
	{
		if (isControllable)	// don't move player at all if not controllable.
		{
			// Apply gravity
			//var jumpButton = Input.GetButton("Jump");
			//var jumpButton = BaoFengZeemoteManager.jumpInput;
			
			// When we reach the apex of the jump we send out a message
			if (jumping && !jumpingReachedApex && verticalSpeed <= 0.0f)
			{
				jumpingReachedApex = true;
				SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
			}
		
			if (IsGrounded ())
				verticalSpeed = 0.0f;
			else
				verticalSpeed -= gravity * Time.deltaTime;
		}
	}
	
	void DidJump ()
	{
		jumping = true;
		jumpingReachedApex = false;
		lastJumpTime = Time.time;
//		lastJumpStartHeight = transform.position.y;
		lastJumpButtonTime = -10.0f;
		
		_characterState = CharacterState.Jumping;
	}
	

	// Update is called once per frame
	void Update() 
	{
		if (!isControllable)
		{
			// kill all inputs if not controllable.
			Input.ResetInputAxes();
		}

		//if (Input.GetButtonDown ("Jump"))
        if (CrossPlatformInputManager.GetButton("Jump") || CrossPlatformInputManager.GetButton("OK"))
		{
			lastJumpButtonTime = Time.time;
		}

		UpdateSmoothedMovementDirection();
		
		// Apply gravity
		// - extra power jump modifies gravity
		// - controlledDescent mode modifies gravity
		ApplyGravity ();

		// Apply jumping logic
		ApplyJumping ();
		
		// Calculate actual motion
		Vector3 vVert = new Vector3 (0.0f, verticalSpeed, 0.0f);
		Vector3 movement = moveDirection * moveSpeed + vVert + inAirVelocity;
		//var movement = moveDirection * moveSpeed +inAirVelocity;
		movement *= Time.deltaTime;
		
		// Move the controller
		CharacterController controller = GetComponent(typeof(CharacterController)) as CharacterController;
		collisionFlags = controller.Move(movement);
		
		// ANIMATION sector

		if(_animator) 
		{
			if(_characterState == CharacterState.Jumping) 
			{
				_animator.SetBool ("Jump",true);
			} 
			else 
			{
				_animator.SetBool ("Jump",false);
			}
		}
		
		// Set rotation to the move direction
		if (IsGrounded())
		{
			
			transform.rotation = Quaternion.LookRotation(moveDirection);
				
		}	
		else
		{
			var xzMove = movement;
			xzMove.y = 0;
			if (xzMove.sqrMagnitude > 0.001)
			{
				transform.rotation = Quaternion.LookRotation(xzMove);
			}
		}	
		
		// We are in jump mode but just became grounded
		if (IsGrounded())
		{
			lastGroundedTime = Time.time;
			inAirVelocity = Vector3.zero;
			if (jumping)
			{
				jumping = false;
				SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit)
	{
	//	Debug.DrawRay(hit.point, hit.normal);
		if (hit.moveDirection.y > 0.01f) 
			return;
	}
	
	float GetSpeed () 
	{
		return moveSpeed;
	}

	public bool IsJumping () 
	{
		return jumping;
	}

	bool IsGrounded () 
	{
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}

	Vector3 GetDirection () {
		return moveDirection;
	}

	bool IsMovingBackwards () {
		return movingBack;
	}

	float GetLockCameraTimer () 
	{
		return lockCameraTimer;
	}
	/*
	bool IsMoving ()
	{
		return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5;
	}
	*/
	bool HasJumpReachedApex ()
	{
		return jumpingReachedApex;
	}

	bool IsGroundedWithTimeout ()
	{
		return lastGroundedTime + groundedTimeout > Time.time;
	}

	void Reset ()
	{
		gameObject.tag = "Player";
	}

}
