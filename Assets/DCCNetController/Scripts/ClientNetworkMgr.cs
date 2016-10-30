using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Networking;

public enum NetState
{
    None,
    Connecting,
    Connected,
    Failed
}

public class ClientNetworkMgr : NetworkManager
{
    [HideInInspector]
    public string netStateInfo;
    [HideInInspector]
    public NetState netState = NetState.None;
    [HideInInspector]
    public string netErrorInfo = "";

    public string hostIP;//网络中姿态控制主机的IP后三位

    [HideInInspector]
    public string localIP = "";//IP后三位

    private SearchServerIP searchIP;
    private NetPlayer localNetPlayer;


    public NetPlayer GetNetPlayer()
    {
        if (localNetPlayer == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponent<NetworkIdentity>().hasAuthority)
                {
                    localNetPlayer = players[i].GetComponent<NetPlayer>();
                    break;
                }
            }
        }
        return localNetPlayer;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnClientConnect:" + conn.address);
        Debug.Log("ID:" + conn.connectionId);
        netState = NetState.Connected;
        base.OnClientConnect(conn);
    }

    // 当网络问题时调用
    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        netErrorInfo = conn.ToString() + "ErrorCode=" + errorCode;
        netState = NetState.Failed;
        base.OnClientError(conn, errorCode);
    }

    // 在客户端上当服务器被断开时调用
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        StopClient();
        Debug.Log("OnClientDisconnect");
        base.OnClientDisconnect(conn);
    }

    //断开自动搜索服务器功能，启动传入IP为为服务器
    public void StartServerByIP(string _ip)
    {
        searchIP.searchedIP = _ip;
        SearchingServerIpSucceed();
        searchIP.StopSearch();
        Destroy(searchIP, 2f);
    }

    void OnDestroy()
    {
        StopClient();
    }

    void FixedUpdate()
    {
        if (!isNetworkActive)
        {
            netState = NetState.Failed;
        }
    }

    void Start()//这个函数理论上进进入一次，多次进入那就错了
    {
        searchIP = gameObject.GetComponent<SearchServerIP>();
        netState = NetState.Connecting;
        netStateInfo = "搜索主机中...";

        StartNetworking();
    }

    void StartNetworking()
    {
        searchIP.onSucceed.AddListener(SearchingServerIpSucceed);
        searchIP.onFailed.AddListener(SearchingServerIpFailed);
    }

    void SearchingServerIpSucceed()
    {
        netStateInfo = "主机IP:" + searchIP.searchedIP;
        networkAddress = searchIP.searchedIP;
        StartClient();
        StartCoroutine(CoAutoReConnectNetwork());
    }

    void SearchingServerIpFailed()
    {
        netState = NetState.Failed;
        netStateInfo = "搜索服务器超时！";
    }

    // 网络稳定性处理
    IEnumerator CoAutoReConnectNetwork()
    {
        //第一次启动延缓检查时间
        yield return new WaitForSeconds(3f);

        bool isConnected = false;
        while (true)
        {
            Debug.Log("Checking!");
            localIP = RefreshLocalIP();
            if (localIP.Length > 3) localIP = localIP.Substring(localIP.Length - 3);
            yield return new WaitForSeconds(3f);
            if (!isNetworkActive)
            {
                isConnected = false;
                Debug.Log("Restart!");
                StopClient();
                yield return new WaitForSeconds(1f);
                StartClient();
                netState = NetState.Connecting;
                netStateInfo = "重启连接中...";
                yield return new WaitForSeconds(5f);
            }
            else//连接成功 或 正在尝试连接
            {
                if (!isConnected)
                {
                    isConnected = IsClientConnected();
                    if (isConnected) netState = NetState.Connected;
                    netStateInfo = "";
                }
            }
        }
    }

    string RefreshLocalIP()
    {
        return Network.player.ipAddress;
    }
}
