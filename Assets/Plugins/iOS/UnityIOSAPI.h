//
//  UnityIOSAPI.h
//  MojingSDK
//
//  Created by 汤申亮 on 15/8/13.
//  Copyright (c) 2015年 baofeng. All rights reserved.
//

#ifndef __MojingSDK__UnityIOSAPI__
#define __MojingSDK__UnityIOSAPI__

#import <Foundation/Foundation.h>
#import "MojingMetalView.h"
#import "MetalForwardDecls.h"
#import <simd/simd.h>

#define LOG_LEVEL_ALL       0
#define LOG_LEVEL_TRACE     LOG_LEVEL_ALL
#define LOG_LEVEL_DEBUG     1000
#define LOG_LEVEL_INFO      2000
#define LOG_LEVEL_WARN      3000
#define LOG_LEVEL_ERROR     4000
#define LOG_LEVEL_FATAL     5000
#define LOG_LEVEL_OFF       6000

typedef void (*UnityRenderingEvent)(int eventId);

@interface Unity_MojingSDK : NSObject

extern "C"
{
    void Unity_Log(int logLevel, char* info, char* filename, int line);

    bool Unity_Init(const char* szMerchantID, const char* szAppID, const char* szAppKey, const char* szChannelID);
    bool Unity_SetEngineVersion(const char* lpszEngine);
    bool Unity_OnSurfaceChanged(int newWidth, int newHeight);
    float Unity_GetGlassesFOV();
    float Unity_GetGlassesSeparation();
    int Unity_GetGlassesSeparationInPixel();
    void Unity_GetScreenSize(float* pSize);
    bool Unity_StartTracker(int nSampleFrequence);
    bool Unity_StartTrackerCalibration();
    float Unity_IsTrackerCalibrated();
    void Unity_getLastHeadView(float* pfViewMatrix);
    void Unity_getLastHeadQuarternion(float &w, float &x, float &y, float &z);
    void Unity_StopTracker(void);
    void Unity_SetTextureID(int iLeftTextureID, int iRightTextureID);
    /*
     Metal
     */
    void Unity_SetTextureID_Metal(void* leftEye, void* rightEye);
    void Unity_SetOverlay_Metal(void* iLeftOverlayTextureID,
                                void* iRightOverlayTextureID,
                                float fLeft,
                                float fTop,
                                float fWidth,
                                float fHeight);
    void Unity_SetOverlay3D_Metal(int iEyeType , void* iTextureID, float fWidth, float fHeight, float fDistanceInMetre);
    void Unity_DestroyMetalLayer();
    
    void Unity_SetOverlay(int iLeftOverlayTextureID, int iRightOverlayTextureID, float fLeft, float fTop, float fWidth, float fHeight);
    void Unity_SetOverlay3D(int iEyeType , int iTextureID, float fWidth, float fHeight, float fDistanceInMetre);
    void Unity_ResetSensorOrientation();
    void Unity_ResetTracker(void);
    int  Unity_GetTextureSize(void);
    bool Unity_SetMojingWorld(const char * szGlassesName, bool bMultithreading, bool bTimeWarp, bool bUseMetal);
    bool Unity_IsInMojingWorld(const char * szGlassesName);
    bool Unity_EnterMojingWorld(const char * szGlassesName);
    bool Unity_ChangeMojingWorld(const char * szGlassesName);
    bool Unity_IsGlassesNeedDistortion(void);
    void Unity_SetCenterLine(int iWdith , int iColR = 255 , int iColG = 255 , int iColB = 255 , int iColA = 255);
    bool Unity_LeaveMojingWorld();
    void Unity_FreeString(char * pReleaseString);
    const char* Unity_GetDefaultMojingWorld(const char* strLanguageCodeByISO639);
    const char* Unity_GetLastMojingWorld(const char* strLanguageCodeByISO639);
    const char* Unity_GetManufacturerList(const char* strLanguageCodeByISO639);
    const char* Unity_GetProductList(const char* strManufacturerKey, const char* strLanguageCodeByISO639);
    const char* Unity_GetGlassList(const char* strProductKey, const char* strLanguageCodeByISO639);
    const char* Unity_GetGlassInfo(const char* strGlassKey, const char* strLanguageCodeByISO639);
    const char* Unity_GenerationGlassKey(const char* strProductQRCode, const char* strGlassQRCode);
    bool Unity_ktxLoadTextureN(const char* const filename, unsigned int* pTexture, unsigned int* pTarget, int* pHeight, int* pWidth, int* pDepth, bool* pIsMipmapped, unsigned int* pGlerror, int* ktxError);
    
    UnityRenderingEvent Unity_GetRenderEventFunc();
    void UnityRenderEvent(int eventID);
    
    void UnitySetGraphicsDevice (void* device, int deviceType, int eventType);
    void Unity_getProjectionMatrix(int eye, bool bVrMode, float fFOV, float fNear, float fFar, float* pfProjectionMatrix, int* pfViewRect);
    const char* Unity_GetSDKVersion();
    const char* Unity_GetGlasses();
    
    const char* Unity_GetUserSettings();
    bool Unity_SetUserSettings(const char * sUserSettings);
    
    void Unity_AppSetContinueInterval(int interval);
    void Unity_AppSetReportInterval(int interval);
    void Unity_AppSetReportImmediate(bool bImmediate);
    void Unity_AppResume();
    void Unity_AppPause();
    void Unity_AppPageStart(const char* szPageName);
    void Unity_AppPageEnd(const char* szPageName);
    void Unity_AppSetEvent(const char* szEventName, const char* szEventChannelID, const char* szEventInName, float eInData, const char* szEventOutName, float eOutData);
    void Unity_ReportLog(int iLogType, const char* szTypeName, const char* szLogContent);
    /*
      mode: 0 --- default value, Send Axis's direction (LEFT/RIGHT/UP/DOWN/CENTER) 
            1 --- Send Axis's position (x,y,z)
            2 --- Send both Axis's direction and postion
     */
    void Unity_SetGamePadAxisMode(int mode);
    //void Unity_RegisterAllGamepad();
}

void Unity_RegisterAllGamepad(UIViewController* viewCon);
@end

#endif /* defined(__MojingSDK__UnityIOSAPI__) */
