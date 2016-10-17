//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System;
public class SetOverlay : MonoBehaviour {

    Texture tex;
    int texID = 0;
    RenderTexture texRend;
    IntPtr texRendPtr = IntPtr.Zero;
    Camera LCamera;
    Camera RCamera;
    Transform CenterPointer;
    void Start()
    {
        tex = Resources.Load("star") as Texture;
        LCamera = GameObject.Find("MojingMain/MojingVrHead/VR Camera Left").GetComponent<Camera>();
        RCamera = GameObject.Find("MojingMain/MojingVrHead/VR Camera Right").GetComponent<Camera>();
        CenterPointer = GameObject.Find("MojingMain/MojingVrHead/GazePointer").transform;
        texID = (int)tex.GetNativeTexturePtr();
#if !UNITY_EDITOR && UNITY_IOS
        texRend = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Default);
        //texRend.anisoLevel = 0;
        //texRend.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 1);
        texRendPtr = texRend.colorBuffer.GetNativeRenderBufferPtr();
#endif
    }

    void Update()
    {
        DrawOverlay();
    }

    // If TW, ATW or needDistortion enable, render by MojingSDK, Call MojingSDK.Unity_SetOverlay
    void DrawOverlay()
    {
        if (tex)
        {
            if (Mojing.SDK.NeedDistortion)
            {
                //MojingSDK.Unity_SetOverlay3D(3, texID, 0.04f, 0.04f, CenterPointer.transform.position.magnitude);
                if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
                {
                    Graphics.Blit(tex, texRend);
                    MojingSDK.Unity_SetOverlay3D_Metal(1, texRendPtr, 0.04f, 0.04f, Vector3.Distance(LCamera.transform.position, CenterPointer.position));
                    MojingSDK.Unity_SetOverlay3D_Metal(2, texRendPtr, 0.04f, 0.04f, Vector3.Distance(RCamera.transform.position, CenterPointer.position));
                }
                else
                {
                    MojingSDK.Unity_SetOverlay3D(1, texID, 0.04f, 0.04f, Vector3.Distance(LCamera.transform.position, CenterPointer.position));
                    MojingSDK.Unity_SetOverlay3D(2, texID, 0.04f, 0.04f, Vector3.Distance(RCamera.transform.position, CenterPointer.position));
                    /*------
                     iEyeType:1----Left camera viewport draw
                              2----Right camera viewport draw
                              3---- Both left camera and right camera viewports draw
                     ------*/
                }
            }
        }
        else
            Debug.Log("There is no Texture!");
    }
    
    //If TW, ATW and needDistortion are disable,  not render by MojingSDK, Call GUI.DrawTexture
    void OnGUI()
    {
        if (tex)
        {
#if UNITY_EDITOR
            if (Mojing.SDK.VRModeEnabled)
            {
                GUI.DrawTexture(new Rect(LCamera.WorldToScreenPoint(CenterPointer.position).x - 25, LCamera.WorldToScreenPoint(CenterPointer.position).y - 25, 50, 50), tex);
                GUI.DrawTexture(new Rect(RCamera.WorldToScreenPoint(CenterPointer.position).x - 25, RCamera.WorldToScreenPoint(CenterPointer.position).y - 25, 50, 50), tex);
            }
            else
                GUI.DrawTexture(new Rect(0.5f * Screen.width - 25, 0.5f * Screen.height - 25, 50, 50), tex);
#else
            if (!Mojing.SDK.NeedDistortion && Mojing.SDK.VRModeEnabled)
            {
                GUI.DrawTexture(new Rect(LCamera.WorldToScreenPoint(CenterPointer.position).x - 25, LCamera.WorldToScreenPoint(CenterPointer.position).y - 25, 50, 50), tex);
                GUI.DrawTexture(new Rect(RCamera.WorldToScreenPoint(CenterPointer.position).x - 25, RCamera.WorldToScreenPoint(CenterPointer.position).y - 25, 50, 50), tex);
            }
            else if (!Mojing.SDK.VRModeEnabled)
            {
                GUI.DrawTexture(new Rect(0.5f * Screen.width - 25, 0.5f * Screen.height - 25, 50, 50), tex);
            }
#endif
        }
        else
            Debug.Log("There is no Texture!");
    }

    void OnDestroy()
    {
        MojingSDK.Unity_SetOverlay3D(3, 0, 1, 1, 1);
    }
}
