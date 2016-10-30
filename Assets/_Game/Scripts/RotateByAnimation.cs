using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RotateByAnimation : MonoBehaviour
{
    public GameObject animObj;//自带旋转的动画
    public GameObject targetObj;//需要旋转的物体
    public Text txtRotat;

    private bool isPlaying = false;
    private Animator anim;
    private ClientNetworkMgr net;

    void Awake()
    {
        anim = animObj.GetComponent<Animator>();
    }

    void Start()
    {
        net = NetworkManager.singleton as ClientNetworkMgr;
        if (net == null)
        {
            Debug.LogError("Net is Null");
        }
    }

    void Update()
    {
        if (isPlaying)
        {
            //3DsMax中出来的动画需要将座椅左右旋转取反
            if (targetObj != null)
            {
                float fix = 1f;
                string sn = SceneManager.GetActiveScene().name;
                if (sn == "1_GameFile1") fix = 1f;
                if (sn == "1_GameFile2") fix = -1f;
                targetObj.transform.rotation = Quaternion.Euler(
                    -animObj.transform.rotation.eulerAngles.x,
                    animObj.transform.rotation.eulerAngles.y/* * fix*/,
                    -animObj.transform.rotation.eulerAngles.z);
                targetObj.transform.position = animObj.transform.position;
            }

            //txtRotat.text = (int)(anim.GetCurrentAnimatorStateInfo(0).normalizedTime
            //    * anim.GetCurrentAnimatorStateInfo(0).length * 1000) + "(ms)" + (net!=null?net.localIP:"");
        }
    }

    void FixedUpdate()
    {
        if (net != null && net.IsClientConnected() && net.GetNetPlayer() != null && isPlaying)
        {
            txtRotat.text = "HostIP=" + net.hostIP + "LocalIP=" + net.localIP;
            if (net.hostIP == net.localIP || net.localIP == "0.0")
            {
                net.GetNetPlayer().CmdUpdateCarPose(targetObj.transform.rotation,
                    targetObj.transform.position, net.localIP);
            }
        }
    }

    public void Play()
    {
        isPlaying = true;
        anim.SetBool("isPlay", true);
    }

    public void EndPlay()
    {
        isPlaying = false;
        anim.SetBool("isPlay", false);

        if (net != null && net.IsClientConnected() && net.GetNetPlayer() != null)
        {
            if (net.hostIP == net.localIP || net.localIP == "0.0")
            {
                net.GetNetPlayer().CmdPlatformServerExec(PlatformCommand.ContentStoped, "");
            }
        }
    }
}
