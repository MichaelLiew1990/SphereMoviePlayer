using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public enum PlatformCommand
{
    ContentStarted,
    ContentStoped
}

public class NetPlayer : NetworkBehaviour
{
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
    public void CmdPlatformServerExec(PlatformCommand cmd, string arg)
    {
        //客户端不需要实现
    }

    string sceneName;
    [ClientRpc]
    void RpcStartGame(string name)
    {
        sceneName = name;
        SceneManager.LoadScene("1_Game" + sceneName);
        Invoke("InvokePlayMovie", 3f);
    }
    
    void InvokePlayMovie()
    {
        CmdPlatformServerExec(PlatformCommand.ContentStarted, "");
        MovieControl ctrl = GameObject.FindObjectOfType<MovieControl>();
        if (ctrl != null)
        {
            ctrl.PlayVedio(sceneName);
        }
    }

    [ClientRpc]
    void RpcStopGame()
    {
        CmdPlatformServerExec(PlatformCommand.ContentStoped, "");
        MovieControl mc = GameObject.FindObjectOfType<MovieControl>();
        if (mc!=null)
        {
            mc.MovieEnd();
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

    [ClientRpc]
    void RpcGameAlreadyStart()
    {
        //TV端实现
    }

    [ClientRpc]
    void RpcGameAlreadyStop()
    {
        //TV端实现
    }
}
