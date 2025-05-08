using System;
using System.Collections;
using UnityEngine;
using ZXing;
using ZXing.Common;

public class QRCodeController : MonoBehaviour
{
    private CameraController cameraController;
    BarcodeReader reader;
    private void Awake()
    {
        cameraController = GetComponent<CameraController>();
    }

    private void Start()
    {
        reader = new BarcodeReader();
    }

    public IEnumerator Scan(Action<string> result)
    {
        yield return cameraController.StartCamera(false);
        
        Result codeResult;
        do
        {
            if(!cameraController.IsCamAvailable) yield break; 
            Texture2D frame = cameraController.GetCameraFrame();
            codeResult = reader.Decode(frame.GetPixels32(), frame.width, frame.height);
            yield return new WaitForSeconds(0.2f);
        } while (codeResult == null);

        cameraController.StopCamera();
        result?.Invoke(codeResult.Text);
    }

    public void Scann()
    {
        StartCoroutine(Scan((result) => { Debug.Log(result); }));
    }
}
