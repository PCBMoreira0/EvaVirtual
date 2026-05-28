using System;
using System.Collections;
using System.Collections.Generic;
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
        reader = new BarcodeReader
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
            }
        };
    }

    public IEnumerator Scan(Action<string> result)
    {
        yield return cameraController.StartCamera(true);

        Result codeResult = null;
        do
        {
            if (!cameraController.IsCamAvailable) break;

            codeResult = reader.Decode(cameraController.GetCameraFramePixels(), cameraController.GetCamWidth(), cameraController.GetCamHeight());
            yield return new WaitForSeconds(0.5f);
        } while (codeResult == null);

        cameraController.StopCamera();
        if (result != null)
            result?.Invoke(codeResult.Text);
        else
            result?.Invoke("");
    }

    public void Scann()
    {
        StartCoroutine(Scan((result) => { Debug.Log(result); }));
    }
}
