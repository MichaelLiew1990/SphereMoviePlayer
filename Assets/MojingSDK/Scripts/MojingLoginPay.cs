using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MojingLoginPay : MonoBehaviour {
    void Awake()
    {
        MojingSDK.Unity_SetEngineVersion("Unity " + Application.unityVersion);
    }
    void Start () {
		//绑定事件
		ArrayList btnsName = new ArrayList();
		btnsName.Add ("SingleLogin");
		btnsName.Add ("DoubleLogin");
		btnsName.Add ("MjAppAutoLogin");
		btnsName.Add ("Register");
		btnsName.Add ("SinglePay");
		btnsName.Add ("DoublePay");
		btnsName.Add ("GetBalance");
		foreach (string btnName in btnsName ) {
			GameObject btnObj = GameObject.Find(btnName);
			Button btn = btnObj.GetComponent<Button>();
			btn.onClick.AddListener(delegate() {    OnClick(btnObj);    });
		}
	}

    //单屏登录
    public static void SingleLogin()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
		javaObject.CallStatic ("mjCallSingleLogin");
#endif
        Debug.Log("SingleLogin");
    }
    //双屏登录
    public static void DoubleLogin()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
        javaObject.CallStatic("mjCallDoubleLogin");
#endif
        Debug.Log("DoubleLogin");
    }
    //同步App登录状态
    public static void MjAppAutoLogin()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
        javaObject.CallStatic("mjAppAutoLogin");
#endif
    }
    //注册
    public static void Register()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
        javaObject.CallStatic("mjCallRegister");
#endif
    }
    //单屏支付1魔币
    public static void SinglePay()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
        javaObject.CallStatic("mjGetPayToken", "1.0", "clientOrder", "0");
#endif
    }
	public static void DoublePay()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
		javaObject.CallStatic("mjGetPayToken", "1.0", "clientOrder", "1");
#endif
	}
    public static void Pay(string token)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
		javaObject.CallStatic("mjPayMobi", "1.00", "北京区服", token , "clientOrder");
#endif
    }
    public static void GetBalance()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
	    javaObject.CallStatic("mjGetBalance");
#endif
    }
    //ButtonClickedEvent
    void OnClick(GameObject btnObj) {
		switch (btnObj.name) {
			case "SingleLogin": //单屏登录
                SingleLogin();
			break;

			case "DoubleLogin": //双屏登录
                DoubleLogin();
			break;

			case "MjAppAutoLogin": //同步App登录状态
                MjAppAutoLogin();
			break;

			case "Register": //注册
                Register();
			break;

			case "SinglePay": //获取支付Token(单屏)
				SinglePay();
			break;

			case "DoublePay": //获取支付Token(双屏)
				DoublePay();
			break;

			case "GetBalance": //获取余额
				GetBalance();
			break;
		}

	}

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_IOS
    void OnGUI()
    {
        GUI.skin.label.fontSize = 30;
        GUI.color = Color.red;
        GUILayout.Label("iOS/Windows can not support Login & Pay Model now~", GUILayout.Width(1500));
    }
#endif
}
