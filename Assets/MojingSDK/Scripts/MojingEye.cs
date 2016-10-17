//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
// 
// Author: Xu Xiang
//------------------------------------------------------------------------------

using UnityEngine;
using System.Reflection;
using System;

[RequireComponent(typeof(Camera))]
public class MojingEye : MonoBehaviour
{
    // Whether this is the left eye or the right eye, or the mono eye.
    public Mojing.Eye eye;

    // The stereo controller in charge of this eye (and whose mono camera
    // we will copy settings from).
    private MojingVRHead head = null;
    public MojingVRHead Head
    {
        // This property is set up to work both in editor and in player.
        get
        {
            if (transform.parent == null)
            { 
                // Should not happen.
                return null;
            }
            if (head == null)
            {
                head = transform.parent.GetComponentInParent<MojingVRHead>();
            }
            return head;
        }
    }

    public bool VRModeEnabled
    {
        get
        {
            return vrModeEnabled;
        }
        set
        { 
            vrModeEnabled = value;
            UpdateVrMode();
        }
    }
    [SerializeField]
    private bool vrModeEnabled = true;

    public void UpdateVrMode()
    {
        try 
        {
            switch (eye)
            {
                case Mojing.Eye.Left:
                    if (!vrModeEnabled)
                        EnableEye(false);
                    else
                        EnableEye(true);
                    break;

                case Mojing.Eye.Right:
                    if (!vrModeEnabled)
                        EnableEye(false);
                    else
                        EnableEye(true);
                    break;

                case Mojing.Eye.Center:
                    if (!vrModeEnabled)
                        EnableEye(true);
                    else
                        EnableEye(false);
                    break;

                case Mojing.Eye.Mono:
                    EnableEye(true);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log( e.ToString());
        }
    }
    void SetTargetTex(Mojing.Eye eye)
    {
        int iFrameIndex = 0;
        if (MojingSDK.Unity_IsATW_ON())
        {
            // Unity_ATW_GetModelFrameIndex 接口中自带睡眠代码
            iFrameIndex = MojingSDK.Unity_ATW_GetModelFrameIndex();
            MojingLog.LogTrace("Unity get iFrameIndex = " + iFrameIndex);
        }
        switch (eye)
        {
            case Mojing.Eye.Left:
                GetComponent<Camera>().targetTexture = MojingRender.StereoScreen[iFrameIndex * 2];
                MojingLog.LogTrace("Use Texture " + (iFrameIndex * 2) + " id = " + MojingRender.StereoScreen[iFrameIndex * 2].GetNativeTexturePtr());

                break;
            case Mojing.Eye.Right:
                GetComponent<Camera>().targetTexture = MojingRender.StereoScreen[iFrameIndex * 2 + 1];
                MojingLog.LogTrace("Use Texture " + (iFrameIndex * 2 + 1) + " id = " + MojingRender.StereoScreen[iFrameIndex * 2+1].GetNativeTexturePtr());
                break;
        }
    }
    public void EnableEye(bool enable)
    {
        MojingLog.LogTrace("Enable Camera " + eye.ToString() + ": " + enable.ToString());
        enabled = enable;
        if (eye == Mojing.Eye.Left || eye == Mojing.Eye.Right)
        {
            // Setup FOV
			GetComponent<Camera>().fieldOfView = MojingSDK.Unity_GetGlassesFOV();

            //*****Solve the problem of splash screen when start up
            if (enable)
            {
                if (Mojing.SDK.NeedDistortion)
                {

#if UNITY_EDITOR_OSX
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2)
                {
                    SetTargetTex(eye);
                }
#else
            SetTargetTex(eye);
#endif
                }
                else
                {
                    GetComponent<Camera>().targetTexture = null;
                }
            }
            //*****
            if ((!Mojing.SDK.bWaitForMojingWord) && Mojing.SDK.VRModeEnabled && Mojing.SDK.NeedDistortion)
                GetComponent<Camera>().enabled = false;
            else
                GetComponent<Camera>().enabled = enable;
        }
        else
		    GetComponent<Camera>().enabled = enable;
    }

    void Start()
    {
        var ctlr = Head;
        if ((ctlr == null) && (eye != Mojing.Eye.Mono))
        {
            Debug.LogError("MojingEye must be child of a MojingVRHead.");
            enabled = false;
        }
        SetUpEye();
    }

    //Render directly to Screen Mode, PerspectiveOffCenter() and CreateMatrix() is needed.
    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {

        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
    float near;
    float far;
    float widthSize;
    float heightSize;
    float separation = 0;
    float[] offSet_L = new float[4];
    float[] offSet_R = new float[4];
    Matrix4x4 CreateMatrix()
    {
        Matrix4x4 m = new Matrix4x4();
        near = GetComponent<Camera>().nearClipPlane;
        far = GetComponent<Camera>().farClipPlane;
        widthSize = Mojing.SDK.mobile.width;
        heightSize = Mojing.SDK.mobile.height;
        separation = Mojing.SDK.lens.separation;
        switch (eye)
        {
            case Mojing.Eye.Left:
                offSet_L[0] = -(widthSize - separation) * 2.0f;
                offSet_L[1] = separation * 2.0f;
                offSet_L[2] = -heightSize * 2.0f;
                offSet_L[3] = -offSet_L[2];
                m = PerspectiveOffCenter(offSet_L[0], offSet_L[1], offSet_L[2], offSet_L[3], near, far);
                break;
            case Mojing.Eye.Right:
                offSet_R[0] = -separation * 2.0f;
                offSet_R[1] = (widthSize - separation) * 2.0f;
                offSet_R[2] = -heightSize * 2.0f;
                offSet_R[3] = -offSet_R[2];
                m = PerspectiveOffCenter(offSet_R[0], offSet_R[1], offSet_R[2], offSet_R[3], near, far);
                break;
        }
        return m;
    }
    public void OnPreCull()
    {
        if (!Mojing.SDK.NeedDistortion && Mojing.SDK.VRModeEnabled)
            GetComponent<Camera>().projectionMatrix = CreateMatrix();
        else
            GetComponent<Camera>().ResetProjectionMatrix();
        if (Mojing.SDK.bWaitForMojingWord)
        {
            EnableEye(false);
            return;
        }
		if ( GetComponent<Camera>() != null)
        {
            SetUpEye();
#if !UNITY_EDITOR && UNITY_ANDROID
            //mojing2 render directly
            if (!Mojing.SDK.NeedDistortion)
                return;

            if(MojingSDK.Unity_IsEnableATW())
            {
                int iFrameIndex = 0;
                if (MojingSDK.Unity_IsATW_ON())
                {
                    // Unity_ATW_GetModelFrameIndex 接口中自带睡眠代码
                    iFrameIndex = MojingSDK.Unity_ATW_GetModelFrameIndex();
                }
                switch (eye)
                {
                    case Mojing.Eye.Left:
                        GetComponent<Camera>().targetTexture = MojingRender.StereoScreen[iFrameIndex * 2];
                        break;
                    case Mojing.Eye.Right:
                        GetComponent<Camera>().targetTexture = MojingRender.StereoScreen[iFrameIndex * 2 + 1];
                        break;
                }
            }
#endif
        }
        else 
        {
            MojingLog.LogError(eye.ToString() + ": no camera found.");
        }
    }

    public void SetUpEye()
    {

        // Do not change any settings of Center Camera except localtion
        if (eye == Mojing.Eye.Center)
        {
            transform.localPosition = 0 * Vector3.right;
        }
        else
        {
            // Setup the rect & transform
            Rect rect = new Rect(0, 0, 1, 1);
            float ipd = Mojing.SDK.lens.separation; // *controller.stereoMultiplier;            

#if UNITY_EDITOR_OSX
			switch (eye)
			{
			case Mojing.Eye.Left:
				rect.width = 0.5f;
				transform.localPosition = (-ipd / 2) * Vector3.right;
				break;
				
			case Mojing.Eye.Right:
				rect.x = 0.5f;
				rect.width = 0.5f;
				transform.localPosition = (ipd / 2) * Vector3.right;
				break;
			}
#elif UNITY_EDITOR  || UNITY_STANDALONE_WIN
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2)
            {
                switch (eye)
                {
                    case Mojing.Eye.Left:
                        if (Mojing.SDK.NeedDistortion)
                        {
                            rect.width = 1.0f;
                        }
                        else
                        {
                            rect.width = 0.5f;
                        }
                        transform.localPosition = (-ipd / 2) * Vector3.right;
                        break;

                    case Mojing.Eye.Right:
                        if (Mojing.SDK.NeedDistortion)
                        {
                            rect.width = 1.0f;
                        }
                        else
                        {
                            rect.x = 0.5f;
                            rect.width = 0.5f;
                        }
                        transform.localPosition = (ipd / 2) * Vector3.right;
                        break;
                }
            }    
            else
            { 
                switch (eye)
                {
                    case Mojing.Eye.Left:
                        rect.width = 0.5f;
                        transform.localPosition = (-ipd / 2) * Vector3.right;
                        break;

                    case Mojing.Eye.Right:
                        rect.x = 0.5f;
                        rect.width = 0.5f;
                        transform.localPosition = (ipd / 2) * Vector3.right;
                        break;
                }
            }
#else
            switch (eye)
            {
                case Mojing.Eye.Left:
                    if (Mojing.SDK.NeedDistortion)
                    {
                        rect.width = 1.0f;
                    }
                    else
                    {
                        rect.width = 0.5f;
                    }
                    transform.localPosition = (-ipd / 2) * Vector3.right;
                    break;

                case Mojing.Eye.Right:
                    if (Mojing.SDK.NeedDistortion)
                    {
                        rect.width = 1.0f;
                    }
                    else
                    {
                        rect.x = 0.5f;
                        rect.width = 0.5f;
                    }
                    transform.localPosition = (ipd / 2) * Vector3.right;
                    break;
            }
#endif

            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            GetComponent<Camera>().rect = rect;
        }
    }
}
