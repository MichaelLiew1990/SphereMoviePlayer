//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using System;
using MojingSample.CrossPlatformInput.MojingInput;

// An implementation of the BaseInputModule that uses the player's gaze and the magnet trigger
// as a raycast generator.  To use, attach to the scene's EventSystem object.  Set the Canvas
// object's Render Mode to World Space, and set its Event Camera to a (mono) camera that is
// controlled by a MojingVrHead.  If you'd like gaze to work with 3D scene objects, add a
// PhysicsRaycaster to the gazing camera, and add a component that implements one of the Event
// interfaces (EventTrigger will work nicely).  The objects must have colliders too.
public class GazeInputModule : BaseInputModule
{
   // [Tooltip("Whether gaze input is active in VR Mode only (true), or all the time (false).")]
     //bool vrModeOnly = false;

    //[Tooltip("Optional object to place at raycast intersections as a 3D cursor. " +"Be sure it is on a layer that raycasts will ignore.")]
    //public GameObject cursor;

    // Time in seconds between the pointer down and up events sent by a magnet click.
    // Allows time for the UI elements to make their state transitions.
    [HideInInspector]
    public float clickTime = 0.1f;  // Based on default time for a button to animate to Pressed.

    // The pixel through which to cast rays, in viewport coordinates.  Generally, the center
    // pixel is best, assuming a monoscopic camera is selected as the Canvas' event camera.
    [HideInInspector]
    public Vector2 hotspot = new Vector2(0.5f, 0.5f);

    private PointerEventData pointerData = null;

    public event Action OnTrigger = null;

    private void DispatchTrigger()
    {
        if (OnTrigger != null)
        {
            OnTrigger();
        }
    }

    public override bool ShouldActivateModule()
    {
        if (!base.ShouldActivateModule())
        {
            return false;
        }
        //return Mojing.SDK.VRModeEnabled || !vrModeOnly;
        return true;
    }

    private Vector2 position;
    public override void  ActivateModule()
    {
        if (pointerData == null)
        {
            pointerData = new PointerEventData(eventSystem);
        }
        position = new Vector2(hotspot.x * Screen.width, hotspot.y * Screen.height);
    }

    public override void DeactivateModule()
    {
        base.DeactivateModule();

        if (pointerData != null)
        {
            HandleClick();
            HandlePointerExitAndEnter(pointerData, null);
            pointerData = null;
        }

        eventSystem.SetSelectedGameObject(null, GetBaseEventData());
        //if (cursor != null)
        //{
        //    cursor.SetActive(false);
       // }
    }

    public override bool IsPointerOverGameObject(int pointerId)
    {
        return pointerData != null && pointerData.pointerEnter != null;
    }

    private bool Triggered { get; set; }

    protected virtual bool IsKeyUp()
    {
		bool touch_end=false;
#if UNITY_IOS
		if (Input.touchCount == 1) {
			if (Input.GetTouch (0).phase == TouchPhase.Ended) {
				touch_end = true;	
			}
		}
		return touch_end;
#else
		if(Input.GetMouseButtonUp(0)){
			touch_end = true;
		}
		return touch_end;
#endif
    }

    protected virtual bool IsKeyDown()
    {
		bool touch_begin=false;
#if UNITY_IOS
		if (Input.touchCount == 1) {
			if (Input.GetTouch (0).phase == TouchPhase.Began) {
				touch_begin = true;	
			}
		}
		return touch_begin;
#else
		if(Input.GetMouseButtonDown(0)){
			touch_begin = true;
		}
		return touch_begin;
#endif
    }

    private GameObject lastGameObject = null;
    public override void Process()
    {
        try
        {
            pointerData.Reset();

            // Find the gameObject which is in the ray of view
            pointerData.position = position;
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            GameObject go = pointerData.pointerCurrentRaycast.gameObject;

            // just do update work if the game object changed.
            if (go != lastGameObject)
            {
                // Send enter events and update the highlight.
                HandlePointerExitAndEnter(pointerData, go);
                // Update the current selection, or clear if it is no longer the current object.
                var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
                if (selected == eventSystem.currentSelectedGameObject)
                {
                    ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
                }
                else
                {
                    eventSystem.SetSelectedGameObject(null, pointerData);
                }
            }

           // PlaceCursor();
            HandleClick();

            lastGameObject = go;
        }
        catch (Exception e)
        {
            MojingLog.LogError(e.ToString());
        }
    }
    /*
	private float distance_default=1.5f;
    private void PlaceCursor()
    {

        if (cursor == null)
        {
            return;
        }
        var go = pointerData.pointerCurrentRaycast.gameObject;

        //cursor.SetActive(go != null);
        cursor.SetActive(true);
        if (cursor.activeInHierarchy)
        {
            Camera cam = pointerData.enterEventCamera;
            if (cam == null)
            {
                cam = Mojing.SDK.getMainCamera();
            }
            if (cam != null)
            {   // Note: rays through screen start at near clipping plane.
                //float dist = cam.nearClipPlane;
				float dist=0;
                if (go != null)
				{//collider
                    dist = pointerData.pointerCurrentRaycast.distance ;
                    dist = dist / Mathf.Abs(Mathf.Cos(vrHead.transform.rotation.eulerAngles.y * Mathf.PI / 180));
					distance_default = dist;
				}
                else
				{//default
                    //dist += 5;
					dist = distance_default;
				}
                cursor.transform.position = cam.transform.position + cam.transform.forward * dist ;
				//Debug.Log (dist.ToString ());
            }
        }
    }
    */
    private void HandleClick()
    {
        if (IsKeyDown())
        {
            var go = pointerData.pointerCurrentRaycast.gameObject;

            if (go != null)
            {
                // Send pointer down event.
                pointerData.pressPosition = pointerData.position;
                pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
                pointerData.pointerPress = ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler) ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

                // Save the pending click state.
                pointerData.rawPointerPress = go;
                pointerData.eligibleForClick = true;
                pointerData.clickCount = 1;
                pointerData.clickTime = Time.unscaledTime;
            }
        }

        if (IsKeyUp())
        {
            if (!pointerData.eligibleForClick && (Time.unscaledTime - pointerData.clickTime < clickTime))
            {
                return;
            }

            // Send pointer up and click events.
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

            DispatchTrigger();

           // Clear the click state.
            pointerData.pointerPress = null;
            pointerData.rawPointerPress = null;
            pointerData.eligibleForClick = false;
            pointerData.clickCount = 0;
        }
    }
}
