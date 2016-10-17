using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public GameObject netManagerPrefab;
    public Button btnUse245Server;
    public Text txtInfo;

    private ClientNetworkMgr net;
    private bool isReadyForPlay = false;

    void Start()
    {
        btnUse245Server.onClick.AddListener(Use245Seaver);
        txtInfo.text = "正在连接...";

        if (!GameObject.Find("NetworkManager"))
        {
            GameObject netObj = GameObject.Instantiate(netManagerPrefab) as GameObject;
            netObj.name = "NetworkManager";
            net = netObj.GetComponent<ClientNetworkMgr>();
        }
        else
        {
            isReadyForPlay = true;
            txtInfo.text = "";
            btnUse245Server.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (!isReadyForPlay)
        {
            if (net!=null && net.netState == NetState.Connected)
            {
                isReadyForPlay = true;
                txtInfo.text = "";
                btnUse245Server.gameObject.SetActive(false);
            }
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void Use245Seaver()
    {
        net.StartServerByIP("192.168.15.245");
        txtInfo.text = "正在连接192.168.15.245...";
    }
}
