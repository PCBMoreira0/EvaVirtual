using System;
using System.Collections;
using UnityEngine;

public class UserEmotionController : MonoBehaviour
{
    [SerializeField] private float delayBeforeScanning = 1.0f;

    private CameraController cameraController;
    private void Awake()
    {
        cameraController = GetComponent<CameraController>();
    }

    public IEnumerator ScanEmotion(APIComunication api, Action<string> result)
    {
        yield return StartCoroutine(cameraController.StartCamera(true));

        yield return new WaitForSeconds(delayBeforeScanning);

        string emotion = "";
        do
        {
            Texture2D texture = cameraController.GetCameraFrame();
            yield return api.GetEmotion(texture, (result) => { emotion = result; });
            Destroy(texture);
            yield return null;
        }
        while (string.IsNullOrEmpty(emotion));

        cameraController.StopCamera();
        result?.Invoke(emotion);
    }

    public void Scan(APIComunication api)
    {
        StartCoroutine(ScanEmotion(api, (result) => { Debug.Log(result); }));
    }
}
