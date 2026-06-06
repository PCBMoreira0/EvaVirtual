using System;
using System.Text;
using System.Threading.Tasks; 
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;

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
    
    [SerializeField] private float reconnectDelay = 2f;
    private bool isIntentionalClose = false;
    private bool isReconnecting = false;

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
        if (websocket != null && websocket.State == WebSocketState.Open) return;

        isReconnecting = false;
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
            
            if (!isIntentionalClose)
            {
                AttemptReconnect();
            }
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            CommandMessage commandMessage = JsonConvert.DeserializeObject<CommandMessage>(message);
            string debug = $"Received OnMessage! Command: {commandMessage.Command}, Parameters: ";
            foreach (var kv in commandMessage.Parameter)
            {
                debug += $"Key: {kv.Key}, Value: {kv.Value} | "; 
            }
            Debug.Log(debug);
            OnMessageReceived?.Invoke(commandMessage);
        };

        try
        {
            await websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Connection failed: {e.Message}");
            if (!isIntentionalClose)
            {
                AttemptReconnect();
            }
        }
    }

    private async void AttemptReconnect()
    {
        if (isReconnecting || isIntentionalClose) return;

        isReconnecting = true;
        Debug.Log($"Attempting to reconnect in {reconnectDelay} seconds...");
        
        await Task.Delay(Mathf.RoundToInt(reconnectDelay * 1000));
        
        if (!isIntentionalClose)
        {
            Connect();
        }
    }


    public async void SendWebSocketMessage(string message)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
    }

    public async void SendWebSocketMessageALONE()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
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


    void Update()
    {
        // Necessário para a biblioteca NativeWebSocket processar mensagens na thread principal
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.DispatchMessageQueue();
        }
        #endif

        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                Debug.LogWarning("SIMULAÇÃO: Fechando a conexão forçadamente para testar reconexão!");
                websocket.Close(); 
            }
        }
    }

    public async Awaitable Disconnect()
    {
        isIntentionalClose = true;
        
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    private async void OnApplicationQuit()
    {
        await Disconnect();
    }
}