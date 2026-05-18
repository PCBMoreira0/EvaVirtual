using System;
using System.Text;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;

public class WebSocketComunication : MonoBehaviour
{
    WebSocket websocket;

    public String mensagem = "";
    public event Action<CommandMessage> OnMessageReceived;

    async void Start()
    {
        Application.runInBackground = true;

        websocket = new WebSocket("ws://localhost:8000/ws/1");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (code) =>
        {
            Debug.Log("Connection closed! Code: " + code);
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            CommandMessage commandMessage = JsonConvert.DeserializeObject<CommandMessage>(message);
            string debug = $"Received OnMessage! Command: {commandMessage.Command}, Parameters: ";;
            foreach (var kv in commandMessage.Parameter)
            {
                debug += $"Key: {kv.Key}, Value: {kv.Value} | "; 
            }
            Debug.Log(debug);
            OnMessageReceived?.Invoke(commandMessage);
        };

        await websocket.Connect();
    }

    public async void SendWebSocketMessage(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
    }

    public async void SendWebSocketMessageALONE()
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(mensagem);
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
