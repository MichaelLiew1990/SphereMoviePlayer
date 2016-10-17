using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;
using UnityEngine.Events;

public class SearchServerIP : MonoBehaviour
{
    public string searchedIP = "";
    public int port = 6677;//用于局域网搜索的特定端口
    [HideInInspector]
    public UnityEvent onSucceed;
    [HideInInspector]
    public UnityEvent onFailed;

    private string strInfo;
    private string specialText = "$$Strom-Mojing-DCC$$";
    private bool isRunning = false;
    private Thread clientThread = null;
    private UdpClient UdpListen = null;
    float flowedime = 0;

    private int isSucceed = 0;
    private int isFailed = 0;

    public void StopSearch()
    {
        isRunning = false;
        if (UdpListen != null) UdpListen.Close();
        if (clientThread != null && clientThread.IsAlive) clientThread.Abort();
    }

    void StartSearch()//客户端收消息直到收到服务器的IP
    {
        strInfo = "Starting Client...";
        if (clientThread != null && clientThread.IsAlive) return;

        clientThread = new Thread(() =>
        {
            UdpListen = new UdpClient(new IPEndPoint(IPAddress.Any, port));

            while (isRunning)
            {
                Thread.Sleep(500);
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
                byte[] bufRev = UdpListen.Receive(ref endpoint);//this method will block, Close() can stop it
                string msg = Encoding.Unicode.GetString(bufRev, 0, bufRev.Length);
                if (msg.Contains(specialText))
                {
                    searchedIP = endpoint.Address.ToString();
                    isSucceed = 1;
                    StopSearch();
                    return;
                }
            }

            UdpListen.Close();
        });

        clientThread.IsBackground = true;
        InitSearch();
        clientThread.Start();
    }

    void Awake()
    {
        onSucceed = new UnityEvent();
        onFailed = new UnityEvent();
    }

    void FixedUpdate()
    {
        CheckTimeout();
        if (isSucceed==1)
        {
            onSucceed.Invoke();
            isSucceed = 2;
            StopSearch();//一定要调用呀
            Destroy(this, 2f);//一定要调用呀
        }
        if (isFailed==1)
        {
            onFailed.Invoke();
            isFailed = 2;
        }
    }

    void Start()
    {
        StartSearch();
    }

    void OnDestroy()
    {
        //一定要调用呀
        StopSearch();
    }

    void CheckTimeout()
    {
        if (isRunning)
        {
            flowedime += Time.deltaTime;
            if (flowedime > 10f)
            {
                isFailed = 1;
                StopSearch();
            }
        }
    }

    void InitSearch()
    {
        isRunning = true;
        flowedime = 0f;
        if (UdpListen != null) UdpListen.Close();
    }
}
