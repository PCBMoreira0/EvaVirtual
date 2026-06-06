using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static Utils.CommandJsonConverter;

[SelectionBase]
public class EvaRobotControll : MonoBehaviour
{
    [SerializeField] private WebSocketComunication webCommunication;
    [SerializeField] private APIComunication apiCommunication;
    [SerializeField] private string xmlScript = "pcb2_evaml.xml";

    [SerializeField] private float timeBetweenCommands = 2f;

    [SerializeField] private AudioSource audioSource;

    private Queue<CommandMessage> commandQueue = new Queue<CommandMessage>();
    private bool isProcessingCommand = false;


    #region CameraModes
    private Vector3 originalCameraPosition;
    [SerializeField] private Vector3 cameraModeCameraPosition;
    [SerializeField] private float viewTransitionDuration = 2f;
    #endregion

    #region Events
    public UnityEvent OnSimulationStarted;
    public UnityEvent OnSimulationEnded;
    #endregion


    #region Controllers
    private TalkController talkController;
    private AudioController audioController;
    private EmotionController emotionController;
    private MotionController motionController;
    private ListenController listenController;
    private UserEmotionController userEmotionController;
    private QRCodeController qrCodeController;
    private LedController ledController;
    private LightController lightController;
    #endregion

    public void teste()
    {
        StartCoroutine(testtt());
    }
    public IEnumerator testtt()
    {
        string qrResult = null;
        yield return qrCodeController.Scan((result) => { qrResult = result; });

        Debug.Log(qrResult);
    }

    private void Awake()
    {
        talkController = GetComponent<TalkController>();
        audioController = GetComponent<AudioController>();
        emotionController = GetComponent<EmotionController>();
        motionController = GetComponent<MotionController>();
        listenController = GetComponent<ListenController>();
        audioSource = GetComponent<AudioSource>();
        userEmotionController = GetComponent<UserEmotionController>();
        qrCodeController = GetComponent<QRCodeController>();
        ledController = GetComponent<LedController>();
        lightController = GetComponent<LightController>();
    }

    private void Start()
    {
        originalCameraPosition = Camera.main.transform.position;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void OnDisable()
    {
        webCommunication.OnConnect -= StartRobot;
        apiCommunication.OnInitializationComplete.RemoveListener(SetWebsocket);
        webCommunication.OnMessageReceived -= OnMessageReceived;
    }

    void OnEnable()
    {

        webCommunication.OnConnect += StartRobot;
        apiCommunication.OnInitializationComplete.AddListener(SetWebsocket);
        webCommunication.OnMessageReceived += OnMessageReceived;
    }

    public void SetScript(string script)
    {
        xmlScript = script;
    }

    public void InitRobot()
    {
        apiCommunication.InitSimulation();
    }

    private void SetWebsocket(string userId)
    {
        webCommunication.SetUserID(userId);
        webCommunication.Connect();
    }

    private void StartRobot()
    {
        webCommunication.SendCommand(SendCommands.SET_SCRIPT, xmlScript);
        webCommunication.SendCommand(SendCommands.START);
        OnSimulationStarted?.Invoke();
    }

    public async void StopRobot()
    {
        await webCommunication.Disconnect();
        await apiCommunication.DeleteSimulator();
        StartCoroutine(ResetRobot());
        OnSimulationEnded?.Invoke();
    }


    private IEnumerator ResetRobot()
    {
        talkController.StopAllCoroutines();
        yield return motionController.ResetPosition();
        yield return emotionController.ChangeEmotion(EmotionType.NEUTRAL);
        talkController.ResetDialogueBox();
        ledController.ResetColor();
        listenController.ResetListen();
        OnSimulationEnded?.Invoke();
    }

    private void OnMessageReceived(CommandMessage commandMessage)
    {
        commandQueue.Enqueue(commandMessage);

        if (!isProcessingCommand)
        {
            StartCoroutine(ExecuteCommand());
        }
    }

    IEnumerator ExecuteCommand()
    {
        isProcessingCommand = true;

        while (commandQueue.Count > 0)
        {
            CommandMessage commandMessage = commandQueue.Dequeue();
            switch (commandMessage.Command)
            {
                case "talk":
                    yield return talkController.Talk(commandMessage.Parameter["text"].Value<string>());
                    webCommunication.SendCommand(SendCommands.TALK_RESPONSE);
                    break;
                case "listen":
                    string listenResult = null;
                    yield return listenController.StartListening((result) => { listenResult = result; });
                    webCommunication.SendCommand(SendCommands.LISTEN_REPSONSE, listenResult);
                    break;
                case "audio":
                    yield return audioController.PlayAudio(commandMessage.Parameter["audio"].Value<string>(), commandMessage.Parameter["block"].Value<bool>());
                    webCommunication.SendCommand(SendCommands.AUDIO_RESPONSE);
                    break;
                case "emotion":
                    yield return emotionController.ChangeEmotion(Enum.Parse<EmotionType>(commandMessage.Parameter["type"].Value<string>()));
                    break;
                case "leds":
                    yield return ledController.ChangeLedColor(commandMessage.Parameter["state"].Value<string>());
                    break;
                case "motion":
                    yield return motionController.Motion("head", commandMessage.Parameter["head"].Value<string>());
                    break;
                case "qrread":
                    string qrResult = null;
                    yield return qrCodeController.Scan((result) => { qrResult = result; });
                    webCommunication.SendCommand(SendCommands.QRREAD_RESPONSE, qrResult);
                    break;
                case "useremotion":
                    string useremotionResult = null;
                    yield return userEmotionController.ScanEmotion(apiCommunication, (result) => { useremotionResult = result; });
                    webCommunication.SendCommand(SendCommands.USEREMOTION_RESPONSE, useremotionResult);
                    break;
                case "end_script":
                    StopRobot();
                    break;

                default:
                    Debug.Log($"{{\"command\":\"{commandMessage.Command}\",\"parameter\":\"{commandMessage.Parameter}\"}}");
                    break;
            }
        }

        isProcessingCommand = false;
    }

    // IEnumerator Execute()
    // {
    //     CommandJson[] commandList = null;
    //     bool block = true;
    //     bool finished = false;
    //     do
    //     {
    //         yield return StartCoroutine(webCommunication.NextCommand((result) => { commandList = result.commands; }));
    //         float startTime = Time.realtimeSinceStartup;
    //         int coroutinesExecuting = commandList.Length;
    //         Action<bool> onParserFinish = (blk) => { coroutinesExecuting--; block = blk; } ;

    //         ledController.ResetColor();

    //         foreach(var command in commandList)
    //         {
    //             if (command.command.Equals("End"))
    //             {
    //                 finished = true;
    //                 coroutinesExecuting--;

    //             }
    //             else
    //             {
    //                 yield return StartCoroutine(Parser(command, onParserFinish));
    //             }
    //         }

    //         yield return new WaitUntil(() => coroutinesExecuting == 0);

    //         float totalTime = Time.realtimeSinceStartup - startTime;
    //         //Debug.Log("entrou tempo | " + block);
    //         //if (block && totalTime < timeBetweenCommands)
    //         //{
    //         //    yield return new WaitForSeconds(timeBetweenCommands - totalTime);
    //         //}

    //     } while (!finished && commandList != null && commandList.Length != 0);

    //     yield return StartCoroutine(ResetRobot());

    //     yield return StartCoroutine(webCommunication.DeleteSimulator());
    // }


    // private IEnumerator Parser(CommandJson command, Action<bool> onParserFinish)
    // {
    //     if (command == null) yield break;

    //     bool block = true;

    //     switch (command)
    //     {
    //         case CommandTalkJson commandTalkJson:
    //             if (talkController != null) 
    //                 yield return StartCoroutine(talkController.Talk(commandTalkJson.text));
    //             break;

    //         case CommandEmotionJson emotionCommand:
    //             if (emotionController != null)
    //                 yield return StartCoroutine(emotionController.ChangeEmotion(Enum.Parse<EmotionType>(emotionCommand.emotion)));
    //             break;

    //         case CommandMotionJson motionCommand:
    //             if (motionController != null)
    //                 StartCoroutine(motionController.Motion(motionCommand.member, motionCommand.direction));
    //             break;

    //         case CommandLedAnimationJson commandLedAnimation: 
    //             yield return StartCoroutine(ledController.ChangeLedColor(commandLedAnimation.color));
    //             break;
    //         case CommandListenJson commandListenJson:
    //             if (listenController != null)
    //             {
    //                 string listenResult = "";
    //                 yield return StartCoroutine(listenController.StartListening((result) => { listenResult = result; }, ledController));
    //                 yield return StartCoroutine(webCommunication.SendInput(listenResult));
    //                 Debug.Log(listenResult);
    //             }
    //             break;

    //         case CommandAudioJson commandAudioJson:
    //             AudioClip audioClip = null;
    //             yield return webCommunication.GetAudio(commandAudioJson.file, (AudioClip clip) => { audioClip = clip; });
    //             if (commandAudioJson.block)
    //                 yield return StartCoroutine(PlayAudio(audioClip));
    //             else
    //             {
    //                 StartCoroutine(PlayAudio(audioClip));
    //                 block = false;
    //             }
    //             break;

    //         case CommandQRCodeJson commandQRCodeJson:
    //             if (qrCodeController != null)
    //             {
    //                 string qrResult = "";
    //                 yield return StartCoroutine(qrCodeController.Scan((result) => { qrResult = result; }));
    //                 yield return StartCoroutine(webCommunication.SendInput(qrResult));
    //             }
    //             break;

    //         case CommandUserEmotionJson commandUserEmotionJson:
    //             if (userEmotionController != null)
    //             {
    //                 string emotionResult = "";
    //                 yield return StartCoroutine(userEmotionController.ScanEmotion((result) => { emotionResult = result; }));
    //                 yield return StartCoroutine(webCommunication.SendInput(emotionResult));
    //                 Debug.Log(emotionResult);
    //             }
    //             break;

    //         case CommandLightJson commandLightJson:
    //             if (lightController != null)
    //             {
    //                 yield return StartCoroutine(lightController.ChangeLight(commandLightJson.color, commandLightJson.state));
    //             }
    //             break;

    //         case CommandWaitJson commandWaitJson:
    //             Debug.Log(commandWaitJson.time);
    //             block = false;
    //             yield return new WaitForSeconds(commandWaitJson.time);
    //             break;
    //     }

    //     onParserFinish?.Invoke(block);
    //     // OnCommandReceived?.Invoke(command);
    //     yield return null;
    // }

    private IEnumerator PlayAudio(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    public enum VIEWS
    {
        NORMAL,
        CAMERA,
        MICROPHONE
    }
    public void TransitionView(VIEWS newView)
    {
        switch (newView)
        {
            case VIEWS.NORMAL:
                StartCoroutine(NormalModeTransition());
                break;
            case VIEWS.CAMERA:
                StartCoroutine(CameraModeTransition());
                break;
        }
    }

    public void TransitionView(int newView)
    {
        switch ((VIEWS)newView)
        {
            case VIEWS.NORMAL:
                StartCoroutine(NormalModeTransition());
                break;
            case VIEWS.CAMERA:
                StartCoroutine(CameraModeTransition());
                break;
        }
    }

    private IEnumerator CameraModeTransition()
    {
        float currentTime = 0;
        StartCoroutine(motionController.Motion("head", MotionTypes.TWO_UP));
        yield return null;
        while (currentTime < viewTransitionDuration)
        {
            Camera.main.transform.position = Vector3.Lerp(originalCameraPosition, cameraModeCameraPosition, currentTime / viewTransitionDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.position = cameraModeCameraPosition;
    }

    private IEnumerator NormalModeTransition()
    {
        float currentTime = 0;
        StartCoroutine(motionController.Motion("head", MotionTypes.TWO_DOWN));

        while (currentTime < viewTransitionDuration)
        {
            Camera.main.transform.position = Vector3.Lerp(cameraModeCameraPosition, originalCameraPosition, currentTime / viewTransitionDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.position = originalCameraPosition;
    }

    private void OnApplicationQuit()
    {
        // StartCoroutine(webCommunication.DeleteSimulator());
    }
}