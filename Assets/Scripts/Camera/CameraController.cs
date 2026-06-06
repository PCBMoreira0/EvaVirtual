using UnityEngine.Android;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem.Composites;
using System;
using UnityEngine.Events;
using System.IO;

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

        webCamTexture = new WebCamTexture(GetCamera(false));
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

        // Espera 1 segundo para a câmera inicializar e entregar a resolução real
        yield return new WaitForSeconds(1);

        int w = webCamTexture.width;
        int h = webCamTexture.height;

        // Criamos um Rect padrão (0,0 inicial; 1,1 cobrindo toda a imagem)
        Rect uvRect = new Rect(0, 0, 1, 1);

        // Se a câmera for Horizontal/Paisagem (Largura maior que Altura)
        if (w > h)
        {
            // Descobre a porcentagem da largura que precisamos manter para virar um quadrado
            float percentageToKeep = (float)h / w;

            uvRect.width = percentageToKeep;
            // Centraliza o corte no eixo X (horizontal)
            uvRect.x = (1f - percentageToKeep) / 2f;
        }
        // Se a câmera for Vertical/Retrato (Altura maior que Largura)
        else if (h > w)
        {
            // Descobre a porcentagem da altura que precisamos manter
            float percentageToKeep = (float)w / h;

            uvRect.height = percentageToKeep;
            // Centraliza o corte no eixo Y (vertical)
            uvRect.y = (1f - percentageToKeep) / 2f;
        }

        // Aplica o corte visual direto no componente RawImage
        camViewport.uvRect = uvRect;

        // Aplica a rotação necessária do celular
        camViewport.rectTransform.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);

        Camera.main.transform.localPosition = cameraModePosition;
        camObject.gameObject.SetActive(true);
    }

    public Texture2D GetCameraFrame()
    {
        if (!IsPlaying) return null;

        return CameraOperations.GetCroppedAndRotatedTexture(webCamTexture);
    }

    public Color32[] GetCameraFramePixels()
    {
        if (!IsPlaying) return null;

        return webCamTexture.GetPixels32();
    }

    public int GetCamWidth()
    {
        if (!IsPlaying) return 0;

        return webCamTexture.width;
    }

    public int GetCamHeight()
    {
        if (!IsPlaying) return 0;

        return webCamTexture.height;
    }

    public void StopCamera()
    {
        if (!IsPlaying) return;
        webCamTexture.Stop();

        Camera.main.transform.localPosition = originalCameraPosition;
        camObject.gameObject.SetActive(false);

        OnCameraDeactivated?.Invoke();
    }

    public void TestSaveImageToFile()
    {
        Texture2D finalPhoto = GetCameraFrame();

        if (finalPhoto != null)
        {
            // 1. Converte a textura para um array de bytes (formato JPG)
            byte[] bytes = finalPhoto.EncodeToJPG(100);

            // 2. Define o caminho onde será salvo (Application.persistentDataPath funciona no PC, Android e iOS)
            string filePath = Path.Combine(Application.persistentDataPath, "Teste_Camera_Recorte.jpg");

            // 3. Salva o arquivo no disco
            File.WriteAllBytes(filePath, bytes);

            Debug.Log("Imagem salva com sucesso no caminho: " + filePath);

            // 4. Limpa a textura da memória da Unity, já que salvamos no disco
            Destroy(finalPhoto);
        }
    }

}
