using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetPlayer : NetworkBehaviour
{
    [SyncVar]
    //[HideInInspector]
    public float sync_H;//服务器更新客户端读取
    [SyncVar]
    //[HideInInspector]
    public float sync_V;//服务器更新客户端读取
    

    //下面的不成功，因为外面获取hostIP值时LocalPlayer并不一定有值，有可能跟服务端调用哪个NetPlayer组件发送消息有关
    //public string hostIP;//用于发送姿态的主客户端（IP后三位）
    //[SyncVar]
    ////[HideInInspector]
    //public string sync_HostIP;//服务器更新客户端读取（IP后三位）

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    [Command]
    public void CmdUpdateCarPose(Quaternion rotate, Vector3 pos, string clientIP)
    {
        //客户端不需要实现
    }

    [Command]
    public void CmdMovieEnd()
    {
        //客户端不需要实现
    }

    [ClientRpc]
    void RpcStartGame(string sceneName)
    {
        SceneManager.LoadScene("1_Game" + sceneName);
    }

    [ClientRpc]
    void RpcStopGame()
    {
        MovieControl mc = GameObject.FindObjectOfType<MovieControl>();
        if (mc!=null)
        {
            mc.MovieEnd();
        }
    }

    [ClientRpc]
    void RpcPlayMovie(string mp4Name)
    {
        MovieControl ctrl = GameObject.FindObjectOfType<MovieControl>();
        if (ctrl != null)
        {
            ctrl.PlayVedio(mp4Name);
        }
    }

    [ClientRpc]
    void RpcUpdateHostIP(string hostIP)
    {
        ClientNetworkMgr net = GameObject.FindObjectOfType<ClientNetworkMgr>();
        if (net != null)
        {
            net.hostIP = hostIP;
        }

        //每次更新host时将服务端姿态置为初始值
        CmdUpdateCarPose(Quaternion.identity, Vector3.zero, "xxx");
    }
}
