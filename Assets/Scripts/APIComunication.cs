using System;
using System.Collections;
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
    #endregion

    public class InputField
    {
        public string input = "";
        public InputField(string input)
        {
            this.input = input;
        }
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
                Debug.Log("ERROR: " + web.error);
            }

            string v = web.downloadHandler.text;
            uuid = JsonUtility.FromJson<InitEndpoint>(v).uuid;
        }

        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/import/{uuid}/?path={xml}", "", "application/Json"))
        {
            // Import endpoint
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
                Debug.Log(web.downloadHandler.text);
            }
        }

        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/start/{uuid}", "", "application/Json"))
        {
            // Start endpoint
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
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
                Debug.Log("ERROR: " + web.error);
            }
        
            string a = web.downloadHandler.text;
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
            }
      
            result?.Invoke(c);
        }
    }

    public IEnumerator GetTTS(string text, Action<AudioClip> result)
    {
        InputField input = new InputField(text);
        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/tts", JsonUtility.ToJson(input), "application/Json"))
        {
            web.downloadHandler = new DownloadHandlerAudioClip(web.uri, AudioType.WAV);
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
            }
            DownloadHandlerAudioClip dhandler = (DownloadHandlerAudioClip)web.downloadHandler;
            result?.Invoke(dhandler.audioClip);
        }
    }

    public async Awaitable DeleteSimulator()
    {
        using (var web = UnityWebRequest.Delete($"{defaultUri}/sim/delete/{uuid}"))
        {
            await web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
            }
        }
    }

    private async void OnDestroy()
    {
        await DeleteSimulator();
    }
}
