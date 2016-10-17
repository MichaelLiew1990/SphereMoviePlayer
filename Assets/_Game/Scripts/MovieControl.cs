using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class MovieControl : MonoBehaviour
{
    public MediaPlayerCtrl mediaCtrl;
    public RotateByAnimation rotateCtrl;
    public Text txtMovie;
    public bool isTestScene = false;

    private bool isPlaying = false;

    public void PlayVedio(string vedioName)
    {
        if (isTestScene) Invoke("InvokePlayRotate", 1.5f);
        else mediaCtrl.Play();
    }

    void InvokePlayRotate()
    {
        rotateCtrl.Play();
    }

    void Awake()
    {
        if (!isTestScene) {
            //mediaCtrl = gameObject.AddComponent<MediaPlayerCtrl>();
            mediaCtrl.m_bLoop = false;
            mediaCtrl.m_bAutoPlay = false;
        }
    }

    void Start()
    {
        if (!isTestScene) {
            mediaCtrl.OnEnd += MovieEnd;
            mediaCtrl.OnReady += MovieReady;
            mediaCtrl.OnVideoFirstFrameReady += MovieFirstReady;
        }
        Invoke("InvokeResetMojingTracker", 1f);
    }

    void InvokeResetMojingTracker()
    {
        MojingSDK.Unity_ResetTracker();
    }

    public void MovieEnd()
    {
        isPlaying = false;
        txtMovie.text = "Movie End!";
        rotateCtrl.EndPlay();

        Invoke("InvokeLoadInitialScene", 1f);
    }

    void InvokeLoadInitialScene()
    {
        SceneManager.LoadScene("0_Initial");
    }

    void MovieReady()
    {
        txtMovie.text = "Movie Ready!";
    }

    void MovieFirstReady()
    {
        isPlaying = true;
        txtMovie.text = "Movie First Ready!";
        rotateCtrl.Play();
    }

    void Update()
    {
        if (isPlaying) {
            txtMovie.text = mediaCtrl.GetSeekPosition() + "(ms)";
        }
    }
}
