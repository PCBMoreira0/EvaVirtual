using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using static Utils.CommandJsonConverter;

public class APIComunication : MonoBehaviour
{
    [SerializeField] private string xml = "teste_EvaML";
    private string uuid = string.Empty;
    private string defaultUri = "http://localhost:8001";

    public event Action OnInitializationComplete;

    private class InitEndpoint
    {
        public string uuid;
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

    public void ChangeXML(string newXML)
    {
        xml = newXML;
    }

    public void UpdateIP(string ip)
    {
        defaultUri = String.Concat("http://", ip);
    }

    public void StartSimulation()
    {
        StartCoroutine(StartSimulationCoroutine());
    }


    IEnumerator StartSimulationCoroutine()
    {
        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/init", "", "application/json"))
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
                    uuid = JsonUtility.FromJson<InitEndpoint>(v).uuid;
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

        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/import/{uuid}/?path={xml}", "", "application/Json"))
        {
            // Import endpoint
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(web.error + $"Conte�do: {web.downloadHandler.text}");
                yield break;
            }
        }

        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/start/{uuid}", "", "application/Json"))
        {
            // Start endpoint
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(web.error);
                yield break;
            }
        }

        OnInitializationComplete?.Invoke();
    }

    public IEnumerator NextCommand(Action<CommandListJson> result)
    {
        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/next/{uuid}", "", "application/Json"))
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
                    CommandListJson commandListJson = JsonConvert.DeserializeObject<CommandListJson>(a);

                    result?.Invoke(commandListJson);
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

    public IEnumerator SendInput(string input)
    {
        InputField inputField = new InputField(input);
        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/send/{uuid}", JsonUtility.ToJson(inputField), "application/Json"))
        {
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("ERROR: " + web.error);
                yield break;
            }
        }
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
        using (var web = UnityWebRequest.Delete($"{defaultUri}/sim/delete/{uuid}"))
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
