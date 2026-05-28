using System;
using System.Text;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

public enum SendCommands
{
    TALK_RESPONSE,
    LISTEN_REPSONSE,
    AUDIO_RESPONSE,
    QRREAD_RESPONSE,
    USEREMOTION_RESPONSE,
    SET_SCRIPT,
    RESET,
    START
}

public class WebSocketComunication : MonoBehaviour
{
    WebSocket websocket = null;
    [SerializeField] private string ipAddress = "localhost:8000";

    public String mensagem = "";
    public event Action<CommandMessage> OnMessageReceived;
    public event Action OnConnect;
    public event Action OnDisconnect;

    [SerializeField] private string userId = "0";

    public void SetUserID(string userId)
    {
        this.userId = userId;
    }

    public void SetIP(string ipAddress)
    {
        this.ipAddress = ipAddress;
    }

    public async void Connect()
    {
        Application.runInBackground = true;

        websocket = new WebSocket("ws://" + ipAddress + "/ws/" + userId);

        websocket.OnOpen += () =>
        {
            OnConnect?.Invoke();
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (code) =>
        {
            Debug.Log("Connection closed! Code: " + code);
            OnDisconnect?.Invoke();
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

    public void SendCommand(SendCommands command, string payload = "")
    {
        switch (command)
        {
            case SendCommands.TALK_RESPONSE:
                SendWebSocketMessage("{\"command\":\"talk_response\",\"parameter\":\"\"}");
                break;
            case SendCommands.AUDIO_RESPONSE:
                SendWebSocketMessage("{\"command\":\"audio_response\",\"parameter\":\"\"}");
                break;
            case SendCommands.LISTEN_REPSONSE:
                SendWebSocketMessage($"{{\"command\":\"listen_response\",\"parameter\":\"{payload}\"}}");
                break;
            case SendCommands.QRREAD_RESPONSE:
                SendWebSocketMessage($"{{\"command\":\"qrread_response\",\"parameter\":\"{payload}\"}}");
                break;
            case SendCommands.USEREMOTION_RESPONSE:
                SendWebSocketMessage($"{{\"command\":\"useremotion_response\",\"parameter\":\"{payload}\"}}");
                break;
            
            case SendCommands.SET_SCRIPT:
                SendWebSocketMessage($"{{\"command\":\"set_script\",\"parameter\":\"{payload}\"}}");
                break;
            case SendCommands.START:
                SendWebSocketMessage("{\"command\":\"start\",\"parameter\":\"\"}");
                break;
            case SendCommands.RESET:
                SendWebSocketMessage("{\"command\":\"reset\",\"parameter\":\"\"}");
                break;
        }
    }

    private async void OnApplicationQuit()
    {
        if(websocket != null)
        {
            await websocket.Close();
        }
    }
}
