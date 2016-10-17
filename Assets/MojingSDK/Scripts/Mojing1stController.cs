//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MojingSample.CrossPlatformInput;

public class Mojing1stController : MonoBehaviour
{
    bool canControl = true;
    bool useFixedUpdate = true;

    private Transform mainTransform;
    private Transform headTransform;

    // The current global direction we want the Mojingcharacter to move in.
    Vector3 inputMoveDirection = Vector3.zero;

    bool inputJump = false;

    public class MojingCharacterControllerMovement
    {
        // The maximum horizontal speed when moving
        public float maxForwardSpeed = 10;
        public float maxSidewaysSpeed = 10;
        public float maxBackwardsSpeed = 10;
        public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));
        public float maxGroundAcceleration = 30;
        public float maxAirAcceleration = 20;
        public float gravity = 10;
        public float maxFallSpeed = 20;

        public CollisionFlags collisionFlags;
        public Vector3 velocity;
        public Vector3 frameVelocity = Vector3.zero;
        public Vector3 hitPoint = Vector3.zero;
        public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
    }
	MojingCharacterControllerMovement movement = new MojingCharacterControllerMovement();

    enum MovementTransferOnJump
    {
        None,
        InitTransfer,
        PermaTransfer,
        PermaLocked
    }

    class MojingCharacterControllerJump
    {
        // Can the Mojingcharacter jump?
        public bool enabled = true;
        public float baseHeight = 1.0f;
        public float extraHeight = 4.1f;
        public float perpAmount = 0.0f;
        public float steepPerpAmount = 0.5f;

        public bool jumping = false;
        public bool holdingJumpButton = false;

        public float lastStartTime = 0.0f;
        public float lastButtonDownTime = -100;

        public Vector3 jumpDir = Vector3.up;
    }

    MojingCharacterControllerJump jumping = new MojingCharacterControllerJump();

    class MojingCharacterControllerMovingPlatform
    {
        public bool enabled = true;

        public MovementTransferOnJump movementTransfer = MovementTransferOnJump.PermaTransfer;

        public Transform hitPlatform;
        public Transform activePlatform;

        public Vector3 activeLocalPoint;
        public Vector3 activeGlobalPoint;

        public Quaternion activeLocalRotation;
        public Quaternion activeGlobalRotation;

        public Matrix4x4 lastMatrix;
        public Vector3 platformVelocity;
        public bool newPlatform;
    }

    MojingCharacterControllerMovingPlatform movingPlatform = new MojingCharacterControllerMovingPlatform();

    class MojingCharacterControllerSliding
    {
        public bool enabled = true;
        public float slidingSpeed = 15;
        public float sidewaysControl = 1.0f;
        public float speedControl = 0.4f;
    }

    MojingCharacterControllerSliding sliding = new MojingCharacterControllerSliding();

    bool grounded = true;
    Vector3 groundNormal = Vector3.zero;

    private Vector3 lastGroundNormal = Vector3.zero;
    private Transform tr;
    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainTransform = GameObject.Find("MojingMain").transform;
        headTransform = GameObject.Find("MojingMain/MojingVrHead").transform;
        tr = transform;
    }

    private void UpdateFunction()
    {
        Vector3 velocity = movement.velocity;
        velocity = ApplyInputVelocityChange(velocity);
        velocity = ApplyGravityAndJumping(velocity);

        // Moving platform support
        Vector3 moveDistance = Vector3.zero;
        if (MoveWithPlatform())
        {
            Vector3 newGlobalPoint = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
            moveDistance = (newGlobalPoint - movingPlatform.activeGlobalPoint);
            if (moveDistance != Vector3.zero)
                controller.Move(moveDistance);

            // Support moving platform rotation as well:
            Quaternion newGlobalRotation = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
            Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse(movingPlatform.activeGlobalRotation);

            var yRotation = rotationDiff.eulerAngles.y;
            if (yRotation != 0)
            {
                // Prevent rotation of the local up vector
                tr.Rotate(0, yRotation, 0);
            }
        }

        Vector3 lastPosition = tr.position;
        Vector3 currentMovementOffset = velocity * Time.deltaTime;
        float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
        if (grounded)
            currentMovementOffset -= pushDownOffset * Vector3.up;

        movingPlatform.hitPlatform = null;
        groundNormal = Vector3.zero;

        // Move Mojingcharacter!
        movement.collisionFlags = controller.Move(currentMovementOffset);

        movement.lastHitPoint = movement.hitPoint;
        lastGroundNormal = groundNormal;

        if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform)
        {
            if (movingPlatform.hitPlatform != null)
            {
                movingPlatform.activePlatform = movingPlatform.hitPlatform;
                movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
                movingPlatform.newPlatform = true;
            }
        }

        Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
        movement.velocity = (tr.position - lastPosition) / Time.deltaTime;
        Vector3 newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);

        if (oldHVelocity == Vector3.zero)
        {
            movement.velocity = new Vector3(0, movement.velocity.y, 0);
        }
        else
        {
            float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
            movement.velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y * Vector3.up;
        }

        if (movement.velocity.y < velocity.y - 0.001)
        {
            if (movement.velocity.y < 0)
            {

                movement.velocity.y = velocity.y;
            }
            else
            {
                jumping.holdingJumpButton = false;
            }
        }

        // We were grounded but just loosed grounding
        if (grounded && !IsGroundedTest())
        {
            grounded = false;

            if (movingPlatform.enabled &&
                (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
            )
            {
                movement.frameVelocity = movingPlatform.platformVelocity;
                movement.velocity += movingPlatform.platformVelocity;
            }

            SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
            tr.position += pushDownOffset * Vector3.up;
        }
        // We were not grounded but just landed on something
        else if (!grounded && IsGroundedTest())
        {
            grounded = true;
            jumping.jumping = false;
            StartCoroutine(SubtractNewPlatformVelocity());

            SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
        }

        // Moving platforms support
        if (MoveWithPlatform())
        {
            movingPlatform.activeGlobalPoint = tr.position + Vector3.up * (controller.center.y - controller.height * 0.5f + controller.radius);
            movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);
            movingPlatform.activeGlobalRotation = tr.rotation;
            movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation;
        }
    }

    void FixedUpdate()
    {
        if (movingPlatform.enabled)
        {
            if (movingPlatform.activePlatform != null)
            {
                if (!movingPlatform.newPlatform)
                {
                    movingPlatform.platformVelocity = (movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint) -
                                                       movingPlatform.lastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)) / Time.deltaTime;
                }
                movingPlatform.lastMatrix = movingPlatform.activePlatform.localToWorldMatrix;
                movingPlatform.newPlatform = false;
            }
            else
            {
                movingPlatform.platformVelocity = Vector3.zero;
            }
        }

        if (useFixedUpdate)
            UpdateFunction();
    }

    private Vector2 GetInput()
    {
        float x = 0.0f;
        float y = 0.0f;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
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

    void Update()
    {
        // Get the input vector from analog joystick
        this.transform.eulerAngles = new Vector3(0, mainTransform.eulerAngles.y, 0);
        mainTransform.position = this.transform.position + new Vector3(0, 4, 0);
        Vector3 directionVector = new Vector3(GetInput().x, 0, GetInput().y);

        if (directionVector != Vector3.zero)
        {
            float directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;
            directionLength = Mathf.Min(1, directionLength);
            directionLength = directionLength * directionLength;
            directionVector = directionVector * directionLength;
        }

        // Apply the direction to the MojingCharacterController
        inputMoveDirection = headTransform.rotation * directionVector;
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
        inputJump = CrossPlatformInputManager.GetButton("Jump");
#elif UNITY_ANDROID||UNITY_IOS
		inputJump = CrossPlatformInputManager.GetButton("OK");
#endif
        if (!useFixedUpdate)
            UpdateFunction();
    }

    private Vector3 ApplyInputVelocityChange(Vector3 velocity)
	{	
		if (!canControl)
			inputMoveDirection = Vector3.zero;
		
		Vector3 desiredVelocity;
		if (grounded && TooSteep()) {

			desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;

			var projectedMoveDir = Vector3.Project(inputMoveDirection, desiredVelocity);

			desiredVelocity = desiredVelocity + projectedMoveDir * sliding.speedControl + (inputMoveDirection - projectedMoveDir) * sliding.sidewaysControl;

			desiredVelocity *= sliding.slidingSpeed;
		}
		else
			desiredVelocity = GetDesiredHorizontalVelocity();
		
		if (movingPlatform.enabled && movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer) {
			desiredVelocity += movement.frameVelocity;
			desiredVelocity.y = 0;
		}
		
		if (grounded)
			desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
		else
			velocity.y = 0;
		
		float maxVelocityChange = GetMaxAcceleration(grounded) * Time.deltaTime;
	    Vector3 velocityChangeVector = (desiredVelocity - velocity);
		if (velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange) {
			velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
		}

		if (grounded || canControl)
			velocity += velocityChangeVector;
		
		if (grounded) {

			velocity.y = Mathf.Min(velocity.y, 0);
		}
		
		return velocity;
	}

    private Vector3 ApplyGravityAndJumping(Vector3 velocity)
    {
        if (!inputJump || !canControl)
        {
            jumping.holdingJumpButton = false;
            jumping.lastButtonDownTime = -100;
        }

        if (inputJump && jumping.lastButtonDownTime < 0 && canControl)
            jumping.lastButtonDownTime = Time.time;

        if (grounded)
            velocity.y = Mathf.Min(0, velocity.y) - movement.gravity * Time.deltaTime;
        else
        {
            velocity.y = movement.velocity.y - movement.gravity * Time.deltaTime;

            if (jumping.jumping && jumping.holdingJumpButton)
            {
                if (Time.time < jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight))
                {
                    velocity += jumping.jumpDir * movement.gravity * Time.deltaTime;
                }
            }
            velocity.y = Mathf.Max(velocity.y, -movement.maxFallSpeed);
        }

        if (grounded)
        {
            // Jump only if the jump button was pressed down in the last 0.2 seconds.
            if (jumping.enabled && canControl && (Time.time - jumping.lastButtonDownTime < 0.2))
            {
                grounded = false;
                jumping.jumping = true;
                jumping.lastStartTime = Time.time;
                jumping.lastButtonDownTime = -100;
                jumping.holdingJumpButton = true;

                // Calculate the jumping direction
                if (TooSteep())
                    jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
                else
                    jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);

                velocity.y = 0;
                velocity += jumping.jumpDir * CalculateJumpVerticalSpeed(jumping.baseHeight);

                if (movingPlatform.enabled &&
                    (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                    movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
                )
                {
                    movement.frameVelocity = movingPlatform.platformVelocity;
                    velocity += movingPlatform.platformVelocity;
                }

                SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                jumping.holdingJumpButton = false;
            }
        }

        return velocity;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0 && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0)
        {
            if ((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001 || lastGroundNormal == Vector3.zero)
                groundNormal = hit.normal;
            else
                groundNormal = lastGroundNormal;

            movingPlatform.hitPlatform = hit.collider.transform;
            movement.hitPoint = hit.point;
            movement.frameVelocity = Vector3.zero;
        }
    }

    private IEnumerator SubtractNewPlatformVelocity() 
    {
	if (movingPlatform.enabled && (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
		movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer))
    {
		if (movingPlatform.newPlatform) {
			Transform platform = movingPlatform.activePlatform;
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			if (grounded && platform == movingPlatform.activePlatform)
				yield return 1;
		}
		movement.velocity -= movingPlatform.platformVelocity;
	}
}

    private bool MoveWithPlatform()
    {
        return (movingPlatform.enabled
            && (grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked)
            && movingPlatform.activePlatform != null);
    }

    private Vector3 GetDesiredHorizontalVelocity()
    {
        Vector3 desiredLocalDirection = tr.InverseTransformDirection(inputMoveDirection);
        float maxSpeed = MaxSpeedInDirection(desiredLocalDirection);
        if (grounded)
        {
            var movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y) * Mathf.Rad2Deg;
            maxSpeed *= movement.slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
        }
        return tr.TransformDirection(desiredLocalDirection * maxSpeed);
    }

    private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
    {
        Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);
        return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
    }

    private bool IsGroundedTest()
    {
        return (groundNormal.y > 0.01);
    }

    float GetMaxAcceleration(bool grounded)
    {
        if (grounded)
            return movement.maxGroundAcceleration;
        else
            return movement.maxAirAcceleration;
    }

    float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        return Mathf.Sqrt(2 * targetJumpHeight * movement.gravity);
    }

    bool IsJumping()
    {
        return jumping.jumping;
    }

    bool IsSliding()
    {
        return (grounded && sliding.enabled && TooSteep());
    }

    bool IsTouchingCeiling()
    {
        return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;
    }

    bool IsGrounded()
    {
        return grounded;
    }

    bool TooSteep()
    {
        return (groundNormal.y <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad));
    }

    Vector3 GetDirection()
    {
        return inputMoveDirection;
    }

    void SetControllable(bool controllable)
    {
        canControl = controllable;
    }

    float MaxSpeedInDirection(Vector3 desiredMovementDirection)
    {
        if (desiredMovementDirection == Vector3.zero)
            return 0;
        else
        {
            float zAxisEllipseMultiplier = (desiredMovementDirection.z > 0 ? movement.maxForwardSpeed : movement.maxBackwardsSpeed) / movement.maxSidewaysSpeed;
            Vector3 temp = new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
            float length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * movement.maxSidewaysSpeed;
            return length;
        }
    }

    void SetVelocity(Vector3 velocity)
    {
        grounded = false;
        movement.velocity = velocity;
        movement.frameVelocity = Vector3.zero;
        SendMessage("OnExternalVelocity");
    }
}