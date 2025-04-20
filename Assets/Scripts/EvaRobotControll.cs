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

    [SerializeField] private AudioSource audioSource;

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
        audioSource = GetComponent<AudioSource>();
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
                    yield return StartCoroutine(emotionController.ChangeEmotion(Enum.Parse<EmotionType>(emotionCommand.emotion)));
                break;

            case APIComunication.CommandMotionJson motionCommand:
                if (motionController != null)
                    yield return StartCoroutine(motionController.Motion(motionCommand.member, motionCommand.direction));
                break;
            
            case APIComunication.CommandListenJson commandListenJson:
                if (listenController != null)
                {
                    string listenResult = "";
                    yield return StartCoroutine(listenController.StartListening((result) => { listenResult = result; }));
                    yield return StartCoroutine(webCommunication.SendInput(listenResult));
                    Debug.Log(listenResult);
                }
                break;

            case APIComunication.CommandAudioJson commandAudioJson:
                yield return PlayAudio(commandAudioJson.file);
                break;
        }

        OnCommandReceived?.Invoke(command);
        yield return null;
    }

    private IEnumerator PlayAudio(string name)
    {
        AudioClip audioClip = null;
        Debug.Log("chegou aqui");
        yield return webCommunication.GetAudio(name, (AudioClip clip) => { audioClip = clip; });
        Debug.Log("terminou, " +  audioClip);
        if(audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
            yield return new WaitForSeconds(audioClip.length);
        }
    }

    private void OnApplicationQuit()
    {
        StartCoroutine(webCommunication.DeleteSimulator());
    }
}