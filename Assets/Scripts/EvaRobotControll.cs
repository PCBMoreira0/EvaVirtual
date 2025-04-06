using JetBrains.Annotations;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[SelectionBase]
public class EvaRobotControll : MonoBehaviour
{
    [SerializeField] private APIComunication webCommunication;
    [SerializeField] private float timeBetweenCommands = 2f;

    #region Events
    public UnityEvent<APIComunication.CommandJson> OnCommandReceived;
    public UnityEvent OnSimulationStarted;
    public UnityEvent OnSimulationEnded;
    #endregion


    #region Controllers
    private TalkController talkController;
    private EmotionController emotionController;
    private MotionController motionController;
    private ListenController listenController;
    #endregion

    private void Awake()
    {
        talkController = GetComponent<TalkController>();
        emotionController = GetComponent<EmotionController>();
        motionController = GetComponent<MotionController>();
        listenController = GetComponent<ListenController>();
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
        StopAllCoroutines();
        webCommunication.StopAllCoroutines();
        talkController.StopAllCoroutines();
        yield return StartCoroutine(webCommunication.DeleteSimulator());
        yield return StartCoroutine(ResetRobot());
    }

    private IEnumerator ResetRobot()
    {
        yield return motionController.ResetPosition();
        emotionController.ChangeEmotion(EmotionType.NEUTRAL);
        talkController.ResetDialogueBox();
        OnSimulationEnded?.Invoke();
    }
    IEnumerator Execute()
    {
        APIComunication.CommandJson currentCommand = null;
        do
        {
            yield return StartCoroutine(webCommunication.NextCommand((result) => { currentCommand = result; }));

            float startTime = Time.realtimeSinceStartup;
            yield return Parser(currentCommand);
            float totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log("Time: " + totalTime);
            if(totalTime < timeBetweenCommands)
            {
                yield return new WaitForSeconds(timeBetweenCommands - totalTime);
            }
            
        } while (currentCommand != null && currentCommand.command != "End of script");

        ResetRobot();

        yield return StartCoroutine(webCommunication.DeleteSimulator());
    }

    private IEnumerator Parser(APIComunication.CommandJson command)
    {
        if (command == null) yield break;

        switch (command)
        {
            case APIComunication.CommandTalkJson commandTalkJson:
                if (talkController != null) 
                    yield return StartCoroutine(talkController.Talk(commandTalkJson.text));
                break;

            case APIComunication.CommandEmotionJson emotionCommand:
                if (emotionController != null)
                    yield return StartCoroutine(emotionController.ChangeEmotion_routine(Enum.Parse<EmotionType>(emotionCommand.emotion)));
                break;

            case APIComunication.CommandMotionJson motionCommand:
                if (motionController != null)
                    yield return StartCoroutine(motionController.Motion(motionCommand.member, motionCommand.direction));
                break;
            
            case APIComunication.CommandListenJson commandListenJson:
                if (listenController != null)
                {
                    string listenResult = "";
                    yield return StartCoroutine(listenController.StartRecording((result) => { listenResult = result; }));
                    yield return StartCoroutine(webCommunication.SendInput(listenResult));
                    Debug.Log(listenResult);
                }
                break;
        }

        OnCommandReceived?.Invoke(command);
        yield return null;
    }

    private void OnApplicationQuit()
    {
        StartCoroutine(webCommunication.DeleteSimulator());
    }
}