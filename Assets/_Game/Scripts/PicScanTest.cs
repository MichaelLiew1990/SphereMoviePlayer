using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class PicScanTest : MonoBehaviour
{
    public string picName;
    public Text textInfo;
    
    void Start()
    {
        GetComponent<MeshRenderer>().material.mainTexture = LoadTexture(picName);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        else
        {
            textInfo.text = "File:" + filePath + " not exist!";
        }

        if (tex == null)
        {
            textInfo.text = "Load failed! please contact Michael!";
        }
        else
        {
            textInfo.text = "";
        }
        return tex;
    }
}
