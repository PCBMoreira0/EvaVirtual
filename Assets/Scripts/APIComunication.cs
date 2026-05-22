using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using static Utils.CommandJsonConverter;
using UnityEngine.Events;
using UnityEditor;

public class APIComunication : MonoBehaviour
{
    [SerializeField] private string xml = "teste_EvaML";
    private string uuid = string.Empty;
    private string defaultUri = "http://localhost:8000";

    public UnityEvent<string> OnInitializationComplete;

    private class InitEndpoint
    {
        public string message;
        public string user_id;
    }

    public class InputField
    {
        public string input = "";
        public InputField(string input)
        {
            this.input = input;
        }
    }

    public class AIResultField
    {
        public string result = "";
    }
    
#if UNITY_EDITOR
    private async void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            await DeleteSimulator();
        }
    }
#endif

    public void UpdateIP(string ip)
    {
        defaultUri = string.Concat("http://", ip);
    }

    public void StartSimulation()
    {
        StartCoroutine(StartSimulationCoroutine());
    }


    public IEnumerator StartSimulationCoroutine()
    {
        using (var web = UnityWebRequest.Get($"{defaultUri}/init"))
        {
            // Init endpoint
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(web.error);
                yield break;
            }

            string v = web.downloadHandler.text;
            if (!string.IsNullOrEmpty(v))
            {
                try
                {
                    uuid = JsonUtility.FromJson<InitEndpoint>(v).user_id;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Erro ao fazer parse do json: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("Speech To Text: Json returns null.");
            }
        }

        OnInitializationComplete?.Invoke(uuid);
    }


    public IEnumerator GetTTS(string text, Action<AudioClip> result)
    {
        InputField input = new InputField(text);
        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/tts/", JsonUtility.ToJson(input), "application/Json"))
        {
            web.downloadHandler = new DownloadHandlerAudioClip(web.uri, AudioType.MPEG);

            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
                yield break;
            }
            DownloadHandlerAudioClip dhandler = (DownloadHandlerAudioClip)web.downloadHandler;
            result?.Invoke(dhandler.audioClip);
        }
    }

    public IEnumerator GetSTT(AudioClip clip, Action<string> result)
    {
        WWWForm www = new WWWForm();
        var b = AudioOperations.ConvertAudioClipToWav(clip);
        www.AddBinaryData("file", b, "audio.wav", "audio/wav");

        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/stt", www))
        {
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(web.error);
                yield break;
            }
            string a = web.downloadHandler.text;

            if (!string.IsNullOrEmpty(a))
            {
                try
                {
                    var c = JsonUtility.FromJson<AIResultField>(a);
                    result?.Invoke(c.result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Erro ao fazer parse do json: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("Speech To Text: Json returns null.");
            }
        }
    }

    public IEnumerator GetAudio(string text, Action<AudioClip> result)
    {
        using (var web = UnityWebRequest.Get($"{defaultUri}/sim/audio/{text}"))
        {
            web.downloadHandler = new DownloadHandlerAudioClip(web.uri, AudioType.WAV);

            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
                yield break;
            }
            DownloadHandlerAudioClip dhandler = (DownloadHandlerAudioClip)web.downloadHandler;
            result?.Invoke(dhandler.audioClip);
        }
    }

    public IEnumerator GetEmotion(Texture2D image, Action<string> result)
    {
        WWWForm www = new WWWForm();
        var b = image.EncodeToPNG();
        www.AddBinaryData("file", b, "image.png", "image/png");

        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/emotion", www))
        {
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(web.error);
                yield break;
            }
            string a = web.downloadHandler.text;

            if (!string.IsNullOrEmpty(a))
            {
                try
                {
                    var c = JsonUtility.FromJson<AIResultField>(a);
                    result?.Invoke(c.result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Erro ao fazer parse do json: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("Emotion Recognition: Json returns null.");
            }
        }
    }

    public async Awaitable DeleteSimulator()
    {
        using (var web = UnityWebRequest.Get($"{defaultUri}/delete/{uuid}"))
        {
            await web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(web.error);
            }
        }
    }

    private async void OnDestroy()
    {
        await DeleteSimulator();
    }
}
