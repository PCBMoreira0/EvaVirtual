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

    public UnityEvent initializationComplete;
    public UnityEvent<CommandJson> command;

    private class InitEndpoint
    {
        public string uuid;
    }

    #region Json Commands
    public class CommandJson
    {
        public string command;
    }

    public class CommandAudioJson : CommandJson
    {
        public string file;
        public bool block;
    }
    public class CommandTalkJson : CommandJson { public string talk; }
    public class CommandWaitJson : CommandJson { public float wait; }
    public class CommandEmotionJson : CommandJson { public string emotion; }
    #endregion

    public void UpdateIP(string ip)
    {
        defaultUri = "http://" + ip;
    }

    public void StartSimulation()
    {
        StartCoroutine(StartSimulationCoroutine());
    }


    IEnumerator StartSimulationCoroutine()
    {
        using (var web = UnityWebRequest.Post(defaultUri + "/sim/init", "", "Content/json"))
        {
            // Init endpoint
            yield return web.SendWebRequest();
            Debug.Log(web.uri);

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
            }

            string v = web.downloadHandler.text;
            uuid = JsonUtility.FromJson<InitEndpoint>(v).uuid;
        }

        using (var web = UnityWebRequest.Post(defaultUri + "/sim/import/" + uuid + "/?path=listen_EvaML", "", "Content/Json"))
        {
            Debug.Log(web.uri);
            // Import endpoint
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
                Debug.Log(web.downloadHandler.text);
            }
        }

        using (var web = UnityWebRequest.Post(defaultUri + "/sim/start/" + uuid, "", "Content/Json"))
        {
            // Start endpoint
            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR: " + web.error);
            }
        }

        initializationComplete?.Invoke();
    }

    public IEnumerator NextCommand()
    {
        using (var web = UnityWebRequest.Post(defaultUri + "/sim/next/" + uuid, "", "Content/Json"))
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
                    var b = JsonUtility.FromJson<CommandTalkJson>(a);
                    c = b;
                    Debug.Log(b.talk);
                    break;
                case "Wait":
                    c = JsonUtility.FromJson<CommandWaitJson>(a);
                    break;
                case "Emotion":
                    c = JsonUtility.FromJson<CommandEmotionJson>(a);
                    break;
            }
      
            command?.Invoke(c);
        }
    }
}
