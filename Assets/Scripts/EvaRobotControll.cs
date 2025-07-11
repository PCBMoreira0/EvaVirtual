using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static Utils.CommandJsonConverter;

[SelectionBase]
public class EvaRobotControll : MonoBehaviour
{
    [SerializeField] private APIComunication webCommunication;
    [SerializeField] private float timeBetweenCommands = 2f;

    [SerializeField] private AudioSource audioSource;

    #region CameraModes
    private Vector3 originalCameraPosition;
    [SerializeField] private Vector3 cameraModeCameraPosition;
    [SerializeField] private float viewTransitionDuration = 2f; 
    #endregion

    #region Events
    public UnityEvent<CommandJson> OnCommandReceived;
    public UnityEvent OnSimulationStarted;
    public UnityEvent OnSimulationEnded;
    #endregion


    #region Controllers
    private TalkController talkController;
    private EmotionController emotionController;
    private MotionController motionController;
    private ListenController listenController;
    private UserEmotionController userEmotionController;
    private QRCodeController qrCodeController;
    private LedController ledController;
    private LightController lightController;
    #endregion

    private void Awake()
    {
        talkController = GetComponent<TalkController>();
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
    }

    private void OnEnable()
    {
        webCommunication.OnInitializationComplete += StartRobot;
    }

    private void OnDisable()
    {
        webCommunication.OnInitializationComplete -= StartRobot;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            StartCoroutine(webCommunication.DeleteSimulator());
        }
    }
#endif

    public void StartRobot()
    {
        StartCoroutine(Execute());
        OnSimulationStarted?.Invoke();
    }
    
    public void StopRobot()
    {
        StartCoroutine(Stop());
        OnSimulationEnded?.Invoke();
    }

    IEnumerator Stop()
    {
        webCommunication.StopAllCoroutines();
        talkController.StopAllCoroutines();
        yield return StartCoroutine(webCommunication.DeleteSimulator());
        yield return StartCoroutine(ResetRobot());
        StopAllCoroutines();
    }

    private IEnumerator ResetRobot()
    {
        yield return StartCoroutine(motionController.ResetPosition());
        yield return StartCoroutine(emotionController.ChangeEmotion(EmotionType.NEUTRAL));
        talkController.ResetDialogueBox();
        ledController.ResetColor();
        OnSimulationEnded?.Invoke();
    }
    IEnumerator Execute()
    {
        CommandJson[] commandList = null;
        bool block = true;
        bool finished = false;
        do
        {
            yield return StartCoroutine(webCommunication.NextCommand((result) => { commandList = result.commands; }));
            float startTime = Time.realtimeSinceStartup;
            int coroutinesExecuting = commandList.Length;
            Action<bool> onParserFinish = (blk) => { coroutinesExecuting--; block = blk; } ;

            ledController.ResetColor();

            foreach(var command in commandList)
            {
                if (command.command.Equals("End"))
                {
                    finished = true;
                    coroutinesExecuting--;

                }
                else
                {
                    yield return StartCoroutine(Parser(command, onParserFinish));
                }
            }

            yield return new WaitUntil(() => coroutinesExecuting == 0);

            float totalTime = Time.realtimeSinceStartup - startTime;
            //Debug.Log("entrou tempo | " + block);
            //if (block && totalTime < timeBetweenCommands)
            //{
            //    yield return new WaitForSeconds(timeBetweenCommands - totalTime);
            //}
            
        } while (!finished && commandList != null && commandList.Length != 0);

        yield return StartCoroutine(ResetRobot());

        yield return StartCoroutine(webCommunication.DeleteSimulator());
    }

    private IEnumerator Parser(CommandJson command, Action<bool> onParserFinish)
    {
        if (command == null) yield break;

        bool block = true;

        switch (command)
        {
            case CommandTalkJson commandTalkJson:
                if (talkController != null) 
                    yield return StartCoroutine(talkController.Talk(commandTalkJson.text));
                break;

            case CommandEmotionJson emotionCommand:
                if (emotionController != null)
                    yield return StartCoroutine(emotionController.ChangeEmotion(Enum.Parse<EmotionType>(emotionCommand.emotion)));
                break;

            case CommandMotionJson motionCommand:
                if (motionController != null)
                    StartCoroutine(motionController.Motion(motionCommand.member, motionCommand.direction));
                break;

            case CommandLedAnimationJson commandLedAnimation: 
                yield return StartCoroutine(ledController.ChangeLedColor(commandLedAnimation.color));
                break;
            case CommandListenJson commandListenJson:
                if (listenController != null)
                {
                    string listenResult = "";
                    yield return StartCoroutine(listenController.StartListening((result) => { listenResult = result; }, ledController));
                    yield return StartCoroutine(webCommunication.SendInput(listenResult));
                    Debug.Log(listenResult);
                }
                break;

            case CommandAudioJson commandAudioJson:
                AudioClip audioClip = null;
                yield return webCommunication.GetAudio(commandAudioJson.file, (AudioClip clip) => { audioClip = clip; });
                if (commandAudioJson.block)
                    yield return StartCoroutine(PlayAudio(audioClip));
                else
                {
                    StartCoroutine(PlayAudio(audioClip));
                    block = false;
                }
                break;

            case CommandQRCodeJson commandQRCodeJson:
                if (qrCodeController != null)
                {
                    string qrResult = "";
                    yield return StartCoroutine(qrCodeController.Scan((result) => { qrResult = result; }));
                    yield return StartCoroutine(webCommunication.SendInput(qrResult));
                }
                break;

            case CommandUserEmotionJson commandUserEmotionJson:
                if (userEmotionController != null)
                {
                    string emotionResult = "";
                    yield return StartCoroutine(userEmotionController.ScanEmotion((result) => { emotionResult = result; }));
                    yield return StartCoroutine(webCommunication.SendInput(emotionResult));
                    Debug.Log(emotionResult);
                }
                break;
            
            case CommandLightJson commandLightJson:
                if (lightController != null)
                {
                    yield return StartCoroutine(lightController.ChangeLight(commandLightJson.color, commandLightJson.state));
                }
                break;

            case CommandWaitJson commandWaitJson:
                Debug.Log(commandWaitJson.time);
                block = false;
                yield return new WaitForSeconds(commandWaitJson.time);
                break;
        }

        onParserFinish?.Invoke(block);
        OnCommandReceived?.Invoke(command);
        yield return null;
    }

    private IEnumerator PlayAudio(AudioClip clip)
    {
        if(clip != null)
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
        StartCoroutine(webCommunication.DeleteSimulator());
    }
}