//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
// 
// Author: Xu Xiang
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MojingLog
{

#if UNITY_EDITOR_OSX
	private static void Unity_Log(int level, string info, string filename, int line)
	{
	}
#else
	#if UNITY_EDITOR || UNITY_STANDALONE_WIN
	    private const string dllName = "libmojing";
#elif UNITY_IOS
		private const string dllName = "__Internal";
#elif UNITY_ANDROID
		private const string dllName = "mojing";
#endif
    [DllImport(dllName)]
	private static extern void Unity_Log(int level, [MarshalAs(UnmanagedType.LPStr)]string info, [MarshalAs(UnmanagedType.LPStr)]string filename, int line);	
#endif

	private static void log(string sInfo, int logLevel)
	{
#if DEBUG
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(2, true);
        System.Diagnostics.StackFrame sf = st.GetFrame(0);
        Unity_Log(logLevel, sInfo, sf.GetFileName(), sf.GetFileLineNumber());
#else
        Unity_Log(logLevel, sInfo, "MojingLog.cs", 38);
#endif
    }

    public static void LogError(string sInfo)
	{
		log (sInfo, 40000);
	}
	
	public static void LogWarn(string sInfo)
	{
		log (sInfo, 30000);
	}
	
	public static void LogTrace(string sInfo)
	{
		log (sInfo, 0);
	}
}

