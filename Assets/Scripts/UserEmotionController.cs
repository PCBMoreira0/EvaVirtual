using System;
using System.Collections;
using UnityEngine;

public class UserEmotionController : MonoBehaviour
{
    [SerializeField] private float delayBeforeScanning = 1.0f;

    private CameraController cameraController;
    private APIComunication api;

    private void Awake()
    {
        api = GetComponent<APIComunication>();
        cameraController = GetComponent<CameraController>();
    }

    public IEnumerator ScanEmotion(Action<string> result)
    {
        yield return StartCoroutine(cameraController.StartCamera(true));

        yield return new WaitForSeconds(delayBeforeScanning);

        string emotion = "";
        do
        {
            Texture2D texture = cameraController.GetCameraFrame();
            yield return StartCoroutine(api.GetEmotion(texture, (result) => { emotion = result; }));

            yield return null;
        }
        while (string.IsNullOrEmpty(emotion));

        cameraController.StopCamera();
        result?.Invoke(emotion);
    }

    public void Scan()
    {
        StartCoroutine(ScanEmotion((result) => { Debug.Log(result); }));
    }
}
