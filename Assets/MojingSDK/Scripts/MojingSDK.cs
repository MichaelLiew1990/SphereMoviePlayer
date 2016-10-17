//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
// 
// Author: Xu Xiang
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using LitJson;
using System.Collections.Generic;

public class GlassInfo
{
    public string URL { get; set; }
    public int ID { get; set; }
    public string Display { get; set; }
    public string KEY { get; set; }
}

public class Glasses
{
    public string ClassName { get; set; }
    public string ReleaseDate { get; set; }
    public List<GlassInfo> GlassList = new List<GlassInfo>();
    public string ERROR { get; set; }
}

public class ProductInfo
{
    public string URL { get; set; }
    public int ID { get; set; }
    public string Display { get; set; }
    public string KEY { get; set; }
    public Glasses GlassList = null;
}

public class Products
{
    public string ClassName { get; set; }
    public string ReleaseDate { get; set; }
    public List<ProductInfo> ProductList = new List<ProductInfo>();
    public string ERROR { get; set; }
}

public class ManufacturerInfo
{
    public string URL { get; set; }
    public int ID { get; set; }
    public string Display { get; set; }
    public string KEY { get; set; }
    public Products ProductList = null;
}

public class Manufacturers
{
    public string ClassName{get;set;}
    public string ReleaseDate { get; set; }
    public List<ManufacturerInfo> ManufacturerList = new List<ManufacturerInfo>();
    public string ERROR { get; set; }
}

public class MojingSDK
{
	//Save Current GlassesKey
	public static string cur_GlassKey = "";
	private static float[] viewMatrix = new float[16];
    private static float[] ATW_ViewMatrix = new float[16];
    public static void Unity_getLastHeadView(ref Matrix4x4 mat)
	{
		Unity_getLastHeadView(viewMatrix);
		
		int i = 0;
		for (int r = 0; r < 4; r++)
		{
			for (int c = 0; c < 4; c++, i++)
			{
				mat[r, c] = viewMatrix[i];
			}
		}
	}
	
	public static Manufacturers GetManufacturers(string strLanguageCodeByISO963)
	{
		try
		{
			string json = GetManufacturerList(strLanguageCodeByISO963);
			Manufacturers list = JsonMapper.ToObject<Manufacturers>(json);
			
			foreach (ManufacturerInfo manufacturer in list.ManufacturerList)
			{
				MojingLog.LogTrace("To get product list of " + manufacturer.Display);
				manufacturer.ProductList = GetProducts(manufacturer.KEY, strLanguageCodeByISO963);
			}
			return list;
		}
		catch (Exception e)
		{
			MojingLog.LogTrace(e.ToString());
		}
		return null;
	}
	
	public static Products GetProducts(string strManufacturerKey, string strLanguageCodeByISO963)
	{
		try
		{
			string json = GetProductList(strManufacturerKey, strLanguageCodeByISO963);
			Products list = JsonMapper.ToObject<Products>(json);
			
			foreach (ProductInfo product in list.ProductList)
			{
				product.GlassList = GetGlasses(product.KEY, strLanguageCodeByISO963);
				foreach (GlassInfo glass in product.GlassList.GlassList)
				{
					if (glass.Display == null || glass.Display == "")
					{
						glass.Display = product.Display;
					}
					else
					{
						glass.Display = product.Display + " " + glass.Display;
					}
				}
			}
			return list;
		}
		catch (Exception e)
		{
			MojingLog.LogTrace(e.ToString());
		}
		return null;
	}
	
	public static Glasses GetGlasses(string strProductKey, string strLanguageCodeByISO963)
	{
		try
		{
			string json = GetGlassList(strProductKey, strLanguageCodeByISO963);
			Glasses list = JsonMapper.ToObject<Glasses>(json);
			return list;
		}
		catch (Exception e)
		{
			MojingLog.LogTrace(e.ToString());
		}
		return null;
	}

#if UNITY_EDITOR_OSX

	public static bool Unity_Init(string szMerchantID, string szAppID, string szAppKey, string szChannelID)
	{
		return true;
	}
	public static bool Unity_EnterMojingWorld(string sGlassesName, bool bMultithread, bool bTimeWarp)
	{
		return true;
	}
	public static bool Unity_SetMojingWorld(string sGlassedName, bool bMultithread, bool bTimeWarp,  bool bUseMetal)
	{
		return true;
	}
	public static bool Unity_IsInMojingWorld(string sGlassedName)
	{
		return true;
	}
	
	public static bool ChangeMojingWorld(string sGlassesName)
	{
		return true;
	}
	
	public static string GetManufacturerList(string strLanguageCodeByISO963)
	{
        return "{\"ClassName\":\"ManufacturerList\",\"ReleaseDate\":\"20150618\",\"ManufacturerList\":[{\"URL\":\"www.mojing.com\",\"ID\":1,\"Display\":\"暴风魔镜\",\"KEY\":\"YZ9ZDF-4XWNFL-XG4BFT-CH9RA4-YYEXFF-DB2TAD\"}]}";
	}
	
	public static string GetProductList(string strManufacturerKey, string strLanguageCodeByISO963)
	{
        return "{\"ClassName\":\"ProductList\",\"ReleaseDate\":\"20150618\",\"ProductList\":[{\"URL\":\"http://www.mojing.cn/front/images/gallery/1.jpg\",\"ID\":1,\"Display\":\"暴风魔镜II\",\"KEY\":\"HG4SYV-CTYH8P-H3AEFR-C9H728-DQA4DD-S8SGXT\"},{\"URL\":\"http://www.mojing.cn/front/images/gallery/1.jpg\",\"ID\":2,\"Display\":\"暴风魔镜III\",\"KEY\":\"SFA8W8-Z8X3DF-AHQBWG-HF2X4S-F74FQQ-9R9YEB\"},{\"URL\":\"http://www.mojing.cn/front/images/gallery/1.jpg\",\"ID\":3,\"Display\":\"暴风魔镜IV\",\"KEY\":\"QTFB8L-DCW8FU-9R42A9-4YDSQ3-QR4X88-H74ZXQ\"},{\"URL\":\"http://www.mojing.cn/front/images/gallery/1.jpg\",\"ID\":5,\"Display\":\"观影镜\",\"KEY\":\"AD2GQH-FKH74X-SQWFC9-HT8MQG-Z5Y5ZL-FR8M98\"}]}";
	}
	
	public static string GetGlassList(string strProductKey, string strLanguageCodeByISO963)
	{
		switch (strProductKey)
		{
		case "HG4SYV-CTYH8P-H3AEFR-C9H728-DQA4DD-S8SGXT":
			return "{\"ClassName\":\"GlassList\",\"ReleaseDate\":\"20150618\",\"GlassList\":[{\"URL\":\"UNKNOWN\",\"ID\":1,\"KEY\":\"CTX3X8-WZ4S2Y-2BEEQV-YVZ7XQ-QBCW8M-EEQY4H\"}]}";
			
		case "SFA8W8-Z8X3DF-AHQBWG-HF2X4S-F74FQQ-9R9YEB":
                return "{\"ClassName\":\"GlassList\",\"ReleaseDate\":\"20150618\",\"GlassList\":[{\"URL\":\"UNKNOWN\",\"ID\":3,\"KEY\":\"EFF5YN-CRX9Y3-WRXBYT-H54HWV-F7FK8N-YTE28M\"},{\"URL\":\"UNKNOWN\",\"ID\":4,\"Display\":\"Plus B镜片\",\"KEY\":\"Z3XXYU-SG4XWH-ESQ828-F7FF88-224BZR-8RHMWH\"},{\"URL\":\"UNKNOWN\",\"ID\":11,\"Display\":\"Plus A镜片\",\"KEY\":\"AWCBXV-86A8WZ-CFS2FN-SWDXDT-9YAWDH-S29VXW\"}]}";
		case "QTFB8L-DCW8FU-9R42A9-4YDSQ3-QR4X88-H74ZXQ":
                return "{\"ClassName\":\"GlassList\",\"ReleaseDate\":\"20150618\",\"GlassList\":[{\"URL\":\"UNKNOWN\",\"ID\":12,\"Display\":\"标准镜片(96)\",\"KEY\":\"HTSTHL-DYQ799-98DBYB-XNAF4Q-4YEQHF-Q7WHCF\"}]}";
		case "AD2GQH-FKH74X-SQWFC9-HT8MQG-Z5Y5ZL-FR8M98":
                return "{\"ClassName\":\"GlassList\",\"ReleaseDate\":\"20150618\",\"GlassList\":[{\"URL\":\"UNKNOWN\",\"ID\":12,\"Display\":\"观影镜\",\"KEY\":\"E2DSY3-4Z2H2T-ACSZEG-SF49HM-2TWZ2X-YZF585\"}]}";
		default:
			return "{\"ERROR\":\"Unknown Product Key\"}";
		}
	} 
	
	public static void Unity_DistortFrame()
	{
		return;
	}
	
	public static bool Unity_IsGlassesNeedDistortion()
	{
		return true;
	}
	
	public static bool Unity_IsGlassesNeedDistortionByName([MarshalAs(UnmanagedType.LPStr)]string szGlassesName)
	{
		return true;
	}
	
	public static bool Unity_LeaveMojingWorld()
	{
		return true;
	}
	
	public static bool Unity_StartTracker(int nSampleFrequence)
	{
		return true;
	}
	
	public static bool Unity_StartTrackerCalibration()
	{
		return true;
	}
	
	public static float Unity_IsTrackerCalibrated()
	{
		return 1;
	}
	
	public static void Unity_StopTracker()
	{
	}
	public static void Unity_getLastHeadView(float[] viewMatrix)
	{
		for (int i=0; i<16; i++)
		{
			viewMatrix[i] = 0;
		}
		viewMatrix[0] = viewMatrix[5] = viewMatrix[10] = viewMatrix[15] = 1.0f;
	}
	
	public static int Unity_GetTextureSize()
	{
		return Screen.width / 2;
	}
	
	public static float Unity_GetGlassesFOV()
	{
		return 60.0f;
	}
	
	public static float Unity_GetGlassesSeparation()
	{
		return 0.063f;
	}
	
	public static int Unity_GetGlassesSeparationInPixel()
	{
		return Screen.width / 2;
	}
	
	public static void Unity_getProjectionMatrix(int eye, bool bVrMode, float fFOV, float fNear, float fFar, float[] pfProjectionMatrix, float[] pfViewRect)
	{
		
	}
	public static bool Unity_OnSurfaceChanged(int newWidth, int newHeight)
	{
		return false;
	}
	
	public static void Unity_GetScreenSize(ref float fWidth, ref float fHeight)
	{
		// 5.5 inch
		fWidth = 4.4f * 0.0254f;
		fHeight = 3.3f * 0.0254f;
	}
	
	public static void SetTextureID(int left_textureID,  int right_textureID)
	{
	}
	
	public static void Unity_ResetSensorOrientation()
	{
	}
	
	public static void Unity_ResetTracker()
	{
	}
	
	public static void SetCenterLine()
	{ 
	}
	public static void SetCenterLine(int iWidth, Color color)
	{
	}
	
	public static void Unity_SetGamePadAxisMode(int mode)
	{ 
	}
	
	private static string getVersion;
	public static string GetSDKVersion()
	{
		return getVersion;
	}
	
	public static bool Unity_SetEngineVersion(string lpszEngine)
	{
		return true;
	}
	
	public static void Unity_AppPageStart(string szPageName)
	{ 
	}
	public static void Unity_AppPageEnd(string szPageName)
	{
	}
	
	public static void Unity_AppSetEvent(string szEventName, string szEventChannelID, string szEventInName, float eInData, string szEventOutName, float eOutData)
	{ 
	}
	
	public static void Unity_AppResume()
	{ 
	}
	
	public static void Unity_AppPause()
	{ 
	}
	public static void Unity_AppReportLog(int iLogType, string szTypeName, string szLogContent)
	{
	}
	
	public static void Unity_SetOverlay(int iLeftOverlayTextureID, int iRightOverlayTextureID, float fLeft, float fTop, float fWidth, float fHeight)
	{ 
	}
	public static void Unity_SetOverlay3D(int iEyeType, int iTextureID, float fWidth, float fHeight, float fDistanceInMetre)
	{
	}
    public static void Unity_SetOverlay_Metal(IntPtr iLeftOverlayTextureID, IntPtr iRightOverlayTextureID, float fLeft, float fTop, float fWidth, float fHeight)
	{
	}
	public static void Unity_SetOverlay3D_Metal(int iEyeType , IntPtr iTextureID, float fWidth, float fHeight, float fDistanceInMetre)
	{
	}
    public static bool Unity_IsEnableATW()
    {
        return false;
    }

    public static bool Unity_IsATW_ON()
    {
        return false;
    }
    public static int Unity_ATW_GetModelFrameIndex()
    {
        return 0;
    }
    private static void Unity_ATW_SetTextureID(int iLeftTextureID, int iRightTextureID, float[] pFrameMatrix)
    {
    }

    public static bool Unity_IsInterFBRPlatform()
    {
        return false;
    }

    public static void ATW_SetTextureID(int iLeftTextureID, int iRightTextureID, Matrix4x4 mat)
    {
    }

    public struct backerInfo
    {
        public uint texid;
        public uint x;
        public uint y;
        public uint width;
        public uint height;
        public uint desX;
        public uint desY;
    };

    public static bool Unity_GetBackerTexID(ref int texid)
    {
        return false;
    }
    public static void Unity_SetBackerTexture(int texid, int x, int y, int width, int height, int desX, int desY)
    {
    }
    public static void Unity_StartBacker(int width, int height, backerInfo[] info, int infolen)
    {
    }

    public static void Unity_SetTextureID_Metal(IntPtr leftEye, IntPtr rightEye)
	{
	}
	public static void Unity_DestroyMetalLayer()
	{
	}

#else
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private const string dllName = "libmojing";
	[DllImport(dllName)]
	public static extern bool Unity_Init([MarshalAs(UnmanagedType.LPStr)]string szMerchantID, [MarshalAs(UnmanagedType.LPStr)]string szAppID, [MarshalAs(UnmanagedType.LPStr)]string szAppKey, [MarshalAs(UnmanagedType.LPStr)]string szChannelID, [MarshalAs(UnmanagedType.LPStr)]string szProfilePath);
    
    public static void Unity_getLastHeadView(float[] viewMatrix)
    {
        for (int i=0; i<16; i++)
        {
           viewMatrix[i] = 0;
        }
        viewMatrix[0] = viewMatrix[5] = viewMatrix[10] = viewMatrix[15] = 1.0f;
    }

    public static bool Unity_IsEnableATW(){
        return false;
    }
    public static  bool Unity_IsATW_ON(){
        return false;
    }
    public static int Unity_ATW_GetModelFrameIndex(){
        return 0;
    }
    public static bool Unity_IsInterFBRPlatform(){
        return false;
    }
    public static void ATW_SetTextureID(int iLeftTextureID, int iRightTextureID, Matrix4x4 mat){
    }
    public static bool Unity_ktxLoadTextureN(string filename, ref int pTexture, ref int pTarget, ref int width, ref int height, ref int depth, ref bool pIsMipmapped, ref int pGlerror, ref int ktxError)
    {
        return false;
    }
    public static bool Unity_IsLowPower()
    {
        return false;
    }
    public static void Unity_SetOverlay3D_Metal(int iEyeType, IntPtr iTextureID, float fWidth, float fHeight, float fDistanceInMetre)
    {
    }
    public static void Unity_DestroyMetalLayer()
    {
    }

#elif UNITY_ANDROID
	private const string dllName = "mojing";
	[DllImport(dllName)]
	public static extern void Unity_getLastHeadView(float[] viewMatrix);
    
    /*以下是为了ATW而设立的接口*/
    [DllImport(dllName)]
    public static extern bool Unity_IsEnableATW();// 是否允许启用ATW
    [DllImport(dllName)]
    public static extern bool Unity_IsATW_ON();// 是否启用了ATW
    [DllImport(dllName)]
    public static extern int Unity_ATW_GetModelFrameIndex();// 获取ATW模式下可以使用的帧序号。这个函数内部自带Sleep
    [DllImport(dllName)]
    public static extern bool Unity_IsInterFBRPlatform();
    
    [DllImport(dllName)]
    private static extern void Unity_ATW_SetTextureID(int iLeftTextureID, int iRightTextureID, float[] pFrameMatrix);// ATW模式下可用，设置模型纹理的同时设置所使用的旋转矩阵
    public static void ATW_SetTextureID(int iLeftTextureID, int iRightTextureID, Matrix4x4 mat)
    {
        int i = 0;
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++, i++)
            {
                ATW_ViewMatrix[i] = mat[r, c];
            }
        }

        Unity_ATW_SetTextureID(iLeftTextureID , iRightTextureID , ATW_ViewMatrix);
        Unity_IssuePluginEvent((int)UnityEventID.SetTextureID);
    }
    /*以上是为了ATW而设立的接口*/

    [DllImport(dllName)]
	public static extern bool Unity_ktxLoadTextureN([MarshalAs(UnmanagedType.LPStr)]string filename, ref int pTexture, ref int pTarget, ref int width, ref int height, ref int depth, ref bool pIsMipmapped, ref int pGlerror, ref int ktxError);

    [DllImport(dllName)]
	public static extern bool Unity_IsLowPower();

    public static void Unity_SetOverlay3D_Metal(int iEyeType, IntPtr iTextureID, float fWidth, float fHeight, float fDistanceInMetre)
    {
    }
    public static void Unity_DestroyMetalLayer()
    {
    }

#elif UNITY_IOS
	private const string dllName = "__Internal";
	[DllImport(dllName)]
	public static extern bool Unity_Init([MarshalAs(UnmanagedType.LPStr)]string szMerchantID, [MarshalAs(UnmanagedType.LPStr)]string szAppID, [MarshalAs(UnmanagedType.LPStr)]string szAppKey, [MarshalAs(UnmanagedType.LPStr)]string szChannelID);
	[DllImport(dllName)]
	public static extern void Unity_getLastHeadView(float[] viewMatrix);
    [DllImport(dllName)]
    public static extern void Unity_AppResume();
    [DllImport(dllName)]
    public static extern void Unity_AppPause();
    public static bool Unity_IsEnableATW(){
        return false;
    }
    public static  bool Unity_IsATW_ON(){
        return false;
    }
    public static int Unity_ATW_GetModelFrameIndex(){
        return 0;
    }
    public static bool Unity_IsInterFBRPlatform(){
        return false;
    }
    public static void ATW_SetTextureID(int iLeftTextureID, int iRightTextureID, Matrix4x4 mat){
    }
    [DllImport(dllName)]
	public static extern bool Unity_ktxLoadTextureN([MarshalAs(UnmanagedType.LPStr)]string filename, ref int pTexture, ref int pTarget, ref int width, ref int height, ref int depth, ref bool pIsMipmapped, ref int pGlerror, ref int ktxError);

    [DllImport(dllName)]
	private static extern void Unity_SetTextureID_Metal(IntPtr leftEye, IntPtr rightEye);

	public static void SetTextureID_Metal(IntPtr leftEye, IntPtr rightEye)
	{
		Unity_SetTextureID_Metal(leftEye, rightEye);
		Unity_IssuePluginEvent((int)UnityEventID.SetTextureID);
	}
    [DllImport(dllName)]
	public static extern void Unity_SetOverlay_Metal(IntPtr iLeftOverlayTextureID, IntPtr iRightOverlayTextureID, float fLeft, float fTop, float fWidth, float fHeight);
	[DllImport(dllName)]
	public static extern void Unity_SetOverlay3D_Metal(int iEyeType , IntPtr iTextureID, float fWidth, float fHeight, float fDistanceInMetre);
	[DllImport(dllName)]
	public static extern void Unity_DestroyMetalLayer();

#endif

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetSDKVersion();
    public static string GetSDKVersion()
    {
        IntPtr ptr = Unity_GetSDKVersion();
        string result = Marshal.PtrToStringAnsi(ptr);
        Unity_FreeString(ptr);
        return result;
    }
	
	enum UnityEventID
	{
		EnterMojingWorld,
		ChangeMojingWorld,
		LeaveMojingWorld,
		DistortFrame,
		SetTextureID,
        SetCenterLine,
        BackerTexture
	};



 
	[DllImport(dllName)]
	private static extern bool Unity_SetMojingWorld([MarshalAs(UnmanagedType.LPStr)]string sGlassesName, bool bMultithread, bool bTimeWarp, bool bUseMetal);

    [DllImport(dllName)]
	public static extern bool Unity_IsInMojingWorld([MarshalAs(UnmanagedType.LPStr)]string sGlassesName);

    [DllImport(dllName)]
	public static extern bool Unity_ChangeMojingWorld([MarshalAs(UnmanagedType.LPStr)]string sGlassesName);

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetRenderEventFunc();
    private static IntPtr _MojingRenderEvent = IntPtr.Zero;
    private static void Unity_IssuePluginEvent(int iEventID)
    {
        _MojingRenderEvent = Unity_GetRenderEventFunc();
        if (_MojingRenderEvent == IntPtr.Zero)
            GL.IssuePluginEvent(iEventID);
        else
            GL.IssuePluginEvent(_MojingRenderEvent, iEventID);
    }
    public static bool Unity_EnterMojingWorld(string sGlassesName, bool bMultithread, bool bTimeWarp)
    {
        MojingLog.LogTrace("---> Unity_EnterMojingWorld");

        bool bUseMetal=false;
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
        {
            bUseMetal = true;
        }
        if (Unity_SetMojingWorld(sGlassesName, bMultithread, bTimeWarp, bUseMetal))
        {
            MojingLog.LogTrace("---> Unity_SetMojingWorld ok");
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2)
                Unity_IssuePluginEvent((int)UnityEventID.EnterMojingWorld);
#else
            Unity_IssuePluginEvent((int)UnityEventID.EnterMojingWorld);
#endif
            return true;
        }
        return false;
    }

	public static bool ChangeMojingWorld(string sGlassesName)
    {
        if (Unity_ChangeMojingWorld(sGlassesName))
        {
            Unity_IssuePluginEvent((int)UnityEventID.ChangeMojingWorld);
            return true;
        }
        return false;
    }

    public static void Unity_DistortFrame()
    {
        Unity_IssuePluginEvent((int)UnityEventID.DistortFrame);
        GL.InvalidateState();
    }

	public static bool Unity_LeaveMojingWorld()
    {
        Unity_IssuePluginEvent((int)UnityEventID.LeaveMojingWorld);
        return true;
    }

    [DllImport(dllName)]
    private static extern void Unity_FreeString(IntPtr pReleaseString);

	private static string PtrToString(IntPtr ptr)
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return Marshal.PtrToStringAnsi(ptr);
#else
		return Marshal.PtrToStringAuto(ptr);
#endif
	}

    [DllImport(dllName)]
	private static extern IntPtr Unity_GetManufacturerList([MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
    public static string GetManufacturerList(string strLanguageCodeByISO963)
    {
        IntPtr ptr = Unity_GetManufacturerList(strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GetManufacturerList: " + result);
        return result;
    }

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetProductList([MarshalAs(UnmanagedType.LPStr)]string strManufacturerKey, [MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
    public static string GetProductList(string strManufacturerKey, string strLanguageCodeByISO963)
    {
        IntPtr ptr = Unity_GetProductList(strManufacturerKey, strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GetManufacturerList: " + result);
        return result;
    }
    

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetGlassList([MarshalAs(UnmanagedType.LPStr)]string strProductKey, [MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
    public static string GetGlassList(string strProductKey, string strLanguageCodeByISO963)
    {
        IntPtr ptr = Unity_GetGlassList(strProductKey, strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GetGlassList: " + result);
        return result;
    }

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetGlassInfo([MarshalAs(UnmanagedType.LPStr)]string strGlassKey, [MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
    public static string GetGlassInfo(string strGlassKey, string strLanguageCodeByISO963)
    {
        IntPtr ptr = Unity_GetGlassInfo(strGlassKey, strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GetGlassInfo: " + result);
        return result;
    }

    [DllImport(dllName)]
    private static extern IntPtr Unity_GenerationGlassKey([MarshalAs(UnmanagedType.LPStr)]string strProductQRCode, [MarshalAs(UnmanagedType.LPStr)]string strGlassQRCode);
    public static string GetGlassKey(string strProductQRCode, string strGlassQRCode)
    {
        IntPtr ptr = Unity_GenerationGlassKey(strProductQRCode, strGlassQRCode);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GenerationGlassKey: " + result);
        return result;
    }

    [DllImport(dllName)]
	public static extern bool Unity_IsGlassesNeedDistortion();

    [DllImport(dllName)]
    public static extern bool Unity_IsGlassesNeedDistortionByName([MarshalAs(UnmanagedType.LPStr)]string szGlassesName);

    [DllImport(dllName)]
	public static extern bool Unity_StartTracker(int nSampleFrequence);
	
    [DllImport(dllName)]
	public static extern int Unity_StartTrackerCalibration();

    [DllImport(dllName)]
    public static extern float Unity_IsTrackerCalibrated();

	[DllImport(dllName)]
	public static extern void Unity_StopTracker();
	
	[DllImport(dllName)]
	public static extern int Unity_GetTextureSize();

    [DllImport(dllName)]
	public static extern float Unity_GetGlassesFOV();

    [DllImport(dllName)]
    public static extern float Unity_GetGlassesSeparation();

    [DllImport(dllName)]
    public static extern int Unity_GetGlassesSeparationInPixel();

    [DllImport(dllName)]
    public static extern void Unity_getProjectionMatrix(int eye, bool bVrMode, float fFOV, float fNear, float fFar, float[] pfProjectionMatrix, float[] pfViewRect);

	[DllImport(dllName)]
	public static extern bool Unity_OnSurfaceChanged(int newWidth, int newHeight);

    [DllImport(dllName)]
    private static extern void Unity_GetScreenSize(float[] pSize);
    public static void Unity_GetScreenSize(ref float fWidth, ref float fHeight)
    {
        float[] fSize = { 0.0f, 0.0f };
        Unity_GetScreenSize(fSize);
        fWidth = fSize[0];
        fHeight = fSize[1];
    }

	[DllImport(dllName)]
	private static extern void Unity_SetTextureID(int iLeftTextureID, int iRightTextureID);

    public static void SetTextureID(int left_textureID, int right_textureID)
    {
        try
        {
            Unity_SetTextureID(left_textureID,right_textureID);
            Unity_IssuePluginEvent((int)UnityEventID.SetTextureID);
        }
        catch (Exception e)
        {
            MojingLog.LogError(e.ToString());
        }
    }
    
    [DllImport(dllName)]
	public static extern void Unity_ResetSensorOrientation();

	[DllImport(dllName)]
	public static extern void Unity_ResetTracker();

	[DllImport(dllName)]
	private static extern void Unity_SetCenterLine(int iWdith, int iColR , int iColG , int iColB , int iColA ); 

    static Color _CenterLineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    static int _CenterLineWidth = 4;
    public static void SetCenterLine()
    {
       Unity_SetCenterLine(_CenterLineWidth, (int)(_CenterLineColor.r * 255),  (int)(_CenterLineColor.g * 255), (int)(_CenterLineColor.b * 255), (int)(_CenterLineColor.a * 255));
        Unity_IssuePluginEvent((int)UnityEventID.SetCenterLine);
    }

    public static void SetCenterLine(int iWidth, Color color)
    {
        _CenterLineColor = color;
        _CenterLineWidth = iWidth;
        SetCenterLine();
    }
    
    [DllImport(dllName)]
	public static extern void Unity_SetGamePadAxisMode(int mode);

    [DllImport(dllName)]
    public static extern bool Unity_SetEngineVersion([MarshalAs(UnmanagedType.LPStr)]string lpszEngine);
    [DllImport(dllName)]
    public static extern void Unity_AppPageStart([MarshalAs(UnmanagedType.LPStr)]string szPageName);
    [DllImport(dllName)]
    public static extern void Unity_AppPageEnd([MarshalAs(UnmanagedType.LPStr)]string szPageName);
    [DllImport(dllName)]
    public static extern void Unity_AppSetEvent([MarshalAs(UnmanagedType.LPStr)]string szEventName, [MarshalAs(UnmanagedType.LPStr)]string szEventChannelID, [MarshalAs(UnmanagedType.LPStr)]string szEventInName, float eInData, [MarshalAs(UnmanagedType.LPStr)]string szEventOutName, float eOutData);
    [DllImport(dllName)]
    public static extern void Unity_AppReportLog(int iLogType, [MarshalAs(UnmanagedType.LPStr)]string szTypeName, [MarshalAs(UnmanagedType.LPStr)]string szLogContent);
    [DllImport(dllName)]
    public static extern void Unity_SetOverlay(int iLeftOverlayTextureID,int iRightOverlayTextureID, float fLeft, float fTop, float fWidth, float fHeight);
    [DllImport(dllName)]
    public static extern void Unity_SetOverlay3D(int iEyeType, int iTextureID, float fWidth, float fHeight, float fDistanceInMetre);

    /*以下是为了TextureBacker而设立的接口*/
    [StructLayout(LayoutKind.Sequential)]
    public struct backerInfo
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint texid;
        [MarshalAs(UnmanagedType.U4)]
        public uint x;
        [MarshalAs(UnmanagedType.U4)]
        public uint y;
        [MarshalAs(UnmanagedType.U4)]
        public uint width;
        [MarshalAs(UnmanagedType.U4)]
        public uint height;
        [MarshalAs(UnmanagedType.U4)]
        public uint desX;
        [MarshalAs(UnmanagedType.U4)]
        public uint desY;
    };

    //[DllImport(dllName)]
	//public static extern bool Unity_ktxLoadTextureN_1([MarshalAs(UnmanagedType.LPStr)]string filename, ref int pTexture, ref int pTarget, bool[] pIsMipmapped, ref int pGlerror, ref int pKvdLen);

    [DllImport(dllName)]
    public static extern bool Unity_GetBackerTexID(ref int texid);

    [DllImport(dllName)]
    public static extern void Unity_SetBackerTexture(int texid, int x, int y, int width, int height, int desX, int desY);

    [DllImport(dllName)]
    private static extern void Unity_SetStartBacker(int width, int height, backerInfo[] info, int infolen);

    public static void Unity_StartBacker(int width, int height, backerInfo[] info, int infolen)
    {
        MojingLog.LogTrace("Unity_StartBacker_Unity_StartBacker");
        Unity_SetStartBacker(width, height, info, infolen);
        try
        {
            Unity_IssuePluginEvent((int)UnityEventID.BackerTexture);
        }
        catch (Exception e)
        {
            MojingLog.LogError(e.ToString());
        }
    }
    /*以上是为了TextureBacker而设立的接口*/

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetUserSettings();
    public static string GetUserSettings()
    {
        IntPtr ptr =  Unity_GetUserSettings();
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GenerationGlassKey: " + result);
        return result;
    }

    [DllImport(dllName)]
    public static extern bool Unity_SetUserSettings([MarshalAs(UnmanagedType.LPStr)]string sUserSettings);

	[DllImport(dllName)]
	private static extern IntPtr Unity_GetLastMojingWorld([MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
	public static string GetLastMojingWorld(string strLanguageCodeByISO963)
	{
		IntPtr ptr = Unity_GetLastMojingWorld(strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GenerationGlassKey: " + result);
        return result;
    }
	
#endif
}
