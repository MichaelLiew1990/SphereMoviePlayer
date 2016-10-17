using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public struct PoseData 
{
    public Quaternion rotation;
    public Vector3 position;
}

public class RotateByMSFile : MonoBehaviour
{
    public List<PoseData> lstPoseData;
    public int videoFPS = 25;
    public string fileName;
    public string mp4Name;
    public GameObject sphereCanvas;
    public GameObject head;

    private MediaPlayerCtrl mediaCtrl;
    private int index = 0;
    private bool isPlaying = false;
    private bool isAndroid = false;

    void Start()
    {
        CheckPlat();
        mediaCtrl = sphereCanvas.GetComponent<MediaPlayerCtrl>();
        lstPoseData = new List<PoseData>();
        if (!ReadFile(isAndroid ? "mnt/sdcard/DCCPlayer/" : AssetPath() + fileName, ref lstPoseData))
            Debug.LogError("ReadFile Error!");
        Invoke("InvokeLoad", 0.2f);
    }

    void InvokeLoad()
    {
        MojingSDK.Unity_ResetTracker();
        mediaCtrl.Load("file:///sdcard/DCCPlayer/" + mp4Name);
        isPlaying = true;
    }

    void CheckPlat()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        isAndroid = false;
#elif UNITY_ANDROID
        isAndroid = true;
#endif
    }

    string AssetPath()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return Application.dataPath + "/StreamingAssets/";
#elif UNITY_ANDROID
        return "file:///sdcard/DCCPlayer/";
#endif
    }

    bool ReadFile(string _file, ref List<PoseData> _data)
    {
        FileStream fs = new FileStream(_file, FileMode.Open, FileAccess.Read);
        StreamReader sr = new StreamReader(fs);
        if (sr == null) return false;
        string line;
        bool isBegin = false;
        while ((line = sr.ReadLine()) != null)
        {
            if (isBegin && line.Contains(")")) break;
            if (!isBegin && line.Contains("animate on"))
            {
                isBegin = true;
                continue;
            }
            //解析字段↓↓↓↓
            //at time 652f cam1.rotation = quat 0.940160 0.008631 0.340106 0.018809
            //at time 652f cam1.position = [1096.819296, 1124.631145, 1246.293511]
            //at time 652f cam1.fov = 101.532578
            if (isBegin)
            {
                if (line.Contains("rotation"))
                {
                    int n = -1;
                    string str = "";
                    string[] lstStr;
                    string sign = "";
                    //新建数据
                    PoseData pose = new PoseData();
                    pose.rotation = Quaternion.identity;
                    pose.position = Vector3.zero;
                    //处理旋转
                    sign = "= quat ";
                    n = line.IndexOf(sign);
                    str = line.Trim().Substring(n + sign.Length);
                    lstStr = str.Split(' ');
                    if (lstStr.Length != 4) Debug.LogError("Roatation not 4:" + line);
                    for (int i = 0; i < 4; i++)
                    {
                        float val = float.Parse(lstStr[i].Trim());
                        if (i == 0) pose.rotation.x = val;
                        if (i == 1) pose.rotation.y = val;
                        if (i == 2) pose.rotation.z = val;
                        if (i == 3) pose.rotation.w = val;
                    }
                    //处理坐标
                    line = sr.ReadLine();//读下一行
                    if (!line.Contains("position")) Debug.LogError("Not Position:" + line);
                    sign = "= [";
                    n = line.IndexOf(sign);
                    str = line.Trim().Substring(n + sign.Length);
                    str = str.Remove(str.Length - 1);
                    lstStr = str.Split(',');
                    if (lstStr.Length != 3) Debug.LogError("Position not 3:" + line);
                    for (int i = 0; i < 3; i++)
                    {
                        float val = float.Parse(lstStr[i].Trim());
                        if (i == 0) pose.position.x = val;
                        if (i == 1) pose.position.y = val;
                        if (i == 2) pose.position.z = val;
                    }
                    //处理FOV
                    line = sr.ReadLine();//读下一行
                    //存储数据
                    _data.Add(pose);
                }
                else
                {
                    Debug.LogError("还没结束就已经不存在Rotation数据了:" + line);
                }
            }
        }
        sr.Close();
        sr.Dispose();
        return true;
    }

    void FixedUpdate()
    {
        if (!isPlaying) return;
        Debug.Log("Fixed" + index);
        if (index > 310) index = 0;
        Quaternion quat = new Quaternion(lstPoseData[index].rotation.x,
            lstPoseData[index].rotation.y,
            lstPoseData[index].rotation.z,
            lstPoseData[index].rotation.w);
        Vector3 pos = new Vector3(lstPoseData[index].position.x,
            lstPoseData[index].position.y,
            lstPoseData[index].position.z);

        //quat *= Quaternion.AngleAxis(Mathf.PI * 0.5f, new Vector3(1f, 0f, 0f));
        //quat.z = -quat.z;
        //float temp = pos.y;
        //pos.y = pos.z;
        //pos.z = temp;

        gameObject.transform.rotation = quat;
        //gameObject.transform.position = pos;

        //Camera.main.transform.position = new Vector3(pos.x, pos.y + 3, pos.z - 8);

        index++;
    }

    void Update()
    {

    }
}
