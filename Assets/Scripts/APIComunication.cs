using System;
using System.Collections;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class APIComunication : MonoBehaviour
{
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
        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/init", "", "Content/json"))
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

        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/import/{uuid}/?path=teste_EvaML", "", "Content/Json"))
        {
            // Import endpoint
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
                Debug.Log(web.downloadHandler.text);
            }
        }

        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/start/{uuid}", "", "Content/Json"))
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
        using (var web = UnityWebRequest.Post($"{defaultUri}/sim/next/{uuid}", "", "Content/Json"))
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
}
