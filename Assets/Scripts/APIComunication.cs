using System;
using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class APIComunication : MonoBehaviour
{
    [SerializeField] private string xml = "teste_EvaML";
    private string uuid = string.Empty;
    private string defaultUri = "http://192.168.1.93:8000";

    public event Action OnInitializationComplete;

    private class InitEndpoint
    {
        public string uuid;
    }

    #region Json Commands
    public class CommandJson
    {
        public string command;

        public static T Parse<T>(CommandJson command) where T : CommandJson
        {
            if(command is T converted)
            {
                return converted;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast {command.GetType()} to {typeof(T)}");
            }
        }
    }

    public class CommandAudioJson : CommandJson
    {
        public string file;
        public bool block;
    }
    public class CommandTalkJson : CommandJson { public string text; }
    public class CommandWaitJson : CommandJson { public float wait; }
    public class CommandEmotionJson : CommandJson { public string emotion; }
    public class CommandMotionJson : CommandJson { public string member; public string direction; }
    public class CommandListenJson : CommandJson { public string state; }
    public class CommandQRCodeJson : CommandJson { }
    public class CommandUserEmotionJson : CommandJson { }
    public class CommandLedAnimation : CommandJson { public string color; }
    #endregion

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
                Debug.LogError(web.error + $"Conteúdo: {web.downloadHandler.text}");
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

    public IEnumerator NextCommand(Action<CommandJson> result)
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
                    var c = JsonUtility.FromJson<CommandJson>(a);

                    switch (c.command)
                    {
                        case "Talk":
                            c = JsonUtility.FromJson<CommandTalkJson>(a);
                            break;
                        case "Wait":
                            c = JsonUtility.FromJson<CommandWaitJson>(a);
                            break;
                        case "Emotion":
                            c = JsonUtility.FromJson<CommandEmotionJson>(a);
                            break;
                        case "Motion":
                            c = JsonUtility.FromJson<CommandMotionJson>(a);
                            break;
                        case "Listen":
                            c = JsonUtility.FromJson<CommandListenJson>(a);
                            break;
                        case "Audio":
                            c = JsonUtility.FromJson<CommandAudioJson>(a);
                            break;
                        case "QR_Read":
                            c = JsonUtility.FromJson<CommandQRCodeJson>(a);
                            break;
                        case "User_emotion":
                            c = JsonUtility.FromJson<CommandUserEmotionJson>(a);
                            break;
                        case "Led_animation":
                            c = JsonUtility.FromJson<CommandLedAnimation>(a);
                            break;
                    }

                    result?.Invoke(c);
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
        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/tts/inference", JsonUtility.ToJson(input), "application/Json"))
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
