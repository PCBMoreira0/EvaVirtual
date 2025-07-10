using UnityEngine.Android;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem.Composites;
using System;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
    public bool IsPlaying { get { return webCamTexture.isPlaying; } }
    public bool IsCamAvailable { get { return isCamAvailable; } private set { isCamAvailable = value; } }

    [SerializeField] private UnityEvent OnCameraActivated;
    [SerializeField] private UnityEvent OnCameraDeactivated;
    [SerializeField] private Vector3 cameraModePosition;
    private Vector3 originalCameraPosition;

    [SerializeField] private GameObject camObject;
    [SerializeField] private RawImage camViewport;
    [SerializeField] private bool isCamAvailable = false;
    private WebCamDevice selectedCamera;
    private WebCamTexture webCamTexture;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        camViewport = camObject.GetComponentInChildren<RawImage>(true);

        originalCameraPosition = Camera.main.transform.localPosition;

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        WebCamDevice[] webCamDevices = WebCamTexture.devices;
        if (webCamDevices.Length == 0)
        {
            IsCamAvailable = false;
            Debug.Log("Nenhuma camera disponível");
            yield break;
        }

        for (int i = 0; i < webCamDevices.Length; i++)
        {
            Debug.Log(webCamDevices[i].name);
        }
        IsCamAvailable = true;

        webCamTexture = new WebCamTexture(GetCamera(false), (int)camViewport.rectTransform.rect.width, (int)camViewport.rectTransform.rect.height);
        camViewport.texture = webCamTexture;
    }

    private string GetCamera(bool isFront)
    {
        if (!IsCamAvailable) return "";

        foreach (var device in WebCamTexture.devices)
        {
            if (device.name.Contains("OBS")) continue;

            if (isFront && device.isFrontFacing)
            {
                selectedCamera = device;
                return device.name;
            }
            else if (!isFront && !device.isFrontFacing)
            {
                selectedCamera = device;
                return device.name;
            }
        }
        selectedCamera = WebCamTexture.devices[0];
        return WebCamTexture.devices[0].name;
    }

    public void StartCam()
    {
        StartCoroutine(StartCamera(true));
    }

    public IEnumerator StartCamera(bool isFrontCamera) 
    {
        if (!IsCamAvailable) yield break;

        if (IsPlaying) yield break;

        OnCameraActivated?.Invoke();

        webCamTexture.deviceName = GetCamera(isFrontCamera);
        webCamTexture.Play();  
        yield return new WaitForSeconds(1);
        camViewport.rectTransform.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);

        Camera.main.transform.localPosition = cameraModePosition;
        camObject.gameObject.SetActive(true);
    }
    
    public Texture2D GetCameraFrame()
    {
        if(!IsPlaying) return null;

        return CameraOperations.RotateTexture(webCamTexture.GetPixels32(), webCamTexture.width, webCamTexture.height, 360 - webCamTexture.videoRotationAngle); 
    }

    public void StopCamera()
    {
        if(!IsPlaying) return;
        webCamTexture.Stop();

        Camera.main.transform.localPosition = originalCameraPosition;
        camObject.gameObject.SetActive(false);

        OnCameraDeactivated?.Invoke();
    }
}
