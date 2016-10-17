//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class Mojing3rdCamera : MonoBehaviour 
{
	private Transform _target;
	private Transform mojingMain;
	
	public float distance = 20.0f;
	public float height = 6.0f;
	public float heightSmoothLag = 0.3f;
	public float snapSmoothLag = 0.2f;
	public float snapMaxSpeed = 720.0f;
	public float clampHeadPositionScreenSpace = 0.75f;

	private Vector3 headOffset = new Vector3(0.0f, 0.0f, 0.0f);
	private Vector3 centerOffset = new Vector3(0.0f, 0.0f, 0.0f);

	private float heightVelocity = 0.0f;
	private float angleVelocity = 0.0f;
//	private bool snap = false;
	private Mojing3rdController controller;
	private float targetHeight = 100000.0f;
	
	void Awake ()
	{	
		mojingMain = GameObject.Find("MojingMain").transform;
		if(!mojingMain && Camera.main)
			mojingMain = Camera.main.transform;
		if(!mojingMain) 
		{
			Debug.Log("Please assign a camera to the ThirdPersonCamera script.");
			enabled = false;	
		}

		_target = transform;
		if (_target)
		{
			controller = _target.GetComponent(typeof(Mojing3rdController)) as Mojing3rdController;
		}
		
		if (controller)
		{
			CharacterController characterController = _target.GetComponent(typeof(CharacterController)) as CharacterController;
			centerOffset = characterController.bounds.center - _target.position;
			headOffset = centerOffset;
			headOffset.y = characterController.bounds.max.y - _target.position.y;
		}
		else
			Debug.Log("Please assign a target to the camera that has a ThirdPersonController script attached.");

		Cut(_target, centerOffset);
	}

	void DebugDrawStuff ()
	{
		Debug.DrawLine(_target.position, _target.position + headOffset);
	}
	
	float AngleDistance (float a, float b)
	{
		a = Mathf.Repeat(a, 360.0f);
		b = Mathf.Repeat(b, 360.0f);
		
		return Mathf.Abs(b - a);
	}

	void Apply (Transform dummyTarget, Vector3 dummyCenter)
	{
		// Early out if we don't have a target
		if (!controller)
			return;
		
		Vector3 targetCenter = _target.position + centerOffset;
		Vector3 targetHead = _target.position + headOffset;

		//	DebugDrawStuff();

		// Calculate the current & target rotation angles
		float originalTargetAngle = _target.eulerAngles.y;
		float currentAngle = mojingMain.eulerAngles.y;

		// Adjust real target angle when camera is locked
		float targetAngle = originalTargetAngle; 
		
		// When pressing Fire2 (alt) the camera will snap to the target direction real quick.
		// It will stop snapping when it reaches the target

		currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, snapSmoothLag, snapMaxSpeed);
		
		// Normal camera motion

		// When jumping don't move camera upwards but only 
		if (controller.IsJumping ())
		{
			// We'd be moving the camera upwards, do that only if it's really high
			var newTargetHeight = targetCenter.y + height;
			if (newTargetHeight < targetHeight || newTargetHeight - targetHeight > 5.0f)
				targetHeight = targetCenter.y + height;
		}
		// When walking always update the target height
		else
		{
			targetHeight = targetCenter.y + height;
		}

		// Damp the height
		float currentHeight = mojingMain.position.y;
		currentHeight = Mathf.SmoothDamp (currentHeight, targetHeight, ref heightVelocity, heightSmoothLag);

		// Convert the angle into a rotation, by which we then reposition the camera
		Quaternion currentRotation = Quaternion.Euler (0.0f, currentAngle, 0.0f);
		
		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		mojingMain.position = targetCenter;
		mojingMain.position += currentRotation * Vector3.back * distance;

		// Set the height of the camera
		mojingMain.position = new Vector3(mojingMain.position.x, currentHeight,mojingMain.position.z);
		// Always look at the target	
		SetUpRotation(targetCenter, targetHead);
	}

	void LateUpdate () 
	{
		Apply (transform, Vector3.zero);
	}

	void Cut (Transform dummyTarget, Vector3 dummyCenter)
	{
		float oldHeightSmooth = heightSmoothLag;
		float oldSnapMaxSpeed = snapMaxSpeed;
		float oldSnapSmooth = snapSmoothLag;
		
		snapMaxSpeed = 10000.0f;
		snapSmoothLag = 0.001f;
		heightSmoothLag = 0.001f;
		
//		snap = true;
		Apply (transform, Vector3.zero);
		
		heightSmoothLag = oldHeightSmooth;
		snapMaxSpeed = oldSnapMaxSpeed;
		snapSmoothLag = oldSnapSmooth;
	}
	
	void SetUpRotation (Vector3 centerPos, Vector3 headPos)
	{
		Vector3 cameraPos = mojingMain.position;
		Vector3 offsetToCenter = centerPos - cameraPos;
		Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0.0f, offsetToCenter.z));
		mojingMain.rotation = yRotation;
	}

	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
}
