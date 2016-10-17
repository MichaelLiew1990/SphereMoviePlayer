//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
// 
// Author: Xu Xiang
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class MojingVRHead : MonoBehaviour
{
    public bool VRModeEnabled
    {
        get
        {
            return vrModeEnabled;
        }
        set
        {
            vrModeEnabled = value;
            foreach (MojingEye eye in Eyes)
            {
                eye.VRModeEnabled = vrModeEnabled;
            }
        }
    }
    [SerializeField]
    private bool vrModeEnabled = true;

    public Camera getMainCamera()
    {
        foreach (MojingEye eye in Eyes)
        {
            if (eye.eye == Mojing.Eye.Center)
            {
				return eye.GetComponent<Camera>();
            }
        }
        return null;
    }
    private MojingEye[] Eyes
    {
        get 
        {
            if (eyes == null)
                eyes = GetComponentsInChildren<MojingEye>(true);
            return eyes;
        }
    }
    private MojingEye[] eyes = null;
    private Transform MyTr;
    private Mojing sdk = null;

    public GlassesTypes GlassesType
    {
        get{
            return glassesType;
        }
        set{
            glassesType = value;
        }
    }
    [SerializeField]
    private GlassesTypes glassesType = GlassesTypes.MojingIII;

    void Awake()
    {
        MojingLog.LogTrace("Enter MojingVRHead.Awake");
        gameObject.SetActive(ConfigItem.MojingSDKActive);
        sdk = Mojing.SDK;
        if (MojingSDK.cur_GlassKey == "")
        {
           //sdk.GlassesKey = Mojing.glassesKeyList[1];
            sdk.GlassesKey = GetCurrentGlasses(GlassesType);
            MojingSDK.cur_GlassKey = sdk.GlassesKey;
        }
        else
            sdk.GlassesKey = MojingSDK.cur_GlassKey;
        Debug.Log(MojingSDK.GetSDKVersion());
        MyTr = this.transform;
        MojingLog.LogTrace("Leave MojingVRHead.Awake");
        if (ConfigItem.ScreenNeverSleep)
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        else
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    void OnEnable()
{
        // Disable all camera until we enter mojing world
    foreach (MojingEye eye in Eyes)
           {
                eye.EnableEye(false);
            }
}

    // Which types of tracking this instance will use.
    public bool trackRotation = true;
    public bool trackPosition = true;

    // If set, the head transform will be relative to it.
    public Transform target;

    // Determine whether head updates early or late in frame.
    // Defaults to false in order to reduce latency.
    // Set this to true if you see jitter due to other scripts using this
    // object's orientation (or a child's) in their own LateUpdate() functions,
    // e.g. to cast rays.
    public bool updateEarly = false;

    // Where is this head looking?
    public Ray Gaze
    {
        get
        {
            UpdateHead();
            return new Ray(transform.position, transform.forward);
        }
    }

    private bool updated;

    void Update()
    {
        updated = false;  // OK to recompute head pose.
        if (updateEarly)
        {
            UpdateHead();
        }
    }


    // Normally, update head pose now.
    void LateUpdate()
    {
        UpdateHead();
    }

    //   Orientation--------------------------
    private void UpdateHead()
    {
        if (updated)
        {  // Only one update per frame, please.
            return;
        }
        updated = true;
        Mojing.SDK.UpdateState();

        if (trackRotation)
        {
            var rot = Mojing.SDK.headPose.Orientation;

            if (target == null)
            {
                transform.localRotation = rot;
            }
            else
            {
                MyTr.transform.rotation = target.rotation * rot;
            }
        }

        if (trackPosition)
        {
            Vector3 pos = Mojing.SDK.headPose.Position;
            if (target == null)
            {
                transform.localPosition = pos;
            }
            else
            {
                transform.position = target.position + target.rotation * pos;
            }
        }
    }

    // Enumerate Glasses
    public enum GlassesTypes
    {
        MojingII,
        MojingIII,
        MojingIIIPlusB,
        MojingIIIPlusA,
        MojingIV,
        MojingMovie,
        MojingYoungD,
        MojingV,
        MojingRIO,
    };

    private string GetCurrentGlasses(GlassesTypes glassesType)
    {
		try{
			switch (glassesType)
			{
			case GlassesTypes.MojingII:
				sdk.GlassesKey = Mojing.glassesKeyList[0];
				break;
			case GlassesTypes.MojingIII:
				sdk.GlassesKey = Mojing.glassesKeyList[1];
				break;
			case GlassesTypes.MojingIIIPlusB:
				sdk.GlassesKey = Mojing.glassesKeyList[2];
				break;
			case GlassesTypes.MojingIIIPlusA:
				sdk.GlassesKey = Mojing.glassesKeyList[3];
				break;
			case GlassesTypes.MojingIV:
				sdk.GlassesKey = Mojing.glassesKeyList[4];
				break;
            case GlassesTypes.MojingMovie:
                sdk.GlassesKey = Mojing.glassesKeyList[5];
                break;
            case GlassesTypes.MojingYoungD:
                sdk.GlassesKey = Mojing.glassesKeyList[6];
                break;
            case GlassesTypes.MojingV:
                sdk.GlassesKey = Mojing.glassesKeyList[7];
                break;
            case GlassesTypes.MojingRIO:
                sdk.GlassesKey = Mojing.glassesKeyList[8];
                break;
                default:
				sdk.GlassesKey = Mojing.glassesKeyList[7];
				break;
			}
		}
		catch
		{
			sdk.GlassesKey=Mojing.glassesKeyList[Mojing.glassesKeyList.Count-1];
		}
        return sdk.GlassesKey;
    }

}
